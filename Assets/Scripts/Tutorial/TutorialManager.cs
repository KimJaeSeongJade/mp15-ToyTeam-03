using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// 본게임 씬에는 추가하지 않는다. 기존 GameManager는 플레이어 참조/UI 역할만 재사용한다.
[DefaultExecutionOrder(-2000)]
public class TutorialManager : MonoBehaviour
{
    public const string CompletionKey = "Tutorial.Completed.v1";
    public static bool HasCompleted => PlayerPrefs.GetInt(CompletionKey, 0) == 1;

    public PlayerStatus Player;
    public GameManager Game;
    public TutorialInputFilter InputFilter;
    public TutorialUI UI;
    public TutorialGoldDelivery GoldDelivery;
    public TutorialCastleCutscene CastleCutscene;
    public BaseEnemy EnemyPrefab;
    public Transform CombatSpawn;
    // 몬스터 등장 위치. CombatSpawn은 첫 웨이브에서 접근한 뒤 대기할 전투 지점이다.
    public Vector3 OutsideSpawnPosition = new Vector3(-16.365416f, 7.09f, 11.89f);
    public WayPointPath TurretPath;
    public BuildPoint BuildPoint;
    public BaseTurret AttackTurret;
    public BaseTurret CombinedTurret;
    [Min(0.1f)] public float HighlightDuration = 1.2f;
    public UnityEvent OnCompleted = new UnityEvent();

    private readonly List<BaseEnemy> _enemies = new List<BaseEnemy>();
    private PlayerInputReader _input;
    private PlayerWallet _wallet;
    private bool _skillUsed;
    private bool _modal;
    private bool _completed;
    private float _resumeTimeScale;
    private bool _resumeInput;
    private Coroutine _routine;
    private TutorialBuildPointGuide _buildGuide;
    private Collider[] _boundaryColliders;

    private void Awake()
    {
        // GameManager의 Awake는 유지하되 Start/ESC 업데이트는 튜토리얼이 소유한다.
        if (Game != null) Game.enabled = false;
        _input = Player != null ? Player.GetComponent<PlayerInputReader>() : null;
        _wallet = Player != null ? Player.GetComponent<PlayerWallet>() : null;
    }

    private IEnumerator Start()
    {
        // 기존 플레이어의 Start에서 공격 풀과 모드가 초기화된 뒤 진행한다.
        yield return null;
        if (!ValidateSetup()) yield break;
        Game.StartGame();
        UI.BindExistingHUD(Game);
        UI.SetKeys(_input);
        UI.SetGold(_wallet.Gold);
        _wallet.OnGoldChanged += OnGoldChanged;
        Player.OnSkillCooldownStarted += OnSkillUsed;
        CastleCutscene.Castle.OnCastleHealthChanged += UI.SetCastleHealth;
        UI.SkipButton.onClick.AddListener(Skip);
        UI.ContinueButton.onClick.AddListener(CancelSkip);
        _buildGuide = gameObject.AddComponent<TutorialBuildPointGuide>();
        _buildGuide.Initialize(BuildPoint, Player.transform, UI);
        GameObject boundary = GameObject.Find("TutorialBoundary");
        _boundaryColliders = boundary != null ? boundary.GetComponentsInChildren<Collider>() : new Collider[0];
        _routine = StartCoroutine(Run());
    }

    private bool ValidateSetup()
    {
        if (Player != null && Game != null && InputFilter != null && UI != null
            && GoldDelivery != null && CastleCutscene != null && CastleCutscene.Castle != null
            && CastleCutscene.FocusCamera != null && CastleCutscene.MonsterPrefab != null
            && CastleCutscene.MonsterSpawn != null && CastleCutscene.HitPoint != null
            && EnemyPrefab != null && CombatSpawn != null && TurretPath != null
            && TurretPath._waypoints != null && TurretPath._waypoints.Count > 0
            && BuildPoint != null && AttackTurret != null && CombinedTurret != null
            && _input != null && _wallet != null) return true;
        Debug.LogError("TutorialManager: 튜토리얼 씬 연결을 확인하세요.", this);
        enabled = false;
        return false;
    }

