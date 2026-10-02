using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTest : MonoBehaviour
{
    // 확인용 터렛 설치 위치
    [SerializeField] private TurretBuildTest _curretTurret;

    public void CallbackDebugLog()
    {
        Debug.Log("콜백 성공. TODO: 추후 건설영역에 설치 요청");
    }

    public void Build(TurretBuildTest target)
    {
        _curretTurret = target;
        _curretTurret.transform.SetParent(transform, false);
    }

    private TurretType GetCurrentTurretType()
    {
        return _curretTurret.TurretType;
    }
}
