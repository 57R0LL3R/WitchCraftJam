
using System;
using UnityEngine;
using Game.Core.Stats;

namespace Game.Core.Health
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStatsComponent))]
    public class HealthComponent : MonoBehaviour, IHealth
    {
        [Header("Configuración")]
        [SerializeField] private CharacterDataSO _characterData;

        private CharacterStatsComponent _statsComponent;
        private float _currentHealth;
        private float _maxHealth;
        private bool _isDead;
        private bool _isInitialized;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsDead => _isDead;
        public bool IsInitialized => _isInitialized;

        public event Action<float, float> OnHealthChanged;
        public event Action<DamageResult> OnDamaged;
        public event Action OnDied;

        private void Awake()
        {
            _statsComponent = GetComponent<CharacterStatsComponent>();

            if (_characterData != null && !_isInitialized)
                Initialize(_characterData);
        }

        public void Initialize(CharacterDataSO data)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("HealthComponent ya está inicializado.", this);
                return;
            }

            if (data == null)
            {
                Debug.LogError("CharacterDataSO es nulo.", this);
                return;
            }

            _statsComponent = GetComponent<CharacterStatsComponent>();

            if (_statsComponent == null)
            {
                Debug.LogError("Falta CharacterStatsComponent.", this);
                return;
            }

            if (!_statsComponent.IsInitialized &&
                !_statsComponent.Initialize(data))
            {
                Debug.LogError("No se pudieron inicializar las estadísticas.", this);
                return;
            }

            if (!_statsComponent.TryGetStatValue(
                StatType.MaxHealth, out float maxHealth))
            {
                Debug.LogError("No se encontró MaxHealth.", this);
                return;
            }

            if (float.IsNaN(maxHealth) ||
                float.IsInfinity(maxHealth) ||
                maxHealth <= 0f)
            {
                Debug.LogError("MaxHealth debe ser finito y mayor que cero.", this);
                return;
            }

            _characterData = data;
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            _isDead = false;
            _isInitialized = true;

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public DamageResult TakeDamage(DamageInfo damageInfo)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("No se puede aplicar daño: HealthComponent no está inicializado.", this);

                return new DamageResult(
                    damageInfo.Amount, 0f, _currentHealth,
                    _maxHealth, false, false);
            }

            if (_isDead || damageInfo.Amount <= 0f)
            {
                return new DamageResult(
                    damageInfo.Amount, 0f, _currentHealth,
                    _maxHealth, false, false);
            }

            float previousHealth = _currentHealth;

            _currentHealth = Mathf.Max(
                0f, _currentHealth - damageInfo.Amount);

            float appliedAmount = previousHealth - _currentHealth;
            bool wasApplied = appliedAmount > 0f;
            bool isFatal = _currentHealth <= 0f && !_isDead;

            if (isFatal)
                _isDead = true;

            DamageResult result = new DamageResult(
                damageInfo.Amount,
                appliedAmount,
                _currentHealth,
                _maxHealth,
                wasApplied,
                isFatal);

            if (wasApplied)
            {
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
                OnDamaged?.Invoke(result);
            }

            if (isFatal)
                OnDied?.Invoke();

            return result;
        }

        public float Heal(float amount)
        {
            if (!_isInitialized || _isDead ||
                float.IsNaN(amount) ||
                float.IsInfinity(amount) ||
                amount <= 0f)
            {
                return 0f;
            }

            float previousHealth = _currentHealth;

            _currentHealth = Mathf.Min(
                _maxHealth, _currentHealth + amount);

            float healedAmount = _currentHealth - previousHealth;

            if (healedAmount > 0f)
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            return healedAmount;
        }

        public void RestoreFullHealth()
        {
            if (!_isInitialized || _isDead ||
                _currentHealth >= _maxHealth)
            {
                return;
            }

            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void ResetHealth()
        {
            if (_characterData == null)
            {
                Debug.LogError("Falta CharacterDataSO para reiniciar la vida.", this);
                return;
            }

            _isInitialized = false;

            if (_statsComponent == null)
                _statsComponent = GetComponent<CharacterStatsComponent>();

            if (_statsComponent == null)
            {
                Debug.LogError("Falta CharacterStatsComponent.", this);
                return;
            }

            _statsComponent.ResetStats();

            if (!_statsComponent.IsInitialized ||
                !_statsComponent.TryGetStatValue(
                    StatType.MaxHealth, out float maxHealth) ||
                float.IsNaN(maxHealth) ||
                float.IsInfinity(maxHealth) ||
                maxHealth <= 0f)
            {
                Debug.LogError("No se pudo recuperar MaxHealth.", this);
                return;
            }

            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            _isDead = false;
            _isInitialized = true;

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void RefreshMaxHealth()
        {
            if (_statsComponent == null)
                _statsComponent = GetComponent<CharacterStatsComponent>();

            if (_statsComponent == null ||
                !_statsComponent.IsInitialized ||
                !_statsComponent.TryGetStatValue(
                    StatType.MaxHealth, out float maxHealth) ||
                float.IsNaN(maxHealth) ||
                float.IsInfinity(maxHealth) ||
                maxHealth <= 0f)
            {
                Debug.LogError("No se pudo actualizar MaxHealth.", this);
                return;
            }

            float previousMaxHealth = _maxHealth;
            _maxHealth = maxHealth;

            // Evita que la vida actual supere el nuevo máximo.
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

            if (!Mathf.Approximately(previousMaxHealth, _maxHealth))
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
}
