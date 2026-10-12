using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
public class PlayerLevelManager : MonoBehaviour
{
    [SerializeField] private float[] _requiredExpPerLevel = { 100f, 150f, 225f, 300f };

    private PlayerStatus _status;
    private PlayerAttackMode _attackMode;
    private PlayerSound _sound;

    [Header("레벨별 업그레이드 사항을 넣어두는 필드\n" +
        "현재는 레벨이 많지 않아 이대로 구현하나 추후 개선 필요")]
    [SerializeField] private float _attackSpeedMultiplier;
    [SerializeField] private float _skillCooldownMultiplier;
    [SerializeField] private BasicAttack[] _basicAttacks;
    [SerializeField] private SkillAttack[] _skillAttacks;

    [SerializeField] private GameObject _lvUpEffect;

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

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.L)) return;
        GainExp(200);
    }

    public void GainExp(float amount)
    {
        if (_status == null || amount <= 0f || IsMaxLevel || _requiredExpPerLevel == null || _requiredExpPerLevel.Length == 0) return;

        _status.Exp += amount;

        // 연속 레벨업시 오류 방지
        while (!IsMaxLevel && RequiredExp > 0 && _status.Exp >= RequiredExp)
        {
            _status.Exp -= RequiredExp;
            _status.Level++;

            ApplyLevelRewards(_status.Level);
        }

        if (IsMaxLevel)
        {
            _status.Exp = 0f;
        }
    }

    // 현재는 레벨별로 Switch로 주고 있으나
    // 추후 확장성을 고려하면 다른 방식으로 구현이 필요
    private void ApplyLevelRewards(int newLevel)
    {
        SoundManager.Instance.Play(_sound.LevelUp);
        Instantiate(_lvUpEffect, transform);

        switch (newLevel)
        {
            case 2:
                // 공속, 쿨감
                float resultAtk = 1f - _attackSpeedMultiplier;
                float resultSkill = 1f - _skillCooldownMultiplier;

                _attackMode.LevelUpMultiply(resultAtk, resultSkill);
                break;
            case 3:
                // 평타 강화 (확산)
                _attackMode.SetBasicAttack(_basicAttacks[1]);
                break;
            case 4:
                // 스킬 업그레이드 (싸이클론)
                _attackMode.SetSkillAttack(_skillAttacks[1]);
                break;
            case 5:
                // 평타 강화 (차지)
                _attackMode.SetBasicAttack(_basicAttacks[2]);
                break;
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
        _attackMode = GetComponent<PlayerAttackMode>();
        _sound = GetComponent<PlayerSound>();
    }
}
