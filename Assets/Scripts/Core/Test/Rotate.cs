using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float _rspeed;
    private void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * _rspeed);
    }
}
