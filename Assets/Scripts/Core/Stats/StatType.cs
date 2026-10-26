namespace Game.Core.Stats
{
    /// &lt;summary&gt;
    /// Identificador único y fuertemente tipado para cada estadística del juego.
    /// Previene cadenas mágicas y errores de escritura.
    /// &lt;/summary&gt;
    public enum StatType
    {
        MaxHealth = 0,
        BaseDamage = 1,
        MoveSpeed = 2,
        Armor = 3,
        CriticalChance = 4,
        CriticalMultiplier = 5
    }
}