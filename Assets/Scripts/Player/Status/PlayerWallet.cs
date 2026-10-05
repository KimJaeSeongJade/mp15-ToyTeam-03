using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int _gold;

    // 플레이어에게 배달 완료된 골드의 현재 잔액을 반환한다.
    public int Gold => _gold;

    // 골드 잔액이 바뀔 때 새 잔액을 전달한다. UI는 이 이벤트를 구독해 표시를 갱신한다.
    // UI 활성화 시에는 Gold로 초기 값을 표시하고, 비활성화 시 구독을 해제한다.
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        //GameManager.Instance._playerWallet = this;
    }

    // 골드 오브젝트가 플레이어에게 도착했을 때 호출한다. 잔액을 더하고 UI에 알린다.
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        _gold += amount;
        OnGoldChanged?.Invoke(_gold);

        Debug.Log($"+{amount}골드 획득. 총 {_gold}골드");
    }

    // 잔액이 충분하면 골드를 차감하고 true를 반환한다. 부족하거나 음수이면 false다.
    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || _gold < amount) return false;
        if (amount == 0) return true;

        _gold -= amount;
        OnGoldChanged?.Invoke(_gold);
        Debug.Log($"-{amount}골드 감소. 총 {_gold}골드");
        return true;
    }

    public bool TryGetBoolSpendGold(int amount)
    {
        if (amount < 0 || _gold < amount) return false;

        return true;
    }
}
