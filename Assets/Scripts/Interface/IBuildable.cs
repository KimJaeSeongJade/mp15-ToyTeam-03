using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildable
{
    public Transform Taret { get; }

    public void SetBuild() { }
    public void UnSetBuild() { }
}
