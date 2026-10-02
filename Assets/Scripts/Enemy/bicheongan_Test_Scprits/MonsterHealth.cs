using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHealth : MonoBehaviour, IDamagable
{
    [SerializeField] private float _hp;
    private bool _isDead;

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
            //DropGold();
            Destroy(gameObject);// 오브젝트 풀 만들시 풀 반환으로 변경 
        }
    }
    /*private void DropGold()
    {
        GameObject gold = GoldPool.Instance.Take();

        if (gold == null)
            return;

        gold.transform.position = transform.position;
    }*/
}
