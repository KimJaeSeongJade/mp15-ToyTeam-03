using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour 
{ 
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float rotateSpeed = 15f; // 유도 각도 회전력
    [SerializeField] private float lifeTime = 5f;      // 소멸 시간 (초단위 기본값 지정)

    [Header("공격 및 감지 설정")]
    [SerializeField] private LayerMask enemyLayer;     // 적 레이어마스크
    [SerializeField] private float bulletDamage = 10f; // 적에게 줄 데미지

    private Transform targetEnemy;                     // 트리거로 감지한 타겟 적

    private void Start()
    {
        // 허공으로 날아가 아무것도 안 만나면 자동 소멸
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 1. 아직 감지된 적이 없다면 생성 당시의 정면 방향으로 직진합니다.
        if (targetEnemy == null)
        {
            transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
            return;
        }

        // 2. 적이 감지되었다면 적의 실시간 위치를 향해 방향을 꺾으며 이동합니다.
        Vector3 direction = (targetEnemy.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        
        // 부드럽게 적을 향해 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        
        // 앞으로 전진
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }

    // 총알의 트리거 영역에 무언가 들어왔을 때 실행됩니다.
    private void OnTriggerEnter(Collider other) 
    { 
        // 레이어마스크로 적(Enemy)인지 먼저 확인합니다.
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0) 
        { 
            // [상황 A] 이미 추적 중인 적이거나, 처음 만난 적에게 완전히 '닿았을' 때
            if (other.transform == targetEnemy || targetEnemy == null)
            {
                // 조금 더 가까이 확실하게 부딪혔는지 검사 (또는 바로 데미지 처리)
                if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.TakeDamage(bulletDamage);
                }

                // 적에게 닿았으므로 총알을 파괴합니다.
                Destroy(gameObject);
                return;
            }

            // [상황 B] 아직 타겟이 없는데 먼 거리의 트리거 감지 범위에 적이 먼저 포착되었을 때
            if (targetEnemy == null)
            {
                targetEnemy = other.transform;
            }
        } 
    } 
    
}