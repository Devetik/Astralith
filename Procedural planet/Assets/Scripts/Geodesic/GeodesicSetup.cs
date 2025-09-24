using UnityEngine;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Script de setup pour le système géodésique
    /// </summary>
    public class GeodesicSetup : MonoBehaviour
    {
        [Header("Configuration")]
        public bool setupOnStart = true;
        public bool showDebugInfo = true;
        
        [Header("Composants")]
        public GeodesicPlanetGenerator planetGenerator;
        public GeodesicCharacterPlacer characterPlacer;
        public Transform characterPrefab;
        
        [Header("Matériaux")]
        public Material landMaterial;
        public Material waterMaterial;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupGeodesicSystem();
            }
        }
        
        /// <summary>
        /// Configure le système géodésique complet
        /// </summary>
        [ContextMenu("Setup Système Géodésique")]
        public void SetupGeodesicSystem()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== SETUP SYSTÈME GÉODÉSIQUE ===");
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
            // Trouve le générateur de planète
            if (planetGenerator == null)
            {
                planetGenerator = FindObjectOfType<GeodesicPlanetGenerator>();
                if (planetGenerator == null)
                {
                    planetGenerator = gameObject.AddComponent<GeodesicPlanetGenerator>();
                    if (showDebugInfo)
                    {
                        Debug.Log("Générateur de planète géodésique créé");
                    }
                }
            }
            
            // Trouve le placer de personnage
            if (characterPlacer == null)
            {
                characterPlacer = FindObjectOfType<GeodesicCharacterPlacer>();
                if (characterPlacer == null)
                {
                    characterPlacer = gameObject.AddComponent<GeodesicCharacterPlacer>();
                    if (showDebugInfo)
                    {
                        Debug.Log("Placer de personnage géodésique créé");
                    }
                }
            }
            
            // Trouve le prefab de personnage
            if (characterPrefab == null)
            {
                // Cherche un prefab de personnage dans la scène
                Transform existingCharacter = FindObjectOfType<Transform>();
                if (existingCharacter != null && existingCharacter.name.ToLower().Contains("character"))
                {
                    characterPrefab = existingCharacter;
                    if (showDebugInfo)
                    {
                        Debug.Log("Prefab de personnage trouvé dans la scène");
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
            if (planetGenerator != null)
            {
                planetGenerator.showDebugInfo = showDebugInfo;
                planetGenerator.landMaterial = landMaterial;
                planetGenerator.waterMaterial = waterMaterial;
                
                if (showDebugInfo)
                {
                    Debug.Log("Générateur de planète configuré");
                }
            }
            
            // Configure le placer de personnage
            if (characterPlacer != null)
            {
                characterPlacer.characterPrefab = characterPrefab;
                characterPlacer.planetGenerator = planetGenerator;
                characterPlacer.showDebugInfo = showDebugInfo;
                
                if (showDebugInfo)
                {
                    Debug.Log("Placer de personnage configuré");
                }
            }
        }
        
        /// <summary>
        /// Génère la planète
        /// </summary>
        private void GeneratePlanet()
        {
            if (planetGenerator != null)
            {
                planetGenerator.GeneratePlanet();
                
                if (showDebugInfo)
                {
                    Debug.Log("Planète générée");
                }
            }
        }
        
        /// <summary>
        /// Place le personnage
        /// </summary>
        [ContextMenu("Placer Personnage")]
        public void PlaceCharacter()
        {
            if (characterPlacer != null)
            {
                characterPlacer.PlaceCharacterOnPlanet();
            }
        }
        
        /// <summary>
        /// Génère une nouvelle seed
        /// </summary>
        [ContextMenu("Nouvelle Seed")]
        public void GenerateNewSeed()
        {
            if (planetGenerator != null)
            {
                planetGenerator.GenerateNewSeed();
                planetGenerator.GeneratePlanet();
                
                if (showDebugInfo)
                {
                    Debug.Log($"Nouvelle seed: {planetGenerator.seed}");
                }
            }
        }
        
        /// <summary>
        /// Test complet du système
        /// </summary>
        [ContextMenu("Test Complet")]
        public void TestCompleteSystem()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== TEST COMPLET SYSTÈME GÉODÉSIQUE ===");
            }
            
            SetupGeodesicSystem();
            PlaceCharacter();
            
            if (showDebugInfo)
            {
                Debug.Log("=== TEST TERMINÉ ===");
            }
        }
        
        /// <summary>
        /// Affiche les informations de debug
        /// </summary>
        [ContextMenu("Afficher Infos Debug")]
        public void ShowDebugInfo()
        {
            if (planetGenerator != null && planetGenerator.geodesicGrid != null)
            {
                var grid = planetGenerator.geodesicGrid;
                Debug.Log($"=== INFORMATIONS GRILLE GÉODÉSIQUE ===");
                Debug.Log($"Cellules totales: {grid.cells.Count}");
                Debug.Log($"Cellules de terre: {grid.cells.Count(c => c.IsLand())}");
                Debug.Log($"Cellules d'eau: {grid.cells.Count(c => c.IsWater())}");
                Debug.Log($"Continents: {grid.continents.Count}");
                
                var mainContinent = grid.continents.FirstOrDefault(c => c.isMainContinent);
                if (mainContinent != null)
                {
                    Debug.Log($"Continent principal: {mainContinent.cellIds.Count} cellules");
                }
                
                Debug.Log("=== FIN INFORMATIONS ===");
            }
        }
    }
}