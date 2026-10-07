using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreedyBullet : Bullet
{
    [SerializeField] private Tire2GreedyFireTurret _ownerTurret; // 이 총알을 발사한 주인 터렛 참조

    // 💡 터렛이 총알을 생성(또는 풀링)한 직후 주인을 주입해주는 함수
    public void SetOwner(Tire2GreedyFireTurret owner)
    {
        if (_ownerTurret != null)
        {
            return;
        }
        _ownerTurret = owner;
    }
    
    protected override void OnTriggerEnter(Collider other) 
    { 
        // 1. 레이어마스크 조건에 부합하는지 확인
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0) 
        { 
            // 2. 상대방에게 IDamageable 인터페이스가 있는지 컴포넌트 추출 시도
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // 💡 [발사 횟수당 공격력 증가 반영] 
                // 주인 터렛이 존재한다면, 터렛이 실시간으로 계산 중인 버프 대미지를 가져와 합산합니다.
                if (_ownerTurret != null)
                {
                    float finalDamage = bulletDamage + _ownerTurret.CurrentDamageBonus;
                    
                    // 3. 데미지 로직 실행 (버프가 반영된 데미지로 적을 타격)
                    DamageLogic(damageable,finalDamage);
                }
            }

            // 충돌했으므로 총알을 오브젝트 풀로 반환
            ReturnToPool();
        } 
    }
}
