using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class BossMove : MonoBehaviour, IMonsterMoveable
{
    [Header("이동속도 퍼센트 예 1.3f면 30% 이동속도 업")]
    [SerializeField] private float _speedPer = 1.3f;
    private List<Transform> waypoints;
    private NavMeshAgent _agent;
    private BaseEnemy reenemy;

    private int arrivePoint;
    private bool _turretDis;
    private bool _turretIngnore;
    private float originalSpeed;
    private float bossUpSpeed;

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

        //arrivePoint = 0;
        //_agent.speed = reenemy.MonSpeed;
        //originalSpeed = _agent.speed;
        //_turretDis = false;
        //_agent.isStopped = false;

    }
    private void Update()
    {
        Move();
    }


    // 경로 정보 받아오기
    public void Initialize(WayPointPath waypoint)
    {
        if (waypoint == null) return;
        if (waypoint._waypoints == null) return;
        if (waypoint._waypoints.Count == 0) return;
        if (_turretDis) return;

        waypoints = waypoint._waypoints;
        Debug.Log("이동");
        arrivePoint = 0; // 이동해 인덱스 도착시 1+ 더하기
        _agent.SetDestination(waypoint._waypoints[arrivePoint].position);

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
        //if (_turretDis) return;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            arrivePoint++;
            if (arrivePoint >= waypoints.Count)
            {
                // 마지막 Waypoint 도착
                Debug.Log("도착");
                //reenemy.ReturnToPool();
                arrivePoint = 0;
                return;
            }
            _agent.SetDestination(waypoints[arrivePoint].position);
        }

    }
    public void Slow(float slowspeed)
    {
        _agent.speed *= (1f - slowspeed);
        Debug.Log("느려짐");
        StartCoroutine(WaitSpeed());
    }

    private IEnumerator WaitSpeed()
    {
        yield return new WaitForSeconds(20f);
        if (originalSpeed > bossUpSpeed)
        {
            _agent.speed = originalSpeed;
        }
        else
        {
            _agent.speed = bossUpSpeed;
        }
        Debug.Log("정상 스피드");
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

    //공격 범위에 터텟이 없을시 다시 웨이 포인트로 이동
    public void ReturnMove()
    {
        _turretDis = false;
        _agent.isStopped = false;
        _agent.SetDestination(waypoints[arrivePoint].position);

    }
    //터렛 무시하고 이동
    public void TurretIngnore()
    {
        _turretIngnore = true;
        _turretDis = false;
        _agent.isStopped = false;
        _agent.SetDestination(waypoints[arrivePoint].position);
        Debug.Log("터렛 무시 발동");

    }
    // 이동속도 증가
    public void UpSpeed()
    {
        bossUpSpeed = originalSpeed * _speedPer;
        _agent.speed = bossUpSpeed;
    }
    private void CacheComponent()
    {
        reenemy = GetComponent<BaseEnemy>();
        _agent = GetComponent<NavMeshAgent>();
    }
}
