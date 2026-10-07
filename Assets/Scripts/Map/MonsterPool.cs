using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class MonsterPool : MonoBehaviour
{
    [SerializeField] private BaseEnemy _monsterPrabas;
    [SerializeField] private MonsterGroup  _monsterGroup;
    
    private ObjectPool<BaseEnemy> monsterPool;

    
    private int _count = 10;

    private void Awake()
    {
        monsterPool = new ObjectPool<BaseEnemy>(
            _monsterPrabas,
            (int)_count,
            _monsterGroup.transform,
            _monsterPrabas =>
            {
                _monsterPrabas.InitData();
            });
    }
    
    public BaseEnemy Pop()
    { 
        return monsterPool.Pop();
    }

   
}
