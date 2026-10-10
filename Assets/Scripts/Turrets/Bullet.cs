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
    protected Transform _target; // 추적할 몬스터 타겟

    public void SetDamage(float newDamage)
    {
        if (newDamage > 0)
        {
            bulletDamage = newDamage;
        }
    }

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
        // 💡 [요청 사항 반영 1] 풀 반환 시 이전 타깃을 해제하여 데이터 꼬임을 방지합니다.
        _target = null;

        transform.SetParent(_returnPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
        gameObject.SetActive(false);
    }

    protected virtual void OnTriggerEnter(Collider other) 
    { 
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0) 
        { 
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                DamageLogic(damageable);
            }
            ReturnToPool();
        } 
    }
    
    private IEnumerator LifeTimeRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);
        ReturnToPool();
    }

    public void InitDate()
    {
        _returnPoint = transform.parent;
    }

    protected virtual void Move()
    {
        // 💡 [요청 사항 반영 2] 타깃이 존재하더라도 이미 사망했거나 비활성화(오브젝트 풀 반환) 상태라면 
        // 유령을 계속 추적하지 않도록 실시간 가드 처리를 거쳐 타깃을 null로 밀어버립니다.
        if (_target != null && !_target.gameObject.activeInHierarchy)
        {
            _target = null;
        }

        // 타깃이 없다면(혹은 방금 사라졌다면) 날아가던 정면 방향 그대로 직진 처리합니다.
        if (_target == null)
        {
            // 💡 꼬임 방지 팁: 이미 transform.forward가 설정되어 있으므로 Vector3.forward(로컬 정면)로 자연스럽게 전진합니다.
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        // 타겟이 있는 방향으로 이동 및 회전
        Vector3 direction = (_target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        transform.forward = direction; // 총알이 몬스터를 바라보도록 회전
    }

    protected virtual void DamageLogic(IDamageable damageable)
    {
        damageable.TakeDamage(bulletDamage);
    }
    protected virtual void DamageLogic(IDamageable damageable, float finalDamage)
    {
        damageable.TakeDamage(finalDamage);
    }
}