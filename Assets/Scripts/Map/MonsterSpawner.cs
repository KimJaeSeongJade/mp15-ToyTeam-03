using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private WaypointPath _waypointPath;
    [SerializeField] private int _count;
    [SerializeField] private GameObject _monsterPrefab;
    
    
    private readonly WaitForSeconds _wait = new WaitForSeconds(2f);
    private int _correntCount;
    
    public void SpawnWave(int waveNumber, Action onMonsterSpawn, Action onSpawnEnd)
    {
        StartCoroutine(WaveStartRoutine(onMonsterSpawn, onSpawnEnd));
    }
    
    private IEnumerator WaveStartRoutine(Action onMonsterSpawn, Action onSpawnEnd)
    {
        _correntCount = 0;
        while(_count > _correntCount)
        {
            yield return _wait;
            Debug.Log("생성");
            GameObject clone = Instantiate(_monsterPrefab,transform.position,transform.rotation);
            // 몬스터한테 waypointPath posititon전달
            onMonsterSpawn?.Invoke(); //몬스터 생성값을 어떻게 할 것인지 waveManager&monster상의
            _correntCount++;
        }
        
        Debug.Log("생성 끝");
        onSpawnEnd?.Invoke();
    }
}
