using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 본게임 MonsterMove를 수정하지 않고 튜토리얼 몬스터의 종료/캐슬 도착을 관리한다.
public class TutorialEnemyRoute : MonoBehaviour
{
    public bool Arrived { get; private set; }
    public bool Failed { get; private set; }
    private NavMeshAgent _agent;
    private BaseEnemy _enemy;
    private CastleHp _castle;
    private readonly List<Vector3> _points = new List<Vector3>();
    private int _index;
    private bool _holdAtGoal;

    public bool Initialize(BaseEnemy enemy, IList<Vector3> requested, CastleHp castle, bool holdAtGoal)
    {
        _enemy = enemy;
        _agent = enemy.GetComponent<NavMeshAgent>();
        _castle = castle;
        _holdAtGoal = holdAtGoal;
        Vector3 previous = enemy.transform.position;
        foreach (Vector3 point in requested)
        {
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 5f, _agent.areaMask)) return Fail();
            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(previous, hit.position, _agent.areaMask, path)
                || path.status != NavMeshPathStatus.PathComplete) return Fail();
            _points.Add(hit.position);
            previous = hit.position;
        }
        if (_points.Count == 0) return Fail();
        _agent.isStopped = false;
        _agent.SetDestination(_points[0]);
        return true;
    }

    private bool Fail()
    {
        Failed = true;
        Debug.LogError("TutorialEnemyRoute: 등장 지점, 건설 구역, 캐슬 사이의 NavMesh 경로를 확인하세요.", this);
        return false;
    }

    private void Update()
    {
        if (Failed || Arrived || _points.Count == 0 || !_agent.isOnNavMesh || _agent.pathPending) return;
        if (_agent.hasPath && _agent.pathStatus != NavMeshPathStatus.PathComplete) { Fail(); return; }
        if (Vector3.Distance(transform.position, _points[_index]) > Mathf.Max(0.5f, _agent.stoppingDistance)) return;
        _index++;
        if (_index < _points.Count) { _agent.SetDestination(_points[_index]); return; }
        Arrived = true;
        _agent.isStopped = true;
        if (!_holdAtGoal) ResolveGoal();
    }

    public void ResolveGoal()
    {
        if (_enemy == null || !_enemy.gameObject.activeInHierarchy || _enemy.MonHp <= 0f) return;
        _castle.CastleTakeDamage(_enemy.MonCastleDam);
        if (_enemy.HitImpact != null) Instantiate(_enemy.HitImpact, transform.position, Quaternion.identity);
        _enemy.ReachGoal();
        _enemy.ReturnToPool();
    }
}
