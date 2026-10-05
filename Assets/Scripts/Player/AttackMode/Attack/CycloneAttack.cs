using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CycloneAttack : SkillAttack
{
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField, Min(0.1f)] private float _radius = 5f;
    [SerializeField, Min(0.1f)] private float _duration = 5f;
    [SerializeField, Min(0.05f)] private float _damageInterval = 0.5f;
    [SerializeField, Min(0f)] private float _damagePerTick = 3f;
    [SerializeField, Min(0f)] private float _pullSpeed = 10f;

    private void Awake()
    {
        if (_enemyMask.value == 0)
            _enemyMask = LayerMask.GetMask("Enemy");
    }

    // 부모 SkillAttack의 한 번 피해 후 종료하는 루틴을 지속 피해 루틴으로 바꾼다.
    protected override IEnumerator AttackRoutine()
    {
        float elapsed = 0f;
        float damageElapsed = 0f;

        while (elapsed < _duration)
        {
            float deltaTime = Time.deltaTime;
            PullEnemies(deltaTime);

            damageElapsed += deltaTime;
            if (damageElapsed >= _damageInterval)
            {
                ApplyDamage();
                damageElapsed -= _damageInterval;
            }

            elapsed += deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    protected override void ApplyDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _radius, _enemyMask,
            QueryTriggerInteraction.Collide);
        HashSet<IDamageable> damaged = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null && damaged.Add(target))
                target.TakeDamage(_damagePerTick);
        }
    }

    private void PullEnemies(float deltaTime)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _radius, _enemyMask,
            QueryTriggerInteraction.Collide);
        HashSet<NavMeshAgent> pulled = new HashSet<NavMeshAgent>();

        foreach (Collider hit in hits)
        {
            NavMeshAgent agent = hit.GetComponentInParent<NavMeshAgent>();
            if (agent == null || !agent.isOnNavMesh || !pulled.Add(agent)) continue;

            Vector3 toCenter = transform.position - agent.transform.position;
            toCenter.y = 0f;
            float distance = toCenter.magnitude;
            if (distance <= 0.05f) continue;

            agent.Move(toCenter / distance * Mathf.Min(_pullSpeed * deltaTime, distance));
        }
    }
}
