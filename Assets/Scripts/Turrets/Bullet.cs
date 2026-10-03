using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour 
{ 
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float bulletDamage = 10f; // 총알의 기본 데미지
    [SerializeField] private float speed = 15f;        // 총알 이동 속도

    private Transform _target; // 추적할 몬스터 타겟

    // 💡 터렛에서 총알을 생성(Instantiate)한 후, 이 함수를 호출해 타겟을 넘겨줍니다.
    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void Update()
    {
        // 타겟이 없다면 앞으로 직진하거나 소멸 처리
        if (_target == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // 💡 타겟(공중 몬스터)이 있는 방향으로 이동 및 회전
        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.forward = direction; // 총알이 몬스터를 바라보도록 회전
    }

    protected virtual void OnTriggerEnter(Collider other) 
    { 
        // 1. 레이어마스크 조건에 부합하는지 확인
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0) 
        { 
            // 2. 상대방에게 IDamageable 인터페이스가 있는지 콤포넌트 추출 시도
            // 💡 철자 주의: IDamagable -> IDamageable (e 추가)
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // 3. 인터페이스의 TakeDamage 메서드 호출
                damageable.TakeDamage(bulletDamage);
            }

            // 충돌했으므로 총알 제거
            Destroy(gameObject); 
        } 
    } 
}