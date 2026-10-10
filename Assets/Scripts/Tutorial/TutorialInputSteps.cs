using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class TutorialMessageStep : TutorialStep
{
    [Min(0f)] public float Duration = 2.5f;
    public bool DarkenOutside;
    public TutorialControls Controls = new TutorialControls();

    public override IEnumerator Execute(TutorialContext context)
    {
        Controls.Apply(context);
        Text.Show(context);
        context.UI.SetSpotlight(DarkenOutside);
        yield return context.Wait(Duration);
        context.UI.SetSpotlight(false);
    }
}

[Serializable]
public class TutorialMovementStep : TutorialStep
{
    public KeyCode Key = KeyCode.W;
    [Min(0.01f)] public float HoldSeconds = 1f;

    public override IEnumerator Execute(TutorialContext context)
    {
        var controls = new TutorialControls();
        context.Input.RequiredMoveKey = Key;
        yield return context.Prompt(Text, controls, PlayerInputAction.Move,
            key: context.KeyName(Key), seconds: HoldSeconds);
        float held = 0f;
        while (held < HoldSeconds)
        {
            if (!context.Paused) held = Input.GetKey(Key) ? held + Time.deltaTime : 0f;
            yield return null;
        }
        context.Input.RequiredMoveKey = null;
    }
}

public enum TutorialPracticeAction { Projectile, Skill }

[Serializable]
public class TutorialPracticeStep : TutorialStep
{
    public TutorialPracticeAction Action;
    [Min(1)] public int TargetCount = 5;
    public bool WaitForSkillToFinish = true;
    [Min(0f)] public float AfterFinishDelay;

    public override IEnumerator Execute(TutorialContext context)
    {
        bool skill = Action == TutorialPracticeAction.Skill;
        if (!skill && context.PracticeAttackTemplate == null)
        { context.Fail("발사 횟수 연습에는 공격 템플릿이 필요합니다."); yield break; }
        int count = 0;
        string key = context.KeyName(skill ? context.Reader.SkillKey : context.Reader.AttackKey);
        System.Action onUsed = () =>
        {
            if (count >= TargetCount) return;
            count++;
            Text.Show(context, count, TargetCount, key);
        };
        if (skill) context.SkillUsed += onUsed;
        else context.ShotFired += onUsed;
        try
        {
            yield return context.Prompt(Text, new TutorialControls { Attack = !skill, Skill = skill },
                skill ? PlayerInputAction.Skill : PlayerInputAction.Attack, 0, TargetCount, key);
            yield return new WaitUntil(() => count >= TargetCount && !context.Paused);
        }
        finally
        {
            if (skill) context.SkillUsed -= onUsed;
            else context.ShotFired -= onUsed;
        }
        context.Input.AllowAttack = false;
        context.Input.AllowSkill = false;
        if (skill && WaitForSkillToFinish) yield return new WaitUntil(() => !context.HasActiveSkill() && !context.Paused);
        context.Input.InputEnabled = false;
        yield return context.Wait(AfterFinishDelay);
    }
}

[Serializable]
public class TutorialExitBuildStep : TutorialStep
{
    public override IEnumerator Execute(TutorialContext context)
    {
        context.HideBuildGuide();
        yield return context.Prompt(Text, new TutorialControls { ExitBuild = true }, PlayerInputAction.ExitBuild,
            key: context.KeyName(context.Reader.ExitBuildKey));
        yield return new WaitUntil(() => context.Player.GetComponent<PlayerAttackMode>().enabled && !context.Paused);
        context.Input.AllowExitBuild = false;
        context.BuildAreas.Allow(null);
    }
}
