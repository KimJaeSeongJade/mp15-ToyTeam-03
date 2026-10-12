using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BossAnim : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private BossMove _move;
    private int _attackNum;
    private bool _isRun;
    private bool _isDead;

    private void Awake()
    {
        CaCheComponent();
    }

    private void Update()
    {
        Walk();
    }

    private void Walk()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool("IsMove", isMoving);
        _animator.SetBool("IsRun", isMoving&& _isRun);
    }
    public void Run()
    {
        _isRun = true;
    }
    public void AttackNum(int num)
    {
        _attackNum = num;
        _animator.SetInteger("AttackNum", num);
    }
    //private void Die()
    //{
    //    _isDead = true;
    //    _agent.isStopped = true;
    //    _animator.SetTrigger("Die");
    //}
    private void CaCheComponent()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _move = GetComponent<BossMove>();
    }
}
