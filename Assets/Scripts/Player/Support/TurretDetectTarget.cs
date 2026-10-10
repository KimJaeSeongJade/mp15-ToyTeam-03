using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TurretDetectTarget : MonoBehaviour
{
    [SerializeField] private Tier2BaseTurret _baseTurret;
    [SerializeField] private LayerMask _targetMask;

    // 💡 [핵심] 범위 내에 들어와 있는 적 유닛 후보들을 관리할 리스트
    private readonly List<Transform> _targetCandidates = new List<Transform>();

    private void Update()
    {
        // 💡 매 프레임 현재 타깃의 상태를 감시하고, 유효하지 않다면 다음 타깃으로 교체합니다.
        CheckAndRefreshTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            Transform newEnemy = other.transform;

            // 중복 추가 방지 가드 후 후보 리스트에 등록
            if (!_targetCandidates.Contains(newEnemy))
            {
                _targetCandidates.Add(newEnemy);
            }

            // 만약 터렛이 현재 아무도 조준하고 있지 않다면, 방금 들어온 적을 즉시 타깃으로 설정
            if (_baseTurret.Target == null)
            {
                _baseTurret.Target = newEnemy;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            Transform exitingEnemy = other.transform;

            // 1. 범위에서 완전히 이탈했으므로 후보 리스트에서 제거
            if (_targetCandidates.Contains(exitingEnemy))
            {
                _targetCandidates.Remove(exitingEnemy);
            }

            // 2. 만약 나가버린 적이 현재 터렛이 조준하고 있던 메인 타깃이었다면 조준 해제
            if (_baseTurret.Target == exitingEnemy)
            {
                _baseTurret.Target = null;
                
                // 3. 리스트에 남은 다른 대기 후보들 중에서 새로운 타깃 선별 시도
                SelectNextAvailableTarget();
            }
        }
    }

    /// <summary>
    /// 💡 현재 타깃이 사망했거나, 비활성화(오브젝트 풀 반환)되었는지 실시간으로 검사하고 갱신하는 헬퍼 함수
    /// </summary>
    private void CheckAndRefreshTarget()
    {
        // 현재 타깃이 존재하는데, 그 적이 죽어서 사라졌거나(null) 풀링으로 인해 비활성화(False) 되었다면
        if (_baseTurret.Target != null)
        {
            if (!_baseTurret.Target.gameObject.activeInHierarchy)
            {
                // 후보 리스트에서도 안전하게 제거
                _targetCandidates.Remove(_baseTurret.Target);
                _baseTurret.Target = null;
            }
        }

        // 현재 조준 중인 타깃이 비어있게 되었다면 다음 후보군 소환
        if (_baseTurret.Target == null)
        {
            SelectNextAvailableTarget();
        }
    }

    /// <summary>
    /// 💡 리스트 후보군 중 '살아있고 유효한 적'을 순차적으로 검색해 타깃으로 낙점하는 연산 함수
    /// </summary>
    private void SelectNextAvailableTarget()
    {
        // 리스트 뒤쪽에서부터 역순으로 정리하면서 유효한 적을 탐색 (안전한 리스트 삭제를 위함)
        for (int i = _targetCandidates.Count - 1; i >= 0; i--)
        {
            Transform candidate = _targetCandidates[i];

            // 1. 대기하는 사이에 파괴되었거나 오브젝트 풀로 돌아가 꺼진 적이 있다면 리스트에서 청소
            if (candidate == null || !candidate.gameObject.activeInHierarchy)
            {
                _targetCandidates.RemoveAt(i);
                continue;
            }

            // 2. 정상적으로 살아있고 활성화된 적을 발견하면 즉시 메인 타깃으로 변경 후 탈출
            _baseTurret.Target = candidate;
            return;
        }

        // 모든 후보 전수조사 후에도 유효한 적이 전혀 없다면 최종 null 유지
        _baseTurret.Target = null;
    }

    private bool IsTargetLayer(int layer)
    {
        return (_targetMask.value & (1 << layer)) != 0;
    }

    // 터렛이 씬에서 꺼지거나 파괴될 때 리스트 깔끔하게 비워주기
    private void OnDisable()
    {
        _targetCandidates.Clear();
    }
}
