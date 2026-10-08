using System.Collections;
using UnityEngine;
using Cinemachine;

public class CanSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera[] cams;
    [SerializeField] private float _camDelay = 2f;

    private void Awake()
    {
        OnCamera(0);
        OnCamera(1);
    }

    //이벤트 구독 / 해제
    //private void OnEnable()  => GameManager.OnCamActivate += Activate;
    //private void OnDisable() => GameManager.OnCamActivate -= Activate;

    
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