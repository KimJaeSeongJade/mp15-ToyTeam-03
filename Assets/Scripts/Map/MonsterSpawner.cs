using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private WayPointPath[] _waypointPath;

    [SerializeField] private GameObject[] PortalEffect;
    //[SerializeField] private int _count;
    [SerializeField] private BaseEnemy _monsterPrefab;
    [SerializeField] private MonsterGroup monsterGroup1;
    [SerializeField] private MonsterPool  _monsterPool;
    
    
    private readonly WaitForSeconds _wait = new WaitForSeconds(2f);
    private int _correntCount;
    private BaseEnemy _monster;
    

    private void Awake() => Portaleffect();

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
                    PortalEffect[waveNumber-1].SetActive(true);
                    for (int j = 0; j < monsterGroup1.monsterDatas[i].count; j++)
                    {
                        _monster = _monsterPool.Pop();
                        _monster.transform.SetPositionAndRotation(_waypointPath[waveNumber-1].transform.position, 
                            transform.rotation);
                        
                        /*_monster = Instantiate(_monsterPrefab, 
                            _waypointPath[waveNumber-1].transform.position, transform.rotation);*/
                        Debug.Log($"{j+1} 마리");
                        _monster.GetComponent<MonsterMove>().Initialize(_waypointPath[waveNumber-1]);
                        onMonsterSpawn?.Invoke(_monster);
                        yield return _wait;
                    }
                } 
            }
            
            break;
        }
        Debug.Log("생성 끝");
        Portaleffect();
        onSpawnEnd?.Invoke();
        
    }
    private void Portaleffect()
    {
        PortalEffect[0].SetActive(false);
        PortalEffect[1].SetActive(false);
    }
}
