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

    private int _listNum;
    private int _listCount;

    public IEnumerable<(BaseEnemy Prefab, int Count)> GetInitMonsterDataPools() 
    {
        foreach (MonsterData monsterData in monsterDatas)
        {
            yield return (monsterData.monster, monsterData.count);
        }
    }

    public void ResetSpwan()
    {
        _listNum = 0;
        _listCount = 0;
    }

    public bool TryGetNext(out MonsterData resultData)
    {
        while (_listNum < monsterDatas.Count)
        {
            resultData = monsterDatas[_listNum];
            if (_listCount < resultData.count)
            {
                _listCount++;
                return true;
            }
            _listNum++;
            _listCount = 0;
        }
        
        resultData = default;
        return false;
    }


    /*private ObjectPool<BaseEnemy> monsterPool;
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
        }#1#
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            monsterPool.Pop();
        }
    }*/
}
