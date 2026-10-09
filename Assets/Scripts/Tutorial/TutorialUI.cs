using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;

public enum TutorialFocus { None, Move, Attack, Skill, Build, Gold, Castle, ExitBuild, BuildSecond, Sell, Information, Experience }

// 기존 HUD 위에 설명/강조/스킵 확인만 추가한다.
public class TutorialUI : MonoBehaviour
{
    public TMP_Text Message;
    public TMP_Text Progress;
    public TMP_Text Gold;
    public RectTransform[] FocusTargets;
    public RectTransform Highlight;
    public Image HighlightImage;
    public GameObject SkipDialog;
    public Button SkipButton;
    public Button ContinueButton;
    [Range(0f, 1f)] public float SpotlightDarkness = 0.8f;
    public Font TutorialFontSource;

    private RectTransform _canvasRect;
    private RectTransform _target;
    private RectTransform _buildTarget;
    private RectTransform _exitBuildTarget;
    private TMP_Text _waveText;
    private RectTransform _spotlightRoot;
    private readonly Image[] _spotlightPanels = new Image[4];
    private readonly Vector3[] _corners = new Vector3[4];
    private bool _spotlightRequested;
    private RectTransform _messageBox;
    private TMP_FontAsset _tutorialFont;
    private Vector2 _lastCanvasSize;

