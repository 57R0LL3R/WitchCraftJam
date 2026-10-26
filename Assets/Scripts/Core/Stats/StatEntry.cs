using System;
using UnityEngine;
namespace Game.Core.Stats
{
    /// &lt;summary&gt;
    /// Estructura serializable que representa un par (Tipo de Estadística, Valor Base).
    /// Contiene reglas de validación y sanitización para prevenir valores inválidos o no finitos.
    /// &lt;/summary&gt;
    [Serializable]
    public struct StatEntry
    {
        [SerializeField] private StatType _type;
        [SerializeField] private float _value;
        public StatType Type => _type;
        public float Value => _value;
        public StatEntry(StatType type, float value)
        {
            _type = type;
            _value = value;
        }
        /// &lt;summary&gt;
        /// Valida si el valor de la estadística es numéricamente correcto y admisible para su tipo.
        /// &lt;/summary&gt;
        public readonly bool IsValid(out string errorMessage)
        {
            if (float.IsNaN(_value) || float.IsInfinity(_value))
            {
                errorMessage = $" El valor de la estadística &#39;{_type}&#39; no es un número finito válido({_value}).";
                return false;
            }
            switch (_type)
            {
                case StatType.MaxHealth:
                case StatType.MoveSpeed:
                    if (_value <= 0f)
                    {
                        errorMessage = $" La estadística &#39;{_type}&#39; debe ser estrictamente mayor a 0 (Valor ingresado: {_value}).";
                        return false;
                    }
                    break;
                case StatType.BaseDamage:
                case StatType.Armor:
                case StatType.CriticalChance:
                case StatType.CriticalMultiplier:
                    if (_value < 0f){
                        errorMessage = $" La estadística &#39;{_type}&#39; no puede ser negativa (Valoringresado: {_value}).";
                        return false;
                    }
                    break;
            }
            errorMessage = string.Empty;
            return true;
        }
        /// &lt;summary&gt;
        /// Sanitiza el valor corrigiendo números no finitos o fuera de rango permitido.
        /// &lt;/summary&gt;
        public void Sanitize()
        {
            if (float.IsNaN(_value) || float.IsInfinity(_value))
            {
                _value = GetDefaultFallbackValue(_type);
                return;
            }
            switch (_type)
            {
                case StatType.MaxHealth:
                    if (_value < 1f) _value = 1f;
                    break;
                case StatType.MoveSpeed:
                    if (_value < 0.1f) _value = 0.1f;
                    break;
                case StatType.BaseDamage:
                case StatType.Armor:
                case StatType.CriticalChance:
                case StatType.CriticalMultiplier:
                    if (_value < 0f) _value = 0f;
                    break;
            }
        }
        public static float GetDefaultFallbackValue(StatType type)
        {
            return type switch
            {
                StatType.MaxHealth => 100f,
                StatType.BaseDamage => 10f,
                StatType.MoveSpeed => 5f,
                StatType.Armor => 0f,
                StatType.CriticalChance => 0.05f,
                StatType.CriticalMultiplier => 1.5f,
                _ => 0f
            };
        }
    }
}