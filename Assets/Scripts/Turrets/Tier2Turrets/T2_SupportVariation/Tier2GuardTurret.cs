using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2GuardTurret : BaseTurret
{
    [Header("Guard Defense Settings (방어력 누적)")]
    [SerializeField] private float _baseDefense = 5f;        // 기본 방어력
    [SerializeField] private float _defBonusPerHit = 2f;     // 피격당 누적되는 추가 방어력
    [SerializeField] private float _maxDefenseBonus = 30f;    // 💡 방어력 증가 최대 상한선
    [SerializeField] private float _decayDelay = 3.0f;       // 이 시간(초) 동안 피격받지 않으면 방어력이 식기 시작함
    [SerializeField] private float _decaySpeed = 10f;        // 방어력이 감소할 때 초당 줄어드는 속도

    private float _currentDefenseBonus = 0f; // 현재 누적된 추가 방어력 수치
    private float _lastHitTime;              // 마지막으로 피격당한 시간
    
    private void Update()
    {
        // 💡 [방어력 복구] 피격 이후 _decayDelay 초 동안 공격을 받지 않으면 방어력 보너스가 원래대로 돌아갑니다.
        if (Time.time >= _lastHitTime + _decayDelay)
        {
            if (_currentDefenseBonus > 0f)
            {
                _currentDefenseBonus -= _decaySpeed * Time.deltaTime;
                _currentDefenseBonus = Mathf.Max(_currentDefenseBonus, 0f); // 0 아래로 떨어지지 않게 고정
            }
        }
    }

    // 💡 부모의 TakeDamage를 오버라이드하여 대미지 경감 및 예열 스택 처리를 전담합니다.
    public override void TakeDamage(float damage)
    {
        // 1. 피격 타이머 갱신 (방어력 감소 루틴 일시정지)
        _lastHitTime = Time.time;

        // 2. [대미지 경감 수식] 들어온 대미지에서 (기본 방어력 + 누적 추가 방어력)을 빼줍니다.
        float totalDefense = _baseDefense + _currentDefenseBonus;
        float finalDamage = damage - totalDefense;

        // 방어력이 너무 높아 대미지가 음수(-)나 0이 되는 것을 방지하기 위해 최소 1의 피해는 입도록 가드합니다.
        finalDamage = Mathf.Max(finalDamage, 1f);

        // 3. 부모 클래스가 가지고 있던 체력 차감 공식 수동 적용
        _currentHp -= finalDamage;
        _currentHp = Mathf.Clamp(_currentHp, 0, _maxHp);

        #if UNITY_EDITOR
        Debug.Log($"🛡️ [{gameObject.name}] 피격! 원래 대미지: {damage} -> 방어 차감 대미지: {finalDamage} (현재 총 방어력: {totalDefense}) / 남은 HP: {_currentHp}");
#endif

        if (_currentHp <= 0)
        {
            Die();
            return; // 터졌다면 스택을 쌓지 않고 조기 종료
        }

        // 4. 💡 [데미지 받을 때마다 방어력 증가] 한 대 맞았으므로 스택 축적
        if (_currentDefenseBonus < _maxDefenseBonus)
        {
            _currentDefenseBonus += _defBonusPerHit;
            // 💡 설정한 방어력 상한선을 넘지 않도록 가드 고정
            _currentDefenseBonus = Mathf.Min(_currentDefenseBonus, _maxDefenseBonus);
        }
    }

    public override void Attack()
    {
        // 최전방 전선에서 두들겨 맞으며 버티는 탱커형 포탑이므로 기본 사격 기능은 비워둡니다.
    }
}
