using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldObjectPool<T> where T : Component, IPoolable
{
    private List<T> objectList; //현재 존재하는 모든 오브젝트 풀
    private List<T> pool;  // 대기 중인 오브젝트 풀

    public List<T> ObjectList { get { return objectList; } }

    private readonly Action<T> onCreate;

    private T gameObject;

    public GoldObjectPool(T _gameObject, int _InitCount, Transform parent = null, Action<T> onCreate = null)
    {
        objectList = new List<T>(_InitCount);
        pool = new List<T>(_InitCount);
        gameObject = _gameObject;
        this.onCreate = onCreate;

        ////가상 리스트 생성
        //var arrObject = new T[_InitCount];
        //for(int i= 0; i < _InitCount; i++)
        //{
        //    arrObject[i] = CreateObject(parent);
        //}


    }


    //protected T CreateObject(Transform parent = null) 
    //{
    //    T result = GameObject.Instantiate(gameObject);
    //}


}
