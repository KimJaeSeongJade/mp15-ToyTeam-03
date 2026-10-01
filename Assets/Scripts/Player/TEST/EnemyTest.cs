using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour, IDamagable
{
    public void TakeDamage(float damage)
    {
        Debug.Log($"{gameObject.name} 에게 {damage}만큼 대미지");
    }    

}
