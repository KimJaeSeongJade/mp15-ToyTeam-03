using UnityEngine;

// 튜토리얼 전용 스킬 복제본의 시작과 종료를 등록한다.
public class TutorialSkillLifetime : MonoBehaviour
{
    public TutorialManager Owner;

    private void OnEnable()
    {
        Owner.Context.RegisterSkill(this);
    }

    private void OnDisable()
    {
        if (Owner == null) return;
        Owner.Context.UnregisterSkill(this);
    }
}
