using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerTest : MonoBehaviour
{
    // 확인용 터렛 설치 위치
    [SerializeField] private BaseTurret _curretTurret;

    public void CallbackDebugLog()
    {
        Debug.Log("콜백 성공. TODO: 추후 건설영역에 설치 요청");
    }

    public void Build(BaseTurret target)
    {
        _curretTurret = target;
        _curretTurret.transform.SetParent(transform, false);

        if(TurretCombinationTable.Instance.TryGetResult(_curretTurret.Type, target.Type, out BaseTurret result))
        {
            _curretTurret = result.GetComponent<BaseTurret>();
            _curretTurret.transform.SetParent(transform, false);
        }
    }

    private TurretType GetCurrentTurretType()
    {
        return _curretTurret.Type;
    }
}
