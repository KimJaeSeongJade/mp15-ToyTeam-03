using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private GoldDrop _goldDropPrefab;

    private ObjectPool<GoldDrop> _goldPool;
    private float _health;
    private bool _isDead;

    private void Awake()
    {
        if (_goldDropPrefab != null)
            _goldPool = new ObjectPool<GoldDrop>(_goldDropPrefab, 1);
    }

    private void OnEnable()
    {
        _health = _maxHealth;
        _isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead || damage <= 0f) return;

        _health -= damage;
        Debug.Log($"{gameObject.name} 에게 {damage}만큼 대미지. 남은 체력: {_health}");

        if (_health > 0f) return;
        Die();
    }

    private void Die()
    {
        _isDead = true;

        if (_goldPool != null)
        {
            GoldDrop gold = _goldPool.Pop();
            gold.transform.position = transform.position;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: GoldDrop 프리팹이 연결되지 않아 골드를 드랍하지 못했습니다.", this);
        }

        gameObject.SetActive(false);
    }
}
