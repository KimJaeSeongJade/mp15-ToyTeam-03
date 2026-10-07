using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private WayPointPath[] _waypointPath;
    
    //[SerializeField] private int _count;
    [SerializeField] private BaseEnemy _monsterPrefab;
    [SerializeField] private MonsterGroup monsterGroup1;
    private readonly WaitForSeconds _wait = new WaitForSeconds(2f);
    private int _correntCount;
    private BaseEnemy _monster;
    

    private void Start()
    {
        SpawnWave(2,null,null);
    }
    
    public void SpawnWave(int waveNumber, Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {        
        StartCoroutine(WaveStartRoutine(waveNumber,onMonsterSpawn, onSpawnEnd));
    }
    
    private IEnumerator WaveStartRoutine(int waveNumber,Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        while (true)
        {
            Debug.Log("몬스터 생성");
            for (int i = 0; i < monsterGroup1.monsterDatas.Count; i++)
            {
                if (monsterGroup1.monsterDatas[i].waypointNum == waveNumber)
                {
                    for (int j = 0; j < monsterGroup1.monsterDatas[i].count; j++)
                    {
                        _monster = Instantiate(_monsterPrefab, 
                            _waypointPath[waveNumber-1].transform.position, transform.rotation);
                        _monster.GetComponent<MonsterMove>().Initialize(_waypointPath[waveNumber-1]);
                        onMonsterSpawn?.Invoke(_monster);
                        yield return _wait;
                    }
                } 
            }
            
            break;
        }
        Debug.Log("생성 끝");
        onSpawnEnd?.Invoke();
        
        /*while (_count > _correntCount) //while문을 통해서 waypoint num이 1이면  waypointnum 1인 몬스터만 생성
        {
            Debug.Log("생성");
            BaseEnemy _monster = Instantiate(_monsterPrefab, transform.position, transform.rotation);
            // 몬스터한테 waypointPath posititon전달
            _monster.GetComponent<MonsterMove>().Initialize(_waypointPath);
            onMonsterSpawn?.Invoke(_monster); //몬스터 생성값을 어떻게 할 것인지 waveManager&monster상의
            _correntCount++;

            yield return _wait;
        }*/
    }
}
