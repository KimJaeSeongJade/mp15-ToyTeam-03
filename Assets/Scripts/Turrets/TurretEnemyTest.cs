using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemyTest : MonoBehaviour, IDamageable
{
    private BaseEnemy Cacheenemy;

    private float goldCount;
    private bool _isDead;
    private bool _isSlow;

    private MonsterMove _speed;

    // 💡 [에러 원천 차단 트릭] BaseEnemy의 모르는 변수를 참조하지 않고,
    // 게임 시작 시점(혹은 풀에서 꺼내질 때)의 체력을 최대 체력으로 직접 기억해 둡니다.
    private float _maxHp; 

    private void Awake()
    {
        CacheComponent();
        
        // 💡 처음 생성될 때 들고 있는 체력을 최대 체력의 기준으로 삼습니다.
        if (Cacheenemy != null)
        {
            _maxHp = Cacheenemy.MonHp;
        }
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

    // 💡 인터페이스 전용 처형 함수 구조 완벽 정제
    public bool CheckExecution(float thresholdRatio)
    {
        if (_isDead) return false;

        // 💡 [버그 해결] 존재하지 않는 부모의 MonMaxHp 대신, 
        // 우리가 Awake 시점에 백업해 둔 자체 _maxHp 변수를 활용해 체력 비율을 구합니다.
        float maxHp = _maxHp > 0 ? _maxHp : 100f; 
        float hpRatio = Cacheenemy.MonHp / maxHp;

        // 기획된 처형 기준 수치 이하인지 검사 (ex: 15% 이하)
        if (hpRatio <= thresholdRatio && Cacheenemy.MonHp > 0)
        {
            Debug.Log($"<color=red>[🚨 적 자체 처형]</color> {gameObject.name}이 체력 기준치({hpRatio * 100f:F1}%) 미달로 스스로 소멸합니다.");
            
            _isDead = true;
            Cacheenemy.MonHp = 0;
            Cacheenemy.ReturnToPool(); // 안전하게 오브젝트 풀 반환
            
            return true; // 처형 성공 신호 리턴
        }

        return false; 
    }
}