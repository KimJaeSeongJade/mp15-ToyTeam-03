using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private int _level = 1;
    [SerializeField] private float _exp;
    [SerializeField] private float _atkPower;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashSpeed = 10f;

    private PlayerLevelManager _levelManager;
    private PlayerAttackMode _attackMode;
    private PlayerBuildMode _buildMode;

    // 다음 레벨까지 필요한 경험치. 계산은 PlayerLevelManager가 담당하며 최대 레벨에서는 0이다.
    public float RequiredExp => _levelManager.RequiredExp;

    // 플레이어 상태 변경을 알리는 이벤트. UI는 필요한 이벤트를 구독해 표시를 갱신한다.
    public event Action<int> OnLevelChanged;
    // 현재 경험치와 다음 레벨 필요 경험치를 함께 전달한다. 레벨 변경 시에도 호출된다.
    public event Action<float, float> OnExpChanged;
    public event Action<float> OnAttackPowerChanged;
    public event Action<float> OnMoveSpeedChanged;
    public event Action<float> OnDashSpeedChanged;

    /// <summary>
    /// True : Attack 모드 / False : Build 모드
    /// </summary>
    public event Action<bool> OnPlayerModeChanged;

    private void Awake() => CacheComponenet();

    public int Level
    {
        get => _level;
        internal set
        {
            if (_level == value) return;
            _level = value;
            OnLevelChanged?.Invoke(_level);
            OnExpChanged?.Invoke(_exp, RequiredExp);
        }
    }

    public float Exp
    {
        get => _exp;
        internal set
        {
            if (_exp == value) return;
            _exp = value;
            OnExpChanged?.Invoke(_exp, RequiredExp);
        }
    }

    public float AttackPower
    {
        get => _atkPower;
        set
        {
            if (_atkPower == value) return;
            _atkPower = value;
            OnAttackPowerChanged?.Invoke(_atkPower);
        }
    }

    public float MoveSpeed
    {
        get => _moveSpeed;
        set
        {
            if (_moveSpeed == value) return;
            _moveSpeed = value;
            OnMoveSpeedChanged?.Invoke(_moveSpeed);
        }
    }

    public float DashSpeed
    {
        get => _dashSpeed;
        set
        {
            if (_dashSpeed == value) return;
            _dashSpeed = value;
            OnDashSpeedChanged?.Invoke(_dashSpeed);
        }
    }

    public void GetExp(float amount)
    {
        _levelManager.GainExp(amount);
    }

    public void PlayerModeChange(bool value)
    {
        OnPlayerModeChanged?.Invoke(value);
    }

    private void CacheComponenet()
    {
        _levelManager = GetComponent<PlayerLevelManager>();
        _attackMode = GetComponent<PlayerAttackMode>();
        _buildMode = GetComponent<PlayerBuildMode>();
    }
}
