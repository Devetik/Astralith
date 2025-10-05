using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HexSphere
{
    /// <summary>
    /// Système de pavage hexagonal amélioré pour la sphère
    /// </summary>
    public class HexSphereTiling : MonoBehaviour
    {
        [Header("Paramètres de Pavage")]
        [Range(1, 6)]
        public int tilingLevel = 3;
        
        [Range(0.1f, 10f)]
        public float radius = 1f;
        
        [Range(0.1f, 2f)]
        public float hexSize = 0.3f;
        
        [Header("Options de Pavage")]
        public bool usePerfectTiling = true;
        public bool shareVertices = true;
        public bool optimizeMesh = true;
        
        [Header("Matériaux")]
        public Material hexMaterial;
        
        // Données de pavage
        [HideInInspector]
        public List<HexTile> hexTiles = new List<HexTile>();
        private Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
        [HideInInspector]
        public Mesh hexMesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        
        private void Start()
        {
            GeneratePerfectHexTiling();
        }
        
        /// <summary>
        /// Génère un pavage hexagonal parfait
        /// </summary>
        [ContextMenu("Générer Pavage Parfait")]
        public void GeneratePerfectHexTiling()
        {
            Debug.Log("Génération d'un pavage hexagonal parfait...");
            
            ClearTiling();
            
            // Étape 1: Créer les centres des hexagones avec pavage hexagonal
            CreateHexCenters();
            
            // Étape 2: Calculer les vertices partagés
            CalculateSharedVertices();
            
            // Étape 3: Générer le mesh optimisé
            GenerateOptimizedMesh();
            
            // Étape 4: Appliquer les matériaux
            ApplyMaterials();
            
            Debug.Log($"Pavage généré: {hexTiles.Count} hexagones, {vertexMap.Count} vertices uniques");
        }
        
        /// <summary>
        /// Crée les centres des hexagones avec un pavage hexagonal correct
        /// </summary>
        private void CreateHexCenters()
        {
            hexTiles.Clear();
            
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
                
                SubdivideTriangle(v1, v2, v3, tilingLevel, subdividedVertices, subdividedTriangles);
            }
            
            // Normaliser les vertices pour qu'ils soient sur la sphère
            for (int i = 0; i < subdividedVertices.Count; i++)
            {
                subdividedVertices[i] = subdividedVertices[i].normalized;
            }
            
            // Créer les tuiles hexagonales
            CreateHexTiles(subdividedVertices);
        }
        
        /// <summary>
        /// Crée les tuiles hexagonales à partir des centres
        /// </summary>
        private void CreateHexTiles(List<Vector3> centers)
        {
            for (int i = 0; i < centers.Count; i++)
            {
                Vector3 center = centers[i];
                Vector3 normal = center.normalized;
                
                HexTile tile = new HexTile(center, normal, i, hexSize);
                hexTiles.Add(tile);
            }
        }
        
        /// <summary>
        /// Calcule les vertices partagés entre les hexagones voisins
        /// </summary>
        private void CalculateSharedVertices()
        {
            vertexMap.Clear();
            
            foreach (HexTile tile in hexTiles)
            {
                // Calculer les 6 vertices de l'hexagone
                Vector3[] hexVertices = CalculateHexVertices(tile.center, tile.normal, hexSize);
                
                for (int i = 0; i < 6; i++)
                {
                    Vector3 vertex = hexVertices[i];
                    
                    // Chercher un vertex existant proche
                    int existingIndex = FindOrCreateVertex(vertex);
                    tile.vertexIndices[i] = existingIndex;
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
        /// Trouve ou crée un vertex dans la map
        /// </summary>
        private int FindOrCreateVertex(Vector3 vertex)
        {
            // Chercher un vertex existant proche
            foreach (var kvp in vertexMap)
            {
                if (Vector3.Distance(kvp.Key, vertex) < 0.001f)
                {
                    return kvp.Value;
                }
            }
            
            // Créer un nouveau vertex
            int index = vertexMap.Count;
            vertexMap[vertex] = index;
            return index;
        }
        
        /// <summary>
        /// Génère le mesh optimisé avec vertices partagés
        /// </summary>
        private void GenerateOptimizedMesh()
        {
            if (hexMesh == null)
            {
                hexMesh = new Mesh();
                hexMesh.name = "HexSphereTiling";
            }
            
            // Créer les arrays de vertices
            Vector3[] vertices = new Vector3[vertexMap.Count];
            Vector2[] uvs = new Vector2[vertexMap.Count];
            Color[] colors = new Color[vertexMap.Count];
            
            foreach (var kvp in vertexMap)
            {
                vertices[kvp.Value] = kvp.Key;
                uvs[kvp.Value] = CalculateUV(kvp.Key);
                colors[kvp.Value] = Color.white;
            }
            
            // Créer les triangles
            List<int> triangles = new List<int>();
            
            foreach (HexTile tile in hexTiles)
            {
                if (!tile.isVisible) continue;
                
                // Ajouter les triangles de l'hexagone
                for (int i = 0; i < 4; i++) // 4 triangles pour un hexagone
                {
                    int baseIndex = i * 3;
                    triangles.Add(tile.vertexIndices[0]);
                    triangles.Add(tile.vertexIndices[i + 1]);
                    triangles.Add(tile.vertexIndices[i + 2]);
                }
            }
            
            // Assigner au mesh
            hexMesh.Clear();
            hexMesh.vertices = vertices;
            hexMesh.triangles = triangles.ToArray();
            hexMesh.uv = uvs;
            hexMesh.colors = colors;
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
        /// Calcule les UVs pour un vertex
        /// </summary>
        private Vector2 CalculateUV(Vector3 vertex)
        {
            // Projection sphérique simple
            float u = 0.5f + Mathf.Atan2(vertex.z, vertex.x) / (2f * Mathf.PI);
            float v = 0.5f - Mathf.Asin(vertex.y) / Mathf.PI;
            return new Vector2(u, v);
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
        /// Nettoie le pavage
        /// </summary>
        [ContextMenu("Nettoyer Pavage")]
        public void ClearTiling()
        {
            hexTiles.Clear();
            vertexMap.Clear();
            
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
        
        /// <summary>
        /// Génère les vertices d'un icosaèdre
        /// </summary>
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
        
        /// <summary>
        /// Génère les triangles de l'icosaèdre
        /// </summary>
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
        
        /// <summary>
        /// Subdivise récursivement un triangle
        /// </summary>
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
        
        /// <summary>
        /// Ajoute un vertex s'il n'existe pas déjà
        /// </summary>
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
            if (hexTiles.Count == 0) return;
            
            Gizmos.color = Color.yellow;
            
            foreach (HexTile tile in hexTiles)
            {
                if (!tile.isVisible) continue;
                
                // Dessiner le centre
                Gizmos.DrawWireSphere(tile.center * radius, 0.02f);
            }
        }
    }
    
    /// <summary>
    /// Représente une tuile hexagonale dans le pavage
    /// </summary>
    [System.Serializable]
    public class HexTile
    {
        public Vector3 center;
        public Vector3 normal;
        public int index;
        public float hexSize;
        public bool isVisible = true;
        public Color color = Color.white;
        
        // Indices des vertices dans le mesh global
        public int[] vertexIndices = new int[6];
        
        public HexTile(Vector3 center, Vector3 normal, int index, float hexSize)
        {
            this.center = center;
            this.normal = normal;
            this.index = index;
            this.hexSize = hexSize;
        }
    }
}
