using System.Collections;
using UnityEngine;
using Cinemachine;   // ← 2.x 네임스페이스

public class CameraDirector : MonoBehaviour
{
    public CinemachineVirtualCamera focusCam;   // ← 타입만 변경
    public float blendTime = 1f;

    Coroutine routine;

    public void FocusFor(Transform target, float duration)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FocusRoutine(target, duration));
    }

    IEnumerator FocusRoutine(Transform target, float duration)
    {
        focusCam.Follow = target;
        focusCam.LookAt = target;
        focusCam.gameObject.SetActive(true);

        yield return new WaitForSeconds(blendTime + duration);

        focusCam.gameObject.SetActive(false);
        routine = null;
    }

    public IEnumerator FocusAndWait(Transform target, float duration, System.Action onArrived)
    {
        focusCam.Follow = target;
        focusCam.LookAt = target;
        focusCam.gameObject.SetActive(true);

        yield return new WaitForSeconds(blendTime);
        onArrived?.Invoke();
        yield return new WaitForSeconds(duration);

        focusCam.gameObject.SetActive(false);
    }
}