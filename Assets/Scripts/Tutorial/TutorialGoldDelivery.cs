using System.Collections;
using UnityEngine;

// 튜토리얼에서만 기존 골드 오브젝트의 배달을 시작한다.
public class TutorialGoldDelivery : MonoBehaviour
{
    public IEnumerator Deliver()
    {
        GoldDrop[] drops = FindObjectsOfType<GoldDrop>();
        foreach (GoldDrop drop in drops)
            if (drop.gameObject.scene == gameObject.scene) drop.StartDelivery();

        // 잔액이 일부만 들어온 순간이 아니라 실제 배달이 모두 끝날 때까지 기다린다.
        bool pending;
        do
        {
            pending = false;
            foreach (GoldDrop drop in drops)
                if (drop != null && drop.gameObject.activeInHierarchy
                    && drop.gameObject.scene == gameObject.scene) pending = true;
            if (pending) yield return null;
        } while (pending);
    }
}
