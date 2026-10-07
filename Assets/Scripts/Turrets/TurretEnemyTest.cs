using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemyTest : MonoBehaviour,IDamageable
{
    //[SerializeField] private float _hp; // 변경예정
    private BaseEnemy Cacheenemy;

    private float goldCount;
    private bool _isDead;
    private bool _isSlow;

    private MonsterMove _speed;

    private void Awake()
    {
        CacheComponent();
    }

    private void OnMouseDown()
    {
        if (_isDead) return;

        Debug.Log($"🎯 [적 공격 시작] {gameObject.name}이 주변 터렛을 공격합니다.");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f);

        foreach (Collider col in hitColliders)
        {
            if (col.TryGetComponent(out BaseTurret turret))
            {
                // 💡 에러 나는 코드 대신 테스트용 고정 대미지(예: 25f)를 다이렉트로 전달합니다.
                float attackDamage = 25f;

                turret.TakeDamage(attackDamage);
                break;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead == true) return;

        Cacheenemy.MonHp -= (damage - Cacheenemy.MonDefend);
        Debug.Log($"몬스터에게 {damage - Cacheenemy.MonDefend} 데미지를 줌");
        if (Cacheenemy.MonHp <= 0)
        {
            Debug.Log("죽음");
            _isDead = true;
            Cacheenemy.ReturnToPool();
        }
    }
    
    public void SlowSpeed(float speed)
    {
        if (!_isSlow)
        {
            _speed.Slow(speed);
            _isSlow = true;
        }
    }
    private void CacheComponent()
    {
        Cacheenemy = GetComponent<BaseEnemy>();
        _speed = GetComponent<MonsterMove>();
    }
}