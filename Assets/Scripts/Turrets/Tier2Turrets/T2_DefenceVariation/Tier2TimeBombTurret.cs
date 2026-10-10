using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2TimeBombTurret : BaseTurret
{
    [Header("Time Bomb Settings")]
    [SerializeField] private float _bombRange = 4f;       // 폭발 범위 (반지름)
    [SerializeField] private float _bombDamage = 150f;     // 폭발 대미지
    [SerializeField] private LayerMask _enemyLayer;        // 적 레이어

    [Header("Gold Spawn Settings")]
    [SerializeField] private GoldDrop _goldPrefab;         // 소환할 골드 프리팹 (PoolObject 상속)
    [SerializeField] private int _spawnGoldCount = 5;      // 파괴 시 소환할 골드 주머니/드롭 개수

    private bool _hasExploded = false; // 중복 폭발 방지 플래그

    // 💡 부모의 TakeDamage를 오버라이드하여 체력이 다했을 때만 폭발과 골드 스폰을 실행합니다.
    public override void TakeDamage(float damage)
    {
        // 부모의 체력 차감 및 기본 로직 실행
        base.TakeDamage(damage);

        // 체력이 0 이하가 되었고, 아직 폭발하지 않았다면 수행
        if (_currentHp <= 0 && !_hasExploded)
        {
            _hasExploded = true;

            // 1. 터렛 파괴 후 일정 범위 내 일괄적 데미지 준 후
            TurretBomb();

            // 2. 골드 소환
            GoldSpawn();
        }
    }

    private void TurretBomb()
    {
        #if UNITY_EDITOR
        Debug.Log($"💥 [{gameObject.name}] 시한폭탄 터렛 작동! 광역 폭발 발생.");
#endif

        // _bombRange만큼의 원형 범위 내에 있는 적 레이어 콜라이더들을 긁어옵니다.
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, _bombRange, _enemyLayer);

        for (int i = 0; i < hitEnemies.Length; i++)
        {
            // 범위 내에 감지된 적들의 IDamageable 인터페이스를 추출하여 폭발 대미지를 꽂습니다.
            if (hitEnemies[i].TryGetComponent(out IDamageable enemyDamageable))
            {
                enemyDamageable.TakeDamage(_bombDamage);
                #if UNITY_EDITOR
                Debug.Log($"[폭발 피격] {hitEnemies[i].name}에게 {_bombDamage}만큼의 폭발 피해를 입혔습니다.");
#endif
            }
        }
    }

    private void GoldSpawn()
    {
        if (_goldPrefab == null) return;

        #if UNITY_EDITOR
        Debug.Log($"🪙 [{gameObject.name}] 파괴 보상으로 골드를 {_spawnGoldCount}개 소환합니다.");
#endif

        for (int i = 0; i < _spawnGoldCount; i++)
        {
            // 💡 이전에 구현하셨던 수동 풀링 큐 구조나 오브젝트 풀 시스템이 있다면 풀에서 꺼내야 합니다.
            // 여기서는 기본 생성 규격(Instantiate) 후 WakeUp 구조로 연동해 둡니다.
            GoldDrop goldInstance = Instantiate(_goldPrefab, transform.position, Quaternion.identity);

            if (goldInstance != null)
            {
                // 위치 초기화 및 데이터 세팅 (GoldDrop 내부의 InitData가 있다면 호출)
                goldInstance.InitData();
                goldInstance.WakeUp();
            }
        }
    }

    // 에디터 인스펙터 선택 시 폭발 범위를 붉은색 원으로 시각화해 줍니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _bombRange);
    }

    // BaseTurret 구조 상 무조건 오버라이드해야 하는 추상 함수
    public override void Attack()
    {
        // 파괴될 때 폭발하는 자폭/방어형 터렛이므로 기본 자동 사격은 비워둡니다.
    }
}