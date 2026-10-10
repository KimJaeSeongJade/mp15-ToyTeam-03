using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 단계들이 공유하는 씬 참조와 실행 서비스. 단계별 몬스터/터렛 설정은 여기에 넣지 않는다.
[Serializable]
public class TutorialContext
{
    public GameManager Game;
    public PlayerStatus Player;
    public TutorialInputFilter Input;
    public TutorialUI UI;
    public TutorialGoldDelivery GoldDelivery;
    public CastleHp Castle;
    public GameObject MovementBoundary;
    public BasicAttack PracticeAttackTemplate;
    [Header("가이드 프리팹 (비어 있으면 기존 표시 사용)")]
    public TutorialGuideStyle BuildGuideStyle = new TutorialGuideStyle();
    public TutorialGuideStyle EnemyGuideStyle = new TutorialGuideStyle();
    public TutorialPresentation FailureText = new TutorialPresentation();
    public string LeftMouseName;
    public string RightMouseName;

    public PlayerInputReader Reader { get; private set; }
    public PlayerWallet Wallet { get; private set; }
    public TutorialSpawnCutscene Camera { get; private set; }
    public TutorialBuildAreaFilter BuildAreas { get; private set; }
    public bool Failed { get; private set; }
    public bool StepCrosshairVisible { get; private set; } = true;
    public bool Paused => _owner.IsPaused;
    public event Action ShotFired;
    public event Action SkillUsed;

    [NonSerialized] private TutorialManager _owner;
    [NonSerialized] private TutorialBuildPointGuide _buildGuide;
    [NonSerialized] private TutorialBuildPointGuide _enemyGuide;
    [NonSerialized] private Collider[] _walls;
    [NonSerialized] private float _castleHp;
    [NonSerialized] private float _castleMaxHp;
    private readonly List<BaseEnemy> _enemies = new List<BaseEnemy>();
    private readonly Dictionary<string, List<BaseEnemy>> _waves = new Dictionary<string, List<BaseEnemy>>();

    public bool Initialize(TutorialManager owner)
    {
        _owner = owner;
        if (Player == null || Game == null || Input == null || UI == null)
        { Fail("공통 참조 Player / Game / Input / UI가 필요합니다."); return false; }
        Reader = Player.GetComponent<PlayerInputReader>();
        Wallet = Player.GetComponent<PlayerWallet>();
        if (Reader == null || Wallet == null) { Fail("플레이어 입력/지갑이 없습니다."); return false; }
        Game.StartGame();
        UI.BindExistingHUD(Game);
        UI.SetGold(Wallet.Gold);
        Wallet.OnGoldChanged += OnGoldChanged;
        Player.OnSkillCooldownStarted += OnSkillUsed;
        Input.PromptAccepted += OnPromptAccepted;
        Camera = owner.gameObject.AddComponent<TutorialSpawnCutscene>();
        BuildAreas = owner.gameObject.AddComponent<TutorialBuildAreaFilter>();
        BuildAreas.Initialize();
        BuildAreas.Allow(null);
        _walls = MovementBoundary != null ? MovementBoundary.GetComponentsInChildren<Collider>() : new Collider[0];
        if (Castle != null) Castle.OnCastleHealthChanged += OnCastleHealthChanged;
        if (PracticeAttackTemplate != null)
        {
            GameObject root = new GameObject("TutorialAttackTemplate");
            root.transform.SetParent(owner.transform, false);
            root.SetActive(false);
            BasicAttack template = UnityEngine.Object.Instantiate(PracticeAttackTemplate, root.transform);
            template.gameObject.SetActive(false);
            template.gameObject.AddComponent<TutorialPracticeShot>().Owner = owner;
            Player.GetComponent<PlayerAttackMode>().SetBasicAttack(template);
        }
        return true;
    }

    public void SetStepCrosshair(bool visible)
    {
        StepCrosshairVisible = visible;
        UI.SetCrosshairVisible(visible);
    }

    public string KeyName(KeyCode key)
    {
        if (key == KeyCode.Mouse0) return LeftMouseName;
        if (key == KeyCode.Mouse1) return RightMouseName;
        return key.ToString();
    }

