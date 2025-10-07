using UnityEngine;
using System.Collections.Generic;
using System;

public class HexasphereFill : MonoBehaviour {
    [Header("🔷 Hexasphere Settings")]
    [SerializeField] public int divisions = 3;
    [SerializeField] public float radius = 1f;
    [SerializeField] public bool generateOnStart = true;
    [SerializeField] public bool showDebugInfo = true;
    [SerializeField] public bool showWireframe = false;
    
    [Header("🎨 Materials")]
    [SerializeField] public Material hexagonMaterial;
    [SerializeField] public Color hexagonColor = Color.blue;
    [SerializeField] public bool fixTriangleOrientation = true;
    [SerializeField] public bool useSphericalUVs = true;
    
    [Header("⚡ Performance")]
    [SerializeField] public bool useChunking = false;
    [SerializeField] public int maxVerticesPerChunk = 65000;
    [SerializeField] public int maxChunks = 100;
    
    [Header("🎯 Subdivision par Zones")]
    [SerializeField] public bool useSelectiveSubdivision = false;
    [SerializeField] public Vector3 focusPoint = Vector3.forward; // Point de focus pour la subdivision
    [SerializeField] public float focusRadius = 0.5f; // Rayon de la zone de focus
    [SerializeField] public int focusDivisions = 5; // Divisions dans la zone de focus
    [SerializeField] public int backgroundDivisions = 1; // Divisions en arrière-plan
    [SerializeField] public bool showFocusDebug = false; // Afficher la zone de focus
    [SerializeField] public bool showFocusPoint = true; // Afficher le point de focus
    [SerializeField] public Color focusPointColor = Color.red; // Couleur du point de focus
    [SerializeField] public float focusPointSize = 0.1f; // Taille du point de focus
    
    // Variables internes
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh hexagonMesh;
    private Dictionary<Point, Point> points = new Dictionary<Point, Point>();
    private List<Triangle> triangles = new List<Triangle>();
    
    // Variables pour le chunking
    private List<Vector3>[] verticesChunks;
    private List<int>[] trianglesChunks;
    private List<Vector2>[] uvsChunks;
    private Mesh[] meshChunks;
    private MeshFilter[] meshFilterChunks;
    private MeshRenderer[] meshRendererChunks;
    private int chunkCount = 0;
    
    // Variables pour le point de focus
    private GameObject focusPointObject;
    private MeshRenderer focusPointRenderer;
    
    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        
        if (meshRenderer == null) {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        // Ajouter le tag "Planet" à l'objet
        if (gameObject.tag != "Planet") {
            gameObject.tag = "Planet";
        }
        
        if (generateOnStart) {
            GenerateHexasphere();
        }
        
        // Créer le point de focus visible
        CreateFocusPoint();
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            GenerateHexasphere();
        }
        
