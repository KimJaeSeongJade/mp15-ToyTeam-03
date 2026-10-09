using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 플레이어의 레이 판정에서 전달한 결과를 표시하는 재사용 UI.
public class BuildPointTurretUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private TMP_Text _buyGoldText;
    [SerializeField] private TMP_Text _sellGoldText;
    [SerializeField] private TMP_Text _buildStateText;
    [Header("빌드포인트 위 위치와 크기 (월드 단위)")]
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 3f, 0f);
    [SerializeField, Min(0.001f)] private float _worldScale = 0.005f;
    [SerializeField] private Shader _backgroundOverlayShader;
    [SerializeField] private Shader _textOverlayShader;

    private RectTransform _rect;
    private Canvas _canvas;
    private Camera _worldCamera;
    private Vector3 _worldPosition;
    private bool _isTracking;
    private Graphic[] _graphics;
    private readonly Dictionary<Graphic, Material> _overlayMaterials = new Dictionary<Graphic, Material>();

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponent<Canvas>();
        if (_canvas != null)
        {
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 32760;
        }
        _graphics = GetComponentsInChildren<Graphic>(true);
    }

    // Cinemachine 등 카메라 이동이 적용된 뒤 화면 위치를 갱신한다.
    private void LateUpdate()
    {
        if (_isTracking)
        {
            UpdatePosition();
            EnsureOverlayMaterials();
        }
    }

    public void Show(BaseTurret resultTurret, bool canBuild, bool canSell, int sellGold,
        Vector3 worldPosition, Camera worldCamera)
    {
        _worldPosition = worldPosition;
        _worldCamera = worldCamera;
        _isTracking = true;
        ITurretInfoSender info = resultTurret;
        SetOptionalText(_nameText, info?.Name);
        SetOptionalText(_infoText, info?.Info);
        SetText(_buyGoldText, resultTurret != null
            ? $"구매: {resultTurret.Cost} G" : "구매: 조합 불가");
        SetText(_sellGoldText, canSell ? $"판매: {sellGold} G" : "판매: 불가");
        SetOptionalText(_buildStateText, resultTurret == null
            ? "조합 불가" : canBuild ? "건설 가능" : "건설 불가 (골드 부족 또는 쿨타임)");
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        UpdatePosition();
        EnsureOverlayMaterials();
    }

    private void EnsureOverlayMaterials()
    {
        if (_graphics == null) return;
        foreach (Graphic graphic in _graphics)
        {
            TMP_Text text = graphic as TMP_Text;
            Material source = text != null ? text.fontSharedMaterial : graphic.material;
            Shader shader = text != null ? _textOverlayShader : _backgroundOverlayShader;
            if (source == null || shader == null) continue;
            if (_overlayMaterials.TryGetValue(graphic, out Material owned))
            {
                if (source == owned) continue;
                Destroy(owned);
            }
            // 폰트 교체 후에도 아틀라스를 유지하고 공유 에셋의 재질은 변경하지 않는다.
            Material material = new Material(source) { shader = shader, renderQueue = 4000 };
            _overlayMaterials[graphic] = material;
            if (text != null) text.fontSharedMaterial = material;
            else graphic.material = material;
        }
    }

    private void OnDestroy()
    {
        foreach (Material material in _overlayMaterials.Values)
            if (material != null) Destroy(material);
        _overlayMaterials.Clear();
    }

    public void Hide()
    {
        _isTracking = false;
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    private void UpdatePosition()
    {
        if (_worldCamera == null || _rect == null || _canvas == null)
        {
            Hide();
            return;
        }

        Vector3 position = _worldPosition + _worldOffset;
        if (Vector3.Dot(position - _worldCamera.transform.position, _worldCamera.transform.forward) <= 0f)
        {
            Hide();
            return;
        }

        _canvas.worldCamera = _worldCamera;
        _rect.position = position;
        // UI의 앞면이 카메라를 향하게 하면서 카메라 화면과 평행하게 유지한다.
        _rect.rotation = _worldCamera.transform.rotation;
        _rect.localScale = Vector3.one * _worldScale;
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null && target.text != value) target.text = value;
    }

    private static void SetOptionalText(TMP_Text target, string value)
    {
        if (target == null) return;
        bool visible = !string.IsNullOrWhiteSpace(value);
        SetText(target, visible ? value : string.Empty);
        if (target.gameObject.activeSelf != visible) target.gameObject.SetActive(visible);
    }
}
