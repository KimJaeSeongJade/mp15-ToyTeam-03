using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SniperTurret : Tier2BaseTurret
{
    private void Update()
    {
        TurretRotate();

        AttackCoolDown();
        Attack();
    }

    private void AttackCoolDown()
    {
        if (canFire) return;

        _elapseTime += Time.deltaTime;
    }

    public override void Attack()
    {
        if (!canFire || _target == null) return;

        _bullet = _bulletPool.Pop();

        _elapseTime = 0;
    }

    private void TurretRotate()
    {
        if (_target == null) return;

        Vector3 targetPosition = new Vector3(_target.position.x, transform.position.y, _target.position.z);

        transform.LookAt(targetPosition);
    }
}
