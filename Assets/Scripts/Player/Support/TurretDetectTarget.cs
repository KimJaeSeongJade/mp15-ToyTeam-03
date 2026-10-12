using UnityEngine;

[DefaultExecutionOrder(-100)]
public class TurretDetectTarget : MonoBehaviour
{
    [SerializeField] private Tier2BaseTurret _baseTurret;
    [SerializeField] private LayerMask _targetMask;

    private readonly TurretTargetList _targets = new TurretTargetList();

    private void Update()
    {
        _baseTurret.Target = _targets.GetTarget(_baseTurret.Target);
    }

    private void OnTriggerEnter(Collider other)
    {
        RegisterTarget(other);
    }

    private void OnTriggerStay(Collider other)
    {
        RegisterTarget(other);
    }

    private void RegisterTarget(Collider other)
    {
        if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;
        _targets.Register(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _targets.Remove(other);
    }

    private void OnDisable()
    {
        _targets.Clear();
        _baseTurret.Target = null;
    }
}
