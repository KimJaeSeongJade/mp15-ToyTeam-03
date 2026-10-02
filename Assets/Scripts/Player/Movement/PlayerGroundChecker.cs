using UnityEngine;

public class PlayerGroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _groundCheckRadius = 0.45f;
    [SerializeField] private float _groundCheckDistance = 0.25f;
    [SerializeField] private float _groundCheckOffset = 0.1f;

    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;

    private void Awake()
    {
        if (_groundMask.value == 0)
        {
            _groundMask = LayerMask.GetMask("Default", "Ground");
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
    }

    public void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * (_groundCheckRadius + _groundCheckOffset);
        IsGrounded = Physics.SphereCast(
            origin,
            _groundCheckRadius,
            Vector3.down,
            out RaycastHit hit,
            _groundCheckDistance,
            _groundMask,
            QueryTriggerInteraction.Ignore);

        GroundNormal = IsGrounded ? hit.normal : Vector3.up;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * (_groundCheckRadius + _groundCheckOffset);
        Vector3 end = origin + Vector3.down * _groundCheckDistance;

        Gizmos.DrawWireSphere(origin, _groundCheckRadius);
        Gizmos.DrawWireSphere(end, _groundCheckRadius);
        Gizmos.DrawLine(origin, end);
    }
#endif
}
