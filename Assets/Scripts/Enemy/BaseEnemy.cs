using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    private bool _finished;
    public event Action<BaseEnemy> Returned;
    public void ReachGoal()
    {
        if (_finished == true) return;

        _finished = true;

        Returned?.Invoke(this);
        Destroy (gameObject); // 임시로 삭제 구현 오브젝트풀 구현 후 반환으로 변경 예정
    }
}
