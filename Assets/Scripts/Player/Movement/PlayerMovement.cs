using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Transform _attackMuzzle;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;
    [SerializeField] private float _groundStickSpeed = 2f;

    private PlayerInputReader _inputReader;
    private PlayerStatus _status;

    private PlayerJump _playerJump;
    private PlayerGroundChecker _groundChecker;

    private float _pitch;
    private Vector3 _cameraOffset;
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
        _cameraTransform.SetPositionAndRotation(
            _cameraPivot.TransformPoint(_cameraOffset),
            _cameraPivot.rotation);
    }

    public void Move()
    {
        _groundChecker.CheckGround();

        float moveSpeed = _inputReader.isPressedDashKey
            ? _status.DashSpeed : _status.MoveSpeed;

        Vector3 input = _inputReader.GetMoveNormalInput();

        Vector3 direction = transform.right * input.x + transform.forward * input.z;

        bool isGroundMoving = _groundChecker.IsGrounded && direction.sqrMagnitude > 0.01f;

        if (isGroundMoving)
        {
            direction = Vector3.ProjectOnPlane(direction, _groundChecker.GroundNormal).normalized;
        }

        Vector3 newVelocity = direction * moveSpeed;

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
        // 기존 프리팹/씬의 옆·뒤 거리만 카메라 오프셋으로 보관하고,
        // 상하 회전 중심은 플레이어의 정면 축 위로 옮긴다.
        Vector3 pivotPosition = _cameraPivot.localPosition;
        _cameraOffset = new Vector3(pivotPosition.x, 0f, pivotPosition.z);
        _cameraPivot.localPosition = new Vector3(0f, pivotPosition.y, 0f);

        _inputReader = GetComponent<PlayerInputReader>();
        _status = GetComponent<PlayerStatus>();
        if (_status == null)
        {
            _status = gameObject.AddComponent<PlayerStatus>();
        }
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _playerJump = GetComponent<PlayerJump>();
        _rb = GetComponent<Rigidbody>();

        _cameraTransform = Camera.main.transform;
    }
}
