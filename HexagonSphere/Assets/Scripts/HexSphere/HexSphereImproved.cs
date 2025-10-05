using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HexSphere
{
    /// <summary>
    /// Version améliorée du générateur de sphère hexagonale avec imbrication parfaite
    /// </summary>
    public class HexSphereImproved : MonoBehaviour
    {
        [Header("Paramètres de la Sphère")]
        [Range(1, 5)]
        public int subdivisionLevel = 2;
        
        [Range(0.1f, 10f)]
        public float radius = 1f;
        
        [Range(0.1f, 2f)]
        public float hexSize = 0.3f;
        
        [Header("Options d'Imbrication")]
        public bool usePerfectTiling = true;
        public bool shareVertices = true;
        public bool useHexagonalGrid = true;
        
        [Header("Matériaux")]
        public Material hexMaterial;
        
        [Header("Options de Génération")]
        public bool generateOnStart = true;
        public bool showGizmos = true;
        public Color gizmoColor = Color.yellow;
        
        // Données générées
        [HideInInspector]
        public List<HexCell> hexCells = new List<HexCell>();
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        [HideInInspector]
        public Mesh hexMesh;
        
        // Pour l'imbrication parfaite
        private Dictionary<Vector3, int> sharedVertices = new Dictionary<Vector3, int>();
        private List<Vector3> globalVertices = new List<Vector3>();
        private List<Vector2> globalUVs = new List<Vector2>();
        private List<Color> globalColors = new List<Color>();
        
        private void Start()
        {
            if (generateOnStart)
            {
                GenerateImprovedHexSphere();
            }
        }
        
        /// <summary>
        /// Génère la sphère hexagonale améliorée
        /// </summary>
        [ContextMenu("Générer Sphère Améliorée")]
        public void GenerateImprovedHexSphere()
        {
            Debug.Log($"Génération d'une sphère hexagonale améliorée - Niveau: {subdivisionLevel}, Rayon: {radius}");
            
            // Nettoyer les données existantes
            ClearHexSphere();
            
            if (usePerfectTiling)
            {
                GeneratePerfectHexTiling();
            }
            else
            {
                GenerateStandardHexSphere();
            }
            
            Debug.Log($"Sphère hexagonale générée avec {hexCells.Count} hexagones");
        }
        
        /// <summary>
        /// Génère un pavage hexagonal parfait
        /// </summary>
        private void GeneratePerfectHexTiling()
        {
            // Étape 1: Créer les centres avec subdivision géodésique
            List<Vector3> centers = GenerateGeodesicCenters();
            
            // Étape 2: Créer les cellules hexagonales
            CreateHexCells(centers);
            
            // Étape 3: Calculer les vertices partagés
            CalculateSharedVertices();
            
            // Étape 4: Générer le mesh optimisé
            GenerateOptimizedMesh();
            
            // Étape 5: Appliquer les matériaux
            ApplyMaterials();
        }
        
        /// <summary>
        /// Génère la sphère standard (méthode originale)
        /// </summary>
        private void GenerateStandardHexSphere()
        {
            // Utiliser la méthode originale
            GenerateHexCenters();
            CalculateHexGeometry();
            GenerateMesh();
            ApplyMaterials();
        }
        
        /// <summary>
        /// Génère les centres géodésiques
        /// </summary>
        private List<Vector3> GenerateGeodesicCenters()
        {
            List<Vector3> centers = new List<Vector3>();
            
            // Commencer avec un icosaèdre
            List<Vector3> icosahedronVertices = GenerateIcosahedronVertices();
            List<int> icosahedronTriangles = GenerateIcosahedronTriangles();
            
            // Subdiviser les faces triangulaires
            List<Vector3> subdividedVertices = new List<Vector3>();
            List<int> subdividedTriangles = new List<int>();
            
            for (int i = 0; i < icosahedronTriangles.Count; i += 3)
            {
                Vector3 v1 = icosahedronVertices[icosahedronTriangles[i]];
                Vector3 v2 = icosahedronVertices[icosahedronTriangles[i + 1]];
                Vector3 v3 = icosahedronVertices[icosahedronTriangles[i + 2]];
                
                SubdivideTriangle(v1, v2, v3, subdivisionLevel, subdividedVertices, subdividedTriangles);
            }
            
            // Normaliser les vertices pour qu'ils soient sur la sphère
            for (int i = 0; i < subdividedVertices.Count; i++)
            {
                subdividedVertices[i] = subdividedVertices[i].normalized;
            }
            
            return subdividedVertices;
        }
        
        /// <summary>
        /// Crée les cellules hexagonales
        /// </summary>
        private void CreateHexCells(List<Vector3> centers)
        {
            hexCells.Clear();
            
            for (int i = 0; i < centers.Count; i++)
            {
                Vector3 center = centers[i];
                Vector3 normal = center.normalized;
                
                HexCell cell = new HexCell(center, normal, i, hexSize);
                hexCells.Add(cell);
            }
        }
        
        /// <summary>
        /// Calcule les vertices partagés pour l'imbrication parfaite
        /// </summary>
        private void CalculateSharedVertices()
        {
            sharedVertices.Clear();
            globalVertices.Clear();
            globalUVs.Clear();
            globalColors.Clear();
            
            foreach (HexCell cell in hexCells)
            {
                // Calculer les vertices de l'hexagone
                Vector3[] hexVertices = CalculateHexVertices(cell.center, cell.normal, hexSize);
                
                // Réinitialiser les indices des vertices
                cell.vertexIndices = new int[6];
                
                for (int i = 0; i < 6; i++)
                {
                    Vector3 vertex = hexVertices[i];
                    int vertexIndex = GetOrCreateSharedVertex(vertex);
                    cell.vertexIndices[i] = vertexIndex;
                }
            }
        }
        
        /// <summary>
        /// Calcule les vertices d'un hexagone
        /// </summary>
        private Vector3[] CalculateHexVertices(Vector3 center, Vector3 normal, float size)
        {
            Vector3[] vertices = new Vector3[6];
            
            // Créer un hexagone dans le plan tangent à la sphère
            Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
            if (right.magnitude < 0.1f)
            {
                right = Vector3.Cross(normal, Vector3.forward).normalized;
            }
            Vector3 forward = Vector3.Cross(right, normal).normalized;
            
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Vector3 localPos = new Vector3(
                    Mathf.Cos(angle) * size,
                    0f,
                    Mathf.Sin(angle) * size
                );
                
                Vector3 worldPos = center + right * localPos.x + forward * localPos.z;
                vertices[i] = worldPos.normalized * radius;
            }
            
            return vertices;
        }
        
        /// <summary>
        /// Obtient ou crée un vertex partagé
        /// </summary>
        private int GetOrCreateSharedVertex(Vector3 vertex)
        {
            // Chercher un vertex existant proche
            foreach (var kvp in sharedVertices)
            {
                if (Vector3.Distance(kvp.Key, vertex) < 0.001f)
                {
                    return kvp.Value;
                }
            }
            
            // Créer un nouveau vertex
            int index = globalVertices.Count;
            sharedVertices[vertex] = index;
            globalVertices.Add(vertex);
            globalUVs.Add(CalculateUV(vertex));
            globalColors.Add(Color.white);
            
            return index;
        }
        
        /// <summary>
        /// Calcule les UVs pour un vertex
        /// </summary>
        private Vector2 CalculateUV(Vector3 vertex)
        {
            // Projection sphérique
            float u = 0.5f + Mathf.Atan2(vertex.z, vertex.x) / (2f * Mathf.PI);
            float v = 0.5f - Mathf.Asin(vertex.y) / Mathf.PI;
            return new Vector2(u, v);
        }
        
        /// <summary>
        /// Génère le mesh optimisé
        /// </summary>
        private void GenerateOptimizedMesh()
        {
            if (hexMesh == null)
            {
                hexMesh = new Mesh();
                hexMesh.name = "HexSphereImproved";
            }
            
            // Créer les triangles
            List<int> triangles = new List<int>();
            
            foreach (HexCell cell in hexCells)
            {
                if (!cell.isVisible) continue;
                
                // Ajouter les triangles de l'hexagone
                for (int i = 0; i < 4; i++) // 4 triangles pour un hexagone
                {
                    triangles.Add(cell.vertexIndices[0]);
                    triangles.Add(cell.vertexIndices[i + 1]);
                    triangles.Add(cell.vertexIndices[i + 2]);
                }
            }
            
            // Assigner au mesh
            hexMesh.Clear();
            hexMesh.vertices = globalVertices.ToArray();
            hexMesh.triangles = triangles.ToArray();
            hexMesh.uv = globalUVs.ToArray();
            hexMesh.colors = globalColors.ToArray();
            hexMesh.RecalculateNormals();
            hexMesh.RecalculateBounds();
            
            // Assigner au MeshFilter
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }
            
            meshFilter.mesh = hexMesh;
        }
        
        /// <summary>
        /// Génère les centres des hexagones (méthode originale)
        /// </summary>
        private void GenerateHexCenters()
        {
            hexCells.Clear();
            
            // Commencer avec un icosaèdre
            List<Vector3> icosahedronVertices = GenerateIcosahedronVertices();
            List<int> icosahedronTriangles = GenerateIcosahedronTriangles();
            
            // Subdiviser les faces triangulaires
            List<Vector3> subdividedVertices = new List<Vector3>();
            List<int> subdividedTriangles = new List<int>();
            
            for (int i = 0; i < icosahedronTriangles.Count; i += 3)
            {
                Vector3 v1 = icosahedronVertices[icosahedronTriangles[i]];
                Vector3 v2 = icosahedronVertices[icosahedronTriangles[i + 1]];
                Vector3 v3 = icosahedronVertices[icosahedronTriangles[i + 2]];
                
                SubdivideTriangle(v1, v2, v3, subdivisionLevel, subdividedVertices, subdividedTriangles);
            }
            
            // Normaliser les vertices pour qu'ils soient sur la sphère
            for (int i = 0; i < subdividedVertices.Count; i++)
            {
                subdividedVertices[i] = subdividedVertices[i].normalized;
            }
            
            // Créer les cellules hexagonales
            for (int i = 0; i < subdividedVertices.Count; i++)
            {
                Vector3 center = subdividedVertices[i];
                Vector3 normal = center.normalized;
                
                HexCell cell = new HexCell(center, normal, i, hexSize);
                hexCells.Add(cell);
            }
        }
        
        /// <summary>
        /// Calcule la géométrie de chaque hexagone (méthode originale)
        /// </summary>
        private void CalculateHexGeometry()
        {
            foreach (HexCell cell in hexCells)
            {
                cell.CalculateVertices(radius, hexSize);
            }
        }
        
        /// <summary>
        /// Génère le mesh (méthode originale)
        /// </summary>
        public void GenerateMesh()
        {
            if (hexMesh == null)
            {
                hexMesh = new Mesh();
                hexMesh.name = "HexSphere";
            }
            
            List<Vector3> allVertices = new List<Vector3>();
            List<int> allTriangles = new List<int>();
            List<Vector2> allUVs = new List<Vector2>();
            List<Color> allColors = new List<Color>();
            
            int vertexOffset = 0;
            
            foreach (HexCell cell in hexCells)
            {
                if (!cell.isVisible) continue;
                
                // Ajouter les vertices
                allVertices.AddRange(cell.vertices);
                
                // Ajouter les triangles avec offset
                for (int i = 0; i < cell.triangles.Length; i++)
                {
                    allTriangles.Add(cell.triangles[i] + vertexOffset);
                }
                
                // Ajouter les UVs
                allUVs.AddRange(cell.uvs);
                
                // Ajouter les couleurs
                for (int i = 0; i < cell.vertices.Length; i++)
                {
                    allColors.Add(cell.color);
                }
                
                vertexOffset += cell.vertices.Length;
            }
            
            hexMesh.Clear();
            hexMesh.vertices = allVertices.ToArray();
            hexMesh.triangles = allTriangles.ToArray();
            hexMesh.uv = allUVs.ToArray();
            hexMesh.colors = allColors.ToArray();
            hexMesh.RecalculateNormals();
            hexMesh.RecalculateBounds();
            
            // Assigner le mesh au MeshFilter
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }
            
            meshFilter.mesh = hexMesh;
        }
        
        /// <summary>
        /// Applique les matériaux
        /// </summary>
        private void ApplyMaterials()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }
            
            if (hexMaterial != null)
            {
                meshRenderer.material = hexMaterial;
            }
        }
        
        /// <summary>
        /// Nettoie la sphère hexagonale
        /// </summary>
        [ContextMenu("Nettoyer Sphère")]
        public void ClearHexSphere()
        {
            hexCells.Clear();
            sharedVertices.Clear();
            globalVertices.Clear();
            globalUVs.Clear();
            globalColors.Clear();
            
            if (hexMesh != null)
            {
                DestroyImmediate(hexMesh);
                hexMesh = null;
            }
            
            if (meshFilter != null)
            {
                meshFilter.mesh = null;
            }
        }
        
        // Méthodes utilitaires (copiées du générateur original)
        private List<Vector3> GenerateIcosahedronVertices()
        {
            List<Vector3> vertices = new List<Vector3>();
            
            float t = (1f + Mathf.Sqrt(5f)) / 2f; // Nombre d'or
            
            // 12 vertices de l'icosaèdre
            vertices.Add(new Vector3(-1, t, 0).normalized);
            vertices.Add(new Vector3(1, t, 0).normalized);
            vertices.Add(new Vector3(-1, -t, 0).normalized);
            vertices.Add(new Vector3(1, -t, 0).normalized);
            
            vertices.Add(new Vector3(0, -1, t).normalized);
            vertices.Add(new Vector3(0, 1, t).normalized);
            vertices.Add(new Vector3(0, -1, -t).normalized);
            vertices.Add(new Vector3(0, 1, -t).normalized);
            
            vertices.Add(new Vector3(t, 0, -1).normalized);
            vertices.Add(new Vector3(t, 0, 1).normalized);
            vertices.Add(new Vector3(-t, 0, -1).normalized);
            vertices.Add(new Vector3(-t, 0, 1).normalized);
            
            return vertices;
        }
        
        private List<int> GenerateIcosahedronTriangles()
        {
            return new List<int>
            {
                0, 11, 5,    0, 5, 1,     0, 1, 7,     0, 7, 10,    0, 10, 11,
                1, 5, 9,     5, 11, 4,    11, 10, 2,   10, 7, 6,    7, 1, 8,
                3, 9, 4,     3, 4, 2,     3, 2, 6,     3, 6, 8,     3, 8, 9,
                4, 9, 5,     2, 4, 11,    6, 2, 10,    8, 6, 7,     9, 8, 1
            };
        }
        
        private void SubdivideTriangle(Vector3 v1, Vector3 v2, Vector3 v3, int level, 
            List<Vector3> vertices, List<int> triangles)
        {
            if (level == 0)
            {
                int index1 = AddVertex(v1, vertices);
                int index2 = AddVertex(v2, vertices);
                int index3 = AddVertex(v3, vertices);
                
                triangles.Add(index1);
                triangles.Add(index2);
                triangles.Add(index3);
                return;
            }
            
            Vector3 v12 = (v1 + v2).normalized;
            Vector3 v23 = (v2 + v3).normalized;
            Vector3 v31 = (v3 + v1).normalized;
            
            SubdivideTriangle(v1, v12, v31, level - 1, vertices, triangles);
            SubdivideTriangle(v2, v23, v12, level - 1, vertices, triangles);
            SubdivideTriangle(v3, v31, v23, level - 1, vertices, triangles);
            SubdivideTriangle(v12, v23, v31, level - 1, vertices, triangles);
        }
        
        private int AddVertex(Vector3 vertex, List<Vector3> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (Vector3.Distance(vertices[i], vertex) < 0.001f)
                {
                    return i;
                }
            }
            
            vertices.Add(vertex);
            return vertices.Count - 1;
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmos || hexCells.Count == 0) return;
            
            Gizmos.color = gizmoColor;
            
            foreach (HexCell cell in hexCells)
            {
                if (!cell.isVisible) continue;
                
                // Dessiner le centre
                Gizmos.DrawWireSphere(cell.center * radius, 0.02f);
            }
        }
    }
}
