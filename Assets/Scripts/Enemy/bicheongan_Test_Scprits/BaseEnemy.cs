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
    [SerializeField] protected float _monCastleDam;
    
    private Transform _tr;

    public float MonHp { get { return _monHp; } set { _monHp = value; } }
    public float MonDefend { get { return _monDefend; } set { _monDefend = value; } }
    public float MonSpeed { get { return _monSpeed; } set { _monSpeed = value; } }
    public float MonGold { get { return _monGlod; } set { _monGlod = value; } }
    public float MonExp { get { return _monExp; } set { _monExp = value; } }
    public float MonCastleDam { get { return _monCastleDam; } set { _monCastleDam = value; } }

    public event Action<BaseEnemy> onRemoved;

    private void Awake()
    {
        CaCheComponents();
    }
    public override void WakeUp()
    {
        transform.SetParent(null);
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        transform.SetParent(_tr);
        onRemoved?.Invoke(this);
        gameObject.SetActive(false);
    }
    private PlayerLevelManager _eXP;
    public void GiveToExpPlayer()
    {
        _eXP.GainExp(MonExp);
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
    public void InitData()
    {
        _tr = transform.parent;
    }
    private void CaCheComponents()
    {
        _eXP = GameManager.Instance.GetComponent<PlayerLevelManager>();
    }
}
