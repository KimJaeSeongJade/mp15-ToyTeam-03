using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPreview : MonoBehaviour
{
    [SerializeField] private Material _previewMat;
    private Renderer _previewRenderer;

    void Start()
    {
        _previewRenderer = GetComponent<Renderer>();
    }

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
        turret.GetComponent<Renderer>().material = null;
    }
}
