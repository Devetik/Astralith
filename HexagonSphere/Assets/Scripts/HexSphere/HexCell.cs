using UnityEngine;

namespace HexSphere
{
    /// <summary>
    /// Représente une cellule hexagonale individuelle sur la sphère
    /// </summary>
    [System.Serializable]
    public class HexCell
    {
        [Header("Position et Orientation")]
        public Vector3 center;
        public Vector3 normal;
        public int index;
        
        [Header("Géométrie")]
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        public int[] vertexIndices = new int[6]; // Indices dans le mesh global
        
        [Header("Propriétés")]
        public Color color = Color.white;
        public float elevation = 0f;
        public bool isVisible = true;
        public float hexSize = 0.3f;
        
        [Header("Voisins")]
        public HexCell[] neighbors = new HexCell[6];
        
        public HexCell(Vector3 center, Vector3 normal, int index)
        {
            this.center = center;
            this.normal = normal;
            this.index = index;
            this.vertices = new Vector3[6];
            this.triangles = new int[12]; // 2 triangles par hexagone
            this.uvs = new Vector2[6];
        }
        
        public HexCell(Vector3 center, Vector3 normal, int index, float hexSize)
        {
            this.center = center;
            this.normal = normal;
            this.index = index;
            this.hexSize = hexSize;
            this.vertices = new Vector3[6];
            this.triangles = new int[12]; // 2 triangles par hexagone
            this.uvs = new Vector2[6];
        }
        
        /// <summary>
        /// Calcule les vertices de l'hexagone
        /// </summary>
        public void CalculateVertices(float radius, float hexSize)
        {
            // Créer un hexagone dans le plan XY
            Vector3[] hexVertices = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                hexVertices[i] = new Vector3(
                    Mathf.Cos(angle) * hexSize,
                    Mathf.Sin(angle) * hexSize,
                    0f
                );
            }
            
            // Transformer l'hexagone pour qu'il soit orienté selon la normale
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);
            Vector3 scaledCenter = center * radius;
            
            for (int i = 0; i < 6; i++)
            {
                vertices[i] = scaledCenter + rotation * hexVertices[i];
            }
            
            // Calculer les triangles
            CalculateTriangles();
            CalculateUVs();
        }
        
        private void CalculateTriangles()
        {
            // Premier triangle (0, 1, 2)
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            
            // Deuxième triangle (0, 2, 3)
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            
            // Troisième triangle (0, 3, 4)
            triangles[6] = 0;
            triangles[7] = 3;
            triangles[8] = 4;
            
            // Quatrième triangle (0, 4, 5)
            triangles[9] = 0;
            triangles[10] = 4;
            triangles[11] = 5;
        }
        
        private void CalculateUVs()
        {
            for (int i = 0; i < 6; i++)
            {
                // UVs basiques pour l'hexagone
                float angle = i * 60f * Mathf.Deg2Rad;
                uvs[i] = new Vector2(
                    0.5f + 0.5f * Mathf.Cos(angle),
                    0.5f + 0.5f * Mathf.Sin(angle)
                );
            }
        }
        
        /// <summary>
        /// Applique l'élévation à la cellule
        /// </summary>
        public void ApplyElevation(float elevationAmount)
        {
            elevation = elevationAmount;
            Vector3 elevationOffset = normal * elevationAmount;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += elevationOffset;
            }
        }
        
        /// <summary>
        /// Obtient la distance entre cette cellule et un point
        /// </summary>
        public float GetDistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(center, point);
        }
        
        /// <summary>
        /// Vérifie si un point est à l'intérieur de cette cellule
        /// </summary>
        public bool ContainsPoint(Vector3 point)
        {
            // Projection du point sur le plan de la cellule
            Vector3 toPoint = point - center;
            float distance = Vector3.Dot(toPoint, normal);
            Vector3 projectedPoint = point - normal * distance;
            
            // Vérification simple basée sur la distance au centre
            return Vector3.Distance(projectedPoint, center) <= Vector3.Distance(vertices[0], center);
        }
    }
}
