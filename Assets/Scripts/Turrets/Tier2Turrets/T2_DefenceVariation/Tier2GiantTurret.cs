using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2GiantTurret : BaseTurret
{
    [SerializeField] private float _auraRadius = 5f;
    [SerializeField] private LayerMask _ememyLayer;

    [Header("Provoke Stop Settings")]
    [SerializeField] private float _stopDuration = 2f; // 몬스터가 정지해 있을 시간 (초)

    // 인터페이스 타입을 담을 수 있도록 List의 규격을 IDamageable로 관리합니다.
    private readonly List<IDamageable> _provokedTargets = new List<IDamageable>();

    private void Update()
    {
        // 실시간 범위 도발 및 정지 감지
        UpdateProvokeAura();
    }

    private void UpdateProvokeAura()
    {
        // 1. 오브젝트 주변 _auraRadius만큼의 넓이로 레이어마스크로 타겟을 탐지
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, _auraRadius, _ememyLayer);

        foreach (Collider col in hitEnemies)
        {
            // 2. 특정 클래스 대신 IDamageable 인터페이스가 있는지 직접 추출합니다.
            if (col.TryGetComponent(out IDamageable damageable))
            {
                // 3. [중복 방지] 이미 이 터렛에 의해 도발 정지 상태인 적은 패스합니다.
                if (!_provokedTargets.Contains(damageable))
                {
                    // 리스트로 담기
                    _provokedTargets.Add(damageable);

#if UNITY_EDITOR
                    if (damageable is Component enemyComp)
                    {
                        Debug.Log($"<color=cyan>[도발 감지]</color> 범위 내 새로운 적 발견: <b>{enemyComp.name}</b> (현재 묶인 적: {_provokedTargets.Count}마리)");
                    }
#endif

                    // 4. 💡 [계약 반영] 유희찬 개발자님 계약에 맞춰 SlowSpeed(1f)를 전달하여 '완전 감속(정지)' 상태로 만듭니다.
                    damageable.SlowSpeed(1f);

                    // 5. 일정시간 뒤 정지 풀기를 위한 타이머 코루틴 실행
                    StartCoroutine(ReleaseProvokedEnemyRoutine(damageable));
                }
            }
        }
    }

    // 💡 일정시간 뒤 정지 풀기 헬퍼 코루틴
    private IEnumerator ReleaseProvokedEnemyRoutine(IDamageable enemy)
    {
        // 지정된 정지 시간만큼 대기
        yield return new WaitForSeconds(_stopDuration);

        // 6. 💡 [별도 복원 처리] 대기하는 동안 적이 죽어 파괴되었을 수 있으므로 안전하게 null 체크를 진행합니다.
        if (enemy != null && enemy is Component enemyComp)
        {
            // 유희찬님의 MonsterHealth 내부의 _isSlow 변수 가드를 우회하기 위해,
            // 인터페이스에 SlowSpeed(0f)를 전달함과 동시에 MonsterMove 컴포넌트를 직접 찾아 
            // 원래 정상 이속 배율(1f)로 강제 원상복구(Reset) 루틴을 수행해 줍니다.
            enemy.SlowSpeed(0f); // 계약 상 감속 없음 상태 전달

            if (enemyComp.TryGetComponent(out MonsterMove speedControl))
            {
                // 💡 MonsterMove 내부에서 원래 속도로 리셋하거나 덮어씌우는 함수를 다이렉트로 호출합니다.
                // 기존 규칙 상 Slow(1f)가 원상복구이므로 이를 강제로 먹여 멈춤을 풀어줍니다.
                speedControl.Slow(1f); 
            }

#if UNITY_EDITOR
            Debug.Log($"<color=lime>[도발 해제]</color> {_stopDuration}초 경과로 <b>{enemyComp.name}</b> 정지 해제 및 이동 재개");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("<color=orange>[도발 알림]</color> 멈춰있던 적이 정지 시간이 끝나기 전에 사망하여 풀에서 반환됨");
#endif
        }

        // 관리가 끝났으므로 도발 추적 리스트에서 제거해 줍니다.
        _provokedTargets.Remove(enemy);
    }

    // BaseTurret 구조 상 무조건 오버라이드해야 하는 추상 함수
    public override void Attack()
    {
        // 도발 수호형 터렛이므로 순수 공격 기능은 비워둡니다.
    }
}