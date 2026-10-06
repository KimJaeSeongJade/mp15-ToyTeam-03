using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : PoolObject
{
    [SerializeField] private int _goldAmount = 50; // 이 골드의 가치
    public int GoldAmount => _goldAmount; // 💡 외부에서 읽어갈 프로퍼티

    public override void WakeUp()
    {
        gameObject.SetActive(true);
        // 필요 시 물리 효과 리셋, 트레일 이펙트 초기화 등 처리
    }

    public override void Sleep()
    {
        gameObject.SetActive(false);
    }

    // 플레이어가 골드를 자석으로 끌고 와서 획득했거나, 시간이 지나서 소멸할 때 호출
    public void CollectOrDestroy()
    {
        // 💡 부모(PoolObject)의 ReturnToPool을 호출하면 
        // 셋업해둔 returnToPool?.Invoke(this)가 실행되어 풀로 안전하게 들어갑니다.
        ReturnToPool();
    }
}
