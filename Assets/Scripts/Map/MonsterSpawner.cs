using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private WayPointPath _waypointPath;
    [SerializeField] private int _count;
    [SerializeField] private BaseEnemy _monsterPrefab;

    private readonly WaitForSeconds _wait = new WaitForSeconds(2f);
    private int _correntCount;
    
    public void SpawnWave(int waveNumber, Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        StartCoroutine(WaveStartRoutine(onMonsterSpawn, onSpawnEnd));
    }
    
    private IEnumerator WaveStartRoutine(Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        _correntCount = 0;

        while (_count > _correntCount)
        {
            yield return _wait;
            Debug.Log("생성");
            BaseEnemy _monster = Instantiate(_monsterPrefab, transform.position, transform.rotation);
            // 몬스터한테 waypointPath posititon전달
            _monster.GetComponent<MonsterMove>().Initialize(_waypointPath);
            onMonsterSpawn?.Invoke(_monster); //몬스터 생성값을 어떻게 할 것인지 waveManager&monster상의
            _correntCount++;
        }
        
        Debug.Log("생성 끝");
        onSpawnEnd?.Invoke();
    }
}
