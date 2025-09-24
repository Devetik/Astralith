using UnityEngine;

/// <summary>
/// Version ultra-simple qui évite tous les problèmes potentiels
/// </summary>
public class PlanetHexUltraSimple : MonoBehaviour
{
    [Header("Configuration Ultra-Simple")]
    public int frequency = 3; // Très petit pour commencer
    public float radius = 50f;
    public int seed = 12345;
    public bool showDebugInfo = true;

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT ULTRA-SIMPLE ===");
        }
        
        CreateUltraSimplePlanet();
    }

    /// <summary>
    /// Crée une planète ultra-simple
    /// </summary>
    [ContextMenu("Créer Planète Ultra-Simple")]
    public void CreateUltraSimplePlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("Création planète ultra-simple...");
        }

        try
        {
            // Crée un GameObject simple
            GameObject planet = new GameObject("UltraSimplePlanet");
            planet.transform.position = Vector3.zero;

            // Ajoute seulement PlanetHexWorld
            var hexWorld = planet.AddComponent<PlanetHexWorld>();
            
            // Configure avec des paramètres très petits
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            // hexWorld.showDebugInfo = showDebugInfo; // Cette propriété n'existe pas
            hexWorld.drawGizmos = false; // Désactive les gizmos pour éviter les problèmes

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
    /// Test avec des paramètres encore plus petits
    /// </summary>
    [ContextMenu("Test Paramètres Minimaux")]
    public void TestMinimalParameters()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST PARAMÈTRES MINIMAUX ===");
        }

        try
        {
            // Crée un GameObject
            GameObject testPlanet = new GameObject("MinimalTestPlanet");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Paramètres minimaux
            hexWorld.frequency = 2; // Minimum possible
            hexWorld.radius = 10f;  // Très petit
            hexWorld.seed = 12345;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            // hexWorld.showDebugInfo = true; // Cette propriété n'existe pas
            hexWorld.drawGizmos = false;

            if (showDebugInfo)
            {
                Debug.Log("Génération avec paramètres minimaux...");
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

    /// <summary>
    /// Test sans subdivision
    /// </summary>
    [ContextMenu("Test Sans Subdivision")]
    public void TestWithoutSubdivision()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST SANS SUBDIVISION ===");
        }

        try
        {
            // Crée un GameObject
            GameObject testPlanet = new GameObject("NoSubdivisionTestPlanet");
            testPlanet.transform.position = Vector3.zero;

            // Ajoute PlanetHexWorld
            var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
            
            // Paramètres pour éviter la subdivision
            hexWorld.frequency = 1; // Pas de subdivision
            hexWorld.radius = 10f;
            hexWorld.seed = 12345;
            hexWorld.buildLatitudeDeg = 70f;
            hexWorld.excludePentagonsFromBuild = true;
            hexWorld.generateOnStart = false;
            // hexWorld.showDebugInfo = true; // Cette propriété n'existe pas
            hexWorld.drawGizmos = false;

            if (showDebugInfo)
            {
                Debug.Log("Génération sans subdivision...");
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
