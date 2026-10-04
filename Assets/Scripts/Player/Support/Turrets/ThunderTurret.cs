using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderTurret : Tier2BaseTurret
{
    private void Update()
    {
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
}
