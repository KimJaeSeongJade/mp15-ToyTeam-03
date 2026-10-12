using System.Collections;
using UnityEngine;
using Cinemachine;  

public class CameraDirector : MonoBehaviour
{
    public CinemachineVirtualCamera focusCam;   
    public float blendTime = 10f;

    [SerializeField] private Transform targetTR;
    Coroutine routine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            FocusFor(targetTR);
        }
    }

    public void FocusFor(Transform target)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FocusRoutine(target));
    }

    IEnumerator FocusRoutine(Transform target)
    {
        focusCam.Follow = target;
        focusCam.LookAt = target;
        focusCam.gameObject.SetActive(true);

        yield return new WaitForSeconds(blendTime);

        focusCam.gameObject.SetActive(false);
        routine = null;
    }

    /*public IEnumerator FocusAndWait(Transform target, float duration, System.Action onArrived)
    {
        focusCam.Follow = target;
        focusCam.LookAt = target;
        focusCam.gameObject.SetActive(true);

        yield return new WaitForSeconds(blendTime);
        onArrived?.Invoke();
        yield return new WaitForSeconds(duration);

        focusCam.gameObject.SetActive(false);
    }*/
}