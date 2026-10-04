using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2BaseBullet : PoolObject
{
    [SerializeField] protected LayerMask _damagableMask;
    [SerializeField] protected float _damage;
    [SerializeField] protected float _moveSpeed;
    [SerializeField] protected float _range;
    [SerializeField] protected float _detectRadius;

    private Transform _returnTr;

    public override void WakeUp() => OnFireStart();
    public override void Sleep() => OnFireEnd();

    protected virtual void DetectTaraget(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent<IDamageable>(out var damageable))
        {
            Hit(damageable);
        }

        ReturnToPool();
    }

    protected void Hit(IDamageable damageable)
    {
        damageable.TakeDamage(_damage);
    }

    protected virtual void OnFireStart()
    {
        gameObject.SetActive(true);

        transform.SetParent(null);
    }

    protected virtual void OnFireEnd()
    {
        transform.SetParent(_returnTr, false);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));

        gameObject.SetActive(false);
    }

    public virtual void InitData()
    {
        _returnTr = transform.parent;
    }
}
