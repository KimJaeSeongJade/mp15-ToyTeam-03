using System.Collections.Generic;
using UnityEngine;

public class GoldShockwaveBullet : GoldExpandingPulseBullet
{
    private readonly HashSet<MonsterHealth> _hit = new HashSet<MonsterHealth>();

    public void Fire(Vector3 position)
    {
        _hit.Clear();
        BeginPulse(position, _range);
    }
    protected override void Detect(Collider other)
    {
        if ((_damagableMask.value & (1 << other.gameObject.layer)) == 0) return;
        MonsterHealth health = other.GetComponentInParent<MonsterHealth>();
        if (health != null && health.isActiveAndEnabled && _hit.Add(health)) Hit(health);
    }
    protected override void OnFireEnd()
    {
        _hit.Clear();
        base.OnFireEnd();
    }
}
