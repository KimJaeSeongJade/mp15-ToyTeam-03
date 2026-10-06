using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretType
{
    Attack,
    Defence,
    Support,
    Gold,
    Tier2
}

[Serializable]
public class TurretCompination
{
    public TurretType ExistType;
    public TurretType AddedType;
    public BaseTurret ResultTurret;
}

public class TurretCombinationTable : SingletonBehaviour<TurretCombinationTable>
{
    [SerializeField] private List <BaseTurret> _baseTurretlist;

    [SerializeField] private List<TurretCompination> _combinationsViewer;

    [SerializeField] private Dictionary<(TurretType, TurretType), BaseTurret> _lookup = new();
    private float[] _buildReadyAt;

    protected override void Awake()
    {
        base.Awake();
        _buildReadyAt = new float[_baseTurretlist.Count];

        foreach(TurretCompination compination in _combinationsViewer)
        {
            if (compination == null || compination.ResultTurret == null) continue;

            var key = (compination.ExistType, compination.AddedType);

            if(_lookup.ContainsKey(key))
            {
                Debug.LogError($"중복된 타워 조합: {key.ExistType} {key.AddedType}", this);
                continue;
            }

            _lookup.Add(key, compination.ResultTurret);
        }
    }


    public bool TryGetResult(TurretType existType, TurretType addType, out BaseTurret result)
    {
        return _lookup.TryGetValue((existType, addType), out result);
    }

    // 선택한 기본 터렛 슬롯의 건설 쿨타임을 확인한다.
    public bool IsBuildReady(int selectedNum)
    {
        return Time.time >= _buildReadyAt[selectedNum];
    }

    // 실제 건설과 결제가 끝난 뒤, 조합 결과가 아닌 기본 터렛의 쿨타임을 시작한다.
    public float StartBuildCooldown(int selectedNum)
    {
        float cooldown = _baseTurretlist[selectedNum].BuildCooldown;
        _buildReadyAt[selectedNum] = Time.time + cooldown;
        return cooldown;
    }

    // TODO: 리펙토링 가능성 연구 필요
    public BaseTurret GetSelectedTurret(int num)
    {
        switch (num)
        {
            case 0:
                return _baseTurretlist[0];
            case 1:
                return _baseTurretlist[1];
            case 2:
                return _baseTurretlist[2];
            case 3:
                return _baseTurretlist[3];
        }

        return null;
    }
}
