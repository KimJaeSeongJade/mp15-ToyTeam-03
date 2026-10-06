using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleMove : MonoBehaviour
{
    [SerializeField] private float _speed = 30f;
    [SerializeField] private Transform _target;

    private void Update()
    {
        SpintSelf();
    }

    private void SpintSelf()
    {
        transform.RotateAround(_target.position, Vector3.up, _speed*Time.deltaTime);
    }
}
