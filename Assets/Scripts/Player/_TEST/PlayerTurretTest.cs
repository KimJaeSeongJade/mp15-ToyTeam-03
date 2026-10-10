using UnityEngine;

public class PlayerTurretTest : BaseTurret, IBuildTargetReceiver
{
    [SerializeField] private TurretPreview _testPreview;

    public void PreviewShow(BaseTurret resultPrefab, bool canBuild, Vector3 position, Quaternion rotation)
    {
        _testPreview.Show(resultPrefab, position, rotation, canBuild);
    }

    public void PreviewHide()
    {
        _testPreview.Hide();
    }
}
