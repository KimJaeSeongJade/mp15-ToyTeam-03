using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    // 레벨 관리는 게임매니저쪽에서
    // 초기 골드는 플레이어가 갖고있음
    // 리스트에 오브젝트 넣어서 
    // 리스트 크기만큼 골드가 뜨게
    
    // wave 끝났을 때 골드 
    public GameState currentState;
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _inGameUI;
    
    private bool IsPause;
    private bool IsStart;

    
    // 웨이브 에서 시작했는지 확인 용도 
    // 웨이브 종료시 받아와야해서 set으로 조건 추가할지 결정
    public bool IsStarted
    {
        get { return IsStart; }
    }
    // ---- 이벤트 함수 ---------------------------------------
    
    private void Awake() => SetSingleton();
    
    private void Start()
    {
        ResetToTitle();
    }

    private void Update()
    {
        PauseManager();
    }
    // --------------------------------------------------------
    
    public event Action OnWaveStarted; // 추후에 TODO REFACTOR
    
    private void PauseManager()
    {
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
                break;
            case GameState.WavePreparation:
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
    
    // 초기 화면
    public void StartGame()
    {
        // 게임 시작 버튼 누르면
        // 판넬 끄고
        _startPanel.gameObject.SetActive(false);
        // 게임 시간 시작
        Run();
        _inGameUI.SetActive(true);
    }
    
    // 진행
    public void Run()
    {
        LockCursor();           // 마우스 커서 잠금
        Time.timeScale = 1;     // 게임 시간 on
    }

    // 일시정지
    public void Pause()
    {
        UnlockCursor();
        Time.timeScale = 0;
    }
    
    // 게임 리셋
    private void ResetToTitle()
    {
        currentState = GameState.Ready;
        // 판넬 켜고
        _startPanel.gameObject.SetActive(true);
        // 일시정지 판넬은 꺼진 상태
        _pausePanel.gameObject.SetActive(false);
        // 인게임 판넬도 꺼진 상태
        _inGameUI.SetActive(false);
        // 게임 시간 정지
        Pause();
        IsStart = false;
        IsPause = false;
        
        // 대신 전체 게임 진행상황도 초기화해야함.
        // 웨이브 = 0, 타이머 = 0, 생존 몬스터 수 0, SpawnFinished false
        // player HP,Gold,위치 초기화
        // 스포너 : Spawn 중지, 생성 몬스터 풀로 복귀
        // Tower 설치된 터렛 제거
        // UI는 웨이브/골드/HP/타워 쿨타임 초기화
        
        // OnGameRestarted?.Invoke(); 로 해서 이벤트로 처리하는게 좋을듯
        // 웨이브 매니저 코루틴 시간 계산 쪽은 StopAllCoroutines()로 하면될듯
        //
    }
    
    // 일시정지 상태에서 게임 재개
    public void ResumeGame()
    {
        _pausePanel.gameObject.SetActive(false);
        IsPause = false;
        Run();
    }
   
    // 게임 일시정지
    private void PauseGame()
    {
        _pausePanel.SetActive(true);
        IsPause = true;
        Pause();
    }
    
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}

// esc 누르면 판넬 나오고 종료/재개
public enum GameState
{
    Ready,
    WavePreparation,
    OnWave,
    Paused,
    GameOver
}


