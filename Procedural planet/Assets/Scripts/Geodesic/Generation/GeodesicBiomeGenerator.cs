using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Générateur de biomes procédural pour les cellules géodésiques
    /// Inspiré des techniques de Sebastian Lague
    /// </summary>
    public class GeodesicBiomeGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        public int seed = 42;
        public bool showDebugInfo = true;
        
        [Header("Paramètres de Génération")]
        [Range(0f, 1f)]
        public float landRatio = 0.3f; // Ratio terre/océan
        
        [Header("Altitude")]
        public float altitudeScale = 0.1f;
        public float altitudeOctaves = 4;
        public float altitudeLacunarity = 2f;
        public float altitudePersistence = 0.5f;
        
        [Header("Température")]
        public float temperatureScale = 0.05f;
        public float temperatureOctaves = 3;
        public float latitudeInfluence = 0.7f; // Influence de la latitude sur la température
        
        [Header("Humidité")]
        public float humidityScale = 0.08f;
        public float humidityOctaves = 3;
        public float humidityLacunarity = 2.1f;
        public float humidityPersistence = 0.6f;
        
        [Header("Références")]
        public GeodesicSphereGrid geodesicGrid;
        
        private void Start()
        {
            if (geodesicGrid == null)
            {
                geodesicGrid = FindObjectOfType<GeodesicSphereGrid>();
            }
        }
        
        /// <summary>
        /// Génère les biomes pour toutes les cellules
        /// </summary>
        public void GenerateBiomes()
        {
            if (geodesicGrid == null || geodesicGrid.cells == null)
            {
                Debug.LogError("GeodesicSphereGrid non trouvé !");
                return;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"=== GÉNÉRATION DE BIOMES ===");
                Debug.Log($"Seed: {seed}");
                Debug.Log($"Cellules: {geodesicGrid.cells.Count}");
            }
            
            // Initialise le générateur de nombres aléatoires
            UnityEngine.Random.InitState(seed);
            
            // Génère les propriétés de base
            GenerateBaseProperties();
            
            // Détermine les biomes
            DetermineBiomes();
            
            if (showDebugInfo)
            {
                LogBiomeStatistics();
            }
        }
        
        /// <summary>
        /// Génère les propriétés de base (altitude, température, humidité)
        /// </summary>
        private void GenerateBaseProperties()
        {
            foreach (GeodesicSphereCell cell in geodesicGrid.cells)
            {
                Vector3 position = cell.centerPosition;
                
                // Génère l'altitude avec du bruit fractal
                cell.altitude = GenerateFractalNoise(position, altitudeScale, (int)altitudeOctaves, altitudeLacunarity, altitudePersistence);
                cell.altitude = Mathf.Clamp01(cell.altitude);
                
                // Génère la température (influence de la latitude)
                float latitude = Mathf.Abs(position.y); // Latitude basée sur la position Y
                float temperatureNoise = GenerateFractalNoise(position, temperatureScale, (int)temperatureOctaves, 2f, 0.5f);
                cell.temperature = Mathf.Lerp(-1f, 1f, latitude) * latitudeInfluence + temperatureNoise * (1f - latitudeInfluence);
                cell.temperature = Mathf.Clamp(cell.temperature, -1f, 1f);
                
                // Génère l'humidité
                cell.humidity = GenerateFractalNoise(position, humidityScale, (int)humidityOctaves, humidityLacunarity, humidityPersistence);
                cell.humidity = Mathf.Clamp01(cell.humidity);
                
                // Calcule la fertilité
                cell.fertility = CalculateFertility(cell.altitude, cell.temperature, cell.humidity);
            }
        }
        
        /// <summary>
        /// Détermine le biome de chaque cellule
        /// </summary>
        private void DetermineBiomes()
        {
            foreach (GeodesicSphereCell cell in geodesicGrid.cells)
            {
                // Détermine d'abord si c'est de l'eau ou de la terre
                if (cell.altitude < landRatio)
                {
                    // C'est de l'eau
                    if (cell.altitude < landRatio * 0.3f)
                    {
                        cell.cellType = GeodesicSphereCell.CellType.Ocean;
                    }
                    else
                    {
                        cell.cellType = GeodesicSphereCell.CellType.Lake;
                    }
                    cell.isBuildable = false;
                }
                else
                {
                    // C'est de la terre - détermine le biome
                    cell.cellType = DetermineLandBiome(cell.altitude, cell.temperature, cell.humidity);
                    cell.isBuildable = CanBuildOnBiome(cell.cellType);
                }
            }
        }
        
        /// <summary>
        /// Détermine le biome terrestre basé sur les propriétés
        /// </summary>
        private GeodesicSphereCell.CellType DetermineLandBiome(float altitude, float temperature, float humidity)
        {
            // Zones polaires (très froid)
            if (temperature < -0.5f)
            {
                if (altitude > 0.8f)
                {
                    return GeodesicSphereCell.CellType.Ice;
                }
                else
                {
                    return GeodesicSphereCell.CellType.Tundra;
                }
            }
            
            // Zones de haute altitude
            if (altitude > 0.8f)
            {
                return GeodesicSphereCell.CellType.Mountain;
            }
            
            // Zones côtières
            if (altitude < landRatio + 0.1f)
            {
                return GeodesicSphereCell.CellType.Coast;
            }
            
            // Zones tempérées et tropicales
            if (temperature > 0.2f)
            {
                if (humidity > 0.6f)
                {
                    return GeodesicSphereCell.CellType.Forest;
                }
                else if (humidity > 0.3f)
                {
                    return GeodesicSphereCell.CellType.Grassland;
                }
                else
                {
                    return GeodesicSphereCell.CellType.Desert;
                }
            }
            else
            {
                // Zones tempérées froides
                if (humidity > 0.5f)
                {
                    return GeodesicSphereCell.CellType.Forest;
                }
                else
                {
                    return GeodesicSphereCell.CellType.Grassland;
                }
            }
        }
        
        /// <summary>
        /// Calcule la fertilité d'une cellule
        /// </summary>
        private float CalculateFertility(float altitude, float temperature, float humidity)
        {
            // La fertilité est maximale avec une température modérée et une humidité élevée
            float temperatureFactor = 1f - Mathf.Abs(temperature);
            float humidityFactor = humidity;
            float altitudeFactor = 1f - Mathf.Abs(altitude - 0.5f) * 2f; // Meilleure à mi-altitude
            
            return (temperatureFactor + humidityFactor + altitudeFactor) / 3f;
        }
        
        /// <summary>
        /// Vérifie si on peut construire sur un biome
        /// </summary>
        private bool CanBuildOnBiome(GeodesicSphereCell.CellType biome)
        {
            switch (biome)
            {
                case GeodesicSphereCell.CellType.Grassland:
                case GeodesicSphereCell.CellType.Coast:
                    return true;
                case GeodesicSphereCell.CellType.Forest:
                case GeodesicSphereCell.CellType.Desert:
                    return true; // Construire possible mais plus difficile
                case GeodesicSphereCell.CellType.Mountain:
                case GeodesicSphereCell.CellType.Tundra:
                case GeodesicSphereCell.CellType.Ice:
                    return false; // Construction difficile
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Génère du bruit fractal (inspiré de Sebastian Lague)
        /// </summary>
        private float GenerateFractalNoise(Vector3 position, float scale, int octaves, float lacunarity, float persistence)
        {
            float value = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float maxValue = 0f;
            
            for (int i = 0; i < octaves; i++)
            {
                float x = (position.x + seed) * scale * frequency;
                float y = (position.y + seed) * scale * frequency;
                float z = (position.z + seed) * scale * frequency;
                
                float noise = Mathf.PerlinNoise(x, y);
                noise += Mathf.PerlinNoise(y, z);
                noise += Mathf.PerlinNoise(z, x);
                noise /= 3f;
                
                value += noise * amplitude;
                maxValue += amplitude;
                
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            
            return value / maxValue;
        }
        
        /// <summary>
        /// Affiche les statistiques des biomes
        /// </summary>
        private void LogBiomeStatistics()
        {
            var biomeCounts = geodesicGrid.cells.GroupBy(c => c.cellType)
                .ToDictionary(g => g.Key, g => g.Count());
            
            Debug.Log("=== STATISTIQUES DES BIOMES ===");
            foreach (var kvp in biomeCounts)
            {
                float percentage = (float)kvp.Value / geodesicGrid.cells.Count * 100f;
                Debug.Log($"{kvp.Key}: {kvp.Value} cellules ({percentage:F1}%)");
            }
            
            // Statistiques des propriétés
            float avgAltitude = geodesicGrid.cells.Average(c => c.altitude);
            float avgTemperature = geodesicGrid.cells.Average(c => c.temperature);
            float avgHumidity = geodesicGrid.cells.Average(c => c.humidity);
            float avgFertility = geodesicGrid.cells.Average(c => c.fertility);
            
            Debug.Log($"Altitude moyenne: {avgAltitude:F3}");
            Debug.Log($"Température moyenne: {avgTemperature:F3}");
            Debug.Log($"Humidité moyenne: {avgHumidity:F3}");
            Debug.Log($"Fertilité moyenne: {avgFertility:F3}");
            Debug.Log("=== FIN STATISTIQUES ===");
        }
        
        /// <summary>
        /// Génère une nouvelle seed
        /// </summary>
        [ContextMenu("Nouvelle Seed")]
        public void GenerateNewSeed()
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            if (showDebugInfo)
            {
                Debug.Log($"Nouvelle seed: {seed}");
            }
        }
        
        /// <summary>
        /// Régénère les biomes
        /// </summary>
        [ContextMenu("Régénérer Biomes")]
        public void RegenerateBiomes()
        {
            GenerateBiomes();
        }
    }
}
