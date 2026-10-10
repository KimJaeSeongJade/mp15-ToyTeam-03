using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : SingletonBehaviour<WaveManager>
{
    [SerializeField] private MonsterSpawner _monsterSpawner;

    // 웨이브 대기 시간
    [SerializeField] private float _prepareTime = 30f;
    [SerializeField] private TextMeshProUGUI _prepareTimeUI;
    [SerializeField] private TextMeshProUGUI _goldUI;
    [SerializeField] private TextMeshProUGUI _castleHealthUI;
    
    [SerializeField] private GameManager _gameManager;
    
    //스테이지 클리어
    [SerializeField] private TextMeshProUGUI _stageClearText;

    // 정보 받아와야하면 추후에 수정

    private int _currentWave = 0;
    private bool _isWaveEnded = false;
    
    [SerializeField] private float _remainingTime;
    [SerializeField] private int _aliveMonsterCount;
    [SerializeField] private bool _isSpawnFinished;

    public int CurrentWave => _currentWave;
    public float RemainingTime => _remainingTime;
    
    // 웨이브 시작 / 종료 액션처리
    // delegate
    public static event Action OnCamActivate;
    public event Action OnWaveStarted;
    public event Action OnWaveEnded;
    
    public event Action OnNextWaveRequested;
    
    public event Action<int> OnWaveChanged;
    public event Action<float> OnPrepareTimeChanged;
    
    public event Action OnAllWavesCleared;
    

    // --- 이벤트 함수 ---------------------------------------------
    
    private void Awake()
    {
        SetSingleton();
    }

    private void Update()
    {
        PrepareWaveUI();
        RefreshGoldUI();
    }

    // 게임 매니저 게임 시작 -> 이벤트 구독 처리
    
    
    private void OnEnable()
    {
        // 게임매니저에서 시작 전달받아와야함.
        //if (GameManager.Instance.currentState != GameState.WavePreparation) return;

        //StartFirstWavePrepare();
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged += UpdateGameStateChanged;
        }

        // GameManager 게임 시작 이벤트 구독 

        // MonsterSpawner 생성 완료 이벤트 구독  // 장수님과 체크해서 이벤트 구독 처리해서 예시 화면 

        // Monster 사망 관련 이벤트 구독         // OnReturn으로 +- 하면서 인원 수 체크하기
        
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (_gameManager != null)
        {
            _gameManager.OnGameStateChanged -= UpdateGameStateChanged;
        }
    }
    

    
    // --------------------------------------------------------------

    private void UpdateGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
                StopWaveProgress();
                ResetWaveData();
                break;

            case GameState.GameOver:
            case GameState.GameClear:
                StopWaveProgress();
                break;
        }
    }
    
    // 웨이브 데이터 초기화 --------------
    private void ResetWaveData()
    {
        _currentWave = 0;
        _remainingTime = 0f;
        _aliveMonsterCount = 0;
        _isSpawnFinished = false;
        _isWaveEnded = false;
    }
    
    private void StopWaveProgress()
    {
        StopAllCoroutines();
        //_monsterSpawner.StopSpawning();
        // 포탈 이펙트 끄고, 스포너쪽 Stop All 코루틴 : 퍼블릭으로 만들어주세요. (필요한 거만 StopCorutine 처리하는게 더 좋아보이긴 함)
        
        // 스포너쪽에서 처리할 거
        /*public void StopSpawning()
        {
            StopAllCoroutines();
            // 포털 이펙트 비활성화
        }*/
    }
    
    
    
    // 게임 시작 시 호출
    public void StartFirstWavePrepare()
    {
        StartCoroutine(PrepareNextWave());
    }

    // 다음 웨이브 준비
    // 코루틴으로
    private IEnumerator PrepareNextWave()
    {
        _remainingTime = _prepareTime;
        bool _isSwitcher =  false;

        _prepareTimeUI.transform.parent.gameObject.SetActive(true);
        while (_remainingTime > 0f)
        {
            OnPrepareTimeChanged?.Invoke(_remainingTime);
                        if (_remainingTime < 8f && _isSwitcher == false)
                        {
                            _isSwitcher = true;
                            OnCamActivate?.Invoke();
                        }
                        _remainingTime -= Time.deltaTime;
                        yield return null;
        }
        StartWave();
    }

    private void PrepareWaveUI()
    {
        // 만약 게임 시작 됐으면 UI 처리한다.
        // 조건식 추후에 고민
        if (_remainingTime > 0f)
        {
            _prepareTimeUI.text =
                $"Wave : {_currentWave+1} Starts in {_remainingTime:F0}...";
        }
        else
        {
            _prepareTimeUI.transform.parent.gameObject.SetActive(false);
        }
    }


    // 실제 웨이브 시작
    private void StartWave()
    {
        /*
        // MonsterSpawner에게 현재 웨이브 시작 요청

        // TODO 여기 호출 부 수정
        _monsterSpawner.SpawnWave(_currentWave+1, AddMonster, SetSpawnFinished);
        
        _currentWave++; // 웨이브 증가
        // 종료조건 파악을 위한 몬스터 수 확인
        _aliveMonsterCount = 0; 
        // 종료 조건
        _isSpawnFinished = false;

        OnWaveChanged?.Invoke(_currentWave);
        OnWaveStarted?.Invoke();
        */
        _isWaveEnded = false;
        _aliveMonsterCount = 0;
        _isSpawnFinished = false;

        _currentWave++;

        OnWaveChanged?.Invoke(_currentWave);
        OnWaveStarted?.Invoke();

        _monsterSpawner.SpawnWave(
            _currentWave,
            AddMonster,
            SetSpawnFinished
        );
    }

    // 몬스터 생성 시 호출
    public void AddMonster(BaseEnemy enemy)
    {
        // ReomoveMonster() 구독 추가 해제
        enemy.onRemoved += RemoveMonster;
        _aliveMonsterCount++;
    }

    // 몬스터 사망 시 호출
    public void RemoveMonster(BaseEnemy enemy)
    {
        enemy.onRemoved -= RemoveMonster;
        _aliveMonsterCount--;

        CheckWaveEnd();
    }
    
    // 이번 웨이브 몬스터 생성 완료 시 호출
    public void SetSpawnFinished()
    {
        _isSpawnFinished = true;

        CheckWaveEnd();
    }

    // 웨이브 종료 조건 확인
    // 몬스터 스폰 끝나고 생존한 몬스터가 없는 경우 종료
    private void CheckWaveEnd()
    {
        // 이미 종료 처리된 웨이브 return
        if (_isWaveEnded) return;
        
        // 게임 자체가 종료된 상태라면 무시
        if (_gameManager.currentState == GameState.GameOver ||
            _gameManager.currentState == GameState.GameClear)
        {
            return;
        }
        
        if (_isSpawnFinished && _aliveMonsterCount <= 0)
        {
            _isWaveEnded = true;
            EndWave();
        }
    }

    // 웨이브 종료
    private void EndWave()
    {
        OnWaveEnded?.Invoke();
        // 웨이브 클리어 UI 출력
        PrintClearText();
        
        // 다음 웨이브 없으면 클리어
        if (_currentWave >= _monsterSpawner.TotalWaveCount)
        {
            OnAllWavesCleared?.Invoke();
            return;
        }
        OnNextWaveRequested?.Invoke();
        // 골드 정산
        // ↑ 이벤트 구독 하여 골드 ui쪽으로 업데이트만 하면 됨 

        // 다음 웨이브 준비 시작

        // StartCoroutine(PrepareNextWave());
    }



    private void PrintClearText()
    {
        _stageClearText.gameObject.SetActive(true);
        _stageClearText.text = "Wave CLEAR!!";
    }

    private void CacheComponents()
    {
        // _player = GetComponent<>(Player); 
    }
    
    private void RefreshGoldUI()
    {
        _goldUI.text = $"보유 골드 : {_gameManager._gold }";
    }

    // fps 실습때 진행한 playerWeapon과 UI로 확인
}



