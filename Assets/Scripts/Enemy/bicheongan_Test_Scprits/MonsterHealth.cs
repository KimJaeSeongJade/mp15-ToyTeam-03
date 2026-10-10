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
    private float _vulnerabilityMultiplier = 1f;
    private float _vulnerabilityUntil;

    public void ApplyVulnerability(float multiplier, float duration)
    {
        if (Time.time >= _vulnerabilityUntil) _vulnerabilityMultiplier = 1f;
        _vulnerabilityMultiplier = Mathf.Max(_vulnerabilityMultiplier, multiplier);
        _vulnerabilityUntil = Mathf.Max(_vulnerabilityUntil, Time.time + duration);
    }

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
        float receivedDamage = Mathf.Max(1, damage - Cacheenemy.MonDefend);
        if (Time.time < _vulnerabilityUntil) receivedDamage *= _vulnerabilityMultiplier;
        Cacheenemy.MonHp -= receivedDamage;
        #if UNITY_EDITOR
        Debug.Log($"몬스터에게 {receivedDamage} 데미지를 줌");
#endif
        Instantiate(Cacheenemy.HitImpact, transform.position, Quaternion.identity);
        if (Cacheenemy.MonHp <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("죽음");
#endif
            _isDead = true;
            Instantiate(Cacheenemy.IsDeadImpack, transform.position, Quaternion.identity);

            Cacheenemy.GiveToExpPlayer();
<<<<<<< Updated upstream
=======
            //#if UNITY_EDITOR
            //Debug.Log($"Exp{Cacheenemy.MonExp}");
            //#endif
>>>>>>> Stashed changes

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

    public bool CheckExecution(float thresholdRatio)
    {
        if (CurrentHp / MaxHp <= thresholdRatio)
        {
            Debug.Log("처형 가능");
            
            TakeDamage(MaxHp);
            return true;
        }

        return false;
    }

    public void ResetHealth()
    {
        _isDead = false;
        _isSlow = false;
        _vulnerabilityMultiplier = 1f;
        _vulnerabilityUntil = 0f;
    }
    private void CacheComponent()
    {
        Cacheenemy = GetComponent<BaseEnemy>();
        _speed = GetComponent<MonsterMove>();
        goldCount = Cacheenemy.MonGold / GameManager.GOLD_AMOUNT;
    }

}
