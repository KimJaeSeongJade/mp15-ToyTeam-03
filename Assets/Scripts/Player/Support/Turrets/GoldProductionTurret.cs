using UnityEngine;

public class GoldProductionTurret : GoldEffectTurret
{
    [SerializeField] private GoldDrop _goldPrefab;
    private ObjectPool<GoldDrop> _goldPool;

    [SerializeField, Min(1)] private int _dropsPerProduction = 3;

    protected override void Start()
    {
        base.Start();

        _goldPool = new ObjectPool<GoldDrop>(_goldPrefab, 3, _muzzle, drop =>
        {
            drop.transform.SetParent(_muzzle, false);
            drop.transform.localPosition = Vector3.zero;
            drop.InitData();
        });

        _muzzle.GetComponent<GoldProductionPoolLifetime>().Initialize(this, _goldPool);
    }
    protected override void ActivateEffect()
    {
        for (int i = 0; i < _dropsPerProduction; i++)
        {
            _goldPool.Pop();
        }

        NotifyEffect();
    }

    // 배달 중인 골드는 생산 타워가 판매되어도 남겨 둔다.
}
