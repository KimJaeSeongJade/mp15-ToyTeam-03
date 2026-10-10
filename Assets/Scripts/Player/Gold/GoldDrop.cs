using System.Collections;
using UnityEngine;

public class GoldDrop : PoolObject
{
    [SerializeField] private int _amount;
    [SerializeField] private float _deliverySpeed = 8f;
    [SerializeField] private float _arrivalDistance = 0.3f;
    [SerializeField] private Transform _character;
    [SerializeField] private float _dropRadius = 1.5f;
    [SerializeField] private float _dropTime = 0.5f;
    [SerializeField] private float _height = 1.5f;

    private Transform _tr;
    private Vector3 _InitPos;
    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _elapsedTime;

    private WaveManager _waveManager;
    private PlayerWallet _playerWallet;
    private bool _isDelivering;
    private bool _isDropping;
    public bool IsAvailableForResonance => isActiveAndEnabled && !_isDelivering && !_isDropping;

    private void Update()
    {
        if (_isDropping)
            RandomDrop();
        else
            DeliveryToPlayer();
    }

    // 풀에서 꺼낼 때마다 웨이브 종료 이벤트를 구독한다.
    public override void WakeUp()
    {
        if (_waveManager != null) _waveManager.OnWaveEnded += StartDelivery;

        _isDelivering = false;
        gameObject.SetActive(true);
        transform.SetParent(null);

        _startPos = transform.position;
        Vector2 offset = Random.insideUnitCircle * _dropRadius;
        _endPos = _startPos + new Vector3(offset.x, 0f, offset.y);
        _elapsedTime = 0f;
        _isDropping = true;
        _character.localPosition = _InitPos;
    }

    // 풀로 돌아갈 때 구독과 이동 상태를 정리한다.
    public override void Sleep()
    {
        if (_waveManager != null) _waveManager.OnWaveEnded -= StartDelivery;

        _isDelivering = false;
        _isDropping = false;
        _character.localPosition = _InitPos;
        gameObject.SetActive(false);
        transform.SetParent(_tr, false);
        transform.localPosition = Vector3.zero;
    }

    private void RandomDrop()
    {
        _elapsedTime += Time.deltaTime;

        float progress = Mathf.Clamp01(_elapsedTime / _dropTime);

        transform.position = Vector3.Lerp(_startPos, _endPos, progress);
        _character.localPosition = _InitPos + Vector3.up * (4f * _height * progress * (1f - progress));

        if (progress >= 1f)
            _isDropping = false;
    }

    // 웨이브 종료 시 플레이어를 찾아 자동 배달을 시작한다.
    public void StartDelivery()
    {
        if (_isDelivering) return;

        if (_playerWallet == null)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("Player를 찾지 못해 골드를 배달할 수 없습니다.", this);
#endif
            return;
        }

        _isDelivering = true;
    }

    private void DeliveryToPlayer()
    {
        if (!_isDelivering) return;

        if (_playerWallet == null)
        {
            _isDelivering = false;
            #if UNITY_EDITOR
            Debug.LogWarning("배달 중 플레이어 참조가 사라졌습니다.", this);
#endif
            return;
        }

        transform.position = 
            Vector3.MoveTowards(transform.position, _playerWallet.transform.position, _deliverySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _playerWallet.transform.position) > _arrivalDistance) return;

        _playerWallet.AddGold(_amount);

        ReturnToPool();
    }

    public void InitData()
    {
        _tr = transform.parent;
        _InitPos = _character.localPosition;

        _waveManager = WaveManager.Instance;


        _amount = GameManager.GOLD_AMOUNT;

        _playerWallet = GameManager.Instance.PlayerStatus.GetComponent<PlayerWallet>();
    }

    private void OnDestroy()
    {
        if (_waveManager != null) _waveManager.OnWaveEnded -= StartDelivery;
    }
}
