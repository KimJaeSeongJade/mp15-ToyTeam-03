using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldTurret : BaseTurret
{
    [Header("Gold Settings")]
    [SerializeField] private GoldDrop _goldPrefab; // 💡 PoolObject를 상속받은 골드 프리팹
    private ObjectPool<GoldDrop> _goldPool;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _productionCoolTime = 3f;

    // 프로젝트 규격(IPoolable)에 맞춘 수동 오브젝트 풀 예시
    private float _lastProductionTime;
    
    private void Start() => Init();
    private void Update() => Attack();

    private void Init()
    {
        _goldPool = new ObjectPool<GoldDrop>(_goldPrefab, 1, _spawnPoint, drop => { drop.InitData();});
    }
    

    // BaseTurret 구조 상 구현해야 하는 추상 함수
    public override void Attack()
    {
        
        if (Time.time >= _lastProductionTime + _productionCoolTime)
        {
            if (_goldPrefab != null && _spawnPoint != null)
            {
                _goldPrefab = _goldPool.Pop();
            }
            // 골드타워는 공격대신 공격 동작으로 골드생성
            _lastProductionTime = Time.time;
        }
    } 
}
