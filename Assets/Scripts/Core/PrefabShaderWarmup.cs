using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// 로딩 씬의 카메라는 실제로 렌더링하고 검은 Overlay UI로 그 결과만 가린다.
[AddComponentMenu("Loading/Prefab Shader Warmup")]
public class PrefabShaderWarmup : MonoBehaviour
{
    public GameObject[] Prefabs = new GameObject[0];
    [Tooltip("로딩 카메라가 실제로 볼 수 있는 위치에 배치합니다.")]
    public Transform SpawnPoint;
    public Canvas LoadingCanvas;
    public Slider ProgressBar;
    public TMP_Text ProgressText;
    [Min(1)] public int RenderFrames = 3;
    [Tooltip("파티클이 나타날 때까지 추가로 기다리는 실제 시간")]
    [Min(0f)] public float PreviewSeconds = 0.25f;
    [Tooltip("복제본의 MonoBehaviour를 제거해 공격·이동·게임매니저 접근을 방지합니다. 스크립트로 생성되는 별도 이펙트는 그 프리팹도 목록에 등록하세요.")]
    public bool VisualsOnly = true;
    public event Action OnCompleted;

    public string NextScenename;

    public int CreatedCount { get; private set; }
    public int TotalCount { get; private set; }
    public bool IsLoading { get; private set; }
    private GameObject _staging;

    private void Awake()
    {
        switch (TutorialManager.HasCompleted)
        {
            case true:
                NextScenename = "MainScene";
                break;

            case false:
                NextScenename = "TutorialScene";
                break;
        }

        SceneFade.Instance._destinationScene = NextScenename;
        OnCompleted += SceneFade.Instance.TransitionToScene;
    }

    private IEnumerator Start()
    {
        if (SpawnPoint == null)
        {
            #if UNITY_EDITOR
            Debug.LogError("PrefabShaderWarmup: 카메라 앞의 SpawnPoint를 연결하세요.", this);
#endif
            yield break;
        }
        PrepareOverlay();
        if (Prefabs != null)
            foreach (GameObject prefab in Prefabs) if (prefab != null) TotalCount++;
        UpdateProgress();
        IsLoading = true;
        yield return null; // 검은 배경과 0 / 전체 진행률부터 표시한다.
        _staging = new GameObject("WarmupStaging");
        _staging.transform.SetParent(transform, false);
        _staging.SetActive(false);
        try
        {
            if (Prefabs != null)
                foreach (GameObject prefab in Prefabs)
                {
                    if (prefab == null) continue;
                    // 비활성 부모 아래 복제하여 게임 스크립트의 Awake가 먼저 실행되지 않게 한다.
                    GameObject instance = Instantiate(prefab, SpawnPoint.position, SpawnPoint.rotation, _staging.transform);
                    if (VisualsOnly)
                    {
                        foreach (MonoBehaviour script in instance.GetComponentsInChildren<MonoBehaviour>(true))
                            Destroy(script);
                        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
                        foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
                        { body.isKinematic = true; body.useGravity = false; }
                        foreach (Camera camera in instance.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
                        foreach (AudioSource audio in instance.GetComponentsInChildren<AudioSource>(true)) audio.enabled = false;
                        foreach (AudioListener listener in instance.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
                        yield return null; // MonoBehaviour가 파괴된 뒤 활성화한다.
                    }
                    instance.SetActive(true);
                    _staging.SetActive(true);
                    foreach (ParticleSystem particles in instance.GetComponentsInChildren<ParticleSystem>()) particles.Play(false);
                    float elapsed = 0f;
                    int frames = 0;
                    while (frames < Mathf.Max(1, RenderFrames) || elapsed < PreviewSeconds)
                    {
                        yield return null;
                        elapsed += Time.unscaledDeltaTime;
                        frames++;
                    }
                    _staging.SetActive(false);
                    Destroy(instance);
                    CreatedCount++;
                    UpdateProgress();
                    yield return null;
                }
        }
        finally
        {
            IsLoading = false;
            if (_staging != null) Destroy(_staging);
        }
        OnCompleted.Invoke();
    }

    private void PrepareOverlay()
    {
        if (LoadingCanvas == null && ProgressBar != null) LoadingCanvas = ProgressBar.GetComponentInParent<Canvas>();
        if (LoadingCanvas == null && ProgressText != null) LoadingCanvas = ProgressText.GetComponentInParent<Canvas>();
        if (LoadingCanvas == null)
        {
            GameObject canvasObject = new GameObject("WarmupLoadingCanvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            LoadingCanvas = canvasObject.GetComponent<Canvas>();
        }
        LoadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        LoadingCanvas.overrideSorting = true;
        LoadingCanvas.sortingOrder = short.MaxValue - 1; // SceneFade가 위에서 덮을 수 있다.
        GameObject background = new GameObject("WarmupBlackBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(LoadingCanvas.transform, false);
        background.transform.SetAsFirstSibling();
        RectTransform rect = background.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image image = background.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    private void UpdateProgress()
    {
        if (ProgressBar != null)
        {
            ProgressBar.minValue = 0f;
            ProgressBar.maxValue = 1f;
            ProgressBar.wholeNumbers = false;
            ProgressBar.interactable = false;
            ProgressBar.value = TotalCount == 0 ? 1f : (float)CreatedCount / TotalCount;
        }
        if (ProgressText != null) ProgressText.text = $"{CreatedCount} / {TotalCount}";
    }

    private void OnDestroy() 
    {
        OnCompleted -= SceneFade.Instance.TransitionToScene;

        if (_staging != null) Destroy(_staging); 
    }
}
