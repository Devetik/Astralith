using UnityEngine;

/// <summary>
/// Version minimale qui évite tous les problèmes
/// </summary>
public class PlanetHexMinimal : MonoBehaviour
{
    [Header("Configuration Minimale")]
    public int frequency = 1; // Minimum absolu
    public float radius = 10f;
    public int seed = 12345;
    public bool showDebugInfo = true;

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT VERSION MINIMALE ===");
        }
        
        CreateMinimalPlanet();
    }

    /// <summary>
    /// Crée une planète minimale
    /// </summary>
    [ContextMenu("Créer Planète Minimale")]
    public void CreateMinimalPlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("Création planète minimale...");
        }

        try
        {
            // Crée un GameObject simple
            GameObject planet = new GameObject("MinimalPlanet");
            planet.transform.position = Vector3.zero;

            // Ajoute seulement PlanetHexWorld
            var hexWorld = planet.AddComponent<PlanetHexWorld>();
            
            // Configure avec des paramètres minimaux
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            hexWorld.drawGizmos = false; // Désactive les gizmos

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
                Debug.Log("Tentative de génération...");
            }
            
            // Génère la planète
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Planète créée avec succès: {hexWorld.cells.Count} cellules");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la création: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Test avec icosaèdre de base seulement
    /// </summary>
    [ContextMenu("Test Icosaèdre de Base")]
    public void TestBasicIcosahedron()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST ICOSAÈDRE DE BASE ===");
        }

        try
        {
            // Crée un GameObject
            GameObject testPlanet = new GameObject("BasicIcosahedronTest");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Paramètres pour icosaèdre de base seulement
            hexWorld.frequency = 1; // Pas de subdivision
            hexWorld.radius = 10f;
            hexWorld.seed = 12345;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            hexWorld.drawGizmos = false;

            if (showDebugInfo)
            {
                Debug.Log("Génération icosaèdre de base...");
            }
            
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Icosaèdre créé: {hexWorld.cells.Count} cellules");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur: {e.Message}");
        }
    }

    /// <summary>
    /// Test avec paramètres encore plus petits
    /// </summary>
    [ContextMenu("Test Paramètres Ultra-Minimaux")]
    public void TestUltraMinimalParameters()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST PARAMÈTRES ULTRA-MINIMAUX ===");
        }

        try
        {
            // Crée un GameObject
            GameObject testPlanet = new GameObject("UltraMinimalTestPlanet");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Paramètres ultra-minimaux
            hexWorld.frequency = 1; // Minimum absolu
            hexWorld.radius = 5f;   // Très petit
            hexWorld.seed = 12345;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            hexWorld.drawGizmos = false;

            if (showDebugInfo)
            {
                Debug.Log("Génération avec paramètres ultra-minimaux...");
            }
            
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Génération réussie: {hexWorld.cells.Count} cellules");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur: {e.Message}");
        }
    }
}
