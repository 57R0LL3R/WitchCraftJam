using System;
using Game.Core.Stats;
using UnityEngine;
namespace Game.Core.Health
{
    /// <summary>
    /// Arnés de pruebas manuales ejecutable desde el menú contextual del Inspector de Unity.
    /// Valida inicializaciones, límites, inputs inválidos y eventos sin afectar la producción.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class HealthTestHarness : MonoBehaviour
    {
        [SerializeField] private HealthComponent _healthComponent;
        [SerializeField] private CharacterDataSO _testDataAsset;
        private int _onDiedCount = 0;
        private void Reset()
        {
            _healthComponent = GetComponent<HealthComponent>();
        }
        private void Awake()
        {
            if (_healthComponent == null)
            {
                _healthComponent = GetComponent<HealthComponent>();
            }
        }
        private void OnEnable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDied += HandleDied;
            }
        }
        private void OnDisable()
        {
            if (_healthComponent != null)
            {
                _healthComponent.OnDied -= HandleDied;
            }
        }
        [ContextMenu("1. Prueba: Componente Sin Configuración")]
        public void TestUninitializedComponent()
        {
            Debug.Log("--- PRUEBA: COMPONENTE SIN CONFIGURACIÓN ---");
            GameObject uninitGO = new GameObject("Uninitialized_Test_Entity");
            HealthComponent hc = uninitGO.AddComponent<HealthComponent>();
            DamageResult result = hc.TakeDamage(new DamageInfo(25f, gameObject));
            float healed = hc.Heal(20f);

            bool passed =
                !result.WasApplied &&
                !result.IsFatal &&
                Mathf.Approximately(healed, 0f) &&
                Mathf.Approximately(hc.MaxHealth, 0f) &&
                !hc.IsDead;

            if (passed)
            {
                Debug.Log("[ÉXITO] La entidad sin inicializar rechazó daño y curación.");
            }
            else
            {
                Debug.LogError("[FALLO] La entidad sin inicializar modificó un estado que debía rechazar.");
            }
            Debug.Log($"Resultado Daño sin Init -> WasApplied: {result.WasApplied}, MaxHealth: {hc.MaxHealth}");
            Debug.Log($"Resultado Curación sin Init -> RealHealed: {healed}");
            DestroyImmediate(uninitGO);
        }

        [ContextMenu("2. Prueba: Inicialización Repetida")]
        public void TestRepeatedInitialization()
        {
            if (_testDataAsset == null)
            {
                Debug.LogError("Asigna _testDataAsset antes de ejecutar esta prueba.", this);
                return;
            }

            GameObject testObject = new GameObject("RepeatedInitialization_Test");

            try
            {
                HealthComponent hc = testObject.AddComponent<HealthComponent>();

                hc.Initialize(_testDataAsset);

                if (!hc.IsInitialized || hc.MaxHealth <= 0f)
                {
                    Debug.LogError("[FALLO] La primera inicialización no funcionó.");
                    return;
                }

                float initialHealth = hc.CurrentHealth;

                hc.TakeDamage(new DamageInfo(10f, gameObject));
                float healthAfterDamage = hc.CurrentHealth;

                if (healthAfterDamage >= initialHealth)
                {
                    Debug.LogError("[FALLO] El daño inicial de prueba no redujo la vida.");
                    return;
                }

                hc.Initialize(_testDataAsset);

                bool passed =
                    Mathf.Approximately(hc.CurrentHealth, healthAfterDamage) &&
                    Mathf.Approximately(hc.MaxHealth, initialHealth);

                if (passed)
                {
                    Debug.Log("[ÉXITO] La segunda inicialización fue rechazada y la vida se conservó.");
                }
                else
                {
                    Debug.LogError("[FALLO] La segunda inicialización modificó el estado de vida.");
                }
            }
            finally
            {
                DestroyImmediate(testObject);
            }
        }


        [ContextMenu("3. Prueba: Daño Cero")]
        public void TestZeroDamage()
        {
            Debug.Log("--- PRUEBA: DAÑO CERO ---");
            DamageResult result = _healthComponent.TakeDamage(new DamageInfo(0f, gameObject));
            Debug.Log($"Resultado Daño Cero -> WasApplied: {result.WasApplied}, AppliedAmount: {result.AppliedAmount}");
        }
        [ContextMenu("4. Prueba: Daño Letal Repetido y Conteo OnDied")]
        public void TestRepeatedLethalDamage()
        {
            Debug.Log("--- PRUEBA: DAÑO LETAL REPETIDO Y CONTEO ONDIED ---");
            _healthComponent.ResetHealth();
            _onDiedCount = 0;
            float hp = _healthComponent.CurrentHealth;
            Debug.Log($"Vida restaurada: {hp}. Conteo OnDied = {_onDiedCount}");
            // Primer impacto letal
            DamageResult res1 = _healthComponent.TakeDamage(new DamageInfo(hp + 50f, gameObject));
            Debug.Log($"Impacto 1 -> WasApplied: {res1.WasApplied}, IsFatal: {res1.IsFatal}, IsDead: {_healthComponent.IsDead}, Conteo OnDied: {_onDiedCount}");
            // Segundo impacto estando muerto
            DamageResult res2 = _healthComponent.TakeDamage(new DamageInfo(50f, gameObject));
            Debug.Log($"Impacto 2 (Post-mortem) -> WasApplied: {res2.WasApplied}, IsFatal: {res2.IsFatal}, Conteo OnDied: {_onDiedCount} ");

            if (_onDiedCount == 1 && !res2.IsFatal && !res2.WasApplied)
            {
                Debug.Log("<color=green>[ÉXITO]</color> OnDied se invocó exactamente 1 vez y los impactos post-mortemfueron rechazados.");
            }
            else
            {
                Debug.LogError("[FALLO] Fallo en la verificación de evento letal único.");
            }
        }
        [ContextMenu("7. Prueba: Dos Entidades Compartiendo el Mismo Asset")]
        public void TestSharedAssetIndependence()
        {
            Debug.Log("--- PRUEBA: INDEPENDENCIA DE INSTANCIAS CON ASSET COMPARTIDO ---");
            if (_testDataAsset == null)
            {
                Debug.LogError("Asigna un CharacterDataSO en _testDataAsset antes de correr esta prueba.");
                return;
            }
            GameObject entityA = new GameObject("Entity_A");
            GameObject entityB = new GameObject("Entity_B");
            HealthComponent hcA = entityA.AddComponent<HealthComponent>();
            HealthComponent hcB = entityB.AddComponent<HealthComponent>();
            hcA.Initialize(_testDataAsset);
            hcB.Initialize(_testDataAsset);
            Debug.Log($"Ambas entidades inicializadas con {_testDataAsset.CharacterName}. MaxHP: {hcA.MaxHealth}");
            hcA.TakeDamage(new DamageInfo(40f, gameObject));
            Debug.Log($"Entity A HP: {hcA.CurrentHealth}/{hcA.MaxHealth}");
            Debug.Log($"Entity B HP: {hcB.CurrentHealth}/{hcB.MaxHealth}");
            bool passed =
            Mathf.Approximately(hcA.CurrentHealth, hcA.MaxHealth - 40f) &&
            Mathf.Approximately(hcB.CurrentHealth, hcB.MaxHealth);

            if (passed)
            {
                Debug.Log("<color=green>[ÉXITO]</color> Las instancias son 100% independientes y conservan su estado dinámico propio.");
            }
            else
            {
                Debug.LogError("[FALLO] Hubo contaminación de estado entre entidades que comparten el ScriptableObject.");
            }
            DestroyImmediate(entityA);
            DestroyImmediate(entityB);
        }
        [ContextMenu("1. Aplicar Daño Normal (25)")]
        public void TestDamage()
        {
            try
            {
                DamageResult result = _healthComponent.TakeDamage(new DamageInfo(25f, gameObject));
                Debug.Log($"[HARNESS] Solicito: {result.RequestedAmount} | Aplicado: {result.AppliedAmount} | Restante: {result.RemainingHealth}/{result.MaxHealth} | Fatal: {result.IsFatal}");

            }
            catch (Exception ex)
            {
                Debug.LogError($"[HARNESS] Excepción durante daño normal: {ex.Message}");
            }
        }
        [ContextMenu("2. Aplicar Daño Letal")]
        public void TestLethalDamage()
        {
            try
            {
                float damageNeeded = _healthComponent.CurrentHealth + 100f;
                DamageResult result = _healthComponent.TakeDamage(new DamageInfo(damageNeeded, gameObject)

                );

                Debug.Log($"[HARNESS] Daño Letal Aplicado. Vida Restante: {result.RemainingHealth} | IsFatal: {result.IsFatal}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HARNESS] Excepción en daño letal: {ex.Message}");
            }
        }
        [ContextMenu("3. Intentar Curación (20)")]
        public void TestHeal()
        {
            float curado = _healthComponent.Heal(20f); Debug.Log($"[HARNESS] Solicitado: 20 | Curación Real Efectuada: {curado} | Vida Actual: {_healthComponent.CurrentHealth}/{_healthComponent.MaxHealth}");
        }
        [ContextMenu("4. Reiniciar Vida (ResetHealth)")]
        public void TestReset()
        {
            _healthComponent.ResetHealth();
            Debug.Log($"[HARNESS] Vida reiniciada exitosamente a {_healthComponent.CurrentHealth}/{_healthComponent.MaxHealth}. Muerto: {_healthComponent.IsDead}");
        }
        [ContextMenu("5. Validar Entradas Inválidas")]
        public void TestInvalidDamage()
        {
            Debug.Log("--- INICIO DE PRUEBAS DE ENTRADA INVÁLIDA ---");
            // Prueba A: Daño Negativo (-15)
            try
            {
                new DamageInfo(-15f, gameObject);
                Debug.LogError("[HARNESS FALLO] DamageInfo no rechazó el daño negativo.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.Log("<color=green>[HARNESS ÉXITO]</color> DamageInfo rechazó correctamente el daño negativo(-15).");
            }
            // Prueba B: Daño NaN
            try
            {
                new DamageInfo(float.NaN, gameObject);
                Debug.LogError("[HARNESS FALLO] DamageInfo no rechazó float.NaN.");
            }
            catch (ArgumentException)
            {
                Debug.Log("<color=green>[HARNESS ÉXITO]</color> DamageInfo rechazó correctamente float.NaN.");
            }
            // Prueba C: Curación Inválida (-50 y NaN)
            float curacionInvalida = _healthComponent.Heal(-50f);
            float curacionNaN = _healthComponent.Heal(float.NaN);
            if (curacionInvalida == 0f && curacionNaN == 0f)
            {
                Debug.Log("<color=green>[HARNESS ÉXITO]</color> HealthComponent rechazó la curación negativa y NaN devolviendo 0.");

            }
            else
            {
                Debug.LogError("[HARNESS FALLO] Curación inválida no retornó 0.");
            }
            Debug.Log("--- FIN DE PRUEBAS DE ENTRADA INVÁLIDA ---");
        }
        private void HandleHealthChanged(float current, float max) => Debug.Log($"<color=cyan>[EVENTO] OnHealthChanged:</color> {current}/{max}");
        private void HandleDamaged(DamageResult r) => Debug.Log($"<color=yellow>[EVENTO] OnDamaged:</color> Aplicado ={r.AppliedAmount}, Restante ={r.RemainingHealth}");
        private void HandleDied()
        {
            _onDiedCount++;
            Debug.LogWarning($"<color=red>[EVENTO OnDied]</color> Se ha notificado la muerte. Conteo total de OnDied:{_onDiedCount}");
        }
    }
}