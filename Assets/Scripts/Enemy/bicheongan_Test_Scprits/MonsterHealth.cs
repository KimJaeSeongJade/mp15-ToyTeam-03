using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamageableturret, ISloowable ,IHealthSource,IDamageable
{
    [SerializeField] private GoldDrop _goldPrabas;
    private ObjectPool<GoldDrop> goldPool;
    private BaseEnemy Cacheenemy;

    private float goldCount;
    private bool _isDead;
    private bool _isSlow;

    public float CurrentHp { get => Cacheenemy.MonHp; }
    public float MaxHp { get => Cacheenemy.MonMaxHp; }

    public event Action OnHealthChanged;

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
                _goldPrabas.InitData();
            }
            );
    }

    public void TakeDamage(float damage)
    {
        if (_isDead == true) return;
        float mindamage = Mathf.Max(1, damage - Cacheenemy.MonDefend);
        if (damage - Cacheenemy.MonDefend > 0)
        {
            Cacheenemy.MonHp -= (damage - Cacheenemy.MonDefend);
            Debug.Log($"몬스터에게 {damage - Cacheenemy.MonDefend} 데미지를 줌");
        }
        else
        {
            Cacheenemy.MonHp -= mindamage;
            Debug.Log($"몬스터에게 {mindamage} 데미지를 줌");
        }
        Instantiate(Cacheenemy.HitImpact, transform.position, Quaternion.identity);
        if (Cacheenemy.MonHp <= 0)
        {
            Debug.Log("죽음");
            _isDead = true;
            Instantiate(Cacheenemy.IsDeadImpack, transform.position, Quaternion.identity);

            Cacheenemy.GiveToExpPlayer();
            Debug.Log($"Exp{Cacheenemy.MonExp}");

            goldPool.PopAll();
            Cacheenemy.ReturnToPool();

        }
        OnHealthChanged?.Invoke();
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
