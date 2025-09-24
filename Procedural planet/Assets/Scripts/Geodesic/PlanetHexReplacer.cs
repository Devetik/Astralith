using UnityEngine;

/// <summary>
/// Remplace l'ancien système géodésique par le nouveau système RimWorld-like
/// </summary>
public class PlanetHexReplacer : MonoBehaviour
{
    [Header("Configuration")]
    public bool showDebugInfo = true;
    public bool replaceOnStart = false;

    [Header("Paramètres de Remplacement")]
    public int frequency = 11;
    public float radius = 10000f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (replaceOnStart)
        {
            ReplaceOldSystem();
        }
    }

    /// <summary>
    /// Remplace complètement l'ancien système par le nouveau
    /// </summary>
    [ContextMenu("Remplacer Ancien Système")]
    public void ReplaceOldSystem()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== REMPLACEMENT ANCIEN SYSTÈME ===");
        }

        // 1. Désactive l'ancien système
        DisableOldSystem();

        // 2. Crée le nouveau système
        CreateNewSystem();

        // 3. Configure le nouveau système
        ConfigureNewSystem();

        if (showDebugInfo)
        {
            Debug.Log("=== REMPLACEMENT TERMINÉ ===");
        }
    }

    /// <summary>
    /// Méthode alternative plus simple - crée juste le nouveau système
    /// </summary>
    [ContextMenu("Créer Nouveau Système (Simple)")]
    public void CreateNewSystemSimple()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== CRÉATION NOUVEAU SYSTÈME SIMPLE ===");
        }

        // Crée directement le nouveau système sans désactiver l'ancien
        CreateNewSystem();
        ConfigureNewSystem();

        if (showDebugInfo)
        {
            Debug.Log("=== CRÉATION TERMINÉE ===");
        }
    }

    /// <summary>
    /// Méthode ultra-simple - crée juste la planète hexagonale
    /// </summary>
    [ContextMenu("Créer Planète Hexagonale (Ultra-Simple)")]
    public void CreateHexPlanetUltraSimple()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== CRÉATION PLANÈTE HEXAGONALE ULTRA-SIMPLE ===");
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
            Debug.Log("=== CRÉATION TERMINÉE ===");
        }
    }

    /// <summary>
    /// Désactive l'ancien système
    /// </summary>
    private void DisableOldSystem()
    {
        if (showDebugInfo)
        {
            Debug.Log("Désactivation de l'ancien système...");
        }

        // Désactive les anciens générateurs par nom de type
        try
        {
            // Trouve tous les MonoBehaviour et filtre par nom de type
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var obj in allMonoBehaviours)
            {
                string typeName = obj.GetType().Name;
                
                // Désactive les anciens générateurs géodésiques
                if (typeName.Contains("GeodesicSphereSetup") || 
                    typeName.Contains("GeodesicDualSetup") ||
                    typeName.Contains("GeodesicSpherePlanetGenerator") ||
                    typeName.Contains("GeodesicDualPlanetGenerator") ||
                    typeName.Contains("GeodesicSphereGrid") ||
                    typeName.Contains("GeodesicDualGrid"))
                {
                    obj.gameObject.SetActive(false);
                    if (showDebugInfo)
                    {
                        Debug.Log($"Ancien générateur désactivé: {typeName} sur {obj.name}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"Erreur lors de la désactivation: {e.Message}");
            }
        }

        if (showDebugInfo)
        {
            Debug.Log("Ancien système désactivé");
        }
    }

    /// <summary>
    /// Crée le nouveau système
    /// </summary>
    private void CreateNewSystem()
    {
        // Crée un nouveau GameObject pour la planète hexagonale
        GameObject hexPlanet = new GameObject("HexPlanet");
        hexPlanet.transform.position = Vector3.zero;

        // Ajoute les composants nécessaires
        var hexWorld = hexPlanet.AddComponent<PlanetHexWorld>();
        var tileSelector = hexPlanet.AddComponent<PlanetTileSelector>();
        var setup = hexPlanet.AddComponent<PlanetHexSetup>();

        // Assigne les références
        setup.hexWorld = hexWorld;
        setup.tileSelector = tileSelector;

        if (showDebugInfo)
        {
            Debug.Log("Nouveau système créé");
        }
    }

    /// <summary>
    /// Configure le nouveau système
    /// </summary>
    private void ConfigureNewSystem()
    {
        var setup = FindObjectOfType<PlanetHexSetup>();
        if (setup != null)
        {
            // Configure les paramètres
            setup.frequency = frequency;
            setup.radius = radius;
            setup.seed = seed;
            setup.buildLatitudeDeg = buildLatitudeDeg;
            setup.planetMaterial = planetMaterial;
            setup.showDebugInfo = showDebugInfo;

            // Génère la planète
            setup.GeneratePlanet();

            if (showDebugInfo)
            {
                Debug.Log("Nouveau système configuré et généré");
            }
        }
    }

    /// <summary>
    /// Test de remplacement
    /// </summary>
    [ContextMenu("Test Remplacement")]
    public void TestReplacement()
    {
        ReplaceOldSystem();
    }

    /// <summary>
    /// Affiche les informations de debug
    /// </summary>
    [ContextMenu("Afficher Infos Debug")]
    public void ShowDebugInfo()
    {
        var hexWorld = FindObjectOfType<PlanetHexWorld>();
        if (hexWorld != null && hexWorld.cells != null)
        {
            Debug.Log($"=== INFORMATIONS NOUVEAU SYSTÈME ===");
            Debug.Log($"Cellules totales: {hexWorld.cells.Count}");
            Debug.Log($"Hexagones: {hexWorld.cells.FindAll(c => !c.isPentagon).Count}");
            Debug.Log($"Pentagones: {hexWorld.cells.FindAll(c => c.isPentagon).Count}");
            Debug.Log($"Constructibles: {hexWorld.cells.FindAll(c => c.canBuild).Count}");
            Debug.Log($"Rayon: {hexWorld.radius}");
            Debug.Log($"Fréquence: {hexWorld.frequency}");
            Debug.Log("=== FIN INFORMATIONS ===");
        }
        else
        {
            Debug.Log("Nouveau système non trouvé");
        }
    }
}
