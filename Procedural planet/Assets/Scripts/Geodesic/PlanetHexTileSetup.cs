using UnityEngine;

/// <summary>
/// Setup final pour le système de tuiles hexagonales
/// </summary>
public class PlanetHexTileSetup : MonoBehaviour
{
    [Header("Configuration")]
    public bool setupOnStart = true;
    public bool showDebugInfo = true;

    [Header("Paramètres de Génération")]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;
    public float tileSize = 1f;

    [Header("Matériaux")]
    public Material landMaterial;
    public Material waterMaterial;
    public Material buildableMaterial;

    [Header("Composants")]
    public PlanetHexTiles hexTiles;
    public PlanetHexTileSelector tileSelector;

    void Start()
    {
        if (setupOnStart)
        {
            SetupHexTileSystem();
        }
    }

    /// <summary>
    /// Configure le système de tuiles hexagonales
    /// </summary>
    [ContextMenu("Setup Système Tuiles Hexagonales")]
    public void SetupHexTileSystem()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== SETUP SYSTÈME TUILES HEXAGONALES ===");
        }

        // Trouve ou crée les composants
        FindOrCreateComponents();

        // Configure les composants
        ConfigureComponents();

        // Crée les tuiles
        CreateTiles();

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
        // Trouve ou crée PlanetHexTiles
        if (hexTiles == null)
        {
            hexTiles = GetComponent<PlanetHexTiles>();
            if (hexTiles == null)
            {
                hexTiles = gameObject.AddComponent<PlanetHexTiles>();
                if (showDebugInfo)
                {
                    Debug.Log("PlanetHexTiles créé");
                }
            }
        }

        // Trouve ou crée PlanetHexTileSelector
        if (tileSelector == null)
        {
            tileSelector = GetComponent<PlanetHexTileSelector>();
            if (tileSelector == null)
            {
                tileSelector = gameObject.AddComponent<PlanetHexTileSelector>();
                if (showDebugInfo)
                {
                    Debug.Log("PlanetHexTileSelector créé");
                }
            }
        }
    }

    /// <summary>
    /// Configure les composants
    /// </summary>
    private void ConfigureComponents()
    {
        // Configure PlanetHexTiles
        if (hexTiles != null)
        {
            hexTiles.frequency = frequency;
            hexTiles.radius = radius;
            hexTiles.seed = seed;
            hexTiles.buildLatitudeDeg = buildLatitudeDeg;
            hexTiles.tileSize = tileSize;
            hexTiles.landMaterial = landMaterial;
            hexTiles.waterMaterial = waterMaterial;
            hexTiles.buildableMaterial = buildableMaterial;
            hexTiles.showDebugInfo = showDebugInfo;

            if (showDebugInfo)
            {
                Debug.Log("PlanetHexTiles configuré");
            }
        }

        // Configure PlanetHexTileSelector
        if (tileSelector != null)
        {
            tileSelector.cam = Camera.main;
            tileSelector.highlightColor = Color.yellow;
            tileSelector.hexTiles = hexTiles;

            if (showDebugInfo)
            {
                Debug.Log("PlanetHexTileSelector configuré");
            }
        }
    }

    /// <summary>
    /// Crée les tuiles
    /// </summary>
    [ContextMenu("Créer Tuiles")]
    public void CreateTiles()
    {
        if (hexTiles != null)
        {
            hexTiles.CreateHexTiles();
            
            if (showDebugInfo)
            {
                Debug.Log($"Tuiles créées: {hexTiles.tileObjects.Count}");
            }
        }
        else
        {
            Debug.LogError("PlanetHexTiles non trouvé !");
        }
    }

    /// <summary>
    /// Génère une nouvelle seed
    /// </summary>
    [ContextMenu("Nouvelle Seed")]
    public void GenerateNewSeed()
    {
        seed = Random.Range(0, int.MaxValue);
        
        if (hexTiles != null)
        {
            hexTiles.seed = seed;
            hexTiles.CreateHexTiles();
        }

        if (showDebugInfo)
        {
            Debug.Log($"Nouvelle seed générée: {seed}");
        }
    }

    /// <summary>
    /// Nettoie les tuiles
    /// </summary>
    [ContextMenu("Nettoyer Tuiles")]
    public void ClearTiles()
    {
        if (hexTiles != null)
        {
            hexTiles.ClearTiles();
        }
    }

    /// <summary>
    /// Obtient les statistiques des tuiles
    /// </summary>
    public string GetTileStats()
    {
        if (hexTiles != null)
        {
            return hexTiles.GetTileStats();
        }
        return "Système non initialisé";
    }

    /// <summary>
    /// Test de génération
    /// </summary>
    [ContextMenu("Test Génération")]
    public void TestGeneration()
    {
        SetupHexTileSystem();
    }

    /// <summary>
    /// Affiche les informations de debug
    /// </summary>
    [ContextMenu("Afficher Infos Debug")]
    public void ShowDebugInfo()
    {
        if (hexTiles != null && hexTiles.hexWorld != null)
        {
            Debug.Log($"=== INFORMATIONS TUILES HEXAGONALES ===");
            Debug.Log($"Tuiles totales: {hexTiles.tileObjects.Count}");
            Debug.Log($"Cellules: {hexTiles.hexWorld.cells.Count}");
            Debug.Log($"Hexagones: {hexTiles.hexWorld.cells.FindAll(c => !c.isPentagon).Count}");
            Debug.Log($"Pentagones: {hexTiles.hexWorld.cells.FindAll(c => c.isPentagon).Count}");
            Debug.Log($"Constructibles: {hexTiles.hexWorld.cells.FindAll(c => c.canBuild).Count}");
            Debug.Log($"Rayon: {hexTiles.radius}");
            Debug.Log($"Fréquence: {hexTiles.frequency}");
            Debug.Log($"Taille des tuiles: {hexTiles.tileSize}");
            Debug.Log("=== FIN INFORMATIONS ===");
        }
    }
}
