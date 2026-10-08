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
        // 중복으로 배열에 담기지 않아야 함
        // 탐지된 적들에서 IDamgable 인터페이스로 _slowRate만큼 이동속도 줄이기
        // _zoneLifeTime이 지난 후에 _slowRate만큼 줄인 이동속도 돌려주기
        // 오브젝트 삭제
    }
}
