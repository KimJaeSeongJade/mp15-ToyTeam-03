using System.Collections.Generic;
using UnityEngine;

// 트리거에 들어온 콜라이더만 관리한다. 같은 몬스터의 여러 콜라이더도 유지한다.
public class TurretTargetList
{
    private class TargetData
    {
        public Collider Body;
        public Transform Target;
        public IHealthSource Health;
        public BaseEnemy Enemy;
    }

    private readonly List<TargetData> _targets = new List<TargetData>();

    public void Register(Collider body)
    {
        foreach (TargetData target in _targets)
        {
            if (target.Body == body) return;
        }

        IHealthSource health = body.GetComponentInParent<IHealthSource>();
        BaseEnemy enemy = body.GetComponentInParent<BaseEnemy>();
        TargetData data = new TargetData
        {
            Body = body,
            Target = enemy != null ? enemy.transform : health != null ? ((Component)health).transform : body.transform,
            Health = health,
            Enemy = enemy
        };
        _targets.Add(data);
        if (enemy != null) enemy.onRemoved += RemoveEnemy;
    }

    public void Remove(Collider body)
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            if (_targets[i].Body == body) RemoveAt(i);
        }
    }

    private void RemoveEnemy(BaseEnemy enemy)
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            if (_targets[i].Enemy == enemy) RemoveAt(i);
        }
    }

    public Transform GetTarget(Transform current)
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            TargetData data = _targets[i];
            if (data.Body == null || !data.Body.enabled || !data.Body.gameObject.activeInHierarchy
                || data.Target == null || !data.Target.gameObject.activeInHierarchy
                || (data.Health != null && data.Health.CurrentHp <= 0f))
            {
                RemoveAt(i);
            }
        }

        foreach (TargetData data in _targets)
        {
            if (data.Target == current) return current;
        }
        return _targets.Count > 0 ? _targets[0].Target : null;
    }

    public static bool IsAlive(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;
        IHealthSource health = target.GetComponentInParent<IHealthSource>();
        return health == null || health.CurrentHp > 0f;
    }

    private void RemoveAt(int index)
    {
        BaseEnemy enemy = _targets[index].Enemy;
        if (enemy != null) enemy.onRemoved -= RemoveEnemy;
        _targets.RemoveAt(index);
    }

    public void Clear()
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            RemoveAt(i);
        }
    }
}
