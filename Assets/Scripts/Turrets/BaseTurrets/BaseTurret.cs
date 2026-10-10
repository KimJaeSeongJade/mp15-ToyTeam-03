using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTurret : MonoBehaviour, IBuildTargetReceiver, IDamageableturret, ITurretInfoSender
{
    [SerializeField] private string _name;
    [SerializeField,TextArea] private string _info;
    public string Name { get => _name;}
    public string Info { get => _info;}
    [SerializeField] private TurretPreview _preview;
    [SerializeField] protected float _maxHp;
    protected float _currentHp;
    [SerializeField] protected int _goldCost;
    [SerializeField] private TurretType _turretType;
    // 💡 [추가] 모든 터렛이 공통으로 사용할 기본 공격력 변수
    [SerializeField] protected float _baseDamage = 10f;
    [SerializeField] private float _buildCooldown = 1f;
    public int GoldCost => _goldCost;
    public float MaxHp => _maxHp;
    public float CurrentHp => _currentHp;
    public int Cost => _goldCost;
    public float BuildCooldown => _buildCooldown;
    public TurretType Type => _turretType;

    // 💡 [추가] 외부(SupportTurret, Bullet 등)에서 공격력을 읽거나 수정할 수 있는 통로(프로퍼티)
    // 이 부분이 있어야 AttackTurret의 43번째 줄 에러가 사라집니다!
    public float Damage 
    { 
        get => _baseDamage; 
        set => _baseDamage = value; 
    }

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

    public void PreviewShow(BaseTurret resultPrefab, bool canBuild, Vector3 position, Quaternion rotation)
    {
        _preview.Show(resultPrefab, position, rotation, canBuild);
    }
    public void PreviewHide()
    {
        _preview.Hide();
    }

    public void GetTurretInfo()
    {
        
    }
}
