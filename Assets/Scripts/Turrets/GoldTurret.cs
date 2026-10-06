using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldTurret : BaseTurret
{
    [Header("Gold Settings")]
    [SerializeField] private Gold _goldPrefab; // 💡 PoolObject를 상속받은 골드 프리팹
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _productionCoolTime = 3f;

    // 프로젝트 규격(IPoolable)에 맞춘 수동 오브젝트 풀 예시
    private Queue<Gold> _poolQueue = new Queue<Gold>();
    private float _lastProductionTime;

    private void Awake()
    {
        // 💡 1. Gold 프리팹의 Property에서 골드 양을 가져와 터렛 설치 비용으로 동기화합니다.
        if (_goldPrefab != null)
        {
            _goldCost = _goldPrefab.GoldAmount; 
        }
    }

    private void Update()
    {
        // 쿨타임마다 골드 생산
        if (Time.time >= _lastProductionTime + _productionCoolTime)
        {
            ProduceGold();
            _lastProductionTime = Time.time;
        }
    }

    private void ProduceGold()
    {
        Gold goldInstance;

        // 풀에 남아있는 오브젝트가 있다면 꺼내고, 없다면 새로 생성
        if (_poolQueue.Count > 0)
        {
            goldInstance = _poolQueue.Dequeue();
        }
        else
        {
            // 새로 생성하는 시점에 딱 1번 Init을 호출하여 반환 델리게이트를 주입합니다.
            goldInstance = Instantiate(_goldPrefab, _spawnPoint.position, Quaternion.identity);
            
            // 💡 2. PoolObject.Init() 규격에 맞춰 반환 액션을 바인딩합니다.
            goldInstance.Init(poolable => {
                _poolQueue.Enqueue((Gold)poolable);
            });
        }

        // 위치 잡아주고 깨우기
        goldInstance.transform.position = _spawnPoint.position;
        goldInstance.WakeUp();
    }

    // BaseTurret 구조 상 구현해야 하는 추상 함수
    public override void Attack() { } 
}
