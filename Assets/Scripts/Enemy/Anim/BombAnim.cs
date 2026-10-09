using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BombAnim : MonoBehaviour
{
    [SerializeField] private LayerMask _TurretMask;
    private NavMeshAgent _agent;
    private Animator _animator;
    private BaseEnemy _return;
    private bool IsSight;

    private bool _isBomb;


    private void Awake()
    {
        CaCheCompoents();
    }

    private void Update()
    {
        Walk();
    }

    private void Walk()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool("IsMove", isMoving);
        BombAttack();
    }

    private void BombAttack()
    {
        if (IsSight == true && !_isBomb)
        {
            _isBomb = true;
            _animator.SetBool("IsBomb", true);


        }
    }

    private void Return() // 공격 모션 후 애니메이션 클립에서 이벤트로 작동
    {
        _return.ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_TurretMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (IsSight == false)
            {
                IsSight = true;
                Debug.Log("터렛 발견");
            }
        }
    }
    public void ResetBomb()
    {
        IsSight = false;
        _isBomb = false;

        _animator.SetBool("IsBomb", false);
        _animator.SetBool("IsMove", false);
    }
    private void OnEnable()
    {
        IsSight = false;
        _isBomb = false;

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        if (_animator != null)
        {

            _animator.SetBool("IsBomb", false);
            _animator.SetBool("IsMove", false);
        }
    }

    private void CaCheCompoents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _return = GetComponent<BaseEnemy>();
    }
}