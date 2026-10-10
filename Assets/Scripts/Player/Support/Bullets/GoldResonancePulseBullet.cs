using System;
using System.Collections.Generic;
using UnityEngine;

public class GoldResonancePulseBullet : GoldExpandingPulseBullet
{
    [SerializeField] private LayerMask _goldMask = ~0;
    private readonly HashSet<GoldDrop> _detected = new HashSet<GoldDrop>();
    private Action<Vector3> _onGoldReached;

    public void Fire(Vector3 position, float radius, Action<Vector3> onGoldReached)
    {
        _detected.Clear();
        _onGoldReached = onGoldReached;
        BeginPulse(position, radius);
        SetVisualVisible(true);
    }
    protected override void Detect(Collider other)
    {
        if ((_goldMask.value & (1 << other.gameObject.layer)) == 0) return;
        GoldDrop drop = other.GetComponentInParent<GoldDrop>();
        if (drop != null && drop.IsAvailableForResonance && _detected.Add(drop))
            _onGoldReached?.Invoke(drop.transform.position);
    }
    protected override void OnFireEnd()
    {
        _onGoldReached = null;
        _detected.Clear();
        SetVisualVisible(false);
        base.OnFireEnd();
    }
}