        // Mettre à jour le point de focus
        UpdateFocusPoint();
    }
    
    public void GenerateHexasphere() {
        Debug.Log($"🔷 Génération Hexasphere avec {divisions} divisions");
        
        // Nettoyer les anciens chunks
        CleanupOldChunks();
        
        // Créer l'icosaèdre de base
        CreateIcosahedron();
        
        // Appliquer les subdivisions
        ApplySubdivisions();
        
        // Générer le mesh
        if (useChunking) {
            GenerateMeshWithChunking();
        } else {
            GenerateMeshSingle();
        }
        
        Debug.Log($"🔷 Hexasphere généré: {points.Count} points, {triangles.Count} triangles");
    }
    
    void CreateIcosahedron() {
        points.Clear();
        triangles.Clear();
        
        // Créer les 12 points de l'icosaèdre
        float t = (1f + Mathf.Sqrt(5f)) / 2f; // Nombre d'or
        
        Point[] vertices = new Point[12];
        vertices[0] = new Point(-1, t, 0);
        vertices[1] = new Point(1, t, 0);
        vertices[2] = new Point(-1, -t, 0);
        vertices[3] = new Point(1, -t, 0);
        vertices[4] = new Point(0, -1, t);
        vertices[5] = new Point(0, 1, t);
        vertices[6] = new Point(0, -1, -t);
        vertices[7] = new Point(0, 1, -t);
        vertices[8] = new Point(t, 0, -1);
        vertices[9] = new Point(t, 0, 1);
        vertices[10] = new Point(-t, 0, -1);
        vertices[11] = new Point(-t, 0, 1);
        
        // Normaliser et ajouter les points
        for (int i = 0; i < 12; i++) {
            vertices[i] = vertices[i].Normalized;
            points[vertices[i]] = vertices[i];
        }
        
        // Créer les 20 triangles de l'icosaèdre
        int[] indices = {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };
        
        for (int i = 0; i < indices.Length; i += 3) {
            triangles.Add(new Triangle(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            ));
        }
        
        Debug.Log($"🔷 Icosaèdre créé: {points.Count} points, {triangles.Count} triangles");
    }
    
    void ApplySubdivisions() {
        if (useSelectiveSubdivision) {
            ApplySelectiveSubdivisions();
        } else {
            ApplyUniformSubdivisions();
        }
        
        // Normaliser tous les points pour s'assurer qu'ils sont sur la sphère
        NormalizeAllPoints();
    }
    
    void ApplyUniformSubdivisions() {
        for (int i = 0; i < divisions; i++) {
            List<Triangle> newTriangles = new List<Triangle>();
            
            foreach (var triangle in triangles) {
                // Diviser chaque triangle en 4
                Point mid1 = GetCachedPoint(Point.Midpoint(triangle.points[0], triangle.points[1]).Normalized);
                Point mid2 = GetCachedPoint(Point.Midpoint(triangle.points[1], triangle.points[2]).Normalized);
                Point mid3 = GetCachedPoint(Point.Midpoint(triangle.points[2], triangle.points[0]).Normalized);
                
                // Créer 4 nouveaux triangles
                newTriangles.Add(new Triangle(triangle.points[0], mid1, mid3));
                newTriangles.Add(new Triangle(triangle.points[1], mid2, mid1));
                newTriangles.Add(new Triangle(triangle.points[2], mid3, mid2));
                newTriangles.Add(new Triangle(mid1, mid2, mid3));
            }
            
            triangles = newTriangles;
            Debug.Log($"🔷 Subdivision uniforme {i + 1}: {points.Count} points, {triangles.Count} triangles");
        }
    }
    
    void ApplySelectiveSubdivisions() {
        // D'abord, appliquer les subdivisions de base
        for (int i = 0; i < backgroundDivisions; i++) {
            List<Triangle> newTriangles = new List<Triangle>();
            
            foreach (var triangle in triangles) {
                Point mid1 = GetCachedPoint(Point.Midpoint(triangle.points[0], triangle.points[1]).Normalized);
                Point mid2 = GetCachedPoint(Point.Midpoint(triangle.points[1], triangle.points[2]).Normalized);
                Point mid3 = GetCachedPoint(Point.Midpoint(triangle.points[2], triangle.points[0]).Normalized);
                
                newTriangles.Add(new Triangle(triangle.points[0], mid1, mid3));
                newTriangles.Add(new Triangle(triangle.points[1], mid2, mid1));
                newTriangles.Add(new Triangle(triangle.points[2], mid3, mid2));
                newTriangles.Add(new Triangle(mid1, mid2, mid3));
            }
            
            triangles = newTriangles;
        }
        
        Debug.Log($"🔷 Subdivision de base: {triangles.Count} triangles");
        
        // Ensuite, appliquer les subdivisions supplémentaires dans la zone de focus
        for (int i = 0; i < focusDivisions - backgroundDivisions; i++) {
            List<Triangle> newTriangles = new List<Triangle>();
            
            foreach (var triangle in triangles) {
                // Vérifier si le triangle est dans la zone de focus
                if (IsTriangleInFocus(triangle)) {
                    // Subdiviser le triangle dans la zone de focus
                    Point mid1 = GetCachedPoint(Point.Midpoint(triangle.points[0], triangle.points[1]).Normalized);
                    Point mid2 = GetCachedPoint(Point.Midpoint(triangle.points[1], triangle.points[2]).Normalized);
                    Point mid3 = GetCachedPoint(Point.Midpoint(triangle.points[2], triangle.points[0]).Normalized);
                    
                    newTriangles.Add(new Triangle(triangle.points[0], mid1, mid3));
                    newTriangles.Add(new Triangle(triangle.points[1], mid2, mid1));
                    newTriangles.Add(new Triangle(triangle.points[2], mid3, mid2));
                    newTriangles.Add(new Triangle(mid1, mid2, mid3));
                } else {
                    // Garder le triangle tel quel en dehors de la zone de focus
                    newTriangles.Add(triangle);
                }
            }
            
            triangles = newTriangles;
            Debug.Log($"🔷 Subdivision focus {i + 1}: {triangles.Count} triangles");
        }
    }
    
    bool IsTriangleInFocus(Triangle triangle) {
        // Calculer le centre du triangle
        Vector3 center = (triangle.points[0].ToVector3() + triangle.points[1].ToVector3() + triangle.points[2].ToVector3()) / 3f;
        
        // Vérifier si le centre est dans la zone de focus
        float distance = Vector3.Distance(center, focusPoint);
        return distance <= focusRadius;
    }
    
    void NormalizeAllPoints() {
        // Créer une nouvelle liste de points normalisés
        Dictionary<Point, Point> normalizedPoints = new Dictionary<Point, Point>();
        
        foreach (var kvp in points) {
            Point normalized = kvp.Value.Normalized;
            normalizedPoints[normalized] = normalized;
        }
        
        points = normalizedPoints;
        
        // Mettre à jour les triangles avec les nouveaux points normalisés
        for (int i = 0; i < triangles.Count; i++) {
            Triangle triangle = triangles[i];
            Point[] newPoints = new Point[3];
            
            for (int j = 0; j < 3; j++) {
                Point normalized = triangle.points[j].Normalized;
                if (points.ContainsKey(normalized)) {
                    newPoints[j] = points[normalized];
                } else {
                    newPoints[j] = normalized;
                    points[normalized] = normalized;
                }
            }
            
            triangles[i] = new Triangle(newPoints[0], newPoints[1], newPoints[2]);
        }
        
        Debug.Log($"🔷 Points normalisés: {points.Count} points sur la sphère");
    }
    
    void CreateFocusPoint() {
        if (focusPointObject == null) {
            // Créer le GameObject du point de focus
            focusPointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            focusPointObject.name = "Focus Point";
            focusPointObject.transform.SetParent(transform);
            
            // Supprimer le collider
            Collider collider = focusPointObject.GetComponent<Collider>();
            if (collider != null) {
                DestroyImmediate(collider);
            }
            
            // Configurer le renderer
            focusPointRenderer = focusPointObject.GetComponent<MeshRenderer>();
            if (focusPointRenderer != null) {
                Material focusMaterial = new Material(Shader.Find("Standard"));
                focusMaterial.color = focusPointColor;
                focusMaterial.SetFloat("_Mode", 3); // Mode transparent
                focusMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                focusMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                focusMaterial.SetInt("_ZWrite", 0);
                focusMaterial.DisableKeyword("_ALPHATEST_ON");
                focusMaterial.EnableKeyword("_ALPHABLEND_ON");
                focusMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                focusMaterial.renderQueue = 3000;
                focusPointRenderer.material = focusMaterial;
            }
        }
        
        UpdateFocusPoint();
    }
    
    void UpdateFocusPoint() {
        if (focusPointObject != null) {
            // Positionner le point de focus
            Vector3 worldPosition = transform.TransformPoint(focusPoint * radius);
            focusPointObject.transform.position = worldPosition;
            focusPointObject.transform.localScale = Vector3.one * focusPointSize;
            
            // Afficher/masquer selon les paramètres
            focusPointObject.SetActive(showFocusPoint && useSelectiveSubdivision);
            
            // Mettre à jour la couleur
            if (focusPointRenderer != null) {
                focusPointRenderer.material.color = focusPointColor;
            }
        }
    }
    
    void GenerateMeshSingle() {
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
                uvs.Add(new Vector2(0.5f, 0.5f));
            }
            
            pointToIndex[point] = vertexIndex++;
        }
        
        // Créer les triangles
        foreach (var triangle in this.triangles) {
            if (pointToIndex.ContainsKey(triangle.points[0]) &&
                pointToIndex.ContainsKey(triangle.points[1]) &&
                pointToIndex.ContainsKey(triangle.points[2])) {
                
                if (fixTriangleOrientation) {
                    // Vérifier l'orientation du triangle
                    Vector3 v0 = vertices[pointToIndex[triangle.points[0]]];
                    Vector3 v1 = vertices[pointToIndex[triangle.points[1]]];
                    Vector3 v2 = vertices[pointToIndex[triangle.points[2]]];
                    
                    Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                    Vector3 center = (v0 + v1 + v2) / 3f;
                    
                    if (Vector3.Dot(normal, center) < 0) {
                        triangles.Add(pointToIndex[triangle.points[0]]);
                        triangles.Add(pointToIndex[triangle.points[2]]);
                        triangles.Add(pointToIndex[triangle.points[1]]);
                    } else {
                        triangles.Add(pointToIndex[triangle.points[0]]);
                        triangles.Add(pointToIndex[triangle.points[1]]);
                        triangles.Add(pointToIndex[triangle.points[2]]);
                    }
                } else {
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
        
        // Configurer le wireframe si demandé
        if (showWireframe) {
            Material wireframeMaterial = new Material(Shader.Find("Unlit/Color"));
            wireframeMaterial.color = Color.white;
            meshRenderer.material = wireframeMaterial;
        }
        
        Debug.Log($"🔷 Mesh généré: {vertices.Count} vertices, {triangles.Count/3} triangles");
    }
    
    void GenerateMeshWithChunking() {
        // Initialiser les chunks
        InitializeChunks();
        
        // Créer un dictionnaire pour mapper les points vers les indices de vertices
        Dictionary<Point, int> pointToIndex = new Dictionary<Point, int>();
        int vertexIndex = 0;
        
        // Ajouter tous les points comme vertices
        foreach (var point in points.Values) {
            verticesChunks[0].Add(point.ToVector3() * radius);
            
            if (useSphericalUVs) {
                Vector3 pos = point.ToVector3();
                float u = 0.5f + Mathf.Atan2(pos.z, pos.x) / (2f * Mathf.PI);
                float v = 0.5f - Mathf.Asin(pos.y) / Mathf.PI;
                uvsChunks[0].Add(new Vector2(u, v));
            } else {
                uvsChunks[0].Add(new Vector2(0.5f, 0.5f));
            }
            
            pointToIndex[point] = vertexIndex++;
        }
        
        // Ajouter les triangles au premier chunk
        foreach (var triangle in this.triangles) {
            if (pointToIndex.ContainsKey(triangle.points[0]) &&
                pointToIndex.ContainsKey(triangle.points[1]) &&
                pointToIndex.ContainsKey(triangle.points[2])) {
                
                if (fixTriangleOrientation) {
                    // Vérifier l'orientation du triangle
                    Vector3 v0 = verticesChunks[0][pointToIndex[triangle.points[0]]];
                    Vector3 v1 = verticesChunks[0][pointToIndex[triangle.points[1]]];
                    Vector3 v2 = verticesChunks[0][pointToIndex[triangle.points[2]]];
                    
                    Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                    Vector3 center = (v0 + v1 + v2) / 3f;
                    
                    if (Vector3.Dot(normal, center) < 0) {
                        trianglesChunks[0].Add(pointToIndex[triangle.points[0]]);
                        trianglesChunks[0].Add(pointToIndex[triangle.points[2]]);
                        trianglesChunks[0].Add(pointToIndex[triangle.points[1]]);
                    } else {
                        trianglesChunks[0].Add(pointToIndex[triangle.points[0]]);
                        trianglesChunks[0].Add(pointToIndex[triangle.points[1]]);
                        trianglesChunks[0].Add(pointToIndex[triangle.points[2]]);
                    }
                } else {
                    trianglesChunks[0].Add(pointToIndex[triangle.points[0]]);
                    trianglesChunks[0].Add(pointToIndex[triangle.points[1]]);
                    trianglesChunks[0].Add(pointToIndex[triangle.points[2]]);
                }
            }
        }
        
        // Créer les meshes des chunks
        CreateChunkMeshes();
        
        Debug.Log($"🔷 Mesh avec chunking généré: {chunkCount} chunks");
    }
    
    void InitializeChunks() {
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
        for (int i = 0; i < maxChunks; i++) {
            if (verticesChunks[i].Count > 0) {
                // Créer le GameObject du chunk
                GameObject chunkObject = new GameObject($"Hexasphere Chunk {i}");
                chunkObject.transform.SetParent(transform);
                
                // Ajouter les composants
                meshFilterChunks[i] = chunkObject.AddComponent<MeshFilter>();
                meshRendererChunks[i] = chunkObject.AddComponent<MeshRenderer>();
                
                // Créer le mesh
                meshChunks[i] = new Mesh();
                meshChunks[i].name = $"Hexasphere Chunk {i} Mesh";
                meshChunks[i].vertices = verticesChunks[i].ToArray();
                meshChunks[i].triangles = trianglesChunks[i].ToArray();
                meshChunks[i].uv = uvsChunks[i].ToArray();
                meshChunks[i].RecalculateNormals();
                meshChunks[i].RecalculateBounds();
                
                // Assigner le mesh
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
        
        // Nettoyer TOUS les enfants (chunks et autres)
        List<Transform> childrenToDestroy = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Hexasphere Chunk") || 
                child.name.Contains("Hexasphere") ||
                child.name.Contains("Chunk") ||
                child.name.Contains("Focus Point")) {
                childrenToDestroy.Add(child);
            }
        }
        
        foreach (Transform child in childrenToDestroy) {
            if (child != null) {
                DestroyImmediate(child.gameObject);
            }
        }
        
        // Réinitialiser le point de focus
        focusPointObject = null;
        focusPointRenderer = null;
        
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
    
    Point GetCachedPoint(Point point) {
        if (points.ContainsKey(point)) {
            return points[point];
        }
        points[point] = point;
        return point;
    }
    
    void OnGUI() {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("🔷 HexasphereFill - 3D Hexasphere");
        GUILayout.Label($"Divisions: {divisions}");
        GUILayout.Label($"Radius: {radius:F2}");
        GUILayout.Label($"Points: {points.Count}");
        GUILayout.Label($"Triangles: {triangles.Count}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔷 Add 3D Hexasphere")) {
            GenerateHexasphere();
        }
        
        GUILayout.Space(5);
        
        GUILayout.Label("Divisions:");
        divisions = (int)GUILayout.HorizontalSlider(divisions, 0, 10);
        
        GUILayout.Label("Radius:");
        radius = GUILayout.HorizontalSlider(radius, 0.1f, 5f);
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("🔧 Set")) {
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
        
        GUILayout.Label($"Chunking: {(useChunking ? "ON" : "OFF")}");
        GUILayout.Label($"Chunks: {chunkCount}");
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("🎯 Toggle Selective Subdivision")) {
            useSelectiveSubdivision = !useSelectiveSubdivision;
            GenerateHexasphere();
        }
        
        if (GUILayout.Button("🎯 Toggle Focus Debug")) {
            showFocusDebug = !showFocusDebug;
        }
        
        if (GUILayout.Button("🎯 Toggle Focus Point")) {
            showFocusPoint = !showFocusPoint;
        }
        
        GUILayout.Label($"Selective Subdivision: {(useSelectiveSubdivision ? "ON" : "OFF")}");
        GUILayout.Label($"Focus Point: {focusPoint}");
        GUILayout.Label($"Focus Radius: {focusRadius:F2}");
        GUILayout.Label($"Focus Divisions: {focusDivisions}");
        GUILayout.Label($"Background Divisions: {backgroundDivisions}");
        GUILayout.Label($"Focus Debug: {(showFocusDebug ? "ON" : "OFF")}");
        GUILayout.Label($"Focus Point Visible: {(showFocusPoint ? "ON" : "OFF")}");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    void OnDrawGizmos() {
        if (showFocusDebug && useSelectiveSubdivision) {
            DrawFocusDebug();
        }
    }
    
    void DrawFocusDebug() {
        // Dessiner la zone de focus
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(focusPoint * radius), focusRadius * radius);
        
        // Dessiner le point de focus
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(focusPoint * radius), 0.05f);
        
        // Dessiner quelques triangles dans la zone de focus
        Gizmos.color = Color.green;
        int triangleCount = 0;
        foreach (var triangle in triangles) {
            if (triangleCount > 50) break; // Limiter pour la performance
            
            if (IsTriangleInFocus(triangle)) {
                Vector3 v0 = transform.TransformPoint(triangle.points[0].ToVector3() * radius);
                Vector3 v1 = transform.TransformPoint(triangle.points[1].ToVector3() * radius);
                Vector3 v2 = transform.TransformPoint(triangle.points[2].ToVector3() * radius);
                
                Gizmos.DrawLine(v0, v1);
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v2, v0);
            }
            triangleCount++;
        }
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
            return new Point(x / length, y / length, z / length);
        }
    }
    
    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }
    
    public static Point Midpoint(Point a, Point b) {
        return new Point((a.x + b.x) / 2f, (a.y + b.y) / 2f, (a.z + b.z) / 2f);
    }
    
    public bool Equals(Point other) {
        if (other == null) return false;
        return Mathf.Approximately(x, other.x) && Mathf.Approximately(y, other.y) && Mathf.Approximately(z, other.z);
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

// Classe Triangle
public class Triangle {
    public Point[] points;
    
    public Triangle(Point a, Point b, Point c) {
        points = new Point[] { a, b, c };
    }
}
