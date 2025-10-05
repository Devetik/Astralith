using UnityEngine;
using System.Collections.Generic;
using System;

namespace HexasphereProcedural {

    /// <summary>
    /// Point avec hauteur variable pour le système de terrain procédural
    /// </summary>
    public class ProceduralPoint {
        public float x, y, z;
        public float height;
        public Vector3 worldPosition;
        public bool isComputed = false;

        public ProceduralPoint(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.height = 0f;
        }

        public Vector3 GetWorldPosition(float radius) {
            if (!isComputed) {
                ComputeWorldPosition(radius);
            }
            return worldPosition;
        }

        private void ComputeWorldPosition(float radius) {
            // Normaliser le point sur la sphère
            float length = Mathf.Sqrt(x * x + y * y + z * z);
            float normalizedX = x / length;
            float normalizedY = y / length;
            float normalizedZ = z / length;

            // Appliquer la hauteur
            float finalRadius = radius + height;
            worldPosition = new Vector3(
                normalizedX * finalRadius,
                normalizedY * finalRadius,
                normalizedZ * finalRadius
            );
            isComputed = true;
        }

        public void SetHeight(float newHeight) {
            height = newHeight;
            isComputed = false; // Force le recalcul
        }
    }

    /// <summary>
    /// Tile hexagonal avec hauteurs variables pour chaque point
    /// </summary>
    public class ProceduralTile {
        public int index;
        public ProceduralPoint[] vertices;
        public ProceduralPoint centerPoint;
        public Vector3 center;
        public bool isPentagon => vertices.Length == 5;
        public bool visible = true;

        public ProceduralTile(ProceduralPoint centerPoint, int index) {
            this.index = index;
            this.centerPoint = centerPoint;
            this.center = centerPoint.GetWorldPosition(1f);
        }

        public void UpdateCenter(float radius) {
            center = centerPoint.GetWorldPosition(radius);
        }
    }


    /// <summary>
    /// Système principal de génération de terrain procédural
    /// </summary>
    public class ProceduralHexasphere : MonoBehaviour {
        
        [Header("Configuration de la sphère")]
        [SerializeField] public int numDivisions = 8;
        [SerializeField] public float radius = 1f;
        
        [Header("Configuration du terrain")]
        [SerializeField] public float noiseScale = 1f;
        [SerializeField] public float heightAmplitude = 0.2f;
        [SerializeField] public int noiseOctaves = 4;
        [SerializeField] public float noisePersistence = 0.5f;
        [SerializeField] public float noiseLacunarity = 2f;
        
        [Header("Seuils de terrain")]
        [SerializeField] public float waterLevel = 0.1f;
        [SerializeField] public float mountainLevel = 0.3f;
        
        [Header("Matériaux")]
        [SerializeField] public Material waterMaterial;
        [SerializeField] public Material landMaterial;
        [SerializeField] public Material mountainMaterial;
        
        [Header("Debug")]
        [SerializeField] public bool showDebugInfo = true;
        
        // Données générées
        public ProceduralPoint[] points;
        public ProceduralTile[] tiles;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        
        // Événements
        public System.Action<ProceduralHexasphere> OnGenerationComplete;
        public System.Action<int> OnTileClick;

