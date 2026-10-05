using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMonster : BaseEnemy
{
    [SerializeField] private float _damage;
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private float _AttackSpeed;
    [SerializeField] private LayerMask _TurretMask;

    private MonsterMove _toTarget;
    private Transform _targetTurret;
    private bool _isTurretInSight;

    private void Awake()
    {
        CacheComponet();
    }

    private void Update()
    {
        Attack();
    }

    private void OnTriggerEnter(Collider other) // 범위는 임시 후 변경 예정
    {
        if ((_TurretMask.value & (1 << other.gameObject.layer)) != 0)
        {
            _targetTurret = other.transform;
            Debug.Log("터렛 발견");
            _isTurretInSight = true;
            _toTarget.MoveToTurret(_targetTurret);
        }

    }

    private void Attack()
    {
        if (!_isTurretInSight) return;

        Vector3 targetdir = (_targetTurret.position - transform.position).normalized;

        Ray ray = new Ray(transform.position, targetdir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, _attackRange, _TurretMask))
        {
            Debug.Log("공격");
            StartCoroutine(AttackSpeed());
        }
    }

    private IEnumerator AttackSpeed()
    {
        yield return new WaitForSeconds(_AttackSpeed);
    }

    private void CacheComponet()
    {
        _toTarget = GetComponent<MonsterMove>();
    }
}
