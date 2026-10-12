using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackToSkyTurret : BaseTurret 
{ 
    [SerializeField] private float _attackRange; 
    [SerializeField] private LayerMask _targetMask; 
    [SerializeField] private Transform _muzzlePoint; 
    [SerializeField] private GameObject _bullets; 
    [SerializeField] private float _bulletCoolTime; 
    
    private int _maxHp; 
    private int _goldCost; 

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
        if (other.GetComponentInParent<IAirMonster>() == null) return;
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

        // 💡 지대공(공중 공격)을 위해 Y축(높이) 제한을 제거하고 몬스터의 실제 위치를 바라봅니다.
        // 포탑 전체가 아닌 포신(Muzzle 또는 Head)만 회전시키고 싶다면 transform 대신 해당 파츠의 Transform을 LookAt 해야 합니다.
        transform.LookAt(_target.position); 
    } 
}
