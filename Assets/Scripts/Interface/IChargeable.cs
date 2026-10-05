public interface IChargeable
{
    bool IsCharging { get; }
    void CheckRelease();
}