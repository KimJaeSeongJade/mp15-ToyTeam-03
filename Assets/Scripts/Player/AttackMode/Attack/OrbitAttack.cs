using UnityEngine;

// 본 탄환이 풀로 돌아가도 발사 시 저장한 중심 궤도를 따라 독립적으로 이동한다.
public class OrbitAttack : BasicAttack
{
    private Vector3 _centerAtLaunch;
    private Quaternion _launchRotation;
    private float _radius;
    private float _rearOffset;
    private float _spinSpeed;
    private float _initialAngle;
    private float _elapsed;

    public void SetOrbit(Vector3 center, Quaternion rotation, float radius, float rearOffset,
        float spinSpeed, float initialAngle, Transform returnAnchor)
    {
        _centerAtLaunch = center;
        _launchRotation = rotation;
        _radius = radius;
        _rearOffset = rearOffset;
        _spinSpeed = spinSpeed;
        _initialAngle = initialAngle;
        _elapsed = 0f;

        transform.SetParent(returnAnchor, false);
        InitTransform();
        transform.SetParent(null);
        transform.SetPositionAndRotation(GetOrbitPosition(0f), rotation);
        ResetTravelStart();
    }

    protected override Vector3 GetNextPosition(float deltaTime)
    {
        _elapsed += deltaTime;
        return GetOrbitPosition(_elapsed);
    }

    private Vector3 GetOrbitPosition(float elapsed)
    {
        Vector3 forward = _launchRotation * Vector3.forward;
        Vector3 center = _centerAtLaunch + forward * (MoveSpeed * elapsed);
        Vector3 radial = _launchRotation *
            (Quaternion.AngleAxis(_initialAngle + _spinSpeed * elapsed, Vector3.forward) * Vector3.right);
        return center - forward * _rearOffset + radial * _radius;
    }
}
