using UnityEngine;

/// <summary>
/// Version progressive qui augmente les paramètres étape par étape
/// </summary>
public class PlanetHexProgressive : MonoBehaviour
{
    [Header("Configuration Progressive")]
    public int startFrequency = 1;
    public int maxFrequency = 11;
    public float startRadius = 10f;
    public float maxRadius = 100f;
    public int seed = 12345;
    public bool showDebugInfo = true;
    public bool testProgressive = true;

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (testProgressive)
        {
            StartCoroutine(TestProgressiveGeneration());
        }
    }

    /// <summary>
    /// Test progressif des paramètres
    /// </summary>
    System.Collections.IEnumerator TestProgressiveGeneration()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT TEST PROGRESSIF ===");
        }

        // Test 1: Icosaèdre de base (frequency = 1)
        yield return StartCoroutine(TestWithParameters(1, 10f, "Icosaèdre de base"));

        // Test 2: Première subdivision (frequency = 2)
        yield return StartCoroutine(TestWithParameters(2, 20f, "Première subdivision"));

        // Test 3: Deuxième subdivision (frequency = 3)
        yield return StartCoroutine(TestWithParameters(3, 30f, "Deuxième subdivision"));

        // Test 4: Troisième subdivision (frequency = 4)
        yield return StartCoroutine(TestWithParameters(4, 40f, "Troisième subdivision"));

        // Test 5: Quatrième subdivision (frequency = 5)
        yield return StartCoroutine(TestWithParameters(5, 50f, "Quatrième subdivision"));

        if (showDebugInfo)
        {
            Debug.Log("=== FIN TEST PROGRESSIF ===");
        }
    }

    /// <summary>
    /// Test avec des paramètres spécifiques
    /// </summary>
    System.Collections.IEnumerator TestWithParameters(int frequency, float radius, string testName)
    {
        if (showDebugInfo)
        {
            Debug.Log($"--- Test: {testName} (freq={frequency}, radius={radius}) ---");
        }

        bool testSuccess = false;
        string errorMessage = "";

        try
        {
            // Crée un GameObject
            GameObject testPlanet = new GameObject($"TestPlanet_{frequency}_{radius}");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Configure les paramètres
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            hexWorld.drawGizmos = false;

            // Assigne le matériau
            if (planetMaterial != null)
            {
                var renderer = testPlanet.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = planetMaterial;
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"Tentative de génération: {testName}...");
            }
            
            // Génère la planète
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"✅ {testName} réussi: {hexWorld.cells.Count} cellules");
                int hexCount = hexWorld.cells.FindAll(c => !c.isPentagon).Count;
                int pentCount = hexWorld.cells.FindAll(c => c.isPentagon).Count;
                Debug.Log($"  - Hexagones: {hexCount}");
                Debug.Log($"  - Pentagones: {pentCount}");
            }

            testSuccess = true;
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
            if (showDebugInfo)
            {
                Debug.LogError($"❌ {testName} échoué: {e.Message}");
            }
        }

        // Attend un peu avant le test suivant
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Crée une planète avec les paramètres optimaux
    /// </summary>
    [ContextMenu("Créer Planète Optimale")]
    public void CreateOptimalPlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== CRÉATION PLANÈTE OPTIMALE ===");
        }

        try
        {
            // Crée un GameObject
            GameObject planet = new GameObject("OptimalPlanet");
            planet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = planet.AddComponent<PlanetHexWorld>();
            var tileSelector = planet.AddComponent<PlanetTileSelector>();
            
            // Configure avec des paramètres optimaux
            hexWorld.frequency = 5; // Bon compromis
            hexWorld.radius = 100f; // Rayon raisonnable
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            hexWorld.drawGizmos = showDebugInfo;

            // Configure le sélecteur
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
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la création: {e.Message}");
        }
    }

    /// <summary>
    /// Test avec une fréquence spécifique
    /// </summary>
    [ContextMenu("Test Fréquence 3")]
    public void TestFrequency3()
    {
        StartCoroutine(TestWithParameters(3, 50f, "Test Fréquence 3"));
    }

    /// <summary>
    /// Test avec une fréquence spécifique
    /// </summary>
    [ContextMenu("Test Fréquence 5")]
    public void TestFrequency5()
    {
        StartCoroutine(TestWithParameters(5, 100f, "Test Fréquence 5"));
    }

    /// <summary>
    /// Test avec une fréquence spécifique
    /// </summary>
    [ContextMenu("Test Fréquence 7")]
    public void TestFrequency7()
    {
        StartCoroutine(TestWithParameters(7, 150f, "Test Fréquence 7"));
    }
}
