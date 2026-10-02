using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class PoolObject : MonoBehaviour, IPoolable
{
    private Action<IPoolable> returnToPool;

    public void Init(Action<IPoolable> _returnToPool)
    {
        returnToPool = _returnToPool;
    }

    public abstract void WakeUp();
    public abstract void Sleep();

    public void ReturnToPool()
    {
        Sleep();
        returnToPool?.Invoke(this);
    }

}

public interface IPoolable
{
    /// <summary>
    /// 생성 후 최초로 1회 호출
    /// </summary>
    public void Init(Action<IPoolable> _returnToPool);
    /// <summary>
    /// 풀에서 빠져나올때 호출
    /// </summary>
    public void WakeUp();
    /// <summary>
    /// 풀에 집어넣을때 호출
    /// </summary>
    public void Sleep();
}
