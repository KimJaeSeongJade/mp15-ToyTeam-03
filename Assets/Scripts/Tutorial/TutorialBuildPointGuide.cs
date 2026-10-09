using UnityEngine;
using System;

[Serializable]
public class TutorialGuideStyle
{
    [Tooltip("대상 위에 표시할 3D 모델 / 월드 스페이스 UI / 이펙트 프리팹")]
    public GameObject WorldArrowPrefab;
    [Tooltip("빌드포인트 바닥의 링 / 이펙트 프리팹. 몬스터 안내에는 사용하지 않습니다.")]
    public GameObject WorldRingPrefab;
    [Tooltip("TutorialGuideScreenUI가 루트에 붙은 UI 프리팹. Add Component → UI → Screen Target Guide View에서 추가합니다.")]
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
    private bool _visible;
    private string _destinationName;
    private TutorialGuideStyle _style;
    private Transform _arrowInstance;
    private Transform _ringInstance;
    private Quaternion _arrowRotation;
    private ScreenTargetGuide _screenTracker;

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
        GameObject screenRoot = new GameObject("TutorialScreenGuide");
        screenRoot.transform.SetParent(transform, false);
        _screenTracker = screenRoot.AddComponent<ScreenTargetGuide>();
        _screenTracker.TargetCanvas = ui.GetComponentInParent<Canvas>();
        _screenTracker.WorldCamera = _camera;
        _screenTracker.Observer = _player;
        _screenTracker.ViewPrefab = _style.ScreenGuidePrefab;
        _screenTracker.Font = ui.Message.font;
        _screenTracker.DefaultColor = color;
        _screenTracker.TargetOffset = _style.ScreenAnchorOffset;
        _screenTracker.LabelFormat = _style.LabelFormat;
        _screenTracker.DistanceFormat = _style.DistanceFormat;
        _screenTracker.SetTarget(point, destinationName);
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

    public void Show(bool visible)
    {
        _visible = visible;
        if (_marker != null) _marker.SetActive(visible);
        if (_screenTracker != null) _screenTracker.Show(visible);
    }

    public void Track(Transform point, string destinationName = null)
    {
        _point = point;
        if (destinationName != null) _destinationName = destinationName;
        if (_screenTracker != null) _screenTracker.SetTarget(point, _destinationName);
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

    }

    private void OnDestroy()
    {
        if (_screenTracker != null) Destroy(_screenTracker.gameObject);
        if (_marker != null) Destroy(_marker);
        if (_material != null) Destroy(_material);
    }
}
