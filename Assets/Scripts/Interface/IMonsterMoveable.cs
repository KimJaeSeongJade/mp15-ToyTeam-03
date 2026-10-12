using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMonsterMoveable
{
    public void Initialize(WayPointPath waypointPath ){ }
    
    public void ResetPath(Vector3 position) { }
}
