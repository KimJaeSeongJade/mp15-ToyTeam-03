using System.Collections.Generic;
using UnityEngine;

public class GoldShockwaveBullet : GoldExpandingPulseBullet
{
    [SerializeField, Min(0)] private int _maxVisibleWaves = 6;
    [SerializeField, Min(0f)] private float _visualGroupDistance = 2f;
    [SerializeField, Min(0.01f)] private float _visualDuration = 0.25f;
    private static readonly List<GoldShockwaveBullet> _visibleWaves = new List<GoldShockwaveBullet>();
    private readonly HashSet<MonsterHealth> _hit = new HashSet<MonsterHealth>();
    private bool _visualVisible;
    private float _hideVisualAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetVisibleWaves() => _visibleWaves.Clear();

    public void Fire(Vector3 position)
    {
        _hit.Clear();
        BeginPulse(position, _range);
        _visualVisible = CanShowVisual(position);
        SetVisualVisible(_visualVisible);
        if (_visualVisible)
        {
            _visibleWaves.Add(this);
            _hideVisualAt = Time.time + _visualDuration;
        }
    }
    private bool CanShowVisual(Vector3 position)
    {
        _visibleWaves.RemoveAll(wave => wave == null || !wave.isActiveAndEnabled || !wave._visualVisible);
        if (_visibleWaves.Count >= _maxVisibleWaves) return false;
        foreach (GoldShockwaveBullet wave in _visibleWaves)
        {
            Vector3 difference = wave.transform.position - position;
            difference.y = 0f;
            if (difference.sqrMagnitude < _visualGroupDistance * _visualGroupDistance) return false;
        }
        return true;
    }
    private void Update()
    {
        if (_visualVisible && Time.time >= _hideVisualAt) HideVisual();
    }
    private void HideVisual()
    {
        _visualVisible = false;
        _visibleWaves.Remove(this);
        SetVisualVisible(false);
    }
    protected override void Detect(Collider other)
    {
        if ((_damagableMask.value & (1 << other.gameObject.layer)) == 0) return;
        MonsterHealth health = other.GetComponentInParent<MonsterHealth>();
        if (health != null && health.isActiveAndEnabled && _hit.Add(health)) Hit(health);
    }
    protected override void OnFireEnd()
    {
        HideVisual();
        _hit.Clear();
        base.OnFireEnd();
    }
    private void OnDisable()
    {
        _visualVisible = false;
        _visibleWaves.Remove(this);
    }
}
