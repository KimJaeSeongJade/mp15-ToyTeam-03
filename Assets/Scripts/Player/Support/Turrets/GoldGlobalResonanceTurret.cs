using UnityEngine;

public class GoldGlobalResonanceTurret : GoldEffectTurret
{
    [SerializeField] private GoldMonsterDetector _detector;
    [SerializeField, Min(1f)] private float _receivedDamageMultiplier = 1.5f;
    [SerializeField, Min(0.1f)] private float _vulnerabilityDuration = 4f;
    protected override void ActivateEffect()
    {
        if (_detector == null) return;
        foreach (MonsterHealth health in _detector.GetMonsters())
        {
            if (health.isActiveAndEnabled) health.ApplyVulnerability(_receivedDamageMultiplier, _vulnerabilityDuration);
        }
        NotifyEffect();
    }
}
