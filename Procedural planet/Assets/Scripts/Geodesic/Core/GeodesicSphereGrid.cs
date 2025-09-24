using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Grille géodésique sphérique comme un ballon de football
    /// </summary>
    public class GeodesicSphereGrid : MonoBehaviour
    {
        [Header("Configuration")]
        public int frequency = 11; // Fréquence de subdivision (n=11 = ~1212 cellules)
        public float cellSize = 1f; // Taille d'une cellule (détermine le rayon automatiquement)
        public bool showDebugInfo = true;
        public bool calculateNeighbors = true; // Calcule les voisins (peut être coûteux)
        public int maxSubdivisions = 5; // Maximum de subdivisions pour éviter les calculs trop longs
        
        [Header("Données")]
        public List<GeodesicSphereCell> cells = new List<GeodesicSphereCell>();
        
        [Header("Debug")]
        public bool showGridGizmos = false;
        public bool showCellTypes = false;
        
        private int nextCellId = 0;
        private float calculatedRadius;
        
        /// <summary>
        /// Calcule le nombre de cellules pour une fréquence donnée
        /// Formule: V = 10*n² + 2
        /// </summary>
        public int GetCellCount(int freq)
        {
            return 10 * freq * freq + 2;
        }
        
        /// <summary>
        /// Calcule le rayon de la sphère basé sur la taille des cellules
        /// </summary>
        public float CalculateRadius(int freq, float cellSize)
        {
            int cellCount = GetCellCount(freq);
            float totalArea = cellCount * cellSize * cellSize;
            return Mathf.Sqrt(totalArea / (4f * Mathf.PI));
        }
        
        
        /// <summary>
        /// Génère la grille géodésique sphérique
        /// </summary>
        public void GenerateSphereGrid()
        {
            if (showDebugInfo)
            {
                Debug.Log($"=== GÉNÉRATION GRILLE SPHÉRIQUE ===");
                Debug.Log($"Fréquence: {frequency}");
                Debug.Log($"Taille des cellules: {cellSize}");
            }
            
            // Calcule le rayon automatiquement
            calculatedRadius = CalculateRadius(frequency, cellSize);
            
            if (showDebugInfo)
            {
                int expectedCells = GetCellCount(frequency);
                Debug.Log($"Cellules attendues: {expectedCells}");
                Debug.Log($"Rayon calculé: {calculatedRadius:F2}");
            }
            
            cells.Clear();
            nextCellId = 0;
            
            // Génère la grille géodésique correcte
            GenerateGeodesicGrid();
            
            if (showDebugInfo)
            {
                int pentagons = cells.Count(c => c.sides == 5);
                int hexagons = cells.Count(c => c.sides == 6);
                Debug.Log($"Grille sphérique générée: {cells.Count} cellules");
                Debug.Log($"- Pentagones: {pentagons}");
                Debug.Log($"- Hexagones: {hexagons}");
                Debug.Log($"- Rayon final: {calculatedRadius:F2}");
                Debug.Log("=== FIN GÉNÉRATION ===");
            }
        }
        
        /// <summary>
        /// Génère la vraie grille géodésique
        /// </summary>
        private void GenerateGeodesicGrid()
        {
            if (showDebugInfo)
            {
                Debug.Log($"Début génération géodésique - Fréquence: {frequency}");
            }
            
            // Génère l'icosaèdre de base
            GenerateIcosahedron();
            
            // Limite la subdivision pour éviter les calculs trop longs
            int actualMaxSubdivisions = Mathf.Min(frequency, maxSubdivisions);
            
            if (showDebugInfo)
            {
                Debug.Log($"Subdivisions limitées à: {actualMaxSubdivisions}");
            }
            
            // Subdivise progressivement
            for (int i = 0; i < actualMaxSubdivisions; i++)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Subdivision {i + 1}/{actualMaxSubdivisions} - Cellules: {cells.Count}");
                }
                
                SubdivideGeodesicGrid();
            }
            
            // Normalise les positions sur la sphère
            NormalizePositions();
            
            // Trouve les voisins (seulement si activé et pas trop de cellules)
            if (calculateNeighbors && cells.Count < 10000)
            {
                FindNeighbors();
            }
            else
            {
                if (showDebugInfo)
                {
                    if (!calculateNeighbors)
                    {
                        Debug.Log("Calcul des voisins désactivé");
                    }
                    else
                    {
                        Debug.Log("Trop de cellules pour calculer les voisins - Skipping");
                    }
                }
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
                GeodesicSphereCell cell = new GeodesicSphereCell(nextCellId++, center, faceVertices);
                cells.Add(cell);
            }
        }
        
        /// <summary>
        /// Subdivise la grille géodésique correctement
        /// </summary>
        private void SubdivideGeodesicGrid()
        {
            List<GeodesicSphereCell> newCells = new List<GeodesicSphereCell>();
            
            foreach (GeodesicSphereCell cell in cells)
            {
                Vector3[] vertices = cell.vertices;
                
                // Subdivise chaque triangle en 4 triangles
                Vector3[] midpoints = new Vector3[3];
                for (int i = 0; i < 3; i++)
                {
                    Vector3 edge = (vertices[i] + vertices[(i + 1) % 3]) / 2f;
                    midpoints[i] = edge.normalized; // Normalise sur la sphère
                }
                
                // Crée 4 nouveaux triangles
                Vector3[] newVertices1 = { vertices[0], midpoints[0], midpoints[2] };
                Vector3[] newVertices2 = { midpoints[0], vertices[1], midpoints[1] };
                Vector3[] newVertices3 = { midpoints[2], midpoints[1], vertices[2] };
                Vector3[] newVertices4 = { midpoints[0], midpoints[1], midpoints[2] };
                
                // Ajoute les nouveaux triangles
                newCells.Add(new GeodesicSphereCell(nextCellId++, (vertices[0] + midpoints[0] + midpoints[2]) / 3f, newVertices1));
                newCells.Add(new GeodesicSphereCell(nextCellId++, (midpoints[0] + vertices[1] + midpoints[1]) / 3f, newVertices2));
                newCells.Add(new GeodesicSphereCell(nextCellId++, (midpoints[2] + midpoints[1] + vertices[2]) / 3f, newVertices3));
                newCells.Add(new GeodesicSphereCell(nextCellId++, (midpoints[0] + midpoints[1] + midpoints[2]) / 3f, newVertices4));
            }
            
            cells = newCells;
        }
        
        /// <summary>
        /// Trouve les voisins de chaque cellule (version optimisée)
        /// </summary>
        private void FindNeighbors()
        {
            if (showDebugInfo)
            {
                Debug.Log($"Calcul des voisins pour {cells.Count} cellules...");
            }
            
            // Crée un dictionnaire pour accélérer la recherche
            Dictionary<Vector3, List<GeodesicSphereCell>> vertexMap = new Dictionary<Vector3, List<GeodesicSphereCell>>();
            
            // Indexe les cellules par leurs sommets
            foreach (GeodesicSphereCell cell in cells)
            {
                foreach (Vector3 vertex in cell.vertices)
                {
                    Vector3 roundedVertex = new Vector3(
                        Mathf.Round(vertex.x * 1000f) / 1000f,
                        Mathf.Round(vertex.y * 1000f) / 1000f,
                        Mathf.Round(vertex.z * 1000f) / 1000f
                    );
                    
                    if (!vertexMap.ContainsKey(roundedVertex))
                    {
                        vertexMap[roundedVertex] = new List<GeodesicSphereCell>();
                    }
                    vertexMap[roundedVertex].Add(cell);
                }
            }
            
            // Trouve les voisins
            foreach (GeodesicSphereCell cell in cells)
            {
                cell.neighborIds.Clear();
                HashSet<int> neighborSet = new HashSet<int>();
                
                foreach (Vector3 vertex in cell.vertices)
                {
                    Vector3 roundedVertex = new Vector3(
                        Mathf.Round(vertex.x * 1000f) / 1000f,
                        Mathf.Round(vertex.y * 1000f) / 1000f,
                        Mathf.Round(vertex.z * 1000f) / 1000f
                    );
                    
                    if (vertexMap.ContainsKey(roundedVertex))
                    {
                        foreach (GeodesicSphereCell otherCell in vertexMap[roundedVertex])
                        {
                            if (otherCell.id != cell.id)
                            {
                                neighborSet.Add(otherCell.id);
                            }
                        }
                    }
                }
                
                cell.neighborIds.AddRange(neighborSet);
                cell.sides = cell.neighborIds.Count;
            }
            
            if (showDebugInfo)
            {
                Debug.Log("Calcul des voisins terminé !");
            }
        }
        
        /// <summary>
        /// Normalise les positions sur la sphère
        /// </summary>
        private void NormalizePositions()
        {
            foreach (GeodesicSphereCell cell in cells)
            {
                cell.centerPosition = cell.centerPosition.normalized * calculatedRadius;
                
                for (int i = 0; i < cell.vertices.Length; i++)
                {
                    cell.vertices[i] = cell.vertices[i].normalized * calculatedRadius;
                }
            }
        }
        
        /// <summary>
        /// Obtient une cellule par son ID
        /// </summary>
        public GeodesicSphereCell GetCell(int id)
        {
            return cells.FirstOrDefault(c => c.id == id);
        }
        
        /// <summary>
        /// Obtient toutes les cellules d'un type donné
        /// </summary>
        public List<GeodesicSphereCell> GetCellsByType(GeodesicSphereCell.CellType type)
        {
            return cells.Where(c => c.cellType == type).ToList();
        }
        
        /// <summary>
        /// Obtient les statistiques de la grille
        /// </summary>
        public string GetGridStats()
        {
            int totalCells = cells.Count;
            int waterCells = cells.Count(c => c.IsWater());
            int landCells = cells.Count(c => c.IsLand());
            int buildableCells = cells.Count(c => c.isBuildable);
            int pentagons = cells.Count(c => c.sides == 5);
            int hexagons = cells.Count(c => c.sides == 6);
            
            return $"Grille: {totalCells} cellules | Eau: {waterCells} | Terre: {landCells} | Constructibles: {buildableCells} | Pentagones: {pentagons} | Hexagones: {hexagons}";
        }
        
        private void OnDrawGizmos()
        {
            if (!showGridGizmos) return;
            
            foreach (GeodesicSphereCell cell in cells)
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
