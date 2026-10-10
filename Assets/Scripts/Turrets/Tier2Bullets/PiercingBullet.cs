using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingBullet : Bullet
{
    private Vector3 _shootDirection; // 발사 순간 고정된 이동 방향
    private bool _isDirCached = false; // 방향이 이미 캐싱되었는지 체크

    // 💡 [핵심 1] 부모의 WakeUp을 확장하여 풀에서 깨어나는 시점에 방향을 꽉 잡아둡니다.
    public override void WakeUp()
    {
        base.WakeUp();
        _isDirCached = false; // 풀에서 재사용될 때 플래그 초기화
    }

    // 💡 [핵심 2] Move()를 재정의하여 발사 첫 프레임에만 방향을 구하고, 이후에는 직진시킵니다.
    protected override void Move()
    {
        // 1. 발사 순간(첫 프레임)에 한 번만 타겟을 향한 방향을 고정 저장합니다.
        if (!_isDirCached)
        {
            if (_target != null)
            {
                // 타겟 위치와 내 위치로 방향을 구한 뒤, Y축 높이 변화를 배제하고 정규화합니다.
                _shootDirection = (_target.position - transform.position);
                _shootDirection.y = 0f;
                _shootDirection.Normalize();

                // 총알이 이동 방향을 바라보도록 정렬
                if (_shootDirection != Vector3.zero)
                {
                    transform.forward = _shootDirection;
                }
            }
            else
            {
                // 혹시라도 타겟이 없는 특수 상황이라면 현재 정면을 방향으로 잡습니다.
                _shootDirection = transform.forward;
            }

            _isDirCached = true; // 방향 고정 완료 플래그 활성화
        }

        // 2. 부모의 speed가 private이므로, 부모의 Move() 가드 내부 로직을 역이용하거나 
        // _target을 강제로 null로 밀어 부모의 '직진 연산 코드'를 안전하게 수행시킵니다.
        // 이 방식을 쓰면 부모의 private speed 변수 값을 고스란히 쓰면서 직진 유도가 완성됩니다.
        _target = null; 
        base.Move(); 
    }

    // 💡 [핵심 3] 관통 탄환이므로 적을 뚫고 지나가야 합니다. 
    // 부모의 OnTriggerEnter(맞으면 풀로 복귀)를 오버라이드하여 복귀를 막습니다.
    protected override void OnTriggerEnter(Collider other)
    {
        // 1. 레이어마스크 조건에 부합하는지 확인
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // 2. 상대방에게 IDamageable 인터페이스가 있는지 컴포넌트 추출 시도
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                // 3. 데미지 로직 실행 (적 타격)
                DamageLogic(damageable);
                
                // 💡 [관통 메커니즘] 부모와 달리 ReturnToPool()을 호출하지 않으므로, 
                // 총알이 파괴되지 않고 적을 그대로 관통하며 직선 경로상의 모든 적을 타격합니다.
            }
        }
    }
}
