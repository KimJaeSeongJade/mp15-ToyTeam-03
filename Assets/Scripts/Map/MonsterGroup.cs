using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGroup : MonoBehaviour 
{

    [System.Serializable]
    public struct MonsterData
    {
        public GameObject monster;
        public int count;
        public int waypointNum;
    }

    public List<MonsterData> monsterDatas;
    private bool _isMake;
    
    private void Awake()
    {
        monsterDatas = new List<MonsterData>();
        WaveMonster();
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
    

    private void WaveMonster()
    {
        monsterDatas.Add(new MonsterData
        {
            monster = new GameObject(),
            count = 3,
            waypointNum = 1
        });
        monsterDatas.Add(new MonsterData
        {
            monster = new GameObject(),
            count = 4,
            waypointNum = 1
        });
        monsterDatas.Add(new MonsterData
        {
            monster = new GameObject(),
            count = 5,
            waypointNum = 2
        });
        monsterDatas.Add(new MonsterData
        {
            monster = new GameObject(),
            count = 6,
            waypointNum = 2
        });
        _isMake = true;
    }
}
