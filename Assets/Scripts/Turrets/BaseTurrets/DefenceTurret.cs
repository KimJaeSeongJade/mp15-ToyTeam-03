using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceTurret : BaseTurret
{
    [SerializeField] private float _defenceRange;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private GameObject _bullets;
    [SerializeField] private float _bulletCoolTime;
    [SerializeField] private float _shield;
    
    [SerializeField] private Transform _target;
    private readonly TurretTargetList _targets = new TurretTargetList();
    private float _lastAttackTime;

    private void Update()
    {
        _target = _targets.GetTarget(_target);
        if (_target == null) return;

        // 쿨타임 체크 후 공격
        if (Time.time >= _lastAttackTime + _bulletCoolTime)
        {
            Attack();
            _lastAttackTime = Time.time;
        }
        TurretRotate();
    }
    public override void Attack()
    {
        _target = _targets.GetTarget(_target);
        if (_target == null) return;

        if (_bullets != null && _muzzlePoint != null)
        {
            Instantiate(_bullets, _muzzlePoint.position, _muzzlePoint.rotation);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterTarget(other);
    }

    private void OnTriggerStay(Collider other)
    {
        RegisterTarget(other);
    }

    private void RegisterTarget(Collider other)
    {
        if (!IsTargetLayer(other.gameObject.layer)) return;
        _targets.Register(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _targets.Remove(other);
    }

    private void OnDisable()
    {
        _targets.Clear();
        _target = null;
    }

    private bool IsTargetLayer(int layer)
    {
        return (_targetMask.value & (1 << layer)) != 0;
    }

    private void TurretRotate()
    {
        if (_target == null) return;

        // 몬스터의 위치 중 Y축(높이)만 내 높이와 맞춰서 윗쪽/아랫쪽으로 기우는 것을 방지
        Vector3 targetPosition = new Vector3(_target.position.x, transform.position.y, _target.position.z);

        // 몬스터 위치를 바라보도록 회전
        transform.LookAt(targetPosition);
    }

    public override void TakeDamage(float damage)
    {
        // 1단계: 쉴드를 반영한 데미지 계산 (음수 방지)
        float finalDamage = Mathf.Max(0, damage - _shield);

        // 2단계: 계산된 최종 데미지를 차감하고 Clamp 처리
        _currentHp -= finalDamage;
        _currentHp = Mathf.Clamp(_currentHp, 0, _maxHp);
        
        #if UNITY_EDITOR
        Debug.Log($"{finalDamage}");
#endif

        // 3단계: 사망 조건 검사
        if (_currentHp <= 0)
        {
            Die();
        }
    }
}
