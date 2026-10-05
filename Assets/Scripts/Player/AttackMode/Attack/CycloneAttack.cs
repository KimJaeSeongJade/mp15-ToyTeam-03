using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CycloneAttack : SkillAttack
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _damageInterval = 0.5f;
    [SerializeField] private float _pullSpeed = 10f;


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
        Collider[] hits = Physics.OverlapSphere(transform.position, _capsuleRadius, _targetMask);
        HashSet<IDamageable> damaged = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target != null && damaged.Add(target))
                target.TakeDamage(_damage);
        }
    }

    private void PullEnemies(float deltaTime)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _capsuleRadius, _targetMask);
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
