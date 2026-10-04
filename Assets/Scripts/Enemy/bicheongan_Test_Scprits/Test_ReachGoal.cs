using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_ReachGoal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BaseEnemy goal = other.GetComponent<BaseEnemy>();

        if (goal != null)
        {
            goal.ReachGoal();
        }
    }
}