    // 원본 프리팹은 수정하지 않고 이 씬에 있는 기존 HUD에만 연결한다.
    public void BindExistingHUD(GameManager game)
    {
        foreach (Transform child in _canvasRect)
        {
            bool keep = Message.transform.IsChildOf(child) || Progress.transform.IsChildOf(child)
                || child == Highlight || child == SkipDialog.transform || child == _spotlightRoot;
            if (!keep) child.gameObject.SetActive(false);
        }
        foreach (Canvas canvas in game.GetComponentsInChildren<Canvas>(true))
        {
            canvas.gameObject.SetActive(true);
            canvas.enabled = true;
        }
        Transform root = game.transform;
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true)) ApplyTutorialFont(text);
        foreach (TMP_Text text in game.PlayerStatus.GetComponentsInChildren<TMP_Text>(true)) ApplyTutorialFont(text);
        SetActive(root, "PausePanel", false);
        SetActive(root, "StageClearText", false);
        SetActive(root, "WaveTimerBackGround", false);
        Gold = Find(root, "GoldText")?.GetComponent<TMP_Text>();
        _waveText = Find(root, "PrepareTimeUI")?.GetComponent<TMP_Text>();
        _buildTarget = Find(root, "Turret1UI") as RectTransform;
        _exitBuildTarget = Find(root, "ExitBuildModeUI") as RectTransform;
        FocusTargets = new RectTransform[12];
        FocusTargets[(int)TutorialFocus.Experience] = Find(root, "ExpText") as RectTransform;
        FocusTargets[(int)TutorialFocus.Attack] = Find(root, "BasicAttackUI") as RectTransform;
        FocusTargets[(int)TutorialFocus.Skill] = Find(root, "SpecialAttackUI") as RectTransform;
        FocusTargets[(int)TutorialFocus.Build] = _buildTarget;
        FocusTargets[(int)TutorialFocus.Gold] = Gold != null ? Gold.rectTransform : null;
        FocusTargets[(int)TutorialFocus.ExitBuild] = _exitBuildTarget;
        FocusTargets[(int)TutorialFocus.BuildSecond] = Find(root, "Turret2UI") as RectTransform;
        FocusTargets[(int)TutorialFocus.Sell] = Find(root, "SellTurretUI") as RectTransform;
        BuildPointTurretUI information = game.PlayerStatus.GetComponentInChildren<BuildPointTurretUI>(true);
        FocusTargets[(int)TutorialFocus.Information] = information != null ? information.GetComponent<RectTransform>() : null;
        if (Gold == null || _buildTarget == null)
            Debug.LogError("TutorialUI: 기존 HUD의 GoldText / Turret1UI 연결을 확인하세요.", this);
    }

    private static Transform Find(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name.Trim() == name) return child;
        return null;
    }

    private static void SetActive(Transform root, string name, bool value)
    {
        Transform child = Find(root, name);
        if (child != null) child.gameObject.SetActive(value);
    }

    private void Awake()
    {
        _canvasRect = (RectTransform)GetComponentInParent<Canvas>().transform;
        _messageBox = Message.rectTransform.parent as RectTransform;
        CreateTutorialFont();
        Message.alignment = TextAlignmentOptions.MidlineLeft;
        Message.enableWordWrapping = false;
        Message.enableAutoSizing = true;
        Message.fontSizeMin = 18f;
        Message.fontSizeMax = 26f;
        Message.overflowMode = TextOverflowModes.Overflow;
        Message.fontSize = 26f;
        Message.margin = new Vector4(16f, 12f, 16f, 12f);
        Message.rectTransform.anchorMin = Vector2.zero;
        Message.rectTransform.anchorMax = Vector2.one;
        Message.rectTransform.offsetMin = Message.rectTransform.offsetMax = Vector2.zero;
        _messageBox.anchorMin = _messageBox.anchorMax = new Vector2(0.5f, 0.5f);
        _messageBox.pivot = new Vector2(0.5f, 0.5f);
        SkipDialog.SetActive(false);
        Highlight.gameObject.SetActive(false);
        CreateSpotlight();
    }

    private void CreateTutorialFont()
    {
        if (TutorialFontSource != null)
        {
            // 원본 폰트 에셋의 작은 단일 아틀라스를 수정하지 않고 튜토리얼만 동적으로 확장한다.
            _tutorialFont = TMP_FontAsset.CreateFontAsset(TutorialFontSource, 40, 5,
                GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (_tutorialFont != null)
            {
                _tutorialFont.name = "Tutorial Korean Font";
                _tutorialFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                if (TMP_Settings.defaultFontAsset != null)
                    _tutorialFont.fallbackFontAssetTable.Add(TMP_Settings.defaultFontAsset);
            }
        }
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true)) ApplyTutorialFont(text);
    }

    private void ApplyTutorialFont(TMP_Text text)
    {
        TMP_FontAsset font = _tutorialFont != null ? _tutorialFont : text.font;
        if (font == null) return;
        text.font = font;
        // 다른 폰트의 아틀라스를 사용하는 머티리얼이 남지 않게 한다.
        text.fontSharedMaterial = font.material;
    }

    private void CreateSpotlight()
    {
        _spotlightRoot = new GameObject("TutorialSpotlight", typeof(RectTransform)).GetComponent<RectTransform>();
        _spotlightRoot.SetParent(_canvasRect, false);
        _spotlightRoot.anchorMin = Vector2.zero;
        _spotlightRoot.anchorMax = Vector2.one;
        _spotlightRoot.sizeDelta = Vector2.zero;
        _spotlightRoot.anchoredPosition = Vector2.zero;
        // 설명/진행/스킵 창은 어둡게 덮지 않는다.
        _spotlightRoot.SetAsFirstSibling();
        for (int i = 0; i < _spotlightPanels.Length; i++)
        {
            Image panel = new GameObject("DimPanel" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            panel.transform.SetParent(_spotlightRoot, false);
            panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = Vector2.zero;
            panel.rectTransform.pivot = Vector2.zero;
            panel.raycastTarget = false;
            _spotlightPanels[i] = panel;
        }
        _spotlightRoot.gameObject.SetActive(false);
    }

    public void SetSpotlight(bool visible)
    {
        _spotlightRequested = visible;
        if (!visible) _spotlightRoot.gameObject.SetActive(false);
    }

    public void Show(string message, TutorialFocus focus, string progress)
    {
        Message.text = message.Replace("\r", " ").Replace("\n", " ");
        ResizeMessageBox();
        SetSpotlight(false);
        Progress.text = progress;
        if (_waveText != null) _waveText.text = progress;
        int index = (int)focus;
        _target = FocusTargets != null && index < FocusTargets.Length ? FocusTargets[index] : null;
        Highlight.gameObject.SetActive(_target != null);
    }

    private void ResizeMessageBox()
    {
        Message.fontSize = Message.fontSizeMax;
        float maxWidth = Mathf.Max(80f, _canvasRect.rect.width - 48f);
        float minWidth = Mathf.Min(260f, maxWidth);
        Vector2 preferred = Message.GetPreferredValues(Message.text, Mathf.Infinity, Mathf.Infinity);
        float width = Mathf.Clamp(preferred.x + 32f, minWidth, maxWidth);
        _messageBox.sizeDelta = new Vector2(width, 64f);
    }

    public void SetGold(int amount) { if (Gold != null) Gold.text = amount.ToString(); }
    public void SetCastleHealth(float current, float max)
    {
        Message.text = $"캐슬 HP: {current:0} / {max:0} - 도착하기 전에 막으세요!";
        ResizeMessageBox();
    }

    public static string KeyName(KeyCode key)
    {
        if (key == KeyCode.Mouse0) return "마우스 왼쪽";
        if (key == KeyCode.Mouse1) return "마우스 오른쪽";
        return key.ToString();
    }

    private void LateUpdate()
    {
        if (_lastCanvasSize != _canvasRect.rect.size)
        {
            _lastCanvasSize = _canvasRect.rect.size;
            ResizeMessageBox();
        }
        bool visible = _target != null && _target.gameObject.activeInHierarchy;
        Highlight.gameObject.SetActive(visible);
        bool dim = _spotlightRequested && !SkipDialog.activeSelf;
        _spotlightRoot.gameObject.SetActive(dim);
        if (!visible)
        {
            _messageBox.localPosition = _canvasRect.rect.center + new Vector2(0f, _canvasRect.rect.height * 0.2f);
            if (dim) UpdateSpotlight(_canvasRect.rect.center, _canvasRect.rect.center);
            return;
        }
        // 서로 다른 CanvasScaler / 렌더 모드에서도 실제 화면상의 HUD 위치를 강조한다.
        _target.GetWorldCorners(_corners);
        Canvas targetCanvas = _target.GetComponentInParent<Canvas>();
        Camera targetCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera;
        Canvas overlay = GetComponentInParent<Canvas>();
        Camera overlayCamera = overlay.renderMode == RenderMode.ScreenSpaceOverlay ? null : overlay.worldCamera;
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        foreach (Vector3 corner in _corners)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(targetCamera, corner);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, overlayCamera, out Vector2 local);
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }
        Highlight.localPosition = (min + max) * 0.5f;
        Highlight.sizeDelta = max - min + Vector2.one * 16f;
        PositionMessageBox(min, max);
        if (dim) UpdateSpotlight(min - Vector2.one * 8f, max + Vector2.one * 8f);
        Color color = HighlightImage.color;
        color.a = 0.25f + 0.35f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f));
        HighlightImage.color = color;
    }

    private void PositionMessageBox(Vector2 targetMin, Vector2 targetMax)
    {
        Rect canvas = _canvasRect.rect;
        Vector2 half = _messageBox.sizeDelta * 0.5f;
        const float gap = 24f;
        const float margin = 24f;
        float centerX = (targetMin.x + targetMax.x) * 0.5f;
        Vector2 position;
        if (targetMax.y + gap + half.y * 2f <= canvas.yMax - margin)
            position = new Vector2(centerX, targetMax.y + gap + half.y);
        else if (targetMin.y - gap - half.y * 2f >= canvas.yMin + margin)
            position = new Vector2(centerX, targetMin.y - gap - half.y);
        else
            position = new Vector2(targetMax.x + gap + half.x, (targetMin.y + targetMax.y) * 0.5f);
        position.x = Mathf.Clamp(position.x, canvas.xMin + margin + half.x, canvas.xMax - margin - half.x);
        position.y = Mathf.Clamp(position.y, canvas.yMin + margin + half.y, canvas.yMax - margin - half.y);
        _messageBox.localPosition = position;
    }

    private void UpdateSpotlight(Vector2 holeMin, Vector2 holeMax)
    {
        Rect canvas = _canvasRect.rect;
        holeMin = Vector2.Max(canvas.min, Vector2.Min(canvas.max, holeMin));
        holeMax = Vector2.Max(holeMin, Vector2.Min(canvas.max, holeMax));
        // 네 사각형이 강조 영역만 비워 두며, 겹치지 않아 어둡기가 균일하다.
        PlacePanel(0, canvas.min, new Vector2(canvas.xMax, holeMin.y));
        PlacePanel(1, new Vector2(canvas.xMin, holeMax.y), canvas.max);
        PlacePanel(2, new Vector2(canvas.xMin, holeMin.y), new Vector2(holeMin.x, holeMax.y));
        PlacePanel(3, new Vector2(holeMax.x, holeMin.y), new Vector2(canvas.xMax, holeMax.y));
    }

    private void PlacePanel(int index, Vector2 min, Vector2 max)
    {
        Image panel = _spotlightPanels[index];
        panel.rectTransform.anchoredPosition = min - _canvasRect.rect.min;
        panel.rectTransform.sizeDelta = max - min;
        panel.color = new Color(0f, 0f, 0f, SpotlightDarkness);
    }

    private void OnDestroy()
    {
        if (_tutorialFont == null) return;
        foreach (Texture2D atlas in _tutorialFont.atlasTextures)
            if (atlas != null) Destroy(atlas);
        if (_tutorialFont.material != null) Destroy(_tutorialFont.material);
        Destroy(_tutorialFont);
    }

    public void HideExplanation()
    {
        _target = null;
        SetSpotlight(false);
        Highlight.gameObject.SetActive(false);
        _messageBox.gameObject.SetActive(false);
        Progress.gameObject.SetActive(false);
        if (_waveText != null) _waveText.text = string.Empty;
    }
}
