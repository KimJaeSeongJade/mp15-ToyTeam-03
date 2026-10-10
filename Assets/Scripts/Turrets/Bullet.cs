using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolObject
{ 
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected float bulletDamage = 10f; // 총알의 기본 데미지
    [SerializeField] private float speed = 15f; // 총알 이동 속도
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private Transform _returnPoint;
    private Transform _target; // 추적할 몬스터 타겟
    
    public void SetDamage(float newDamage)
    {
        // 터렛에서 넘겨준 버프 데미지가 정상적인 양수일 때만 덮어씁니다.
        if (newDamage > 0)
        {
            bulletDamage = newDamage;
        }
    }

    // 💡 터렛에서 총알을 생성(Instantiate)한 후 호출하여 타겟을 넘겨받는 함수
    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void OnEnable()
    {
        StartCoroutine(LifeTimeRoutine());
    }

    private void Update()
    {
        Move();
    }

    public override void WakeUp()
    {
        transform.SetParent(null);
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        transform.SetParent(_returnPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
        gameObject.SetActive(false);
    }

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
                DamageLogic(damageable);
            }

            // 충돌했으므로 총알 제거
            //Destroy(gameObject);
            ReturnToPool();
        } 
    }

    
    private IEnumerator LifeTimeRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);
        // 풀링 종료
        ReturnToPool();
    }

    public void InitDate()
    {
        _returnPoint = transform.parent;
    }

    protected virtual void Move()
    {
        // 타겟이 없다면 앞으로 직진하거나 소멸 처리
        if (_target == null)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // 💡 타겟이 있는 방향으로 이동 및 회전
        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.forward = direction; // 총알이 몬스터를 바라보도록 회전
    }

    protected virtual void DamageLogic(IDamageable damageable)
    {
        damageable.TakeDamage(bulletDamage);
    }
}