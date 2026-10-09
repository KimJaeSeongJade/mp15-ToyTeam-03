using TMPro;
using UnityEngine;
using System;

[Serializable]
public class TutorialGuideStyle
{
    [Tooltip("대상 위에 표시할 3D 모델 / 월드 스페이스 UI / 이펙트 프리팹")]
    public GameObject WorldArrowPrefab;
    [Tooltip("빌드포인트 바닥의 링 / 이펙트 프리팹. 몬스터 안내에는 사용하지 않습니다.")]
    public GameObject WorldRingPrefab;
    [Tooltip("TutorialGuideScreenUI가 루트에 붙은 UI 프리팹. 기존 안내 Canvas 아래 생성합니다.")]
    public TutorialGuideScreenUI ScreenGuidePrefab;
    public Vector3 WorldArrowOffset = new Vector3(0f, 3.8f, 0f);
    public Vector3 WorldRingOffset = new Vector3(0f, 0.15f, 0f);
    public Vector3 ScreenAnchorOffset = new Vector3(0f, 5.8f, 0f);
    public Vector3 WorldArrowScale = Vector3.one;
    public Vector3 WorldRingScale = Vector3.one;
    public bool ArrowFacesCamera = true;
    [Min(0f)] public float BobAmount = 0.25f;
    [Min(0f)] public float BobSpeed = 3f;
    [Tooltip("기본 생성 표시의 색상입니다. 지정한 프리팹의 색상은 유지합니다.")]
    public Color DefaultColor = new Color(1f, 0.8f, 0.15f);
    public string LabelFormat = "{이름} · {거리} m";
    public string DistanceFormat = "{거리} m";
}

// 튜토리얼 건설 단계에서만 생성/표시한다. 원본 HUD와 BuildPoint는 수정하지 않는다.
public class TutorialBuildPointGuide : MonoBehaviour
{
    private Transform _point;
    private Transform _player;
    private Camera _camera;
    private GameObject _marker;
    private LineRenderer _arrow;
    private LineRenderer _ring;
    private Material _material;
    private TMP_Text _label;
    private TMP_Text _direction;
    private Canvas _canvas;
    private bool _visible;
    private string _destinationName;
    private TutorialGuideStyle _style;
    private Transform _arrowInstance;
    private Transform _ringInstance;
    private Quaternion _arrowRotation;
    private TutorialGuideScreenUI _screenGuide;

    public void Initialize(BuildPoint point, Transform player, TutorialUI ui)
    {
        Initialize(point.transform, player, ui, "건설 지점", true);
    }

    public void Initialize(Transform point, Transform player, TutorialUI ui, string destinationName, bool showRing,
        TutorialGuideStyle style = null)
    {
        _point = point;
        _destinationName = destinationName;
        _player = player;
        _camera = Camera.main;
        _canvas = ui.GetComponentInParent<Canvas>();
        _style = style ?? new TutorialGuideStyle();
        _marker = new GameObject("TutorialBuildPointMarker");
        _marker.transform.SetParent(transform, false);
        Color color = _style.DefaultColor;
        if (showRing)
        {
            if (_style.WorldRingPrefab != null)
                _ringInstance = CreateWorldPrefab(_style.WorldRingPrefab, _style.WorldRingScale);
            else
            {
                _ring = CreateLine("PlacementRing", color);
                _ring.loop = true;
                _ring.positionCount = 64;
            }
        }
        if (_style.WorldArrowPrefab != null)
        {
            _arrowInstance = CreateWorldPrefab(_style.WorldArrowPrefab, _style.WorldArrowScale);
            _arrowRotation = _arrowInstance.localRotation;
        }
        else
        {
            _arrow = CreateLine("PlacementArrow", color);
            _arrow.positionCount = 5;
        }
        if (_style.ScreenGuidePrefab != null)
        {
            _screenGuide = Instantiate(_style.ScreenGuidePrefab, _canvas.transform, false);
            _screenGuide.Initialize();
        }
        else
        {
            _label = CreateText("BuildPointDestination", ui.Message.font, 24f, color);
            _label.rectTransform.sizeDelta = new Vector2(300f, 40f);
            _direction = CreateText("BuildPointDirection", ui.Message.font, 58f, color);
            _direction.text = ">";
            _direction.rectTransform.sizeDelta = new Vector2(70f, 70f);
        }
        UpdateRing();
        Show(false);
    }

    private Transform CreateWorldPrefab(GameObject prefab, Vector3 scale)
    {
        Transform instance = Instantiate(prefab, _marker.transform, false).transform;
        instance.localScale = Vector3.Scale(instance.localScale, scale);
        instance.gameObject.SetActive(true);
        return instance;
    }