    private IEnumerator Run()
    {
        Vector3 start = Player.transform.position;
        yield return Teach("W A S D로 이동하고 마우스로 주변을 둘러보세요.", TutorialFocus.Move, "이동", false, false, false);
        yield return new WaitUntil(() => (Player.transform.position - start).sqrMagnitude > 1f);

        yield return Teach($"{TutorialUI.KeyName(_input.AttackKey)}로 몬스터를 처치하세요.", TutorialFocus.Attack, "웨이브 1 / 2", true, false, false);
        int totalGold = Mathf.CeilToInt((AttackTurret.Cost + CombinedTurret.Cost) / (float)GameManager.GOLD_AMOUNT)
            * GameManager.GOLD_AMOUNT;
        BaseEnemy target = Spawn(CombatSpawn.position, false, totalGold);
        if (target == null) yield break;
        yield return new WaitUntil(() => target == null || !target.gameObject.activeInHierarchy);

        _skillUsed = false;
        yield return Teach($"{TutorialUI.KeyName(_input.SkillKey)}를 누르고 바닥을 조준한 뒤 놓으세요.", TutorialFocus.Skill, "웨이브 1 / 2", true, true, false);
        BaseEnemy skillTarget = Spawn(CombatSpawn.position + Vector3.right * 1.5f, false, 0);
        if (skillTarget == null) yield break;
        yield return new WaitUntil(() => _skillUsed);
        UI.Show("남은 몬스터를 처치하세요.", TutorialFocus.Attack, "웨이브 1 / 2");
        yield return new WaitUntil(() => skillTarget == null || !skillTarget.gameObject.activeInHierarchy);

        yield return Teach("웨이브가 끝나면 골드가 자동으로 배달됩니다.", TutorialFocus.Gold, "웨이브 1 완료", false, false, false);
        yield return GoldDelivery.Deliver();
        // 튜토리얼 전용 보장. 실제 가격 변경 후에도 건설 단계에서 막히지 않는다.
        int needed = AttackTurret.Cost + CombinedTurret.Cost;
        if (_wallet.Gold < needed) _wallet.AddGold(needed - _wallet.Gold);

        _buildGuide.Show(true);
        yield return Teach("노란 화살표로 이동한 뒤 1번 공격 터렛을 선택해 바닥의 표시를 조준하고 설치하세요.", TutorialFocus.Build, "준비 시간: 무제한", true, false, true);
        yield return new WaitUntil(() => InstalledTurret != null);

        yield return Teach("같은 지점에 공격 터렛을 한 번 더 설치하면 속사 터렛으로 조합됩니다.", TutorialFocus.Build, "준비 시간: 무제한", true, false, true);
        yield return new WaitUntil(() => InstalledTurret != null && InstalledTurret.GetType() == CombinedTurret.GetType());
        _buildGuide.Show(false);

        yield return Teach($"{TutorialUI.KeyName(_input.ExitBuildKey)}로 건설 모드를 종료하세요.", TutorialFocus.ExitBuild, "전투 준비", false, false, false);
        InputFilter.AllowExitBuild = true;
        yield return new WaitUntil(() => Player.GetComponent<PlayerAttackMode>().enabled);
        InputFilter.AllowExitBuild = false;

        yield return Teach("터렛은 사거리 안의 몬스터를 자동으로 공격합니다.", TutorialFocus.Build, "웨이브 2 / 2", false, false, false);
        for (int i = 0; i < 2; i++)
        {
            BaseEnemy enemy = Spawn(TurretPath.transform.position, true, GameManager.GOLD_AMOUNT);
            if (enemy == null) yield break;
            yield return new WaitUntil(() => enemy == null || !enemy.gameObject.activeInHierarchy);
            // 경로를 통과해 놓쳤으면 같은 단계에서 다시 시연한다. 성공으로 오인하지 않는다.
            if (enemy != null && enemy.MonHp > 0f) { i--; yield return new WaitForSeconds(1f); }
        }
        UI.Show("두 번째 웨이브도 막아냈습니다.", TutorialFocus.Gold, "웨이브 2 완료");
        yield return GoldDelivery.Deliver();

        yield return Teach("몬스터가 캐슬에 도착하면 HP가 감소합니다.", TutorialFocus.Castle, "캐슬 방어", false, false, false);
        InputFilter.InputEnabled = false;
        yield return CastleCutscene.Play(Player);
        UI.Show("캐슬 HP가 0이 되면 게임오버입니다. 도착하기 전에 막으세요!", TutorialFocus.Castle, "캐슬 방어");
        yield return new WaitForSeconds(3f);
        Complete();
    }

    private BaseTurret InstalledTurret => BuildPoint.GetComponentInChildren<BaseTurret>();

    private IEnumerator Teach(string text, TutorialFocus focus, string progress, bool attack, bool skill, bool build)
    {
        InputFilter.InputEnabled = false;
        InputFilter.AllowAttack = attack;
        InputFilter.AllowSkill = skill;
        InputFilter.AllowBuild = build;
        InputFilter.AllowExitBuild = false;
        UI.Show(text, focus, progress);
        UI.SetSpotlight(true);
        yield return new WaitForSeconds(HighlightDuration);
        UI.SetSpotlight(false);
        InputFilter.InputEnabled = true;
    }

