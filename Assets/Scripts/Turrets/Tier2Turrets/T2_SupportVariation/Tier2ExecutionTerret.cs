using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2ExecutionTurret : BaseTurret
{
    [Header("Execution Settings")]
    [SerializeField] private float _executionRange = 6f;        // 처형 감지 범위 (반지름)
    [SerializeField] private float _executionThreshold = 0.15f; // 처형 기준 체력 비율 (15%)
    [SerializeField] private LayerMask _enemyLayer;             // 감지할 적 레이어
    [SerializeField] private float _scanInterval = 0.2f;        // 범위 내 적을 스캔할 주기 (초)

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _executionFxPrefab; // 처형 성공 시 팡 터질 이펙트

    private float _nextScanTime;

    private void Update()
    {
        if (Time.time >= _nextScanTime)
        {
            ExecuteLowHpEnemies();
            _nextScanTime = Time.time + _scanInterval;
        }
    }

    private void ExecuteLowHpEnemies()
    {
        // 사거리 내 적 레이어 수집
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, _executionRange, _enemyLayer);

        for (int i = 0; i < hitEnemies.Length; i++)
        {
            // 💡 [핵심] 순수 인터페이스(IDamageable)만 추출합니다.
            if (hitEnemies[i].TryGetComponent(out IDamageable damageable))
            {
                // 💡 적 유닛 안에 선언된 인터페이스 처형 함수를 직접 호출합니다!
                // 적이 스스로 조건을 검사하고 처형에 성공(true)했다면 터렛은 이펙트만 예쁘게 띄워줍니다.
                // TODO IDamageable 내 추가
                if (damageable.CheckExecution(_executionThreshold))
                {
                    PlayExecutionEffect(hitEnemies[i].transform.position);
                }
            }
        }
    }

    private void PlayExecutionEffect(Vector3 spawnPosition)
    {
        if (_executionFxPrefab != null)
        {
            Vector3 fxPos = spawnPosition + Vector3.up * 0.5f;
            ParticleSystem fxInstance = Instantiate(_executionFxPrefab, fxPos, Quaternion.identity);
            
            var mainModule = fxInstance.main;
            mainModule.stopAction = ParticleSystemStopAction.Destroy;
        }
    }

    public override void Attack() { }
}
