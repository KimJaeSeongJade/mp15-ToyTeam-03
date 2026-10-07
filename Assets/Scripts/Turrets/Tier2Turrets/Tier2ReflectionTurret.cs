using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2ReflectionTurret : BaseTurret
{
    [Header("Reflection Settings")]
    [SerializeField] protected float _sheildDamage = 15f; // 반격(반사) 대미지
    [SerializeField] private float _reflectionRange = 3f;  // 반격 가능한 주변 범위
    [SerializeField] private LayerMask _enemyLayer;        // 몬스터 레이어

    // 💡 1. 부모의 TakeDamage를 오버라이드하여 데미지를 먼저 처리하고 반격합니다.
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        ReflectDamage();
    }

    private void ReflectDamage()
    {
        // 내 주변 범위(_reflectionRange) 안의 몬스터 레이어를 긁어옵니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _reflectionRange, _enemyLayer);

        foreach (Collider col in hitColliders)
            // 나를 때린 적(IDamageable을 가지고 있는 대상)을 검출합니다.
            if (col.TryGetComponent(out IDamageable monsterDamageable))
            {
                // 몬스터의 TakeDamage를 호출하여 반격 대미지를 꽂아넣습니다.
                monsterDamageable.TakeDamage(_sheildDamage);
                Debug.Log($"[반격 성공] {col.name}에게 {_sheildDamage}만큼의 반사 대미지를 주었습니다.");
                
                // 단일 대상 반격이므로 한 마리만 때리고 루프를 나갑니다.
                break; 
            }
    }
}