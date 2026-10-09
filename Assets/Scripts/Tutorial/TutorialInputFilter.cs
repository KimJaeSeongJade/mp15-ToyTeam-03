using UnityEngine;

// 튜토리얼 씬의 플레이어에만 추가한다. 본게임 프리팹은 변경하지 않는다.
[RequireComponent(typeof(PlayerInputReader))]
[DefaultExecutionOrder(-2500)]
public class TutorialInputFilter : MonoBehaviour, IPlayerInputFilter
{
    public bool InputEnabled { get; set; }
    public bool AllowAttack { get; set; }
    public bool AllowSkill { get; set; }
    public bool AllowBuild { get; set; }
    public bool AllowExitBuild { get; set; }

    private PlayerInputReader _reader;
    private void OnEnable()
    {
        _reader = GetComponent<PlayerInputReader>();
        _reader.InputFilter = this;
    }

    private void OnDisable()
    {
        if (_reader != null && ReferenceEquals(_reader.InputFilter, this)) _reader.InputFilter = null;
    }

    public bool Allows(PlayerInputAction action)
    {
        if (!InputEnabled || Time.timeScale == 0f) return false;
        switch (action)
        {
            case PlayerInputAction.Move:
            case PlayerInputAction.Look: return true;
            case PlayerInputAction.Attack: return AllowAttack;
            case PlayerInputAction.Skill: return AllowSkill;
            case PlayerInputAction.ExitBuild: return AllowExitBuild;
            case PlayerInputAction.Turret1: return AllowBuild;
            default: return false;
        }
    }
}
