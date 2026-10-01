using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuildMode : MonoBehaviour
{
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private float _rayDistance;
    private Ray ray;

    private void Start()
    {
        ray = Camera.main.ScreenPointToRay(Vector3.zero);
    }

    void Update()
    {
        RayToBuildPoint();
    }

    private void RayToBuildPoint()
    {
        if(Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _targetMask))
        {
            Debug.Log(hit.transform.name);
        }
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * _rayDistance);
    }

#endif
}
