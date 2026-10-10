using System;
using System.Collections;
using UnityEngine;

// 일반 직렬화 데이터다. GameObject에 붙이지 않는다.
[Serializable]
public abstract class TutorialStep
{
    public bool Enabled = true;
    public string Name;
    [Tooltip("이 단계에서 화면 중앙의 크로스헤어를 표시합니다.")]
    public bool ShowCrosshair = true;
    public TutorialPresentation Text = new TutorialPresentation();
    public abstract IEnumerator Execute(TutorialContext context);
}

public enum TutorialCrosshairVisibility { InheritStep, Show, Hide }

[Serializable]
public class TutorialPresentation
{
    public string Title;
    [TextArea(2, 4)] public string Message;
    public TutorialFocus Focus;
    [Tooltip("지정하면 기본 HUD 강조 대상 대신 사용합니다.")]
    public RectTransform FocusOverride;
    [Tooltip("이 안내 문구를 표시할 때의 크로스헤어 상태: 단계 설정 유지 / 표시 / 숨김")]
    public TutorialCrosshairVisibility Crosshair;

    public void Show(TutorialContext context, int current = 0, int target = 0, string key = "", float seconds = 0f)
    {
        context.UI.SetCrosshairVisible(Crosshair == TutorialCrosshairVisibility.InheritStep
            ? context.StepCrosshairVisible : Crosshair == TutorialCrosshairVisibility.Show);
        context.UI.Show(context.Format(Message, current, target, key, seconds), Focus,
            context.Format(Title, current, target, key, seconds), FocusOverride, this);
    }
}

[Serializable]
public class TutorialControls
{
    public bool InputEnabled = true;
    public bool Attack;
    public bool Skill;
    public bool Build;
    public bool ExitBuild;
    [Tooltip("0: 1번 / 1: 2번 / -1: 배운 두 슬롯 모두 허용")]
    [Range(-1, 1)] public int BuildSlot = -1;

    public void Apply(TutorialContext context)
    {
        context.Input.InputEnabled = InputEnabled && !context.Paused;
        context.Input.AllowAttack = Attack;
        context.Input.AllowSkill = Skill;
        context.Input.AllowBuild = Build;
        context.Input.AllowExitBuild = ExitBuild;
        context.Input.AllowedBuildSlot = BuildSlot;
    }
}
