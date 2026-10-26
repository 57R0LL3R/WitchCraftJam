using System.Collections.Generic;
using UnityEngine;
using Game.Core.Stats;
using Game.Core.Combat;

namespace Game.Core.Player
{
    [RequireComponent(typeof(CharacterStatsComponent))]
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Ataque")]
        [SerializeField, Min(0.1f)] private float _attackRange = 1f;
        [SerializeField, Min(0.1f)] private float _attackCooldown = 0.5f;
        [SerializeField] private LayerMask _targetLayers;

        private CharacterStatsComponent _stats;
        private Camera _camera;

        private Vector2 _facingDirection = Vector2.down;
        private Vector2 _aimScreenPosition;

        private float _nextAttackTime;
        private float _baseDamage;

        private readonly HashSet<Collider2D> _detectedTargets =
            new HashSet<Collider2D>();

        private void Awake()
        {
            _stats = GetComponent<CharacterStatsComponent>();
            _camera = Camera.main;
        }

        private void Start()
        {
            if (!_stats.IsInitialized ||
                !_stats.TryGetStatValue(StatType.BaseDamage, out _baseDamage))
            {
                Debug.LogError(
                    "PlayerAttack necesita estadísticas inicializadas y BaseDamage.",
                    this
                );

                enabled = false;
                return;
            }

            if (_camera == null)
                Debug.LogWarning("PlayerAttack no encuentra una cámara con tag MainCamera.", this);
        }

        public void SetFacingDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
                _facingDirection = direction.normalized;
        }

        public void SetAimScreenPosition(Vector2 screenPosition)
        {
            _aimScreenPosition = screenPosition;

            if (_camera == null)
                return;

            Vector3 worldPosition = _camera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z)
            );

            Vector2 direction = (Vector2)(worldPosition - transform.position);

            if (direction.sqrMagnitude > 0.01f)
                _facingDirection = direction.normalized;
        }

        public void Attack()
        {
            if (Time.time < _nextAttackTime)
                return;

            _nextAttackTime = Time.time + _attackCooldown;

            Vector2 attackCenter =
                (Vector2)transform.position +
                _facingDirection * (_attackRange * 0.5f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackCenter,
                _attackRange * 0.5f,
                _targetLayers
            );

            _detectedTargets.Clear();

            foreach (Collider2D hit in hits)
            {
                if (hit == null || hit.transform.root == transform.root)
                    continue;

                if (!_detectedTargets.Add(hit))
                    continue;

                Debug.Log(
                    $"Objetivo detectado: {hit.name}. Daño base: {_baseDamage}. Daño aún no aplicado.",
                    hit
                );
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 direction = _facingDirection.sqrMagnitude > 0.01f
                ? _facingDirection.normalized
                : Vector2.down;

            Vector2 center =
                (Vector2)transform.position +
                direction * (_attackRange * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, _attackRange * 0.5f);
        }
    }
}