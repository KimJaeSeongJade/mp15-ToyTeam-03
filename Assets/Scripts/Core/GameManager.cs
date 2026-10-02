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
    
    // ---- 이벤트 함수 ---------------------------------------
    
    private void Awake() => SetSingleton();
    
    private void Start()
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
        IsPause = false;
    }

    private void Update()
    {
        PauseManager();
    }
    // --------------------------------------------------------
    
    public event Action OnWaveStarted;
    
    private void PauseManager()
    {
        // esc 누르면 검증
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 정지 상태 아니면 정지
            if (IsPause == false)
            {
                _pausePanel.gameObject.SetActive(true);
                Time.timeScale = 0;
                IsPause = true;
                return;
            }
            // 정지 상태면 재개
            if (IsPause == true)
            {
                _pausePanel.gameObject.SetActive(false);
                Time.timeScale = 1;
                IsPause = false;
                return;
            }
        }
        // esc 누르면 update가 아닌 delegate 이벤트로 추가 TODO
    }
    public void ResumeGame()
    {
        _pausePanel.gameObject.SetActive(false);
        IsPause = false;
        Run();
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
                break;
            case GameState.OnWave:
                // 플레이 시작 로직
                break;
            case GameState.Paused:
                // 일시 정지 로직
                break;
            case GameState.GameOver:
                // 게임 오버 로직
                break;
        }
        
    }
    
    public void Run()
    {
        LockCursor();           // 마우스 커서 잠금
        Time.timeScale = 1;     // 게임 시간 on
    }

    public void Pause()
    {
        UnlockCursor();
        Time.timeScale = 0;
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

    public void StartGame()
    {
        // 게임 시작 버튼 누르면
        // 판넬 끄고
        _startPanel.gameObject.SetActive(false);
        // 게임 시간 시작
        Run();
        _inGameUI.SetActive(true);
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


