using UnityEngine;
using System.Collections.Generic;
using System;

namespace HexasphereTest {
    
    public class HexaTest : MonoBehaviour {
        
        [Header("🔷 Configuration Hexasphere")]
        [SerializeField] public int divisions = 3;
        [SerializeField] public float radius = 1.0f;
        [SerializeField] public bool generateOnStart = true;
        [SerializeField] public bool showDebugInfo = true;
        [SerializeField] public bool showWireframe = false;
        [SerializeField] public bool fixTriangleOrientation = true;
        [SerializeField] public bool useSphericalUVs = true;
        
        [Header("⚡ Optimisation Hexasphere")]
        [SerializeField] public bool useChunking = true;
        [SerializeField] public int maxVerticesPerChunk = 65500;
        [SerializeField] public int maxChunks = 100;
        [SerializeField] public bool useLOD = false;
        [SerializeField] public float lodDistance = 10f;
        [SerializeField] public Camera lodCamera;
        
        [Header("🎨 Matériaux")]
        [SerializeField] public Material hexagonMaterial;
        [SerializeField] public Color hexagonColor = Color.blue;
        
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private Mesh hexagonMesh;
        
        // Structures Hexasphere
        private Dictionary<Point, Point> points = new Dictionary<Point, Point>();
        private List<Triangle> triangles = new List<Triangle>();
        
        // Système de chunking (comme Hexasphere)
        private List<Vector3>[] verticesChunks;
        private List<int>[] trianglesChunks;
        private List<Vector2>[] uvsChunks;
        private Mesh[] meshChunks;
        private MeshFilter[] meshFilterChunks;
        private MeshRenderer[] meshRendererChunks;
        private int chunkCount = 0;
        
        // Variables LOD
        private int originalDivisions;
        private int currentLODDivisions;
        private float lastLODDistance = -1f;
        
