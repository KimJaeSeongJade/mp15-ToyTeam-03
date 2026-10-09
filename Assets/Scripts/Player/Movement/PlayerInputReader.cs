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


    // 필터가 없는 씬에서는 기존 입력을 그대로 통과시킨다.
    public IPlayerInputFilter InputFilter { get; set; }
    private bool Allows(PlayerInputAction action) => InputFilter == null || InputFilter.Allows(action);
    public KeyCode AttackKey => _attackKey;
    public KeyCode SkillKey => _skillKey;
    public KeyCode ExitBuildKey => _exitBuildKey;

    public bool isPressedAttackUp => Allows(PlayerInputAction.Attack) && Input.GetKeyUp(_attackKey);
    public bool isPressedAttack => Allows(PlayerInputAction.Attack) && Input.GetKey(_attackKey);
    public bool isPressedAttackDown => Allows(PlayerInputAction.Attack) && Input.GetKeyDown(_attackKey);
    public bool isPressedSkillUp => Allows(PlayerInputAction.Skill) && Input.GetKeyUp(_skillKey);
    public bool isPressedSkill => Allows(PlayerInputAction.Skill) && Input.GetKey(_skillKey);
    public bool isPressedSkillDown => Allows(PlayerInputAction.Skill) && Input.GetKeyDown(_skillKey);
    public bool isPressedExitBuild => Allows(PlayerInputAction.ExitBuild) && Input.GetKeyDown(_exitBuildKey);
    public bool isPressedDashKey => Allows(PlayerInputAction.Move) && Input.GetKey(_dashkey);
    public bool isPressedJumpKey => Allows(PlayerInputAction.Move) && Input.GetKeyDown(_jumpKey);

    // 알파키 1~4를 0~3번 터렛 슬롯으로 변환한다. 입력이 없으면 -1.
    public int GetSelectedTurretIndex()
    {
        for (int slot = 0; slot < 4; slot++)
        {
            KeyCode key = (KeyCode)((int)KeyCode.Alpha1 + slot);
            if (Allows((PlayerInputAction)((int)PlayerInputAction.Turret1 + slot))
                && Input.GetKeyDown(key)) return slot;
        }

        return -1;
    }

    public Vector3 GetMoveInput()
    {       
        if (!Allows(PlayerInputAction.Move)) return Vector3.zero;
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
        if(!Allows(PlayerInputAction.Look) || Time.timeScale == 0) return Vector3.zero;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        return new Vector3(-mouseY, mouseX, 0f);
    }
}
