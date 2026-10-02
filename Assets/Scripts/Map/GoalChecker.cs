using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalChecker : MonoBehaviour
{
    private bool _isCheck = false;
    private int _count;
    
    public bool IsCheck{ get => _isCheck; } //Ui한테 한 마리당 골로 들어갔다는 알림
    public int Count{ get => _count; }  //Ui에서 이번 라운드 몇마리가 골로 들어갔는지 확인 위한count

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
