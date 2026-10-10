using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private AudioClip _walk;
    [SerializeField] private AudioClip _run;
    [SerializeField] private AudioClip _jump;
    [SerializeField] private AudioClip _land;

    [Header("공격 / 스킬")]
    [SerializeField] private AudioClip _basicAttack;
    [SerializeField] private AudioClip _spreadAttack;
    [SerializeField] private AudioClip _chargeStart;
    [SerializeField] private AudioClip _chargeRelease;
    [SerializeField] private AudioClip _skill;
    [SerializeField] private AudioClip _cycloneSkill;

    [Header("건설")]
    [SerializeField] private AudioClip _build;
    [SerializeField] private AudioClip _sell;
    [SerializeField] private AudioClip _selectTurret;
    [SerializeField] private AudioClip _exitBuild;

    [Header("성장 / 골드")]
    [SerializeField] private AudioClip _levelUp;
    [SerializeField] private AudioClip _goldGain;
    [SerializeField] private AudioClip _goldSpend;

    public AudioClip Walk => _walk;
    public AudioClip Run => _run;
    public AudioClip Jump => _jump;
    public AudioClip Land => _land;
    public AudioClip BasicAttack => _basicAttack;
    public AudioClip SpreadAttack => _spreadAttack;
    public AudioClip ChargeStart => _chargeStart;
    public AudioClip ChargeRelease => _chargeRelease;
    public AudioClip Skill => _skill;
    public AudioClip CycloneSkill => _cycloneSkill;
    public AudioClip Build => _build;
    public AudioClip Sell => _sell;
    public AudioClip SelectTurret => _selectTurret;
    public AudioClip ExitBuild => _exitBuild;
    public AudioClip LevelUp => _levelUp;
    public AudioClip GoldGain => _goldGain;
    public AudioClip GoldSpend => _goldSpend;
}
