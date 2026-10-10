using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private WayPointPath[] _waypointPath;
    [SerializeField] private WaveManager _waveManager;
    [SerializeField] private GameObject[] PortalEffect;
    private ObjectPool<BaseEnemy> _monsterGroupPool;
    [SerializeField] private List<MonsterGroup> _monsterGroupList;
    public int TotalWaveCount => _monsterGroupList.Count; // 게임 오버 판정을 확인하기 위해 읽기 전용 프로퍼티 추가
    
    private readonly WaitForSeconds _wait = new WaitForSeconds(2f);
    private BaseEnemy _monster;
    private bool _isSpawnEnd;
    
    
    private void Start()
    {
        Init();
    } 
    private void OnEnable()
    {
        _waveManager.OnWaveEnded += HandleSpawnEnd;
    }

    private void OnDisable()
    {
        _waveManager.OnWaveEnded -= HandleSpawnEnd;
    }
    

    private void Init()
    {
        _monsterGroupPool = new ObjectPool<BaseEnemy>(
            _monsterGroupList[0].GetInitMonsterDataPools(),
            transform,
            monster => monster.InitData());
    }
    
    public void SpawnWave(int waveNumber, Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        StartCoroutine(WaveStartRoutine(waveNumber,onMonsterSpawn, onSpawnEnd));
    }
    
    private IEnumerator WaveStartRoutine(int waveNumber,Action<BaseEnemy> onMonsterSpawn, Action onSpawnEnd)
    {
        _isSpawnEnd = true;
        MonsterGroup _localMonsterGroup = _monsterGroupList[waveNumber-1];
        _localMonsterGroup.ResetSpwan();

        while (_localMonsterGroup.TryGetNext(out MonsterGroup.MonsterData resultData))
        {
            int pathNum = resultData.waypointNum;
            WayPointPath targetPath = _waypointPath[pathNum-1];

            if (_isSpawnEnd != false)
            { 
                _monster = _monsterGroupPool.Pop(resultData.monster); 
                _monster.transform.SetPositionAndRotation(targetPath.transform.position, transform.rotation);
                
                BossMove _boss = _monster.GetComponent<BossMove>();
            
                if (_boss != null)
                {
                    _boss.Initialize(targetPath);
                }
                else
                {
                    MonsterMove _normalMon = _monster.GetComponent<MonsterMove>();
                    if (_normalMon != null)
                    {
                        _normalMon.Initialize(targetPath);
                    }
                }
                onMonsterSpawn?.Invoke(_monster);
                yield return _wait;   
            }
        }
        
        #if UNITY_EDITOR
        Debug.Log("생성 끝");
#endif
        PortaleffectFalse();
        onSpawnEnd?.Invoke();
        
    }
    
    private void HandleSpawnEnd()
    {
        _isSpawnEnd = false;
        Debug.Log("HandleSpawnEnd");
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
