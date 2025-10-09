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
    
    [Header("🌍 Génération Procédurale de Planète")]
    [SerializeField] public bool useProceduralGeneration = true;
    [SerializeField] public float noiseScale = 1f;
    [SerializeField] public float heightAmplitude = 0.2f;
    [SerializeField] public bool useAdvancedNoise = true;
    [SerializeField] public float baseNoiseScale = 1f;
    [SerializeField] public int baseOctaves = 4;
    [SerializeField] public float basePersistence = 0.5f;
    [SerializeField] public float baseLacunarity = 2f;
    [SerializeField] public float detailNoiseScale = 2f;
    [SerializeField] public int detailOctaves = 3;
    [SerializeField] public float detailPersistence = 0.3f;
    [SerializeField] public float detailLacunarity = 2f;
    [SerializeField] public bool useRidgeNoise = true;
    [SerializeField] public float ridgeNoiseScale = 0.5f;
    [SerializeField] public float ridgeIntensity = 0.3f;
    
    [Header("🌊 Niveaux de Terrain")]
    [SerializeField] public float waterLevel = 0.0f;
    [SerializeField] public float mountainLevel = 0.3f;
    [SerializeField] public bool useFlatOceans = true;
    [SerializeField] public bool forceOceanLevel = true;
    
    [Header("🌊 Système Océans Avancé")]
    [SerializeField] public bool useAdvancedOceanSystem = true;
    [SerializeField] public bool preserveBaseShape = true;
    [SerializeField] public float oceanFlatteningStrength = 1f;
    
    [Header("🎨 Matériaux de Planète")]
    [SerializeField] public Material waterMaterial;
    [SerializeField] public Material landMaterial;
    [SerializeField] public Material mountainMaterial;
    
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
    
    [Header("🔄 Subdivision Dynamique")]
    [SerializeField] public bool useDynamicSubdivision = true; // Subdivision dynamique
    [SerializeField] public Transform dynamicFocusTarget; // Cible pour la focalisation dynamique
    [SerializeField] public float updateThreshold = 0.1f; // Seuil de mise à jour
    [SerializeField] public float updateInterval = 0.5f; // Intervalle de mise à jour (secondes)
    [SerializeField] public bool smoothTransition = true; // Transition douce
    [SerializeField] public float transitionSpeed = 2f; // Vitesse de transition
    [SerializeField] public bool autoRegenerate = true; // Régénération automatique
    [SerializeField] public bool showDynamicDebug = false; // Debug de la subdivision dynamique
    [SerializeField] public bool continuousUpdate = false; // Mise à jour continue (ignore les seuils)
    
    [Header("🎯 Subdivision Automatique")]
    [SerializeField] public bool useAutoSubdivision = true; // Subdivision automatique basée sur la proximité
    [SerializeField] public float autoSubdivisionRadius = 0.3f; // Rayon de subdivision automatique
    [SerializeField] public int maxAutoSubdivisions = 3; // Nombre maximum de subdivisions automatiques
    [SerializeField] public float subdivisionThreshold = 0.1f; // Seuil pour déclencher la subdivision
    [SerializeField] public float updateCooldown = 0.1f; // Cooldown entre les mises à jour (secondes)
    [SerializeField] public bool showAutoSubdivisionDebug = false; // Debug de la subdivision automatique
    
    [Header("🔄 Réduction Automatique")]
    [SerializeField] public bool useAutoReduction = true; // Réduction automatique des subdivisions
    [SerializeField] public float reductionRadius = 0.5f; // Rayon au-delà duquel réduire les subdivisions
    [SerializeField] public float reductionThreshold = 0.2f; // Seuil pour déclencher la réduction
    [SerializeField] public bool showReductionDebug = false; // Debug de la réduction
    
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
    
    // Variables pour la subdivision dynamique
    private Vector3 lastFocusPoint;
    private float lastUpdateTime;
    private bool isUpdating = false;
    private Vector3 targetFocusPoint;
    private Vector3 currentFocusPoint;
    
    // Variables pour la subdivision automatique
    private float lastAverageDistance = 0f; // Distance moyenne précédente
    private int lastDivisions = 0; // Nombre de divisions précédent
    private float lastAutoUpdateTime = 0f; // Temps de la dernière mise à jour automatique
    
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
        
        // Initialiser la subdivision dynamique
        InitializeDynamicSubdivision();
        
        // Initialiser la subdivision automatique
        InitializeAutoSubdivision();
    }
    
    void InitializeDynamicSubdivision() {
        // Initialiser les variables de subdivision dynamique
        lastFocusPoint = focusPoint;
        currentFocusPoint = focusPoint;
        targetFocusPoint = focusPoint;
        lastUpdateTime = 0f;
        isUpdating = false;
        
        // Trouver automatiquement la caméra principale si pas de cible assignée
        if (dynamicFocusTarget == null) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                dynamicFocusTarget = mainCamera.transform;
            }
        }
    }
    
    void InitializeAutoSubdivision() {
        // Initialiser les variables de subdivision automatique
        lastAverageDistance = 0f;
        lastDivisions = divisions;
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            GenerateHexasphere();
        }
        
        // Mettre à jour le point de focus
        UpdateFocusPoint();
        
        // Gérer la subdivision dynamique
        if (useDynamicSubdivision && useSelectiveSubdivision) {
            UpdateDynamicSubdivision();
        }
        
        // Gérer la subdivision automatique (indépendante de useSelectiveSubdivision)
        if (useAutoSubdivision) {
            UpdateAutoSubdivision();
        } 
    }
    
    public void GenerateHexasphere() {
        
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
        }
    }
    
    bool IsTriangleInFocus(Triangle triangle) {
        // Calculer le centre du triangle (normalisé sur la sphère)
        Vector3 center = (triangle.points[0].ToVector3() + triangle.points[1].ToVector3() + triangle.points[2].ToVector3()) / 3f;
        center = center.normalized; // S'assurer que c'est sur la sphère
        
        // Calculer l'angle entre le centre du triangle et le point de focus
        float angle = Vector3.Angle(center, focusPoint);
        
        // Convertir l'angle en distance angulaire (en radians)
        float angularDistance = angle * Mathf.Deg2Rad;
        
        // Vérifier si le triangle est dans la zone de focus
        return angularDistance <= focusRadius;
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
                // Créer un matériau transparent pour la sphère
                Material focusMaterial = new Material(Shader.Find("Standard"));
                
                // Configuration pour la transparence
                focusMaterial.SetFloat("_Mode", 3); // Mode transparent
                focusMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                focusMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                focusMaterial.SetInt("_ZWrite", 0);
                focusMaterial.DisableKeyword("_ALPHATEST_ON");
                focusMaterial.EnableKeyword("_ALPHABLEND_ON");
                focusMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                focusMaterial.renderQueue = 3000;
                
                // Couleur transparente
                Color transparentColor = focusPointColor;
                transparentColor.a = 0.3f; // 30% d'opacité
                focusMaterial.color = transparentColor;
                
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
            // Visible seulement dans la scène, pas en simulation
            bool shouldShow = showFocusPoint && useSelectiveSubdivision;
            if (Application.isPlaying) {
                shouldShow = false; // Masquer en simulation
            }
            focusPointObject.SetActive(shouldShow);
            
            // Mettre à jour la couleur avec transparence
            if (focusPointRenderer != null) {
                Color transparentColor = focusPointColor;
                transparentColor.a = 0.3f; // 30% d'opacité
                focusPointRenderer.material.color = transparentColor;
            }
        }
    }
    
    void UpdateDynamicSubdivision() {
        if (dynamicFocusTarget == null) return;
        
        // Calculer le nouveau point de focus basé sur la cible
        Vector3 targetPosition = dynamicFocusTarget.position;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        Vector3 newFocusPoint = directionToTarget;
        
        // Vérifier si le point de focus a changé significativement
        float distanceChange = Vector3.Distance(newFocusPoint, lastFocusPoint);
        bool shouldUpdate = continuousUpdate || (distanceChange > updateThreshold);
        
        // Vérifier l'intervalle de temps
        bool timeToUpdate = continuousUpdate || (Time.time - lastUpdateTime > updateInterval);
        
        if (shouldUpdate && timeToUpdate && !isUpdating) {
            // Démarrer la mise à jour
            isUpdating = true;
            targetFocusPoint = newFocusPoint;
            lastUpdateTime = Time.time;

            
            // Régénérer immédiatement si autoRegenerate est activé
            if (autoRegenerate) {
                focusPoint = targetFocusPoint;
                currentFocusPoint = targetFocusPoint;
                RegenerateMeshWithNewFocus();
                isUpdating = false;
            }
        }
        
        // Transition douce vers le nouveau point de focus (seulement si pas déjà traité)
        if (isUpdating && !autoRegenerate) {
            if (smoothTransition) {
                // Transition douce
                currentFocusPoint = Vector3.Lerp(currentFocusPoint, targetFocusPoint, transitionSpeed * Time.deltaTime);
                focusPoint = currentFocusPoint;
                
                // Vérifier si la transition est terminée
                if (Vector3.Distance(currentFocusPoint, targetFocusPoint) < 0.01f) {
                    focusPoint = targetFocusPoint;
                    currentFocusPoint = targetFocusPoint;
                    isUpdating = false;
                }
            } else {
                // Transition instantanée
                focusPoint = targetFocusPoint;
                currentFocusPoint = targetFocusPoint;
                isUpdating = false;
            }
        }
        
        // Mettre à jour le point de focus précédent
        lastFocusPoint = newFocusPoint;
    }
    
    void RegenerateMeshWithNewFocus() {
        // Vérifier si le point de focus a vraiment changé
        if (Vector3.Distance(focusPoint, lastFocusPoint) < 0.001f) {
            return; // Pas de changement significatif, pas besoin de régénérer
        }
        
        // Nettoyer les anciens chunks avant de régénérer
        CleanupOldChunks();
        
        // Régénérer la géométrie complète avec le nouveau point de focus
        RegenerateGeometryWithFocus();
    }
    
    void RegenerateGeometryWithFocus() {
        // Régénérer la géométrie de base avec le nouveau point de focus
        CreateIcosahedron();
        ApplySubdivisions();
        
        // Régénérer le mesh
        if (useChunking) {
            GenerateMeshWithChunking();
        } else {
            GenerateMeshSingle();
        }
    }
    
    void UpdateAutoSubdivision() {
        if (dynamicFocusTarget == null) {
            return;
        }
        
        // Vérifier le cooldown pour éviter les mises à jour trop fréquentes
        if (Time.time - lastAutoUpdateTime < updateCooldown) {
            return;
        }
        
        // Calculer le point de focus actuel
        Vector3 targetPosition = dynamicFocusTarget.position;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        
        // Calculer la distance moyenne de tous les triangles
        float averageDistance = 0f;
        int triangleCount = 0;
        
        foreach (var triangle in triangles) {
            float distance = GetTriangleDistanceFromFocus(triangle, directionToTarget);
            averageDistance += distance;
            triangleCount++;
        }
        
        if (triangleCount > 0) {
            averageDistance /= triangleCount;
        }
        
        
        // Déterminer le niveau de subdivision basé sur la distance moyenne
        int targetDivisions = divisions;
        
        if (averageDistance <= autoSubdivisionRadius) {
            // Proche - augmenter les subdivisions
            targetDivisions = Mathf.Min(divisions + 1, maxAutoSubdivisions);
        } else if (averageDistance > reductionRadius && useAutoReduction) {
            // Loin - réduire les subdivisions
            targetDivisions = Mathf.Max(1, divisions - 1);
        }
        
        
        // Si le niveau de subdivision a changé et que le changement est significatif, régénérer le mesh
        if (targetDivisions != divisions && Mathf.Abs(averageDistance - lastAverageDistance) > subdivisionThreshold) {
            divisions = targetDivisions;
            lastDivisions = divisions;
            lastAverageDistance = averageDistance;
            
            // Mettre à jour le temps de la dernière mise à jour
            lastAutoUpdateTime = Time.time;
            
            // Régénérer la géométrie complète avec le nouveau niveau de subdivision
            RegenerateGeometryWithFocus();
        } 
    }
    
    bool IsTriangleInAutoSubdivisionZone(Triangle triangle, Vector3 focusDirection) {
        // Calculer le centre du triangle
        Vector3 center = (triangle.points[0].ToVector3() + triangle.points[1].ToVector3() + triangle.points[2].ToVector3()) / 3f;
        center = center.normalized;
        
        // Calculer l'angle entre le centre du triangle et le point de focus
        float angle = Vector3.Angle(center, focusDirection);
        float angularDistance = angle * Mathf.Deg2Rad;
        
        // Vérifier si le triangle est dans la zone de subdivision automatique
        return angularDistance <= autoSubdivisionRadius;
    }
    
    float GetTriangleDistanceFromFocus(Triangle triangle, Vector3 focusDirection) {
        // Calculer le centre du triangle
        Vector3 center = (triangle.points[0].ToVector3() + triangle.points[1].ToVector3() + triangle.points[2].ToVector3()) / 3f;
        center = center.normalized;
        
        // Calculer l'angle entre le centre du triangle et le point de focus
        float angle = Vector3.Angle(center, focusDirection);
        float angularDistance = angle * Mathf.Deg2Rad;
        
        return angularDistance;
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
        
        // Appliquer la génération procédurale si activée
        if (useProceduralGeneration) {
            ApplyProceduralGeneration(vertices, uvs, triangles);
        } else {
            // Créer le mesh simple
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
        }
        
        // Ajouter un collider pour la détection (après avoir créé le mesh)
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null) {
            collider = gameObject.AddComponent<MeshCollider>();
        }
        collider.sharedMesh = hexagonMesh;
        collider.convex = false; // Important pour les raycasts
        
        // Forcer la mise à jour des bounds du renderer
        if (meshRenderer != null) {
            meshRenderer.bounds = hexagonMesh.bounds;
        }
    }
    
    // Méthode publique pour forcer la mise à jour des bounds
    public void UpdateRendererBounds() {
        if (meshRenderer != null && hexagonMesh != null) {
            meshRenderer.bounds = hexagonMesh.bounds;
        }
        
        // Mettre à jour aussi les bounds des chunks
        if (meshRendererChunks != null) {
            for (int i = 0; i < meshRendererChunks.Length; i++) {
                if (meshRendererChunks[i] != null && meshChunks[i] != null) {
                    meshRendererChunks[i].bounds = meshChunks[i].bounds;
                }
            }
        }
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
        
        // Appliquer la génération procédurale si activée
        if (useProceduralGeneration) {
            ApplyProceduralGenerationToChunks();
        }
        
        // Créer les meshes des chunks
        CreateChunkMeshes();
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
                chunkObject.transform.localPosition = Vector3.zero; // S'assurer que le chunk est à (0,0,0) par rapport au parent
                chunkObject.transform.localRotation = Quaternion.identity; // Pas de rotation
                chunkObject.transform.localScale = Vector3.one; // Échelle normale
                chunkObject.tag = "Planet"; // Ajouter le tag Planet au chunk
                
                // Ajouter les composants
                meshFilterChunks[i] = chunkObject.AddComponent<MeshFilter>();
                meshRendererChunks[i] = chunkObject.AddComponent<MeshRenderer>();
                
                // Créer le mesh d'abord
                meshChunks[i] = new Mesh();
                meshChunks[i].name = $"Hexasphere Chunk {i} Mesh";
                meshChunks[i].vertices = verticesChunks[i].ToArray();
                meshChunks[i].triangles = trianglesChunks[i].ToArray();
                meshChunks[i].uv = uvsChunks[i].ToArray();
                meshChunks[i].RecalculateNormals();
                meshChunks[i].RecalculateBounds();
                
                // Appliquer la génération procédurale aux chunks si activée
                if (useProceduralGeneration) {
                    CreateProceduralChunkMesh(i);
                } else {
                    // Assigner le mesh simple
                    meshFilterChunks[i].mesh = meshChunks[i];
                    
                    // Configurer le matériau simple
                    if (hexagonMaterial == null) {
                        hexagonMaterial = new Material(Shader.Find("Standard"));
                        hexagonMaterial.color = hexagonColor;
                    }
                    meshRendererChunks[i].material = hexagonMaterial;
                }
                
                // Ajouter un collider pour la détection (après avoir créé le mesh)
                MeshCollider collider = chunkObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshChunks[i];
                collider.convex = false; // Important pour les raycasts
                
                chunkCount++;
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
    }
    
    Point GetCachedPoint(Point point) {
        if (points.ContainsKey(point)) {
            return points[point];
        }
        points[point] = point;
        return point;
    }
    

    
    void OnDrawGizmos() {
        if (showFocusDebug && useSelectiveSubdivision) {
            DrawFocusDebug();
        }
    }
    
    void OnDrawGizmosSelected() {
        // Dessiner le point de focus dans la scène
        if (showFocusPoint && useSelectiveSubdivision) {
            DrawFocusPointGizmo();
        }
    }
    
    void DrawFocusPointGizmo() {
        // Dessiner la sphère de focus
        Gizmos.color = new Color(focusPointColor.r, focusPointColor.g, focusPointColor.b, 0.3f);
        Vector3 worldPosition = transform.TransformPoint(focusPoint * radius);
        Gizmos.DrawWireSphere(worldPosition, focusRadius * radius);
        
        // Dessiner le point central
        Gizmos.color = focusPointColor;
        Gizmos.DrawWireSphere(worldPosition, focusPointSize);
        
        // Dessiner une ligne du centre vers le point de focus
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, worldPosition);
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
    
    // === GÉNÉRATION PROCÉDURALE DE PLANÈTE ===
    
    float GenerateHeight(Vector3 position) {
        if (useAdvancedNoise) {
            return GenerateAdvancedHeight(position);
        } else {
            return GeneratePerlinHeight(position);
        }
    }
    
    float GeneratePerlinHeight(Vector3 position) {
        // Utiliser les coordonnées sphériques pour un bruit plus naturel
        float latitude = Mathf.Asin(position.y);
        float longitude = Mathf.Atan2(position.z, position.x);
        
        // Convertir en coordonnées UV pour le bruit
        float u = (longitude + Mathf.PI) / (2f * Mathf.PI);
        float v = (latitude + Mathf.PI / 2f) / Mathf.PI;
        
        // Générer plusieurs octaves de bruit
        float height = 0f;
        float frequency = 1f;
        float amplitude = heightAmplitude;
        float maxValue = 0f;

        for (int i = 0; i < 6; i++) {
            float noiseValue = Mathf.PerlinNoise(
                u * noiseScale * frequency,
                v * noiseScale * frequency
            );
            height += noiseValue * amplitude;
            maxValue += amplitude;
            frequency *= 2f;
            amplitude *= 0.5f;
        }

        // Normaliser le résultat
        if (maxValue > 0) {
            height = height / maxValue;
        }

        return height;
    }
    
    float GenerateAdvancedHeight(Vector3 position) {
        // Coordonnées sphériques
        float latitude = Mathf.Asin(position.y);
        float longitude = Mathf.Atan2(position.z, position.x);
        
        // Convertir en coordonnées UV
        float u = (longitude + Mathf.PI) / (2f * Mathf.PI);
        float v = (latitude + Mathf.PI / 2f) / Mathf.PI;
        
        // Bruit de base (grandes structures)
        float baseHeight = GenerateFractalNoise(u, v, baseNoiseScale, baseOctaves, basePersistence, baseLacunarity);
        
        // Bruit de détail (petites structures)
        float detailHeight = GenerateFractalNoise(u, v, detailNoiseScale, detailOctaves, detailPersistence, detailLacunarity);
        
        // Bruit de ridges (montagnes)
        float ridgeHeight = 0f;
        if (useRidgeNoise) {
            ridgeHeight = GenerateRidgeNoise(u, v, ridgeNoiseScale) * ridgeIntensity;
        }
        
        // Combiner les bruits
        float totalHeight = (baseHeight + detailHeight * 0.3f + ridgeHeight) * heightAmplitude;
        
        return totalHeight;
    }
    
    float GenerateFractalNoise(float u, float v, float scale, int octaves, float persistence, float lacunarity) {
        float height = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;
        
        for (int i = 0; i < octaves; i++) {
            float noiseValue = Mathf.PerlinNoise(u * scale * frequency, v * scale * frequency);
            height += noiseValue * amplitude;
            maxValue += amplitude;
            frequency *= lacunarity;
            amplitude *= persistence;
        }
        
        if (maxValue > 0) {
            height = height / maxValue;
        }
        
        return height;
    }
    
    float GenerateRidgeNoise(float u, float v, float scale) {
        float noise1 = Mathf.PerlinNoise(u * scale, v * scale);
        float noise2 = Mathf.PerlinNoise(u * scale * 2f, v * scale * 2f);
        
        // Créer des ridges en utilisant la valeur absolue
        float ridge = Mathf.Abs(noise1 - 0.5f) * 2f;
        ridge = 1f - ridge;
        ridge = ridge * ridge;
        
        return ridge;
    }
    
    void ApplyProceduralGeneration(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
        if (!useProceduralGeneration) return;
        
        // Appliquer les hauteurs procédurales
        for (int i = 0; i < vertices.Count; i++) {
            Vector3 originalVertex = vertices[i];
            Vector3 normalizedVertex = originalVertex.normalized;
            
            float height = GenerateHeight(normalizedVertex);
            
            // Nouveau système d'océans qui préserve la forme de base
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(normalizedVertex, height);
            } else {
                // Ancien système (pour compatibilité)
                if (useFlatOceans && height <= waterLevel) {
                    height = 0f; // Océans plats au niveau 0
                } else if (height > waterLevel) {
                    if (forceOceanLevel) {
                        height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Appliquer la hauteur au vertex
            vertices[i] = normalizedVertex * (radius + height);
        }
        
        // Créer le mesh avec multi-matériaux
        CreateProceduralMesh(vertices, uvs, triangles);
    }
    
    void ApplyProceduralGenerationToChunks() {
        if (!useProceduralGeneration) return;
        
        // Appliquer la génération procédurale à tous les chunks
        for (int chunkIndex = 0; chunkIndex < maxChunks; chunkIndex++) {
            if (verticesChunks[chunkIndex].Count > 0) {
                // Appliquer les hauteurs procédurales
                for (int i = 0; i < verticesChunks[chunkIndex].Count; i++) {
                    Vector3 originalVertex = verticesChunks[chunkIndex][i];
                    Vector3 normalizedVertex = originalVertex.normalized;
                    
                    float height = GenerateHeight(normalizedVertex);
                    
                    // Nouveau système d'océans qui préserve la forme de base
                    if (useAdvancedOceanSystem && preserveBaseShape) {
                        height = ApplyAdvancedOceanSystem(normalizedVertex, height);
                    } else {
                        // Ancien système (pour compatibilité)
                        if (useFlatOceans && height <= waterLevel) {
                            height = 0f; // Océans plats au niveau 0
                        } else if (height > waterLevel) {
                            if (forceOceanLevel) {
                                height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
                            }
                        }
                    }
                    
                    // Appliquer la hauteur au vertex
                    verticesChunks[chunkIndex][i] = normalizedVertex * (radius + height);
                }
            }
        }
    }
    
    void CreateProceduralMesh(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
        // Séparer les triangles par type de terrain
        List<int> waterTriangles = new List<int>();
        List<int> landTriangles = new List<int>();
        List<int> mountainTriangles = new List<int>();
        
        for (int i = 0; i < triangles.Count; i += 3) {
            int p1 = triangles[i];
            int p2 = triangles[i + 1];
            int p3 = triangles[i + 2];
            
            // Calculer l'altitude moyenne du triangle
            float avgHeight = (GetVertexHeight(vertices[p1]) + GetVertexHeight(vertices[p2]) + GetVertexHeight(vertices[p3])) / 3f;
            
            // Assigner au bon type de terrain
            if (avgHeight <= waterLevel) { // Océans au niveau de l'eau
                waterTriangles.Add(p1);
                waterTriangles.Add(p2);
                waterTriangles.Add(p3);
            } else if (avgHeight <= mountainLevel) { // Seuil de montagne normal
                landTriangles.Add(p1);
                landTriangles.Add(p2);
                landTriangles.Add(p3);
            } else {
                mountainTriangles.Add(p1);
                mountainTriangles.Add(p2);
                mountainTriangles.Add(p3);
            }
        }
        
        // Créer le mesh avec submeshes
        Mesh mesh = new Mesh();
        mesh.name = "HexasphereFill Planet";
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        
        // Créer les submeshes
        mesh.subMeshCount = 3;
        mesh.SetTriangles(waterTriangles.ToArray(), 0);
        mesh.SetTriangles(landTriangles.ToArray(), 1);
        mesh.SetTriangles(mountainTriangles.ToArray(), 2);
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Appliquer le mesh
        if (meshFilter != null) {
            meshFilter.mesh = mesh;
        }
        
        // Appliquer les matériaux
        ApplyPlanetMaterials();
    }
    
    float GetVertexHeight(Vector3 vertex) {
        Vector3 normalizedVertex = vertex.normalized;
        float height = GenerateHeight(normalizedVertex);
        
        // Appliquer le système d'océans avancé si activé
        if (useAdvancedOceanSystem && preserveBaseShape) {
            height = ApplyAdvancedOceanSystem(normalizedVertex, height);
        } else {
            // Ancien système (pour compatibilité)
            if (useFlatOceans && height <= waterLevel) {
                height = 0f; // Océans plats au niveau 0
            } else if (height > waterLevel) {
                if (forceOceanLevel) {
                    height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
                }
            }
        }
        
        return height;
    }
    
    void ApplyPlanetMaterials() {
        if (meshRenderer == null) return;
        
        // Créer des matériaux par défaut si aucun n'est assigné
        if (waterMaterial == null) {
            waterMaterial = CreateDefaultMaterial(Color.blue, "Water");
        }
        if (landMaterial == null) {
            landMaterial = CreateDefaultMaterial(Color.green, "Land");
        }
        if (mountainMaterial == null) {
            mountainMaterial = CreateDefaultMaterial(Color.gray, "Mountain");
        }
        
        // Assigner les matériaux
        Material[] materials = { waterMaterial, landMaterial, mountainMaterial };
        meshRenderer.materials = materials;
    }
    
    Material CreateDefaultMaterial(Color color, string name) {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.5f);
        return mat;
    }
    
    // === SYSTÈME OCÉANS AVANCÉ ===
    
    float ApplyAdvancedOceanSystem(Vector3 vertex, float originalHeight) {
        // Préserver la forme de base : ne pas modifier la hauteur globale
        // Seulement aplatir les zones qui devraient être submergées
        
        if (originalHeight <= waterLevel) {
            // Zone qui devrait être submergée
            // Au lieu de mettre à 0, on aplatit progressivement vers le niveau de l'eau
            float flatteningFactor = Mathf.Clamp01((waterLevel - originalHeight) / waterLevel);
            float flattenedHeight = Mathf.Lerp(originalHeight, waterLevel, flatteningFactor * oceanFlatteningStrength);
            
            // Pour les océans plats, on peut encore les aplatir complètement si souhaité
            if (useFlatOceans) {
                return waterLevel; // Niveau constant pour les océans
            } else {
                return flattenedHeight; // Aplatissement progressif
            }
        } else {
            // Zone terrestre : garder la hauteur originale
            // Pas de modification de la forme de base
            return originalHeight;
        }
    }
    
    void CreateProceduralChunkMesh(int chunkIndex) {
        // Séparer les triangles par type de terrain pour ce chunk
        List<int> waterTriangles = new List<int>();
        List<int> landTriangles = new List<int>();
        List<int> mountainTriangles = new List<int>();
        
        for (int i = 0; i < trianglesChunks[chunkIndex].Count; i += 3) {
            int p1 = trianglesChunks[chunkIndex][i];
            int p2 = trianglesChunks[chunkIndex][i + 1];
            int p3 = trianglesChunks[chunkIndex][i + 2];
            
            // Calculer l'altitude moyenne du triangle
            float avgHeight = (GetVertexHeight(verticesChunks[chunkIndex][p1]) + 
                              GetVertexHeight(verticesChunks[chunkIndex][p2]) + 
                              GetVertexHeight(verticesChunks[chunkIndex][p3])) / 3f;
            
            // Assigner au bon type de terrain
            if (avgHeight <= waterLevel) { // Océans au niveau de l'eau
                waterTriangles.Add(p1);
                waterTriangles.Add(p2);
                waterTriangles.Add(p3);
            } else if (avgHeight <= mountainLevel) { // Seuil de montagne normal
                landTriangles.Add(p1);
                landTriangles.Add(p2);
                landTriangles.Add(p3);
            } else {
                mountainTriangles.Add(p1);
                mountainTriangles.Add(p2);
                mountainTriangles.Add(p3);
            }
        }
        
        // Créer le mesh avec submeshes
        Mesh chunkMesh = new Mesh();
        chunkMesh.name = $"HexasphereFill Chunk {chunkIndex} Planet";
        chunkMesh.vertices = verticesChunks[chunkIndex].ToArray();
        chunkMesh.uv = uvsChunks[chunkIndex].ToArray();
        
        // Créer les submeshes
        chunkMesh.subMeshCount = 3;
        chunkMesh.SetTriangles(waterTriangles.ToArray(), 0);
        chunkMesh.SetTriangles(landTriangles.ToArray(), 1);
        chunkMesh.SetTriangles(mountainTriangles.ToArray(), 2);
        
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        
        // Appliquer le mesh
        meshFilterChunks[chunkIndex].mesh = chunkMesh;
        
        // Appliquer les matériaux de planète
        ApplyPlanetMaterialsToChunk(chunkIndex);
    }
    
    void ApplyPlanetMaterialsToChunk(int chunkIndex) {
        if (meshRendererChunks[chunkIndex] == null) return;
        
        // Créer des matériaux par défaut si aucun n'est assigné
        if (waterMaterial == null) {
            waterMaterial = CreateDefaultMaterial(Color.blue, "Water");
        }
        if (landMaterial == null) {
            landMaterial = CreateDefaultMaterial(Color.green, "Land");
        }
        if (mountainMaterial == null) {
            mountainMaterial = CreateDefaultMaterial(Color.gray, "Mountain");
        }
        
        // Assigner les matériaux
        Material[] materials = { waterMaterial, landMaterial, mountainMaterial };
        meshRendererChunks[chunkIndex].materials = materials;
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
