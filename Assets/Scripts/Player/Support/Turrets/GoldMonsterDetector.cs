using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class GoldMonsterDetector : MonoBehaviour
{
    [SerializeField] private LayerMask _targetMask = 1 << 9;
    private readonly Dictionary<Collider, MonsterHealth> _contacts = new Dictionary<Collider, MonsterHealth>();
    private readonly List<Collider> _remove = new List<Collider>();
    private readonly HashSet<MonsterHealth> _unique = new HashSet<MonsterHealth>();
    [SerializeField] private List<MonsterHealth> _monsters = new List<MonsterHealth>();
    private SphereCollider _trigger;

    private void Awake()
    {
        _trigger = GetComponent<SphereCollider>();
        _trigger.isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }
    private void OnTriggerEnter(Collider other) => Register(other);
    private void OnTriggerStay(Collider other) => Register(other);
    private void Register(Collider other)
    {
        if (!isActiveAndEnabled) return;
        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;
        MonsterHealth health = other.GetComponentInParent<MonsterHealth>();
        if (health == null || !health.isActiveAndEnabled) return;
        _contacts[other] = health;
        if (!_monsters.Contains(health)) _monsters.Add(health);
    }
    private void OnTriggerExit(Collider other)
    {
        _contacts.Remove(other);
        GetMonsters();
    }

    // 발동 시 풀 반환/파괴된 대상과 오래된 접촉을 제거하고, 몬스터별로 중복을 제외한다.
    public IReadOnlyList<MonsterHealth> GetMonsters()
    {
        _monsters.Clear();
        _unique.Clear();
        _remove.Clear();
        if (!isActiveAndEnabled || !_trigger.enabled) return _monsters;
        Vector3 center = transform.TransformPoint(_trigger.center);
        Vector3 scale = transform.lossyScale;
        float radius = _trigger.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        foreach (var contact in _contacts)
        {
            Collider collider = contact.Key;
            MonsterHealth health = contact.Value;
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy ||
                health == null || !health.isActiveAndEnabled ||
                (collider.ClosestPoint(center) - center).sqrMagnitude > radius * radius)
            {
                _remove.Add(collider);
                continue;
            }
            if (_unique.Add(health)) _monsters.Add(health);
        }
        foreach (Collider collider in _remove) _contacts.Remove(collider);
        return _monsters;
    }
    private void OnDisable()
    {
        _contacts.Clear();
        _monsters.Clear();
    }
}
