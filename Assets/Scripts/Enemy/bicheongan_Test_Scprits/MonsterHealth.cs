using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageable
{
    //[SerializeField] private float _hp; // 변경예정
    [SerializeField] private GoldDrop _goldPrabas;
    private ObjectPool<GoldDrop> goldPool;
    private BaseEnemy Cacheenemy;

    private float goldCount;
    private bool _isDead;
    private bool _isSlow;

    private MonsterMove _speed;

    private void Awake()
    {
        CacheComponent();
        goldPool = new ObjectPool<GoldDrop>(
            _goldPrabas,
            (int)goldCount,
            transform,
            _goldPrabas =>
            {
                _goldPrabas.InitDate();
            }
            );
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

        Cacheenemy.MonHp -= (damage - Cacheenemy.MonDefend);
        Debug.Log($"몬스터에게 {damage - Cacheenemy.MonDefend} 데미지를 줌");
        if (Cacheenemy.MonHp <= 0)
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
        _speed = GetComponent<MonsterMove>();
        goldCount = Cacheenemy.MonGold / GameManager.GOLD_AMOUNT;
    }
}
