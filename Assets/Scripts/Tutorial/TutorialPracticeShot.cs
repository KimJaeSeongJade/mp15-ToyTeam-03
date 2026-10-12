using UnityEngine;

// 튜토리얼에서만 만든 비활성 공격 템플릿에 붙인다. 풀 생성은 세지 않고 실제 발사만 센다.
public class TutorialPracticeShot : MonoBehaviour
{
    public TutorialManager Owner;
    private void OnEnable()
    {
        if (Owner != null) Owner.NotifyPracticeShot();
    }
}
