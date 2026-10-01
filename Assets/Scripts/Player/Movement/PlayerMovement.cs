using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Transform _attackMuzzle;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;
    [SerializeField] private float _groundStickSpeed = 2f;

    private PlayerInputReader _inputReader;

    private PlayerJump _playerJump;
    private PlayerGroundChecker _groundChecker;

    private float _pitch;
    private Transform _cameraTransform;
    private Rigidbody _rb;

    public Transform GetCamera { get => _cameraTransform;}

    private void Awake() => CacheComponent();

    private void Update()
    {
        Rotate();
    }

    private void LateUpdate()
    {
        Move();
        SetCameraTransform();
    }

    private void SetCameraTransform()
    {
        _cameraTransform.SetPositionAndRotation(_cameraPivot.position, _cameraPivot.rotation);
    }

    public void Move()
    {
        _groundChecker.CheckGround();

        float moveDash =
            _inputReader.isPressedDashKey == true 
            ? _dashSpeed : _moveSpeed;

        Vector3 input = _inputReader.GetMoveNormalInput();

        Vector3 direction = transform.right * input.x + transform.forward * input.z;

        bool isGroundMoving = _groundChecker.IsGrounded && direction.sqrMagnitude > 0.01f;

        if (isGroundMoving)
        {
            direction = Vector3.ProjectOnPlane(direction, _groundChecker.GroundNormal).normalized;
        }

        Vector3 newVelocity = direction * moveDash;

        bool isJumping = _playerJump != null && _playerJump.IsJumping;
        if (!isGroundMoving || isJumping)
        {
            newVelocity.y = _rb.velocity.y;
        }

        if (_groundChecker.IsGrounded && !isJumping && input.sqrMagnitude <= 0.01f)
        {
            newVelocity.y = -_groundStickSpeed;
        }

        _rb.velocity = newVelocity;
    }

    public void Rotate()
    {
        Vector3 input = _inputReader.GetMouseInput() * _mouseSensitivity;

        transform.Rotate(0f, input.y, 0f, Space.Self);

        _pitch = Mathf.Clamp(_pitch + input.x, _minPitch, _maxPitch);

        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, _cameraPivot.localEulerAngles.z);

        _attackMuzzle.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, _cameraPivot.localEulerAngles.z);
    }

    private void CacheComponent()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _playerJump = GetComponent<PlayerJump>();
        _rb = GetComponent<Rigidbody>();

        _cameraTransform = Camera.main.transform;
    }
}
