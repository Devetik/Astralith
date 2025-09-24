using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Générateur de planète utilisant une grille géodésique
    /// </summary>
    public class GeodesicPlanetGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        public int seed = 42;
        public float planetRadius = 5f;
        public int subdivisionLevel = 3;
        
        [Header("Génération de Terrain")]
        public float noiseScale = 0.1f;
        public float oceanLevel = 0.3f;
        public float mountainThreshold = 0.7f;
        public float desertThreshold = 0.8f;
        
        [Header("Composants")]
        public GeodesicGrid geodesicGrid;
        public Material landMaterial;
        public Material waterMaterial;
        
        [Header("Debug")]
        public bool showDebugInfo = true;
        public bool generateOnStart = true;
        
        private GameObject currentPlanetGO;
        private List<Vector3> landVertices = new List<Vector3>();
        private List<Vector3> waterVertices = new List<Vector3>();
        private List<int> landTriangles = new List<int>();
        private List<int> waterTriangles = new List<int>();
        
        private void Start()
        {
            if (generateOnStart)
            {
                GeneratePlanet();
            }
        }
        
        /// <summary>
        /// Génère la planète complète
        /// </summary>
        public void GeneratePlanet()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== GÉNÉRATION PLANÈTE GÉODÉSIQUE ===");
            }
            
            // Initialise le générateur de bruit
            Random.InitState(seed);
            
            // Crée ou trouve la grille géodésique
            if (geodesicGrid == null)
            {
                geodesicGrid = GetComponent<GeodesicGrid>();
                if (geodesicGrid == null)
                {
                    geodesicGrid = gameObject.AddComponent<GeodesicGrid>();
                }
            }
            
            // Configure la grille
            geodesicGrid.subdivisionLevel = subdivisionLevel;
            geodesicGrid.planetRadius = planetRadius;
            geodesicGrid.showDebugInfo = showDebugInfo;
            
            // Génère la grille
            geodesicGrid.GenerateGrid();
            
            // Génère le terrain
            GenerateTerrain();
            
            // Analyse les continents
            geodesicGrid.AnalyzeContinents();
            
            // Crée les meshes
            CreatePlanetMeshes();
            
            if (showDebugInfo)
            {
                Debug.Log("=== FIN GÉNÉRATION ===");
            }
        }
        
        /// <summary>
        /// Génère le terrain pour chaque cellule
        /// </summary>
        private void GenerateTerrain()
        {
            if (showDebugInfo)
            {
                Debug.Log("Génération du terrain...");
            }
            
            foreach (GeodesicCell cell in geodesicGrid.cells)
            {
                // Génère l'altitude basée sur le bruit
                float noiseValue = GenerateNoise(cell.centerPosition);
                cell.altitude = (noiseValue - 0.5f) * 2000f; // Altitude de -1000m à +1000m
                
                // Génère la température
                cell.temperature = GenerateTemperature(cell.centerPosition);
                
                // Génère l'humidité
                cell.humidity = GenerateHumidity(cell.centerPosition);
                
                // Détermine le type de cellule
                cell.DetermineCellType();
                
                // Ajuste le type basé sur la température et l'humidité
                AdjustCellType(cell);
            }
            
            if (showDebugInfo)
            {
                int landCells = geodesicGrid.cells.Count(c => c.IsLand());
                int waterCells = geodesicGrid.cells.Count(c => c.IsWater());
                Debug.Log($"Terrain généré: {landCells} cellules de terre, {waterCells} cellules d'eau");
            }
        }
        
        /// <summary>
        /// Génère une valeur de bruit pour une position
        /// </summary>
        private float GenerateNoise(Vector3 position)
        {
            float x = position.x * noiseScale;
            float y = position.y * noiseScale;
            float z = position.z * noiseScale;
            
            // Utilise plusieurs octaves de bruit
            float noise1 = Mathf.PerlinNoise(x, y) * 0.5f;
            float noise2 = Mathf.PerlinNoise(x * 2, y * 2) * 0.25f;
            float noise3 = Mathf.PerlinNoise(x * 4, y * 4) * 0.125f;
            
            return noise1 + noise2 + noise3;
        }
        
        /// <summary>
        /// Génère la température basée sur la latitude
        /// </summary>
        private float GenerateTemperature(Vector3 position)
        {
            float latitude = Mathf.Abs(position.y) / planetRadius;
            float baseTemp = 30f - (latitude * 60f); // 30°C à l'équateur, -30°C aux pôles
            
            // Ajoute de la variation
            float noise = GenerateNoise(position) * 10f;
            
            return baseTemp + noise;
        }
        
        /// <summary>
        /// Génère l'humidité basée sur la position
        /// </summary>
        private float GenerateHumidity(Vector3 position)
        {
            float noise = GenerateNoise(position);
            return Mathf.Clamp01(noise);
        }
        
        /// <summary>
        /// Ajuste le type de cellule basé sur la température et l'humidité
        /// </summary>
        private void AdjustCellType(GeodesicCell cell)
        {
            if (cell.altitude < 0f) return; // Pas d'ajustement pour les cellules sous l'eau
            
            if (cell.temperature < -10f)
            {
                cell.cellType = GeodesicCell.CellType.Tundra;
            }
            else if (cell.temperature > 30f && cell.humidity < 0.3f)
            {
                cell.cellType = GeodesicCell.CellType.Desert;
            }
            else if (cell.humidity > 0.7f)
            {
                cell.cellType = GeodesicCell.CellType.Forest;
            }
            else if (cell.altitude > 500f)
            {
                cell.cellType = GeodesicCell.CellType.Mountain;
            }
            else
            {
                cell.cellType = GeodesicCell.CellType.Land;
            }
        }
        
        /// <summary>
        /// Crée les meshes de la planète
        /// </summary>
        private void CreatePlanetMeshes()
        {
            if (showDebugInfo)
            {
                Debug.Log("Création des meshes...");
            }
            
            // Supprime l'ancienne planète
            if (currentPlanetGO != null)
            {
                DestroyImmediate(currentPlanetGO);
            }
            
            // Crée le GameObject de la planète
            currentPlanetGO = new GameObject("GeodesicPlanet");
            currentPlanetGO.transform.SetParent(transform, false);
            
            // Sépare les cellules en terre et eau
            List<GeodesicCell> landCells = geodesicGrid.cells.Where(c => c.IsLand()).ToList();
            List<GeodesicCell> waterCells = geodesicGrid.cells.Where(c => c.IsWater()).ToList();
            
            // Crée le mesh de la terre
            if (landCells.Count > 0)
            {
                CreateLandMesh(landCells);
            }
            
            // Crée le mesh de l'eau
            if (waterCells.Count > 0)
            {
                CreateWaterMesh(waterCells);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Meshes créés: {landCells.Count} cellules de terre, {waterCells.Count} cellules d'eau");
            }
        }
        
        /// <summary>
        /// Crée le mesh de la terre
        /// </summary>
        private void CreateLandMesh(List<GeodesicCell> landCells)
        {
            GameObject landGO = new GameObject("LandMesh");
            landGO.transform.SetParent(currentPlanetGO.transform, false);
            landGO.tag = "Land";
            
            MeshFilter meshFilter = landGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = landGO.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = landGO.AddComponent<MeshCollider>();
            
            // Génère le mesh
            Mesh landMesh = GenerateMeshFromCells(landCells, true);
            meshFilter.mesh = landMesh;
            meshCollider.sharedMesh = landMesh;
            
            // Assigne le matériau
            if (landMaterial != null)
            {
                meshRenderer.material = landMaterial;
            }
            else
            {
                meshRenderer.material = CreateDefaultLandMaterial();
            }
        }
        
        /// <summary>
        /// Crée le mesh de l'eau
        /// </summary>
        private void CreateWaterMesh(List<GeodesicCell> waterCells)
        {
            GameObject waterGO = new GameObject("WaterMesh");
            waterGO.transform.SetParent(currentPlanetGO.transform, false);
            waterGO.tag = "Water";
            
            MeshFilter meshFilter = waterGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = waterGO.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = waterGO.AddComponent<MeshCollider>();
            
            // Génère le mesh
            Mesh waterMesh = GenerateMeshFromCells(waterCells, false);
            meshFilter.mesh = waterMesh;
            meshCollider.sharedMesh = waterMesh;
            
            // Assigne le matériau
            if (waterMaterial != null)
            {
                meshRenderer.material = waterMaterial;
            }
            else
            {
                meshRenderer.material = CreateDefaultWaterMaterial();
            }
        }
        
        /// <summary>
        /// Génère un mesh à partir d'une liste de cellules
        /// </summary>
        private Mesh GenerateMeshFromCells(List<GeodesicCell> cells, bool isLand)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            
            int vertexIndex = 0;
            
            foreach (GeodesicCell cell in cells)
            {
                // Ajoute les sommets de la cellule
                foreach (Vector3 vertex in cell.vertices)
                {
                    Vector3 worldVertex = vertex.normalized * (planetRadius + (isLand ? cell.altitude : 0f));
                    vertices.Add(worldVertex);
                    normals.Add(worldVertex.normalized);
                }
                
                // Ajoute les triangles
                for (int i = 0; i < cell.vertices.Length - 2; i++)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + i + 1);
                    triangles.Add(vertexIndex + i + 2);
                }
                
                vertexIndex += cell.vertices.Length;
            }
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        /// <summary>
        /// Crée un matériau par défaut pour la terre
        /// </summary>
        private Material CreateDefaultLandMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            return mat;
        }
        
        /// <summary>
        /// Crée un matériau par défaut pour l'eau
        /// </summary>
        private Material CreateDefaultWaterMaterial()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.blue;
            return mat;
        }
        
        /// <summary>
        /// Obtient une position de spawn sur le continent principal
        /// </summary>
        public Vector3 GetSpawnPosition()
        {
            if (geodesicGrid == null)
            {
                Debug.LogWarning("Grille géodésique non initialisée !");
                return Vector3.zero;
            }
            
            return geodesicGrid.GetSpawnPositionOnMainContinent();
        }
        
        /// <summary>
        /// Génère une nouvelle seed
        /// </summary>
        public void GenerateNewSeed()
        {
            seed = Random.Range(0, int.MaxValue);
            if (showDebugInfo)
            {
                Debug.Log($"Nouvelle seed générée: {seed}");
            }
        }
        
        /// <summary>
        /// Test de génération
        /// </summary>
        [ContextMenu("Test Génération")]
        public void TestGeneration()
        {
            GeneratePlanet();
        }
    }
}
