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

            _animator.SetBool("IsBomb", true);

            StartCoroutine(Delay());

        }
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1f);
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

    private void CaCheCompoents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _return = GetComponent<BaseEnemy>();
    }
}