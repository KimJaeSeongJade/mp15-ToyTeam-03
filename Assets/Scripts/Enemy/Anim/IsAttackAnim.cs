using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IsAttackAnim : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;

    private void Awake()
    {
        CaCheCompoents();
    }

    private void Update()
    {
        Walk();
        Attack();
    }

    private void Walk()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool("IsMove", isMoving);
    }

    private void Attack()
    {
        bool isAttack = _agent.velocity.sqrMagnitude == 0f;
        _animator.SetBool("IsAttack", isAttack);
    }



    private void CaCheCompoents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }
}
