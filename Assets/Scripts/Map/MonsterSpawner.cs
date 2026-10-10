using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private WayPointPath[] _waypointPath;

    [SerializeField] private GameObject[] PortalEffect;
    //[SerializeField] private int _count;
    private ObjectPool<BaseEnemy> _monsterGroupPool;
    [SerializeField] private List<MonsterGroup> _monsterGroupList;
    public int TotalWaveCount => _monsterGroupList.Count; // 게임 오버 판정을 확인하기 위해 읽기 전용 프로퍼티 추가
    
    private readonly WaitForSeconds _wait = new WaitForSeconds(2f);
    private BaseEnemy _monster;


    private void Awake()
    {
        PortaleffectTure();
    }

    private void Start() => Init();

    private void Init()
    {
        _monsterGroupPool = new ObjectPool<BaseEnemy>(
            _monsterGroupList[0].GetInitMonsterDataPools(),
            transform,
            monster => monster.InitData());
    }

    private bool _isCamera()
    {
        return  true;
    }
    public void SpawnWave(int waveNumber, Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        StartCoroutine(WaveStartRoutine(waveNumber,onMonsterSpawn, onSpawnEnd));
    }
    
    private IEnumerator WaveStartRoutine(int waveNumber,Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        /*while (true)
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
                            _waypointPath[waveNumber-1].transform.position, transform.rotation);#1#
                        Debug.Log($"{j+1} 마리");
                        _monster.GetComponent<MonsterMove>().Initialize(_waypointPath[waveNumber-1]);
                        onMonsterSpawn?.Invoke(_monster);
                        yield return _wait;
                    }
                } 
            }
            
            break;
        }*/

        MonsterGroup _localMonsterGroup = _monsterGroupList[waveNumber-1];
        _localMonsterGroup.ResetSpwan();

        while (_localMonsterGroup.TryGetNext(out MonsterGroup.MonsterData resultData))
        {
            int pathNum = resultData.waypointNum;
            WayPointPath targetPath = _waypointPath[pathNum-1];

            _monster = _monsterGroupPool.Pop(resultData.monster);
            _monster.transform.SetPositionAndRotation(targetPath.transform.position, transform.rotation);
            _monster.GetComponent<MonsterMove>().Initialize(targetPath);
            onMonsterSpawn?.Invoke(_monster);
            yield return _wait;

        }
        
        Debug.Log("생성 끝");
        PortaleffectFalse();
        onSpawnEnd?.Invoke();
        
    }
    private void PortaleffectFalse()
    {
        PortalEffect[0].SetActive(false);
        PortalEffect[1].SetActive(false);
    }

    private void PortaleffectTure()
    {
        PortalEffect[0].SetActive(true);
        PortalEffect[1].SetActive(true);
    }
}
