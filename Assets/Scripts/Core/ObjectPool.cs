using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectPool<T> where T : Component, IPoolable
{
    /// <summary> 현재 존재하는 모든 오브젝트 풀 </summary>
    private List<T> objectList;
    /// <summary> 대기중인 오브젝트 풀 </summary>
    private List<T> pool;

    /// <summary> 오브젝트 풀 가져오기 </summary>
    public List<T> ObjectList { get => objectList; }
    /// <summary> 오브젝트 풀 생성시 함수를 실행시킬수 있게 하는 Action 필드</summary>
    private readonly Action<T> onCreate; 

    /// <summary> 풀할 오브젝트 </summary>
    private T gameObject;

    /// <summary> 오브젝트 풀 </summary>
    /// <param name="_gameObject"> 풀링 할 오브젝트 </param>
    /// <param name="_initCount"> 풀링 할 개수 </param>
    /// <param name="parent"> 부모 오브젝트 (없으면 null) </param>
    /// <param name="onCreate"> 오브젝트 생성시 실행할 Action (없으면 null) </param>
    public ObjectPool(T _gameObject, int _initCount, Transform parent = null, Action<T> onCreate = null)
    {
        // 생성 시 전체 오브젝트 풀 리스트
        objectList = new List<T>(_initCount);
        // 생성 시 대기 오브젝트 풀 리스트
        pool = new List<T>(_initCount);
        // 풀링 할 오브젝트
        gameObject = _gameObject;

        // 오브젝트 생성 Action 등록
        this.onCreate = onCreate;

        // 가상 리스트 생성
        var arrObject = new T[_initCount];
        for (int i = 0; i < _initCount; ++i)
        {
            arrObject[i] = CreateObject(parent);
        }

        // 풀 2개에 넣기
        objectList.AddRange(arrObject);
        pool.AddRange(arrObject);
    }

    // 꺼내기
    public T Pop()
    {
        var poolCount = pool.Count;
        T result = null;

        if (poolCount > 0)
        {
            // 대기중인 풀 삭제
            result = pool[poolCount - 1];
            pool.RemoveAt(poolCount - 1);
            result.WakeUp();

            return result;
        }

        // 대기중인 풀이 없다면 만들어서 넣어주기
        result = CreateObject();
        objectList.Add(result);
        result.WakeUp();

        return result;
    }

    // 모두 꺼내기
    public void PopAll()
    {
        foreach (var obj in objectList)
        {
            obj.WakeUp();
        }
    }

    // 넣기
    public void Push(T _object)
    {
        pool.Add(_object);
    }

    // 모든 풀링 넣기
    public void ReturnAll()
    {
        foreach (var obj in objectList)
        {
            if (pool.Contains(obj)) continue;

            obj.Sleep();
            ReturnToPool(obj);
        }
    }

    // 모두 삭제하기
    public void DestroyAll()
    {
        foreach (var obj in objectList)
        {
            Object.Destroy(obj.gameObject);
        }
    }

    // 넣기
    private void ReturnToPool(IPoolable _object)
    {
        Push(_object as T);
    }

    // 생성하기
    protected T CreateObject(Transform parent = null)
    {
        T result = GameObject.Instantiate(gameObject, parent);

        result.gameObject.SetActive(false);
        result.Init(ReturnToPool);

        // 오브젝트 생성시 등록된 Action 실행
        onCreate?.Invoke(result);

        return result;
    }
}
