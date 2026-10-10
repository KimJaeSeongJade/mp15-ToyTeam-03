using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tier2BountyTurret : BaseTurret
{
    [Header("Bounty Settings")]
    [SerializeField] private float _scanRange = 7f;         // 현상금 표식을 부여할 사거리
    [SerializeField] private int _bountyReward = 50;       // 현상금 적 처치 시 추가 골드 드랍량
    [SerializeField] private float _bountyInterval = 7f;    // 표식을 부여할 쿨타임 (초)
    [SerializeField] private LayerMask _enemyLayer;         // 감지할 적 레이어

    private float _nextBountyTime;
    private IDamageable _currentTarget; // 현재 현상금 표식이 찍힌 적

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // 💡 지정된 주기마다 사거리 내 적을 탐색해 현상금 표식을 부여합니다.
        if (Time.time >= _nextBountyTime)
        {
            ApplyBountyMark();
        }
    }

    private void ApplyBountyMark()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, _scanRange, _enemyLayer);
        if (hitEnemies.Length == 0) return;

        BaseEnemy highestHpEnemy = null;
        float maxHp = -1f;

        // 1. 범위 내 적들 중 가장 체력이 높은 몬스터(정예/보스급)를 탐색합니다.
        for (int i = 0; i < hitEnemies.Length; i++)
        {
            if (hitEnemies[i].TryGetComponent(out BaseEnemy enemy))
            {
                if (enemy.MonHp > maxHp)
                {
                    maxHp = enemy.MonHp;
                    highestHpEnemy = enemy;
                }
            }
        }

        // 2. 가장 강한 적을 찾았다면 현상금 표식 부여 시퀀스 시작
        if (highestHpEnemy != null && highestHpEnemy.TryGetComponent(out IDamageable damageable))
        {
            _currentTarget = damageable;
            _nextBountyTime = Time.time + _bountyInterval;

            // 💡 [현상금 핵심] 몬스터 내부의 스크립트를 더럽히지 않고, 
            // 몬스터의 GameObject에 "나(터렛)"의 존재와 현상금 액수를 컴포넌트로 임시 부착합니다.
            BountyMark mark = highestHpEnemy.gameObject.AddComponent<BountyMark>();
            mark.Setup(_bountyReward);

            Debug.Log($"<color=yellow>[💰 현상금 부여]</color> <b>{highestHpEnemy.name}</b>에게 {_bountyReward}골드의 현상금 표식이 추가되었습니다!");
        }
    }

    public override void Attack()
    {
        // 아군 터렛들의 타겟팅을 유도하고 골드를 수급하는 지원형 포탑이므로 기본 공격은 비워둡니다.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _scanRange);
    }
}
