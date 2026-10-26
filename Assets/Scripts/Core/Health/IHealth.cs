using System;
namespace Game.Core.Health
{
    /// <summary>
    /// Contrato público para cualquier entidad susceptible de recibir daño o curarse.
    /// </summary>
    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
        event Action<float, float> OnHealthChanged;
        event Action<DamageResult> OnDamaged;
        event Action OnDied;
        DamageResult TakeDamage(DamageInfo damageInfo);
        float Heal(float amount);
    }
}