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
    private PlayerSound _sound;
    private IChargeable _chargingAttack;

    private RaycastHit hit;
    private float _elapseTime;
    private float _skillReadyAt;

    private bool canFire => _elapseTime >= _fireTime;
    private bool canSkill => Time.time >= _skillReadyAt;
    private bool isAttackMode = true;

    private void Awake() => CacheComponenet();
    private void Start() => Init();

    private void Update()
    {
        FireCoolDown();

        Attack();

        SkillInit();
        SkillCasting();

        AttackModeEnd();
    }


    private void FireCoolDown()
    {
        if (canFire) return;

        _elapseTime += Time.deltaTime;
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
        {
            _animation.SetCharging(true);
            SoundManager.Instance.Play(_sound.ChargeStart);
        }
        else
        {
            _animation.PlayAttack();
            SoundManager.Instance.Play(attack is SpreadAttack ? _sound.SpreadAttack : _sound.BasicAttack);
        }

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
        SoundManager.Instance.Play(_skillAttack is CycloneAttack ? _sound.CycloneSkill : _sound.Skill);

        _skillReadyAt = Time.time + _skillTime;
        _status.NotifySkillCooldownStarted(_skillTime);
    }

    private void AttackModeEnd()
    {
        if (!_buildMode.SetSelectedTurret()) return;

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
        _sound = GetComponent<PlayerSound>();
    }

    private void Init()
    {
        enabled = isAttackMode;

        SetBasicAttack(_basicAttack);

        _elapseTime = _fireTime;
        _skillReadyAt = Time.time;
    }
}
