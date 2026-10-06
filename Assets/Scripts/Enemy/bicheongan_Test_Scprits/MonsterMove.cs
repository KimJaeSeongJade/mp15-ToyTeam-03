using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterMove : MonoBehaviour
{
    //[SerializeField] private WayPointPath _start; //스폰 포인트 참조
    private List<Transform> waypoints;
    private int arrivePoint;
    private NavMeshAgent _agent;
    private BaseEnemy reenemy;
    //[SerializeField] private float speed = 50f;//BaseEnemy 만들어지면 변경 예정

    private bool _turretDis;
    private float originalSpeed;

    private void Awake()
    {
        CacheComponent();
        _agent.speed = reenemy.MonSpeed;
        originalSpeed = _agent.speed;
    }
    //private void Start()
    //{
    //    //Initialize(_start);
    //}

    private void Update()
    {
        Move();
    }


    // 경로 정보 받아오기
    public void Initialize(WayPointPath waypoint)
    {
        waypoints = waypoint._waypoints;
        Debug.Log("이동");
        arrivePoint = 0; // 이동해 인덱스 도착시 1+ 더하기
        _agent.SetDestination(waypoint._waypoints[arrivePoint].position);

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
                Debug.Log("도착");
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
        Debug.Log("느려짐");
        StartCoroutine(WaitSpeed());
    }
    private IEnumerator WaitSpeed()
    {
        yield return new WaitForSeconds(20f);
        _agent.speed = originalSpeed;
        Debug.Log("정상 스피드");
    }
    public void MoveToTurret(Transform turret)
    {
        _turretDis = true;
        _agent.SetDestination(turret.position);
    }
    private void CacheComponent()
    {
        reenemy = GetComponent<BaseEnemy>();
        _agent = GetComponent<NavMeshAgent>();
    }
}
