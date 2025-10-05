using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HexSphere
{
    /// <summary>
    /// Générateur de sphère hexagonale géodésique
    /// </summary>
    public class HexSphereGenerator : MonoBehaviour
    {
        [Header("Paramètres de la Sphère")]
        [Range(1, 5)]
        public int subdivisionLevel = 2;
        
        [Range(0.1f, 10f)]
        public float radius = 1f;
        
        [Range(0.1f, 2f)]
        public float hexSize = 0.3f;
        
        [Header("Matériaux")]
        public Material hexMaterial;
        
        [Header("Options de Génération")]
        public bool generateOnStart = true;
        public bool showGizmos = true;
        public Color gizmoColor = Color.yellow;
        
        [Header("Élévation")]
        public bool useElevation = false;
        public AnimationCurve elevationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float maxElevation = 0.1f;
        
        [Header("Couleurs")]
        public Gradient colorGradient;
        public bool useGradient = false;
        
        // Données générées
        [HideInInspector]
        public List<HexCell> hexCells = new List<HexCell>();
        
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        [HideInInspector]
        public Mesh hexMesh;
        
        private void Start()
        {
            if (generateOnStart)
            {
                GenerateHexSphere();
            }
        }
        
        /// <summary>
        /// Génère la sphère hexagonale complète
        /// </summary>
        [ContextMenu("Générer Sphère Hexagonale")]
        public void GenerateHexSphere()
        {
            Debug.Log($"Génération d'une sphère hexagonale - Niveau: {subdivisionLevel}, Rayon: {radius}");
            
            // Nettoyer les données existantes
            ClearHexSphere();
            
            // Générer les centres des hexagones
            GenerateHexCenters();
            
            // Calculer les vertices et triangles
            CalculateHexGeometry();
            
            // Générer le mesh
            GenerateMesh();
            
            // Appliquer les matériaux
            ApplyMaterials();
            
            Debug.Log($"Sphère hexagonale générée avec {hexCells.Count} hexagones");
        }
        
        /// <summary>
        /// Génère les centres des hexagones sur la sphère
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
            CreateHexCells(subdividedVertices);
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
        
        /// <summary>
        /// Crée les cellules hexagonales à partir des vertices
        /// </summary>
        private void CreateHexCells(List<Vector3> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 center = vertices[i];
                Vector3 normal = center.normalized;
                
                HexCell cell = new HexCell(center, normal, i, hexSize);
                hexCells.Add(cell);
            }
        }
        
        /// <summary>
        /// Calcule la géométrie de chaque hexagone
        /// </summary>
        private void CalculateHexGeometry()
        {
            foreach (HexCell cell in hexCells)
            {
                cell.CalculateVertices(radius, hexSize);
                
                // Appliquer l'élévation si activée
                if (useElevation)
                {
                    float elevationAmount = elevationCurve.Evaluate(Random.Range(0f, 1f)) * maxElevation;
                    cell.ApplyElevation(elevationAmount);
                }
                
                // Appliquer les couleurs
                if (useGradient)
                {
                    float t = Random.Range(0f, 1f);
                    cell.color = colorGradient.Evaluate(t);
                }
            }
        }
        
        /// <summary>
        /// Génère le mesh combiné de tous les hexagones
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
        /// Obtient la cellule hexagonale la plus proche d'un point
        /// </summary>
        public HexCell GetNearestHexCell(Vector3 worldPosition)
        {
            if (hexCells.Count == 0) return null;
            
            HexCell nearest = hexCells[0];
            float nearestDistance = nearest.GetDistanceToPoint(worldPosition);
            
            foreach (HexCell cell in hexCells)
            {
                float distance = cell.GetDistanceToPoint(worldPosition);
                if (distance < nearestDistance)
                {
                    nearest = cell;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
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
                
                // Dessiner les vertices
                for (int i = 0; i < cell.vertices.Length; i++)
                {
                    int nextIndex = (i + 1) % cell.vertices.Length;
                    Gizmos.DrawLine(cell.vertices[i], cell.vertices[nextIndex]);
                }
            }
        }
        
        private void OnValidate()
        {
            // Limiter les valeurs
            subdivisionLevel = Mathf.Clamp(subdivisionLevel, 1, 5);
            radius = Mathf.Max(0.1f, radius);
            hexSize = Mathf.Max(0.01f, hexSize);
        }
    }
}
