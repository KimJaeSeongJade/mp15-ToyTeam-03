using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private float hp = 50f;

    // 인터페이스에 있던 메서드를 구현합니다.
    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name}이 {damage}의 데미지를 입었습니다. 남은 체력: {hp}");

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
