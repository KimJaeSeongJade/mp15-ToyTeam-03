using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TutorialGuideScreenUI : MonoBehaviour
{
    public TMP_Text Label;
    public TMP_Text Distance;
    [Tooltip("화면 밖 방향을 표시하는 별도 자식 UI. 오른쪽이 0도입니다.")]
    public RectTransform DirectionArrow;
    [Tooltip("위쪽을 향하는 이미지이면 -90을 지정합니다.")]
    public float ArrowAngleOffset;
    public Vector2 OffscreenLabelOffset = new Vector2(0f, -48f);

    private RectTransform _rect;
    private Vector3 _labelPosition;
    private Quaternion _arrowRotation;
    private bool _initialized;

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        _rect = GetComponent<RectTransform>();
        _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
        if (Label != null) _labelPosition = Label.rectTransform.localPosition;
        if (DirectionArrow != null) _arrowRotation = DirectionArrow.localRotation;
        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
    }

    public void UpdateDisplay(Vector2 position, bool offscreen, float angle, string label, string distance)
    {
        Initialize();
        _rect.localPosition = position;
        if (Label != null)
        {
            Label.text = label;
            if (Label.rectTransform != _rect && Label.rectTransform != DirectionArrow)
                Label.rectTransform.localPosition = _labelPosition + (Vector3)(offscreen ? OffscreenLabelOffset : Vector2.zero);
        }
        if (Distance != null) Distance.text = distance;
        if (DirectionArrow != null && DirectionArrow != _rect)
        {
            DirectionArrow.gameObject.SetActive(offscreen);
            DirectionArrow.localRotation = Quaternion.Euler(0f, 0f, angle + ArrowAngleOffset) * _arrowRotation;
        }
    }
}
