using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [SerializeField] private float _slowRange;
    [SerializeField] private LayerMask _target;
    [SerializeField] private float _slowRate;
    [SerializeField] private float _zoneLifeTime;
    
    private List<IDamageable> _affectedTarget = new List<IDamageable>();
    private void Start()
    {
        SlowEffect();
    }

    private void SlowEffect()
    {
        // 오브젝트가 생성이 될 때 _slowRange만큼의 넓이로 레이어마스크로 타겟을 탐지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _slowRange, _target);
        // 리스트로 담기
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].TryGetComponent<IDamageable>(out IDamageable target))
            {
                if (!_affectedTarget.Contains(target))
                {
                    _affectedTarget.Add(target);
                    
                    target.SlowSpeed(_slowRate);
                }
            }
        }

        StartCoroutine(DurationRoutine());
        // 중복으로 배열에 담기지 않아야 함
        // 탐지된 적들에서 IDamgable 인터페이스로 _slowRate만큼 이동속도 줄이기
        // _zoneLifeTime이 지난 후에 _slowRate만큼 줄인 이동속도 돌려주기
        // 오브젝트 삭제
    }
    
    private IEnumerator DurationRoutine()
    {
        // _zoneLifeTime이 지난 후에
        yield return new WaitForSeconds(_zoneLifeTime);

        // _slowRate만큼 줄인 이동속도 돌려주기
        for (int i = 0; i < _affectedTarget.Count; i++)
        {
            IDamageable target = _affectedTarget[i];

            // 대기 시간 도중 적이 먼저 파괴(사망)되었을 수 있으므로 null 체크
            if (target != null)
            {
                // 💡 [해결 방법] 인터페이스에 복구 함수가 없다면, 
                // 곱해서 줄였던 비율을 수학적으로 원래대로 돌리는 역산 값을 적용해 줍니다.
                // 예: 50% 줄였다면(0.5), 다시 원래대로 돌리기 위한 역산 비율을 계산해 전달합니다.
                float restoreRate = -_slowRate / (1f - _slowRate);
                target.SlowSpeed(restoreRate); 
            }
        }

        _affectedTarget.Clear();

        // 오브젝트 삭제
        Destroy(gameObject);
    }
}
