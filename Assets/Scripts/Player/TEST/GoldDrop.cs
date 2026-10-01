using UnityEngine;

public class GoldDrop : PoolObject
{
    [SerializeField] private int _cost = 100;
    [SerializeField] private float _deliverySpeed = 8f;
    [SerializeField] private float _arrivalDistance = 0.3f;

    private WaveManager _waveManager;
    private Transform _player;
    private bool _isDelivering;

    // 풀에서 꺼낼 때마다 웨이브 종료 이벤트를 구독한다.
    public override void WakeUp()
    {
        _isDelivering = false;
        _player = null;
        gameObject.SetActive(true);

        _waveManager = WaveManager.Instance;
        if (_waveManager != null)
            _waveManager.OnWaveEnded += StartDelivery;
        else
            Debug.LogWarning("WaveManager를 찾지 못해 GoldDrop 자동 배달을 시작할 수 없습니다.", this);
    }

    // 풀로 돌아갈 때 구독과 이동 상태를 정리한다.
    public override void Sleep()
    {
        if (_waveManager != null)
            _waveManager.OnWaveEnded -= StartDelivery;

        _waveManager = null;
        _player = null;
        _isDelivering = false;
        gameObject.SetActive(false);
    }

    // 웨이브 종료 시 플레이어를 찾아 자동 배달을 시작한다.
    // TODO: WaveManager가 없는 테스트 씬에서는 플레이 모드의 컴포넌트 메뉴에서도 호출할 수 있다. 추후 제거
    [ContextMenu("Test Start Delivery")]
    public void StartDelivery()
    {
        if (_isDelivering) return;

        // TODO: 플레이어 참조를 직접 전달받도록 바꾼다. 현재는 테스트를 위해 태그로 찾는다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("Player 태그가 붙은 오브젝트를 찾지 못해 골드를 배달할 수 없습니다.", this);
            return;
        }

        _player = playerObject.transform;
        _isDelivering = true;
    }

    private void Update()
    {
        if (!_isDelivering) return;

        if (_player == null)
        {
            _isDelivering = false;
            Debug.LogWarning("배달 중 플레이어 참조가 사라졌습니다.", this);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _player.position,
            _deliverySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _player.position) > _arrivalDistance) return;

        _player.GetComponent<PlayerWallet>().AddGold(_cost);
        ReturnToPool();
    }

    private void OnDestroy()
    {
        if (_waveManager != null)
            _waveManager.OnWaveEnded -= StartDelivery;
    }
}
