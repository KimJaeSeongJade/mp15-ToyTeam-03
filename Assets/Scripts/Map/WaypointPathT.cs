using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPathT : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoint; 
    public Transform[] Waypoint { get => _waypoint; } 
    
}
