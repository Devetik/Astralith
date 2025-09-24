using UnityEngine;

/// <summary>
/// Version finale optimisée basée sur les tests réussis
/// </summary>
public class PlanetHexOptimal : MonoBehaviour
{
    [Header("Configuration Optimale")]
    public int frequency = 7; // Testé et fonctionnel
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;
    public bool showDebugInfo = true;

    [Header("Matériaux")]
    public Material planetMaterial;

    [Header("Composants")]
    public PlanetHexWorld hexWorld;
    public PlanetTileSelector tileSelector;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT VERSION OPTIMALE ===");
        }
        
        CreateOptimalPlanet();
    }

    /// <summary>
    /// Crée la planète optimale
    /// </summary>
    [ContextMenu("Créer Planète Optimale")]
    public void CreateOptimalPlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("Création planète optimale...");
        }

        try
        {
            // Crée un GameObject
            GameObject planet = new GameObject("OptimalPlanet");
            planet.transform.position = Vector3.zero;

            // Ajoute les composants
            hexWorld = planet.AddComponent<PlanetHexWorld>();
            tileSelector = planet.AddComponent<PlanetTileSelector>();
            
            // Configure PlanetHexWorld
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = buildLatitudeDeg;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            hexWorld.drawGizmos = showDebugInfo;

            // Configure PlanetTileSelector
            tileSelector.cam = Camera.main;
            tileSelector.highlightColor = Color.yellow;

            // Assigne le matériau
            if (planetMaterial != null)
            {
                var renderer = planet.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = planetMaterial;
                }
            }

            if (showDebugInfo)
            {
                Debug.Log("Génération planète optimale...");
            }
            
            // Génère la planète
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Planète optimale créée: {hexWorld.cells.Count} cellules");
                int hexCount = hexWorld.cells.FindAll(c => !c.isPentagon).Count;
                int pentCount = hexWorld.cells.FindAll(c => c.isPentagon).Count;
                int buildableCount = hexWorld.cells.FindAll(c => c.canBuild).Count;
                
                Debug.Log($"- Hexagones: {hexCount}");
                Debug.Log($"- Pentagones: {pentCount}");
                Debug.Log($"- Constructibles: {buildableCount}");
                Debug.Log($"- Ratio constructible: {(float)buildableCount / hexWorld.cells.Count * 100f:F1}%");
                Debug.Log($"- Ratio hexagones: {(float)hexCount / hexWorld.cells.Count * 100f:F2}%");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la création: {e.Message}");
        }
    }

    /// <summary>
    /// Génère une nouvelle seed
    /// </summary>
    [ContextMenu("Nouvelle Seed")]
    public void GenerateNewSeed()
    {
        seed = Random.Range(0, int.MaxValue);
        
        if (hexWorld != null)
        {
            hexWorld.seed = seed;
            hexWorld.Generate();
        }

        if (showDebugInfo)
        {
            Debug.Log($"Nouvelle seed générée: {seed}");
        }
    }

    /// <summary>
    /// Test avec différentes fréquences
    /// </summary>
    [ContextMenu("Test Fréquence 5")]
    public void TestFrequency5()
    {
        TestWithFrequency(5, 100f);
    }

    [ContextMenu("Test Fréquence 7")]
    public void TestFrequency7()
    {
        TestWithFrequency(7, 150f);
    }

    [ContextMenu("Test Fréquence 9")]
    public void TestFrequency9()
    {
        TestWithFrequency(9, 200f);
    }

    /// <summary>
    /// Test avec une fréquence spécifique
    /// </summary>
    private void TestWithFrequency(int testFrequency, float testRadius)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Test avec fréquence {testFrequency}, rayon {testRadius}");
        }

        try
        {
            // Crée un GameObject de test
            GameObject testPlanet = new GameObject($"TestPlanet_Freq{testFrequency}");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var testHexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Configure les paramètres de test
            testHexWorld.frequency = testFrequency;
            testHexWorld.radius = testRadius;
            testHexWorld.seed = seed;
            testHexWorld.buildLatitudeDeg = buildLatitudeDeg;
            testHexWorld.excludePentagonsFromBuild = true;
            testHexWorld.generateOnStart = false;
            testHexWorld.drawGizmos = false;

            // Génère la planète de test
            testHexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"✅ Test réussi: {testHexWorld.cells.Count} cellules");
                int hexCount = testHexWorld.cells.FindAll(c => !c.isPentagon).Count;
                int pentCount = testHexWorld.cells.FindAll(c => c.isPentagon).Count;
                int buildableCount = testHexWorld.cells.FindAll(c => c.canBuild).Count;
                
                Debug.Log($"  - Hexagones: {hexCount}");
                Debug.Log($"  - Pentagones: {pentCount}");
                Debug.Log($"  - Constructibles: {buildableCount}");
                Debug.Log($"  - Ratio constructible: {(float)buildableCount / testHexWorld.cells.Count * 100f:F1}%");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Test échoué: {e.Message}");
        }
    }

    /// <summary>
    /// Obtient les statistiques de la planète
    /// </summary>
    public string GetPlanetStats()
    {
        if (hexWorld == null || hexWorld.cells == null)
        {
            return "Générateur non initialisé";
        }

        int totalCells = hexWorld.cells.Count;
        int hexCount = hexWorld.cells.FindAll(c => !c.isPentagon).Count;
        int pentCount = hexWorld.cells.FindAll(c => c.isPentagon).Count;
        int buildableCount = hexWorld.cells.FindAll(c => c.canBuild).Count;

        return $"Cellules: {totalCells}\n" +
               $"Hexagones: {hexCount}\n" +
               $"Pentagones: {pentCount}\n" +
               $"Constructibles: {buildableCount}\n" +
               $"Ratio constructible: {(float)buildableCount / totalCells * 100f:F1}%\n" +
               $"Ratio hexagones: {(float)hexCount / totalCells * 100f:F2}%";
    }

    /// <summary>
    /// Test de performance
    /// </summary>
    [ContextMenu("Test Performance")]
    public void TestPerformance()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST PERFORMANCE ===");
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Crée un GameObject de test
            GameObject testPlanet = new GameObject("PerformanceTestPlanet");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var testHexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Configure les paramètres
            testHexWorld.frequency = frequency;
            testHexWorld.radius = radius;
            testHexWorld.seed = seed;
            testHexWorld.buildLatitudeDeg = buildLatitudeDeg;
            testHexWorld.excludePentagonsFromBuild = true;
            testHexWorld.generateOnStart = false;
            testHexWorld.drawGizmos = false;

            // Génère la planète
            testHexWorld.Generate();
            
            stopwatch.Stop();
            
            if (showDebugInfo)
            {
                Debug.Log($"✅ Performance test réussi:");
                Debug.Log($"  - Temps: {stopwatch.ElapsedMilliseconds}ms");
                Debug.Log($"  - Cellules: {testHexWorld.cells.Count}");
                Debug.Log($"  - Cellules/ms: {testHexWorld.cells.Count / (float)stopwatch.ElapsedMilliseconds:F2}");
            }
        }
        catch (System.Exception e)
        {
            stopwatch.Stop();
            Debug.LogError($"❌ Test performance échoué: {e.Message}");
            Debug.LogError($"Temps écoulé: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
