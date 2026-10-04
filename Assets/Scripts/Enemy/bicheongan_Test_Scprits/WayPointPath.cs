using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointPath : MonoBehaviour
{
    [SerializeField] private List<Transform> givewaypoints;

    public List<Transform> _waypoints 
    {
        get
        {
           return givewaypoints;
        }
    }

}
