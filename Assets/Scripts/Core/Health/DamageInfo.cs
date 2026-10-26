using System;
using UnityEngine;
namespace Game.Core.Health
{
    /// <summary>
    /// Estructura inmutable de datos que transporta la solicitud de daño.
    /// Rechaza categóricamente montos negativos o no finitos lanzando excepciones explícitas.
    /// </summary>
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public GameObject Source { get; }
        public DamageInfo(float amount, GameObject source = null)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount))
            {
                throw new ArgumentException("El monto de daño debe ser un número finito válido.", nameof(amount));
            }
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "El monto de daño no puede ser negativo.");
            }
            Amount = amount;
            Source = source;
        }
    }
}