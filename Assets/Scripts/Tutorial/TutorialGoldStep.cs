using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class TutorialGoldStep : TutorialStep
{
    [Min(0f)] public float BeforeDelivery = 1.5f;
    [Min(0f)] public float AfterDelivery = 0.75f;

    public override IEnumerator Execute(TutorialContext context)
    {
        if (context.GoldDelivery == null) { context.Fail("골드 배달 컴포넌트를 연결하세요."); yield break; }
        context.Input.InputEnabled = false;
        Bounds bounds = new Bounds(context.Player.transform.position, Vector3.zero);
        foreach (GoldDrop drop in UnityEngine.Object.FindObjectsOfType<GoldDrop>())
            if (drop.gameObject.scene == context.Player.gameObject.scene) bounds.Encapsulate(drop.transform.position);
        float distance = Mathf.Max(10f, bounds.extents.magnitude * 1.6f + 5f);
        Vector3 offset = -context.Player.transform.forward * distance + context.Player.transform.right * distance * 0.3f
            + Vector3.up * distance * 0.65f;
        context.Camera.BeginView(context.Player, bounds.center + Vector3.up, offset);
        Text.Show(context);
        yield return context.Wait(BeforeDelivery);
        yield return context.GoldDelivery.Deliver();
        yield return context.Wait(AfterDelivery);
        context.Camera.RestoreCamera();
    }
}
