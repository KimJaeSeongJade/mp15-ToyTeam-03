using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : PoolObject
{
    [SerializeField] protected float _monHp;
    [SerializeField] protected float _monDefend;
    [SerializeField] protected float _monSpeed;
    [SerializeField] protected float _monGlod;
    [SerializeField] protected float _monExp;
    

    public float MonHp { get { return _monHp; } set { _monHp = value; } }
    public float MonDefend { get { return _monDefend; } set { _monDefend = value; } }
    public float MonSpeed { get { return _monSpeed; } set { _monSpeed = value; } }
    public float MonGold { get { return _monGlod; } set { _monGlod = value; } }
    public float MonExp { get { return _monExp; } set { _monExp = value; } }

    public event Action<BaseEnemy> onRemoved;
    public override void WakeUp()
    {
        
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        onRemoved?.Invoke(this);
        gameObject.SetActive(false);
    }

    private bool _finished;
    public event Action<BaseEnemy> Returned;
    public void ReachGoal()
    {
        if (_finished == true) return;

        _finished = true;
        Debug.Log("결승");
        Returned?.Invoke(this);
    }
}
