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
    private readonly Dictionary<T, List<T>> poolsByPrefab;
    private readonly Dictionary<T, T> prefabByObject;
    private readonly Transform poolParent;

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

    // 여러 프리팹을 한 풀에서 관리한다. 같은 프리팹은 한 번만 등록한다.
    public ObjectPool(IEnumerable<(T prefab, int count)> entries, Transform parent = null, Action<T> onCreate = null)
    {
        objectList = new List<T>();
        pool = new List<T>();
        poolsByPrefab = new Dictionary<T, List<T>>();
        prefabByObject = new Dictionary<T, T>();
        poolParent = parent;
        this.onCreate = onCreate;

        foreach (var entry in entries)
        {
            if (!poolsByPrefab.TryGetValue(entry.prefab, out List<T> available))
            {
                available = new List<T>(entry.count);
                poolsByPrefab.Add(entry.prefab, available);
            }

            for (int i = 0; i < entry.count; i++)
            {
                T created = CreateObject(entry.prefab, parent);
                objectList.Add(created);
                available.Add(created);
            }
        }
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

    // 여러 프리팹을 등록한 풀에서는 꺼낼 종류를 지정한다.
    public T Pop(T prefab)
    {
        if (!poolsByPrefab.TryGetValue(prefab, out List<T> available))
        {
            available = new List<T>();
            poolsByPrefab.Add(prefab, available);
        }
        T result;

        if (available.Count > 0)
        {
            int last = available.Count - 1;
            result = available[last];
            available.RemoveAt(last);
        }
        else
        {
            result = CreateObject(prefab, poolParent);
            objectList.Add(result);
        }

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
        T returned = _object as T;
        if (poolsByPrefab == null)
            Push(returned);
        else
            poolsByPrefab[prefabByObject[returned]].Add(returned);
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

    private T CreateObject(T prefab, Transform parent)
    {
        T result = GameObject.Instantiate(prefab, parent);
        result.gameObject.SetActive(false);
        result.Init(ReturnToPool);
        prefabByObject.Add(result, prefab);
        onCreate?.Invoke(result);
        return result;
    }
}
