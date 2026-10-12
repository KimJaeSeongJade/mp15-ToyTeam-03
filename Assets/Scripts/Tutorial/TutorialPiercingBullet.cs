using UnityEngine;

// 튜토리얼 관통탄만 사용한다. 발사 후에는 타겟의 위치/생존 상태를 추적하지 않는다.
public class TutorialPiercingBullet : PiercingBullet
{
    [SerializeField, Min(0f)] private float _flightSpeed = 15f;
    private Vector3 _launchDirection;

    public void Aim(Vector3 targetPosition)
    {
        _launchDirection = (targetPosition - transform.position).normalized;
        if (_launchDirection.sqrMagnitude == 0f) _launchDirection = transform.forward;
        transform.forward = _launchDirection;
    }

    protected override void Move()
    {
        transform.position += _launchDirection * (_flightSpeed * Time.deltaTime);
    }
}
