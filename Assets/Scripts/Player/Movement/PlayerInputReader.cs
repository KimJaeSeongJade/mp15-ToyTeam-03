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


    public bool isPressedAttackUp => Input.GetKeyUp(_attackKey);
    public bool isPressedAttack => Input.GetKey(_attackKey);
    public bool isPressedAttackDown => Input.GetKeyDown(_attackKey);
    public bool isPressedSkillUp => Input.GetKeyUp(_skillKey);
    public bool isPressedSkill => Input.GetKey(_skillKey);
    public bool isPressedSkillDown => Input.GetKeyDown(_skillKey);
    public bool isPressedExitBuild => Input.GetKeyDown(_exitBuildKey);
    public bool isPressedDashKey => Input.GetKey(_dashkey);
    public bool isPressedJumpKey => Input.GetKeyDown(_jumpKey);

    // 알파키 1~4를 0~3번 터렛 슬롯으로 변환한다. 입력이 없으면 -1.
    public int GetSelectedTurretIndex()
    {
        for (int slot = 0; slot < 4; slot++)
        {
            KeyCode key = (KeyCode)((int)KeyCode.Alpha1 + slot);
            if (Input.GetKeyDown(key)) return slot;
        }

        return -1;
    }

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
