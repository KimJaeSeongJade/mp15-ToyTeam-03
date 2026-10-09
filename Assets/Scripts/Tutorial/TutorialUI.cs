using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialFocus { None, Move, Attack, Skill, Build, Gold, Castle, ExitBuild }

// 기존 HUD 위에 설명/강조/스킵 확인만 추가한다.
public class TutorialUI : MonoBehaviour
{
    public TMP_Text Message;
    public TMP_Text Progress;
    public TMP_Text Gold;
    public TMP_Text CastleHealth;
    public TMP_Text AttackLabel;
    public TMP_Text SkillLabel;
    public RectTransform[] FocusTargets;
    public RectTransform Highlight;
    public Image HighlightImage;
    public GameObject SkipDialog;
    public Button SkipButton;
    public Button ContinueButton;
    [Range(0f, 1f)] public float SpotlightDarkness = 0.8f;

    private RectTransform _canvasRect;
    private RectTransform _target;
    private RectTransform _buildTarget;
    private RectTransform _exitBuildTarget;
    private TMP_Text _waveText;
    private RectTransform _spotlightRoot;
    private readonly Image[] _spotlightPanels = new Image[4];
    private readonly Vector3[] _corners = new Vector3[4];
    private bool _spotlightRequested;

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
        SetActive(root, "PausePanel", false);
        SetActive(root, "StageClearText", false);
        SetActive(root, "WaveTimerBackGround", false);
        Gold = Find(root, "GoldText")?.GetComponent<TMP_Text>();
        CastleHealth = null; // 기존 HUD에 캐슬 HP 표시가 추가되면 여기에 연결한다.
        AttackLabel = null;
        SkillLabel = null;
        _waveText = Find(root, "PrepareTimeUI")?.GetComponent<TMP_Text>();
        _buildTarget = Find(root, "Turret1UI") as RectTransform;
        _exitBuildTarget = Find(root, "ExitBuildModeUI") as RectTransform;
        FocusTargets = new RectTransform[8];
        FocusTargets[(int)TutorialFocus.Attack] = Find(root, "BasicAttackUI") as RectTransform;
        FocusTargets[(int)TutorialFocus.Skill] = Find(root, "SpecialAttackUI") as RectTransform;
        FocusTargets[(int)TutorialFocus.Build] = _buildTarget;
        FocusTargets[(int)TutorialFocus.Gold] = Gold != null ? Gold.rectTransform : null;
        FocusTargets[(int)TutorialFocus.ExitBuild] = _exitBuildTarget;
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
        SkipDialog.SetActive(false);
        Highlight.gameObject.SetActive(false);
        CreateSpotlight();
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
        Message.text = message;
        SetSpotlight(false);
        Progress.text = progress;
        if (_waveText != null) _waveText.text = progress;
        int index = (int)focus;
        _target = FocusTargets != null && index < FocusTargets.Length ? FocusTargets[index] : null;
        Highlight.gameObject.SetActive(_target != null);
    }

    public void SetGold(int amount) { if (Gold != null) Gold.text = amount.ToString(); }
    public void SetCastleHealth(float current, float max)
    {
        if (CastleHealth != null) CastleHealth.text = $"캐슬 HP: {current:0} / {max:0}";
        else Message.text = $"캐슬 HP: {current:0} / {max:0} — 도착하기 전에 막으세요!";
    }

    public void SetKeys(PlayerInputReader input)
    {
        if (AttackLabel != null) AttackLabel.text = $"{KeyName(input.AttackKey)}\n일반 공격";
        if (SkillLabel != null) SkillLabel.text = $"{KeyName(input.SkillKey)}\n스킬 (누른 뒤 놓기)";
    }

    public static string KeyName(KeyCode key)
    {
        if (key == KeyCode.Mouse0) return "마우스 왼쪽";
        if (key == KeyCode.Mouse1) return "마우스 오른쪽";
        return key.ToString();
    }

    private void LateUpdate()
    {
        bool visible = _target != null && _target.gameObject.activeInHierarchy;
        Highlight.gameObject.SetActive(visible);
        bool dim = visible && _spotlightRequested && !SkipDialog.activeSelf;
        _spotlightRoot.gameObject.SetActive(dim);
        if (!visible) return;
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
        if (dim) UpdateSpotlight(min - Vector2.one * 8f, max + Vector2.one * 8f);
        Color color = HighlightImage.color;
        color.a = 0.25f + 0.35f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f));
        HighlightImage.color = color;
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
}
