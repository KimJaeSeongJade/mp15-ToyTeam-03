using System;
using System.Collections;
using UnityEngine;

public class WaveManager : SingletonBehaviour<WaveManager>
{
    // 웨이브 대기 시간
    [SerializeField] private float _prepareTime = 30f;

    // 정보 받아와야하면 추후에 수정
    private int _currentWave = 0;   
    private float _remainingTime;   
    private int _aliveMonsterCount;
    private bool _isSpawnFinished;

    public int CurrentWave => _currentWave;
    public float RemainingTime => _remainingTime;

    // 웨이브 시작 / 종료 액션처리
    public event Action OnWaveStarted;
    public event Action OnWaveEnded;
    public event Action<int> OnWaveChanged;
    public event Action<float> OnPrepareTimeChanged;

    // --- 이벤트 함수 ---------------------------------------------
    
    private void Awake()
    {
        SetSingleton();
    }

    private void OnEnable()
    {
        // GameManager 게임 시작 이벤트 구독 
        // MonsterSpawner 생성 완료 이벤트 구독
        // Monster 사망 관련 이벤트 구독
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

    // 실제 웨이브 시작
    private void StartWave()
    {
        _currentWave++; // 웨이브 증가

        _aliveMonsterCount = 0;
        _isSpawnFinished = false;

        OnWaveChanged?.Invoke(_currentWave);
        OnWaveStarted?.Invoke();

        // MonsterSpawner에게 현재 웨이브 시작 요청
    }

    // 몬스터 생성 시 호출
    public void AddMonster()
    {
        _aliveMonsterCount++;
    }

    // 몬스터 사망 시 호출
    public void RemoveMonster()
    {
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
        // 배달 함수 호출
        // 다음 웨이브 준비 시작

        StartCoroutine(PrepareNextWave());
    }
}


