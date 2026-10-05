using System.Collections.Generic;
using UnityEngine;

public class ChargeAttack : BasicAttack, IChargeable
{
    [SerializeField] private float _maxChargeTime = 2f;
    [SerializeField] private float _minimumDamage = 10f;
    [SerializeField] private float _maximumDamage = 30f;

    private float _chargeStartTime;
    private bool _charging;
    private Transform _muzzle;

    HashSet<IDamageable> damaged = new HashSet<IDamageable>();

    public bool IsCharging => _charging;

    public override void WakeUp()
    {
        // BasicAttack.WakeUp이 부모에서 분리하므로 발사 지점을 먼저 보관한다.
        _muzzle = transform.parent;
        base.WakeUp();

        // 차지 중 이동은 GetNextPosition에서 막고 Update는 입력 해제를 감지한다.
        transform.SetParent(_muzzle, false);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _chargeStartTime = Time.time;
        _charging = true;
    }

    private void Update() => CheckRelease();

    protected override Vector3 GetNextPosition(float deltaTime) => transform.position;

    public void CheckRelease()
    {
        if (!_charging || !Input.GetMouseButtonUp(0)) return;

        ReleaseCharge();
    }

    public void ReleaseCharge()
    {
        if (!_charging) return;

        damaged.Clear();

        float chargeRatio = Mathf.Clamp01((Time.time - _chargeStartTime) / _maxChargeTime);
        float damageRatio = Mathf.Lerp(_minimumDamage, _maximumDamage, chargeRatio);

        Vector3 start = _muzzle.position;
        Vector3 forward = _muzzle.forward;
        Vector3 end = start + forward * _range;

        Collider[] hits = Physics.OverlapCapsule(start, end, _detectRadius, _damagableMask);

        foreach (Collider collider in hits)
        {
            IDamageable target = collider.GetComponent<IDamageable>();
            if (target != null && damaged.Add(target))
                target.TakeDamage(damageRatio);
        }

        ReturnToPool();
    }

    // 건설 모드 전환 등으로 차지를 취소할 때 사용한다.
    public void CancelCharge()
    {
        if (_charging) ReturnToPool();
    }

    public override void Sleep()
    {
        _charging = false;
        base.Sleep();
    }
}
