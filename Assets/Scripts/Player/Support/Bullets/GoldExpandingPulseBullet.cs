using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public abstract class GoldExpandingPulseBullet : Tier2BaseBullet
{
    [SerializeField, Min(0.01f)] private float _duration = 0.5f;
    [SerializeField] private Transform _waveVisual;
    private SphereCollider _trigger;
    private float _elapsed;
    private float _radius;
    private bool _running;
    private ParticleSystem[] _visualParticles;
    private Renderer[] _visualRenderers;

    protected virtual void Awake()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        if (_waveVisual != null)
        {
            _visualParticles = _waveVisual.GetComponentsInChildren<ParticleSystem>(true);
            _visualRenderers = _waveVisual.GetComponentsInChildren<Renderer>(true);
            // 풀링되는 시각 오브젝트가 파티클 종료 시 스스로 파괴되지 않게 한다.
            foreach (ParticleSystem particle in _visualParticles)
            {
                var main = particle.main;
                main.stopAction = ParticleSystemStopAction.None;
            }
        }
    }
    protected void BeginPulse(Vector3 position, float radius)
    {
        transform.position = position;
        _radius = radius;
        _elapsed = 0f;
        _running = true;
        SetRadius(0.001f);
    }
    private void FixedUpdate()
    {
        if (!_running) return;
        // 최대 반경으로 물리 감지가 한 번 진행된 뒤 풀에 반환한다.
        if (_elapsed >= _duration) { ReturnToPool(); return; }
        _elapsed += Time.fixedDeltaTime;
        SetRadius(_radius * Mathf.Clamp01(_elapsed / _duration));
    }
    private void SetRadius(float radius)
    {
        _trigger.radius = Mathf.Max(0.001f, radius);
        if (_waveVisual != null) _waveVisual.localScale = Vector3.one * radius * 2f;
    }
    private void OnTriggerEnter(Collider other) { if (_running) Detect(other); }
    private void OnTriggerStay(Collider other) { if (_running) Detect(other); }
    protected abstract void Detect(Collider other);
    protected void SetVisualVisible(bool visible)
    {
        if (_waveVisual == null) return;
        // 시각 루트만 끈다. 발사체 루트의 물리 판정은 계속 동작한다.
        if (_waveVisual != transform) _waveVisual.gameObject.SetActive(visible);
        else foreach (Renderer renderer in _visualRenderers) renderer.enabled = visible;

        foreach (ParticleSystem particle in _visualParticles)
        {
            particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (visible && particle.gameObject.activeInHierarchy) particle.Play(false);
        }
    }
    protected override void OnFireEnd()
    {
        _running = false;
        base.OnFireEnd();
    }
}
