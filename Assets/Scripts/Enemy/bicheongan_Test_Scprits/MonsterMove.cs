using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMove : MonoBehaviour, IMonsterMoveable
{
    //[SerializeField] private WayPointPath _start; //스폰 포인트 참조
    private List<Transform> waypoints;
    private int arrivePoint;
    private NavMeshAgent _agent;
    private BaseEnemy reenemy;

    private bool _turretDis;
    private float originalSpeed;

    private void Awake()
    {
        CacheComponent();
        _agent.speed = reenemy.MonSpeed;
        originalSpeed = _agent.speed;
    }

    public void Reset()
    {
        waypoints = null;
        arrivePoint = 0;
        _agent.speed = reenemy.MonSpeed;
        originalSpeed = _agent.speed;
        _turretDis = false;

        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
    }

    private void Update()
    {
        Move();
    }

    // 경로 정보 받아오기
    public void Initialize(WayPointPath waypoint)
    {
        waypoints = waypoint._waypoints;
        #if UNITY_EDITOR
        Debug.Log("이동");
#endif

        ResetPath(waypoint.transform.position);
        arrivePoint = 0; // 이동해 인덱스 도착시 1+ 더하기
        _agent.SetDestination(waypoint._waypoints[arrivePoint].position);

        _agent.velocity = Vector3.zero;
    }

    public void ResetPath(Vector3 warpPos)
    {
        _agent.ResetPath();
        _agent.Warp(warpPos);
    }


    public void Move()
    {
        if (waypoints == null) return;
        if (waypoints.Count == 0) return;
        if (_agent.pathPending) return;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            arrivePoint++;
            if (_turretDis) return;
            if (arrivePoint >= waypoints.Count)
            {
                // 마지막 Waypoint 도착
                #if UNITY_EDITOR
                Debug.Log("도착");
#endif
                reenemy.ReturnToPool();
                arrivePoint = 0;
                return;
            }
            _agent.SetDestination(waypoints[arrivePoint].position);
        }

    }
    public void Slow(float slowspeed)
    {
        _agent.speed *= (1f - slowspeed);
        #if UNITY_EDITOR
        Debug.Log("느려짐");
#endif
        StartCoroutine(WaitSpeed());
    }
    
    private IEnumerator WaitSpeed()
    {
        yield return new WaitForSeconds(20f);
        _agent.speed = originalSpeed;
        #if UNITY_EDITOR
        Debug.Log("정상 스피드");
#endif
    }
    public void MoveToTurret(Transform turret)
    {
        _turretDis = true;
        _agent.SetDestination(turret.position);
    }

    //터렛에 도착시 이동을 멈춤
    public void MoveStop(Transform turret)
    {
        _agent.SetDestination(turret.position);
        transform.LookAt(turret.position);
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
    }

    //공격 범위에 더텟이 없을시 다시 웨이 포인트로 이동
    public void ReturnMove()
    {
        _turretDis = false;
        _agent.isStopped = false;
        _agent.SetDestination(waypoints[arrivePoint].position);

    }
    private void CacheComponent()
    {
        reenemy = GetComponent<BaseEnemy>();
        _agent = GetComponent<NavMeshAgent>();
    }
}

