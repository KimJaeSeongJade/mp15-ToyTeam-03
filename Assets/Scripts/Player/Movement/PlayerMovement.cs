using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Transform _attackMuzzle;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;
    [SerializeField, Range(0f, 1f)] private float _pitchCameraDistanceScale = 0.55f;
    [SerializeField] private float _aimDistance = 100f;
    [SerializeField] private float _groundStickSpeed = 2f;

    private PlayerInputReader _inputReader;
    private PlayerStatus _status;

    private PlayerJump _playerJump;
    private PlayerGroundChecker _groundChecker;

    private float _pitch;
    private Camera _camera;
    private Transform _cameraTransform;
    private Rigidbody _rb;
    private int _enemyLayer;
    private readonly RaycastHit[] _aimHits = new RaycastHit[32];

    public Transform GetCamera { get => _cameraTransform;}

    private void Awake() => CacheComponent();
    private void Update() => Rotate();
    private void LateUpdate()
    {
        Move();
        SetCameraTransform();
        UpdateAttackAim();
    }

    private void SetCameraTransform()
    {
        float maxPitch = Mathf.Max(Mathf.Abs(_minPitch), Mathf.Abs(_maxPitch));
        float pitchAmount = Mathf.InverseLerp(0f, maxPitch, Mathf.Abs(_pitch));
        float cameraDepth = Mathf.Lerp(
            _cameraPivot.localPosition.z,
            _cameraPivot.localPosition.z * _pitchCameraDistanceScale,
            pitchAmount);

        Vector3 eyePosition = transform.TransformPoint(new Vector3(0f, _cameraPivot.localPosition.y, 0f));
        Vector3 shoulderOffset = new Vector3(_cameraPivot.localPosition.x, 0f, cameraDepth);
        _cameraTransform.SetPositionAndRotation(
            eyePosition + _cameraPivot.rotation * shoulderOffset,
            _cameraPivot.rotation);
    }

    // 화면 중앙이 가리키는 지점을 AttackTR 위치에서 바라본다.
    public void UpdateAttackAim()
    {
        Ray aimRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 aimPoint = aimRay.GetPoint(_aimDistance);
        float nearestDistance = _aimDistance;
        bool hasAttackTarget = false;

        int hitCount = Physics.RaycastNonAlloc(
            aimRay, _aimHits, _aimDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _aimHits[i];
            if (hit.collider.transform.IsChildOf(transform) || hit.distance >= nearestDistance)
                continue;

            nearestDistance = hit.distance;
            aimPoint = hit.point;
            hasAttackTarget = hit.collider.gameObject.layer == _enemyLayer;
        }

        _attackMuzzle.rotation = Quaternion.LookRotation(aimPoint - _attackMuzzle.position, transform.up);
        _status.SetAttackTarget(hasAttackTarget);
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
    }

    private void CacheComponent()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _status = GetComponent<PlayerStatus>();
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _playerJump = GetComponent<PlayerJump>();
        _rb = GetComponent<Rigidbody>();
        _enemyLayer = LayerMask.NameToLayer("Enemy");

        _camera = Camera.main;
        _cameraTransform = _camera.transform;
    }
}
