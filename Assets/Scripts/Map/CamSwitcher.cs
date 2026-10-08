using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera[] cams;

    [SerializeField] private float _camDelay = 2f;
    [SerializeField] private MonsterGroup _monsterGroupList;
    
    
    private void Awake()
    {
        OnCamera(0);
        OnCamera(1);
    }
    private void OnEnable()  => WaveManager.OnCamActivate += Activate;
    private void OnDisable() => WaveManager.OnCamActivate -= Activate;

    
    

    public void Activate()
    {
        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].gameObject.SetActive(true);

            cams[i].Priority = 2;

            yield return new WaitForSeconds(_camDelay);
        }
        
        yield return new WaitForSeconds(_camDelay);
        
        for (int i = 0; i < cams.Length; i++)
        {
            cams[i].gameObject.SetActive(false);
        }
    }
    
    private void OnCamera(int index)
    {
        cams[index].gameObject.SetActive(false);
    }
    
}

