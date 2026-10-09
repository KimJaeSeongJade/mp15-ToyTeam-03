// 어떤 씬의 입력 제한인지 PlayerInputReader가 알 필요 없는 공통 연결 지점.
public interface IPlayerInputFilter
{
    bool Allows(PlayerInputAction action);
}

public enum PlayerInputAction
{
    Move, Look, Attack, Skill, ExitBuild, Turret1, Turret2, Turret3, Turret4
}
