using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Grille géodésique dual pour cellules uniformes de construction
    /// </summary>
    public class GeodesicDualGrid : MonoBehaviour
    {
        [Header("Configuration")]
        public int subdivisionLevel = 4;
        public float planetRadius = 5f;
        public bool showDebugInfo = true;
        
        [Header("Données")]
        public List<GeodesicDualCell> cells = new List<GeodesicDualCell>();
        public List<ContinentData> continents = new List<ContinentData>();
        
        [Header("Debug")]
        public bool showGridGizmos = false;
        public bool showCellTypes = false;
        public bool showContinents = false;
        
        private int nextCellId = 0;
        
        
        [System.Serializable]
        public class ContinentData
        {
            public int id;
            public string name;
            public List<int> cellIds;
            public Vector3 center;
            public float area;
            public bool isMainContinent;
            
            public ContinentData(int id, string name)
            {
                this.id = id;
                this.name = name;
                this.cellIds = new List<int>();
                this.center = Vector3.zero;
                this.area = 0f;
                this.isMainContinent = false;
            }
        }
        
        /// <summary>
        /// Génère la grille géodésique dual
        /// </summary>
        public void GenerateDualGrid()
        {
            if (showDebugInfo)
            {
                Debug.Log($"=== GÉNÉRATION GRILLE DUAL ===");
                Debug.Log($"Niveau de subdivision: {subdivisionLevel}");
                Debug.Log($"Rayon de la planète: {planetRadius}");
            }
            
            cells.Clear();
            nextCellId = 0;
            
            // Génère l'icosaèdre de base
            GenerateIcosahedron();
            
            // Subdivise selon le niveau
            for (int i = 0; i < subdivisionLevel; i++)
            {
                SubdivideGrid();
            }
            
            // Normalise les positions sur la sphère
            NormalizePositions();
            
            // Calcule les voisins
            CalculateNeighbors();
            
            if (showDebugInfo)
            {
                Debug.Log($"Grille dual générée: {cells.Count} cellules");
                Debug.Log("=== FIN GÉNÉRATION ===");
            }
        }
        
        /// <summary>
        /// Génère l'icosaèdre de base
        /// </summary>
        private void GenerateIcosahedron()
        {
            // Crée les 12 sommets de l'icosaèdre
            float t = (1f + Mathf.Sqrt(5f)) / 2f; // Nombre d'or
            
            Vector3[] vertices = {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            
            // Crée les 20 faces triangulaires
            int[][] faces = {
                new int[] {0, 11, 5}, new int[] {0, 5, 1}, new int[] {0, 1, 7}, new int[] {0, 7, 10}, new int[] {0, 10, 11},
                new int[] {1, 5, 9}, new int[] {5, 11, 4}, new int[] {11, 10, 2}, new int[] {10, 7, 6}, new int[] {7, 1, 8},
                new int[] {3, 9, 4}, new int[] {3, 4, 2}, new int[] {3, 2, 6}, new int[] {3, 6, 8}, new int[] {3, 8, 9},
                new int[] {4, 9, 5}, new int[] {2, 4, 11}, new int[] {6, 2, 10}, new int[] {8, 6, 7}, new int[] {9, 8, 1}
            };
            
            // Crée les cellules initiales
            for (int i = 0; i < faces.Length; i++)
            {
                Vector3[] faceVertices = {
                    vertices[faces[i][0]],
                    vertices[faces[i][1]],
                    vertices[faces[i][2]]
                };
                
                Vector3 center = (faceVertices[0] + faceVertices[1] + faceVertices[2]) / 3f;
                GeodesicDualCell cell = new GeodesicDualCell(nextCellId++, center, faceVertices);
                cells.Add(cell);
            }
        }
        
        /// <summary>
        /// Subdivise la grille
        /// </summary>
        private void SubdivideGrid()
        {
            List<GeodesicDualCell> newCells = new List<GeodesicDualCell>();
            
            foreach (GeodesicDualCell cell in cells)
            {
                // Subdivise chaque triangle en 4 triangles
                Vector3[] vertices = cell.vertices;
                Vector3 center = cell.centerPosition;
                
                // Crée 4 nouveaux triangles
                Vector3[] newVertices1 = { vertices[0], vertices[1], center };
                Vector3[] newVertices2 = { vertices[1], vertices[2], center };
                Vector3[] newVertices3 = { vertices[2], vertices[0], center };
                Vector3[] newVertices4 = { center, center, center }; // Triangle central
                
                // Ajoute les nouveaux triangles
                newCells.Add(new GeodesicDualCell(nextCellId++, newVertices1[0], newVertices1));
                newCells.Add(new GeodesicDualCell(nextCellId++, newVertices2[0], newVertices2));
                newCells.Add(new GeodesicDualCell(nextCellId++, newVertices3[0], newVertices3));
                newCells.Add(new GeodesicDualCell(nextCellId++, center, newVertices4));
            }
            
            cells = newCells;
        }
        
        /// <summary>
        /// Normalise les positions sur la sphère
        /// </summary>
        private void NormalizePositions()
        {
            foreach (GeodesicDualCell cell in cells)
            {
                cell.centerPosition = cell.centerPosition.normalized * planetRadius;
                
                for (int i = 0; i < cell.vertices.Length; i++)
                {
                    cell.vertices[i] = cell.vertices[i].normalized * planetRadius;
                }
            }
        }
        
        /// <summary>
        /// Calcule les voisins de chaque cellule
        /// </summary>
        private void CalculateNeighbors()
        {
            foreach (GeodesicDualCell cell in cells)
            {
                cell.neighborIds.Clear();
                
                foreach (GeodesicDualCell otherCell in cells)
                {
                    if (cell.id == otherCell.id) continue;
                    
                    // Vérifie si les cellules partagent un sommet
                    bool shareVertex = false;
                    foreach (Vector3 vertex in cell.vertices)
                    {
                        foreach (Vector3 otherVertex in otherCell.vertices)
                        {
                            if (Vector3.Distance(vertex, otherVertex) < 0.01f)
                            {
                                shareVertex = true;
                                break;
                            }
                        }
                        if (shareVertex) break;
                    }
                    
                    if (shareVertex)
                    {
                        cell.neighborIds.Add(otherCell.id);
                    }
                }
            }
        }
        
        /// <summary>
        /// Obtient une cellule par son ID
        /// </summary>
        public GeodesicDualCell GetCell(int id)
        {
            return cells.FirstOrDefault(c => c.id == id);
        }
        
        /// <summary>
        /// Obtient les cellules voisines
        /// </summary>
        public List<GeodesicDualCell> GetNeighbors(int cellId)
        {
            GeodesicDualCell cell = GetCell(cellId);
            if (cell == null) return new List<GeodesicDualCell>();
            
            List<GeodesicDualCell> neighbors = new List<GeodesicDualCell>();
            foreach (int neighborId in cell.neighborIds)
            {
                GeodesicDualCell neighbor = GetCell(neighborId);
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        private void OnDrawGizmos()
        {
            if (!showGridGizmos) return;
            
            foreach (GeodesicDualCell cell in cells)
            {
                if (showCellTypes)
                {
                    Gizmos.color = cell.GetDebugColor();
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                
                // Dessine les arêtes de la cellule
                for (int i = 0; i < cell.vertices.Length; i++)
                {
                    Vector3 start = cell.vertices[i];
                    Vector3 end = cell.vertices[(i + 1) % cell.vertices.Length];
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}
