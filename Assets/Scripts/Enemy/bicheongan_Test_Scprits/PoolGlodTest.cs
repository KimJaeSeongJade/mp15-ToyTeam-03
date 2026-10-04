using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolGlodTest : PoolObject
{
    public override void Sleep()
    {
        gameObject.SetActive(false);
    }

    public override void WakeUp()
    {
        gameObject.SetActive(true);
    }

}
