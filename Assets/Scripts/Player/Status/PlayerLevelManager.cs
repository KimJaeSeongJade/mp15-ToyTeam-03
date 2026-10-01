using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
public class PlayerLevelManager : MonoBehaviour
{
    [SerializeField] private float[] _requiredExpPerLevel = { 100f, 150f, 225f, 300f };

    private PlayerStatus _status;

    // 경험치 요구량 배열을 기준으로 도달 가능한 마지막 레벨을 반환한다.
    public int MaxLevel => (_requiredExpPerLevel?.Length ?? 0) + 1;
    // 현재 레벨이 마지막 레벨인지 확인한다.
    public bool IsMaxLevel => _status != null && _status.Level >= MaxLevel;
    // 현재 레벨에서 다음 레벨까지 필요한 경험치를 반환한다. 최대 레벨에서는 0이다.
    public float RequiredExp => IsMaxLevel 
        || _status == null
        || _status.Level < 1
        || _requiredExpPerLevel == null
        || _requiredExpPerLevel.Length == 0 ? 0 : _requiredExpPerLevel[_status.Level - 1];

    private void Awake() => CacheComponent();

    public void GainExp(float amount)
    {
        if (_status == null || amount <= 0f || IsMaxLevel || _requiredExpPerLevel == null || _requiredExpPerLevel.Length == 0) return;

        _status.Exp += amount;

        // 연속 레벨업시 오류 방지
        while (!IsMaxLevel && RequiredExp > 0 && _status.Exp >= RequiredExp)
        {
            _status.Exp -= RequiredExp;
            _status.Level++;

            // TODO: 레벨업시 변화할 내용 구현
        }

        if (IsMaxLevel)
        {
            _status.Exp = 0f;
        }
    }

#if UNITY_EDITOR
    // 인스펙터에서 설정한 레벨별 요구 경험치가 최소 1 이상이 되도록 보정한다.
    private void OnValidate()
    {
        if (_requiredExpPerLevel == null) return;

        for (int i = 0; i < _requiredExpPerLevel.Length; i++)
        {
            _requiredExpPerLevel[i] = Mathf.Max(1, _requiredExpPerLevel[i]);
        }
    }
#endif

    private void CacheComponent()
    {
        _status = GetComponent<PlayerStatus>();
    }
}
