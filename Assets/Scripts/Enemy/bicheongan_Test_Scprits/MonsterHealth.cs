using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float _hp;
    [SerializeField] private GoldDrop _goldPrabas;
    private ObjectPool<GoldDrop> goldPool;
    private BaseEnemy Cacheenemy;
    private bool _isDead;
    private bool _isSlow;

    private MonsterMove _speed;// 임시 BaseEnemy 완성시 변경예정

    private void Awake()
    {
        CacheComponent();
        _speed = GetComponent<MonsterMove>();// 임시 BaseEnemy 완성시 변경예정
        goldPool = new ObjectPool<GoldDrop>(_goldPrabas, 20);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }
    }
    public void TakeDamage(float damage)
    {
        if (_isDead == true) return;

        _hp -= damage;
        if(_hp <= 0)
        {
            Debug.Log("죽음");
            _isDead = true;
            goldPool.PopAll();
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
    }
}
