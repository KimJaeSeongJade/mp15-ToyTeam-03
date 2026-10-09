using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster : BaseEnemy
{
    [SerializeField] private float _damage;
    [SerializeField] private LayerMask _TurretMask;

    [Header("공격 퍼센트 예 1.7f면 70% 공력력 업")]
    [SerializeField] private float _upDamage;

    private BossMove _move;
    private BossAnim _bossAnim;
    private MonsterHealth _hP;
    private Transform _targetTurret;
    private bool _isTurretInSight;
    private bool _isIgnoreTurret;
    private float _originalDamage;
    private int _phase = 1;

    protected override void Awake()
    {
        base.Awake();
        CaCheComponents();
        _originalDamage = _damage;
    }

    private void Update()
    {
        ReturnCheck();
        Phase();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_TurretMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (_isTurretInSight == false)
            {
                _targetTurret = other.transform;
                _isTurretInSight = true;
                Debug.Log("터렛 발견");

                _move.MoveToTurret(_targetTurret);
                StartCoroutine(Delay());
            }
        }

    }
    private void ReturnCheck()
    {
        if (_isTurretInSight == false || _targetTurret != null) return;
        _isTurretInSight = false;
        _targetTurret = null;
        _move.ReturnMove();
    }

    public void Attack()
    {
        if (!_isTurretInSight) return;
        if (_isIgnoreTurret) return;
        if (_targetTurret == null) return;

        Debug.Log("공격");
        IDamageableturret damageable = _targetTurret.GetComponent<IDamageableturret>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            Debug.Log($"{_damage} 데미지 줌");
        }

    }
    private void Phase()
    {
        if (_hP == null) return;
        // hp가 없으면 리턴
        if (_hP.MaxHp <= 0) return;
        float HpPersent = _hP.CurrentHp/ _hP.MaxHp;

        //70% 공격력 증가
        if(HpPersent<=0.7f && _phase == 1)
        {
            _phase = 2;
            _damage = _originalDamage * _upDamage;
            Debug.Log("공격력 강화");
            _bossAnim.AttackNum(1);
        }
        //50% 이동속도 증가
        if(HpPersent <= 0.5f  && _phase == 2)
        {
            _phase = 3;
            _move.UpSpeed();
            _bossAnim.Run();
            Debug.Log("이동속도 강화");
        }
        if(HpPersent <=0.3f && _phase == 3)
        {
            _isIgnoreTurret = true;
            _isTurretInSight = false;
            _targetTurret = null;

            _move.TurretIngnore();
            Debug.Log("터렛 무시");
        }
    }

    protected override void ResetWakeUp()
    {
        base.ResetWakeUp();

        _damage = _originalDamage;
        _targetTurret = null;
        _isTurretInSight = false;
        _isIgnoreTurret = false;
    }
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        _move.MoveStop(_targetTurret);
    }
    protected override void CaCheComponents()
    {
        base.CaCheComponents();
        _move = GetComponent<BossMove>();
        _hP = GetComponent<MonsterHealth>();
        _bossAnim = GetComponent<BossAnim>();
    }

}
