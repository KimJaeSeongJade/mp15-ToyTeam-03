using UnityEngine;
using System.Collections;

// 튜토리얼용 총알 복제본에만 추가한다. 기존 Bullet의 타겟 API를 재사용한다.
[RequireComponent(typeof(Bullet))]
public class TutorialProjectileTargeting : MonoBehaviour
{
    private Bullet _bullet;
    private BaseTurret _owner;
    private bool _enteredPool;

    private void Awake()
    {
        _bullet = GetComponent<Bullet>();
        _owner = GetComponentInParent<BaseTurret>();
    }

    private void OnDisable() => _enteredPool = true;

    private void OnEnable()
    {
        // ObjectPool의 최초 생성 활성화에서는 아직 InitDate/Init이 완료되지 않았다.
        if (!_enteredPool) return;
        SphereCollider range = _owner != null ? _owner.GetComponent<SphereCollider>() : null;
        if (range == null)
        {
            StartCoroutine(ReturnUnused());
            return;
        }
        Vector3 center = range.transform.TransformPoint(range.center);
        Vector3 scale = range.transform.lossyScale;
        float radius = range.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        BaseEnemy nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (BaseEnemy enemy in FindObjectsOfType<BaseEnemy>())
        {
            if (enemy.gameObject.scene != gameObject.scene || enemy.MonHp <= 0f) continue;
            Collider body = enemy.GetComponent<Collider>();
            Vector3 closest = body != null ? body.ClosestPoint(center) : enemy.transform.position;
            if ((closest - center).sqrMagnitude > radius * radius) continue;
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearest = enemy;
            nearestDistance = distance;
        }
        _bullet.SetTarget(nearest != null && !(_bullet is TutorialPiercingBullet) ? nearest.transform : null);
        if (nearest != null)
        {
            _bullet.SetDamage(_owner.Damage);
            if (_bullet is TutorialPiercingBullet piercing) piercing.Aim(nearest.transform.position);
        }
        else StartCoroutine(ReturnUnused());
    }

    private IEnumerator ReturnUnused()
    {
        // WakeUp/Pop이 끝난 뒤 반환한다. 사거리 밖의 다음 웨이브 몬스터를 먼저 죽이지 않는다.
        yield return null;
        if (gameObject.activeInHierarchy) _bullet.ReturnToPool();
    }
}
