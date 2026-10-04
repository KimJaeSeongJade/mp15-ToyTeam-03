using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour{
    [Header("공격 설정")]
    public float damage = 10f;          // 적의 공격력
    public float attackRange = 2f;      // 공격 사정거리
    public float attackCooldown = 1.5f; // 공격 주기 (초)
    private float lastAttackTime;       // 마지막 공격 시간 저장

    [Header("레이어 설정")]
    public LayerMask turretLayer;       // 감지할 터렛 레이어 (터렛들이 포함된 레이어 선택)

    void Update()
    {
        // 쿨타임이 지났는지 먼저 확인
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            CheckAndAttack();
        }
    }

    void CheckAndAttack()
    {
        // 적 위치 기준, attackRange 반경 내에 turretLayer를 가진 터렛을 물리적으로 탐지합니다.
        Collider[] hitTurrets = Physics.OverlapSphere(transform.position, attackRange, turretLayer);

        // 범위 내에 터렛이 발견되었다면
        if (hitTurrets.Length > 0)
        {
            // 가장 첫 번째로 감지된 터렛의 콜라이더를 가져옵니다.
            Collider turretCollider = hitTurrets[0];

            // 부모 또는 본인 오브젝트에서 BaseTurret 컴포넌트를 가져옵니다.
            // DefenceTurret은 BaseTurret을 상속받았으므로 BaseTurret으로 가져올 수 있습니다.
            DefenceTurret targetTurret = turretCollider.GetComponentInParent<DefenceTurret>();

            if (targetTurret != null)
            {
                Attack(targetTurret);
            }
        }
    }

    void Attack(DefenceTurret target)
    {
        // 공격 시간 최신화
        lastAttackTime = Time.time;

        // 터렛의 TakeDamage 메서드를 호출하여 데미지를 줍니다.
        target.TakeDamage(damage);

    }

    // 에디터에서 적의 공격 범위를 시각적으로 확인
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
