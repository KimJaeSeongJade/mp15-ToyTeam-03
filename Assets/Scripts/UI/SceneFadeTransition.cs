using UnityEngine;

// 씬에 추가해 UnityEvent에서 Play를 연결한다. 페이드 본체는 필요할 때 자동 생성된다.
[AddComponentMenu("UI/Scene Fade Transition")]
public class SceneFadeTransition : MonoBehaviour
{
    [Tooltip("Build Settings에 등록된 다음 씬 이름 또는 경로")]
    public string DestinationScene;
    [Min(0f)] public float FadeInDuration = 0.5f;
    [Min(0f)] public float FadeOutDuration = 0.5f;
    [Tooltip("검어진 뒤 제거할 기존 씬 오브젝트. 튜토리얼의 GameManager를 연결합니다.")]
    public GameObject DestroyBeforeLoad;

    public void Play()
    {
        SceneFade fade = SceneFade.Instance;
        if (fade.IsFading) return;
        fade.FadeInDuration = FadeInDuration;
        fade.FadeOutDuration = FadeOutDuration;
        fade.LoadSceneWithFade(DestinationScene, DestroyBeforeLoad);
    }
}
