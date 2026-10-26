using UnityEngine;

namespace Game.Core.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class CombatEncounterTarget : MonoBehaviour
    {
        [Header("Formas de iniciar combate")]
        [SerializeField] private bool _startOnProximity;
        [SerializeField] private bool _canInteract = true;

        private void Reset()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_startOnProximity)
                return;

            if (!other.CompareTag("Player"))
                return;

            RequestCombat();
        }

        public void Interact()
        {
            if (!_canInteract)
                return;

            RequestCombat();
        }

        public void RequestCombat()
        {
            if (CombatModeController.Instance == null)
                return;

            CombatModeController.Instance.EnterCombat();
        }

        // Un ataque hostil de un NPC puede llamar este método.
        public void RequestHostileEncounter()
        {
            RequestCombat();
        }
    }
}