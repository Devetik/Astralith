using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Générateur de planète utilisant la grille géodésique dual
    /// Remplace l'ancien système de génération
    /// </summary>
    public class GeodesicDualPlanetGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        public int seed = 42;
        public float planetRadius = 5f;
        public int subdivisionLevel = 4;
        
        [Header("Génération de Terrain")]
        public float noiseScale = 0.1f;
        public float landRatio = 0.3f; // 30% de terre, 70% d'eau
        
        [Header("Composants")]
        public GeodesicDualGrid geodesicGrid;
        public Material landMaterial;
        public Material waterMaterial;
        
        [Header("Debug")]
        public bool showDebugInfo = true;
        public bool generateOnStart = true;
        
        private GameObject currentPlanetGO;
        
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
                Debug.Log("=== GÉNÉRATION PLANÈTE GÉODÉSIQUE DUAL ===");
            }
            
            // Initialise le générateur de bruit
            Random.InitState(seed);
            
            // Crée ou trouve la grille géodésique
            if (geodesicGrid == null)
            {
                geodesicGrid = GetComponent<GeodesicDualGrid>();
                if (geodesicGrid == null)
                {
                    geodesicGrid = gameObject.AddComponent<GeodesicDualGrid>();
                }
            }
            
            // Configure la grille
            geodesicGrid.subdivisionLevel = subdivisionLevel;
            geodesicGrid.planetRadius = planetRadius;
            geodesicGrid.showDebugInfo = showDebugInfo;
            
            // Génère la grille
            geodesicGrid.GenerateDualGrid();
            
            // Génère le terrain simple
            GenerateSimpleTerrain();
            
            // Crée les meshes
            CreatePlanetMeshes();
            
            if (showDebugInfo)
            {
                Debug.Log("=== FIN GÉNÉRATION ===");
            }
        }
        
        /// <summary>
        /// Génère un terrain simple (Eau ou Terre uniquement)
        /// </summary>
        private void GenerateSimpleTerrain()
        {
            if (showDebugInfo)
            {
                Debug.Log("Génération du terrain simple (Eau/Terre uniquement)...");
            }
            
            foreach (GeodesicDualCell cell in geodesicGrid.cells)
            {
                // Génère un bruit simple pour déterminer terre/eau
                float noiseValue = GenerateNoise(cell.centerPosition);
                
                // Détermine si c'est terre ou eau basé sur le ratio
                if (noiseValue > landRatio)
                {
                    cell.cellType = GeodesicDualCell.CellType.Clearing; // Terre
                    cell.altitude = 0f; // Pas de relief
                    cell.isBuildable = true;
                }
                else
                {
                    cell.cellType = GeodesicDualCell.CellType.Ocean; // Eau
                    cell.altitude = 0f; // Pas de relief
                    cell.isBuildable = false;
                }
            }
            
            if (showDebugInfo)
            {
                int waterCells = geodesicGrid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Ocean);
                int landCells = geodesicGrid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Clearing);
                int buildableCells = geodesicGrid.cells.Count(c => c.isBuildable);
                
                Debug.Log($"Terrain généré:");
                Debug.Log($"- Cellules d'eau: {waterCells}");
                Debug.Log($"- Cellules de terre: {landCells}");
                Debug.Log($"- Cellules constructibles: {buildableCells}");
                Debug.Log($"- Ratio terre: {(float)landCells / geodesicGrid.cells.Count * 100f:F1}%");
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
            currentPlanetGO = new GameObject("GeodesicDualPlanet");
            currentPlanetGO.transform.SetParent(transform, false);
            
            // Sépare les cellules en terre et eau
            List<GeodesicDualCell> landCells = geodesicGrid.cells.Where(c => c.IsLand()).ToList();
            List<GeodesicDualCell> waterCells = geodesicGrid.cells.Where(c => c.IsWater()).ToList();
            
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
        private void CreateLandMesh(List<GeodesicDualCell> landCells)
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
        private void CreateWaterMesh(List<GeodesicDualCell> waterCells)
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
        /// Génère un mesh à partir d'une liste de cellules (sphère simple sans relief)
        /// </summary>
        private Mesh GenerateMeshFromCells(List<GeodesicDualCell> cells, bool isLand)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            
            int vertexIndex = 0;
            
            foreach (GeodesicDualCell cell in cells)
            {
                // Ajoute les sommets de la cellule (sphère simple sans relief)
                foreach (Vector3 vertex in cell.vertices)
                {
                    Vector3 worldVertex = vertex.normalized * planetRadius; // Sphère simple
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
        /// Obtient les statistiques de la planète
        /// </summary>
        public string GetPlanetStats()
        {
            if (geodesicGrid == null) return "Grille non initialisée";
            
            int totalCells = geodesicGrid.cells.Count;
            int oceanCells = geodesicGrid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Ocean);
            int clearingCells = geodesicGrid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Clearing);
            int mountainCells = geodesicGrid.cells.Count(c => c.cellType == GeodesicDualCell.CellType.Mountain);
            int buildableCells = geodesicGrid.cells.Count(c => c.isBuildable);
            
            return $"Planète: {totalCells} cellules | Océans: {oceanCells} | Clairières: {clearingCells} | Montagnes: {mountainCells} | Constructibles: {buildableCells}";
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
