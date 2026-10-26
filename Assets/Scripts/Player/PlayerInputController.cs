using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Combat;

namespace Game.Core.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAttack))]
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerMovement _movement;
        private PlayerAttack _attack;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _attack = GetComponent<PlayerAttack>();
        }

        public void OnMove(InputValue value)
        {
            Vector2 input = value.Get<Vector2>();

            _movement.SetMoveInput(input);

            if (CombatModeController.Instance == null ||
                !CombatModeController.Instance.IsInCombat)
            {
                if (input.sqrMagnitude > 0.01f)
                    _attack.SetFacingDirection(input);
            }
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
                _movement.RequestJump();
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
                _attack.Attack();
        }

        public void OnPrimaryAttack(InputValue value)
        {
            if (value.isPressed)
                _attack.Attack();
        }

        public void OnToggleCombat(InputValue value)
        {
            if (value.isPressed &&
                CombatModeController.Instance != null)
            {
                CombatModeController.Instance.ToggleModeForTesting();
            }
        }
        [Header("Interacción")]
        [SerializeField, Min(0.1f)] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactionLayers;
        public void OnInteract(InputValue value)
        {
            if (!value.isPressed)
                return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                _interactionRadius,
                _interactionLayers
            );

            foreach (Collider2D hit in hits)
            {
                CombatEncounterTarget target =
                    hit.GetComponentInParent<CombatEncounterTarget>();

                if (target == null)
                    continue;

                target.Interact();
                return;
            }

            Debug.Log("No hay ningún objetivo interactuable cerca.", this);
        }

        public void OnAim(InputValue value)
        {
            _attack.SetAimScreenPosition(value.Get<Vector2>());
        }
    }
}