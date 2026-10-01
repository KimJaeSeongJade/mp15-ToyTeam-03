using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalChecker : MonoBehaviour
{
    private bool _isCheck = false;
    
    public bool IsCheck{ get => _isCheck; }
    private int _count;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Monster"))
        {
            _isCheck = true;
            Destroy(other.gameObject);
            _count++;
        }
        Debug.Log(_count);
    }
}
