using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tire2GreedyFireTurret : AttackTurret
{
    [Header("Greedy Stack Settings (발사 횟수당 공격력 증가)")]
    [SerializeField] private float _damageBonusPerShot = 1f; // 1발 발사할 때마다 누적되는 추가 공격력
    [SerializeField] private float _maxDamageBonus = 20f;    // 공격력 증가 최대 상한선
    [SerializeField] private float _stackDecayDelay = 2.0f;  // 이 시간(초) 동안 공격을 안 하면 식기 시작함
    [SerializeField] private float _stackDecaySpeed = 5f;    // 식을 때 초당 감소하는 대미지 수치

    [SerializeField]private float _currentDamageBonus = 0f; // 현재 누적된 추가 대미지 보너스 (총알이 읽어감)
    private float _lastAttackTime;

    // 💡 총알이 피격 시점에 나의 누적 대미지를 실시간으로 조회할 수 있도록 열어둡니다.
    public float CurrentDamageBonus => _currentDamageBonus;

    protected override void Update()
    {
        base.Update();
        // 💡 [쿨타임 시스템] 지정된 대기시간 동안 공격이 멈추면 누적 버프가 서서히 감소합니다.
        CoolingTurret();
    }

    

    // 💡 부모의 Attack 추상/가상 함수를 오버라이드합니다.
    public override void Attack()
    {
        // 💡 기획 요건에 맞게 터렛은 순수하게 총알 생성(또는 기존 풀링 규격 사용) 역할만 수행합니다.
        // ※ 기존 프로젝트의 풀링 함수나 오브젝트 생성 로직을 이 자리에 대입하세요.
        GreedyBullet bulletInstance = null; 

        if (bulletInstance != null)
        {
            // 💡 생성 직후 총알에게 주인 터렛이 '나(this)'임을 알려주어 연결 고리만 형성합니다.
            bulletInstance.SetOwner(this);
        }

        // 💡 발사 횟수를 카운트하기 위해 시간 기록 및 스택 축적
        _lastAttackTime = Time.time;

        if (_currentDamageBonus < _maxDamageBonus)
        {
            _currentDamageBonus += _damageBonusPerShot;
            _currentDamageBonus = Mathf.Min(_currentDamageBonus, _maxDamageBonus); // 상한선 제한
            if (_bullets != null && _muzzlePoint != null)
            {
                // 총알 오브젝트를 생성합니다.
                /*GameObject bulletObj = Instantiate(_bullets, _muzzlePoint.position, _muzzlePoint.rotation);

                // 생성된 총알 오브젝트에서 Bullet 컴포넌트를 가져옵니다.
                Bullet bulletScript = bulletObj.GetComponent<Bullet>();

                if (bulletScript != null)
                {
                    // 💡 2. 서포트 타워로 인해 실시간 변동된 최종 공격력(this.Damage)을 총알에 세팅합니다.
                    bulletScript.SetDamage(this.Damage);

                    // 총알이 날아갈/추적할 타겟을 지정해 줍니다.
                    bulletScript.SetTarget(_target);
                }*/
                _bullets = _bulletPool.Pop();
                (_bullets as GreedyBullet).SetOwner(this);
            }
            Debug.Log($"[부익부 빈익빈] 총알 생성 완료! 현재 누적 버프: +{_currentDamageBonus} (상한: {_maxDamageBonus})");
        }
    }

    private void CoolingTurret()
    {
        if (Time.time >= _lastAttackTime + _stackDecayDelay)
        {
            if (_currentDamageBonus > 0f)
            {
                _currentDamageBonus -= _stackDecaySpeed * Time.deltaTime;
                _currentDamageBonus = Mathf.Max(_currentDamageBonus, 0f); // 0 아래로 방어
            }
        }
    }
}