        void Start() {
            GenerateHexasphere();
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.R)) {
                RegenerateTerrain();
            }
        }

        /// <summary>
        /// Génère la sphère hexagonale complète
        /// </summary>
        public void GenerateHexasphere() {
            Debug.Log("Génération de la sphère hexagonale procédurale...");
            
            // Générer les points de base
            GenerateBasePoints();
            
            // Générer les tiles
            GenerateTiles();
            
            // Appliquer le bruit de Perlin
            ApplyPerlinNoise();
            
            // Générer le mesh
            GenerateMesh();
            
            // Appliquer les matériaux
            ApplyMaterials();
            
            OnGenerationComplete?.Invoke(this);
            
            Debug.Log($"Génération terminée : {points.Length} points, {tiles.Length} tiles");
        }

        /// <summary>
        /// Génère les points de base de la sphère
        /// </summary>
        private void GenerateBasePoints() {
            // Créer un icosaèdre de base
            List<ProceduralPoint> pointList = new List<ProceduralPoint>();
            List<Triangle> triangles = new List<Triangle>();
            
            // Points de l'icosaèdre
            float t = (1f + Mathf.Sqrt(5f)) / 2f; // Nombre d'or
            
            // 12 points de l'icosaèdre
            Vector3[] icosahedronVertices = {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            
            // Normaliser et créer les points
            foreach (Vector3 vertex in icosahedronVertices) {
                Vector3 normalized = vertex.normalized;
                pointList.Add(new ProceduralPoint(normalized.x, normalized.y, normalized.z));
            }
            
            // Créer les 20 faces triangulaires de l'icosaèdre
            int[][] icosahedronFaces = {
                new int[] {0, 11, 5}, new int[] {0, 5, 1}, new int[] {0, 1, 7}, new int[] {0, 7, 10}, new int[] {0, 10, 11},
                new int[] {1, 5, 9}, new int[] {5, 11, 4}, new int[] {11, 10, 2}, new int[] {10, 7, 6}, new int[] {7, 1, 8},
                new int[] {3, 9, 4}, new int[] {3, 4, 2}, new int[] {3, 2, 6}, new int[] {3, 6, 8}, new int[] {3, 8, 9},
                new int[] {4, 9, 5}, new int[] {2, 4, 11}, new int[] {6, 2, 10}, new int[] {8, 6, 7}, new int[] {9, 8, 1}
            };
            
            // Créer les triangles initiaux
            foreach (int[] face in icosahedronFaces) {
                triangles.Add(new Triangle(pointList[face[0]], pointList[face[1]], pointList[face[2]]));
            }
            
            // Subdiviser les faces
            for (int division = 0; division < numDivisions; division++) {
                SubdivideTriangles(pointList, triangles);
            }
            
            points = pointList.ToArray();
        }

        /// <summary>
        /// Subdivise les triangles
        /// </summary>
        private void SubdivideTriangles(List<ProceduralPoint> pointList, List<Triangle> triangles) {
            List<Triangle> newTriangles = new List<Triangle>();
            Dictionary<string, ProceduralPoint> edgePoints = new Dictionary<string, ProceduralPoint>();
            
            foreach (Triangle triangle in triangles) {
                // Créer les points du milieu des arêtes
                ProceduralPoint mid1 = GetOrCreateMidPoint(triangle.p1, triangle.p2, edgePoints, pointList);
                ProceduralPoint mid2 = GetOrCreateMidPoint(triangle.p2, triangle.p3, edgePoints, pointList);
                ProceduralPoint mid3 = GetOrCreateMidPoint(triangle.p3, triangle.p1, edgePoints, pointList);
                
                // Créer 4 nouveaux triangles
                newTriangles.Add(new Triangle(triangle.p1, mid1, mid3));
                newTriangles.Add(new Triangle(mid1, triangle.p2, mid2));
                newTriangles.Add(new Triangle(mid3, mid2, triangle.p3));
                newTriangles.Add(new Triangle(mid1, mid2, mid3));
            }
            
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }

        /// <summary>
        /// Obtient ou crée un point milieu entre deux points
        /// </summary>
        private ProceduralPoint GetOrCreateMidPoint(ProceduralPoint p1, ProceduralPoint p2, 
            Dictionary<string, ProceduralPoint> edgePoints, List<ProceduralPoint> pointList) {
            
            string key1 = $"{p1.x},{p1.y},{p1.z}-{p2.x},{p2.y},{p2.z}";
            string key2 = $"{p2.x},{p2.y},{p2.z}-{p1.x},{p1.y},{p1.z}";
            
            if (edgePoints.ContainsKey(key1)) {
                return edgePoints[key1];
            }
            if (edgePoints.ContainsKey(key2)) {
                return edgePoints[key2];
            }
            
            // Créer un nouveau point milieu
            ProceduralPoint midPoint = new ProceduralPoint(
                (p1.x + p2.x) / 2f,
                (p1.y + p2.y) / 2f,
                (p1.z + p2.z) / 2f
            );
            
            // Normaliser le point pour qu'il soit sur la sphère
            float length = Mathf.Sqrt(midPoint.x * midPoint.x + midPoint.y * midPoint.y + midPoint.z * midPoint.z);
            midPoint.x /= length;
            midPoint.y /= length;
            midPoint.z /= length;
            
            edgePoints[key1] = midPoint;
            pointList.Add(midPoint);
            return midPoint;
        }

        /// <summary>
        /// Génère les tiles hexagonaux
        /// </summary>
        private void GenerateTiles() {
            // Pour l'instant, créons des tiles simples basés sur les points
            List<ProceduralTile> tileList = new List<ProceduralTile>();
            
            // Créer un tile pour chaque point (simplifié)
            for (int i = 0; i < points.Length; i++) {
                ProceduralPoint centerPoint = points[i];
                ProceduralTile tile = new ProceduralTile(centerPoint, i);
                tile.vertices = new ProceduralPoint[] { centerPoint };
                tileList.Add(tile);
            }
            
            tiles = tileList.ToArray();
        }

        /// <summary>
        /// Applique le bruit de Perlin pour générer les hauteurs
        /// </summary>
        private void ApplyPerlinNoise() {
            Debug.Log("Application du bruit de Perlin...");
            
            foreach (ProceduralPoint point in points) {
                Vector3 worldPos = point.GetWorldPosition(radius);
                float height = GeneratePerlinHeight(
                    worldPos, noiseScale, heightAmplitude, 
                    noiseOctaves, noisePersistence, noiseLacunarity
                );
                point.SetHeight(height);
            }
            
            // Mettre à jour les centres des tiles
            foreach (ProceduralTile tile in tiles) {
                tile.UpdateCenter(radius);
            }
        }

        /// <summary>
        /// Génère le mesh de la sphère
        /// </summary>
        private void GenerateMesh() {
            if (meshFilter == null) {
                meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter == null) {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }
            
            if (meshRenderer == null) {
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }
            
            Mesh mesh = new Mesh();
            mesh.name = "ProceduralHexasphere";
            
            // Créer les vertices
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            
            // Ajouter tous les points comme vertices
            foreach (ProceduralPoint point in points) {
                Vector3 worldPos = point.GetWorldPosition(radius);
                vertices.Add(worldPos);
                uvs.Add(new Vector2(point.x, point.y));
            }
            
            // Créer une triangulation basée sur la distance
            CreateTriangulation(vertices, triangles);
            
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            meshFilter.mesh = mesh;
        }
        
        /// <summary>
        /// Crée une triangulation cohérente
        /// </summary>
        private void CreateTriangulation(List<Vector3> vertices, List<int> triangles) {
            // Utiliser une approche de triangulation de Delaunay simplifiée
            for (int i = 0; i < vertices.Count; i++) {
                // Trouver les 6 points les plus proches
                List<int> nearestPoints = FindNearestPoints(i, vertices, 6);
                
                // Créer des triangles avec ces points
                for (int j = 0; j < nearestPoints.Count - 2; j++) {
                    int p1 = i;
                    int p2 = nearestPoints[j];
                    int p3 = nearestPoints[j + 1];
                    
                    // Vérifier que le triangle est valide
                    if (IsValidTriangleIndices(p1, p2, p3, vertices)) {
                        triangles.Add(p1);
                        triangles.Add(p2);
                        triangles.Add(p3);
                    }
                }
            }
        }
        
        /// <summary>
        /// Trouve les points les plus proches d'un point donné
        /// </summary>
        private List<int> FindNearestPoints(int centerIndex, List<Vector3> vertices, int count) {
            List<int> nearest = new List<int>();
            List<float> distances = new List<float>();
            
            for (int i = 0; i < vertices.Count; i++) {
                if (i == centerIndex) continue;
                
                float distance = Vector3.Distance(vertices[centerIndex], vertices[i]);
                
                if (nearest.Count < count) {
                    nearest.Add(i);
                    distances.Add(distance);
                } else {
                    // Trouver la distance maximale dans la liste
                    int maxIndex = 0;
                    float maxDistance = distances[0];
                    for (int j = 1; j < distances.Count; j++) {
                        if (distances[j] > maxDistance) {
                            maxDistance = distances[j];
                            maxIndex = j;
                        }
                    }
                    
                    // Remplacer si la nouvelle distance est plus petite
                    if (distance < maxDistance) {
                        nearest[maxIndex] = i;
                        distances[maxIndex] = distance;
                    }
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Vérifie si trois indices forment un triangle valide
        /// </summary>
        private bool IsValidTriangleIndices(int p1, int p2, int p3, List<Vector3> vertices) {
            if (p1 == p2 || p2 == p3 || p1 == p3) return false;
            
            Vector3 v1 = vertices[p1];
            Vector3 v2 = vertices[p2];
            Vector3 v3 = vertices[p3];
            
            // Vérifier que les points ne sont pas colinéaires
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v1;
            Vector3 cross = Vector3.Cross(edge1, edge2);
            
            return cross.magnitude > 0.001f; // Seuil de tolérance
        }
        
        /// <summary>
        /// Vérifie si trois points forment un triangle valide
        /// </summary>
        private bool IsValidTriangle(ProceduralPoint p1, ProceduralPoint p2, ProceduralPoint p3) {
            // Calculer les distances entre les points
            float d12 = Vector3.Distance(new Vector3(p1.x, p1.y, p1.z), new Vector3(p2.x, p2.y, p2.z));
            float d23 = Vector3.Distance(new Vector3(p2.x, p2.y, p2.z), new Vector3(p3.x, p3.y, p3.z));
            float d31 = Vector3.Distance(new Vector3(p3.x, p3.y, p3.z), new Vector3(p1.x, p1.y, p1.z));
            
            // Vérifier que les points ne sont pas trop éloignés
            float maxDistance = 0.5f; // Ajuster selon les besoins
            return d12 < maxDistance && d23 < maxDistance && d31 < maxDistance;
        }

        /// <summary>
        /// Applique les matériaux selon le type de terrain
        /// </summary>
        private void ApplyMaterials() {
            if (meshRenderer == null) return;
            
            // Pour l'instant, utiliser un matériau simple
            // Dans une version plus avancée, on pourrait utiliser des matériaux multiples
            if (landMaterial != null) {
                meshRenderer.material = landMaterial;
            }
        }

        /// <summary>
        /// Génère une hauteur basée sur le bruit de Perlin
        /// </summary>
        private float GeneratePerlinHeight(Vector3 position, float scale, float amplitude, int octaves, float persistence, float lacunarity) {
            float height = 0f;
            float frequency = 1f;
            float amplitudeCurrent = amplitude;

            for (int i = 0; i < octaves; i++) {
                float noiseValue = Mathf.PerlinNoise(
                    position.x * scale * frequency,
                    position.z * scale * frequency
                );
                height += noiseValue * amplitudeCurrent;
                frequency *= lacunarity;
                amplitudeCurrent *= persistence;
            }

            return height;
        }

        /// <summary>
        /// Régénère le terrain avec de nouveaux paramètres
        /// </summary>
        public void RegenerateTerrain() {
            Debug.Log("Régénération du terrain...");
            ApplyPerlinNoise();
            GenerateMesh();
        }

        /// <summary>
        /// Obtient le type de terrain d'un point
        /// </summary>
        public TerrainType GetTerrainType(ProceduralPoint point) {
            if (point.height < waterLevel) {
                return TerrainType.Water;
            } else if (point.height > mountainLevel) {
                return TerrainType.Mountain;
            } else {
                return TerrainType.Land;
            }
        }

        void OnDrawGizmos() {
            if (!showDebugInfo || points == null) return;
            
            // Dessiner les points
            Gizmos.color = Color.yellow;
            foreach (ProceduralPoint point in points) {
                Vector3 worldPos = point.GetWorldPosition(radius);
                Gizmos.DrawWireSphere(worldPos, 0.01f);
            }
            
            // Dessiner les tiles
            Gizmos.color = Color.cyan;
            foreach (ProceduralTile tile in tiles) {
                if (tile.vertices != null && tile.vertices.Length >= 3) {
                    for (int i = 0; i < tile.vertices.Length; i++) {
                        Vector3 start = tile.vertices[i].GetWorldPosition(radius);
                        Vector3 end = tile.vertices[(i + 1) % tile.vertices.Length].GetWorldPosition(radius);
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Triangle pour la génération de mesh
    /// </summary>
    public class Triangle {
        public ProceduralPoint p1, p2, p3;
        
        public Triangle(ProceduralPoint p1, ProceduralPoint p2, ProceduralPoint p3) {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }

    /// <summary>
    /// Types de terrain
    /// </summary>
    public enum TerrainType {
        Water,
        Land,
        Mountain
    }
}
