using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2BufferHealTurret : BaseTurret
{
    [Header("Aura Settings (범위)")]
    [SerializeField] private float _buffRadius = 6f;         // 버프 및 힐이 도달할 사거리 (반지름)

    [Header("Damage Buff Settings (공격력 증가)")]
    [SerializeField] private float _damageBuffAmount = 5f;    // 주변 터렛에게 올려줄 고정 공격력 수치

    [Header("Heal Settings (체력 회복)")]
    [SerializeField] private float _healAmount = 25f;        // 틱당 아군 터렛 회복량
    [SerializeField] private float _healInterval = 3f;       // 힐이 들어갈 주기 (초)

    private float _nextHealTime;
    // 버프를 주고 있는 터렛들을 기억하여 중복 증가 및 해제를 관리하는 리스트
    private readonly List<BaseTurret> _buffedTurrets = new List<BaseTurret>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // 1. 실시간 주변 아군 터렛 탐색 및 공격력 버프 토템 상시 가동
        UpdateBuffAura();

        // 2. 일정 시간(_healInterval)마다 범위 내 터렛 체력 회복
        if (Time.time >= _nextHealTime)
        {
            ApplyAreaHeal();
            _nextHealTime = Time.time + _healInterval;
        }
    }

    // 💡 [공격력 버프 메커니즘] 범위 내 아군을 찾아서 부모(BaseTurret)에 기입된 Damage 수치를 올립니다.
    private void UpdateBuffAura()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _buffRadius);
        List<BaseTurret> currentInZone = new List<BaseTurret>();

        foreach (Collider col in hitColliders)
        {
            // 자신을 제외한 주변의 다른 아군 터렛 컴포넌트를 탐색합니다.
            if (col.gameObject != this.gameObject && col.TryGetComponent(out BaseTurret targetTurret))
            {
                currentInZone.Add(targetTurret);

                // 새롭게 범위에 들어온 터렛이 있다면 공격력 프로퍼티 수정하여 버프 부여
                if (!_buffedTurrets.Contains(targetTurret))
                {
                    targetTurret.Damage += _damageBuffAmount;
                    _buffedTurrets.Add(targetTurret);
                    Debug.Log($"⚔️ [공격력 버프] {targetTurret.name}의 공격력이 {_damageBuffAmount}만큼 증가했습니다.");
                }
            }
        }

        // 💡 [버프 해제 가드] 버프를 받던 타워가 범위를 벗어나거나 철거되었다면 원래대로 복구
        for (int i = _buffedTurrets.Count - 1; i >= 0; i--)
        {
            if (_buffedTurrets[i] == null || !currentInZone.Contains(_buffedTurrets[i]))
            {
                if (_buffedTurrets[i] != null)
                {
                    _buffedTurrets[i].Damage -= _damageBuffAmount;
                    Debug.Log($"↩️ [버프 해제] {_buffedTurrets[i].name}이 범위를 벗어나 공격력이 원상복구되었습니다.");
                }
                _buffedTurrets.RemoveAt(i);
            }
        }
    }

    // 💡 [체력 회복 메커니즘] 범위 내 터렛들의 체력을 안전하게 복구합니다.
    private void ApplyAreaHeal()
    {
        if (_buffedTurrets.Count == 0) return;

        Debug.Log($"💚 [{gameObject.name}] 광역 회복 발동! 주변 아군 터렛 치유 중...");

        for (int i = 0; i < _buffedTurrets.Count; i++)
        {
            BaseTurret target = _buffedTurrets[i];

            if (target != null)
            {
                // 부모 클래스가 열어준 public 프로퍼티나 함수 인터페이스가 없으므로, 
                // 음수(-) 데미지를 넘겨주어 부모의 TakeDamage 내부 계산식을 통해 체력을 보충 우회합니다.
                // (만약 부모에 public void Heal(float amount)가 따로 있다면 그걸로 바꾸셔도 좋습니다)
                target.TakeDamage(-_healAmount);
            }
        }
    }

    // 💡 이 터렛이 철거되거나 파괴될 때 주변 버프를 완전히 끄고 퇴근하도록 정리
    private void OnDisable()
    {
        foreach (BaseTurret turret in _buffedTurrets)
        {
            if (turret != null)
            {
                turret.Damage -= _damageBuffAmount;
            }
        }
        _buffedTurrets.Clear();
    }

    // BaseTurret 구조 상 무조건 오버라이드해야 하는 공격 함수
    public override void Attack()
    {
        // 아군을 보조하는 순수 유틸리티 지원형 터렛이므로 발사 기능은 비워둡니다.
    }

    // 인스펙터 창에서 초록색 수호 범위를 그려주는 디버깅용 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _buffRadius);
    }
}
