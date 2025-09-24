using UnityEngine;

/// <summary>
/// Version finale optimisée basée sur les tests
/// </summary>
public class PlanetHexFinal : MonoBehaviour
{
    [Header("Configuration Finale")]
    public int frequency = 5; // Bon compromis performance/qualité
    public float radius = 100f;
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
            Debug.Log("=== DÉBUT VERSION FINALE ===");
        }
        
        CreateFinalPlanet();
    }

    /// <summary>
    /// Crée la planète finale optimisée
    /// </summary>
    [ContextMenu("Créer Planète Finale")]
    public void CreateFinalPlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("Création planète finale...");
        }

        try
        {
            // Crée un GameObject
            GameObject planet = new GameObject("FinalPlanet");
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
                Debug.Log("Génération planète finale...");
            }
            
            // Génère la planète
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Planète finale créée: {hexWorld.cells.Count} cellules");
                int hexCount = hexWorld.cells.FindAll(c => !c.isPentagon).Count;
                int pentCount = hexWorld.cells.FindAll(c => c.isPentagon).Count;
                int buildableCount = hexWorld.cells.FindAll(c => c.canBuild).Count;
                
                Debug.Log($"- Hexagones: {hexCount}");
                Debug.Log($"- Pentagones: {pentCount}");
                Debug.Log($"- Constructibles: {buildableCount}");
                Debug.Log($"- Ratio constructible: {(float)buildableCount / hexWorld.cells.Count * 100f:F1}%");
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
    [ContextMenu("Test Fréquence 3")]
    public void TestFrequency3()
    {
        TestWithFrequency(3, 50f);
    }

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
                Debug.Log($"  - Hexagones: {hexCount}");
                Debug.Log($"  - Pentagones: {pentCount}");
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
               $"Ratio constructible: {(float)buildableCount / totalCells * 100f:F1}%";
    }
}
