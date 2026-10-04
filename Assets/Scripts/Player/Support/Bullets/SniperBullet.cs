using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBullet : Tier2BaseBullet
{
    private Vector3 _startPos;

    private void FixedUpdate() => MoveFoward();

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

    protected override void DetectTaraget(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent<IDamageable>(out var damageable))
        {
            Hit(damageable);
        }
    }
}
