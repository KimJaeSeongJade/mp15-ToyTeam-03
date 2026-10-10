using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class TutorialBuildPreparation
{
    public BuildPoint Point;
    public bool SnapToNavMesh;
    public BaseTurret ProvidedTurret;
    public bool DisableProvidedTurret = true;
}

[Serializable]
public class TutorialPrepareStep : TutorialStep
{
    public bool EmptyWallet;
    public TutorialBuildPreparation[] BuildPoints = new TutorialBuildPreparation[0];

    public override IEnumerator Execute(TutorialContext context)
    {
        if (EmptyWallet) context.Wallet.TrySpendGold(context.Wallet.Gold);
        foreach (TutorialBuildPreparation setup in BuildPoints)
        {
            if (setup == null || setup.Point == null) { context.Fail("준비 단계의 빌드포인트가 비어 있습니다."); yield break; }
            if (setup.SnapToNavMesh && NavMesh.SamplePosition(setup.Point.transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                setup.Point.transform.position = hit.position + Vector3.up * 0.03f;
            if (setup.ProvidedTurret != null && !setup.Point.HasTurret)
            {
                if (!setup.Point.TryBuildTurret(setup.ProvidedTurret, out int cost))
                { context.Fail("판매 시연용 터렛을 배치할 수 없습니다."); yield break; }
                if (setup.DisableProvidedTurret)
                    foreach (BaseTurret turret in setup.Point.GetComponentsInChildren<BaseTurret>()) turret.enabled = false;
            }
        }
        yield break;
    }
}

public enum TutorialBuildCompletion { Installed, AimedAtPoint }

[Serializable]
public class TutorialBuildStep : TutorialStep
{
    public BuildPoint Point;
    public BaseTurret ExpectedTurret;
    [Range(0, 1)] public int Slot;
    public string MarkerLabel;
    public bool RequireSlotKey = true;
    public bool AllowPlacement = true;
    public TutorialBuildCompletion Completion;

    public override IEnumerator Execute(TutorialContext context)
    {
        if (Point == null) { context.Fail("건설 단계의 빌드포인트가 비어 있습니다."); yield break; }
        context.GuideBuild(Point, MarkerLabel);
        var controls = new TutorialControls { Attack = AllowPlacement, Build = RequireSlotKey, BuildSlot = Slot };
        if (RequireSlotKey)
            yield return context.Prompt(Text, controls, Slot == 0 ? PlayerInputAction.Turret1 : PlayerInputAction.Turret2);
        else
            yield return context.Prompt(Text, controls, PlayerInputAction.Attack);
        yield return new WaitUntil(() => !context.Paused && (Completion == TutorialBuildCompletion.AimedAtPoint
            ? context.IsLookingAt(Point) : context.IsInstalled(Point, ExpectedTurret)));
    }
}

[Serializable]
public class TutorialSellStep : TutorialStep
{
    public BuildPoint Point;
    public string MarkerLabel;
    public TutorialPresentation EnterText = new TutorialPresentation();
    [Range(0, 1)] public int Slot;

    public override IEnumerator Execute(TutorialContext context)
    {
        if (Point == null || !Point.HasTurret) { context.Fail("판매할 터렛을 준비하세요."); yield break; }
        context.GuideBuild(Point, MarkerLabel);
        yield return context.Prompt(EnterText, new TutorialControls { Build = true, BuildSlot = Slot },
            Slot == 0 ? PlayerInputAction.Turret1 : PlayerInputAction.Turret2);
        yield return new WaitUntil(() => context.IsLookingAt(Point) && !context.Paused);
        yield return context.Prompt(Text, new TutorialControls { Skill = true, BuildSlot = Slot }, PlayerInputAction.Skill);
        yield return new WaitUntil(() => !Point.HasTurret && !context.Paused);
        context.Input.AllowSkill = false;
    }
}

[Serializable]
public class TutorialWalletCheckStep : TutorialStep
{
    [Min(0)] public int ExpectedGold;

    public override IEnumerator Execute(TutorialContext context)
    {
        if (context.Wallet.Gold != ExpectedGold)
            context.Fail($"단계 {Name}: 필요한 잔액은 {ExpectedGold}G, 현재 {context.Wallet.Gold}G입니다.");
        yield break;
    }
}
