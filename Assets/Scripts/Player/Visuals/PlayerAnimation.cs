using UnityEngine;

// 플레이어 상태를 Animator Controller의 이동 블렌드 트리와 상체 레이어에 전달한다.
[RequireComponent(typeof(PlayerInputReader), typeof(PlayerJump))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    [SerializeField] private string PRAM_MOVE_SPEED = "MoveSpeed";
    [SerializeField] private string PRAM_IS_JUMP = "IsJump";
    [SerializeField] private string PRAM_IS_CHARGE = "IsCharge";
    [SerializeField] private string PRAM_ATTACK = "Attack";
    [SerializeField] private string PRAM_SKILL = "Skill";

    private int _moveSpeedId;
    private int _isJumpId;
    private int _isChargeId;
    private int _attackId;
    private int _skillId;

    private PlayerInputReader _inputReader;
    private PlayerJump _playerJump;

    private bool _chargingRequested;

    private void Awake() => CacheComponent();
    private void Start() => Init();


    private void LateUpdate()
    {
        Vector3 moveInput = _inputReader.GetMoveNormalInput();
        float moveSpeed = moveInput.sqrMagnitude <= 0.01f
            ? 0f
            : _inputReader.isPressedDashKey ? 1f : 0.5f;

        _animator.SetFloat(_moveSpeedId, moveSpeed);
        _animator.SetBool(_isJumpId, _playerJump.IsJumping);
        _animator.SetBool(_isChargeId, _chargingRequested);
        _chargingRequested = false;
    }

    // 공격과 스킬이 같은 프레임에 신호를 보내도 차지 중인 쪽의 요청을 유지한다.
    public void SetCharging(bool charging)
    {
        if (charging)
            _chargingRequested = true;
    }

    public void PlayAttack()
    {
        _chargingRequested = false;
        _animator.SetBool(_isChargeId, false);
        _animator.SetTrigger(_attackId);
    }

    public void PlaySkill()
    {
        _chargingRequested = false;
        _animator.SetBool(_isChargeId, false);
        _animator.SetTrigger(_skillId);
    }

    private void CacheComponent()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _playerJump = GetComponent<PlayerJump>();
    }

    private void Init()
    {
        _moveSpeedId = Animator.StringToHash(PRAM_MOVE_SPEED);
        _isJumpId = Animator.StringToHash(PRAM_IS_JUMP);
        _isChargeId = Animator.StringToHash(PRAM_IS_CHARGE);
        _attackId = Animator.StringToHash(PRAM_ATTACK);
        _skillId = Animator.StringToHash(PRAM_SKILL);
    }
}
