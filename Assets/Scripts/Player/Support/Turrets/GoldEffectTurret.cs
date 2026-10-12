using UnityEngine;
using UnityEngine.Events;

// 별도의 Start로 공격용 부모 풀 초기화를 대체한다.
public abstract class GoldEffectTurret : Tier2BaseTurret
{
    [SerializeField, Min(0.1f)] protected float _effectInterval = 5f;
    [SerializeField] private ParticleSystem _activationEffect;
    [SerializeField] private UnityEvent _onEffect = new UnityEvent();
    private float _nextEffectTime;

    protected virtual void Start() => _nextEffectTime = Time.time + _effectInterval;

    protected virtual void Update() => Attack();

    public override void Attack()
    {
        if (Time.time < _nextEffectTime) return;
        _nextEffectTime = Time.time + _effectInterval;
        ActivateEffect();
    }

    protected abstract void ActivateEffect();

    protected void NotifyEffect()
    {
        if (_activationEffect != null) _activationEffect.Play(true);
        _onEffect.Invoke();
    }
}
