using UnityEngine;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Setup pour remplacer l'ancien système par le nouveau géodésique
    /// Garde l'UI existant pour la compatibilité
    /// </summary>
    public class GeodesicDualSetup : MonoBehaviour
    {
        [Header("Configuration")]
        public bool replaceOldSystem = true;
        public bool showDebugInfo = true;
        
        [Header("Nouveau Système Géodésique")]
        public GeodesicDualPlanetGenerator geodesicGenerator;
        public GeodesicDualGrid geodesicGrid;
        
        [Header("Matériaux")]
        public Material landMaterial;
        public Material waterMaterial;
        
        [Header("Paramètres de Génération")]
        public int seed = 42;
        public float planetRadius = 5f;
        public int subdivisionLevel = 4; // ~1200 cellules
        public float noiseScale = 0.1f;
        public float landRatio = 0.3f; // 30% de terre, 70% d'eau
        
        private void Start()
        {
            if (replaceOldSystem)
            {
                SetupNewGeodesicSystem();
            }
        }
        
        /// <summary>
        /// Configure le nouveau système géodésique
        /// </summary>
        [ContextMenu("Setup Nouveau Système Géodésique")]
        public void SetupNewGeodesicSystem()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== SETUP NOUVEAU SYSTÈME GÉODÉSIQUE ===");
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
            // Trouve le générateur géodésique
            if (geodesicGenerator == null)
            {
                geodesicGenerator = FindObjectOfType<GeodesicDualPlanetGenerator>();
                if (geodesicGenerator == null)
                {
                    geodesicGenerator = gameObject.AddComponent<GeodesicDualPlanetGenerator>();
                    if (showDebugInfo)
                    {
                        Debug.Log("Générateur géodésique dual créé");
                    }
                }
            }
            
            // Trouve la grille géodésique
            if (geodesicGrid == null)
            {
                geodesicGrid = FindObjectOfType<GeodesicDualGrid>();
                if (geodesicGrid == null)
                {
                    geodesicGrid = gameObject.AddComponent<GeodesicDualGrid>();
                    if (showDebugInfo)
                    {
                        Debug.Log("Grille géodésique dual créée");
                    }
                }
            }
        }
        
        /// <summary>
        /// Configure les composants
        /// </summary>
        private void ConfigureComponents()
        {
            // Configure le générateur
            if (geodesicGenerator != null)
            {
                geodesicGenerator.seed = seed;
                geodesicGenerator.planetRadius = planetRadius;
                geodesicGenerator.subdivisionLevel = subdivisionLevel;
                geodesicGenerator.noiseScale = noiseScale;
                geodesicGenerator.landRatio = landRatio;
                geodesicGenerator.landMaterial = landMaterial;
                geodesicGenerator.waterMaterial = waterMaterial;
                geodesicGenerator.showDebugInfo = showDebugInfo;
                
                if (showDebugInfo)
                {
                    Debug.Log("Générateur géodésique configuré");
                }
            }
            
            // Configure la grille
            if (geodesicGrid != null)
            {
                geodesicGrid.subdivisionLevel = subdivisionLevel;
                geodesicGrid.planetRadius = planetRadius;
                geodesicGrid.showDebugInfo = showDebugInfo;
                
                if (showDebugInfo)
                {
                    Debug.Log("Grille géodésique configurée");
                }
            }
        }
        
        /// <summary>
        /// Génère la planète
        /// </summary>
        public void GeneratePlanet()
        {
            if (geodesicGenerator != null)
            {
                geodesicGenerator.GeneratePlanet();
                
                if (showDebugInfo)
                {
                    Debug.Log("Planète géodésique générée");
                }
            }
        }
        
        /// <summary>
        /// Génère une nouvelle seed (compatible avec l'ancien UI)
        /// </summary>
        public void GenerateNewSeed()
        {
            seed = Random.Range(0, int.MaxValue);
            
            if (geodesicGenerator != null)
            {
                geodesicGenerator.seed = seed;
                geodesicGenerator.GeneratePlanet();
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Nouvelle seed générée: {seed}");
            }
        }
        
        /// <summary>
        /// Obtient les statistiques de la planète (compatible avec l'ancien UI)
        /// </summary>
        public string GetPlanetStats()
        {
            if (geodesicGenerator != null)
            {
                return geodesicGenerator.GetPlanetStats();
            }
            return "Générateur non initialisé";
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
            if (geodesicGenerator != null && geodesicGenerator.geodesicGrid != null)
            {
                var grid = geodesicGenerator.geodesicGrid;
                Debug.Log($"=== INFORMATIONS GRILLE GÉODÉSIQUE DUAL ===");
                Debug.Log($"Cellules totales: {grid.cells.Count}");
                Debug.Log($"Cellules constructibles: {grid.cells.Count(c => c.isBuildable)}");
                Debug.Log($"Océans: {grid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Ocean)}");
                Debug.Log($"Clairières: {grid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Clearing)}");
                Debug.Log($"Montagnes: {grid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Mountain)}");
                Debug.Log($"Forêts: {grid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Forest)}");
                Debug.Log("=== FIN INFORMATIONS ===");
            }
        }
        
        /// <summary>
        /// Remplace complètement l'ancien système
        /// </summary>
        [ContextMenu("Remplacer Ancien Système")]
        public void ReplaceOldSystem()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== REMPLACEMENT ANCIEN SYSTÈME ===");
            }
            
            // Désactive l'ancien système
            var oldGenerator = FindObjectOfType<PlanetGenerator>();
            if (oldGenerator != null)
            {
                oldGenerator.gameObject.SetActive(false);
                if (showDebugInfo)
                {
                    Debug.Log("Ancien générateur désactivé");
                }
            }
            
            var oldNetworkedGenerator = FindObjectOfType<PlanetGeneratorNetworked>();
            if (oldNetworkedGenerator != null)
            {
                oldNetworkedGenerator.gameObject.SetActive(false);
                if (showDebugInfo)
                {
                    Debug.Log("Ancien générateur réseau désactivé");
                }
            }
            
            // Active le nouveau système
            SetupNewGeodesicSystem();
            
            if (showDebugInfo)
            {
                Debug.Log("=== REMPLACEMENT TERMINÉ ===");
            }
        }
    }
}