    private BaseEnemy Spawn(Vector3 position, bool move, int gold)
    {
        // 경계 밖에서 출현하고, 기존 전투 지점까지 실제 NavMesh 경로로 접근한다.
        if (!NavMesh.SamplePosition(OutsideSpawnPosition, out NavMeshHit spawnHit, 5f, NavMesh.AllAreas)
            || !NavMesh.SamplePosition(position, out NavMeshHit destinationHit, 5f, NavMesh.AllAreas))
            return SpawnFailure("등장/전투 지점이 NavMesh 위에 없습니다.");
        NavMeshPath approach = new NavMeshPath();
        if (!NavMesh.CalculatePath(spawnHit.position, destinationHit.position, NavMesh.AllAreas, approach)
            || approach.status != NavMeshPathStatus.PathComplete)
            return SpawnFailure("경계 밖 등장 지점에서 전투 지점까지 이동할 수 없습니다.");
        // 새 인스턴스를 사용하여 기존 몬스터 풀의 재사용 상태에 의존하지 않는다.
        GameObject staging = new GameObject("TutorialEnemyStaging");
        staging.transform.SetParent(transform);
        staging.SetActive(false);
        BaseEnemy enemy = Instantiate(EnemyPrefab, spawnHit.position, Quaternion.identity, staging.transform);
        enemy.MonGold = gold;
        enemy.MonExp = 0f;
        enemy.MonHp = 20f;
        enemy.MonDefend = 0f;
        enemy.MonSpeed = 3f;
        staging.SetActive(true);
        enemy.InitData();
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent == null || !agent.Warp(spawnHit.position))
        {
            Destroy(staging);
            return SpawnFailure("몬스터 NavMeshAgent 배치를 확인하세요.");
        }
        agent.speed = enemy.MonSpeed;
        // 이 벽은 플레이어의 이동 제한용이다. 몬스터와의 충돌만 개별적으로 제외한다.
        foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
            foreach (Collider wall in _boundaryColliders) Physics.IgnoreCollision(collider, wall);
        if (move) enemy.GetComponent<MonsterMove>().Initialize(TurretPath);
        else
        {
            agent.isStopped = false;
            agent.SetPath(approach);
        }
        _enemies.Add(enemy);
        return enemy;
    }

    private BaseEnemy SpawnFailure(string reason)
    {
        Debug.LogError($"TutorialManager: {reason} OutsideSpawnPosition을 확인하세요.", this);
        UI.Show("몬스터 등장 경로 설정을 확인하세요. ESC로 나갈 수 있습니다.", TutorialFocus.None, "경로 확인 필요");
        return null;
    }

    private void Update()
    {
        if (_completed || UI == null || InputFilter == null || _routine == null) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_modal) CancelSkip();
            else OpenSkip();
        }
    }

    private void OpenSkip()
    {
        _modal = true;
        _resumeTimeScale = Time.timeScale;
        _resumeInput = InputFilter.InputEnabled;
        InputFilter.InputEnabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UI.SkipDialog.SetActive(true);
    }

    private void CancelSkip()
    {
        if (!_modal) return;
        _modal = false;
        UI.SkipDialog.SetActive(false);
        Time.timeScale = _resumeTimeScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(ResumeInput());
    }

    private IEnumerator ResumeInput()
    {
        yield return null; // 확인 버튼 클릭이 일반 공격 입력으로 전달되지 않게 한다.
        if (!_modal && !_completed) InputFilter.InputEnabled = _resumeInput;
    }

    private void Skip()
    {
        if (_routine != null) StopCoroutine(_routine);
        CastleCutscene.RestoreCamera();
        foreach (BaseEnemy enemy in _enemies)
            if (enemy != null && enemy.gameObject.activeInHierarchy) enemy.ReturnToPool();
        Complete();
    }

    private void Complete()
    {
        if (_completed) return;
        _completed = true;
        if (_buildGuide != null) _buildGuide.Show(false);
        _modal = false;
        InputFilter.InputEnabled = false;
        UI.SkipDialog.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerPrefs.SetInt(CompletionKey, 1);
        PlayerPrefs.Save();
        UI.Show("튜토리얼 클리어!", TutorialFocus.None, "완료");
        OnCompleted.Invoke(); // 타이틀/게임 씬 진입 연결은 보류한다.
    }

    private void OnSkillUsed(float duration) => _skillUsed = true;
    private void OnGoldChanged(int total, int amount) => UI.SetGold(total);

    private void OnDestroy()
    {
        if (_wallet != null) _wallet.OnGoldChanged -= OnGoldChanged;
        if (Player != null) Player.OnSkillCooldownStarted -= OnSkillUsed;
        if (CastleCutscene != null && CastleCutscene.Castle != null && UI != null)
            CastleCutscene.Castle.OnCastleHealthChanged -= UI.SetCastleHealth;
        if (UI != null)
        {
            UI.SkipButton.onClick.RemoveListener(Skip);
            UI.ContinueButton.onClick.RemoveListener(CancelSkip);
        }
        if (_modal) Time.timeScale = _resumeTimeScale;
    }
}
