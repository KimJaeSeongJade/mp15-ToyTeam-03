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