using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class SceneFade : SingletonBehaviour<SceneFade>
{
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private Image _image;

    [Tooltip("화면이 완전히 어두워질 때까지 걸리는 시간")]
    [Min(0f)] public float FadeInDuration = 0.5f;
    [Tooltip("화면이 다시 밝아질 때까지 걸리는 시간")]
    [Min(0f)] public float FadeOutDuration = 0.5f;
    public Color FadeColor = Color.black;

    [Header("인스펙터에서 씬 이동 연결")]
    [Tooltip("Build Settings에 등록된 다음 씬 이름 또는 경로")]
    public string _destinationScene;
    [Tooltip("완전히 검어진 뒤 제거할 기존 씬 오브젝트. 튜토리얼 GameManager를 연결합니다.")]
    [SerializeField] private GameObject _destroyBeforeLoad;
    public GameObject DestroeyBeforeLoad { set => _destroyBeforeLoad = value; }

    public bool IsFading { get; private set; }

    public event Action AtBlack
    {
        add { Register(value); }
        remove { if (!IsFading) _atBlack -= value; }
    }

    private Action _atBlack;

    private readonly List<object> _waits = new List<object>();

    public bool Register(Action action)
    {
        if (action == null) return false;
        if (IsFading)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("SceneFade: 페이드 중에는 새 Action을 등록할 수 없습니다.", this);
#endif
            return false;
        }
        _atBlack += action;
        return true;
    }

    public void ClearActions()
    {
        if (!IsFading) _atBlack = null;
    }

    // 비동기 작업은 Action의 반환만으로 완료를 알 수 없으므로 직접 대기 대상으로 등록한다.
    public void WaitFor(AsyncOperation operation)
    {
        if (IsFading && operation != null) _waits.Add(operation);
    }

    public void WaitFor(Coroutine coroutine)
    {
        if (IsFading && coroutine != null) _waits.Add(coroutine);
    }

    public void Play()
    {
        if (IsFading || !isActiveAndEnabled) return;
        IsFading = true;
        StartCoroutine(FadeRoutine());
    }

    // UnityEvent에 직접 연결할 수 있는 매개변수 없는 진입점.
    public void TransitionToScene()
    {
        LoadSceneWithFade(_destinationScene, _destroyBeforeLoad);
    }

    public bool LoadSceneWithFade(string sceneName, GameObject destroyBeforeLoad = null)
    {
        if (IsFading || !isActiveAndEnabled) return false;
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            #if UNITY_EDITOR
            Debug.LogError($"SceneFade: Build Settings에 등록된 씬을 지정하세요: {sceneName}", this);
#endif
            return false;
        }
        Register(() => WaitFor(StartCoroutine(LoadSceneRoutine(sceneName, destroyBeforeLoad))));
        Play();
        return true;
    }

    private IEnumerator LoadSceneRoutine(string sceneName, GameObject destroyBeforeLoad)
    {
        if (destroyBeforeLoad != null)
        {
            // Destroy는 프레임 끝에 반영된다. 다음 씬의 싱글톤 Awake보다 먼저 정리한다.
            Destroy(destroyBeforeLoad);
            yield return null;
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (operation != null) yield return operation;
    }

    private IEnumerator FadeRoutine()
    {
        _group.blocksRaycasts = true;
        _image.color = new Color(FadeColor.r, FadeColor.g, FadeColor.b, 1f);
        try
        {
            yield return FadeAlpha(0f, 1f, FadeInDuration);
            yield return null; // 완전히 검은 화면을 렌더링한 뒤 Action 실행.
            Delegate[] callbacks = _atBlack != null ? _atBlack.GetInvocationList() : new Delegate[0];
            foreach (Delegate callback in callbacks)
            {
                try { ((Action)callback).Invoke(); }
                catch (Exception exception) { {
#if UNITY_EDITOR
                    Debug.LogException(exception, this);
#endif
                } }
            }
            for (int i = 0; i < _waits.Count; i++) yield return _waits[i];
            yield return null; // 새 씬의 초기화가 진행된 뒤 화면을 밝힌다.
            yield return FadeAlpha(1f, 0f, FadeOutDuration);
        }
        finally
        {
            ResetFade();
        }
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        _group.alpha = from;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        _group.alpha = to;
    }

    private void ResetFade()
    {
        _atBlack = null;
        _waits.Clear();
        IsFading = false;
        if (_group != null)
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
        }
    }

    private void OnDisable()
    {
        if (Instance != this) return;
        StopAllCoroutines();
        ResetFade();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ResetFade();
    }
}
