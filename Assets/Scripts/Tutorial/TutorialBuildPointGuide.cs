using TMPro;
using UnityEngine;

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

    public void Initialize(BuildPoint point, Transform player, TutorialUI ui)
    {
        Initialize(point.transform, player, ui, "건설 지점", true);
    }

    public void Initialize(Transform point, Transform player, TutorialUI ui, string destinationName, bool showRing)
    {
        _point = point;
        _destinationName = destinationName;
        _player = player;
        _camera = Camera.main;
        _canvas = ui.GetComponentInParent<Canvas>();
        _marker = new GameObject("TutorialBuildPointMarker");
        _marker.transform.SetParent(transform, false);
        _material = new Material(Shader.Find("Sprites/Default"));
        Color color = new Color(1f, 0.8f, 0.15f);
        LineRenderer ring = CreateLine("PlacementRing", color);
        _ring = ring;
        ring.gameObject.SetActive(showRing);
        ring.loop = true;
        ring.positionCount = 64;
        for (int i = 0; i < 64; i++)
        {
            float angle = i * Mathf.PI * 2f / 64f;
            ring.SetPosition(i, _point.position + new Vector3(Mathf.Cos(angle) * 1.4f, 0.15f, Mathf.Sin(angle) * 1.4f));
        }
        _arrow = CreateLine("PlacementArrow", color);
        _arrow.positionCount = 5;
        _label = CreateText("BuildPointDestination", ui.Message.font, 24f, color);
        _label.rectTransform.sizeDelta = new Vector2(300f, 40f);
        _direction = CreateText("BuildPointDirection", ui.Message.font, 58f, color);
        _direction.text = ">";
        _direction.rectTransform.sizeDelta = new Vector2(70f, 70f);
        Show(false);
    }

    private LineRenderer CreateLine(string name, Color color)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(_marker.transform, false);
        LineRenderer line = child.AddComponent<LineRenderer>();
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
    }

    public void Track(Transform point, string destinationName = null)
    {
        _point = point;
        if (destinationName != null) _destinationName = destinationName;
        if (_ring == null || point == null) return;
        for (int i = 0; i < _ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / _ring.positionCount;
            _ring.SetPosition(i, point.position + new Vector3(Mathf.Cos(angle) * 1.4f, 0.15f, Mathf.Sin(angle) * 1.4f));
        }
    }

    private void LateUpdate()
    {
        if (!_visible || _camera == null || _point == null) return;
        float bob = Mathf.Sin(Time.time * 3f) * 0.25f;
        Vector3 tip = _point.position + Vector3.up * (3.8f + bob);
        Vector3 side = _camera.transform.right * 0.45f;
        _arrow.SetPosition(0, tip + Vector3.up * 1.5f);
        _arrow.SetPosition(1, tip);
        _arrow.SetPosition(2, tip + Vector3.up * 0.55f - side);
        _arrow.SetPosition(3, tip);
        _arrow.SetPosition(4, tip + Vector3.up * 0.55f + side);

        Vector3 screen = _camera.WorldToScreenPoint(_point.position + Vector3.up * 5.8f);
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
        _label.rectTransform.localPosition = local + (offscreen ? Vector2.down * 48f : Vector2.zero);
        _label.text = $"{_destinationName} · {Vector3.Distance(_player.position, _point.position):0} m";
        _direction.gameObject.SetActive(offscreen);
        _direction.rectTransform.localPosition = local;
        _direction.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private void OnDestroy()
    {
        if (_label != null) Destroy(_label.gameObject);
        if (_direction != null) Destroy(_direction.gameObject);
        if (_marker != null) Destroy(_marker);
        if (_material != null) Destroy(_material);
    }
}
