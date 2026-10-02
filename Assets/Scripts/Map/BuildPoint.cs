using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class BuildPoint : MonoBehaviour
{
    [SerializeField]private BaseTurret _currentTurret;
    

    //플레이어한테 매개변수 받아서 
    public void CheckCurrentTurret(BaseTurret turret)
    {
        
        if (_currentTurret == null)
        {
            // turret 설치
            _currentTurret = Instantiate(turret, transform.position, transform.rotation);
        }
        else //있다면
        {
            //TurretCombinationTable.Instance.TryGetResult(_currentTurret.Type, turret.Type, out GameObject result);
            //Instantiate(result,transform.position,transform.rotation);
            // _currentTurret 체크해서 있다면 조합된걸 반환

        }
        
    }
    


}
