using UnityEngine;

/// <summary>
/// Version simplifiée pour tester les paramètres sans yield return
/// </summary>
public class PlanetHexSimpleTest : MonoBehaviour
{
    [Header("Configuration Test")]
    public int frequency = 3;
    public float radius = 50f;
    public int seed = 12345;
    public bool showDebugInfo = true;

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT TEST SIMPLE ===");
        }
    }

    /// <summary>
    /// Test avec fréquence 3
    /// </summary>
    [ContextMenu("Test Fréquence 3")]
    public void TestFrequency3()
    {
        TestWithParameters(3, 50f, "Test Fréquence 3");
    }

    /// <summary>
    /// Test avec fréquence 5
    /// </summary>
    [ContextMenu("Test Fréquence 5")]
    public void TestFrequency5()
    {
        TestWithParameters(5, 100f, "Test Fréquence 5");
    }

    /// <summary>
    /// Test avec fréquence 7
    /// </summary>
    [ContextMenu("Test Fréquence 7")]
    public void TestFrequency7()
    {
        TestWithParameters(7, 150f, "Test Fréquence 7");
    }

    /// <summary>
    /// Test avec fréquence 9
    /// </summary>
    [ContextMenu("Test Fréquence 9")]
    public void TestFrequency9()
    {
        TestWithParameters(9, 200f, "Test Fréquence 9");
    }

    /// <summary>
    /// Test avec fréquence 11
    /// </summary>
    [ContextMenu("Test Fréquence 11")]
    public void TestFrequency11()
    {
        TestWithParameters(11, 250f, "Test Fréquence 11");
    }

    /// <summary>
    /// Test avec des paramètres spécifiques
    /// </summary>
    private void TestWithParameters(int testFrequency, float testRadius, string testName)
    {
        if (showDebugInfo)
        {
            Debug.Log($"--- {testName} (freq={testFrequency}, radius={testRadius}) ---");
        }

        try
        {
            // Crée un GameObject
            GameObject testPlanet = new GameObject($"TestPlanet_{testFrequency}_{testRadius}");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Configure les paramètres
            hexWorld.frequency = testFrequency;
            hexWorld.radius = testRadius;
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
                int buildableCount = hexWorld.cells.FindAll(c => c.canBuild).Count;
                
                Debug.Log($"  - Hexagones: {hexCount}");
                Debug.Log($"  - Pentagones: {pentCount}");
                Debug.Log($"  - Constructibles: {buildableCount}");
                Debug.Log($"  - Ratio constructible: {(float)buildableCount / hexWorld.cells.Count * 100f:F1}%");
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
            {
                Debug.LogError($"❌ {testName} échoué: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Crée une planète avec les paramètres actuels
    /// </summary>
    [ContextMenu("Créer Planète")]
    public void CreatePlanet()
    {
        TestWithParameters(frequency, radius, "Planète avec paramètres actuels");
    }

    /// <summary>
    /// Test progressif manuel
    /// </summary>
    [ContextMenu("Test Progressif Manuel")]
    public void TestProgressiveManual()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT TEST PROGRESSIF MANUEL ===");
        }

        // Test 1: Fréquence 3
        TestWithParameters(3, 50f, "Test 1: Fréquence 3");
        
        // Test 2: Fréquence 5
        TestWithParameters(5, 100f, "Test 2: Fréquence 5");
        
        // Test 3: Fréquence 7
        TestWithParameters(7, 150f, "Test 3: Fréquence 7");
        
        // Test 4: Fréquence 9
        TestWithParameters(9, 200f, "Test 4: Fréquence 9");
        
        // Test 5: Fréquence 11
        TestWithParameters(11, 250f, "Test 5: Fréquence 11");

        if (showDebugInfo)
        {
            Debug.Log("=== FIN TEST PROGRESSIF MANUEL ===");
        }
    }
}
