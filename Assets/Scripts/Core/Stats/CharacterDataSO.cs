using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Stats
{
    [CreateAssetMenu(
        fileName = "NewCharacterData",
        menuName = "Game/Stats/Character Data")]
    public class CharacterDataSO : ScriptableObject
    {
        [Header("Información básica")]
        [SerializeField] private string _characterName = "Nuevo Personaje";

        [Header("Estadísticas base")]
        [SerializeField] private List<StatEntry> _baseStats =
            new List<StatEntry>
            {
                new StatEntry(StatType.MaxHealth, 100f),
                new StatEntry(StatType.BaseDamage, 10f),
                new StatEntry(StatType.MoveSpeed, 5f),
                new StatEntry(StatType.Armor, 0f),
                new StatEntry(StatType.CriticalChance, 0.05f),
                new StatEntry(StatType.CriticalMultiplier, 1.5f)
            };

        public string CharacterName => _characterName;

        public IReadOnlyList<StatEntry> BaseStats => _baseStats;

        public float MaxHealth
        {
            get
            {
                return TryGetStatValue(StatType.MaxHealth, out float value)
                    ? value
                    : 0f;
            }
        }

        public bool TryGetStatValue(StatType type, out float value)
        {
            value = 0f;

            if (_baseStats == null)
                return false;

            foreach (StatEntry entry in _baseStats)
            {
                if (entry.Type == type && entry.IsValid(out _))
                {
                    value = entry.Value;
                    return true;
                }
            }

            return false;
        }

        public bool ValidateAndSanitize()
        {
            if (_baseStats == null)
                _baseStats = new List<StatEntry>();

            if (string.IsNullOrWhiteSpace(_characterName))
                _characterName = "Nuevo Personaje";

            bool isValid = true;
            HashSet<StatType> registeredTypes = new HashSet<StatType>();

            for (int i = 0; i < _baseStats.Count; i++)
            {
                StatEntry entry = _baseStats[i];

                if (!registeredTypes.Add(entry.Type))
                {
                    Debug.LogError(
                        $"[{name}] La estadística '{entry.Type}' está duplicada.",
                        this);

                    isValid = false;
                    continue;
                }

                if (!entry.IsValid(out string error))
                {
                    Debug.LogError(
                        $"[{name}] Estadística inválida: {error}",
                        this);

                    isValid = false;
                }
            }

            if (!registeredTypes.Contains(StatType.MaxHealth))
            {
                Debug.LogError(
                    $"[{name}] Falta la estadística obligatoria MaxHealth.",
                    this);

                isValid = false;
            }

            return isValid;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_characterName))
                _characterName = "Nuevo Personaje";
        }
    }
}