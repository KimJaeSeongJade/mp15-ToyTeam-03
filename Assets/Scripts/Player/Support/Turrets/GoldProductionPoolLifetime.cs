using UnityEngine;

// 판매 후에도 배달 중인 골드를 보존하고, 배달이 끝나면 풀을 정리한다.
public class GoldProductionPoolLifetime : MonoBehaviour
{
    private GoldProductionTurret _owner;
    private ObjectPool<GoldDrop> _pool;
    public void Initialize(GoldProductionTurret owner, ObjectPool<GoldDrop> pool)
    {
        _owner = owner;
        _pool = pool;
    }

    private void Update()
    {
        if (_owner != null || _pool == null) return;

        foreach (GoldDrop drop in _pool.ObjectList)
        {
            if (drop.gameObject.activeSelf) return;
        }

        Destroy(gameObject);
    }

    private void OnDestroy() => _pool?.DestroyAll();
}
