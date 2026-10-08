using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera[] cams;
    [SerializeField] private GameObject[] PortalEffect;
    [SerializeField] private float _camDelay = 2f;
    private int _currentWave=1;
    
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
        }
        else
        {
            PortalEffect[0].SetActive(true);
        }
        cams[_currentWave-1].gameObject.SetActive(true);
        cams[_currentWave-1].Priority = 0;

        yield return new WaitForSeconds(_camDelay);
        OnCamera(_currentWave-1);
        /*yield return new WaitForSeconds(_camDelay);

        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].gameObject.SetActive(false);
        }*/
    }
    
    private void OnCamera(int index)
    {
        cams[index].gameObject.SetActive(false);
    }
    
}