using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAttack : MonoBehaviour
{
    [SerializeField] protected LayerMask _targetMask;
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _damageDelay = 0.5f;
    [SerializeField] protected float _capsuleRadius = 1f;
    [SerializeField] protected float _capsuleHeight = 3f;

    protected virtual void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    protected virtual IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_damageDelay);

        ApplyDamage();

        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    protected virtual void ApplyDamage()
    {
        float halfDistance = (_capsuleHeight * 0.5f - _capsuleRadius);

        Vector3 offset = transform.up * halfDistance;
        Vector3 center = transform.position;

        Collider[] hits = Physics.OverlapCapsule(
            center - offset,
            center + offset,
            _capsuleRadius,
            _targetMask,
            QueryTriggerInteraction.Collide);

        var damagedTargets = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null && damagedTargets.Add(target))
            {
                target.TakeDamage(_damage);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float halfDistance = Mathf.Max(0f, _capsuleHeight * 0.5f - _capsuleRadius);
        Vector3 offset = transform.up * halfDistance;
        Vector3 bottom = transform.position - offset;
        Vector3 top = transform.position + offset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottom, _capsuleRadius);
        Gizmos.DrawWireSphere(top, _capsuleRadius);

        Gizmos.DrawLine(bottom + transform.right * _capsuleRadius,
                        top + transform.right * _capsuleRadius);
        Gizmos.DrawLine(bottom - transform.right * _capsuleRadius,
                        top - transform.right * _capsuleRadius);
        Gizmos.DrawLine(bottom + transform.forward * _capsuleRadius,
                        top + transform.forward * _capsuleRadius);
        Gizmos.DrawLine(bottom - transform.forward * _capsuleRadius,
                        top - transform.forward * _capsuleRadius);
    }
#endif
}
