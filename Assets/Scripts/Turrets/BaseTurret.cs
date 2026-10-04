using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTurret : MonoBehaviour
{
    [SerializeField] protected float _maxHp;
    protected float _currentHp;
    [SerializeField] protected int _goldCost;
    [SerializeField] private TurretType _turretType;

    public float MaxHp => _maxHp;
    public float CurrentHp => _currentHp;
    public int Cost => _goldCost;
    public TurretType Type => _turretType;

    // 게임 시작 시 현재 체력을 최대 체력으로 채워줍니다.
    protected virtual void Awake()
    {
        _currentHp = _maxHp;
    }

    public virtual void Attack()
    {
        
    }
    
    public virtual void TakeDamage(float damage)
    {
        _currentHp -= damage;
        _currentHp = Mathf.Clamp(_currentHp, 0, _maxHp);

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        Destroy(gameObject);
    }
}
