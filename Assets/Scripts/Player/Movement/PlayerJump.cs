
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private InputActionReference _jumpAction;

    private PlayerGroundChecker _groundChecker;
    private PlayerSound _sound;
    private Rigidbody _rb;

    private bool _wasGrounded = true;

    [SerializeField] private float _jumpForece = 5f;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _bufferTime = 0.1f;
    [SerializeField] private float _groundedAfterJumpIgnoreTime = 0.12f;

    private bool _jumpConsumed;
    private float _coyoteElapseTimer;
    private float _bufferElapseTimer;
    private float _groundedAfterJumpIgnoreTimer;

    public bool IsJumping { get; private set; }

    private bool _canCoyote => _coyoteElapseTimer > 0f;
    private bool _canBuffer => _bufferElapseTimer > 0f;
    private bool _isCanJump => _canCoyote && _canBuffer && !_jumpConsumed;

    private void Awake() => CacheComponent();
    private void OnEnable()
    {
        _jumpAction.action.Enable();
        _jumpAction.action.performed += OnJump;
    }

    private void FixedUpdate()
    {
        _groundChecker.CheckGround();

        UpdateJumpIgnoreTimer();
        ResetJumpWhenLanded();
        CoyoteTime();
        JumpBuffer();
        ExecuteJump();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _bufferElapseTimer = _bufferTime;
    }

    private void ExecuteJump()
    {
        if (!_isCanJump) return;

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

        _coyoteElapseTimer = Mathf.Max(0f, _coyoteElapseTimer - Time.fixedDeltaTime);
    }

    private void JumpBuffer()
    {
        if (_bufferElapseTimer <= 0f) return;

        _bufferElapseTimer = Mathf.Max(0f, _bufferElapseTimer - Time.fixedDeltaTime);
    }

    private void ResetJumpWhenLanded()
    {
        if (_groundedAfterJumpIgnoreTimer > 0f) return;

        if (!_groundChecker.IsGrounded)
        {
            _wasGrounded = false;
            return;
        }

        if (!_wasGrounded)
        {
            SoundManager.Instance.Play(_sound.Land);
        }

        _wasGrounded = true;

        IsJumping = false;
        _jumpConsumed = false;
    }

    private void UpdateJumpIgnoreTimer()
    {
        if (_groundedAfterJumpIgnoreTimer <= 0f) return;

        _groundedAfterJumpIgnoreTimer = Mathf.Max( 0f, _groundedAfterJumpIgnoreTimer - Time.fixedDeltaTime);
    }

    private void CacheComponent()
    {
        _rb = GetComponent<Rigidbody>();
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _sound = GetComponent<PlayerSound>();
    }
}
