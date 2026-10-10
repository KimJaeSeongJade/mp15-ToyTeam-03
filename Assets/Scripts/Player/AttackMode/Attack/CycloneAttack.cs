using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CycloneAttack : SkillAttack
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _damageInterval = 0.5f;
    [SerializeField] private float _pullSpeed = 10f;
    private Collider[] _hits = new Collider[64];
    private int _hitCount;
    private readonly HashSet<IDamageable> _damaged = new HashSet<IDamageable>();
    private readonly HashSet<NavMeshAgent> _pulled = new HashSet<NavMeshAgent>();


    // 부모 SkillAttack의 한 번 피해 후 종료하는 루틴을 지속 피해 루틴으로 바꾼다.
    protected override IEnumerator AttackRoutine()
    {
        float elapsed = 0f;
        float damageElapsed = 0f;

        while (elapsed < _duration)
        {
            float deltaTime = Time.deltaTime;
            DetectEnemies();
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
        _damaged.Clear();

        for (int i = 0; i < _hitCount; i++)
        {
            Collider hit = _hits[i];
            if (hit == null || !hit.gameObject.activeInHierarchy) continue;
            IDamageable target = hit.GetComponentInParent<IDamageable>();

            if (target != null && _damaged.Add(target))
                target.TakeDamage(_damage);
        }
    }

    private void PullEnemies(float deltaTime)
    {
        _pulled.Clear();

        for (int i = 0; i < _hitCount; i++)
        {
            Collider hit = _hits[i];
            if (hit == null || !hit.gameObject.activeInHierarchy) continue;
            NavMeshAgent agent = hit.GetComponentInParent<NavMeshAgent>();

            if (agent == null || !agent.isOnNavMesh || !_pulled.Add(agent)) continue;

            Vector3 toCenter = transform.position - agent.transform.position;
            toCenter.y = 0f;
            float distance = toCenter.magnitude;

            if (distance <= 0.05f) continue;

            agent.Move(toCenter / distance * Mathf.Min(_pullSpeed * deltaTime, distance));
        }
    }

    private void DetectEnemies()
    {
        // 버퍼가 가득 찼을 때만 확장하고 다시 검색해 대상이 누락되지 않게 한다.
        while (true)
        {
            _hitCount = Physics.OverlapSphereNonAlloc(transform.position, _capsuleRadius, _hits, _targetMask);
            if (_hitCount < _hits.Length) return;
            System.Array.Resize(ref _hits, _hits.Length * 2);
        }
    }
}
