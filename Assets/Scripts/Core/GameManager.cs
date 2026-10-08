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
    public GameState currentState;  // 현재 게임 상태
    // --- 판넬 ------------
    [SerializeField] private GameObject _startPanel;    // 시작 화면 UI
    [SerializeField] private GameObject _pausePanel;    // 일시정지 UI
    //---------------------
    
    // --- 인게임 UI -----------------------------------------------
    [SerializeField] private GameObject _inGameUI;      // 인게임 UI
    [SerializeField] private GameObject _buildModeUI;   // 빌드모드 UI
    [SerializeField] private GameObject _attackModeUI; // 공격모드 UI
    [SerializeField] private Image _skillCooldownImage; // 스킬 쿨타임 이미지
    [SerializeField] private TextMeshProUGUI _skillCooldownText; // 스킬 쿨타임 텍스트
    
    [SerializeField] private Image _buildCooldownImage;
    [SerializeField] private TextMeshProUGUI _buildCooldownText;

    [SerializeField] private Image _cursorImage;
    [SerializeField] private Sprite[]  _cursorSprites;
    [SerializeField] private GameObject _crosshair;
    
    [Serializable]
    private struct TurretInfo
    {
        public Image TurretImage;
        public TextMeshProUGUI TurretText;
        public float TurretCooldown;
    }

    [SerializeField] private TurretInfo[] _turretInfo;

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
        
        //if (currentState != null)
        BindPlayerModeUI();
    }
    
    private void Start()
    {
        Init();
        //ResetToTitle(); // TODO 게임 데이터 초기화 연결해야함. (이벤트로)
        ChangeState(GameState.Ready);
    }

    private void Update()
    {
        PauseManager();

    }
    // --------------------------------------------------------
    
    public event Action OnWaveStarted; // 추후에 TODO REFACTOR
    
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
                //WaveManager.Instance.StartFirstWavePrepare();
                // 라운드 시작 전 로직
                // 웨이브 준비 UI 잠시 띄웠다가 지우기 or
                // 30초 위에 띄우고 웨이브 준비 단계
                break;
            case GameState.OnWave:
                // 웨이브 시작 때 로직
                // 웨이브 중임을 알리는 텍스트 : 남은 몹
                // 만약 클리어면 클리어 로직 처리하고 다음 웨이브 준비
                // 만약 클리어 못했으면 게임 오버 상태로 변경
                break;
            case GameState.Paused:
                // 일시 정지 로직
                break;
            case GameState.GameOver:
                // 게임 오버 로직
                break;
        }
    }

    private void BindPlayerModeUI()
    {
        PlayerStatus.OnPlayerModeChanged += SetPlayerModeUI;
        PlayerStatus.OnSkillCooldownStarted += SetPlayerSkillCooldownUI;
        PlayerStatus.OnBuildCooldownStarted += SetPlayerBuildCooldownUI;
        PlayerStatus.OnCursorChanged += SetCrosshair;
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
        
        _buildRemainingTime = _turretInfo[slotNum].TurretCooldown;
        _turretInfo[slotNum].TurretText.gameObject.SetActive(true); // 기본값 false
        _turretInfo[slotNum].TurretImage.gameObject.SetActive(true);
        while (_buildRemainingTime > 0f)    // 스킬 쿨 돌때만
        {
            _buildRemainingTime -= Time.deltaTime;// 스킬 시전했으면 쿨타임 보여주기
            // 스킬 쿨타임 표기
            _turretInfo[slotNum].TurretText.text = _buildRemainingTime.ToString("F0");
            // 스킬 쿨타임 fillAmount
            _turretInfo[slotNum].TurretImage.fillAmount = Mathf.Clamp01(_buildRemainingTime / _turretInfo[slotNum].TurretCooldown);
            yield return null;
        }
        _turretInfo[slotNum].TurretText.gameObject.SetActive(false); // 기본값 false
        _turretInfo[slotNum].TurretImage.gameObject.SetActive(false);
    }
    
    
    private IEnumerator TurretInitRoutine()
    {
        yield return new WaitUntil(() => TurretCombinationTable.Instance != null);

        //_turretCombinationTable = TurretCombinationTable.Instance;
        
        for (int i = 0; i < _turretInfo.Length; i++)
        {
            _turretInfo[i].TurretCooldown = TurretCombinationTable.Instance.GetSelectedTurret(i).BuildCooldown;
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
    
    // 게임 리셋
    private void ResetToTitle()
    {
        Init();
        // 게임 시간 정지
        Pause();
        canPause = false;
        
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
    GameOver
}

public enum CrosshairType
{
    Default,
    Attack,
    Build
}

