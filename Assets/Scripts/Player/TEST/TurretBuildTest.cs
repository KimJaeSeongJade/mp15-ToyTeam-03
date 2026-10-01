using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBuildTest : MonoBehaviour
{
    [SerializeField] private TurretPreview _preview;

    [SerializeField] private TurretType _turretType;

    public TurretType TurretType => _turretType;

    public void CanBuild(bool value)
    {
        _preview.StartPreview(this, value);
    }
}
