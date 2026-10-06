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

    private void Awake()
    {
        monsterDatas = new List<MonsterData>();
        StartWave(2);  //gameManager가 호출 wave번호를 
        //Group();
    }
    
    /*private void Group()
    {
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            Debug.Log(i);
            Debug.Log($"monsterDatas.Count: {monsterDatas.Count} monsterDatas[i].count:" +
                      $"{monsterDatas[i].count} monsterDatas[i].waypointNum: " +
                      $"{monsterDatas[i].waypointNum})");
            }
        } */
    private void StartWave(int num)
    {
        if (num >= 2)
        {
            monsterDatas.Add(new MonsterData
            {
                monster = new GameObject(),
                count = 10,
                waypointNum = 2
            });
        }
        else if (num >= 1)
        {
            monsterDatas.Add(new MonsterData
            {
                monster = new GameObject(),
                count = 5,
                waypointNum = 1
            });
        }
        
    }
    
    
}
