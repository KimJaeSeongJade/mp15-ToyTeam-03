using System;
using System.Collections;
using UnityEngine;

public class WaveManager : SingletonBehaviour<WaveManager>
{
    [SerializeField] private MonsterSpawner _monsterSpawner;

    // 웨이브 대기 시간
    [SerializeField] private float _prepareTime = 30f;
    [SerializeField] private TMPro.TextMeshProUGUI _prepareText;
    private GameManager _gameManager;

    // 정보 받아와야하면 추후에 수정

    private int _currentWave = 0;
    private float _remainingTime;
    [SerializeField] private int _aliveMonsterCount;
    [SerializeField] private bool _isSpawnFinished;

    public int CurrentWave => _currentWave;
    public float RemainingTime => _remainingTime;

    // 웨이브 시작 / 종료 액션처리
    // delegate
    public event Action OnWaveStarted;
    public event Action OnWaveEnded;
    public event Action<int> OnWaveChanged;
    public event Action<float> OnPrepareTimeChanged;

    // --- 이벤트 함수 ---------------------------------------------
    
    private void Awake()
    {
        SetSingleton();
    }

    private void Update()
    {

    }

    // 게임 매니저 게임 시작 -> 이벤트 구독 처리
    
    
    private void OnEnable()
    {
        // 게임매니저에서 시작 전달받아와야함.
        StartFirstWavePrepare();
        Debug.Log("웨이브 준비 단계");

        // GameManager 게임 시작 이벤트 구독 

        // MonsterSpawner 생성 완료 이벤트 구독  // 장수님과 체크해서 이벤트 구독 처리해서 예시 화면 

        // Monster 사망 관련 이벤트 구독         // OnReturn으로 +- 하면서 인원 수 체크하기
        
    }

    private void OnDisable()
    {
        
        // 이벤트 구독 해제
    }
    
    // --------------------------------------------------------------

    
    // 게임 시작 시 호출
    private void StartFirstWavePrepare()
    {
        StartCoroutine(PrepareNextWave());
    }

    // 다음 웨이브 준비
    // 코루틴으로
    private IEnumerator PrepareNextWave()
    {
        _remainingTime = _prepareTime;

        while (_remainingTime > 0f)
        {
            OnPrepareTimeChanged?.Invoke(_remainingTime);

            _remainingTime -= Time.deltaTime;

            yield return null;
        }
        StartWave();
    }

    private void PrepareWaveUI()
    {
        
    }
    
    // while (_remainingTime > 0)
    // {
    //     UI에 남은 시간 전달
    //
    //     _remainingTime -= Time.deltaTime
    //
    //     yield return null
    // }
    


    // 실제 웨이브 시작
    private void StartWave()
    {
        // MonsterSpawner에게 현재 웨이브 시작 요청
        // TODO 여기 호출 부 수정
        _monsterSpawner.SpawnWave(1, AddMonster, SetSpawnFinished);
        
        
        _currentWave++; // 웨이브 증가
        // 종료조건 파악을 위한 몬스터 수 확인
        _aliveMonsterCount = 0; 
        // 종료 조건
        _isSpawnFinished = false;

        OnWaveChanged?.Invoke(_currentWave);
        OnWaveStarted?.Invoke();
    }

    // 몬스터 생성 시 호출
    public void AddMonster()
    {
        // ReomoveMonster() 구독 추가 해제
        // enemy.onRemoved += RemoveMonster;
        _aliveMonsterCount++;
    }

    // 몬스터 사망 시 호출
    public void RemoveMonster()
    {
        // enemy.onRemoved -= RemoveMonster;
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
        if (_isSpawnFinished && _aliveMonsterCount <= 0)
        {
            EndWave();
        }
    }

    // 웨이브 종료
    private void EndWave()
    {
        OnWaveEnded?.Invoke();

        // 드랍 골드 정산
        // 골드 정산 메서드 
        
        
        // 배달 함수 호출
        
        // ↑ 이벤트 구독 하여 골드 ui쪽으로 업데이트만 하면 됨 
        
        // 다음 웨이브 준비 시작
        
        StartCoroutine(PrepareNextWave());
    }


    private void CacheComponents()
    {
        // _player = GetComponent<>(Player); 
    }

    private void RefreshGold()
    {
        //Player.OnGoldChange += RefreshGoldUI;
    }

    private void RefreshGoldUI(int gold)
    {
        Debug.Log($"Gold : {gold}");
        //_playerGoldText.text = gold.ToString();
    }
    // fps 실습때 진행한 playerWeapon과 UI로 확인
}



