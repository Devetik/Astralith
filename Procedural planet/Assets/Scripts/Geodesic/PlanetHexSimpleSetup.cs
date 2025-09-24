using UnityEngine;

/// <summary>
/// Setup ultra-simple pour le système de planète hexagonale
/// Évite tous les problèmes de performance et de boucles infinies
/// </summary>
public class PlanetHexSimpleSetup : MonoBehaviour
{
    [Header("Configuration")]
    public bool setupOnStart = true;
    public bool showDebugInfo = true;

    [Header("Paramètres de Génération")]
    public int frequency = 11; // n=11 ≈ 1212 cellules
    public float radius = 10000f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (setupOnStart)
        {
            SetupHexPlanet();
        }
    }

    /// <summary>
    /// Setup ultra-simple de la planète hexagonale
    /// </summary>
    [ContextMenu("Setup Planète Hexagonale Simple")]
    public void SetupHexPlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== SETUP PLANÈTE HEXAGONALE SIMPLE ===");
        }

        // Crée un nouveau GameObject pour la planète
        GameObject hexPlanet = new GameObject("HexPlanet");
        hexPlanet.transform.position = Vector3.zero;

        // Ajoute les composants nécessaires
        var hexWorld = hexPlanet.AddComponent<PlanetHexWorld>();
        var tileSelector = hexPlanet.AddComponent<PlanetTileSelector>();

        // Configure les paramètres
        hexWorld.frequency = frequency;
        hexWorld.radius = radius;
        hexWorld.seed = seed;
        hexWorld.buildLatitudeDeg = buildLatitudeDeg;
        hexWorld.excludePentagonsFromBuild = true;
        hexWorld.generateOnStart = false; // On génère manuellement
        hexWorld.drawGizmos = showDebugInfo;

        // Configure le sélecteur
        tileSelector.cam = Camera.main;
        tileSelector.highlightColor = Color.yellow;

        // Assigne le matériau
        if (planetMaterial != null)
        {
            var renderer = hexPlanet.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = planetMaterial;
            }
        }

        // Génère la planète
        hexWorld.Generate();

        if (showDebugInfo)
        {
            Debug.Log($"Planète générée: {hexWorld.cells.Count} cellules");
            int hexCount = hexWorld.cells.FindAll(c => !c.isPentagon).Count;
            int pentCount = hexWorld.cells.FindAll(c => c.isPentagon).Count;
            int buildableCount = hexWorld.cells.FindAll(c => c.canBuild).Count;
            
            Debug.Log($"- Hexagones: {hexCount}");
            Debug.Log($"- Pentagones: {pentCount}");
            Debug.Log($"- Constructibles: {buildableCount}");
            Debug.Log("=== SETUP TERMINÉ ===");
        }
    }

    /// <summary>
    /// Génère une nouvelle seed
    /// </summary>
    [ContextMenu("Nouvelle Seed")]
    public void GenerateNewSeed()
    {
        seed = Random.Range(0, int.MaxValue);
        
        var hexWorld = FindObjectOfType<PlanetHexWorld>();
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
    /// Test de génération
    /// </summary>
    [ContextMenu("Test Génération")]
    public void TestGeneration()
    {
        SetupHexPlanet();
    }
}
