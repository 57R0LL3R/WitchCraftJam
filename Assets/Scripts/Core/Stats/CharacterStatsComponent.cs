using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Stats
{
    [DisallowMultipleComponent]
    public class CharacterStatsComponent : MonoBehaviour
    {
        [Header("Configuración base")]
        [SerializeField] private CharacterDataSO _characterData;

        private readonly Dictionary<StatType, float> _runtimeStats =
            new Dictionary<StatType, float>();

        public CharacterDataSO Data => _characterData;
        public bool IsInitialized { get; private set; }

        public string CharacterName =>
            IsInitialized && _characterData != null
                ? _characterData.CharacterName
                : "No Inicializado";

        private void Awake()
        {
            if (_characterData != null && !IsInitialized)
                Initialize(_characterData);
        }

        public bool Initialize(CharacterDataSO data)
        {
            if (IsInitialized)
            {
                Debug.LogWarning(
                    $"[{nameof(CharacterStatsComponent)}] Ya está inicializado.",
                    this);

                return false;
            }

            if (data == null)
            {
                Debug.LogError("CharacterDataSO es nulo.", this);
                return false;
            }

            if (!data.ValidateAndSanitize())
            {
                Debug.LogError(
                    $"No se pudo inicializar '{data.CharacterName}': revisa sus estadísticas.",
                    this);

                return false;
            }

            Dictionary<StatType, float> newStats =
                new Dictionary<StatType, float>();

            foreach (StatEntry entry in data.BaseStats)
            {
                newStats.Add(entry.Type, entry.Value);
            }

            _characterData = data;
            _runtimeStats.Clear();

            foreach (KeyValuePair<StatType, float> stat in newStats)
                _runtimeStats.Add(stat.Key, stat.Value);

            IsInitialized = true;
            return true;
        }

        public bool TryGetStatValue(StatType type, out float value)
        {
            if (!IsInitialized)
            {
                value = 0f;
                return false;
            }

            return _runtimeStats.TryGetValue(type, out value);
        }

        public float GetStatValue(StatType type)
        {
            if (TryGetStatValue(type, out float value))
                return value;

            Debug.LogWarning(
                $"No se encontró la estadística '{type}' en '{gameObject.name}'.",
                this);

            return StatEntry.GetDefaultFallbackValue(type);
        }

        public void ResetStats()
        {
            if (_characterData == null)
            {
                Debug.LogError(
                    "No se puede reiniciar: falta CharacterDataSO.",
                    this);

                return;
            }

            IsInitialized = false;
            Initialize(_characterData);
        }
    }
}