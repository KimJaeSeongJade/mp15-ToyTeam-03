using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tier2BaseTurret : BaseTurret
{
    [SerializeField] protected Transform _target;
    [SerializeField] protected Transform _muzzle;
    [SerializeField] protected Tier2BaseBullet _bullet;
    [SerializeField] private int _initMaxCount;
    [SerializeField] private float _fireTime;

    protected ObjectPool<Tier2BaseBullet> _bulletPool;
    protected float _elapseTime;
    protected bool canFire => _elapseTime >= _fireTime;

    public Transform Target
    {
        get { return _target; }
        set
        {
            _target = value;
        }
    }

    private void Start() => Init();

    private void Init()
    {
        _bulletPool = new ObjectPool<Tier2BaseBullet>(
            _bullet,
            _initMaxCount,
            _muzzle,
            bullet =>
            {
                bullet.InitData();
            }
            );

        _elapseTime = _fireTime;
    }
}
