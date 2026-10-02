using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamagable
{
    [SerializeField] private float _hp;
    [SerializeField] private GoldDrop _goldPrabas;
    private ObjectPool<GoldDrop> goldPool;
    private BaseEnemy Cacheenemy;

    private bool _isDead;

    private void Awake()
    {
        CacheComponent();
        goldPool = new ObjectPool<GoldDrop>(_goldPrabas, 20);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }
    }
    public void TakeDamage(float damage)
    {
        if (_isDead == true) return;

        _hp -= damage;
        if(_hp <= 0)
        {
            Debug.Log("죽음");
            _isDead = true;
            goldPool.PopAll();
            Cacheenemy.ReturnToPool();
            //Destroy(gameObject);// 몬스터 오브젝트 풀 만들시 풀 반환으로 변경 
        }
    }
    private void CacheComponent()
    {
        Cacheenemy = GetComponent<BaseEnemy>();
    }

}
