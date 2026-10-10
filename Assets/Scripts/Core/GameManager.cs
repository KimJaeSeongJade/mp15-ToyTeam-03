using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : SingletonBehaviour<GameManager>
{
    // 레벨 관리는 게임매니저쪽에서
    // 초기 골드는 플레이어가 갖고있음
    // 리스트에 오브젝트 넣어서 
    // 리스트 크기만큼 골드가 뜨게
    
    // wave 끝났을 때 골드 
    public const int GOLD_AMOUNT = 20;
    // public const float CASTLE_HP = 500;
    public GameState currentState;  // 현재 게임 상태
    // --- 판넬 ------------
    [SerializeField] private GameObject _startPanel;    // 시작 화면 UI
    [SerializeField] private GameObject _pausePanel;    // 일시정지 UI
    
    [SerializeField] private GameObject _gameResultPanel; // 게임 오버 / 클리어 UI 
    [SerializeField] private TextMeshProUGUI _gameResultText;
    //---------------------
    
    // --- 플레이어 경험치 / 레벨 UI -------------------
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private Image _expBarImage;
    // ------------------------------------------------
    
    // --- 인게임 UI -----------------------------------------------
    [SerializeField] private GameObject _inGameUI;      // 인게임 UI
    [SerializeField] private GameObject _buildModeUI;   // 빌드모드 UI
    [SerializeField] private GameObject _attackModeUI; // 공격모드 UI
    [SerializeField] private Image _skillCooldownImage; // 스킬 쿨타임 이미지
    [SerializeField] private TextMeshProUGUI _skillCooldownText; // 스킬 쿨타임 텍스트

    [SerializeField] private Image _cursorImage;
    [SerializeField] private Sprite[]  _cursorSprites;
    [SerializeField] private GameObject _crosshair;
    
    [Serializable]
    private struct TurretInfo
    {
        public Image TurretImage;
        public TextMeshProUGUI TurretText;
        public TextMeshProUGUI TurretGoldText; //설치 골드
        public float TurretCooldown;
    }

    [SerializeField] private TurretInfo[] _turretInfo;

    private Coroutine[] _turretCooldownCoroutines;
    
    // -----------------------------------------------------------------
    
    [SerializeField] private CastleHp _castleHp;
    [SerializeField] private TextMeshProUGUI _castleHpText;
    
    // [SerializeField] private CastleHp _castleHp;
    
    // 게임 초기화를 위한 웨이브 매니저에게 상태 변경 전달해줘서 웨이브쪽에서 웨이브 관련 코루틴 종료처리
    public event Action<GameState> OnGameStateChanged;
    
    public bool IsStarted { get; private set; }
    
    // -------------------------------------------------------------
    // 구조체 또는 클래스를 적용하여 UI 이미지와 텍스트를 하나의 필드로 정리하기 TODO
    
    [field:SerializeField] public PlayerStatus PlayerStatus{get; private set;}
    //[SerializeField] private TurretCombinationTable _turretCombinationTable;
    
    private PlayerWallet _wallet;
    private float _skillRemainingTime;
    private float _buildRemainingTime;
    private float _buildCooldown;
    
    
    public int _gold => _wallet.Gold;
    
    // 입력받은 슬롯 넘버
    
    private bool IsPause;
    private bool canPause;  // 일시정지 위한 게임 시작 여부 검증
    
    //private bool IsTitle;
    
    // 웨이브 에서 시작했는지 확인 용도 
    // 웨이브 종료시 받아와야해서 set으로 조건 추가할지 결정
    // public bool IsStarted
    // {
    //     get { return hasStart; }
    // }
    // ---- 이벤트 함수 ---------------------------------------

    protected override void Awake()
    {
        base.Awake();

        CacheComponents();

        InitStart();

        //if (currentState != null)

    }

    private void InitStart()
    {
        BindPlayerModeUI();
        ChangeState(GameState.Ready);
        RefreshCastleHealthUI(_castleHp.CurrentHP, _castleHp.MaxHp);
        // 플레이어 초기 레벨 UI
        RefreshLevelUI(PlayerStatus.Level);

        // 플레이어 초기 경험치 UI
        RefreshExpUI(PlayerStatus.Exp, PlayerStatus.RequiredExp);
    }

    private void Update()
    {
        PauseManager();

    }
    // --------------------------------------------------------
    
    public event Action OnWaveStarted; // 추후에 TODO REFACTOR
    
    // 플레이어 레벨 변경 시 UI 갱신
    private void RefreshLevelUI(int level)
    {
        _levelText.text = $"Lv.{level}";
    }
    
    // 플레이어 경험치 변경 시 UI 갱신
    private void RefreshExpUI(float currentExp, float requiredExp)
    {
        // 최대 레벨일 경우 (필요 경험치 0)
        if (requiredExp <= 0f)
        {
            _expText.text = "MAX";
            _expBarImage.fillAmount = 1f;
            return;
        }

        // 현재 경험치 / 필요 경험치 텍스트 갱신
        _expText.text = $"{currentExp:F0}/{requiredExp:F0}";

        // 경험치 바 fillAmount 갱신
        _expBarImage.fillAmount =
            Mathf.Clamp01(currentExp / requiredExp);
    }
    
    private void PauseManager()
    {
        if (!canPause) return;
        // esc 누르면 검증
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 정지 상태면 재개
            if (IsPause)
            {
                ResumeGame();
            }
            // 정지 상태 아니면 정지
            else
            {
                PauseGame();
            }
        }
        // esc 누르면 update가 아닌 delegate 이벤트로 추가 TODO
    }
    
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        
        switch (currentState)
        {
            case GameState.Ready:
                // 준비 상태 로직
                ResetToTitle();
                break;
            case GameState.WavePreparation:
                _gameResultPanel.SetActive(false);
                WaveManager.Instance.StartFirstWavePrepare();
                // 라운드 시작 전 로직
                // 웨이브 준비 UI 잠시 띄웠다가 지우기 or
                // 30초 위에 띄우고 웨이브 준비 단계
                break;
            case GameState.Paused:
                // 일시 정지 로직
                break;
            case GameState.GameOver:
                // 게임 오버 로직
                Pause();
                ShowGameResult("Game Over!!");
                break;
            case GameState.GameClear:
                Pause();
                // 게임 클리어 로직
                ShowGameResult("Clear!!");
                break;
        }
        // 웨이브 매니저가 상태 변경 정보 체크
        OnGameStateChanged?.Invoke(currentState);
    }

    private void BindPlayerModeUI()
    {
        // 플레이어 스킬, 상태, 조준점 ------------------
        PlayerStatus.OnPlayerModeChanged += SetPlayerModeUI;
        PlayerStatus.OnSkillCooldownStarted += SetPlayerSkillCooldownUI;
        PlayerStatus.OnBuildCooldownStarted += SetPlayerBuildCooldownUI;
        PlayerStatus.OnCursorChanged += SetCrosshair;
        // ----------------------------------------
        
        // 플레이어 경험치 / 레벨 UI -------------------
        PlayerStatus.OnExpChanged += RefreshExpUI;
        PlayerStatus.OnLevelChanged += RefreshLevelUI;
        // ----------------------------------------

        // 게임 오버 판정 체크 --------------
        _castleHp.OnCastleHealthChanged += RefreshCastleHealthUI;
        _castleHp.OnCastleHealthChanged += CheckGameOver;
        //------------------------------

        WaveManager.Instance.OnAllWavesCleared += CheckGameClear;
        WaveManager.Instance.OnNextWaveRequested += HandleNextWaveRequested;
    }

    private void HandleNextWaveRequested()
    {
        ChangeState(GameState.WavePreparation);
    }
    

    private void SetCrosshair(CrosshairType crosshairType)
    {   // -1 : 기본 / 0 : 공격 / 1 : 건설
        switch (crosshairType)
        {
            case CrosshairType.Attack:
                _cursorImage.sprite =  _cursorSprites[(int)CrosshairType.Attack];
                break;
            case CrosshairType.Build:
                _cursorImage.sprite = _cursorSprites[(int)CrosshairType.Build];
                break;
            case CrosshairType.Default:
                _cursorImage.sprite = _cursorSprites[(int)CrosshairType.Default];
                break;
        }
    }
    

    private void SetPlayerModeUI(bool isAttackMode)
    {
        // 공격 모드
        if (isAttackMode)
        {
            _attackModeUI.gameObject.SetActive(true);
            _buildModeUI.gameObject.SetActive(false);
        }
        // 빌드 모드
        else
        {
            _attackModeUI.gameObject.SetActive(false);
            _buildModeUI.gameObject.SetActive(true);
        }
    }

    private void SetPlayerBuildCooldownUI(int slotNum, float buildCooldown)
    {
        // 잘못된 슬롯 번호 방지
        if (slotNum < 0 || slotNum >= _turretInfo.Length)
            return;

        // 해당 슬롯의 기존 쿨타임 코루틴 중단
        if (_turretCooldownCoroutines[slotNum] != null)
        {
            StopCoroutine(_turretCooldownCoroutines[slotNum]);
        }

        // 해당 슬롯의 새로운 쿨타임 코루틴 시작
        _turretCooldownCoroutines[slotNum] =
            StartCoroutine(UpdateUIRoutine(slotNum, buildCooldown));
    }
    
    
    // 레벨업 할 때 평타 업글/ 스킬 업글 어떻게 받아올지 TODO
    private void SetPlayerSkillCooldownUI(float skillCooldown)
    {
        StartCoroutine(UpdateUIRoutine(skillCooldown));
    }
    
    /// <summary>
    /// 스킬 쿨타임 UI 코루틴
    /// </summary>
    /// <param name="skillCooldown"></param>
    /// <returns></returns>
    private IEnumerator UpdateUIRoutine(float skillCooldown)
    {
        _skillRemainingTime = skillCooldown;
        _skillCooldownImage.gameObject.SetActive(true);
        _skillCooldownText.gameObject.SetActive(true); // 기본값 false
        while (_skillRemainingTime > 0f)    // 스킬 쿨 돌때만
        {
            _skillRemainingTime -= Time.deltaTime;// 스킬 시전했으면 쿨타임 보여주기
            // 스킬 쿨타임 표기
            _skillCooldownText.text = _skillRemainingTime.ToString("F0");
            // 스킬 쿨타임 fillAmount
            _skillCooldownImage.fillAmount = Mathf.Clamp01(_skillRemainingTime / skillCooldown);
            yield return null;
        }
        _skillCooldownText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 터렛 쿨타임 UI 코루틴 (함수 오버로딩)
    /// </summary>
    /// <param name="slotNum"></param>
    /// <param name="buildCooldown"></param>
    /// <returns></returns>
    private IEnumerator UpdateUIRoutine(int slotNum, float buildCooldown)
    {
        // 슬롯별 독립적인 쿨타임 관리
        float remainingTime = _turretInfo[slotNum].TurretCooldown;
        float _buildCooldown = _turretInfo[slotNum].TurretCooldown;

        // 해당 슬롯의 터렛 UI 정보
        TurretInfo turret = _turretInfo[slotNum];

        // 쿨타임이 없으면 UI 비활성화
        if (_buildCooldown <= 0f)
        {
            turret.TurretText.gameObject.SetActive(false);
            turret.TurretImage.gameObject.SetActive(false);
            yield break;
        }

        // 터렛 설치 쿨타임 UI 활성화 (기본값 false)
        turret.TurretImage.gameObject.SetActive(true);
        turret.TurretText.gameObject.SetActive(true);

        while (remainingTime > 0f) // 터렛 쿨타임 진행 중
        {
            remainingTime -= Time.deltaTime; // 남은 쿨타임 감소
            remainingTime = Mathf.Max(0f, remainingTime);

            // 터렛 쿨타임 표기
            turret.TurretText.text =
                Mathf.CeilToInt(remainingTime).ToString();

            // 터렛 쿨타임 fillAmount
            turret.TurretImage.fillAmount =
                Mathf.Clamp01(remainingTime / _buildCooldown);

            yield return null;
        }

        // 쿨타임 종료 후 UI 비활성화
        turret.TurretImage.fillAmount = 0f;
        turret.TurretText.gameObject.SetActive(false);
        turret.TurretImage.gameObject.SetActive(false);
    }
    
    
    private IEnumerator TurretInitRoutine()
    {
        // TurretCombinationTable 초기화 대기
        yield return new WaitUntil(() =>
            TurretCombinationTable.Instance != null);

        //_turretCombinationTable = TurretCombinationTable.Instance;

        for (int i = 0; i < _turretInfo.Length; i++)
        {
            // 선택된 기본 터렛 데이터 가져오기
            BaseTurret turretData =
                TurretCombinationTable.Instance.GetSelectedTurret(i);

            // 터렛 데이터가 없으면 다음 슬롯으로
            if (turretData == null)
            {
                Debug.LogWarning($"터렛 슬롯 {i} 데이터가 없습니다.");
                continue;
            }

            // 터렛 기본 설치 쿨타임 초기화
            _turretInfo[i].TurretCooldown =
                turretData.BuildCooldown;

            // 터렛 설치 골드 UI 상시 표시
            // BuildGoldCost는 BaseTurret의 실제 비용 프로퍼티로 교체 필요
            //_turretInfo[i].TurretGoldText.text = $"{turretData.BuildGoldCost}G";

            // 터렛 쿨타임 UI 초기 상태 (기본값 false)
            _turretInfo[i].TurretImage.fillAmount = 0f;
            _turretInfo[i].TurretText.gameObject.SetActive(false);
            _turretInfo[i].TurretImage.gameObject.SetActive(false);
        }
    }
    
    // 초기 화면
    public void StartGame()
    {
        // 게임 시작 버튼 누르면
        // 판넬 끄고
        _startPanel.gameObject.SetActive(false);
        // 게임 시간 시작
        Run();
        _inGameUI.SetActive(true);
        // 시작 시 빌드 모드 UI와 스킬 쿨타임, 터렛 쿨타임 false 처리
        _buildModeUI.gameObject.SetActive(false);
        _skillCooldownImage.gameObject.SetActive(false);
        _skillCooldownText.gameObject.SetActive(false); 
        //_buildCooldownText.gameObject.SetActive(false);
        canPause = true;    // 시작하면 일시정지 가능하게
        ChangeState(GameState.WavePreparation);
    }
    
    // 진행
    public void Run()
    {
        Time.timeScale = 1;     // 게임 시간 on
        LockCursor();           // 마우스 커서 잠금
    }

    // 일시정지
    public void Pause()
    {
        Time.timeScale = 0;
        UnlockCursor();
        _crosshair.SetActive(false);
    }
    
    //  UI 매니저 쪽으로 추후 수정
    private void RefreshCastleHealthUI(float currentHp, float maxHp)
    {
        _castleHpText.text = $"{currentHp:F0}/{maxHp:F0}";
    }


    private void ShowGameResult(string resultMessage)
    {
        _inGameUI.gameObject.SetActive(false);
        _gameResultText.text = resultMessage;
        _gameResultPanel.SetActive(true);
    }
    
    private void CheckGameClear()
    {
        // 이미 게임 오버나 클리어 상태면 return
        if (currentState == GameState.GameOver ||
            currentState == GameState.GameClear)
            return;
        ChangeState(GameState.GameClear);
    }

    
    // 게임 오버 체크

    private void CheckGameOver(float currentHp, float maxHp)
    {
        // 캐슬 체력 0 이하 & 현재 게임 상태가 게임오버가 아니면 게임 오버 처리
        if (currentHp <= 0f &&
            currentState != GameState.GameOver &&
            currentState != GameState.GameClear)
        {
            ChangeState(GameState.GameOver);
        }
    }
    
    // 현재 hp 변경되면 값을 못 받아와서 초기값 받아와야함.
    // 그냥 받아오는 구조 대신 이거도 상수로 내가 가지고 있는게 낫지 않을까
    // 근데 이전 회의 때 게임 매니저가 캐슬 hp 보유하지 않기로 결정.

    
    // 결과 창 버튼이 할 일
    public void ReturnToTitle()
    {
        ChangeState(GameState.Ready);
    }
    
    
    // 게임 리셋
    private void ResetToTitle()
    {
        Init();
        // 게임 시간 정지
        Pause();
        canPause = false;
        IsPause = false;
        
        _gameResultPanel.SetActive(false);

        // TODO RestartGame();
        // OnGameRestarted?.Invoke(); 로 해서 이벤트로 처리하는게 좋을듯

        // 대신 전체 게임 진행상황도 초기화해야함.
        // 웨이브 = 0, 타이머 = 0, 생존 몬스터 수 0, SpawnFinished false
        // player HP,Gold,위치 초기화
        // 스포너 : Spawn 중지, 생성 몬스터 풀로 복귀
        // Tower 설치된 터렛 제거
        // UI는 웨이브/골드/HP/타워 쿨타임 초기화

        // 웨이브 매니저 코루틴 시간 계산 쪽은 StopAllCoroutines()로 하면될듯
        //
    }
    
    // 일시정지 상태에서 게임 재개
    public void ResumeGame()
    {
        Run();
        LockCursor();
        _pausePanel.gameObject.SetActive(false);
        _crosshair.SetActive(true);
        IsPause = false;
    }
    
    // 게임 일시정지
    private void PauseGame()
    {
        Pause();
        UnlockCursor();
        _crosshair.gameObject.SetActive(false);
        _pausePanel.SetActive(true);
        IsPause = true;
    }

    private void CacheComponents()
    {
        _wallet = PlayerStatus.GetComponent<PlayerWallet>();
        // 슬롯별 쿨타임 코루틴 관리
        _turretCooldownCoroutines = new Coroutine[_turretInfo.Length];
        StartCoroutine(TurretInitRoutine());
    }

    private void Init()
    {
        // 판넬 켜고
        _startPanel.gameObject.SetActive(true);
        // 일시정지 판넬은 꺼진 상태
        _pausePanel.gameObject.SetActive(false);
        // 인게임 판넬도 꺼진 상태
        _inGameUI.SetActive(false);
        // 결과 판넬 꺼진 상태
        _gameResultPanel.SetActive(false);
        // 스킬 쿨타임 반영 ui 꺼짐 상태
        _skillCooldownImage.gameObject.SetActive(false);
        _skillCooldownText.gameObject.SetActive(false);
        // 결과 출력 판넬 꺼진 상태
        _gameResultPanel.SetActive(false);
    }

    
    // --- 마우스 커서 잠금/해제 -----------------------
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
       // StartCoroutine(UpdateCursorRoutine());
    }

    // private IEnumerator UpdateCursorRoutine()
    // {
    //     yield return new WaitUntil(() => !IsPause);
    //
    // }
    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    // ----------------------------------------------
}

public enum GameState
{
    Ready,
    WavePreparation,
    OnWave,
    Paused,
    GameOver,
    GameClear
}

public enum CrosshairType
{
    Default,
    Attack,
    Build
}

