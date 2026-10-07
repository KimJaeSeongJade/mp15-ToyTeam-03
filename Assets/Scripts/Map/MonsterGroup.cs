using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGroup : MonoBehaviour 
{
    
    [System.Serializable]
    public struct MonsterData
    {
        public BaseEnemy monster;
        public int count;
        public int waypointNum;
    }

    public List<MonsterData> monsterDatas;
    private ObjectPool<BaseEnemy> monsterPool;
    private bool _isMake;
    
    private void Awake()
    {
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            monsterPool = new ObjectPool<BaseEnemy>(
                monsterDatas[i].monster,
                monsterDatas[i].count,
                transform,
                _monsterPrabas =>
                {
                    _monsterPrabas.InitData();
                }); 
        }
        
        //monsterDatas = new List<MonsterData>();
        //WaveMonster();
    }

    private void Start()
    {
        if (!_isMake)
        {
            Debug.Log("생성이 안됨");
            return;
        }
        /*while (true)
        {
            for (int i = 0; i < monsterDatas.Count; i++)
            {
                if (monsterDatas[i].waypointNum == 1)
                {
                    Debug.Log($"monsterDatas.Count: {monsterDatas.Count}, " +
                              $"monsterDatas[{i}].count: {monsterDatas[i].count}" +
                              $"monsterDatas[{i}].waypointNum: {monsterDatas[i].waypointNum}");  
                }  
            }
            break;
        }*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            monsterPool.Pop();
        }
    }
}
