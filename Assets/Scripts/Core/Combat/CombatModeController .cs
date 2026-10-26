using System;
using UnityEngine;

namespace Game.Core.Combat
{
    public enum GameMode
    {
        Exploration,
        Combat
    }

    public class CombatModeController : MonoBehaviour
    {
        public static CombatModeController Instance { get; private set; }

        [Header("Modo inicial")]
        [SerializeField] private GameMode _initialMode = GameMode.Exploration;

        [Header("Pruebas")]
        [SerializeField] private bool _enableManualModeSwitch = true;

        public GameMode CurrentMode { get; private set; }

        public bool IsInCombat => CurrentMode == GameMode.Combat;

        public event Action<GameMode> OnModeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError(
                    "Solo puede existir un CombatModeController activo.",
                    this
                );

                enabled = false;
                return;
            }

            Instance = this;
            CurrentMode = _initialMode;
        }

        public bool EnterCombat()
        {
            if (IsInCombat)
                return false;

            SetMode(GameMode.Combat);
            return true;
        }

        public bool ExitCombat()
        {
            if (!IsInCombat)
                return false;

            SetMode(GameMode.Exploration);
            return true;
        }

        public void ToggleModeForTesting()
        {
            if (!_enableManualModeSwitch)
                return;

            if (IsInCombat)
                ExitCombat();
            else
                EnterCombat();
        }

        private void SetMode(GameMode newMode)
        {
            if (CurrentMode == newMode)
                return;

            CurrentMode = newMode;
            OnModeChanged?.Invoke(CurrentMode);

            Debug.Log($"Modo de juego: {CurrentMode}", this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}