using UnityEngine;

[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerJump : MonoBehaviour
{
    private PlayerInputReader _inputReader;
    private PlayerGroundChecker _groundChecker;
    private PlayerSound _sound;
    private bool _wasGrounded = true;

    [SerializeField] private float _jumpForece;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _bufferTime = 0.1f;
    [SerializeField] private float _groundedAfterJumpIgnoreTime = 0.12f;

    private Rigidbody _rb;
    private bool _jumpConsumed;
    private float _coyoteElapseTimer;
    private float _bufferElapseTimer;
    private float _groundedAfterJumpIgnoreTimer;

    public bool IsJumping { get; private set; }

    private bool _canCoyote => _coyoteElapseTimer > 0f;
    private bool _canBuffer => _bufferElapseTimer > 0f;
    private bool _isCanJump => _canCoyote && _canBuffer && !_jumpConsumed;

    private void Awake() => CacheComponent();
    private void Update() => JumpBuffer();
    private void FixedUpdate()
    {
        _groundChecker.CheckGround();
        UpdateJumpIgnoreTimer();
        ResetJumpWhenLanded();
        CoyoteTime();
        JumpCorrection();
    }

    private void ExecuteJump()
    {
        _jumpConsumed = true;
        IsJumping = true;
        _coyoteElapseTimer = 0f;
        _bufferElapseTimer = 0f;
        _groundedAfterJumpIgnoreTimer = _groundedAfterJumpIgnoreTime;

        Vector3 velocity = _rb.velocity;
        velocity.y = 0f;
        _rb.velocity = velocity;

        _rb.AddForce(Vector3.up * _jumpForece, ForceMode.Impulse);
        _wasGrounded = false;
        SoundManager.Instance.Play(_sound.Jump);
    }

    private void CoyoteTime()
    {
        if (_groundChecker.IsGrounded && !IsJumping)
        {
            _coyoteElapseTimer = _coyoteTime;
            return;
        }

        if (_coyoteElapseTimer <= 0f) return;

        _coyoteElapseTimer -= Time.fixedDeltaTime;
    }

    private void JumpBuffer()
    {
        if (_inputReader.isPressedJumpKey)
        {
            _bufferElapseTimer = _bufferTime;
            return;
        }

        if (_bufferElapseTimer <= 0f) return;

        _bufferElapseTimer -= Time.deltaTime;
    }

    private void JumpCorrection()
    {
        if (!_isCanJump) return;

        ExecuteJump();
    }

    private void ResetJumpWhenLanded()
    {
        if (_groundedAfterJumpIgnoreTimer > 0f) return;
        if (!_groundChecker.IsGrounded)
        {
            _wasGrounded = false;
            return;
        }
        if (!_wasGrounded) SoundManager.Instance.Play(_sound.Land);
        _wasGrounded = true;

        IsJumping = false;
        _jumpConsumed = false;
    }

    private void UpdateJumpIgnoreTimer()
    {
        if (_groundedAfterJumpIgnoreTimer <= 0f) return;

        _groundedAfterJumpIgnoreTimer -= Time.fixedDeltaTime;
    }

    private void CacheComponent()
    {
        _rb = GetComponent<Rigidbody>();
        _inputReader = GetComponent<PlayerInputReader>();
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _sound = GetComponent<PlayerSound>();

        if (_groundChecker == null)
        {
            _groundChecker = gameObject.AddComponent<PlayerGroundChecker>();
        }
    }
}
