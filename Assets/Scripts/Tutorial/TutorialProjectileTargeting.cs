using UnityEngine;

// 튜토리얼용 총알 복제본에만 추가한다. 기존 Bullet의 타겟 API를 재사용한다.
[RequireComponent(typeof(Bullet))]
public class TutorialProjectileTargeting : MonoBehaviour
{
    private void OnEnable()
    {
        BaseEnemy nearest = null;
        float nearestDistance = float.PositiveInfinity;
        foreach (BaseEnemy enemy in FindObjectsOfType<BaseEnemy>())
        {
            if (enemy.gameObject.scene != gameObject.scene || enemy.MonHp <= 0f) continue;
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearest = enemy;
            nearestDistance = distance;
        }
        GetComponent<Bullet>().SetTarget(nearest != null ? nearest.transform : null);
    }
}
