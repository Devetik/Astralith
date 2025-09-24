using UnityEngine;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Setup pour le système géodésique sphérique (ballon de football)
    /// </summary>
    public class GeodesicSphereSetup : MonoBehaviour
    {
        [Header("Configuration")]
        public bool setupOnStart = true;
        public bool showDebugInfo = true;
        
        [Header("Nouveau Système Sphérique")]
        public GeodesicSpherePlanetGenerator sphereGenerator;
        public GeodesicSphereGrid sphereGrid;
        
        [Header("Matériaux")]
        public Material landMaterial;
        public Material waterMaterial;
        
        [Header("Paramètres de Génération")]
        public int seed = 42;
        
        [Header("Configuration des Cellules")]
        public CellConfiguration cellConfig = CellConfiguration._1212; // Configuration prédéfinie
        public int customFrequency = 11; // Fréquence personnalisée (si Custom)
        public float cellSize = 1f; // Taille d'une cellule
        public float landRatio = 0.3f; // 30% de terre, 70% d'eau
        
        public enum CellConfiguration
        {
            _252,        // n=5 = 252 cellules
            _642,        // n=8 = 642 cellules  
            _1212,       // n=11 = 1212 cellules
            _2252,       // n=15 = 2252 cellules
            _4002,       // n=20 = 4002 cellules
            _6002,       // n=25 = 6002 cellules
            _9002,       // n=30 = 9002 cellules
            _16002,      // n=40 = 16002 cellules
            _25002,      // n=50 = 25002 cellules
            _40002,      // n=63 = 40002 cellules
            Custom       // Fréquence personnalisée
        }
        
        /// <summary>
        /// Obtient la fréquence basée sur la configuration choisie
        /// </summary>
        public int GetFrequency()
        {
            switch (cellConfig)
            {
                case CellConfiguration._252:
                    return 5;   // 252 cellules
                case CellConfiguration._642:
                    return 8;   // 642 cellules
                case CellConfiguration._1212:
                    return 11;  // 1212 cellules
                case CellConfiguration._2252:
                    return 15;  // 2252 cellules
                case CellConfiguration._4002:
                    return 20;  // 4002 cellules
                case CellConfiguration._6002:
                    return 25;  // 6002 cellules
                case CellConfiguration._9002:
                    return 30;  // 9002 cellules
                case CellConfiguration._16002:
                    return 40;  // 16002 cellules
                case CellConfiguration._25002:
                    return 50;  // 25002 cellules
                case CellConfiguration._40002:
                    return 63;  // 40002 cellules
                case CellConfiguration.Custom:
                    return customFrequency;
                default:
                    return 11;
            }
        }
        
        /// <summary>
        /// Obtient le nombre de cellules pour la configuration actuelle
        /// </summary>
        public int GetCellCount()
        {
            int freq = GetFrequency();
            return 10 * freq * freq + 2;
        }
        
        /// <summary>
        /// Obtient le nom de la configuration actuelle
        /// </summary>
        public string GetConfigName()
        {
            int cellCount = GetCellCount();
            return $"{cellCount} cellules";
        }
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupSphereSystem();
            }
        }
        
        /// <summary>
        /// Configure le système sphérique
        /// </summary>
        [ContextMenu("Setup Système Sphérique")]
        public void SetupSphereSystem()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== SETUP SYSTÈME SPHÉRIQUE ===");
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
            // Trouve le générateur sphérique
            if (sphereGenerator == null)
            {
                sphereGenerator = FindObjectOfType<GeodesicSpherePlanetGenerator>();
                if (sphereGenerator == null)
                {
                    sphereGenerator = gameObject.AddComponent<GeodesicSpherePlanetGenerator>();
                    if (showDebugInfo)
                    {
                        Debug.Log("Générateur sphérique créé");
                    }
                }
            }
            
            // Trouve la grille sphérique
            if (sphereGrid == null)
            {
                sphereGrid = FindObjectOfType<GeodesicSphereGrid>();
                if (sphereGrid == null)
                {
                    sphereGrid = gameObject.AddComponent<GeodesicSphereGrid>();
                    if (showDebugInfo)
                    {
                        Debug.Log("Grille sphérique créée");
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
            if (sphereGenerator != null)
            {
                sphereGenerator.seed = seed;
                sphereGenerator.frequency = GetFrequency();
                sphereGenerator.cellSize = cellSize;
                sphereGenerator.landRatio = landRatio;
                sphereGenerator.landMaterial = landMaterial;
                sphereGenerator.waterMaterial = waterMaterial;
                sphereGenerator.showDebugInfo = showDebugInfo;
                
                if (showDebugInfo)
                {
                    Debug.Log("Générateur sphérique configuré");
                }
            }
            
            // Configure la grille
            if (sphereGrid != null)
            {
                sphereGrid.frequency = GetFrequency();
                sphereGrid.cellSize = cellSize;
                sphereGrid.showDebugInfo = showDebugInfo;
                
                if (showDebugInfo)
                {
                    Debug.Log("Grille sphérique configurée");
                }
            }
        }
        
        /// <summary>
        /// Génère la planète
        /// </summary>
        public void GeneratePlanet()
        {
            if (sphereGenerator != null)
            {
                sphereGenerator.GeneratePlanet();
                
                if (showDebugInfo)
                {
                    Debug.Log("Planète sphérique générée");
                }
            }
        }
        
        /// <summary>
        /// Génère une nouvelle seed (compatible avec l'ancien UI)
        /// </summary>
        public void GenerateNewSeed()
        {
            seed = Random.Range(0, int.MaxValue);
            
            if (sphereGenerator != null)
            {
                sphereGenerator.seed = seed;
                sphereGenerator.GeneratePlanet();
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
            if (sphereGenerator != null)
            {
                return sphereGenerator.GetPlanetStats();
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
            if (sphereGenerator != null && sphereGenerator.sphereGrid != null)
            {
                var grid = sphereGenerator.sphereGrid;
                Debug.Log($"=== INFORMATIONS GRILLE SPHÉRIQUE ===");
                Debug.Log($"Cellules totales: {grid.cells.Count}");
                Debug.Log($"Cellules constructibles: {grid.cells.Count(c => c.isBuildable)}");
                Debug.Log($"Eau: {grid.cells.Count(c => c.cellType == GeodesicSphereCell.CellType.Water)}");
                Debug.Log($"Terre: {grid.cells.Count(c => c.cellType == GeodesicSphereCell.CellType.Land)}");
                Debug.Log($"Pentagones: {grid.cells.Count(c => c.sides == 5)}");
                Debug.Log($"Hexagones: {grid.cells.Count(c => c.sides == 6)}");
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
            SetupSphereSystem();
            
            if (showDebugInfo)
            {
                Debug.Log("=== REMPLACEMENT TERMINÉ ===");
            }
        }
        
        [ContextMenu("Afficher Info Configuration")]
        public void ShowConfigInfo()
        {
            int freq = GetFrequency();
            int cellCount = GetCellCount();
            string configName = GetConfigName();
            
            Debug.Log($"=== CONFIGURATION ACTUELLE ===");
            Debug.Log($"Configuration: {configName}");
            Debug.Log($"Fréquence: {freq}");
            Debug.Log($"Nombre de cellules: {cellCount}");
            Debug.Log($"Taille des cellules: {cellSize}");
            Debug.Log($"Ratio terre: {landRatio * 100f:F1}%");
            Debug.Log("=============================");
        }
        
        [ContextMenu("Liste Toutes Configurations")]
        public void ListAllConfigurations()
        {
            Debug.Log("=== CONFIGURATIONS DISPONIBLES ===");
            Debug.Log("252 cellules (n=5)");
            Debug.Log("642 cellules (n=8)");
            Debug.Log("1212 cellules (n=11)");
            Debug.Log("2252 cellules (n=15)");
            Debug.Log("4002 cellules (n=20)");
            Debug.Log("6002 cellules (n=25)");
            Debug.Log("9002 cellules (n=30)");
            Debug.Log("16002 cellules (n=40)");
            Debug.Log("25002 cellules (n=50)");
            Debug.Log("40002 cellules (n=63)");
            Debug.Log("Custom: Fréquence personnalisée");
            Debug.Log("================================");
        }
    }
}
