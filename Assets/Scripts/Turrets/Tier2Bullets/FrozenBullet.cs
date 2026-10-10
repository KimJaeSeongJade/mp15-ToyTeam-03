using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenBullet : Bullet
{
    [SerializeField] private LayerMask _target;
    [SerializeField] private GameObject _slowZone;
    protected virtual void OnTriggerEnter(Collider other) 
    { 
        // 1. 레이어마스크 조건에 부합하는지 확인
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0) 
        { 
            // 2. 상대방에게 IDamageable 인터페이스가 있는지 컴포넌트 추출 시도
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // 3. 인터페이스의 TakeDamage 메서드 호출 (버프 반영된 데미지 입력)
                // damageable.TakeDamage(bulletDamage);
                ReturnToPool();
                Instantiate(_slowZone, Vector3.zero, Quaternion.identity);
            }
        } 
    }
}