    private LineRenderer CreateLine(string name, Color color)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(_marker.transform, false);
        LineRenderer line = child.AddComponent<LineRenderer>();
        if (_material == null) _material = new Material(Shader.Find("Sprites/Default"));
        line.sharedMaterial = _material;
        line.startColor = line.endColor = color;
        line.startWidth = line.endWidth = 0.12f;
        line.useWorldSpace = true;
        line.numCapVertices = 4;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    private TMP_Text CreateText(string name, TMP_FontAsset font, float size, Color color)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(_canvas.transform, false);
        TMP_Text text = child.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        return text;
    }

    public void Show(bool visible)
    {
        _visible = visible;
        if (_marker != null) _marker.SetActive(visible);
        if (_label != null) _label.gameObject.SetActive(visible);
        if (_direction != null) _direction.gameObject.SetActive(false);
        if (_screenGuide != null) _screenGuide.gameObject.SetActive(visible);
    }

    public void Track(Transform point, string destinationName = null)
    {
        _point = point;
        if (destinationName != null) _destinationName = destinationName;
        UpdateRing();
    }

    private void UpdateRing()
    {
        if (_point == null) return;
        if (_ringInstance != null) _ringInstance.position = _point.position + _style.WorldRingOffset;
        if (_ring == null) return;
        for (int i = 0; i < _ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / _ring.positionCount;
            _ring.SetPosition(i, _point.position + _style.WorldRingOffset
                + new Vector3(Mathf.Cos(angle) * 1.4f, 0f, Mathf.Sin(angle) * 1.4f));
        }
    }

    private void LateUpdate()
    {
        if (!_visible || _camera == null || _point == null) return;
        float bob = Mathf.Sin(Time.time * _style.BobSpeed) * _style.BobAmount;
        Vector3 tip = _point.position + _style.WorldArrowOffset + Vector3.up * bob;
        Vector3 side = _camera.transform.right * 0.45f;
        if (_arrowInstance != null)
        {
            _arrowInstance.position = tip;
            if (_style.ArrowFacesCamera) _arrowInstance.rotation = _camera.transform.rotation * _arrowRotation;
        }
        if (_arrow != null)
        {
            _arrow.SetPosition(0, tip + Vector3.up * 1.5f);
            _arrow.SetPosition(1, tip);
            _arrow.SetPosition(2, tip + Vector3.up * 0.55f - side);
            _arrow.SetPosition(3, tip);
            _arrow.SetPosition(4, tip + Vector3.up * 0.55f + side);
        }
        UpdateRing();

        Vector3 screen = _camera.WorldToScreenPoint(_point.position + _style.ScreenAnchorOffset);
        Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 delta = (Vector2)screen - center;
        if (screen.z < 0f) delta = -delta;
        float marginX = Mathf.Min(160f, Screen.width * 0.2f);
        float marginY = Mathf.Min(120f, Screen.height * 0.2f);
        bool offscreen = screen.z <= 0f || screen.x < marginX || screen.x > Screen.width - marginX
            || screen.y < marginY || screen.y > Screen.height - marginY;
        Vector2 position = screen;
        if (offscreen)
        {
            if (delta.sqrMagnitude < 0.001f) delta = Vector2.down;
            Vector2 extent = center - new Vector2(marginX, marginY);
            float factor = Mathf.Min(extent.x / Mathf.Max(Mathf.Abs(delta.x), 0.001f),
                extent.y / Mathf.Max(Mathf.Abs(delta.y), 0.001f));
            position = center + delta * factor;
        }
        Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, position, uiCamera, out Vector2 local);
        string distance = Vector3.Distance(_player.position, _point.position).ToString("0");
        string label = (_style.LabelFormat ?? "").Replace("{이름}", _destinationName).Replace("{거리}", distance);
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        if (_screenGuide != null)
        {
            string distanceText = (_style.DistanceFormat ?? "").Replace("{거리}", distance);
            _screenGuide.UpdateDisplay(local, offscreen, angle, label, distanceText);
        }
        else
        {
            _label.rectTransform.localPosition = local + (offscreen ? Vector2.down * 48f : Vector2.zero);
            _label.text = label;
            _direction.gameObject.SetActive(offscreen);
            _direction.rectTransform.localPosition = local;
            _direction.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnDestroy()
    {
        if (_label != null) Destroy(_label.gameObject);
        if (_direction != null) Destroy(_direction.gameObject);
        if (_screenGuide != null) Destroy(_screenGuide.gameObject);
        if (_marker != null) Destroy(_marker);
        if (_material != null) Destroy(_material);
    }
}
