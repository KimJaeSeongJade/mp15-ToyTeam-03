using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private KeyCode _attackKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode _skillKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode _exitBuildKey = KeyCode.E;
    [SerializeField] private KeyCode _dashkey = KeyCode.LeftShift;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode _primaryKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode _subKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode _thirdKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode _forthKey = KeyCode.Alpha4;


    public bool isPressedAttackUp => Input.GetKeyUp(_attackKey);
    public bool isPressedAttack => Input.GetKey(_attackKey);
    public bool isPressedAttackDown => Input.GetKeyDown(_attackKey);
    public bool isPressedSkillUp => Input.GetKeyUp(_skillKey);
    public bool isPressedSkill => Input.GetKey(_skillKey);
    public bool isPressedSkillDown => Input.GetKeyDown(_skillKey);
    public bool isPressedExitBuild => Input.GetKeyDown(_exitBuildKey);
    public bool isPressedDashKey => Input.GetKey(_dashkey);
    public bool isPressedJumpKey => Input.GetKeyDown(_jumpKey);
    public bool isPressedPrimary => Input.GetKeyDown(_primaryKey);
    public bool isPressedSub => Input.GetKeyDown(_subKey);
    public bool isPressedThird => Input.GetKeyDown(_thirdKey);
    public bool isPressedForth => Input.GetKeyDown(_forthKey);

    public Vector3 GetMoveInput()
    {       
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        return new Vector3(moveX, 0f, moveZ);
    }

    public Vector3 GetMoveNormalInput()
    {
        return GetMoveInput().normalized;
    }

    public Vector3 GetMouseInput()
    {
        if(Time.timeScale == 0) return Vector3.zero;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        return new Vector3(-mouseY, mouseX, 0f);
    }
}
