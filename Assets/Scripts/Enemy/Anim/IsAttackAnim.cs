using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IsAttackAnim : MonoBehaviour
{
    [SerializeField] private LayerMask _TurretMask;
    private NavMeshAgent _agent;
    private Animator _animator;
    private bool IsSight;
    private bool _isAttack;
    private Transform _target;

    private void Awake()
    {
        CaCheCompoents();
    }

    private void Update()
    {
        if(_target == null && IsSight == true)
        {
            IsSight = false;
            _isAttack = false;
            _animator.SetBool("IsAttack", false);
        }
        Walk();
    }

    private void Walk()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool("IsMove", isMoving);
        if (_target != null && _agent.velocity.sqrMagnitude == 0)
        {
            Attack();
        }
        else if(_target == null)
        {
            _isAttack = false;
            _animator.SetBool("IsAttack", false);
        }
    }

    private void Attack()
    {

        if (IsSight == true && !_isAttack)
        {
            _isAttack = true;
            _animator.SetBool("IsAttack", _isAttack);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if ((_TurretMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (IsSight == false)
            {
                IsSight = true;
                _target = other.transform;
                Debug.Log("터렛 발견");
            }
        }
    }
    private void OnEnable()
    {
        IsSight = false;
        _isAttack = false;
        _target = null;

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        if (_animator != null)
        {
            _animator.SetBool("IsMove", false);
            _animator.SetBool("IsAttack", false);
        }
    }

    private void CaCheCompoents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }
}
