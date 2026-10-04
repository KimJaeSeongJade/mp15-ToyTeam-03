using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TurretDetectTarget : MonoBehaviour
{
    [SerializeField] private Tier2BaseTurret _baseTurret;
    [SerializeField] private LayerMask _targetMask;

    private void OnTriggerEnter(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            if (_baseTurret.Target == null)
            {
                _baseTurret.Target = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTargetLayer(other.gameObject.layer))
        {
            if (_baseTurret.Target == other.transform)
            {
                _baseTurret.Target = null;
            }
        }
    }

    private bool IsTargetLayer(int layer)
    {
        return (_targetMask.value & (1 << layer)) != 0;
    }
}
