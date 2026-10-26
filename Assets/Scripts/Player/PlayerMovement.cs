using UnityEngine;
using Game.Core.Stats;
using Game.Core.Combat;

namespace Game.Core.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStatsComponent))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Salto")]
        [SerializeField, Min(0f)] private float _jumpForce = 8f;

        [Header("Suelo")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField, Min(0.01f)] private float _groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask _groundLayers;

        private Rigidbody2D _rb;
        private CharacterStatsComponent _stats;

        private Vector2 _moveInput;
        private float _moveSpeed;
        private float _defaultGravityScale;
        private bool _jumpRequested;
        private bool _isGrounded;
        private bool _isInitialized;

        private GameMode _currentMode = GameMode.Exploration;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _stats = GetComponent<CharacterStatsComponent>();

            _defaultGravityScale = _rb.gravityScale;
        }

        private void Start()
        {
            if (!_stats.IsInitialized ||
                !_stats.TryGetStatValue(StatType.MoveSpeed, out _moveSpeed))
            {
                Debug.LogError(
                    "PlayerMovement necesita estadísticas inicializadas y MoveSpeed.",
                    this
                );

                enabled = false;
                return;
            }

            _isInitialized = true;

            if (CombatModeController.Instance != null)
            {
                _currentMode = CombatModeController.Instance.CurrentMode;
                CombatModeController.Instance.OnModeChanged += HandleModeChanged;
            }

            ApplyModeSettings();
        }

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void RequestJump()
        {
            if (_currentMode == GameMode.Combat)
                _jumpRequested = true;
        }

        private void FixedUpdate()
        {
            if (!_isInitialized)
                return;

            if (_currentMode == GameMode.Exploration)
            {
                _rb.linearVelocity = _moveInput * _moveSpeed;
                return;
            }

            // En combate solo se controla el desplazamiento horizontal.
            _rb.linearVelocity = new Vector2(
                _moveInput.x * _moveSpeed,
                _rb.linearVelocity.y
            );

            if (_groundCheck != null)
            {
                _isGrounded = Physics2D.OverlapCircle(
                    _groundCheck.position,
                    _groundCheckRadius,
                    _groundLayers
                ) != null;
            }
            else
            {
                _isGrounded = false;
            }

            if (_jumpRequested && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(
                    _rb.linearVelocity.x,
                    _jumpForce
                );
            }

            _jumpRequested = false;
        }

        private void HandleModeChanged(GameMode newMode)
        {
            _currentMode = newMode;
            _moveInput = Vector2.zero;
            _jumpRequested = false;

            ApplyModeSettings();
        }

        private void ApplyModeSettings()
        {
            if (_currentMode == GameMode.Exploration)
            {
                _rb.gravityScale = 0f;
                _rb.linearVelocity = Vector2.zero;
            }
            else
            {
                _rb.gravityScale = _defaultGravityScale;

                // Evita que el personaje conserve velocidad vertical
                // de una transición anterior.
                _rb.linearVelocity = new Vector2(
                    _rb.linearVelocity.x,
                    0f
                );
            }
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
            _jumpRequested = false;

            if (_rb != null)
                _rb.linearVelocity = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (CombatModeController.Instance != null)
            {
                CombatModeController.Instance.OnModeChanged -= HandleModeChanged;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                _groundCheck.position,
                _groundCheckRadius
            );
        }
    }
}