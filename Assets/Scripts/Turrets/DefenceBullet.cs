using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceBullet : Bullet
{
    protected override void OnTriggerEnter(Collider other)
    {
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0) 
        { 
            // 2. 상대방에게 IDamageable 인터페이스가 있는지 콤포넌트 추출 시도
            // 💡 철자 주의: IDamagable -> IDamageable (e 추가)
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // 3. 인터페이스의 TakeDamage 메서드 호출
                // TODO 인터페이스로 몬스터한테 슬로우효과 구현
                damageable.SlowSpeed(bulletDamage);
            }

            // 충돌했으므로 총알 제거
            Destroy(gameObject); 
        } 
    }
}
