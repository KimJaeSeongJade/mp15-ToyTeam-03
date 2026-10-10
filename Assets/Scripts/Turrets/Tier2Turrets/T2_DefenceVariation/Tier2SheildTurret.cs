using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2SheildTurret : BaseTurret
{
    [Header("Iron Fortress Settings")]
    [SerializeField] private float _maxShield = 200f;       // 최대 보호막 양
    [SerializeField] private float _shieldRegenSpeed = 20f; // 초당 보호막 재생 수치
    [SerializeField] private float _regenDelay = 3f;        // 마지막 피격 후 재생이 시작될 대기시간(초)

    private float _currentShield;
    private float _lastHitTime;

    protected override void Awake()
    {
        base.Awake();
        _currentShield = _maxShield;
    }

    private void Update()
    {
        // 💡 마지막으로 맞은 지 _regenDelay 초가 지났다면 보호막 서서히 충전
        if (Time.time >= _lastHitTime + _regenDelay)
        {
            if (_currentShield < _maxShield)
            {
                _currentShield += _shieldRegenSpeed * Time.deltaTime;
                _currentShield = Mathf.Min(_currentShield, _maxShield); // 상한선 고정
            }
        }
    }

    // 💡 부모의 TakeDamage를 덮어써서 체력 차감 전 보호막이 대신 흡수하도록 만듭니다.
    public override void TakeDamage(float damage)
    {
        _lastHitTime = Time.time; // 피격 타이머 갱신 (보호막 재생 일시정지)

        if (_currentShield > 0)
        {
            // 1. 데미지가 보호막 잔여량보다 작을 때 -> 보호막만 깎임
            if (_currentShield >= damage)
            {
                _currentShield -= damage;
                #if UNITY_EDITOR
                Debug.Log($"🛡️ [보호막 흡수] 보호막이 대미지를 차단함! 남은 보호막: {_currentShield}");
#endif
                return; // 체력 차감 없이 종료
            }
            // 2. 데미지가 보호막보다 클 때 -> 보호막을 깨트리고 남은 대미지만 계산
            else
            {
                damage -= _currentShield;
                _currentShield = 0;
                #if UNITY_EDITOR
                Debug.Log("💥 [보호막 파괴] 보호막이 깨졌습니다! 남은 대미지가 체력에 들어갑니다.");
#endif
            }
        }

        // 3. 보호막이 없는 상태이거나 뚫고 남은 잔여 대미지는 부모 체력 로직(base.TakeDamage)에 전달
        base.TakeDamage(damage);
        #if UNITY_EDITOR
        Debug.Log($"❤️ [터렛 체력 피격] 남은 체력: {_currentHp} / {_maxHp}");
#endif
    }

    public override void Attack()
    {
        // 순수 방어/도발형 고기방패 터렛이므로 빈칸으로 둡니다.
    }
}
