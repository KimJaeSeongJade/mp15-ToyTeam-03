using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretType
{
    Attack,
    Defence,
    Support,
    Gold
}

[Serializable]
public class TurretCompination
{
    public TurretType ExistType;
    public TurretType AddedType;
    public GameObject ResultTurret;
}

public class TurretCombinationTable : SingletonBehaviour<TurretCombinationTable>
{
    [SerializeField] private List <TurretBuildTest> _baseTurretlist;

    [SerializeField] private List<TurretCompination> _combinationsViewer;

    [SerializeField] private Dictionary<(TurretType, TurretType), GameObject> _lookup = new();

    protected override void Awake()
    {
        base.Awake();

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


    public bool TryGetResult(TurretType existType, TurretType addType, out GameObject result)
    {
        return _lookup.TryGetValue((existType, addType), out result);
    }

    // TODO: 리펙토링 가능성 연구 필요
    public TurretBuildTest GetSelectedTurret(int num)
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
