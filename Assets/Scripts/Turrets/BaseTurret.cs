using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTurret : MonoBehaviour, IBuildTargetReceiver
{
    [SerializeField] protected float _maxHp;
    protected float _currentHp;
    [SerializeField] protected int _goldCost;
    [SerializeField] private TurretType _turretType;

    [SerializeField] private TurretPreview _preview;


    public float MaxHp => _maxHp;
    public float CurrentHp => _currentHp;
    public int Cost => _goldCost;
    public TurretType Type => _turretType;

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

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void PreviewShow(BaseTurret resultPrefab, bool canBuild, Vector3 position, Quaternion rotation)
    {
        _preview.Show(resultPrefab, position, rotation, canBuild);
    }

    public void PreviewHide()
    {
        _preview.Hide();
    }
}
