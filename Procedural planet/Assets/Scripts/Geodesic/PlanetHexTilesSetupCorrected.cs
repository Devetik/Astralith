using UnityEngine;

/// <summary>
/// Setup corrigé pour les tuiles hexagonales - une seule sphère, vrais hexagones
/// </summary>
public class PlanetHexTilesSetupCorrected : MonoBehaviour
{
    [Header("Configuration Planète")]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;

    [Header("Matériaux")]
    public Material landMaterial;
    public Material waterMaterial;
    public Material buildableMaterial;

    [Header("Composants")]
    public PlanetHexTilesCorrected hexTiles;
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
        Debug.Log("=== SETUP PLANÈTE HEXAGONALE CORRIGÉE ===");

        try
        {
            // Trouve ou crée le composant PlanetHexTilesCorrected
            hexTiles = GetComponent<PlanetHexTilesCorrected>();
            if (hexTiles == null)
            {
                hexTiles = gameObject.AddComponent<PlanetHexTilesCorrected>();
            }

            // Configure les paramètres
            hexTiles.frequency = frequency;
            hexTiles.radius = radius;
            hexTiles.seed = seed;
            hexTiles.buildLatitudeDeg = buildLatitudeDeg;
            hexTiles.showDebugInfo = true;

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
}
