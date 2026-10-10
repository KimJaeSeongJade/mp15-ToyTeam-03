using System;

public interface IHealthSource
{
    float CurrentHp { get; }
    float MaxHp { get; }

    event Action OnHealthChanged;
}
