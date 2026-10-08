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
    [SerializeField] private GameObject _IsDeadImpack;
    [SerializeField] private GameObject _hitImpact;

    [SerializeField] protected float _maxMonHp;
    private Transform _tr;
    private PlayerLevelManager _eXP;

    public float MonHp { get { return _monHp; } set { _monHp = value; } }
    public float MonDefend { get { return _monDefend; } set { _monDefend = value; } }
    public float MonSpeed { get { return _monSpeed; } set { _monSpeed = value; } }
    public float MonGold { get { return _monGlod; } set { _monGlod = value; } }
    public float MonExp { get { return _monExp; } set { _monExp = value; } }
    public float MonCastleDam { get { return _monCastleDam; } set { _monCastleDam = value; } }
    public GameObject IsDeadImpack => _IsDeadImpack;
    public GameObject HitImpact => _hitImpact;
    public float MonMaxHp { get => _maxMonHp; set { _maxMonHp = value; } }

    public event Action<BaseEnemy> onRemoved;

    protected virtual void Awake()
    {
        _maxMonHp = _monHp;
        CaCheComponents();
    }
    public override void WakeUp()
    {
        _monHp = _maxMonHp;
        transform.SetParent(null);
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        transform.SetParent(_tr);
        onRemoved?.Invoke(this);
        gameObject.SetActive(false);
    }
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
    protected virtual void CaCheComponents()
    {
        _eXP = GameManager.Instance.PlayerStatus.GetComponent<PlayerLevelManager>();
    }
}
