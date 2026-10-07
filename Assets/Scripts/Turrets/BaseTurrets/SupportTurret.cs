using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportTurret : BaseTurret
{
    [SerializeField] private float _buffDamage;
    [SerializeField] private float _buffRange; // 필요 시 콜라이더 반지름 설정용
    [SerializeField] private LayerMask _turretMask;
    
    [Header("코루틴 버프 설정")]
    [SerializeField] private float _buffDuration = 3f;  // 버프가 지속되는 시간
    [SerializeField] private float _buffCooldown = 5f;  // 버프를 다시 주기까지의 쿨타임

    // 범위 내에 들어와 있는 타워들을 관리하는 리스트 (주석 1번)
    private List<IBuffable> _detectedTurrets = new List<IBuffable>();

    protected override void Awake()
    {
        base.Awake();
        // 1-3. 주기적으로 버프가 작동하도록 코루틴을 시작합니다.
        StartCoroutine(BuffCycleRoutine());
    }

    // 1. 트리거로 어택 타워 감지
    private void OnTriggerEnter(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            // 1-1. 인터페이스(IBuffable)를 가지고 있는지 검사하여 감지
            if (other.TryGetComponent<IBuffable>(out IBuffable buffableTarget))
            {
                if (!_detectedTurrets.Contains(buffableTarget))
                {
                    _detectedTurrets.Add(buffableTarget);
                }
            }
        }
    }

    // 공격 대상 범위 벗어남 처리 (리스트에서 제거)
    private void OnTriggerExit(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            if (other.TryGetComponent<IBuffable>(out IBuffable buffableTarget))
            {
                if (_detectedTurrets.Contains(buffableTarget))
                {
                    _detectedTurrets.Remove(buffableTarget);
                }
            }
        }
    }

    private bool IsTargetLayer(int layer)
    {
        return (_turretMask.value & (1 << layer)) != 0;
    }

    // 1-3. 주기적으로 버프가 들어갔다가 나오게 처리하는 코루틴 체인
    private System.Collections.IEnumerator BuffCycleRoutine()
    {
        while (true)
        {
            // 버프 작동 시점
            DamageBuff(true);

            // 1-4. 코루틴으로 일정 시간(지속 시간) 동안 대기
            yield return new WaitForSeconds(_buffDuration);

            // 버프 준 만큼 다시 빼기 시점
            DamageBuff(false);

            // 다음 버프를 주기 전까지의 쿨타임 대기
            yield return new WaitForSeconds(_buffCooldown - _buffDuration);
        }
    }

    // 1-2. 공격력 버프를 따로 처리할 함수
    private void DamageBuff(bool isGivingBuff)
    {
        // 리스트를 돌면서 현재 범위 내에 있는 모든 타워에게 인터페이스 함수 호출
        for (int i = _detectedTurrets.Count - 1; i >= 0; i--)
        {
            IBuffable turret = _detectedTurrets[i];

            // 혹시 그 사이에 타워가 파괴되었을 경우 리스트에서 예외 처리
            if (turret == null || (turret as MonoBehaviour) == null)
            {
                _detectedTurrets.RemoveAt(i);
                continue;
            }

            // 버프를 주는 타이밍이면 +, 빼는 타이밍이면 - 수치를 넘겨줌
            float amount = isGivingBuff ? _buffDamage : -_buffDamage;
            turret.ApplyDamageBuff(amount);
        }
    }

    // 서포트 타워 자체가 파괴될 때 혹시 적용 중이던 버프가 남아있다면 깔끔하게 회수
    private void OnDestroy()
    {
        StopAllCoroutines();
        DamageBuff(false);
    }
}