    public string Format(string text, int current = 0, int target = 0, string key = "", float seconds = 0f)
    {
        return (text ?? "").Replace("{현재횟수}", current.ToString()).Replace("{목표횟수}", target.ToString())
            .Replace("{키}", key).Replace("{시간}", seconds.ToString("0.##"))
            .Replace("{레벨}", Player != null ? Player.Level.ToString() : "")
            .Replace("{골드}", Wallet != null ? Wallet.Gold.ToString() : "")
            .Replace("{캐슬HP}", _castleHp.ToString("0")).Replace("{최대캐슬HP}", _castleMaxHp.ToString("0"));
    }

    public IEnumerator Prompt(TutorialPresentation text, TutorialControls controls, PlayerInputAction action,
        int current = 0, int target = 0, string key = "", float seconds = 0f)
    {
        controls.Apply(this);
        text.Show(this, current, target, key, seconds);
        Input.BeginPrompt(action);
        UI.SetSpotlight(true);
        yield return new WaitUntil(() => !Input.WaitingForPrompt && !Paused);
    }

    public IEnumerator Wait(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!Paused) elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public bool HasActiveSkill()
    {
        foreach (SkillAttack skill in UnityEngine.Object.FindObjectsOfType<SkillAttack>())
            if (skill.gameObject.scene == _owner.gameObject.scene) return true;
        return false;
    }

    public void GuideBuild(BuildPoint point, string label)
    {
        BuildAreas.Allow(point);
        if (_buildGuide == null)
        {
            _buildGuide = _owner.gameObject.AddComponent<TutorialBuildPointGuide>();
            _buildGuide.Initialize(point.transform, Player.transform, UI, label, true, BuildGuideStyle);
        }
        else _buildGuide.Track(point.transform, label);
        _buildGuide.Show(true);
    }

    public void GuideEnemy(BaseEnemy enemy, string label)
    {
        if (enemy == null) return;
        if (_enemyGuide == null)
        {
            _enemyGuide = _owner.gameObject.AddComponent<TutorialBuildPointGuide>();
            _enemyGuide.Initialize(enemy.transform, Player.transform, UI, label, false, EnemyGuideStyle);
        }
        else _enemyGuide.Track(enemy.transform, label);
        _enemyGuide.Show(true);
    }

    public void HideBuildGuide() { if (_buildGuide != null) _buildGuide.Show(false); }
    public void HideEnemyGuide() { if (_enemyGuide != null) _enemyGuide.Show(false); }

    public bool IsInstalled(BuildPoint point, BaseTurret prefab)
    {
        if (!point.HasTurret) return false;
        foreach (Transform child in point.transform)
        {
            BaseTurret turret = child.GetComponent<BaseTurret>();
            if (turret != null && (prefab == null || (turret.GetType() == prefab.GetType()
                && turret.name.StartsWith(prefab.name, StringComparison.Ordinal)))) return true;
        }
        return false;
    }

    public bool IsLookingAt(BuildPoint point)
    {
        UnityEngine.Camera camera = UnityEngine.Camera.main;
        if (camera == null) return false;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        return Physics.Raycast(ray, out RaycastHit hit, 5f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Collide)
            && hit.collider.GetComponentInParent<BuildPoint>() == point;
    }

