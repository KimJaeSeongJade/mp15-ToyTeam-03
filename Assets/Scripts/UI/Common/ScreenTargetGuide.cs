using TMPro;
using UnityEngine;

public class ScreenTargetGuide : MonoBehaviour
{
    [Header("연결")]
    public Transform Target;
    [Tooltip("거리 기준 Transform. 비어 있으면 월드 카메라 기준으로 계산합니다.")]
    public Transform Observer;
    [Tooltip("비어 있으면 이 컴포넌트의 부모 Canvas를 사용합니다.")]
    public Canvas TargetCanvas;
    [Tooltip("비어 있으면 Main Camera를 사용합니다.")]
    public Camera WorldCamera;
    [Tooltip("선택 사항. 비어 있으면 기본 텍스트와 화살표를 생성합니다.")]
    public TutorialGuideScreenUI ViewPrefab;

    [Header("표시")]
    public bool Visible = true;
    public string TargetName;
    public Vector3 TargetOffset;
    public Vector2 EdgePadding = new Vector2(160f, 120f);
    public string LabelFormat = "{이름} · {거리} m";
    public string DistanceFormat = "{거리} m";
    [Tooltip("기본 생성 UI의 폰트. 커스텀 프리팹의 폰트는 유지합니다.")]
    public TMP_FontAsset Font;
    public Color DefaultColor = new Color(1f, 0.8f, 0.15f);

    private TutorialGuideScreenUI _view;
    private readonly Vector3[] _corners = new Vector3[4];

    public void SetTarget(Transform target, string displayName = null)
    {
        Target = target;
        TargetName = displayName;
        if (target == null) HideView();
    }

    public void Show(bool visible)
    {
        Visible = visible;
        if (!visible) HideView();
    }

    private void LateUpdate()
    {
        if (TargetCanvas == null) TargetCanvas = GetComponentInParent<Canvas>();
        Camera camera = WorldCamera != null ? WorldCamera : Camera.main;
        if (!Visible || Target == null || !Target.gameObject.activeInHierarchy || TargetCanvas == null || camera == null)
        { HideView(); return; }
        if (_view == null) CreateView();
        Camera uiCamera = TargetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : TargetCanvas.worldCamera;
        RectTransform canvasRect = TargetCanvas.transform as RectTransform;
        canvasRect.GetWorldCorners(_corners);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        foreach (Vector3 corner in _corners)
        {
            Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint(uiCamera, corner);
            min = Vector2.Min(min, screenCorner);
            max = Vector2.Max(max, screenCorner);
        }
        // 플레이 카메라의 뷰포트와 UI 영역이 겹치는 부분에 안내를 배치한다.
        min = Vector2.Max(min, camera.pixelRect.min);
        max = Vector2.Min(max, camera.pixelRect.max);
        if (max.x <= min.x || max.y <= min.y) { HideView(); return; }
        Vector2 center = (min + max) * 0.5f;
        Vector2 size = max - min;
        Vector2 padding = Vector2.Min(Vector2.Max(EdgePadding, Vector2.zero), size * 0.2f);
        Vector3 screen = camera.WorldToScreenPoint(Target.position + TargetOffset);
        Vector2 delta = (Vector2)screen - center;
        if (screen.z < 0f) delta = -delta;
        bool offscreen = screen.z <= 0f || screen.x < min.x + padding.x || screen.x > max.x - padding.x
            || screen.y < min.y + padding.y || screen.y > max.y - padding.y;
        Vector2 position = screen;
        if (offscreen)
        {
            if (delta.sqrMagnitude < 0.001f) delta = Vector2.down;
            Vector2 extent = size * 0.5f - padding;
            float factor = Mathf.Min(extent.x / Mathf.Max(Mathf.Abs(delta.x), 0.001f),
                extent.y / Mathf.Max(Mathf.Abs(delta.y), 0.001f));
            position = center + delta * factor;
        }
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, position, uiCamera, out Vector2 local))
        { HideView(); return; }
        Transform origin = Observer != null ? Observer : camera.transform;
        string distance = Vector3.Distance(origin.position, Target.position).ToString("0");
        string name = string.IsNullOrEmpty(TargetName) ? Target.name : TargetName;
        string label = (LabelFormat ?? "").Replace("{이름}", name).Replace("{거리}", distance);
        string distanceText = (DistanceFormat ?? "").Replace("{거리}", distance);
        _view.gameObject.SetActive(true);
        _view.UpdateDisplay(local, offscreen, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, label, distanceText);
    }

    private void CreateView()
    {
        if (ViewPrefab != null) _view = Instantiate(ViewPrefab, TargetCanvas.transform, false);
        else
        {
            GameObject root = new GameObject("TutorialGuideScreenUI", typeof(RectTransform));
            root.transform.SetParent(TargetCanvas.transform, false);
            _view = root.AddComponent<TutorialGuideScreenUI>();
            _view.Label = CreateText(root.transform, "Label", 24f, new Vector2(300f, 40f));
            TMP_Text arrow = CreateText(root.transform, "Direction", 58f, new Vector2(70f, 70f));
            arrow.text = ">";
            _view.DirectionArrow = arrow.rectTransform;
        }
        _view.Initialize();
    }

    private TMP_Text CreateText(Transform parent, string name, float size, Vector2 dimensions)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        TMP_Text text = child.AddComponent<TextMeshProUGUI>();
        if (Font != null) text.font = Font;
        text.fontSize = size;
        text.color = DefaultColor;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        text.rectTransform.sizeDelta = dimensions;
        return text;
    }

    private void HideView() { if (_view != null) _view.gameObject.SetActive(false); }
    private void OnDisable() => HideView();
    private void OnDestroy() { if (_view != null) Destroy(_view.gameObject); }
}
