using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackMode : MonoBehaviour
{
    [SerializeField] private BasicAttack _basicAttack;
    [SerializeField] private ObjectPool<BasicAttack> _attackPools;
    [SerializeField] private SkillAttack _skillAttack;
    [SerializeField] private int _initMaxCount;
    [SerializeField] private Transform _magicMuzzle;
    [SerializeField] private GameObject _skillIndicator;
    [SerializeField] private float _fireTime;
    [SerializeField] private float _skillTime;
    [SerializeField] private float _skillRandge;

    private PlayerInputReader _inputReader;
    private PlayerStatus _status;
    private PlayerBuildMode _buildMode;
    private PlayerAnimation _animation;
    private IChargeable _chargingAttack;

    private RaycastHit hit;
    private float _elapseTime;
    private float _elapseSkillTime;

    private bool canFire => _elapseTime >= _fireTime;
    private bool canSkill => _elapseSkillTime >= _skillTime;
    private bool isAttackMode = true;

    private void Awake() => CacheComponenet();
    private void Start() => Init();

    private void Update()
    {
        FireCoolDown();
        SkillCoolDown();

        Attack();

        SkillInit();
        SkillCasting();

        AttackModeEnd();
    }


    // TODO: 스킬 구현
    private void FireCoolDown()
    {
        if (canFire) return;

        _elapseTime += Time.deltaTime;
    }

    private void SkillCoolDown()
    {
        if (canSkill) return;

        _elapseSkillTime += Time.deltaTime;
    }

    private void Attack()
    {
        if (_chargingAttack != null)
        {
            if (_chargingAttack.IsCharging)
            {
                _animation.SetCharging(true);
                return;
            }
            _animation.SetCharging(false);
            _animation.PlayAttack();
            _chargingAttack = null;
        }

        if (!canFire || !_inputReader.isPressedAttack) return;

        BasicAttack attack = _attackPools.Pop();
        _chargingAttack = attack as IChargeable;
        if (_chargingAttack != null)
            _animation.SetCharging(true);
        else
            _animation.PlayAttack();

        _elapseTime = 0;
    }

    private void SkillCasting()
    {
        if (!canSkill || !_inputReader.isPressedSkill)
        {
            _animation.SetCharging(false);
            _skillIndicator.SetActive(false);
            return;
        }

        _animation.SetCharging(true);

        Vector3 centerPoint = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = Camera.main.ScreenPointToRay(centerPoint);

        if (Physics.Raycast(ray, out hit, _skillRandge, LayerMask.GetMask("Ground")))
        {
            _skillIndicator.transform.position = hit.point + Vector3.up * 0.03f;
            _skillIndicator.SetActive(true);

#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * _skillRandge, Color.red);
#endif
        }
        else
        {
            _skillIndicator.SetActive(false);
        }
    }
    private void SkillInit()
    {
        if (!canSkill || !_inputReader.isPressedSkillUp || !_skillIndicator.activeInHierarchy) return;

        Instantiate(_skillAttack, hit.point, Quaternion.identity);
        _animation.SetCharging(false);
        _animation.PlaySkill();

        _elapseSkillTime = 0;
    }

    // TODO: 추후 변경 가능성 있음. 키 입력 구독 처리로 refac 예정
    public void SetSelectedTurret()
    {
        if (!_inputReader.isPressedPrimary
            && !_inputReader.isPressedSub
            && !_inputReader.isPressedThird
            && !_inputReader.isPressedForth) return;

        if (_inputReader.isPressedPrimary)
        {
            _buildMode.SetSelectedTurret(0);
        }
        else if (_inputReader.isPressedSub)
        {
            _buildMode.SetSelectedTurret(1);

        }
        else if (_inputReader.isPressedThird)
        {
            _buildMode.SetSelectedTurret(2);

        }
        else if (_inputReader.isPressedForth)
        {
            _buildMode.SetSelectedTurret(3);
        }
    }

    private void AttackModeEnd()
    {
        if (!_inputReader.isPressedPrimary
            && !_inputReader.isPressedSub
            && !_inputReader.isPressedThird
            && !_inputReader.isPressedForth) return;

        SetSelectedTurret();

        isAttackMode = false;

        _status.PlayerModeChange(false);

        enabled = isAttackMode;
        _buildMode.enabled = !isAttackMode;
    }

    public void LevelUpMultiply(float atkMultiply, float skillMultiply)
    {
        _fireTime *= atkMultiply;
        _skillTime *= skillMultiply;
    }

    // 이전 풀은 보관하고, 공격이 바뀔 때마다 새 프리팹으로 풀을 만든다.
    public void SetBasicAttack(BasicAttack newBasicAttack)
    {
        _basicAttack = newBasicAttack;

        _attackPools = new ObjectPool<BasicAttack>(
            newBasicAttack,
            _initMaxCount,
            _magicMuzzle,
            attack =>
            {
                // 풀이 부족해 나중에 생성된 탄환도 총구에서 출발하고 이곳으로 반환된다.
                attack.transform.SetParent(_magicMuzzle, false);
                attack.InitTransform();
            });
    }

    public void SetSkillAttack(SkillAttack newSkillAttack)
    {
        _skillAttack = newSkillAttack;
    }

    private void CacheComponenet()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _status = GetComponent<PlayerStatus>();
        _buildMode = GetComponent<PlayerBuildMode>();
        _animation = GetComponent<PlayerAnimation>();
    }

    private void Init()
    {
        enabled = isAttackMode;

        SetBasicAttack(_basicAttack);

        _elapseTime = _fireTime;
        _elapseSkillTime = _skillTime;
    }
}
