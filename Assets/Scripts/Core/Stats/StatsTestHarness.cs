using System;
using UnityEngine;
using Game.Core.Health;

namespace Game.Core.Stats
{
    /// <summary>
    /// Arnés de pruebas exhaustivo para verificar el sistema de datos base y estadísticas.
    /// Se ejecuta desde el menú contextual del Inspector de Unity.
    /// </summary>
    
    [RequireComponent(
    typeof(CharacterStatsComponent),
    typeof(HealthComponent))]
        public class StatsTestHarness : MonoBehaviour
    {
        [SerializeField] private CharacterStatsComponent _statsComponent;
        [SerializeField] private HealthComponent _healthComponent;
        [SerializeField] private CharacterDataSO _testCharacterAsset;

        private void Reset()
        {
            _statsComponent = GetComponent<CharacterStatsComponent>();
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void Awake()
        {
            if (_statsComponent == null) _statsComponent = GetComponent<CharacterStatsComponent>();
            if (_healthComponent == null) _healthComponent = GetComponent<HealthComponent>();
        }

  
        [ContextMenu("1. Prueba: Consulta de Estadísticas Válidas")]
        public void TestValidStatsQuery()
        {
            if (_statsComponent == null)
                _statsComponent = GetComponent<CharacterStatsComponent>();

            if (_statsComponent == null)
            {
                Debug.LogError("Falta CharacterStatsComponent.", this);
                return;
            }

            if (!_statsComponent.IsInitialized)
            {
                if (_testCharacterAsset == null)
                {
                    Debug.LogError("Asigna _testCharacterAsset en el Inspector.", this);
                    return;
                }

                if (!_statsComponent.Initialize(_testCharacterAsset))
                    return;
            }

            Debug.Log($"Personaje: {_statsComponent.CharacterName}");

            foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
            {
                if (_statsComponent.TryGetStatValue(type, out float value))
                    Debug.Log($"{type}: {value}");
                else
                    Debug.LogWarning($"No se encontró la estadística {type}.");
            }
        }
        
        
        [ContextMenu("2. Prueba: Sanitización de Datos Inválidos (NaN / Negativos)")]
        public void TestInvalidDataSanitization()
        {
            Debug.Log("--- PRUEBA 2: SANITIZACIÓN DE DATOS INVÁLIDOS ---");

            StatEntry entryNaN = new StatEntry(StatType.MaxHealth, float.NaN);
            StatEntry entryNegative = new StatEntry(StatType.MoveSpeed, -10f);

            bool isNaNValid = entryNaN.IsValid(out string errNaN);
            bool isNegValid = entryNegative.IsValid(out string errNeg);

            Debug.Log($"entryNaN es válida: {isNaNValid} -> Error: {errNaN}");
            Debug.Log($"entryNegative es válida: {isNegValid} -> Error: {errNeg}");

            entryNaN.Sanitize();
            entryNegative.Sanitize();

            Debug.Log($"entryNaN sanitizada: {entryNaN.Value}");
            Debug.Log($"entryNegative sanitizada: {entryNegative.Value}");
        }

        [ContextMenu("3. Prueba: Detección de Registros Duplicados")]
        public void TestDuplicateStatEntries()
        {
            Debug.Log("--- PRUEBA 3: DETECCIÓN DE REGISTROS DUPLICADOS ---");

            CharacterDataSO mockData = ScriptableObject.CreateInstance<CharacterDataSO>();
            Debug.Log("Verificando tolerancia a duplicados en CharacterDataSO...");
            mockData.ValidateAndSanitize();
            Debug.Log($"MaxHealth obtenido en mockData: {mockData.MaxHealth}");

            DestroyImmediate(mockData);
        }

        [ContextMenu("4. Prueba: Independencia entre Instancias Compartidas")]
        public void TestInstanceIndependence()
        {
            Debug.Log("--- PRUEBA 4: INDEPENDENCIA DE INSTANCIAS ---");
            if (_testCharacterAsset == null)
            {
                Debug.LogError("Asigna un CharacterDataSO en _testCharacterAsset.");
                return;
            }

            GameObject entityA = new GameObject("Test_Hero_A");
            GameObject entityB = new GameObject("Test_Hero_B");

            CharacterStatsComponent statsA = entityA.AddComponent<CharacterStatsComponent>();
            CharacterStatsComponent statsB = entityB.AddComponent<CharacterStatsComponent>();

            HealthComponent healthA = entityA.AddComponent<HealthComponent>();
            HealthComponent healthB = entityB.AddComponent<HealthComponent>();

            healthA.Initialize(_testCharacterAsset);
            healthB.Initialize(_testCharacterAsset);

            Debug.Log($"A - HP Inicial: {healthA.CurrentHealth}/{healthA.MaxHealth}");
            Debug.Log($"B - HP Inicial: {healthB.CurrentHealth}/{healthB.MaxHealth}");

            healthA.TakeDamage(new DamageInfo(35f, gameObject));

            Debug.Log($"Tras daño a A -> A HP: {healthA.CurrentHealth}/{healthA.MaxHealth}");
            Debug.Log($"Tras daño a A -> B HP: {healthB.CurrentHealth}/{healthB.MaxHealth}");

            if (healthA.CurrentHealth == (healthA.MaxHealth - 35f) && healthB.CurrentHealth == healthB.MaxHealth)
            {
                Debug.Log("<color=green>[ÉXITO]</color> Las dos entidades comparten el ScriptableObject pero mantienen un estado runtime 100% aislado.");
            }
            else
            {
                Debug.LogError("[FALLO] Ocurrió una contaminación de estado entre las dos entidades.");
            }

            DestroyImmediate(entityA);
            DestroyImmediate(entityB);
        }

        [ContextMenu("5. Prueba: Integración con HealthComponent")]
        public void TestHealthComponentIntegration()
        {
            Debug.Log("--- PRUEBA 5: INTEGRACIÓN CON HEALTH COMPONENT ---");
            if (_healthComponent == null) _healthComponent = GetComponent<HealthComponent>();

            if (!_healthComponent.IsInitialized)
            {
                if (_testCharacterAsset != null)
                {
                    _healthComponent.Initialize(_testCharacterAsset);
                }
                else
                {
                    Debug.LogError("Asigna un CharacterDataSO en _testCharacterAsset.");
                    return;
                }
            }

            Debug.Log($"[Integración] HealthComponent MaxHealth: {_healthComponent.MaxHealth}");
            Debug.Log($"[Integración] HealthComponent CurrentHealth: {_healthComponent.CurrentHealth}");

            DamageResult result = _healthComponent.TakeDamage(new DamageInfo(20f, gameObject));
            Debug.Log($"[Integración] Resultado daño: Applied={result.AppliedAmount}, Remaining={result.RemainingHealth}/{result.MaxHealth}");
        }
    }
}
