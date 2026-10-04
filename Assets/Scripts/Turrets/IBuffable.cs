using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuffable
{
    // 서포트 타워가 호출하여 버프 수치를 더하거나 빼줄 함수
    void ApplyDamageBuff(float buffAmount);
}
