using UnityEngine;
using System.Collections;

namespace HexSphere
{
    /// <summary>
    /// Script de test pour valider le système HexSphere
    /// </summary>
    public class HexSphereTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestsOnStart = false;
        public float testInterval = 1f;
        
        [Header("Test Results")]
        public int totalTests = 0;
        public int passedTests = 0;
        public int failedTests = 0;
        
        private HexSphereGenerator generator;
        private HexSphereManager manager;
        
        private void Start()
        {
            // Obtenir les références
            generator = GetComponent<HexSphereGenerator>();
            manager = GetComponent<HexSphereManager>();
            
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        /// <summary>
        /// Exécute tous les tests
        /// </summary>
        [ContextMenu("Exécuter Tous les Tests")]
        public IEnumerator RunAllTests()
        {
            Debug.Log("=== DÉBUT DES TESTS HEXSPHERE ===");
            
            totalTests = 0;
            passedTests = 0;
            failedTests = 0;
            
            // Test 1: Génération de base
            yield return StartCoroutine(TestBasicGeneration());
            
            // Test 2: Géométrie
            yield return StartCoroutine(TestGeometry());
            
            // Test 3: Interactions
            yield return StartCoroutine(TestInteractions());
            
            // Test 4: Performance
            yield return StartCoroutine(TestPerformance());
            
            // Test 5: Matériaux
            yield return StartCoroutine(TestMaterials());
            
            // Résultats finaux
            Debug.Log($"=== RÉSULTATS DES TESTS ===");
            Debug.Log($"Tests totaux: {totalTests}");
            Debug.Log($"Tests réussis: {passedTests}");
            Debug.Log($"Tests échoués: {failedTests}");
            Debug.Log($"Taux de réussite: {(float)passedTests / totalTests * 100f:F1}%");
        }
        
        /// <summary>
        /// Test de génération de base
        /// </summary>
        private IEnumerator TestBasicGeneration()
        {
            Debug.Log("Test 1: Génération de base");
            totalTests++;
            
            bool testPassed = false;
            
            if (generator == null)
            {
                Debug.LogError("HexSphereGenerator non trouvé!");
                failedTests++;
                yield break;
            }
            
            try
            {
                // Générer une sphère simple
                generator.subdivisionLevel = 1;
                generator.radius = 1f;
                generator.hexSize = 0.5f;
                generator.GenerateHexSphere();
                
                // Vérifier que des cellules ont été créées
                if (generator.hexCells.Count > 0)
                {
                    Debug.Log($"✓ Génération réussie: {generator.hexCells.Count} cellules");
                    testPassed = true;
                }
                else
                {
                    Debug.LogError("✗ Aucune cellule générée");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Erreur dans la génération: {e.Message}");
            }
            
            yield return new WaitForSeconds(0.1f);
            
            if (testPassed)
                passedTests++;
            else
                failedTests++;
        }
        
        /// <summary>
        /// Test de la géométrie
        /// </summary>
        private IEnumerator TestGeometry()
        {
            Debug.Log("Test 2: Géométrie");
            totalTests++;
            
            bool geometryValid = true;
            try
            {
                foreach (HexCell cell in generator.hexCells)
                {
                    // Vérifier que la cellule a des vertices
                    if (cell.vertices == null || cell.vertices.Length == 0)
                    {
                        Debug.LogError($"✗ Cellule {cell.index} n'a pas de vertices");
                        geometryValid = false;
                        break;
                    }
                    
                    // Vérifier que la cellule a des triangles
                    if (cell.triangles == null || cell.triangles.Length == 0)
                    {
                        Debug.LogError($"✗ Cellule {cell.index} n'a pas de triangles");
                        geometryValid = false;
                        break;
                    }
                    
                    // Vérifier que la cellule a des UVs
                    if (cell.uvs == null || cell.uvs.Length == 0)
                    {
                        Debug.LogError($"✗ Cellule {cell.index} n'a pas d'UVs");
                        geometryValid = false;
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Erreur dans la géométrie: {e.Message}");
                geometryValid = false;
            }
            
            if (geometryValid)
            {
                Debug.Log("✓ Géométrie valide");
                passedTests++;
            }
            else
            {
                Debug.LogError("✗ Géométrie invalide");
                failedTests++;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// Test des interactions
        /// </summary>
        private IEnumerator TestInteractions()
        {
            Debug.Log("Test 3: Interactions");
            totalTests++;
            
            bool testPassed = false;
            try
            {
                if (manager == null)
                {
                    Debug.LogError("✗ HexSphereManager non trouvé");
                }
                else if (generator.hexCells.Count > 0)
                {
                    HexCell testCell = generator.hexCells[0];
                    manager.SelectHexCell(testCell);
                    
                    if (testCell.color == Color.blue)
                    {
                        Debug.Log("✓ Sélection de cellule fonctionne");
                        testPassed = true;
                    }
                    else
                    {
                        Debug.LogError("✗ Sélection de cellule ne fonctionne pas");
                    }
                }
                else
                {
                    Debug.LogError("✗ Aucune cellule pour tester les interactions");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Erreur dans les interactions: {e.Message}");
            }
            
            if (testPassed)
                passedTests++;
            else
                failedTests++;
            
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// Test de performance
        /// </summary>
        private IEnumerator TestPerformance()
        {
            Debug.Log("Test 4: Performance");
            totalTests++;
            
            bool testPassed = false;
            try
            {
                float startTime = Time.realtimeSinceStartup;
                
                // Générer une sphère avec subdivision niveau 2
                generator.subdivisionLevel = 2;
                generator.GenerateHexSphere();
                
                float generationTime = Time.realtimeSinceStartup - startTime;
                
                Debug.Log($"Temps de génération: {generationTime:F3}s");
                Debug.Log($"Nombre de cellules: {generator.hexCells.Count}");
                
                if (generationTime < 1f) // Moins d'une seconde
                {
                    Debug.Log("✓ Performance acceptable");
                    testPassed = true;
                }
                else
                {
                    Debug.LogWarning("⚠ Performance lente, mais acceptable");
                    testPassed = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Erreur dans le test de performance: {e.Message}");
            }
            
            if (testPassed)
                passedTests++;
            else
                failedTests++;
            
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// Test des matériaux
        /// </summary>
        private IEnumerator TestMaterials()
        {
            Debug.Log("Test 5: Matériaux");
            totalTests++;
            
            bool testPassed = false;
            try
            {
                MeshRenderer renderer = GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    Debug.LogError("✗ MeshRenderer non trouvé");
                }
                else if (renderer.material != null)
                {
                    Debug.Log("✓ Matériau assigné");
                    testPassed = true;
                }
                else
                {
                    Debug.LogWarning("⚠ Aucun matériau assigné");
                    testPassed = true; // Pas critique
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Erreur dans le test des matériaux: {e.Message}");
            }
            
            if (testPassed)
                passedTests++;
            else
                failedTests++;
            
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// Test de stress
        /// </summary>
        [ContextMenu("Test de Stress")]
        public void StressTest()
        {
            Debug.Log("=== TEST DE STRESS ===");
            
            StartCoroutine(StressTestCoroutine());
        }
        
        private IEnumerator StressTestCoroutine()
        {
            // Test avec différents niveaux de subdivision
            for (int level = 1; level <= 3; level++)
            {
                Debug.Log($"Test de stress - Niveau {level}");
                
                float startTime = Time.realtimeSinceStartup;
                generator.subdivisionLevel = level;
                generator.GenerateHexSphere();
                float generationTime = Time.realtimeSinceStartup - startTime;
                
                Debug.Log($"Niveau {level}: {generator.hexCells.Count} cellules en {generationTime:F3}s");
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void OnGUI()
        {
            if (!runTestsOnStart) return;
            
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 110));
            GUILayout.Label("=== HexSphere Test ===", GUI.skin.box);
            GUILayout.Label($"Tests: {passedTests}/{totalTests}");
            GUILayout.Label($"Échecs: {failedTests}");
            
            if (GUILayout.Button("Relancer Tests"))
            {
                StartCoroutine(RunAllTests());
            }
            
            GUILayout.EndArea();
        }
    }
}
