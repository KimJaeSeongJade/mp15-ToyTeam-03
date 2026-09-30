using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolTest : MonoBehaviour
{
    [Header("키보드 A를 눌러 발사")]
    [SerializeField] private PoolBulletTest bullet;

    [SerializeField] private Transform parent;


    private ObjectPool<PoolBulletTest> bulletPool;

    private void Start()
    {
        bulletPool = new ObjectPool<PoolBulletTest>(bullet, 10);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            bullet = bulletPool.Pop();
        }
    }
}