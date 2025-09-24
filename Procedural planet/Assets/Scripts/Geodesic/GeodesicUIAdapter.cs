using UnityEngine;

namespace Geodesic
{
    /// <summary>
    /// Adaptateur pour intégrer le nouveau système géodésique avec l'UI existant
    /// </summary>
    public class GeodesicUIAdapter : MonoBehaviour
    {
        [Header("Configuration")]
        public bool useNewSystem = true;
        public bool showDebugInfo = true;
        
        [Header("Composants")]
        public GeodesicDualSetup geodesicSetup;
        public GeodesicDualPlanetGenerator geodesicGenerator;
        
        [Header("UI Compatibility")]
        public bool generateOnStart = true;
        
        private void Start()
        {
            if (generateOnStart)
            {
                InitializeNewSystem();
            }
        }
        
        /// <summary>
        /// Initialise le nouveau système
        /// </summary>
        public void InitializeNewSystem()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== INITIALISATION NOUVEAU SYSTÈME GÉODÉSIQUE ===");
            }
            
            // Trouve ou crée le setup
            if (geodesicSetup == null)
            {
                geodesicSetup = FindObjectOfType<GeodesicDualSetup>();
                if (geodesicSetup == null)
                {
                    geodesicSetup = gameObject.AddComponent<GeodesicDualSetup>();
                }
            }
            
            // Trouve le générateur
            if (geodesicGenerator == null)
            {
                geodesicGenerator = FindObjectOfType<GeodesicDualPlanetGenerator>();
            }
            
            // Configure le système
            if (geodesicSetup != null)
            {
                geodesicSetup.SetupNewGeodesicSystem();
            }
            
            if (showDebugInfo)
            {
                Debug.Log("=== INITIALISATION TERMINÉE ===");
            }
        }
        
        /// <summary>
        /// Génère une nouvelle planète (compatible avec l'ancien UI)
        /// </summary>
        public void GeneratePlanet()
        {
            if (useNewSystem && geodesicSetup != null)
            {
                geodesicSetup.GeneratePlanet();
            }
            else
            {
                Debug.LogWarning("Nouveau système géodésique non initialisé !");
            }
        }
        
        /// <summary>
        /// Génère une nouvelle seed (compatible avec l'ancien UI)
        /// </summary>
        public void GenerateNewSeed()
        {
            if (useNewSystem && geodesicSetup != null)
            {
                geodesicSetup.GenerateNewSeed();
            }
            else
            {
                Debug.LogWarning("Nouveau système géodésique non initialisé !");
            }
        }
        
        /// <summary>
        /// Obtient les statistiques (compatible avec l'ancien UI)
        /// </summary>
        public string GetPlanetStats()
        {
            if (useNewSystem && geodesicSetup != null)
            {
                return geodesicSetup.GetPlanetStats();
            }
            return "Système non initialisé";
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
        /// Test de nouvelle seed
        /// </summary>
        [ContextMenu("Test Nouvelle Seed")]
        public void TestNewSeed()
        {
            GenerateNewSeed();
        }
        
        /// <summary>
        /// Affiche les statistiques
        /// </summary>
        [ContextMenu("Afficher Statistiques")]
        public void ShowStats()
        {
            Debug.Log(GetPlanetStats());
        }
    }
}
