using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBulletTest : PoolObject
{
    public override void Sleep()
    {
        gameObject.SetActive(false);
    }

    public override void WakeUp()
    {
        gameObject.SetActive(true);
        StartCoroutine(ShotTime());
    }


    private IEnumerator ShotTime()
    {
        yield return new WaitForSeconds(2f);
        ReturnToPool();
    }
}