    public List<BaseEnemy> CreateWave(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) { Fail("웨이브 ID가 비어 있습니다."); return null; }
        if (_waves.TryGetValue(id, out List<BaseEnemy> previous) && previous.Exists(IsAlive))
        { Fail($"아직 진행 중인 웨이브 ID입니다: {id}"); return null; }
        var wave = new List<BaseEnemy>();
        _waves[id] = wave;
        return wave;
    }

    public List<BaseEnemy> GetWave(string id)
    {
        if (id != null && _waves.TryGetValue(id, out List<BaseEnemy> wave)) return wave;
        Fail($"등록되지 않은 웨이브 ID입니다: {id}");
        return null;
    }

    public static bool IsAlive(BaseEnemy enemy) => enemy != null && enemy.gameObject.activeInHierarchy;

    public BaseEnemy Spawn(TutorialEnemyDefinition definition)
    {
        if (definition == null || definition.Prefab == null || definition.SpawnPoint == null || definition.Destination == null)
        { Fail("몬스터 프리팹 / 등장 / 도착 Transform을 지정하세요."); return null; }
        if (!NavMesh.SamplePosition(definition.SpawnPoint.position, out NavMeshHit spawn, 5f, NavMesh.AllAreas)
            || !NavMesh.SamplePosition(definition.Destination.position, out NavMeshHit destination, 5f, NavMesh.AllAreas))
        { Fail("등장/전투 지점이 NavMesh 위에 없습니다."); return null; }
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(spawn.position, destination.position, NavMesh.AllAreas, path)
            || path.status != NavMeshPathStatus.PathComplete)
        { Fail("몬스터 등장 지점에서 목적지까지 이동할 수 없습니다."); return null; }
        GameObject staging = new GameObject("TutorialEnemyStaging");
        staging.transform.SetParent(_owner.transform);
        staging.SetActive(false);
        BaseEnemy enemy = UnityEngine.Object.Instantiate(definition.Prefab, spawn.position, definition.SpawnPoint.rotation, staging.transform);
        enemy.MonGold = definition.Gold;
        enemy.MonExp = definition.GrantNextLevel && Player.Level == 1
            ? Mathf.Max(0f, Player.RequiredExp - Player.Exp) : definition.Experience;
        enemy.MonHp = definition.Health;
        enemy.MonMaxHp = definition.Health;
        enemy.MonDefend = definition.Defence;
        enemy.MonCastleDam = definition.CastleDamage;
        enemy.MonSpeed = definition.Speed;
        staging.SetActive(true);
        enemy.InitData();
        MonsterMove movement = enemy.GetComponent<MonsterMove>();
        if (movement != null) movement.enabled = false;
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent == null || !agent.Warp(spawn.position))
        { UnityEngine.Object.Destroy(staging); Fail("몬스터 NavMeshAgent 배치를 확인하세요."); return null; }
        agent.speed = definition.Speed;
        foreach (Collider body in enemy.GetComponentsInChildren<Collider>())
            foreach (Collider wall in _walls) Physics.IgnoreCollision(body, wall);
        agent.isStopped = false;
        agent.SetPath(path);
        _enemies.Add(enemy);
        if (definition.Route != null && definition.Route.Length > 0)
        {
            if (Castle == null) { Fail("캐슬 참조가 필요합니다."); return null; }
            var points = new List<Vector3> { definition.Destination.position };
            foreach (Transform point in definition.Route)
            {
                if (point == null) { Fail("몬스터 경유 Transform이 비어 있습니다."); return null; }
                points.Add(point.position);
            }
            if (!enemy.gameObject.AddComponent<TutorialEnemyRoute>().Initialize(enemy, points, Castle, definition.HoldAtGoal))
            { enemy.ReturnToPool(); Fail("몬스터 경유/캐슬 경로가 끊겼습니다."); return null; }
        }
        return enemy;
    }

    public void Fail(string reason)
    {
        Failed = true;
        #if UNITY_EDITOR
        Debug.LogError($"TutorialManager: {reason}", _owner);
#endif
        if (UI != null) { FailureText.Show(this); UI.SetSpotlight(false); }
        if (Input != null) Input.CancelPrompt();
    }

    public void NotifyShot()
    {
        if (!Paused && Input.InputEnabled && Time.timeScale > 0f) ShotFired?.Invoke();
    }
    private void OnSkillUsed(float duration) { if (!Paused && Input.InputEnabled) SkillUsed?.Invoke(); }
    private void OnGoldChanged(int total, int amount) => UI.SetGold(total);
    private void OnPromptAccepted() => UI.SetSpotlight(false);
    private void OnCastleHealthChanged(float current, float max) { _castleHp = current; _castleMaxHp = max; }

    public void StopWorld()
    {
        if (Camera != null) Camera.RestoreCamera();
        HideBuildGuide();
        HideEnemyGuide();
        foreach (BaseEnemy enemy in _enemies)
            if (IsAlive(enemy)) enemy.ReturnToPool();
    }

    public void Dispose()
    {
        StopWorld();
        if (Wallet != null) Wallet.OnGoldChanged -= OnGoldChanged;
        if (Player != null) Player.OnSkillCooldownStarted -= OnSkillUsed;
        if (Input != null) Input.PromptAccepted -= OnPromptAccepted;
        if (Castle != null) Castle.OnCastleHealthChanged -= OnCastleHealthChanged;
        ShotFired = null;
        SkillUsed = null;
    }
}
