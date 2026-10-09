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
    public CastleHp Castle;
    public Transform CastleGoal;
    public BaseEnemy EnemyPrefab;
    public BaseEnemy ArmoredEnemyPrefab;
    public Transform CombatSpawn;
    // 씬 오브젝트를 이동해 등장/전투/경유 위치를 조정한다.
    public Transform OutsideSpawn;
    public Transform SecondarySpawn;
    public Transform SecondCombatPoint;
    public Transform PrimaryLanePoint;
    public Transform SecondaryLanePoint;
    public BuildPoint BuildPoint;
    public BuildPoint SaleBuildPoint;
    public BuildPoint SecondaryBuildPoint;
    public BaseTurret AttackTurret;
    public BaseTurret CombinedTurret;
    public BaseTurret DefenceTurret;
    public BaseTurret ReverseCombinedTurret;
    public BaseTurret SaleTurret;
    public BasicAttack PracticeAttackTemplate;
    [Min(0.1f)] public float HighlightDuration = 1.2f;
    public UnityEvent OnCompleted = new UnityEvent();

    private readonly List<BaseEnemy> _enemies = new List<BaseEnemy>();
    private PlayerInputReader _input;
    private PlayerWallet _wallet;
    private bool _attackPractice;
    private bool _skillPractice;
    private int _practiceShots;
    private int _practiceSkills;
    private bool _modal;
    private bool _completed;
    private float _resumeTimeScale;
    private bool _resumeInput;
    private Coroutine _routine;
    private TutorialBuildPointGuide _buildGuide;
    private Collider[] _boundaryColliders;
    private TutorialBuildPointGuide _enemyGuide;
    private TutorialSpawnCutscene _spawnCutscene;
    private TutorialBuildAreaFilter _buildAreas;
    private bool _lastWaveKilled;
    private bool _waveFailed;

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
        UI.SetGold(_wallet.Gold);
        _wallet.OnGoldChanged += OnGoldChanged;
        Player.OnSkillCooldownStarted += OnSkillUsed;
        InputFilter.PromptAccepted += OnPromptAccepted;
        Castle.OnCastleHealthChanged += UI.SetCastleHealth;
        UI.SkipButton.onClick.AddListener(Skip);
        UI.ContinueButton.onClick.AddListener(CancelSkip);
        _buildGuide = gameObject.AddComponent<TutorialBuildPointGuide>();
        _buildGuide.Initialize(BuildPoint, Player.transform, UI);
        _spawnCutscene = gameObject.AddComponent<TutorialSpawnCutscene>();
        _wallet.TrySpendGold(_wallet.Gold);
        PlaceAdditionalPoint(SaleBuildPoint);
        PlaceAdditionalPoint(SecondaryBuildPoint);
        if (!SaleBuildPoint.HasTurret)
        {
            SaleBuildPoint.TryBuildTurret(SaleTurret, out int unusedCost);
            foreach (BaseTurret turret in SaleBuildPoint.GetComponentsInChildren<BaseTurret>()) turret.enabled = false;
        }
        _buildAreas = gameObject.AddComponent<TutorialBuildAreaFilter>();
        _buildAreas.Initialize();
        _buildAreas.Allow(null);
        // 비활성 템플릿을 복제하므로 풀 생성/확장 시 OnEnable이 발사로 오인되지 않는다.
        GameObject templateRoot = new GameObject("TutorialAttackTemplate");
        templateRoot.transform.SetParent(transform, false);
        templateRoot.SetActive(false);
        BasicAttack template = Instantiate(PracticeAttackTemplate, templateRoot.transform);
        template.gameObject.SetActive(false);
        template.gameObject.AddComponent<TutorialPracticeShot>().Owner = this;
        Player.GetComponent<PlayerAttackMode>().SetBasicAttack(template);
        GameObject boundary = GameObject.Find("TutorialBoundary");
        _boundaryColliders = boundary != null ? boundary.GetComponentsInChildren<Collider>() : new Collider[0];
        _routine = StartCoroutine(Run());
    }

    private bool ValidateSetup()
    {
        if (Player != null && Game != null && InputFilter != null && UI != null
            && GoldDelivery != null && Castle != null && CastleGoal != null
            && EnemyPrefab != null && CombatSpawn != null && SecondCombatPoint != null
            && OutsideSpawn != null && SecondarySpawn != null
            && PrimaryLanePoint != null && SecondaryLanePoint != null
            && BuildPoint != null && SaleBuildPoint != null && SecondaryBuildPoint != null
            && AttackTurret != null && CombinedTurret != null && DefenceTurret != null
            && ReverseCombinedTurret != null && SaleTurret != null && ArmoredEnemyPrefab != null
            && SaleTurret.Cost / 2 >= DefenceTurret.Cost + ReverseCombinedTurret.Cost
            && (AttackTurret.Cost + CombinedTurret.Cost) % GameManager.GOLD_AMOUNT == 0
            && PracticeAttackTemplate != null
            && _input != null && _wallet != null) return true;
        Debug.LogError("TutorialManager: 튜토리얼 씬 연결을 확인하세요.", this);
        enabled = false;
        return false;
    }

    private IEnumerator Run()
    {
        foreach (KeyCode key in new[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D })
        {
            InputFilter.RequiredMoveKey = key;
            yield return Teach($"{key} 키를 1초 동안 눌러 이동하세요.", TutorialFocus.Move, "이동 연습", false, false, false, PlayerInputAction.Move);
            float held = 0f;
            while (held < 0.5f)
            {
                if (!_modal) held = Input.GetKey(key) ? held + Time.deltaTime : 0f;
                yield return null;
            }
        }
        InputFilter.RequiredMoveKey = null;
        yield return Teach("Shift로 달리고 Space로 점프할 수 있습니다.", TutorialFocus.None, "이동 연습", false, false, false);

        _attackPractice = true;
        _practiceShots = 0;
        yield return Teach($"{TutorialUI.KeyName(_input.AttackKey)}를 누르거나 유지해 5발 발사하세요.", TutorialFocus.Attack, "일반 공격 연습 0 / 5", true, false, false, PlayerInputAction.Attack);
        yield return new WaitUntil(() => _practiceShots >= 5);
        _attackPractice = false;

        _skillPractice = true;
        _practiceSkills = 0;
        yield return Teach("우클릭을 누르고 바닥을 조준한 뒤 놓으세요. 3회 시전, 쿨타임 3초.", TutorialFocus.Skill, "스킬 연습 0 / 3", false, true, false, PlayerInputAction.Skill);
        yield return new WaitUntil(() => _practiceSkills >= 3);
        _skillPractice = false;
        InputFilter.AllowSkill = false;
        yield return new WaitUntil(() => !HasActiveSkill());
        InputFilter.InputEnabled = false;
        yield return new WaitForSeconds(1.25f);
        UI.Show("벽 밖에서 몬스터가 나타났습니다!", TutorialFocus.None, "직접 전투");
        int totalGold = Mathf.CeilToInt((AttackTurret.Cost + CombinedTurret.Cost) / (float)GameManager.GOLD_AMOUNT)
            * GameManager.GOLD_AMOUNT;
        float levelUpExp = Player.Level == 1 ? Mathf.Max(0f, Player.RequiredExp - Player.Exp) : 0f;
        BaseEnemy target = Spawn(CombatSpawn.position, totalGold, experience: levelUpExp);
        if (target == null) yield break;
        yield return _spawnCutscene.Play(Player, target);
        _enemyGuide = gameObject.AddComponent<TutorialBuildPointGuide>();
        _enemyGuide.Initialize(target.transform, Player.transform, UI, "몬스터", false);
        _enemyGuide.Show(true);
        InputFilter.AllowAttack = true;
        InputFilter.AllowSkill = true;
        InputFilter.InputEnabled = true;
        UI.Show("화살표의 몬스터를 일반 공격과 스킬로 처치하세요.", TutorialFocus.None, "직접 전투");
        yield return new WaitForSeconds(1f);
        BaseEnemy skillTarget = Spawn(SecondCombatPoint.position, 0);
        if (skillTarget == null) yield break;
        yield return new WaitUntil(() => target == null || !target.gameObject.activeInHierarchy);
        UI.Show($"레벨 {Player.Level}! 레벨이 오를수록 강해집니다. 지금은 공격 속도가 빨라졌어요.", TutorialFocus.Experience, "레벨업");
        UI.SetSpotlight(true);
        InputFilter.InputEnabled = false;
        yield return new WaitForSeconds(HighlightDuration);
        UI.SetSpotlight(false);
        InputFilter.InputEnabled = true;
        UI.Show("남은 몬스터를 일반 공격과 스킬로 처치하세요.", TutorialFocus.None, "직접 전투");
        if (skillTarget != null && skillTarget.gameObject.activeInHierarchy) _enemyGuide.Track(skillTarget.transform);
        yield return new WaitUntil(() => skillTarget == null || !skillTarget.gameObject.activeInHierarchy);
        _enemyGuide.Show(false);

        yield return ShowGoldDelivery();

        SelectBuildArea(BuildPoint, 0);
        yield return Teach("1번 공격 터렛을 선택하고 노란 표시를 조준해 설치하세요.", TutorialFocus.Build, "기본 터렛 건설", true, false, true, PlayerInputAction.Turret1);
        yield return new WaitUntil(() => InstalledTurret != null);
        yield return ExitBuild();

        // 기본 터렛의 전투를 먼저 보여 주고, 동일한 경로의 강한 몬스터와 비교한다.
        do { yield return WatchTowerWave(false, false); } while (!_waveFailed && !_lastWaveKilled);
        if (_waveFailed) yield break;
        yield return WatchTowerWave(true, true);
        if (_waveFailed) yield break;
        yield return Teach("캐슬 HP가 0이 되면 게임오버입니다. 더 강한 터렛이 필요합니다.", TutorialFocus.None, "업그레이드 필요", false, false, false);

        SelectBuildArea(BuildPoint, 1);
        yield return Teach("2번 방어 터렛을 선택해 공격 터렛 위에 설치하세요.", TutorialFocus.BuildSecond, "공격 + 방어 = 관통", true, false, true, PlayerInputAction.Turret2);
        yield return new WaitUntil(() => InstalledTurret != null && InstalledTurret.GetType() == CombinedTurret.GetType());
        yield return ExitBuild();
        do { yield return WatchTowerWave(true, false); } while (!_waveFailed && !_lastWaveKilled);
        if (_waveFailed) yield break;

        // 여기까지 보상은 추가하지 않는다. 실제 건설 가격을 지불한 뒤 잔액은 0G다.
        if (_wallet.Gold != 0) { SetupFailure("재배치 단계의 잔액은 0G여야 합니다."); yield break; }
        InputFilter.InputEnabled = false;
        BaseEnemy scout = SpawnLane(false, true, false);
        if (scout == null) yield break;
        NavMeshAgent scoutAgent = scout.GetComponent<NavMeshAgent>();
        scoutAgent.isStopped = true; // 판매/새 구역 건설을 완료할 때까지 준비 시간을 보장한다.
        UI.Show("다음 몬스터는 다른 길에서 옵니다. 지금은 건설할 골드가 없습니다.", TutorialFocus.None, "다른 등장 방향");
        yield return _spawnCutscene.Play(Player, scout);
        _enemyGuide.Track(scout.transform, "다음 웨이브");
        _enemyGuide.Show(true);

        SelectBuildArea(SaleBuildPoint, 0, "판매할 기본 터렛");
        yield return Teach("1번으로 건설 모드에 들어가 판매용 터렛을 조준하세요.", TutorialFocus.Build, "터렛 판매", false, false, true, PlayerInputAction.Turret1);
        yield return new WaitUntil(() => IsLookingAt(SaleBuildPoint));
        yield return Teach("설치된 터렛을 우클릭하면 구매가의 50%를 돌려받습니다.", TutorialFocus.Sell, "터렛 판매", false, true, false, PlayerInputAction.Skill);
        yield return new WaitUntil(() => !SaleBuildPoint.HasTurret);
        InputFilter.AllowSkill = false;

        SelectBuildArea(SecondaryBuildPoint, 1);
        yield return Teach("다음 길의 노란 표시로 이동해 2번 방어 터렛을 먼저 설치하세요.", TutorialFocus.BuildSecond, "새 구역 건설", true, false, true, PlayerInputAction.Turret2);
        yield return new WaitUntil(() => IsInstalled(SecondaryBuildPoint, DefenceTurret));
        InputFilter.AllowedBuildSlot = 0;
        yield return Teach("이번에는 방어 터렛 위에 1번 공격 터렛을 선택하세요.", TutorialFocus.Build, "방어 + 공격 = 거대", false, false, true, PlayerInputAction.Turret1);
        yield return new WaitUntil(() => IsLookingAt(SecondaryBuildPoint));
        yield return Teach("정보 UI의 구매·판매 금액을 확인하고 클릭해 조합하세요.", TutorialFocus.Information, "순서에 따라 다른 조합", true, false, false, PlayerInputAction.Attack);
        yield return new WaitUntil(() => IsInstalled(SecondaryBuildPoint, ReverseCombinedTurret));
        yield return ExitBuild();

        InputFilter.InputEnabled = true;
        InputFilter.AllowAttack = true;
        InputFilter.AllowSkill = true;
        InputFilter.AllowBuild = true;
        InputFilter.AllowedBuildSlot = -1;
        InputFilter.AllowExitBuild = true;
        _buildAreas.Allow(SecondaryBuildPoint);
        UI.Show("배운 조작으로 마지막 웨이브를 진행하세요.", TutorialFocus.None, "실전 웨이브");
        _enemyGuide.Show(false);
        List<BaseEnemy> finalWave = new List<BaseEnemy> { scout };
        if (scout.gameObject.activeInHierarchy) scoutAgent.isStopped = false;
        for (int i = 0; i < 2; i++)
        {
            BaseEnemy enemy = SpawnLane(i == 0, true, false);
            if (enemy == null) yield break;
            finalWave.Add(enemy);
            yield return new WaitForSeconds(1.5f);
        }
        // 처치 횟수는 확인하지 않는다. 사망/골인으로 전원이 웨이브에서 나가면 종료한다.
        yield return new WaitUntil(() => finalWave.TrueForAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy)
            || finalWave.Exists(enemy => enemy != null && enemy.GetComponent<TutorialEnemyRoute>().Failed));
        if (finalWave.Exists(enemy => enemy != null && enemy.GetComponent<TutorialEnemyRoute>().Failed))
        { SetupFailure("실전 웨이브의 경로가 끊겼습니다."); yield break; }
        Complete();
    }

    private BaseTurret InstalledTurret => BuildPoint.GetComponentInChildren<BaseTurret>();

    private bool HasActiveSkill()
    {
        foreach (SkillAttack skill in FindObjectsOfType<SkillAttack>())
            if (skill.gameObject.scene == gameObject.scene) return true;
        return false;
    }

    private void PlaceAdditionalPoint(BuildPoint point)
    {
        if (NavMesh.SamplePosition(point.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            point.transform.position = hit.position + Vector3.up * 0.03f;
    }

    private void SelectBuildArea(BuildPoint point, int slot, string label = "건설 지점")
    {
        _buildAreas.Allow(point);
        InputFilter.AllowedBuildSlot = slot;
        _buildGuide.Track(point.transform, label);
        _buildGuide.Show(true);
    }

    private bool IsInstalled(BuildPoint point, BaseTurret prefab)
    {
        if (!point.HasTurret) return false;
        foreach (Transform child in point.transform)
        {
            BaseTurret turret = child.GetComponent<BaseTurret>();
            if (turret != null && turret.GetType() == prefab.GetType() && turret.name.StartsWith(prefab.name)) return true;
        }
        return false;
    }

    private bool IsLookingAt(BuildPoint point)
    {
        Camera camera = Camera.main;
        if (camera == null) return false;
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        return Physics.Raycast(ray, out RaycastHit hit, 5f, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Collide)
            && hit.collider.GetComponentInParent<BuildPoint>() == point;
    }

    private IEnumerator ExitBuild()
    {
        _buildGuide.Show(false);
        yield return Teach($"{TutorialUI.KeyName(_input.ExitBuildKey)}로 건설 모드를 종료하세요.", TutorialFocus.ExitBuild, "전투 준비", false, false, false, PlayerInputAction.ExitBuild);
        yield return new WaitUntil(() => Player.GetComponent<PlayerAttackMode>().enabled);
        InputFilter.AllowExitBuild = false;
        _buildAreas.Allow(null);
    }

    private IEnumerator ShowGoldDelivery()
    {
        InputFilter.InputEnabled = false;
        Bounds bounds = new Bounds(Player.transform.position, Vector3.zero);
        foreach (GoldDrop drop in FindObjectsOfType<GoldDrop>())
            if (drop.gameObject.scene == gameObject.scene) bounds.Encapsulate(drop.transform.position);
        float distance = Mathf.Max(10f, bounds.extents.magnitude * 1.6f + 5f);
        Vector3 offset = -Player.transform.forward * distance + Player.transform.right * distance * 0.3f
            + Vector3.up * distance * 0.65f;
        _spawnCutscene.BeginView(Player, bounds.center + Vector3.up, offset);
        UI.Show("웨이브가 끝나면 골드가 캐릭터에게 날아옵니다.", TutorialFocus.None, "골드 배달");
        yield return new WaitForSeconds(1.5f); // 설명/카메라 구도를 먼저 보여 준다.
        yield return GoldDelivery.Deliver();
        yield return new WaitForSeconds(0.75f);
        _spawnCutscene.RestoreCamera();
    }

    private BaseEnemy SpawnLane(bool armored, bool secondary, bool holdAtGoal)
    {
        Vector3 pass = (secondary ? SecondaryLanePoint : PrimaryLanePoint).position;
        BaseEnemy enemy = Spawn(pass, 0, armored, secondary);
        if (enemy == null) return null;
        TutorialEnemyRoute route = enemy.gameObject.AddComponent<TutorialEnemyRoute>();
        if (!route.Initialize(enemy, new[] { pass, CastleGoal.position }, Castle, holdAtGoal))
        {
            enemy.ReturnToPool();
            SetupFailure("몬스터가 건설 구역에서 캐슬까지 이동할 수 없습니다.");
            return null;
        }
        return enemy;
    }

    private IEnumerator WatchTowerWave(bool armored, bool showCastleHit)
    {
        _lastWaveKilled = false;
        InputFilter.InputEnabled = true;
        InputFilter.AllowAttack = false;
        InputFilter.AllowSkill = false;
        InputFilter.AllowBuild = false;
        InputFilter.AllowExitBuild = false;
        BaseEnemy enemy = SpawnLane(armored, false, showCastleHit);
        if (enemy == null) { _waveFailed = true; yield break; }
        TutorialEnemyRoute route = enemy.GetComponent<TutorialEnemyRoute>();
        UI.Show(armored ? (showCastleHit ? "단단한 몬스터는 기본 터렛으로 막기 어렵습니다." : "관통 터렛으로 같은 단단한 몬스터를 다시 막아보세요.")
            : "기본 터렛이 일반 몬스터를 자동으로 공격합니다.", TutorialFocus.None, armored ? "단단한 몬스터 웨이브" : "기본 몬스터 웨이브");
        yield return new WaitUntil(() => !enemy.gameObject.activeInHierarchy || route.Arrived || route.Failed);
        if (route.Failed) { SetupFailure("터렛 전투 경로가 끊겼습니다."); yield break; }
        _lastWaveKilled = enemy.MonHp <= 0f;
        if (showCastleHit && _lastWaveKilled)
        { SetupFailure("단단한 몬스터가 골인 전에 죽었습니다. 기본 터렛과 방어력 밸런스를 확인하세요."); yield break; }
        if (showCastleHit && route.Arrived && enemy.gameObject.activeInHierarchy)
        {
            InputFilter.InputEnabled = false;
            _spawnCutscene.BeginView(Player, enemy.transform.position + Vector3.up, new Vector3(-4f, 4f, -6f));
            UI.Show("놓친 몬스터가 캐슬에 도착하면 캐슬 HP가 감소합니다.", TutorialFocus.None, "캐슬 피해");
            yield return new WaitForSeconds(0.8f);
            route.ResolveGoal();
            yield return new WaitForSeconds(1.5f);
            _spawnCutscene.RestoreCamera();
        }
        yield return new WaitForSeconds(0.5f);
    }

    private void SetupFailure(string reason)
    {
        _waveFailed = true;
        Debug.LogError($"TutorialManager: {reason}", this);
        UI.Show("튜토리얼 설정을 확인하세요. ESC로 스킵할 수 있습니다.", TutorialFocus.None, "설정 확인 필요");
    }

    private IEnumerator Teach(string text, TutorialFocus focus, string progress, bool attack, bool skill, bool build, PlayerInputAction? prompt = null)
    {
        InputFilter.InputEnabled = false;
        InputFilter.AllowAttack = attack;
        InputFilter.AllowSkill = skill;
        InputFilter.AllowBuild = build;
        InputFilter.AllowExitBuild = prompt == PlayerInputAction.ExitBuild;
        UI.Show(text, focus, progress);
        if (prompt.HasValue)
        {
            InputFilter.BeginPrompt(prompt.Value);
            UI.SetSpotlight(true);
            yield return new WaitUntil(() => !InputFilter.WaitingForPrompt && !_modal);
        }
        else
        {
            // 조작할 키가 없는 골드/터렛/캐슬 설명은 이동/시점을 잠그지 않는다.
            InputFilter.InputEnabled = true;
            yield return new WaitForSeconds(HighlightDuration);
        }
    }

    private BaseEnemy Spawn(Vector3 position, int gold, bool armored = false, bool secondary = false, float experience = 0f)
    {
        Vector3 spawn = (secondary ? SecondarySpawn : OutsideSpawn).position;
        // 경계 밖에서 출현하고, 기존 전투 지점까지 실제 NavMesh 경로로 접근한다.
        if (!NavMesh.SamplePosition(spawn, out NavMeshHit spawnHit, 5f, NavMesh.AllAreas)
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
        BaseEnemy enemy = Instantiate(armored ? ArmoredEnemyPrefab : EnemyPrefab, spawnHit.position, Quaternion.identity, staging.transform);
        enemy.MonGold = gold;
        enemy.MonExp = experience;
        enemy.MonHp = armored ? 100f : 20f;
        enemy.MonMaxHp = enemy.MonHp;
        enemy.MonDefend = armored ? 25f : 0f;
        enemy.MonCastleDam = 10f;
        enemy.MonSpeed = armored ? 3f : 1.5f;
        staging.SetActive(true);
        enemy.InitData();
        MonsterMove movement = enemy.GetComponent<MonsterMove>();
        if (movement != null) movement.enabled = false;
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
        agent.isStopped = false;
        agent.SetPath(approach);
        _enemies.Add(enemy);
        return enemy;
    }

    private BaseEnemy SpawnFailure(string reason)
    {
        Debug.LogError($"TutorialManager: {reason} 등장/전투 지점 Transform을 확인하세요.", this);
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
        if (_spawnCutscene != null) _spawnCutscene.RestoreCamera();
        foreach (BaseEnemy enemy in _enemies)
            if (enemy != null && enemy.gameObject.activeInHierarchy) enemy.ReturnToPool();
        Complete();
    }

    private void Complete()
    {
        if (_completed) return;
        _completed = true;
        if (_buildGuide != null) _buildGuide.Show(false);
        if (_enemyGuide != null) _enemyGuide.Show(false);
        _modal = false;
        InputFilter.CancelPrompt();
        Player.GetComponent<PlayerBuildMode>().enabled = false;
        UI.SkipDialog.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerPrefs.SetInt(CompletionKey, 1);
        PlayerPrefs.Save();
        UI.HideExplanation();
        Debug.Log("이제 튜토리얼 클리어!", this);
        OnCompleted.Invoke(); // 타이틀/게임 씬 진입 연결은 보류한다.
    }

    public void NotifyPracticeShot()
    {
        if (!_attackPractice || _modal || !InputFilter.InputEnabled || Time.timeScale == 0f) return;
        _practiceShots++;
        UI.Show("마우스 왼쪽을 누르거나 유지해 5발 발사하세요.", TutorialFocus.Attack, $"일반 공격 연습 {Mathf.Min(_practiceShots, 5)} / 5");
    }

    private void OnSkillUsed(float duration)
    {
        if (!_skillPractice || _modal) return;
        _practiceSkills++;
        UI.Show("우클릭을 누르고 바닥을 조준한 뒤 놓으세요. 3회 시전, 쿨타임 3초.", TutorialFocus.Skill, $"스킬 연습 {Mathf.Min(_practiceSkills, 3)} / 3");
    }
    private void OnPromptAccepted() => UI.SetSpotlight(false);
    private void OnGoldChanged(int total, int amount) => UI.SetGold(total);

    private void OnDestroy()
    {
        if (_wallet != null) _wallet.OnGoldChanged -= OnGoldChanged;
        if (Player != null) Player.OnSkillCooldownStarted -= OnSkillUsed;
        if (InputFilter != null) InputFilter.PromptAccepted -= OnPromptAccepted;
        if (Castle != null && UI != null)
            Castle.OnCastleHealthChanged -= UI.SetCastleHealth;
        if (UI != null)
        {
            UI.SkipButton.onClick.RemoveListener(Skip);
            UI.ContinueButton.onClick.RemoveListener(CancelSkip);
        }
        if (_modal) Time.timeScale = _resumeTimeScale;
    }
}
