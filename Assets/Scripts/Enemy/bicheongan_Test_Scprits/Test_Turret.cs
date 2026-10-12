using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Turret : MonoBehaviour, IDamageable
{
    [SerializeField] private float _hp = 100f;
    [SerializeField] private float _slowRate = 0.2f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _range = 10f;
    private bool _isDead;

    private void Awake()
    {
        IDamageable damage = GetComponent<IDamageable>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAttack();
        }
    }

    private void TestAttack()
    {
        Collider[] targets = Physics.OverlapSphere(
            transform.position,
            _range,
            _enemyLayer
        );

        if (targets.Length == 0)
        {
            #if UNITY_EDITOR
            Debug.Log("못찾음");
#endif
        }

        IDamageable target = targets[0].GetComponent<IDamageable>();

        if (target == null)
            return;

        target.SlowSpeed(0.2f);

        #if UNITY_EDITOR
        Debug.Log("느려는 공격");
#endif
    }

    public void TakeDamage(float damage)
    {
        if (_isDead == true) return;

        _hp -= damage;

        if (_hp <= 0)
        {
            _isDead = true;
            #if UNITY_EDITOR
            Debug.Log("터렛 파괴");
#endif
            Destroy(gameObject);
        }
    }

    public void SlowSpeed(float speed)
    {
        
    }

    public bool CheckExecution(float thresholdRatio)
    {
        return false;
    }
}
