using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    // 데미지를 받는 메서드 (받을 데미지 수치를 매개변수로 받음)
    void TakeDamage(float damage);
    void SlowSpeed(float speed);

    // TODO MonsterHealth 내 추가할 내용
    // bool CheckExecution(float thresholdRatio);
}
