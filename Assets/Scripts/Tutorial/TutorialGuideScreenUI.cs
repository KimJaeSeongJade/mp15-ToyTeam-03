using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 교체할 화면 가이드 프리팹의 루트에 붙이고 필요한 자식 UI를 연결한다.
[RequireComponent(typeof(RectTransform))]
public class TutorialGuideScreenUI : MonoBehaviour
{
    [Tooltip("이름·거리 표시. 문구는 Guide Style / Label Format에서 설정합니다.")]
    public TMP_Text Label;
    [Tooltip("거리를 별도 칸에 표시할 때만 지정합니다.")]
    public TMP_Text Distance;
    [Tooltip("화면 밖 대상 방향을 나타내는 자식 오브젝트. 오른쪽이 각도 0도입니다.")]
    public RectTransform DirectionArrow;
    [Tooltip("화살표 이미지가 위쪽을 향하면 -90을 지정합니다.")]
    public float ArrowAngleOffset;
    public Vector2 OffscreenLabelOffset = new Vector2(0f, -48f);

    private RectTransform _rect;
    private Vector3 _labelPosition;
    private Quaternion _arrowRotation;

    public void Initialize()
    {
        _rect = GetComponent<RectTransform>();
        _rect.anchorMin = _rect.anchorMax = new Vector2(0.5f, 0.5f);
        if (Label != null) _labelPosition = Label.rectTransform.localPosition;
        if (DirectionArrow != null) _arrowRotation = DirectionArrow.localRotation;
        foreach (Graphic graphic in GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
    }

    public void UpdateDisplay(Vector2 position, bool offscreen, float angle, string label, string distance)
    {
        _rect.localPosition = position;
        if (Label != null)
        {
            Label.text = label;
            // 루트 자체가 텍스트이면 전체 가이드 위치를 덮어쓰지 않는다.
            if (Label.rectTransform != _rect && Label.rectTransform != DirectionArrow)
                Label.rectTransform.localPosition = _labelPosition + (Vector3)(offscreen ? OffscreenLabelOffset : Vector2.zero);
        }
        if (Distance != null) Distance.text = distance;
        // 화살표는 자식에 둔다. 루트를 끄면 이름·거리까지 사라지므로 제외한다.
        if (DirectionArrow != null && DirectionArrow != _rect)
        {
            DirectionArrow.gameObject.SetActive(offscreen);
            DirectionArrow.localRotation = Quaternion.Euler(0f, 0f, angle + ArrowAngleOffset) * _arrowRotation;
        }
    }
}
