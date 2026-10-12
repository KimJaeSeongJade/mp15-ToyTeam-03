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
    public int AllowedBuildSlot { get; set; } = 0;
    public KeyCode? RequiredMoveKey { get; set; }
    public bool AllowExitBuild { get; set; }
    public bool WaitingForPrompt { get; private set; }
    public event System.Action PromptAccepted;
    private PlayerInputAction _promptAction;

    private PlayerInputReader _reader;
    public void BeginPrompt(PlayerInputAction action)
    {
        _promptAction = action;
        WaitingForPrompt = true;
        InputEnabled = false;
    }

    public void CancelPrompt()
    {
        WaitingForPrompt = false;
        InputEnabled = false;
    }

    // PlayerAttackMode.Update보다 먼저 해제하여 확인한 클릭이 실제 조작으로 이어진다.
    private void Update()
    {
        if (!WaitingForPrompt || Time.timeScale == 0f) return;
        bool pressed = false;
        switch (_promptAction)
        {
            case PlayerInputAction.Move:
                pressed = RequiredMoveKey.HasValue ? Input.GetKeyDown(RequiredMoveKey.Value)
                    : Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)
                    || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D);
                break;
            case PlayerInputAction.Attack: pressed = Input.GetKeyDown(_reader.AttackKey); break;
            case PlayerInputAction.Skill: pressed = Input.GetKeyDown(_reader.SkillKey); break;
            case PlayerInputAction.Turret1: pressed = Input.GetKeyDown(KeyCode.Alpha1); break;
            case PlayerInputAction.Turret2: pressed = Input.GetKeyDown(KeyCode.Alpha2); break;
            case PlayerInputAction.ExitBuild: pressed = Input.GetKeyDown(_reader.ExitBuildKey); break;
        }
        if (!pressed) return;
        WaitingForPrompt = false;
        InputEnabled = true;
        PromptAccepted?.Invoke();
    }
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
            case PlayerInputAction.Turret1: return AllowBuild && (AllowedBuildSlot == 0 || AllowedBuildSlot == -1);
            case PlayerInputAction.Turret2: return AllowBuild && (AllowedBuildSlot == 1 || AllowedBuildSlot == -1);
            default: return false;
        }
    }
}