        void Start() {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            
            if (meshRenderer == null) {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
            if (meshFilter == null) {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            
            // Trouver automatiquement la caméra principale si pas assignée
            if (lodCamera == null) {
                lodCamera = Camera.main;
                if (lodCamera == null) {
                    lodCamera = FindObjectOfType<Camera>();
                }
            }
            
            if (generateOnStart) {
                GenerateHexasphere();
            }
        }
        
        void Update() {
            if (Input.GetKeyDown(KeyCode.G)) {
                GenerateHexasphere();
            }
            
            // Mise à jour du LOD si activé
            if (useLOD && lodCamera != null) {
                UpdateLOD();
            }
        }
        
        public void GenerateHexasphere() {
            Debug.Log($"🔷 Génération Hexasphere avec {divisions} divisions");
            
            // Nettoyer les anciens chunks
            CleanupOldChunks();
            
            // Créer l'icosaèdre de base (comme Hexasphere)
            CreateIcosahedron();
            Debug.Log($"🔷 Icosaèdre créé: {points.Count} points, {triangles.Count} triangles");
            
            // Appliquer les subdivisions (comme Hexasphere)
            ApplySubdivisions();
            Debug.Log($"🔷 Après subdivisions: {points.Count} points, {triangles.Count} triangles");
            
            // Générer le mesh final
            GenerateMesh();
            
            Debug.Log($"🔷 Hexasphere généré: {points.Count} points, {triangles.Count} triangles");
        }
        
        public void ResetLOD() {
            // Réinitialiser le LOD
            originalDivisions = 0;
            currentLODDivisions = 0;
            lastLODDistance = -1f;
            Debug.Log("🎯 LOD réinitialisé");
        }
        
        void CreateIcosahedron() {
            // Créer les 12 points de l'icosaèdre (comme Hexasphere)
            Point[] corners = new Point[12];
            
            // Calculer les points de l'icosaèdre
            float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f; // Nombre d'or
            
            corners[0] = new Point(-1, t, 0);
            corners[1] = new Point(1, t, 0);
            corners[2] = new Point(-1, -t, 0);
            corners[3] = new Point(1, -t, 0);
            
            corners[4] = new Point(0, -1, t);
            corners[5] = new Point(0, 1, t);
            corners[6] = new Point(0, -1, -t);
            corners[7] = new Point(0, 1, -t);
            
            corners[8] = new Point(t, 0, -1);
            corners[9] = new Point(t, 0, 1);
            corners[10] = new Point(-t, 0, -1);
            corners[11] = new Point(-t, 0, 1);
            
            // Normaliser tous les points
            for (int i = 0; i < corners.Length; i++) {
                corners[i] = corners[i].Normalized;
            }
            
            // Créer les 20 triangles de l'icosaèdre (comme Hexasphere)
            Triangle[] baseTriangles = new Triangle[] {
                new Triangle(corners[0], corners[11], corners[5], false),
                new Triangle(corners[0], corners[5], corners[1], false),
                new Triangle(corners[0], corners[1], corners[7], false),
                new Triangle(corners[0], corners[7], corners[10], false),
                new Triangle(corners[0], corners[10], corners[11], false),
                
                new Triangle(corners[1], corners[5], corners[9], false),
                new Triangle(corners[5], corners[11], corners[4], false),
                new Triangle(corners[11], corners[10], corners[2], false),
                new Triangle(corners[10], corners[7], corners[6], false),
                new Triangle(corners[7], corners[1], corners[8], false),
                
                new Triangle(corners[3], corners[9], corners[4], false),
                new Triangle(corners[3], corners[4], corners[2], false),
                new Triangle(corners[3], corners[2], corners[6], false),
                new Triangle(corners[3], corners[6], corners[8], false),
                new Triangle(corners[3], corners[8], corners[9], false),
                
                new Triangle(corners[4], corners[9], corners[5], false),
                new Triangle(corners[2], corners[4], corners[11], false),
                new Triangle(corners[6], corners[2], corners[10], false),
                new Triangle(corners[8], corners[6], corners[7], false),
                new Triangle(corners[9], corners[8], corners[1], false)
            };
            
            // Initialiser les points et triangles
            points.Clear();
            triangles.Clear();
            
            for (int i = 0; i < corners.Length; i++) {
                points[corners[i]] = corners[i];
            }
            
            for (int i = 0; i < baseTriangles.Length; i++) {
                triangles.Add(baseTriangles[i]);
            }
        }
        
        void ApplySubdivisions() {
            Debug.Log($"🔷 Application de {divisions} subdivisions");
            
            // Créer une nouvelle liste de triangles pour les subdivisions
            List<Triangle> newTriangles = new List<Triangle>();
            List<Point> bottom = new List<Point>();
            
            // Traiter chaque triangle de base
            for (int f = 0; f < triangles.Count; f++) {
                List<Point> prev = null;
                Point point0 = triangles[f].points[0];
                bottom.Clear();
                bottom.Add(point0);
                
                // Subdiviser les arêtes (comme Hexasphere)
                List<Point> left = point0.Subdivide(triangles[f].points[1], divisions, GetCachedPoint);
                List<Point> right = point0.Subdivide(triangles[f].points[2], divisions, GetCachedPoint);
                
                // Créer les triangles imbriqués (comme Hexasphere)
                for (int i = 1; i <= divisions; i++) {
                    prev = bottom;
                    bottom = left[i].Subdivide(right[i], i, GetCachedPoint);
                    
                    // S'assurer que tous les points sont dans le cache
                    foreach (var point in bottom) {
                        AddPointToCache(point);
                    }
                    
                    // Créer le triangle de base (comme Hexasphere)
                    Triangle baseTriangle = new Triangle(prev[0], bottom[0], bottom[1]);
                    newTriangles.Add(baseTriangle);
                    
                    // Créer les triangles imbriqués (comme Hexasphere)
                    for (int j = 1; j < i; j++) {
                        Triangle tri1 = new Triangle(prev[j], bottom[j], bottom[j + 1]);
                        Triangle tri2 = new Triangle(prev[j - 1], prev[j], bottom[j]);
                        newTriangles.Add(tri1);
                        newTriangles.Add(tri2);
                    }
                }
            }
            
            // Remplacer les anciens triangles par les nouveaux
            triangles.Clear();
            triangles.AddRange(newTriangles);
            
            Debug.Log($"🔷 Subdivisions appliquées: {triangles.Count} triangles créés");
        }
        
        void GenerateMesh() {
            if (useChunking) {
                GenerateMeshWithChunking();
            } else {
                GenerateMeshSingle();
            }
        }
        
        void GenerateMeshSingle() {
            // Créer le mesh à partir des points et triangles
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Créer un dictionnaire pour mapper les points vers les indices de vertices
            Dictionary<Point, int> pointToIndex = new Dictionary<Point, int>();
            int vertexIndex = 0;
            
            // Ajouter tous les points comme vertices
            foreach (var point in points.Values) {
                vertices.Add(point.ToVector3() * radius);
                
                // Calculer les UVs sphériques
                if (useSphericalUVs) {
                    Vector3 pos = point.ToVector3();
                    float u = 0.5f + Mathf.Atan2(pos.z, pos.x) / (2f * Mathf.PI);
                    float v = 0.5f - Mathf.Asin(pos.y) / Mathf.PI;
                    uvs.Add(new Vector2(u, v));
                } else {
                    uvs.Add(new Vector2(0.5f, 0.5f)); // UV simple
                }
                
                pointToIndex[point] = vertexIndex++;
            }
            
            // Créer les triangles avec orientation correcte
            foreach (var triangle in this.triangles) {
                if (pointToIndex.ContainsKey(triangle.points[0]) &&
                    pointToIndex.ContainsKey(triangle.points[1]) &&
                    pointToIndex.ContainsKey(triangle.points[2])) {
                    
                    if (fixTriangleOrientation) {
                        // Vérifier l'orientation du triangle
                        Vector3 v0 = vertices[pointToIndex[triangle.points[0]]];
                        Vector3 v1 = vertices[pointToIndex[triangle.points[1]]];
                        Vector3 v2 = vertices[pointToIndex[triangle.points[2]]];
                        
                        // Calculer la normale pour vérifier l'orientation
                        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                        Vector3 center = (v0 + v1 + v2) / 3f;
                        
                        // Si la normale pointe vers l'intérieur, inverser l'ordre
                        if (Vector3.Dot(normal, center) < 0) {
                            // Triangle inversé - inverser l'ordre des vertices
                            triangles.Add(pointToIndex[triangle.points[0]]);
                            triangles.Add(pointToIndex[triangle.points[2]]);
                            triangles.Add(pointToIndex[triangle.points[1]]);
                        } else {
                            // Triangle correct - ordre normal
                            triangles.Add(pointToIndex[triangle.points[0]]);
                            triangles.Add(pointToIndex[triangle.points[1]]);
                            triangles.Add(pointToIndex[triangle.points[2]]);
                        }
                    } else {
                        // Ordre normal sans vérification
                        triangles.Add(pointToIndex[triangle.points[0]]);
                        triangles.Add(pointToIndex[triangle.points[1]]);
                        triangles.Add(pointToIndex[triangle.points[2]]);
                    }
                }
            }
            
            // Créer le mesh
            hexagonMesh = new Mesh();
            hexagonMesh.name = "Hexasphere Mesh";
            hexagonMesh.vertices = vertices.ToArray();
            hexagonMesh.triangles = triangles.ToArray();
            hexagonMesh.uv = uvs.ToArray();
            hexagonMesh.RecalculateNormals();
            hexagonMesh.RecalculateBounds();
            
            // Assigner le mesh
            meshFilter.mesh = hexagonMesh;
            
            // Configurer le matériau
            if (hexagonMaterial == null) {
                hexagonMaterial = new Material(Shader.Find("Standard"));
                hexagonMaterial.color = hexagonColor;
            }
            meshRenderer.material = hexagonMaterial;
            
            Debug.Log($"🔷 Mesh généré: {vertices.Count} vertices, {triangles.Count/3} triangles");
            
            // Déboguer la structure des hexagones
            if (showDebugInfo) {
                DebugHexagonStructure();
            }
        }
        
        void GenerateMeshWithChunking() {
            Debug.Log($"⚡ Génération avec chunking (max {maxVerticesPerChunk} vertices par chunk)");
            
            // Initialiser les chunks
            InitializeChunks();
            
            // Créer un dictionnaire pour mapper les points vers les indices de vertices
            Dictionary<Point, int> pointToIndex = new Dictionary<Point, int>();
            int vertexIndex = 0;
            int currentChunk = 0;
            
            // Ajouter tous les points comme vertices
            foreach (var point in points.Values) {
                // Vérifier si on doit passer au chunk suivant
                if (vertexIndex >= maxVerticesPerChunk) {
                    currentChunk++;
                    vertexIndex = 0;
                    if (currentChunk >= maxChunks) {
                        Debug.LogWarning($"⚠️ Limite de chunks atteinte ({maxChunks})");
                        break;
                    }
                }
                
                verticesChunks[currentChunk].Add(point.ToVector3() * radius);
                
                // Calculer les UVs sphériques
                if (useSphericalUVs) {
                    Vector3 pos = point.ToVector3();
                    float u = 0.5f + Mathf.Atan2(pos.z, pos.x) / (2f * Mathf.PI);
                    float v = 0.5f - Mathf.Asin(pos.y) / Mathf.PI;
                    uvsChunks[currentChunk].Add(new Vector2(u, v));
                } else {
                    uvsChunks[currentChunk].Add(new Vector2(0.5f, 0.5f));
                }
                
                pointToIndex[point] = vertexIndex++;
            }
            
            // Créer les triangles avec orientation correcte
            foreach (var triangle in this.triangles) {
                if (pointToIndex.ContainsKey(triangle.points[0]) &&
                    pointToIndex.ContainsKey(triangle.points[1]) &&
                    pointToIndex.ContainsKey(triangle.points[2])) {
                    
                    if (fixTriangleOrientation) {
                        // Vérifier l'orientation du triangle
                        Vector3 v0 = verticesChunks[currentChunk][pointToIndex[triangle.points[0]]];
                        Vector3 v1 = verticesChunks[currentChunk][pointToIndex[triangle.points[1]]];
                        Vector3 v2 = verticesChunks[currentChunk][pointToIndex[triangle.points[2]]];
                        
                        // Calculer la normale pour vérifier l'orientation
                        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                        Vector3 center = (v0 + v1 + v2) / 3f;
                        
                        // Si la normale pointe vers l'intérieur, inverser l'ordre
                        if (Vector3.Dot(normal, center) < 0) {
                            // Triangle inversé - inverser l'ordre des vertices
                            trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[0]]);
                            trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[2]]);
                            trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[1]]);
                        } else {
                            // Triangle correct - ordre normal
                            trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[0]]);
                            trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[1]]);
                            trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[2]]);
                        }
                    } else {
                        // Ordre normal sans vérification
                        trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[0]]);
                        trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[1]]);
                        trianglesChunks[currentChunk].Add(pointToIndex[triangle.points[2]]);
                    }
                }
            }
            
            // Créer les meshes pour chaque chunk
            CreateChunkMeshes();
            
            Debug.Log($"⚡ Chunking terminé: {chunkCount} chunks créés");
            
            // Déboguer la structure des hexagones
            if (showDebugInfo) {
                DebugHexagonStructure();
            }
        }
        
        void DebugHexagonStructure() {
            Debug.Log($"🔷 Structure des hexagones:");
            Debug.Log($"🔷 Points: {points.Count}");
            Debug.Log($"🔷 Triangles: {triangles.Count}");
            
            // Analyser la distribution des triangles
            int triangleCount = triangles.Count;
            int expectedHexagons = triangleCount / 6; // Chaque hexagone = 6 triangles
            Debug.Log($"🔷 Hexagones attendus: {expectedHexagons}");
            
            // Vérifier la connectivité
            Dictionary<Point, List<Triangle>> pointToTriangles = new Dictionary<Point, List<Triangle>>();
            foreach (var triangle in triangles) {
                foreach (var point in triangle.points) {
                    if (!pointToTriangles.ContainsKey(point)) {
                        pointToTriangles[point] = new List<Triangle>();
                    }
                    pointToTriangles[point].Add(triangle);
                }
            }
            
            Debug.Log($"🔷 Points avec triangles: {pointToTriangles.Count}");
            
            // Analyser les points avec 6 triangles (hexagones)
            int hexagonPoints = 0;
            foreach (var kvp in pointToTriangles) {
                if (kvp.Value.Count == 6) {
                    hexagonPoints++;
                }
            }
            
            Debug.Log($"🔷 Points hexagones (6 triangles): {hexagonPoints}");
        }
        
        void InitializeChunks() {
            // Initialiser les arrays de chunks
            verticesChunks = new List<Vector3>[maxChunks];
            trianglesChunks = new List<int>[maxChunks];
            uvsChunks = new List<Vector2>[maxChunks];
            meshChunks = new Mesh[maxChunks];
            meshFilterChunks = new MeshFilter[maxChunks];
            meshRendererChunks = new MeshRenderer[maxChunks];
            
            for (int i = 0; i < maxChunks; i++) {
                verticesChunks[i] = new List<Vector3>();
                trianglesChunks[i] = new List<int>();
                uvsChunks[i] = new List<Vector2>();
            }
            
            chunkCount = 0;
        }
        
        void CreateChunkMeshes() {
            // Créer les meshes pour chaque chunk
            for (int i = 0; i < maxChunks; i++) {
                if (verticesChunks[i].Count == 0) break;
                
                // Créer le mesh pour ce chunk
                meshChunks[i] = new Mesh();
                meshChunks[i].name = $"Hexasphere Chunk {i}";
                meshChunks[i].vertices = verticesChunks[i].ToArray();
                meshChunks[i].triangles = trianglesChunks[i].ToArray();
                meshChunks[i].uv = uvsChunks[i].ToArray();
                meshChunks[i].RecalculateNormals();
                meshChunks[i].RecalculateBounds();
                
                // Créer les composants pour ce chunk
                GameObject chunkObject = new GameObject($"Hexasphere Chunk {i}");
                chunkObject.transform.SetParent(transform);
                
                meshFilterChunks[i] = chunkObject.AddComponent<MeshFilter>();
                meshRendererChunks[i] = chunkObject.AddComponent<MeshRenderer>();
                
                meshFilterChunks[i].mesh = meshChunks[i];
                
                // Configurer le matériau
                if (hexagonMaterial == null) {
                    hexagonMaterial = new Material(Shader.Find("Standard"));
                    hexagonMaterial.color = hexagonColor;
                }
                meshRendererChunks[i].material = hexagonMaterial;
                
                chunkCount++;
                
                Debug.Log($"⚡ Chunk {i} créé: {verticesChunks[i].Count} vertices, {trianglesChunks[i].Count/3} triangles");
            }
        }
        
        void CleanupOldChunks() {
            // Nettoyer les anciens chunks
            if (meshFilterChunks != null) {
                for (int i = 0; i < meshFilterChunks.Length; i++) {
                    if (meshFilterChunks[i] != null) {
                        if (meshFilterChunks[i].gameObject != null) {
                            DestroyImmediate(meshFilterChunks[i].gameObject);
                        }
                    }
                }
            }
            
            // Nettoyer le mesh principal
            if (meshFilter != null && meshFilter.mesh != null) {
                meshFilter.mesh = null;
            }
            
            // Réinitialiser les variables
            chunkCount = 0;
            if (verticesChunks != null) {
                for (int i = 0; i < verticesChunks.Length; i++) {
                    if (verticesChunks[i] != null) {
                        verticesChunks[i].Clear();
                    }
                    if (trianglesChunks != null && trianglesChunks[i] != null) {
                        trianglesChunks[i].Clear();
                    }
                    if (uvsChunks != null && uvsChunks[i] != null) {
                        uvsChunks[i].Clear();
                    }
                }
            }
            
            Debug.Log("🧹 Anciens chunks nettoyés");
        }
        
        void UpdateLOD() {
            if (lodCamera == null) return;
            
            // Calculer la distance entre la caméra et l'objet
            float distance = Vector3.Distance(lodCamera.transform.position, transform.position);
            
            // Sauvegarder les divisions originales au premier appel
            if (originalDivisions == 0) {
                originalDivisions = divisions;
                currentLODDivisions = divisions;
            }
            
            // Déterminer le niveau de LOD basé sur la distance
            int targetDivisions = originalDivisions;
            if (distance > lodDistance * 2) {
                targetDivisions = Mathf.Max(1, originalDivisions / 4); // LOD très bas
            } else if (distance > lodDistance) {
                targetDivisions = Mathf.Max(1, originalDivisions / 2); // LOD bas
            }
            // Si distance <= lodDistance, garder les divisions originales (détail maximum)
            
            // Vérifier si le LOD a changé significativement
            bool lodChanged = false;
            if (Mathf.Abs(distance - lastLODDistance) > lodDistance * 0.1f) { // Seuil de 10% pour éviter les changements trop fréquents
                lodChanged = true;
                lastLODDistance = distance;
            }
            
            // Si le niveau de LOD a changé, régénérer
            if (lodChanged && targetDivisions != currentLODDivisions) {
                currentLODDivisions = targetDivisions;
                divisions = targetDivisions;
                GenerateHexasphere();
                Debug.Log($"🎯 LOD mis à jour: Distance={distance:F1}, Divisions={targetDivisions}");
            }
        }
        
        // Méthode pour obtenir un point caché (comme Hexasphere)
        Point GetCachedPoint(Point point) {
            if (points.ContainsKey(point)) {
                return points[point];
            }
            points[point] = point;
            return point;
        }
        
        // Méthode pour ajouter un point au cache
        void AddPointToCache(Point point) {
            if (!points.ContainsKey(point)) {
                points[point] = point;
            }
        }
        
        void OnGUI() {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 250));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🔷 HexaTest - Hexasphere");
            GUILayout.Label($"Divisions: {divisions}");
            GUILayout.Label($"Points: {points.Count}");
            GUILayout.Label($"Triangles: {triangles.Count}");
            GUILayout.Label($"Radius: {radius:F2}");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🔷 Générer Hexasphere")) {
                GenerateHexasphere();
            }
            
            if (GUILayout.Button("🔷 Augmenter Divisions")) {
                divisions = Mathf.Min(divisions + 1, 10);
                ResetLOD(); // Réinitialiser le LOD
                GenerateHexasphere();
            }
            
            if (GUILayout.Button("🔷 Réduire Divisions")) {
                divisions = Mathf.Max(divisions - 1, 0);
                ResetLOD(); // Réinitialiser le LOD
                GenerateHexasphere();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔷 Toggle Wireframe")) {
                showWireframe = !showWireframe;
                if (meshRenderer != null) {
                    meshRenderer.material = showWireframe ? 
                        new Material(Shader.Find("Unlit/Color")) : hexagonMaterial;
                }
            }
            
            GUILayout.Label($"Wireframe: {(showWireframe ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔷 Toggle Orientation Fix")) {
                fixTriangleOrientation = !fixTriangleOrientation;
                GenerateHexasphere();
            }
            
            if (GUILayout.Button("🔷 Toggle Spherical UVs")) {
                useSphericalUVs = !useSphericalUVs;
                GenerateHexasphere();
            }
            
            GUILayout.Label($"Orientation Fix: {(fixTriangleOrientation ? "ON" : "OFF")}");
            GUILayout.Label($"Spherical UVs: {(useSphericalUVs ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("⚡ Toggle Chunking")) {
                useChunking = !useChunking;
                GenerateHexasphere();
            }
            
            if (GUILayout.Button("⚡ Toggle LOD")) {
                useLOD = !useLOD;
                if (useLOD) {
                    ResetLOD(); // Réinitialiser le LOD quand on l'active
                }
                GenerateHexasphere();
            }
            
            if (GUILayout.Button("🎯 Reset LOD")) {
                ResetLOD();
                GenerateHexasphere();
            }
            
            GUILayout.Label($"Chunking: {(useChunking ? "ON" : "OFF")}");
            GUILayout.Label($"LOD: {(useLOD ? "ON" : "OFF")}");
            GUILayout.Label($"Chunks: {chunkCount}");
            
            if (useLOD && lodCamera != null) {
                float distance = Vector3.Distance(lodCamera.transform.position, transform.position);
                GUILayout.Label($"Distance: {distance:F1}");
                GUILayout.Label($"LOD Distance: {lodDistance:F1}");
                GUILayout.Label($"Original Divisions: {originalDivisions}");
                GUILayout.Label($"Current LOD: {currentLODDivisions}");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
    
    // Classe Point (simplifiée d'Hexasphere)
    public class Point : IEqualityComparer<Point>, IEquatable<Point> {
        public float x, y, z;
        
        public Point(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        public Point Normalized {
            get {
                float length = Mathf.Sqrt(x * x + y * y + z * z);
                if (length == 0) return new Point(0, 0, 0);
                return new Point(x / length, y / length, z / length);
            }
        }
        
        public Vector3 ToVector3() {
            return new Vector3(x, y, z);
        }
        
        // Méthode Subdivide (comme Hexasphere)
        public List<Point> Subdivide(Point point, int count, System.Func<Point, Point> getCachedPoint) {
            List<Point> segments = new List<Point>(count + 1);
            segments.Add(this);
            
            for (int i = 1; i < count; i++) {
                float t = (float)i / count;
                Point newPoint = new Point(
                    x + (point.x - x) * t,
                    y + (point.y - y) * t,
                    z + (point.z - z) * t
                );
                newPoint = newPoint.Normalized;
                newPoint = getCachedPoint(newPoint);
                segments.Add(newPoint);
            }
            
            segments.Add(point);
            return segments;
        }
        
        // Implémentation des interfaces
        public bool Equals(Point other) {
            if (other == null) return false;
            return Mathf.Abs(x - other.x) < 0.0001f &&
                   Mathf.Abs(y - other.y) < 0.0001f &&
                   Mathf.Abs(z - other.z) < 0.0001f;
        }
        
        public bool Equals(Point x, Point y) {
            return x.Equals(y);
        }
        
        public int GetHashCode(Point obj) {
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() ^ obj.z.GetHashCode();
        }
        
        public override int GetHashCode() {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
    }
    
    // Classe Triangle (simplifiée d'Hexasphere)
    public class Triangle {
        public Point[] points;
        
        public Triangle(Point point1, Point point2, Point point3) : this(point1, point2, point3, true) {
        }
        
        public Triangle(Point point1, Point point2, Point point3, bool register) {
            this.points = new Point[] { point1, point2, point3 };
        }
    }
}
