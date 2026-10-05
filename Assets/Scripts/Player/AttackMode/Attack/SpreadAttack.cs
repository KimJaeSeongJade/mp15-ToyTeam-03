using UnityEngine;

public class SpreadAttack : BasicAttack
{
    [SerializeField] private OrbitAttack _subBullet;
    [SerializeField, Min(1)] private int _subBuleltCount = 1;
    [SerializeField] private float _spinSpeed = 360f;
    [SerializeField, Min(0f)] private float _width = 1f;
    [SerializeField, Min(0f)] private float _rearOffset = 0.5f;

    private ObjectPool<OrbitAttack> _leftPool;
    private ObjectPool<OrbitAttack> _rightPool;
    private Transform _leftMuzzle;
    private Transform _rightMuzzle;

    private void Awake() => Init();

    public override void WakeUp()
    {
        base.WakeUp();

        FireSide(_leftPool, _leftMuzzle, transform.position, transform.rotation, _width, 180f);
        FireSide(_rightPool, _rightMuzzle, transform.position, transform.rotation, _width, 0f);
    }

    private void FireSide(ObjectPool<OrbitAttack> pool, Transform muzzle, Vector3 center,
        Quaternion launchRotation, float radius, float initialAngle)
    {
        OrbitAttack orbit = pool.Pop();
        orbit.SetOrbit(center, launchRotation, radius, _rearOffset,
            _spinSpeed, initialAngle, muzzle);
    }

    private void Init()
    {
        _leftMuzzle = CreateSideMuzzle("Left");
        _rightMuzzle = CreateSideMuzzle("Right");

        _leftPool = new ObjectPool<OrbitAttack>(
            _subBullet, _subBuleltCount, _leftMuzzle, projectile => projectile.InitTransform());
        _rightPool = new ObjectPool<OrbitAttack>(
            _subBullet, _subBuleltCount, _rightMuzzle, projectile => projectile.InitTransform());
    }

    private Transform CreateSideMuzzle(string side)
    {
        GameObject muzzle = new GameObject($"Spread{side}Muzzle");
        muzzle.transform.SetParent(transform, false);
        return muzzle.transform;
    }
}
