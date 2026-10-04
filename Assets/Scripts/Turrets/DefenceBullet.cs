using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceBullet : Bullet
{
    protected override void DamageLogic(IDamageable damageable)
    {
        damageable.SlowSpeed(bulletDamage);
    }
}
