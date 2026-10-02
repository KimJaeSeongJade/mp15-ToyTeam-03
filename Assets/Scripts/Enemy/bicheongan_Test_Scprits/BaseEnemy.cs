using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : PoolObject
{
    public event Action<BaseEnemy> onRemoved;
    public override void WakeUp()
    {
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        onRemoved?.Invoke(this);
        gameObject.SetActive(false);
    }

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
