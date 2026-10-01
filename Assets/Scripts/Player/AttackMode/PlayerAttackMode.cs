using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackMode : MonoBehaviour
{
    [SerializeField] private BasicAttack _basicAttack;
    [SerializeField] private ObjectPool<BasicAttack> _attackPools;
    [SerializeField] private int _initMaxCount;
    [SerializeField] private Transform _migicMuzzle;
    [SerializeField] private float _fireTime;

    private PlayerInputReader _inputReader;
    private PlayerBuildMode _buildMode;

    private float _elapseTime;

    private bool canFire => _elapseTime >= _fireTime;
    private bool isAttackMode = true;


    private void Awake() => CacheComponenet();
    private void Start() => Init();

    // Update is called once per frame
    private void Update()
    {
        FireCoolDown();
        Attack();

        AttackModeEnd();
    }

    private void FireCoolDown()
    {
        if (canFire) return;

        _elapseTime += Time.deltaTime;
    }

    private void Attack()
    {
        if (!canFire || !_inputReader.isPressedAttack) return;

        _basicAttack = _attackPools.Pop();

        _elapseTime = 0;
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

        enabled = isAttackMode;
        _buildMode.enabled = !isAttackMode;
    }

    private void CacheComponenet()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _buildMode = GetComponent<PlayerBuildMode>();
    }

    private void Init()
    {
        enabled = isAttackMode;

        _attackPools = new ObjectPool<BasicAttack>(
            _basicAttack,
            _initMaxCount,
            _migicMuzzle,
            _basicAttack =>
            {
                AttackPoolInit(_basicAttack);
            }
            );

        _elapseTime = _fireTime;
    }

    private void AttackPoolInit(BasicAttack basicAttack)
    {
        basicAttack.InitTransform();
    }
}
