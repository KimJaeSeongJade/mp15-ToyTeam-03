using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera[] cams;
    [SerializeField] private GameObject[] PortalEffect;
    [SerializeField] private float _camDelay = 2f;
    [SerializeField] private TrialRender _trialRender;
    private int _currentWave=1;
    private bool _hasEffect = false;
    private void Awake()
    {
        OnCamera(0);
        OnCamera(1);
    }
    
    private void OnEnable()
    {
        WaveManager.OnCamActivate += Activate;
        WaveManager.Instance.OnWaveStarted += OnWaveStarted;
    }

    private void OnDisable()
    {
        WaveManager.OnCamActivate -= Activate;   
        WaveManager.Instance.OnWaveStarted -= OnWaveStarted;
    }

    private void OnWaveStarted()
    {
        _currentWave++;
    }
    

    public void Activate()
    {
        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        if (_currentWave > 1)
        {
            PortalEffect[0].SetActive(true);
            PortalEffect[1].SetActive(true);
            if (!_hasEffect)
            {
                _trialRender.Move(1);
                _hasEffect = true;
            }
        }
        else
        {
            PortalEffect[0].SetActive(true);
            _trialRender.Move(0);
        }
        cams[_currentWave-1].gameObject.SetActive(true);
        cams[_currentWave-1].Priority = 0;

        yield return new WaitForSeconds(_camDelay);
        OnCamera(_currentWave-1);
  
    }
    
    private void OnCamera(int index)
    {
        cams[index].gameObject.SetActive(false);
    }
    
}