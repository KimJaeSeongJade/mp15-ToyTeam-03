using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTurret : BaseTurret
{
    [SerializeField] private float _attackRange;
    [SerializeField] private LayerMask _targetLayer;

    public override void Attack()
    {
        
    }
    
}
