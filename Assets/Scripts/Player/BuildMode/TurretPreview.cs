using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPreview : MonoBehaviour
{
    [SerializeField] private Material _previewMat;
    private Renderer _previewRenderer;

    void Start()
    {
        _previewRenderer = GetComponentInChildren<Renderer>();
        _previewMat = _previewRenderer.material;
    }

    // TODO: 메터리얼 참조 방식 변경 해야함
    public void StartPreview(TurretBuildTest turret, bool value)
    {
        switch(value)
        {
            case true:
                _previewMat.color = Color.green;
                break;

            case false:

                _previewMat.color = Color.red;
                break;
        }

        turret.GetComponentInChildren<Renderer>().material = _previewMat;
    }

    public void EndPreview(TurretBuildTest turret)
    {
        turret.GetComponentInChildren<Renderer>().material = null;
    }
}
