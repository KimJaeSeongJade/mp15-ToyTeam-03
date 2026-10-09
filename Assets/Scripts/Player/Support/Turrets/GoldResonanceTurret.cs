using UnityEngine;

public class GoldResonanceTurret : GoldEffectTurret
{
    [SerializeField, Min(0f)] private float _goldDetectionRadius = 15f;

    [SerializeField] private GoldResonancePulseBullet _pulsePrefab;
    private ObjectPool<GoldResonancePulseBullet> _pulses;

    protected override void Start()
    {
        base.Start();

        _bulletPool = new ObjectPool<Tier2BaseBullet>(_bullet, 3, transform, bullet =>
        {
            bullet.transform.SetParent(transform, false);
            bullet.InitData();
        });

        _pulses = new ObjectPool<GoldResonancePulseBullet>(_pulsePrefab, 1, transform, bullet =>
        {
            bullet.transform.SetParent(transform, false);
            bullet.InitData();
        });
    }

    protected override void ActivateEffect()
    {
        _pulses.Pop().Fire(transform.position, _goldDetectionRadius, SpawnShockwave);

        NotifyEffect();
    }

    private void SpawnShockwave(Vector3 position) => (_bulletPool.Pop() as GoldShockwaveBullet).Fire(position);

    private void OnDestroy()
    {
        _pulses?.DestroyAll();
        _bulletPool?.DestroyAll();
    }
}
