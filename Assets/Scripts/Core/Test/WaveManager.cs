using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private float _prepareTime = 30f;

    private float _remainingTime;

    
    // 30초 종료 이후 웨이브 시작했음을 event로 구독
    public event Action OnWaveStarted;
    
    
    private void StartPreparation()
    {
        _remainingTime = _prepareTime;

        while (_remainingTime > 0)
        {
            // UI 갱신
            
            // 시간 감소
            
            // 한 프레임 기다림
        }
    }
    
    // StartWave 요청
}
