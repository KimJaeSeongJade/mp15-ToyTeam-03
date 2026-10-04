using System.Collections;
using UnityEngine;

public class GoldDrop : PoolObject
{
    [SerializeField] private int _amount = 100;
    [SerializeField] private float _deliverySpeed = 8f;
    [SerializeField] private float _arrivalDistance = 0.3f;

    private Transform _tr;

    [SerializeField] private WaveManager _waveManager;
    private PlayerWallet _playerWallet;
    private bool _isDelivering;

    public int Amount => _amount;

    private void Awake() => CacheComponenet();
    private void Update() => DeliveryToPlayer();

    // 풀에서 꺼낼 때마다 웨이브 종료 이벤트를 구독한다.
    public override void WakeUp()
    {
        _waveManager.OnWaveEnded += StartDelivery;

        _isDelivering = false;
        gameObject.SetActive(true);
        transform.SetParent(null);
    }

    // 풀로 돌아갈 때 구독과 이동 상태를 정리한다.
    public override void Sleep()
    {
        _waveManager.OnWaveEnded -= StartDelivery;

        _isDelivering = false;
        gameObject.SetActive(false);
        transform.SetParent(_tr);
    }

    // 웨이브 종료 시 플레이어를 찾아 자동 배달을 시작한다.
    public void StartDelivery()
    {
        if (_isDelivering) return;

        if (_playerWallet == null)
        {
            Debug.LogWarning("Player를 찾지 못해 골드를 배달할 수 없습니다.", this);
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
            Debug.LogWarning("배달 중 플레이어 참조가 사라졌습니다.", this);
            return;
        }

        transform.position = 
            Vector3.MoveTowards(transform.position, _playerWallet.transform.position, _deliverySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _playerWallet.transform.position) > _arrivalDistance) return;

        _playerWallet.AddGold(_amount);

        ReturnToPool();
    }

    private void CacheComponenet()
    {
        //StartCoroutine(WaitForGameManager());
    }
    private IEnumerator WaitForGameManager()
    {
        yield return new WaitUntil(() => GameManager.Instance);

        // TODO: 게임매니져에 플레이어 필드 구현시 해제
        // _playerWallet = GameManager.Instance.Player.GetComponent<PlayerWallet>();

        // TODO: 골드 기본값을 매니져에서 참조해 초기화. 구현시해제, 변수명 수정
        // _amount = GameManager.Instance.GOLD_AMOUNT;

        // 웨이브 매니져 싱글턴 변경시 수정
        _waveManager = WaveManager.Instance;
    }

    public void InitDate()
    {
        _tr = transform.parent;

        _waveManager = WaveManager.Instance;

        _playerWallet = GameManager.Instance._playerWallet;
    }
}
