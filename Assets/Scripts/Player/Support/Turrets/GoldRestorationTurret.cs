using UnityEngine;

public class GoldRestorationTurret : GoldEffectTurret
{
    [SerializeField] private CastleHp _castle;

    [SerializeField, Min(0f)] private float _healAmount = 20f;

    [SerializeField, Min(1)] private int _uses = 1;

    [SerializeField, Min(0f)] private float _destroyDelay = 1f;

    [SerializeField] private GameObject _readyVisual;

    private WaveManager _waveManager;

    protected override void Start()
    {
        // TODO: GameManager에 캐슬 참조가 제공되면 해당 참조를 받아 사용하도록 변경한다.
        // 현재는 인스펙터 미지정 시 Start에서 한 번 검색하는 임시 처리다.
        if (_castle == null) _castle = FindObjectOfType<CastleHp>();

        _waveManager = WaveManager.Instance;

        if (_waveManager != null) _waveManager.OnWaveEnded += OnWaveEnded;

        if (_readyVisual != null) _readyVisual.SetActive(true);
    }

    protected override void Update() { }

    public override void Attack() { }

    private void OnWaveEnded()
    {
        if (!isActiveAndEnabled || _uses <= 0 || _castle == null) return;

        ActivateEffect();
    }

    protected override void ActivateEffect()
    {
        _castle.Heal(_healAmount);

        NotifyEffect();

        if (--_uses > 0) return;

        if (_readyVisual != null) _readyVisual.SetActive(false);

        Destroy(gameObject, _destroyDelay);
    }

    private void OnDestroy()
    {
        if (_waveManager != null) _waveManager.OnWaveEnded -= OnWaveEnded;
    }
}
