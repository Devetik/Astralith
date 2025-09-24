using UnityEngine;

/// <summary>
/// Setup pour le système de planète hexagonale RimWorld-like
/// </summary>
public class PlanetHexSetup : MonoBehaviour
{
    [Header("Configuration")]
    public bool setupOnStart = true;
    public bool showDebugInfo = true;

    [Header("Paramètres de Génération")]
    public int frequency = 11; // n=11 ≈ 1212 cellules
    public float radius = 10000f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;
    public bool excludePentagonsFromBuild = true;

    [Header("Biomes")]
    public float altitudeScale = 1f;
    public float temperatureBase = 0.5f;
    public float humidityBase = 0.5f;

    [Header("Matériaux")]
    public Material planetMaterial;

    [Header("Composants")]
    public PlanetHexWorld hexWorld;
    public PlanetTileSelector tileSelector;

    void Start()
    {
        if (setupOnStart)
        {
            SetupHexPlanet();
        }
    }

    /// <summary>
    /// Configure le système de planète hexagonale
    /// </summary>
    [ContextMenu("Setup Planète Hexagonale")]
    public void SetupHexPlanet()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== SETUP PLANÈTE HEXAGONALE RIMWORLD-LIKE ===");
        }

        // Trouve ou crée les composants
        FindOrCreateComponents();

        // Configure les composants
        ConfigureComponents();

        // Génère la planète
        GeneratePlanet();

        if (showDebugInfo)
        {
            Debug.Log("=== SETUP TERMINÉ ===");
        }
    }

    /// <summary>
    /// Trouve ou crée les composants nécessaires
    /// </summary>
    private void FindOrCreateComponents()
    {
        // Trouve ou crée PlanetHexWorld
        if (hexWorld == null)
        {
            hexWorld = GetComponent<PlanetHexWorld>();
            if (hexWorld == null)
            {
                hexWorld = gameObject.AddComponent<PlanetHexWorld>();
                if (showDebugInfo)
                {
                    Debug.Log("PlanetHexWorld créé");
                }
            }
        }

        // Trouve ou crée PlanetTileSelector
        if (tileSelector == null)
        {
            tileSelector = GetComponent<PlanetTileSelector>();
            if (tileSelector == null)
            {
                tileSelector = gameObject.AddComponent<PlanetTileSelector>();
                if (showDebugInfo)
                {
                    Debug.Log("PlanetTileSelector créé");
                }
            }
        }
    }

    /// <summary>
    /// Configure les composants
    /// </summary>
    private void ConfigureComponents()
    {
        // Configure PlanetHexWorld
        if (hexWorld != null)
        {
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = buildLatitudeDeg;
            hexWorld.excludePentagonsFromBuild = excludePentagonsFromBuild;
            hexWorld.altitudeScale = altitudeScale;
            hexWorld.temperatureBase = temperatureBase;
            hexWorld.humidityBase = humidityBase;
            hexWorld.generateOnStart = false; // On génère manuellement
            hexWorld.drawGizmos = showDebugInfo;

            if (showDebugInfo)
            {
                Debug.Log("PlanetHexWorld configuré");
            }
        }

        // Configure PlanetTileSelector
        if (tileSelector != null)
        {
            tileSelector.cam = Camera.main;
            tileSelector.highlightColor = Color.yellow;

            if (showDebugInfo)
            {
                Debug.Log("PlanetTileSelector configuré");
            }
        }

        // Assigne le matériau
        if (planetMaterial != null)
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = planetMaterial;
                if (showDebugInfo)
                {
                    Debug.Log("Matériau assigné");
                }
            }
        }
    }

    /// <summary>
    /// Génère la planète
    /// </summary>
    [ContextMenu("Générer Planète")]
    public void GeneratePlanet()
    {
        if (hexWorld != null)
        {
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
            }
        }
        else
        {
            Debug.LogError("PlanetHexWorld non trouvé !");
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
               $"Constructibles: {buildableCount}";
    }

    /// <summary>
    /// Test de génération
    /// </summary>
    [ContextMenu("Test Génération")]
    public void TestGeneration()
    {
        GeneratePlanet();
    }

    /// <summary>
    /// Affiche les informations de debug
    /// </summary>
    [ContextMenu("Afficher Infos Debug")]
    public void ShowDebugInfo()
    {
        if (hexWorld != null && hexWorld.cells != null)
        {
            Debug.Log($"=== INFORMATIONS PLANÈTE HEXAGONALE ===");
            Debug.Log($"Cellules totales: {hexWorld.cells.Count}");
            Debug.Log($"Hexagones: {hexWorld.cells.FindAll(c => !c.isPentagon).Count}");
            Debug.Log($"Pentagones: {hexWorld.cells.FindAll(c => c.isPentagon).Count}");
            Debug.Log($"Constructibles: {hexWorld.cells.FindAll(c => c.canBuild).Count}");
            Debug.Log($"Rayon: {hexWorld.radius}");
            Debug.Log($"Fréquence: {hexWorld.frequency}");
            Debug.Log("=== FIN INFORMATIONS ===");
        }
    }
}
