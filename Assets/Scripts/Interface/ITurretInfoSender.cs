using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITurretInfoSender
{
    public string Name { get; }
    public string Info { get; }

    public void GetTurretInfo() { }
}
