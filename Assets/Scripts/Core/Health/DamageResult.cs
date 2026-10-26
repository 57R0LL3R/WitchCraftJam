namespace Game.Core.Health
{
    /// <summary>
    /// Estructura inmutable que describe la resolución exacta de un impacto de daño.
    /// </summary>
    public readonly struct DamageResult
    {
        public float RequestedAmount { get; }
        public float AppliedAmount { get; }
        public float RemainingHealth { get; }
        public float MaxHealth { get; }
        public bool WasApplied { get; }
        public bool IsFatal { get; }
        public DamageResult(float requestedAmount, float appliedAmount, float remainingHealth, float maxHealth, bool wasApplied, bool isFatal)
        {
            RequestedAmount = requestedAmount;
            AppliedAmount = appliedAmount;
            RemainingHealth = remainingHealth;
            MaxHealth = maxHealth;
            WasApplied = wasApplied;
            IsFatal = isFatal;
        }
    }
}