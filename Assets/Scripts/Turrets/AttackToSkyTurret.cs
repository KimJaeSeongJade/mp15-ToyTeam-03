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
    private float _lastAttackTime; 

    private void Update() 
    { 
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
        if (_bullets != null && _muzzlePoint != null) 
        { 
            Instantiate(_bullets, _muzzlePoint.position, _muzzlePoint.rotation); 
        } 
    } 

    // 공격 대상 감지 
    private void OnTriggerEnter(Collider other) 
    { 
        if (IsTargetLayer(other.gameObject.layer)) 
        { 
            // 💡 오브젝트가 공중 몬스터 인터페이스(IAirMonster)를 가지고 있는지 검사
            if (other.TryGetComponent<IAirMonster>(out IAirMonster airMonster))
            {
                if (_target == null) 
                { 
                    _target = other.transform; 
                } 
            }
        } 
    } 

    // 공격 대상 범위 벗어남 
    private void OnTriggerExit(Collider other) 
    { 
        if (IsTargetLayer(other.gameObject.layer)) 
        { 
            // 벗어난 오브젝트가 현재 타겟인 경우에만 해제 
            if (_target != null && _target == other.transform) 
            { 
                _target = null; 
            } 
        } 
    } 

    // 레이어마스크 검사 공통 함수 
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
