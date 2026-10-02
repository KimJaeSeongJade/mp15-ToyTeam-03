using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    [SerializeField] private WayPointPath _start; //스폰 포인트 참조
    private List<Transform> waypoints;
    private int arrivePoint;
    private float speed = 50f;

    private void Start()
    {
        Initialize(_start);
    }

    private void Update()
    {
        Move();
    }


    // 경로 정보 받아오기
    public void Initialize(WayPointPath waypoint)
    {
        waypoints = waypoint._waypoints;
        arrivePoint = 0; // 이동해 인덱스 도착시 1+ 더하기

    }

    public void Move()
    {

        Transform target = waypoints[arrivePoint];
        Transform LastPoint = waypoints[waypoints.Count - 1];

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (transform.position == target.position && (arrivePoint < waypoints.Count - 1))
        {
            arrivePoint++;
        }
        if (transform.position == LastPoint.position)
        {
            Debug.Log("도착");
            Destroy(gameObject); // 임시
        }
    }
}
