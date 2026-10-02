using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : PoolObject
{
    [SerializeField] private LayerMask _damagableMask;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _damage;
    [SerializeField] private float _range;
    [SerializeField] private float _detectRadius;
    
    private Transform _returnTr;
    private Vector3 _startPos;

    private void FixedUpdate() => MoveFoward();
    public override void WakeUp() => OnFireStart();
    public override void Sleep() => OnFireEnd();

    private void MoveFoward()
    {
        Vector3 currentPosition = transform.position;
        Vector3 move = transform.forward * (_moveSpeed * Time.fixedDeltaTime);
        Vector3 nextPosition = currentPosition + move;
        float moveDistance = move.magnitude;
        RaycastHit hit = default;
        bool hasHit = moveDistance > 0f &&
            Physics.SphereCast(currentPosition, _detectRadius, transform.forward, out hit, moveDistance, _damagableMask, QueryTriggerInteraction.Ignore);
        
        if (hasHit)
        {
            transform.position = hit.point;
            DetectTaraget(hit);
            return;
        }

        transform.position = nextPosition;

        Vector3 diff = _startPos - transform.position;
        if (diff.sqrMagnitude >= _range * _range)
        {
            ReturnToPool();
        }
    }

    protected virtual void DetectTaraget(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent<IDamagable>(out var damageable))
        {
            Hit(damageable);
        }

        ReturnToPool();
    }

    protected void Hit(IDamagable damageable)
    {
        damageable.TakeDamage(_damage);
    }

    private void OnFireStart()
    {
        gameObject.SetActive(true);

        transform.SetParent(null);
        _startPos = transform.position;

#if UNITY_EDITOR
        Debug.DrawLine(_startPos, transform.forward * _range, Color.red, 2f);
#endif
    }

    private void OnFireEnd()
    {
        transform.SetParent(_returnTr, false);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));

        gameObject.SetActive(false);
    }

    public void InitTransform()
    {
        _returnTr = transform.parent;

        
    }
}
