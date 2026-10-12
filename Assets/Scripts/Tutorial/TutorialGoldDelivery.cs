using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 튜토리얼에서만 기존 골드 오브젝트의 배달을 시작한다.
public class TutorialGoldDelivery : MonoBehaviour
{
    private readonly List<GoldDrop> _drops = new List<GoldDrop>();

    public void Register(BaseEnemy enemy)
    {
        foreach (GoldDrop drop in enemy.GetComponentsInChildren<GoldDrop>(true))
        {
            _drops.Add(drop);
        }
    }

    public Bounds GetBounds(Vector3 playerPosition)
    {
        Bounds bounds = new Bounds(playerPosition, Vector3.zero);
        foreach (GoldDrop drop in _drops)
        {
            if (drop != null && drop.gameObject.activeInHierarchy)
                bounds.Encapsulate(drop.transform.position);
        }
        return bounds;
    }

    public IEnumerator Deliver()
    {
        foreach (GoldDrop drop in _drops)
        {
            if (drop != null && drop.gameObject.activeInHierarchy) drop.StartDelivery();
        }

        // 잔액이 일부만 들어온 순간이 아니라 실제 배달이 모두 끝날 때까지 기다린다.
        bool pending;
        do
        {
            pending = false;
            foreach (GoldDrop drop in _drops)
            {
                if (drop != null && drop.gameObject.activeInHierarchy) pending = true;
            }
            if (pending) yield return null;
        } while (pending);
    }

    public void StopDelivery()
    {
        foreach (GoldDrop drop in _drops)
        {
            if (drop != null && drop.gameObject.activeInHierarchy) drop.ReturnToPool();
        }
    }
}
