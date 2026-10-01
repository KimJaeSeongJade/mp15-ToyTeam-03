using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBuildTest : MonoBehaviour
{
    // 확인용 터렛

    [SerializeField] private TurretPreview _preview;

    [SerializeField] private TurretType _turretType;

    public TurretType TurretType => _turretType;

    public void CanBuild(bool value)
    {
        _preview.StartPreview(this, value);
    }

    public void TryBuild()
    {
        if(true)
        {
            _preview.EndPreview(this);
        }

    }
}
