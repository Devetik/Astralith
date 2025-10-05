using UnityEngine;

/// <summary>
/// Setup avec contrôle de distance pour les tuiles hexagonales
/// </summary>
public class PlanetHexTilesSetupWithDistance : MonoBehaviour
{
    [Header("Configuration Planète")]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;

    [Header("Contrôle Distance")]
    [Range(0.01f, 0.5f)]
    public float hexDistance = 0.08f;
    [Range(0.1f, 2.0f)]
    public float hexScale = 1.0f;

    [Header("Matériaux")]
    public Material landMaterial;
    public Material waterMaterial;
    public Material buildableMaterial;

    [Header("Composants")]
    public PlanetHexTilesWithDistanceControl hexTiles;
    public PlanetHexTileSelector tileSelector;

    void Start()
    {
        SetupHexagonalPlanet();
    }

    /// <summary>
    /// Configure la planète hexagonale
    /// </summary>
    [ContextMenu("Setup Planète Hexagonale")]
    public void SetupHexagonalPlanet()
    {
        Debug.Log("=== SETUP PLANÈTE HEXAGONALE AVEC CONTRÔLE DISTANCE ===");

        try
        {
            // Trouve ou crée le composant PlanetHexTilesWithDistanceControl
            hexTiles = GetComponent<PlanetHexTilesWithDistanceControl>();
            if (hexTiles == null)
            {
                hexTiles = gameObject.AddComponent<PlanetHexTilesWithDistanceControl>();
            }

            // Configure les paramètres
            hexTiles.frequency = frequency;
            hexTiles.radius = radius;
            hexTiles.seed = seed;
            hexTiles.buildLatitudeDeg = buildLatitudeDeg;
            hexTiles.showDebugInfo = true;

            // Configure le contrôle de distance
            hexTiles.hexDistance = hexDistance;
            hexTiles.hexScale = hexScale;

            // Assigne les matériaux
            hexTiles.landMaterial = landMaterial;
            hexTiles.waterMaterial = waterMaterial;
            hexTiles.buildableMaterial = buildableMaterial;

            // Crée les tuiles
            hexTiles.CreateHexTiles();

            // Trouve ou crée le sélecteur de tuiles
            tileSelector = GetComponent<PlanetHexTileSelector>();
            if (tileSelector == null)
            {
                tileSelector = gameObject.AddComponent<PlanetHexTileSelector>();
            }

            // Configure le sélecteur
            tileSelector.cam = Camera.main;
            tileSelector.selectedTileId = -1;
            tileSelector.highlightColor = Color.yellow;
            tileSelector.highlightScale = 1.1f;

            Debug.Log("✅ Setup planète hexagonale terminé !");
            Debug.Log($"📊 Statistiques: {hexTiles.GetTileStats()}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors du setup: {e.Message}");
        }
    }

    /// <summary>
    /// Met à jour la distance des hexagones
    /// </summary>
    public void UpdateHexDistance()
    {
        if (hexTiles != null)
        {
            hexTiles.hexDistance = hexDistance;
            hexTiles.hexScale = hexScale;
            hexTiles.CreateHexTiles();
            Debug.Log($"Distance hexagone mise à jour: {hexDistance:F3}");
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
            Debug.Log($"Nouvelle seed générée: {seed}");
        }
    }

    /// <summary>
    /// Obtient les statistiques
    /// </summary>
    public string GetPlanetStats()
    {
        if (hexTiles == null)
        {
            return "Planète non initialisée";
        }

        return hexTiles.GetTileStats();
    }

    /// <summary>
    /// Ajuste la distance des hexagones
    /// </summary>
    [ContextMenu("Ajuster Distance Hexagones")]
    public void AdjustHexDistance()
    {
        UpdateHexDistance();
    }
}
