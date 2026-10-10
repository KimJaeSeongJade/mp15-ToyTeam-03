using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenBullet : Bullet
{
    [SerializeField] private GameObject _slowZone; // 인스펙터에서 SlowZone 프리팹 할당 필수

    // 부모(Bullet)의 OnTriggerEnter를 오버라이드하여 데미지 없이 장판 생성만 처리합니다.
    protected override void OnTriggerEnter(Collider other) 
    { 
        // 💡 [수정] 레이어 체크 대신 상대방에게 IDamageable 인터페이스가 있는지 즉시 검출합니다.
        if (other.TryGetComponent<IDamageable>(out IDamageable damageable)) 
        { 
            // 1. 부딪힌 적의 위치를 파악하고 바닥 높이(Y축) 보정
            Vector3 spawnPosition = other.transform.position;
            Vector3 zonePos = new Vector3(spawnPosition.x, 0.05f, spawnPosition.z);

            // 2. 풀로 들어가기 전에 적 발밑 위치에 장판 프리팹 즉시 생성
            if (_slowZone != null)
            {
                Instantiate(_slowZone, zonePos, Quaternion.identity);
            }

            // 3. 장판 생성이 완료되었으므로 총알은 오브젝트 풀로 복귀
            ReturnToPool();
        } 
    }
}
