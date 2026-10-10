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
    private readonly List<IBuffable> _detectedTurrets = new List<IBuffable>();

    // 실제로 버프를 부여받아 대미지가 올라가 있는 대상 목록
    private readonly List<IBuffable> _buffedTurrets = new List<IBuffable>();

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

    // 💡 [요청 사항 반영 1] 공격 대상 범위 벗어남 처리 시 즉시 버프를 안전하게 회수합니다.
    private void OnTriggerExit(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            if (other.TryGetComponent<IBuffable>(out IBuffable buffableTarget))
            {
                // 감지 리스트에서 먼저 제외
                if (_detectedTurrets.Contains(buffableTarget))
                {
                    _detectedTurrets.Remove(buffableTarget);
                }
                
                // 💡 [핵심] 만약 이 타워가 실제로 버프를 받아 대미지가 늘어난 상태라면,
                // 3초 쿨타임이 끝날 때까지 기다리지 않고 이탈하는 순간 즉시 버프를 회수합니다!
                if (_buffedTurrets.Contains(buffableTarget))
                {
                    // 버프를 받은 대상에게만 안전하게 차감 처리 (중복 차감 원천 방지)
                    buffableTarget.ApplyDamageBuff(-_buffDamage);
                    _buffedTurrets.Remove(buffableTarget);
                    
                    Debug.Log($"↩️ [범위 이탈 회수] {(buffableTarget as MonoBehaviour).name}이 범위를 벗어나 공격력 버프를 즉시 회수했습니다.");
                }
            }
        }
    }

    private bool IsTargetLayer(int layer)
    {
        return (_turretMask.value & (1 << layer)) != 0;
    }

    // 1-3. 주기적으로 버프가 들어갔다가 나오게 처리하는 코루틴 체인
    private IEnumerator BuffCycleRoutine()
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
        if (isGivingBuff)
        {
            for (int i = _detectedTurrets.Count - 1; i >= 0; i--)
            {
                IBuffable turret = _detectedTurrets[i];

                if (turret == null || (turret as MonoBehaviour) == null)
                {
                    _detectedTurrets.RemoveAt(i);
                    continue;
                }

                // 💡 중복 지급 방지 가드: 이미 버프를 받고 있다면 패스
                if (!_buffedTurrets.Contains(turret))
                {
                    turret.ApplyDamageBuff(_buffDamage);
                    _buffedTurrets.Add(turret);
                }
            }
        }
        else
        {
            // 정주기 버프 해제 시점
            for (int i = _buffedTurrets.Count - 1; i >= 0; i--)
            {
                IBuffable turret = _buffedTurrets[i];

                if (turret == null || (turret as MonoBehaviour) == null)
                {
                    _buffedTurrets.RemoveAt(i);
                    continue;
                }

                // 💡 실제로 버프를 쥐고 있는 대상 목록에 존재할 때만 안전하게 차감
                turret.ApplyDamageBuff(-_buffDamage);
                Debug.Log($"↩️ [정기 버프 회수] {(turret as MonoBehaviour).name}의 공격력 버프 {_buffDamage}를 회수했습니다.");
            }

            _buffedTurrets.Clear();
        }
    }

    // 💡 [요청 사항 반영 2] 서포트 타워 자체가 파괴될 때 아직 회수 안 된 버프가 있다면 리셋하고 소멸합니다.
    private void OnDestroy()
    {
        StopAllCoroutines();
        
        // 💡 코루틴 사이클 내부의 해제를 호출하기보다, OnDestroy 시점에 살아남은 
        // 찌꺼기 버프 리스트만 타겟팅하여 단발성으로 안전하게 영수증 정산을 해버립니다.
        for (int i = _buffedTurrets.Count - 1; i >= 0; i--)
        {
            IBuffable turret = _buffedTurrets[i];

            if (turret != null && (turret as MonoBehaviour) != null)
            {
                // 버프가 실존하는 대상만 마이너스하므로 마이너스 스탯 버그가 발생하지 않습니다.
                turret.ApplyDamageBuff(-_buffDamage);
                Debug.Log($"💥 [지원터렛 파괴] 파괴 디버프로 인해 {(turret as MonoBehaviour).name}의 공격력 버프를 긴급 회수했습니다.");
            }
        }

        _detectedTurrets.Clear();
        _buffedTurrets.Clear();
    }
}