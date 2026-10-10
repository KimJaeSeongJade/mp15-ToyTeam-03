using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackMonster : BaseEnemy
{
    [SerializeField] private float _damage;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private LayerMask _TurretMask;

    private MonsterMove _toTarget;
    private Transform _targetTurret;
    private bool _isTurretInSight;
    private bool _isAttacking;

    protected override void Awake()
    {
        base.Awake();
        CaCheComponents();
    }

    private void Update()
    {
        Attack();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_TurretMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (_isTurretInSight == false)
            {
                _targetTurret = other.transform;
                _isTurretInSight = true;
                #if UNITY_EDITOR
                Debug.Log("터렛 발견");
#endif

                _toTarget.MoveToTurret(_targetTurret);
            }
        }

    }

    private void Attack()
    {
        if (!_isTurretInSight) return;


        if (_targetTurret != null)
        {
            _toTarget.MoveStop(_targetTurret);
            if (_isAttacking == false)
            {
                _isAttacking = true;
                #if UNITY_EDITOR
                Debug.Log("공격");
#endif
                IDamageableturret damageable = _targetTurret.GetComponent<IDamageableturret>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_damage);
                    #if UNITY_EDITOR
                    Debug.Log($"{_damage} 데미지 줌");
#endif

                    StartCoroutine(AttackSpeed());
                }
            }
        }
        else
        {
            _isTurretInSight = false;
            _targetTurret = null;
            _toTarget.ReturnMove();;

        }
    }

    private IEnumerator AttackSpeed()
    {
        yield return new WaitForSeconds(_attackSpeed);
        _isAttacking = false;
    }
    protected override void ResetWakeUp()
    {
        base.ResetWakeUp();

        _targetTurret = null;
        _isTurretInSight = false;
        _isAttacking = false;
    }
    protected override void CaCheComponents()
    {
        base.CaCheComponents();
        _toTarget = GetComponent<MonsterMove>();
    }
}
