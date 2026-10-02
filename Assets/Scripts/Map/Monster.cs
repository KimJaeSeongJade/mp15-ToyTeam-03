using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster: MonoBehaviour
{
    [SerializeField] private WaypointPath _waypoint;
    [SerializeField] private float _speed = 3f;

    private int _currentIndex = 0;

//외부에서 waypointPath값을 받아와서 저장
    public void SetWaypoint(WaypointPath waypoint) 
    {
        _waypoint = waypoint;
    }


    private void Update()
    {
        if (_waypoint == null) return;
    
        Transform target = _waypoint.Waypoint[_currentIndex];
    
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            _speed * Time.deltaTime);

        if (transform.position == target.position)
        {
            _currentIndex++;
            if (_currentIndex >= _waypoint.Waypoint.Length)
            {
                _currentIndex = 0;
            }
        }
    }
}
