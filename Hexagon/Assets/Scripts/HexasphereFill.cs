using UnityEngine;
using System.Collections.Generic;
using System;

// Enum pour les types de terrain
public enum TerrainType {
    Water = 0,
    Land = 1,
    Mountain = 2
}

public class HexasphereFill : MonoBehaviour {

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
    
    [Header("🔍 Subdivision par Frontières de Matériaux")]
    [SerializeField] public bool useMaterialBoundarySubdivision = true; // Subdivision des triangles frontières
    [SerializeField] public bool applyToEntirePlanet = true; // Appliquer à toute la planète (pas seulement le focus)
    [SerializeField] public bool showBoundaryDebug = false; // Afficher les triangles frontières
    [SerializeField] public Color boundaryTriangleColor = Color.yellow; // Couleur des triangles frontières
    [SerializeField] public float boundaryDetectionThreshold = 0.1f; // Seuil de détection des frontières
    [SerializeField] public int maxBoundarySubdivisions = 2; // Nombre de subdivisions supplémentaires pour les frontières
    [SerializeField] public bool showSubdivisionInfo = true; // Afficher les informations de subdivision
    [SerializeField] public bool useOptimizedNeighborDetection = true; // Utiliser la détection optimisée des voisins
    [SerializeField] public bool useSubdivisionLevelDetection = true; // Utiliser la détection basée sur le niveau de subdivision
    [SerializeField] public float complexityThreshold = 0.1f; // Seuil de complexité pour déterminer si un triangle doit être subdivisé
    [SerializeField] public float altitudeWeight = 0.4f; // Poids de la variation d'altitude
    [SerializeField] public float terrainWeight = 0.3f; // Poids de la variation de terrain
    [SerializeField] public float sizeWeight = 0.2f; // Poids de la taille du triangle
    [SerializeField] public float boundaryWeight = 0.1f; // Poids de la proximité aux frontières

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
    [SerializeField] public float heightAmplitude = 0.2f;
    [SerializeField] public float heightMultiplier = 1f; // Multiplicateur global de hauteur (1.0 = normal, 2.0 = double, etc.)
    [SerializeField] public bool autoScaleHeightWithRadius = true; // Ajuster automatiquement la hauteur avec le radius
    [SerializeField] public float heightScalePower = 0.7f; // Puissance de l'ajustement (0.5 = doux, 1.0 = proportionnel)
    [SerializeField] public bool use3DNoise = true; // Utiliser le bruit 3D (recommandé)
    
    [Header("🌍 Paramètres Noisemap 3D")]
    [SerializeField] public float noise3DScale = 1f; // Échelle pour la noisemap 3D
    [SerializeField] public int noise3DOctaves = 6; // Nombre d'octaves pour la 3D
    [SerializeField] public float noise3DPersistence = 0.5f; // Persistance pour la 3D
    [SerializeField] public float noise3DLacunarity = 2f; // Lacunarité pour la 3D
    [SerializeField] public bool use3DRidgeNoise = true; // Bruit de ridges 3D
    [SerializeField] public float ridge3DScale = 0.5f; // Échelle des ridges 3D
    [SerializeField] public float ridge3DIntensity = 0.3f; // Intensité des ridges 3D
    
    [Header("🔧 Correction des Fentes")]
    [SerializeField] public bool fixIcosahedronSeams = true; // Corriger les fentes de l'icosaèdre
    [SerializeField] public float vertexTolerance = 0.0001f; // Tolérance pour identifier les vertices partagés
    [SerializeField] public bool usePreciseSeamDetection = true; // Détection précise des arêtes
    [SerializeField] public float seamDetectionRadius = 0.01f; // Rayon de détection des arêtes
    [SerializeField] public bool useFastCache = true; // Utiliser un cache rapide optimisé
    [SerializeField] public bool disableCacheForPerformance = false; // Désactiver le cache pour les performances
    
    [Header("🌊 Niveaux de Terrain")]
    [SerializeField] public float waterLevel = 0.0f;
    [SerializeField] public float mountainLevel = 0.3f;
    [SerializeField] public bool autoScaleTerrainLevels = true; // Ajuster automatiquement les niveaux avec le radius
    [SerializeField] public bool useFlatOceans = true;
    [SerializeField] public bool smoothOceanSeams = true; // Lisser les fentes dans les océans
    [SerializeField] public float oceanSmoothingRadius = 0.05f; // Rayon de lissage des océans
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
    
    [Header("🔷 Système de Chunks par Face")]
    [SerializeField] public bool useIcosahedronChunking = true; // Chunking basé sur les 20 faces de l'icosaèdre
    [SerializeField] public bool showChunkDebug = false; // Debug des chunks
    [SerializeField] public Color[] chunkColors = new Color[20]; // Couleurs pour chaque chunk
    
    [Header("🛡️ Protection des Chunks")]
    [SerializeField] public bool useChunkProtection = true; // Protection contre l'explosion des meshes
    [SerializeField] public int maxVerticesPerFaceChunk = 4000; // Limite de vertices par chunk de face (ultra conservateur)
    [SerializeField] public int maxTrianglesPerFaceChunk = 6000; // Limite de triangles par chunk de face (ultra conservateur)
    [SerializeField] public bool autoSplitLargeChunks = true; // Diviser automatiquement les chunks trop gros
    [SerializeField] public int maxSubChunksPerFace = 64; // Nombre max de sous-chunks par face (ultra augmenté)
    
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
    
    // Variables pour le chunking par face d'icosaèdre
    private List<Vector3>[] faceVerticesChunks;
    private List<int>[] faceTrianglesChunks;
    private List<Vector2>[] faceUvsChunks;
    private Mesh[] faceMeshChunks;
    private MeshFilter[] faceMeshFilterChunks;
    private MeshRenderer[] faceMeshRendererChunks;
    private GameObject[] faceChunkObjects;
    private int[] faceChunkTriangleCounts;
    private bool[] faceChunkActive;
    
    // Variables pour les sous-chunks (protection contre l'explosion)
    private List<List<Vector3>>[] faceSubVerticesChunks;
    private List<List<int>>[] faceSubTrianglesChunks;
    private List<List<Vector2>>[] faceSubUvsChunks;
    private List<Mesh>[] faceSubMeshChunks;
    private List<MeshFilter>[] faceSubMeshFilterChunks;
    private List<MeshRenderer>[] faceSubMeshRendererChunks;
    private List<GameObject>[] faceSubChunkObjects;
    private int[] faceSubChunkCounts;
    
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
    
    // Variables pour la subdivision par frontières de matériaux
    private List<Triangle> boundaryTriangles = new List<Triangle>(); // Triangles à la frontière
    private Dictionary<Triangle, TerrainType> triangleTerrainTypes = new Dictionary<Triangle, TerrainType>(); // Types de terrain par triangle
    
    // Cache pour les hauteurs des vertices (correction des fentes)
    private Dictionary<Vector3, float> vertexHeightCache = new Dictionary<Vector3, float>();
    private Dictionary<Vector3, Vector3> normalizedVertexCache = new Dictionary<Vector3, Vector3>();
    
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
        if (useIcosahedronChunking) {
            GenerateMeshWithIcosahedronChunking();
        } else if (useChunking) {
            GenerateMeshWithChunking();
        } else {
            GenerateMeshSingle();
        }
        
        // Afficher les informations de subdivision
        if (showSubdivisionInfo) {
            DisplaySubdivisionInfo();
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
        
        // Enfin, appliquer la subdivision des triangles frontières si activée
        if (useMaterialBoundarySubdivision) {
            ApplyMaterialBoundarySubdivision();
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
    
    // === MÉTHODES POUR LA SUBDIVISION PAR FRONTIÈRES DE MATÉRIAUX ===
    
    void ApplyMaterialBoundarySubdivision() {
        // Détecter les triangles frontières (dans le focus ou sur toute la planète)
        DetectBoundaryTriangles();
        
        // Subdiviser les triangles frontières détectés
        SubdivideBoundaryTriangles();
    }
    
    void DetectBoundaryTriangles() {
        boundaryTriangles.Clear();
        triangleTerrainTypes.Clear();
        
        // D'abord, déterminer le type de terrain pour chaque triangle
        foreach (var triangle in triangles) {
            // Analyser tous les triangles si applyToEntirePlanet est activé, sinon seulement ceux en focus
            bool shouldAnalyze = applyToEntirePlanet || IsTriangleInFocus(triangle);
            
            if (shouldAnalyze) {
                TerrainType terrainType = DetermineTriangleTerrainType(triangle);
                triangleTerrainTypes[triangle] = terrainType;
            }
        }
        
        // Ensuite, détecter les triangles frontières
        foreach (var triangle in triangles) {
            bool shouldCheck = applyToEntirePlanet || IsTriangleInFocus(triangle);
            
            if (shouldCheck && IsTriangleOnBoundary(triangle)) {
                boundaryTriangles.Add(triangle);
            }
        }
        
        if (showBoundaryDebug) {
            string scope = applyToEntirePlanet ? "sur toute la planète" : "dans la zone de focus";
            Debug.Log($"🔍 {boundaryTriangles.Count} triangles frontières détectés {scope}");
        }
    }
    
    TerrainType DetermineTriangleTerrainType(Triangle triangle) {
        // Calculer l'altitude moyenne du triangle
        Vector3 v1 = triangle.points[0].ToVector3() * radius;
        Vector3 v2 = triangle.points[1].ToVector3() * radius;
        Vector3 v3 = triangle.points[2].ToVector3() * radius;
        
        float avgHeight = (GetVertexHeight(v1) + GetVertexHeight(v2) + GetVertexHeight(v3)) / 3f;
        
        // Déterminer le type de terrain basé sur l'altitude
        float effectiveWaterLevel = GetEffectiveWaterLevel();
        float effectiveMountainLevel = GetEffectiveMountainLevel();
        
        if (avgHeight <= effectiveWaterLevel) {
            return TerrainType.Water;
        } else if (avgHeight <= effectiveMountainLevel) {
            return TerrainType.Land;
        } else {
            return TerrainType.Mountain;
        }
    }
    
    bool IsTriangleOnBoundary(Triangle triangle) {
        if (!triangleTerrainTypes.ContainsKey(triangle)) {
            return false;
        }
        
        // Utiliser la nouvelle méthode de détection basée sur le niveau de subdivision
        if (useSubdivisionLevelDetection) {
            return ShouldTriangleBeSubdivided(triangle);
        }
        
        // Ancienne méthode : vérifier les voisins
        TerrainType triangleType = triangleTerrainTypes[triangle];
        
        // Vérifier si le triangle a des voisins avec des types de terrain différents
        List<Triangle> neighbors = useOptimizedNeighborDetection ? 
            FindTriangleNeighborsOptimized(triangle) : 
            FindTriangleNeighbors(triangle);
        
        foreach (var neighbor in neighbors) {
            if (triangleTerrainTypes.ContainsKey(neighbor)) {
                TerrainType neighborType = triangleTerrainTypes[neighbor];
                if (neighborType != triangleType) {
                    return true; // Triangle à la frontière
                }
            }
        }
        
        return false;
    }
    
    List<Triangle> FindTriangleNeighbors(Triangle triangle) {
        List<Triangle> neighbors = new List<Triangle>();
        
        // Trouver les triangles qui partagent au moins un point avec ce triangle
        foreach (var otherTriangle in triangles) {
            if (otherTriangle == triangle) continue;
            
            // Vérifier si les triangles partagent des points
            int sharedPoints = 0;
            foreach (var point1 in triangle.points) {
                foreach (var point2 in otherTriangle.points) {
                    if (point1.Equals(point2)) {
                        sharedPoints++;
                        break;
                    }
                }
            }
            
            // Si ils partagent au moins 2 points, ils sont voisins
            if (sharedPoints >= 2) {
                neighbors.Add(otherTriangle);
            }
        }
        
        return neighbors;
    }
    
    // Méthode optimisée pour trouver les voisins d'un triangle
    List<Triangle> FindTriangleNeighborsOptimized(Triangle triangle) {
        List<Triangle> neighbors = new List<Triangle>();
        
        // Créer un set des points du triangle pour une recherche plus rapide
        HashSet<Point> trianglePoints = new HashSet<Point>(triangle.points);
        
        // Trouver les triangles qui partagent au moins 2 points
        foreach (var otherTriangle in triangles) {
            if (otherTriangle == triangle) continue;
            
            int sharedPoints = 0;
            foreach (var point in otherTriangle.points) {
                if (trianglePoints.Contains(point)) {
                    sharedPoints++;
                    if (sharedPoints >= 2) {
                        neighbors.Add(otherTriangle);
                        break;
                    }
                }
            }
        }
        
        return neighbors;
    }
    
    // === NOUVELLE MÉTHODE DE DÉTECTION BASÉE SUR LE NIVEAU DE SUBDIVISION ===
    
    bool ShouldTriangleBeSubdivided(Triangle triangle) {
        // Analyser la complexité du triangle pour déterminer s'il devrait être subdivisé
        float complexity = CalculateTriangleComplexity(triangle);
        
        // Vérifier si le triangle est dans une zone qui nécessite plus de subdivision
        bool needsMoreSubdivision = complexity > complexityThreshold;
        
        if (showBoundaryDebug && needsMoreSubdivision) {
            Debug.Log($"🔍 Triangle complexe détecté (complexité: {complexity:F3})");
        }
        
        return needsMoreSubdivision;
    }
    
    float CalculateTriangleComplexity(Triangle triangle) {
        // Calculer la complexité du triangle basée sur plusieurs facteurs
        
        // 1. Variation d'altitude dans le triangle
        float altitudeVariation = CalculateAltitudeVariation(triangle);
        
        // 2. Variation de type de terrain dans le triangle
        float terrainVariation = CalculateTerrainVariation(triangle);
        
        // 3. Taille du triangle (plus petit = plus complexe)
        float sizeComplexity = CalculateSizeComplexity(triangle);
        
        // 4. Proximité aux frontières de matériaux
        float boundaryProximity = CalculateBoundaryProximity(triangle);
        
        // Normaliser les poids pour qu'ils totalisent 1.0
        float totalWeight = altitudeWeight + terrainWeight + sizeWeight + boundaryWeight;
        float normalizedAltitudeWeight = altitudeWeight / totalWeight;
        float normalizedTerrainWeight = terrainWeight / totalWeight;
        float normalizedSizeWeight = sizeWeight / totalWeight;
        float normalizedBoundaryWeight = boundaryWeight / totalWeight;
        
        // Combiner les facteurs avec des poids configurables
        float totalComplexity = 
            altitudeVariation * normalizedAltitudeWeight +
            terrainVariation * normalizedTerrainWeight +
            sizeComplexity * normalizedSizeWeight +
            boundaryProximity * normalizedBoundaryWeight;
        
        if (showBoundaryDebug) {
            Debug.Log($"🔍 Complexité triangle: Alt={altitudeVariation:F3}*{normalizedAltitudeWeight:F2} + " +
                     $"Ter={terrainVariation:F3}*{normalizedTerrainWeight:F2} + " +
                     $"Size={sizeComplexity:F3}*{normalizedSizeWeight:F2} + " +
                     $"Bound={boundaryProximity:F3}*{normalizedBoundaryWeight:F2} = {totalComplexity:F3}");
        }
        
        return totalComplexity;
    }
    
    float CalculateAltitudeVariation(Triangle triangle) {
        // Calculer la variation d'altitude entre les points du triangle
        Vector3 v1 = triangle.points[0].ToVector3() * radius;
        Vector3 v2 = triangle.points[1].ToVector3() * radius;
        Vector3 v3 = triangle.points[2].ToVector3() * radius;
        
        float h1 = GetVertexHeight(v1);
        float h2 = GetVertexHeight(v2);
        float h3 = GetVertexHeight(v3);
        
        // Calculer la variance des hauteurs
        float avgHeight = (h1 + h2 + h3) / 3f;
        float variance = Mathf.Pow(h1 - avgHeight, 2) + Mathf.Pow(h2 - avgHeight, 2) + Mathf.Pow(h3 - avgHeight, 2);
        variance /= 3f;
        
        return Mathf.Sqrt(variance);
    }
    
    float CalculateTerrainVariation(Triangle triangle) {
        // Calculer la variation de type de terrain dans le triangle
        Vector3 v1 = triangle.points[0].ToVector3() * radius;
        Vector3 v2 = triangle.points[1].ToVector3() * radius;
        Vector3 v3 = triangle.points[2].ToVector3() * radius;
        
        TerrainType t1 = GetTerrainTypeForVertex(v1);
        TerrainType t2 = GetTerrainTypeForVertex(v2);
        TerrainType t3 = GetTerrainTypeForVertex(v3);
        
        // Compter les types différents
        int differentTypes = 0;
        if (t1 != t2) differentTypes++;
        if (t2 != t3) differentTypes++;
        if (t1 != t3) differentTypes++;
        
        return differentTypes / 3f; // Normaliser entre 0 et 1
    }
    
    float CalculateSizeComplexity(Triangle triangle) {
        // Calculer la complexité basée sur la taille du triangle
        Vector3 v1 = triangle.points[0].ToVector3();
        Vector3 v2 = triangle.points[1].ToVector3();
        Vector3 v3 = triangle.points[2].ToVector3();
        
        // Calculer l'aire du triangle
        float area = Vector3.Cross(v2 - v1, v3 - v1).magnitude / 2f;
        
        // Plus l'aire est grande, plus le triangle devrait être subdivisé
        return Mathf.Clamp01(area * 10f); // Normaliser
    }
    
    float CalculateBoundaryProximity(Triangle triangle) {
        // Calculer la proximité aux frontières de matériaux
        Vector3 center = (triangle.points[0].ToVector3() + triangle.points[1].ToVector3() + triangle.points[2].ToVector3()) / 3f;
        
        // Chercher des triangles voisins avec des types différents
        List<Triangle> neighbors = useOptimizedNeighborDetection ? 
            FindTriangleNeighborsOptimized(triangle) : 
            FindTriangleNeighbors(triangle);
        
        int differentNeighbors = 0;
        TerrainType triangleType = triangleTerrainTypes[triangle];
        
        foreach (var neighbor in neighbors) {
            if (triangleTerrainTypes.ContainsKey(neighbor)) {
                TerrainType neighborType = triangleTerrainTypes[neighbor];
                if (neighborType != triangleType) {
                    differentNeighbors++;
                }
            }
        }
        
        return Mathf.Clamp01(differentNeighbors / 3f); // Normaliser
    }
    
    TerrainType GetTerrainTypeForVertex(Vector3 vertex) {
        // Déterminer le type de terrain pour un vertex
        float height = GetVertexHeight(vertex);
        
        if (height <= waterLevel) {
            return TerrainType.Water;
        } else if (height <= mountainLevel) {
            return TerrainType.Land;
        } else {
            return TerrainType.Mountain;
        }
    }
    
    void SubdivideBoundaryTriangles() {
        if (maxBoundarySubdivisions <= 0) {
            if (showBoundaryDebug) {
                Debug.Log("⚠️ Aucune subdivision supplémentaire demandée pour les frontières");
            }
            return;
        }
        
        int actualSubdivisions = 0;
        
        // Appliquer les subdivisions supplémentaires pour les triangles frontières
        for (int subdivisionLevel = 0; subdivisionLevel < maxBoundarySubdivisions; subdivisionLevel++) {
            List<Triangle> newTriangles = new List<Triangle>();
            List<Triangle> newBoundaryTriangles = new List<Triangle>();
            
            // Re-détecter les triangles frontières à chaque niveau
            DetectBoundaryTriangles();
            
            if (boundaryTriangles.Count == 0) {
                if (showBoundaryDebug) {
                    Debug.Log($"✅ Plus de triangles frontières détectés, arrêt à la subdivision {subdivisionLevel}");
                }
                break;
            }
            
            foreach (var triangle in triangles) {
                if (boundaryTriangles.Contains(triangle)) {
                    // Subdiviser le triangle frontière
                    Point mid1 = GetCachedPoint(Point.Midpoint(triangle.points[0], triangle.points[1]).Normalized);
                    Point mid2 = GetCachedPoint(Point.Midpoint(triangle.points[1], triangle.points[2]).Normalized);
                    Point mid3 = GetCachedPoint(Point.Midpoint(triangle.points[2], triangle.points[0]).Normalized);
                    
                    Triangle sub1 = new Triangle(triangle.points[0], mid1, mid3);
                    Triangle sub2 = new Triangle(triangle.points[1], mid2, mid1);
                    Triangle sub3 = new Triangle(triangle.points[2], mid3, mid2);
                    Triangle sub4 = new Triangle(mid1, mid2, mid3);
                    
                    newTriangles.Add(sub1);
                    newTriangles.Add(sub2);
                    newTriangles.Add(sub3);
                    newTriangles.Add(sub4);
                    
                    if (showBoundaryDebug) {
                        Debug.Log($"🔧 Triangle frontière subdivisé (niveau {subdivisionLevel + 1})");
                    }
                } else {
                    // Garder le triangle tel quel
                    newTriangles.Add(triangle);
                }
            }
            
            triangles = newTriangles;
            actualSubdivisions++;
            
            if (showBoundaryDebug) {
                Debug.Log($"📊 Niveau {subdivisionLevel + 1} terminé - {triangles.Count} triangles, {boundaryTriangles.Count} frontières restantes");
            }
        }
        
        if (showBoundaryDebug) {
            int totalSubdivisions = backgroundDivisions + focusDivisions + actualSubdivisions;
            Debug.Log($"✅ Subdivision des frontières terminée - {triangles.Count} triangles finaux");
            Debug.Log($"📊 Subdivisions appliquées: {actualSubdivisions}/{maxBoundarySubdivisions} niveaux de frontières");
            Debug.Log($"📊 Total: {totalSubdivisions} subdivisions (Base: {backgroundDivisions} + Focus: {focusDivisions} + Frontières: {actualSubdivisions})");
        }
    }
    
    void DisplaySubdivisionInfo() {
        int totalSubdivisions = backgroundDivisions + focusDivisions;
        if (useMaterialBoundarySubdivision) {
            totalSubdivisions += maxBoundarySubdivisions;
        }
        
        Debug.Log($"📊 INFORMATIONS DE SUBDIVISION:");
        Debug.Log($"   • Subdivisions de base: {backgroundDivisions}");
        Debug.Log($"   • Subdivisions de focus: {focusDivisions}");
        if (useMaterialBoundarySubdivision) {
            Debug.Log($"   • Subdivisions frontières: {maxBoundarySubdivisions}");
        }
        Debug.Log($"   • Total: {totalSubdivisions} subdivisions");
        Debug.Log($"   • Triangles finaux: {triangles.Count}");
        Debug.Log($"   • Points finaux: {points.Count}");
        
        if (useMaterialBoundarySubdivision && boundaryTriangles.Count > 0) {
            Debug.Log($"   • Triangles frontières détectés: {boundaryTriangles.Count}");
        }
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
    
    void GenerateMeshWithIcosahedronChunking() {
        // Initialiser les chunks par face
        InitializeIcosahedronChunks();
        
        // Créer un dictionnaire pour mapper les points vers les indices de vertices
        Dictionary<Point, int> pointToIndex = new Dictionary<Point, int>();
        int vertexIndex = 0;
        
        // Ajouter tous les points comme vertices dans le premier chunk
        foreach (var point in points.Values) {
            faceVerticesChunks[0].Add(point.ToVector3() * radius);
            
            if (useSphericalUVs) {
                Vector3 pos = point.ToVector3();
                float u = 0.5f + Mathf.Atan2(pos.z, pos.x) / (2f * Mathf.PI);
                float v = 0.5f - Mathf.Asin(pos.y) / Mathf.PI;
                faceUvsChunks[0].Add(new Vector2(u, v));
            } else {
                faceUvsChunks[0].Add(new Vector2(0.5f, 0.5f));
            }
            
            pointToIndex[point] = vertexIndex++;
        }
        
        // Appliquer la génération procédurale AVANT de créer les meshes
        if (useProceduralGeneration) {
            ApplyProceduralGenerationToAllChunks();
        }
        
        // Distribuer les triangles selon leur face d'origine
        DistributeTrianglesToFaceChunks(pointToIndex);
        
        // Créer les meshes des chunks par face
        CreateFaceChunkMeshes();
        
        // Désactiver le mesh principal quand on utilise le système de chunks
        if (meshFilter != null) {
            meshFilter.mesh = null;
        }
        if (meshRenderer != null) {
            meshRenderer.enabled = false;
        }
        
        // Supprimer le collider du mesh principal pour éviter les conflits
        MeshCollider mainCollider = GetComponent<MeshCollider>();
        if (mainCollider != null) {
            DestroyImmediate(mainCollider);
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
    
    void InitializeIcosahedronChunks() {
        // Initialiser les 20 chunks (une par face de l'icosaèdre)
        int faceCount = 20;
        
        faceVerticesChunks = new List<Vector3>[faceCount];
        faceTrianglesChunks = new List<int>[faceCount];
        faceUvsChunks = new List<Vector2>[faceCount];
        faceMeshChunks = new Mesh[faceCount];
        faceMeshFilterChunks = new MeshFilter[faceCount];
        faceMeshRendererChunks = new MeshRenderer[faceCount];
        faceChunkObjects = new GameObject[faceCount];
        faceChunkTriangleCounts = new int[faceCount];
        faceChunkActive = new bool[faceCount];
        
        // Initialiser les sous-chunks pour la protection
        faceSubVerticesChunks = new List<List<Vector3>>[faceCount];
        faceSubTrianglesChunks = new List<List<int>>[faceCount];
        faceSubUvsChunks = new List<List<Vector2>>[faceCount];
        faceSubMeshChunks = new List<Mesh>[faceCount];
        faceSubMeshFilterChunks = new List<MeshFilter>[faceCount];
        faceSubMeshRendererChunks = new List<MeshRenderer>[faceCount];
        faceSubChunkObjects = new List<GameObject>[faceCount];
        faceSubChunkCounts = new int[faceCount];
        
        for (int i = 0; i < faceCount; i++) {
            faceVerticesChunks[i] = new List<Vector3>();
            faceTrianglesChunks[i] = new List<int>();
            faceUvsChunks[i] = new List<Vector2>();
            faceChunkTriangleCounts[i] = 0;
            faceChunkActive[i] = true;
            
            // Initialiser les sous-chunks
            faceSubVerticesChunks[i] = new List<List<Vector3>>();
            faceSubTrianglesChunks[i] = new List<List<int>>();
            faceSubUvsChunks[i] = new List<List<Vector2>>();
            faceSubMeshChunks[i] = new List<Mesh>();
            faceSubMeshFilterChunks[i] = new List<MeshFilter>();
            faceSubMeshRendererChunks[i] = new List<MeshRenderer>();
            faceSubChunkObjects[i] = new List<GameObject>();
            faceSubChunkCounts[i] = 0;
            
            // Initialiser les couleurs des chunks si pas déjà fait
            if (chunkColors[i] == Color.clear) {
                chunkColors[i] = Color.HSVToRGB(i / 20f, 0.8f, 0.8f);
            }
        }
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
    
    void DistributeTrianglesToFaceChunks(Dictionary<Point, int> pointToIndex) {
        // Créer une liste des faces originales de l'icosaèdre
        List<Triangle> originalFaces = GetOriginalIcosahedronFaces();
        
        // Pour chaque triangle, déterminer à quelle face d'origine il appartient
        foreach (var triangle in this.triangles) {
            int faceIndex = FindOriginalFaceForTriangle(triangle, originalFaces);
            
            if (faceIndex >= 0 && faceIndex < 20) {
                // Ajouter le triangle au chunk correspondant
                if (pointToIndex.ContainsKey(triangle.points[0]) &&
                    pointToIndex.ContainsKey(triangle.points[1]) &&
                    pointToIndex.ContainsKey(triangle.points[2])) {
                    
                    if (fixTriangleOrientation) {
                        // Vérifier l'orientation du triangle
                        Vector3 v0 = faceVerticesChunks[0][pointToIndex[triangle.points[0]]];
                        Vector3 v1 = faceVerticesChunks[0][pointToIndex[triangle.points[1]]];
                        Vector3 v2 = faceVerticesChunks[0][pointToIndex[triangle.points[2]]];
                        
                        Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                        Vector3 center = (v0 + v1 + v2) / 3f;
                        
                        if (Vector3.Dot(normal, center) < 0) {
                            faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[0]]);
                            faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[2]]);
                            faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[1]]);
                        } else {
                            faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[0]]);
                            faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[1]]);
                            faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[2]]);
                        }
                    } else {
                        faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[0]]);
                        faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[1]]);
                        faceTrianglesChunks[faceIndex].Add(pointToIndex[triangle.points[2]]);
                    }
                    
                    faceChunkTriangleCounts[faceIndex]++;
                }
            }
        }
        
        // Vérifier et diviser les chunks trop gros si nécessaire
        if (useChunkProtection && autoSplitLargeChunks) {
            SplitOversizedFaceChunks();
        } else {
            Debug.LogWarning("⚠️ Protection des chunks désactivée! useChunkProtection=" + useChunkProtection + ", autoSplitLargeChunks=" + autoSplitLargeChunks);
        }
    }
    
    List<Triangle> GetOriginalIcosahedronFaces() {
        List<Triangle> originalFaces = new List<Triangle>();
        
        // Créer les 20 faces originales de l'icosaèdre
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        
        Point[] vertices = new Point[12];
        vertices[0] = new Point(-1, t, 0).Normalized;
        vertices[1] = new Point(1, t, 0).Normalized;
        vertices[2] = new Point(-1, -t, 0).Normalized;
        vertices[3] = new Point(1, -t, 0).Normalized;
        vertices[4] = new Point(0, -1, t).Normalized;
        vertices[5] = new Point(0, 1, t).Normalized;
        vertices[6] = new Point(0, -1, -t).Normalized;
        vertices[7] = new Point(0, 1, -t).Normalized;
        vertices[8] = new Point(t, 0, -1).Normalized;
        vertices[9] = new Point(t, 0, 1).Normalized;
        vertices[10] = new Point(-t, 0, -1).Normalized;
        vertices[11] = new Point(-t, 0, 1).Normalized;
        
        int[] indices = {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };
        
        for (int i = 0; i < indices.Length; i += 3) {
            originalFaces.Add(new Triangle(
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            ));
        }
        
        return originalFaces;
    }
    
    int FindOriginalFaceForTriangle(Triangle triangle, List<Triangle> originalFaces) {
        // Calculer le centre du triangle
        Vector3 center = (triangle.points[0].ToVector3() + triangle.points[1].ToVector3() + triangle.points[2].ToVector3()) / 3f;
        center = center.normalized;
        
        // Trouver la face originale la plus proche
        float minDistance = float.MaxValue;
        int closestFaceIndex = 0;
        
        for (int i = 0; i < originalFaces.Count; i++) {
            Vector3 faceCenter = (originalFaces[i].points[0].ToVector3() + 
                                 originalFaces[i].points[1].ToVector3() + 
                                 originalFaces[i].points[2].ToVector3()) / 3f;
            faceCenter = faceCenter.normalized;
            
            float distance = Vector3.Distance(center, faceCenter);
            if (distance < minDistance) {
                minDistance = distance;
                closestFaceIndex = i;
            }
        }
        
        return closestFaceIndex;
    }
    
    void SplitOversizedFaceChunks() {
        
        for (int faceIndex = 0; faceIndex < 20; faceIndex++) {
            int triangleCount = faceTrianglesChunks[faceIndex].Count;
            int vertexCount = faceVerticesChunks[0].Count; // Tous les vertices sont partagés
            
            
            // Vérifier si le chunk dépasse les limites (triangles OU vertices)
            bool exceedsLimits = triangleCount > maxTrianglesPerFaceChunk || vertexCount > maxVerticesPerFaceChunk;
            
            if (exceedsLimits) {
                Debug.Log($"⚠️ Chunk {faceIndex} dépasse la limite! Triangles: {triangleCount}/{maxTrianglesPerFaceChunk}, Vertices: {vertexCount}/{maxVerticesPerFaceChunk}");
                
                if (faceSubChunkCounts[faceIndex] < maxSubChunksPerFace) {
                    // Calculer le nombre de sous-chunks nécessaires basé sur la limite la plus restrictive
                    int trianglesNeeded = (triangleCount / maxTrianglesPerFaceChunk) + 1;
                    int verticesNeeded = (vertexCount / maxVerticesPerFaceChunk) + 1;
                    int subChunkCount = Mathf.Min(maxSubChunksPerFace, Mathf.Max(trianglesNeeded, verticesNeeded));
                    
                    // Pour les très gros chunks, être ultra agressif
                    if (triangleCount > 25000 || vertexCount > 12500) {
                        subChunkCount = Mathf.Max(subChunkCount, 16); // Minimum 16 sous-chunks pour les gros chunks
                    }
                    if (triangleCount > 50000 || vertexCount > 25000) {
                        subChunkCount = Mathf.Max(subChunkCount, 32); // Minimum 32 sous-chunks pour les très gros chunks
                    }
                    if (triangleCount > 100000 || vertexCount > 50000) {
                        subChunkCount = Mathf.Max(subChunkCount, 48); // Minimum 48 sous-chunks pour les énormes chunks
                    }
                    
                    
                    // Créer les sous-chunks
                    for (int subIndex = 0; subIndex < subChunkCount; subIndex++) {
                        faceSubVerticesChunks[faceIndex].Add(new List<Vector3>());
                        faceSubTrianglesChunks[faceIndex].Add(new List<int>());
                        faceSubUvsChunks[faceIndex].Add(new List<Vector2>());
                    }
                    
                    // Distribuer les triangles entre les sous-chunks de manière équitable
                    int trianglesPerSubChunk = triangleCount / subChunkCount;
                    int remainder = triangleCount % subChunkCount;
                    
                    int currentTriangle = 0;
                    for (int subIndex = 0; subIndex < subChunkCount; subIndex++) {
                        // Calculer le nombre de triangles pour ce sous-chunk
                        int trianglesForThisSubChunk = trianglesPerSubChunk;
                        if (subIndex < remainder) {
                            trianglesForThisSubChunk++; // Distribuer le reste équitablement
                        }
                        
                        Debug.Log($"🔧 Sous-chunk {faceIndex}.{subIndex} recevra {trianglesForThisSubChunk} triangles");
                        
                        // Ajouter les triangles à ce sous-chunk
                        for (int t = 0; t < trianglesForThisSubChunk; t++) {
                            if (currentTriangle < triangleCount) {
                                faceSubTrianglesChunks[faceIndex][subIndex].Add(faceTrianglesChunks[faceIndex][currentTriangle]);
                                faceSubTrianglesChunks[faceIndex][subIndex].Add(faceTrianglesChunks[faceIndex][currentTriangle + 1]);
                                faceSubTrianglesChunks[faceIndex][subIndex].Add(faceTrianglesChunks[faceIndex][currentTriangle + 2]);
                                currentTriangle += 3;
                            }
                        }
                    }
                    
                    // Copier seulement les vertices utilisés par chaque sous-chunk
                    for (int subIndex = 0; subIndex < subChunkCount; subIndex++) {
                        // Créer un dictionnaire pour mapper les indices de vertices
                        Dictionary<int, int> vertexIndexMap = new Dictionary<int, int>();
                        int newVertexIndex = 0;
                        
                        // Parcourir les triangles de ce sous-chunk pour identifier les vertices utilisés
                        for (int i = 0; i < faceSubTrianglesChunks[faceIndex][subIndex].Count; i++) {
                            int originalIndex = faceSubTrianglesChunks[faceIndex][subIndex][i];
                            
                            if (!vertexIndexMap.ContainsKey(originalIndex)) {
                                // Ajouter le vertex s'il n'est pas déjà dans la liste
                                // Utiliser les vertices avec génération procédurale appliquée
                                faceSubVerticesChunks[faceIndex][subIndex].Add(faceVerticesChunks[0][originalIndex]);
                                faceSubUvsChunks[faceIndex][subIndex].Add(faceUvsChunks[0][originalIndex]);
                                vertexIndexMap[originalIndex] = newVertexIndex++;
                            }
                            
                            // Mettre à jour l'index du triangle
                            faceSubTrianglesChunks[faceIndex][subIndex][i] = vertexIndexMap[originalIndex];
                        }
                        
                    }
                    
                    // Vider le chunk principal
                    faceTrianglesChunks[faceIndex].Clear();
                    faceChunkTriangleCounts[faceIndex] = 0;
                    faceSubChunkCounts[faceIndex] = subChunkCount;
                    
                    Debug.Log($"✅ Chunk {faceIndex} divisé en {subChunkCount} sous-chunks ({triangleCount} triangles)");
                } else {
                    Debug.LogError($"❌ Impossible de diviser le chunk {faceIndex} - limite de sous-chunks atteinte!");
                }
            }
        }
    }
    
    void CreateFaceChunkMeshes() {
        for (int i = 0; i < 20; i++) {
            // Vérifier s'il y a des sous-chunks pour cette face
            if (faceSubChunkCounts[i] > 0) {
                // Créer les sous-chunks
                CreateSubChunkMeshes(i);
            } else if (faceTrianglesChunks[i].Count > 0) {
                // Créer le chunk principal normal
                CreateSingleFaceChunkMesh(i);
            }
        }
    }
    
    void CreateSingleFaceChunkMesh(int faceIndex) {
        // Vérifier les limites avant de créer le mesh
        int triangleCount = faceTrianglesChunks[faceIndex].Count;
        int vertexCount = faceVerticesChunks[0].Count;
        
        if (triangleCount > maxTrianglesPerFaceChunk || vertexCount > maxVerticesPerFaceChunk) {
            Debug.LogError($"❌ Chunk {faceIndex} dépasse encore les limites après division! Triangles: {triangleCount}/{maxTrianglesPerFaceChunk}, Vertices: {vertexCount}/{maxVerticesPerFaceChunk}");
            Debug.LogError($"💡 Ce chunk devrait être divisé en sous-chunks. Vérifiez que useChunkProtection et autoSplitLargeChunks sont activés.");
            return;
        }
        
        
        // Créer le GameObject du chunk par face
        GameObject chunkObject = new GameObject($"Icosahedron Face Chunk {faceIndex}");
        chunkObject.transform.SetParent(transform);
        chunkObject.transform.localPosition = Vector3.zero;
        chunkObject.transform.localRotation = Quaternion.identity;
        chunkObject.transform.localScale = Vector3.one;
        chunkObject.tag = "Planet";
        
        // Ajouter les composants
        faceMeshFilterChunks[faceIndex] = chunkObject.AddComponent<MeshFilter>();
        faceMeshRendererChunks[faceIndex] = chunkObject.AddComponent<MeshRenderer>();
        faceChunkObjects[faceIndex] = chunkObject;
        
        // Créer le mesh
        faceMeshChunks[faceIndex] = new Mesh();
        faceMeshChunks[faceIndex].name = $"Icosahedron Face Chunk {faceIndex} Mesh";
        faceMeshChunks[faceIndex].vertices = faceVerticesChunks[0].ToArray(); // Tous les vertices sont partagés
        faceMeshChunks[faceIndex].triangles = faceTrianglesChunks[faceIndex].ToArray();
        faceMeshChunks[faceIndex].uv = faceUvsChunks[0].ToArray(); // Tous les UVs sont partagés
        faceMeshChunks[faceIndex].RecalculateNormals();
        faceMeshChunks[faceIndex].RecalculateBounds();
        
        // Assigner le mesh
        faceMeshFilterChunks[faceIndex].mesh = faceMeshChunks[faceIndex];
        
        // Appliquer la génération procédurale si activée
        if (useProceduralGeneration) {
            CreateProceduralFaceChunkMesh(faceIndex);
        } else {
            // Configurer le matériau avec couleur de debug
            Material chunkMaterial = new Material(Shader.Find("Standard"));
            if (showChunkDebug) {
                chunkMaterial.color = chunkColors[faceIndex];
            } else {
                chunkMaterial.color = hexagonColor;
            }
            faceMeshRendererChunks[faceIndex].material = chunkMaterial;
        }
        
        // Ajouter un collider
        MeshCollider collider = chunkObject.AddComponent<MeshCollider>();
        collider.sharedMesh = faceMeshChunks[faceIndex];
        collider.convex = false;
    }
    
    void CreateSubChunkMeshes(int faceIndex) {
        // Créer les sous-chunks pour cette face
        for (int subIndex = 0; subIndex < faceSubChunkCounts[faceIndex]; subIndex++) {
            if (faceSubTrianglesChunks[faceIndex][subIndex].Count > 0) {
                // Vérifier les limites du sous-chunk
                int triangleCount = faceSubTrianglesChunks[faceIndex][subIndex].Count;
                int vertexCount = faceSubVerticesChunks[faceIndex][subIndex].Count;
                
                if (triangleCount > maxTrianglesPerFaceChunk || vertexCount > maxVerticesPerFaceChunk) {
                    Debug.LogError($"❌ Sous-chunk {faceIndex}.{subIndex} dépasse les limites! Triangles: {triangleCount}/{maxTrianglesPerFaceChunk}, Vertices: {vertexCount}/{maxVerticesPerFaceChunk}");
                    Debug.LogError($"💡 Le sous-chunk a encore trop de vertices. Le système de division doit être amélioré.");
                    continue;
                }
                
                
                // Créer le GameObject du sous-chunk
                GameObject subChunkObject = new GameObject($"Icosahedron Face {faceIndex} SubChunk {subIndex}");
                subChunkObject.transform.SetParent(transform);
                subChunkObject.transform.localPosition = Vector3.zero;
                subChunkObject.transform.localRotation = Quaternion.identity;
                subChunkObject.transform.localScale = Vector3.one;
                subChunkObject.tag = "Planet";
                
                // Ajouter les composants
                MeshFilter subMeshFilter = subChunkObject.AddComponent<MeshFilter>();
                MeshRenderer subMeshRenderer = subChunkObject.AddComponent<MeshRenderer>();
                
                // Stocker les références
                faceSubMeshFilterChunks[faceIndex].Add(subMeshFilter);
                faceSubMeshRendererChunks[faceIndex].Add(subMeshRenderer);
                faceSubChunkObjects[faceIndex].Add(subChunkObject);
                
                // Créer le mesh du sous-chunk
                Mesh subMesh = new Mesh();
                subMesh.name = $"Icosahedron Face {faceIndex} SubChunk {subIndex} Mesh";
                subMesh.vertices = faceSubVerticesChunks[faceIndex][subIndex].ToArray();
                subMesh.triangles = faceSubTrianglesChunks[faceIndex][subIndex].ToArray();
                subMesh.uv = faceSubUvsChunks[faceIndex][subIndex].ToArray();
                subMesh.RecalculateNormals();
                subMesh.RecalculateBounds();
                
                // Stocker le mesh
                faceSubMeshChunks[faceIndex].Add(subMesh);
                
                // Appliquer la génération procédurale si activée
                if (useProceduralGeneration) {
                    CreateProceduralSubChunkMesh(faceIndex, subIndex);
                } else {
                    // Assigner le mesh simple
                    subMeshFilter.mesh = subMesh;
                    
                    // Configurer le matériau avec couleur de debug
                    Material chunkMaterial = new Material(Shader.Find("Standard"));
                    if (showChunkDebug) {
                        chunkMaterial.color = chunkColors[faceIndex];
                    } else {
                        chunkMaterial.color = hexagonColor;
                    }
                    subMeshRenderer.material = chunkMaterial;
                }
                
                // Ajouter un collider
                MeshCollider collider = subChunkObject.AddComponent<MeshCollider>();
                collider.sharedMesh = subMesh;
                collider.convex = false;
            }
        }
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
        // Nettoyer les anciens chunks par face
        if (faceChunkObjects != null) {
            for (int i = 0; i < faceChunkObjects.Length; i++) {
                if (faceChunkObjects[i] != null) {
                    DestroyImmediate(faceChunkObjects[i]);
                }
            }
        }
        
        // Nettoyer les sous-chunks
        if (faceSubChunkObjects != null) {
            for (int i = 0; i < faceSubChunkObjects.Length; i++) {
                if (faceSubChunkObjects[i] != null) {
                    for (int j = 0; j < faceSubChunkObjects[i].Count; j++) {
                        if (faceSubChunkObjects[i][j] != null) {
                            DestroyImmediate(faceSubChunkObjects[i][j]);
                        }
                    }
                }
            }
        }
        
        // Nettoyer les anciens chunks normaux
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
        
        // Supprimer le collider principal pour éviter les conflits avec les chunks
        MeshCollider mainCollider = GetComponent<MeshCollider>();
        if (mainCollider != null) {
            DestroyImmediate(mainCollider);
        }
        
        // Nettoyer TOUS les enfants (chunks et autres)
        List<Transform> childrenToDestroy = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++) {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Hexasphere Chunk") || 
                child.name.Contains("Icosahedron Face Chunk") ||
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
        
        // Réinitialiser les variables de subdivision par frontières
        boundaryTriangles.Clear();
        triangleTerrainTypes.Clear();
        
        // Vider le cache des hauteurs pour forcer le recalcul
        ClearVertexHeightCache();
        
        // Réinitialiser les variables
        chunkCount = 0;
        
        // Réinitialiser les tableaux de chunks par face
        faceVerticesChunks = null;
        faceTrianglesChunks = null;
        faceUvsChunks = null;
        faceMeshChunks = null;
        faceMeshFilterChunks = null;
        faceMeshRendererChunks = null;
        faceChunkObjects = null;
        faceChunkTriangleCounts = null;
        faceChunkActive = null;
        
        // Réinitialiser les sous-chunks
        faceSubVerticesChunks = null;
        faceSubTrianglesChunks = null;
        faceSubUvsChunks = null;
        faceSubMeshChunks = null;
        faceSubMeshFilterChunks = null;
        faceSubMeshRendererChunks = null;
        faceSubChunkObjects = null;
        faceSubChunkCounts = null;
        
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
        
        if (showBoundaryDebug && useMaterialBoundarySubdivision) {
            DrawBoundaryDebug();
        }
    }
    
    void OnDrawGizmosSelected() {
        // Dessiner le point de focus dans la scène
        if (showFocusPoint && useSelectiveSubdivision) {
            DrawFocusPointGizmo();
        }
        
        // Dessiner les gizmos de debug des chunks par face
        if (showChunkDebug && useIcosahedronChunking && faceChunkObjects != null) {
            DrawChunkDebugGizmos();
        }
    }
    
    void DrawChunkDebugGizmos() {
        for (int i = 0; i < faceChunkObjects.Length; i++) {
            if (faceChunkObjects[i] != null && faceChunkActive[i]) {
                // Dessiner un gizmo pour chaque chunk actif
                Gizmos.color = chunkColors[i];
                Gizmos.DrawWireSphere(faceChunkObjects[i].transform.position, 0.1f);
                
                // Afficher le nombre de triangles
                if (faceChunkTriangleCounts[i] > 0) {
                    Vector3 labelPos = faceChunkObjects[i].transform.position + Vector3.up * 0.2f;
                    UnityEditor.Handles.Label(labelPos, $"Chunk {i}: {faceChunkTriangleCounts[i]} triangles");
                }
            }
            
            // Dessiner les sous-chunks s'ils existent
            if (faceSubChunkObjects[i] != null && faceSubChunkObjects[i].Count > 0) {
                for (int j = 0; j < faceSubChunkObjects[i].Count; j++) {
                    if (faceSubChunkObjects[i][j] != null) {
                        // Dessiner un gizmo plus petit pour les sous-chunks
                        Gizmos.color = new Color(chunkColors[i].r, chunkColors[i].g, chunkColors[i].b, 0.5f);
                        Gizmos.DrawWireSphere(faceSubChunkObjects[i][j].transform.position, 0.05f);
                        
                        // Afficher les informations du sous-chunk
                        Vector3 subLabelPos = faceSubChunkObjects[i][j].transform.position + Vector3.up * 0.15f;
                        UnityEditor.Handles.Label(subLabelPos, $"SubChunk {i}.{j}");
                    }
                }
            }
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
    
    void DrawBoundaryDebug() {
        // Dessiner les triangles frontières
        Gizmos.color = boundaryTriangleColor;
        int triangleCount = 0;
        
        // Si on affiche sur toute la planète, analyser tous les triangles
        if (applyToEntirePlanet) {
            // Re-détecter les triangles frontières pour l'affichage
            DetectBoundaryTriangles();
        }
        
        foreach (var triangle in boundaryTriangles) {
            if (triangleCount > 200) break; // Limiter pour la performance (augmenté pour toute la planète)
            
            Vector3 v0 = transform.TransformPoint(triangle.points[0].ToVector3() * radius);
            Vector3 v1 = transform.TransformPoint(triangle.points[1].ToVector3() * radius);
            Vector3 v2 = transform.TransformPoint(triangle.points[2].ToVector3() * radius);
            
            // Dessiner le triangle avec des lignes plus épaisses
            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v0);
            
            // Dessiner le centre du triangle
            Vector3 center = (v0 + v1 + v2) / 3f;
            Gizmos.DrawWireSphere(center, 0.02f);
            
            triangleCount++;
        }
        
        if (showBoundaryDebug) {
            string scope = applyToEntirePlanet ? "sur toute la planète" : "dans la zone de focus";
            Debug.Log($"🎨 {triangleCount} triangles frontières affichés {scope}");
        }
    }
    
    // === GÉNÉRATION PROCÉDURALE DE PLANÈTE ===
    
    float GenerateHeight(Vector3 position) {
        // Utiliser uniquement le système 3D (plus performant et naturel)
        float height = Generate3DHeight(position);
        
        // Appliquer l'amplitude de base et le multiplicateur global
        return height * heightAmplitude * heightMultiplier;
    }
    
    float GetEffectiveHeight(Vector3 normalizedVertex) {
        float height = GenerateHeight(normalizedVertex);
        
        // Ajuster la hauteur proportionnellement au radius
        if (autoScaleHeightWithRadius && radius > 1f) {
            // Utiliser un ajustement proportionnel configurable
            float scaleFactor = Mathf.Pow(radius, heightScalePower);
            height *= scaleFactor;
        }
        
        return height;
    }
    
    public float GetEffectiveWaterLevel() {
        if (autoScaleTerrainLevels && autoScaleHeightWithRadius && radius > 1f) {
            float scaleFactor = Mathf.Pow(radius, heightScalePower);
            return waterLevel * scaleFactor;
        }
        return waterLevel;
    }
    
    public float GetEffectiveMountainLevel() {
        if (autoScaleTerrainLevels && autoScaleHeightWithRadius && radius > 1f) {
            float scaleFactor = Mathf.Pow(radius, heightScalePower);
            return mountainLevel * scaleFactor;
        }
        return mountainLevel;
    }
    
    
    
    float Generate3DHeight(Vector3 position) {
        // Utiliser directement les coordonnées 3D pour éviter les coupures
        Vector3 scaledPosition = position * noise3DScale;
        
        // Générer le bruit fractal 3D
        float baseHeight = Generate3DFractalNoise(scaledPosition, noise3DOctaves, noise3DPersistence, noise3DLacunarity);
        
        // Ajouter le bruit de ridges 3D si activé
        float ridgeHeight = 0f;
        if (use3DRidgeNoise) {
            ridgeHeight = Generate3DRidgeNoise(scaledPosition, ridge3DScale) * ridge3DIntensity;
        }
        
        // Combiner les bruits
        float totalHeight = (baseHeight + ridgeHeight);
        
        return totalHeight;
    }
    
    float Generate3DFractalNoise(Vector3 position, int octaves, float persistence, float lacunarity) {
        float height = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;
        
        // Ajouter des offsets pour éviter la symétrie
        Vector3 offset1 = new Vector3(12.34f, 56.78f, 90.12f);
        Vector3 offset2 = new Vector3(34.56f, 78.90f, 12.34f);
        Vector3 offset3 = new Vector3(56.78f, 90.12f, 34.56f);
        
        for (int i = 0; i < octaves; i++) {
            // Utiliser des combinaisons asymétriques pour éviter la symétrie
            Vector3 pos1 = (position + offset1) * frequency;
            Vector3 pos2 = (position + offset2) * frequency;
            Vector3 pos3 = (position + offset3) * frequency;
            
            float noise1 = Mathf.PerlinNoise(pos1.x, pos1.y);
            float noise2 = Mathf.PerlinNoise(pos2.y, pos2.z);
            float noise3 = Mathf.PerlinNoise(pos3.z, pos3.x);
            
            // Combiner avec des poids différents pour chaque octave
            float weight1 = 0.4f + (i * 0.1f);
            float weight2 = 0.3f + (i * 0.05f);
            float weight3 = 0.3f - (i * 0.05f);
            
            float noiseValue = noise1 * weight1 + noise2 * weight2 + noise3 * weight3;
            
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
    
    float Generate3DRidgeNoise(Vector3 position, float scale) {
        // Générer plusieurs couches de bruit 3D pour les ridges
        float noise1 = Mathf.PerlinNoise(position.x * scale, position.y * scale);
        float noise2 = Mathf.PerlinNoise(position.y * scale, position.z * scale);
        float noise3 = Mathf.PerlinNoise(position.z * scale, position.x * scale);
        
        // Combiner les bruits pour créer des ridges 3D
        float combinedNoise = (noise1 + noise2 + noise3) / 3f;
        
        // Créer des ridges en utilisant la valeur absolue
        float ridge = Mathf.Abs(combinedNoise - 0.5f) * 2f;
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
            
            float height = GetEffectiveHeight(normalizedVertex);
            
            // Nouveau système d'océans qui préserve la forme de base
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(normalizedVertex, height);
            } else {
                // Ancien système (pour compatibilité)
                float effectiveWaterLevel = GetEffectiveWaterLevel();
                if (useFlatOceans && height <= effectiveWaterLevel) {
                    height = 0f; // Océans plats au niveau 0
                } else if (height > effectiveWaterLevel) {
                    if (forceOceanLevel) {
                        height = height - effectiveWaterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Appliquer la hauteur au vertex
            vertices[i] = normalizedVertex * (radius + height);
        }
        
        // Créer le mesh avec multi-matériaux
        CreateProceduralMesh(vertices, uvs, triangles);
    }
    
    void ApplyProceduralGenerationToAllChunks() {
        if (!useProceduralGeneration) return;
        
        // Appliquer la génération procédurale aux vertices partagés (pour tous les chunks)
        for (int i = 0; i < faceVerticesChunks[0].Count; i++) {
            Vector3 originalVertex = faceVerticesChunks[0][i];
            Vector3 normalizedVertex = originalVertex.normalized;
            
            float height = GetEffectiveHeight(normalizedVertex);
            
            // Nouveau système d'océans qui préserve la forme de base
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(normalizedVertex, height);
            } else {
                // Ancien système (pour compatibilité)
                float effectiveWaterLevel = GetEffectiveWaterLevel();
                if (useFlatOceans && height <= effectiveWaterLevel) {
                    height = 0f; // Océans plats au niveau 0
                } else if (height > effectiveWaterLevel) {
                    if (forceOceanLevel) {
                        height = height - effectiveWaterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Appliquer la hauteur au vertex
            faceVerticesChunks[0][i] = normalizedVertex * (radius + height);
        }
        
        // Mettre à jour les meshes des chunks normaux
        int updatedChunks = 0;
        for (int faceIndex = 0; faceIndex < 20; faceIndex++) {
            if (faceChunkTriangleCounts[faceIndex] > 0 && faceMeshChunks[faceIndex] != null) {
                faceMeshChunks[faceIndex].vertices = faceVerticesChunks[0].ToArray();
                faceMeshChunks[faceIndex].RecalculateNormals();
                faceMeshChunks[faceIndex].RecalculateBounds();
                
                // Mettre à jour le mesh du MeshFilter
                if (faceMeshFilterChunks[faceIndex] != null) {
                    faceMeshFilterChunks[faceIndex].mesh = faceMeshChunks[faceIndex];
                    
                    // Appliquer les textures procédurales au chunk normal
                    CreateProceduralFaceChunkMesh(faceIndex);
                    
                    updatedChunks++;
                }
            }
        }
        
        if (updatedChunks > 0) {
            Debug.Log($"🔄 {updatedChunks} chunks normaux mis à jour");
        }
        
        // Appliquer la génération procédurale aux sous-chunks
        int updatedSubChunks = 0;
        for (int faceIndex = 0; faceIndex < 20; faceIndex++) {
            if (faceSubChunkCounts[faceIndex] > 0) {
                for (int subIndex = 0; subIndex < faceSubChunkCounts[faceIndex]; subIndex++) {
                    if (faceSubTrianglesChunks[faceIndex][subIndex].Count > 0) {
                        // Appliquer la génération procédurale aux vertices de ce sous-chunk
                        for (int i = 0; i < faceSubVerticesChunks[faceIndex][subIndex].Count; i++) {
                            Vector3 originalVertex = faceSubVerticesChunks[faceIndex][subIndex][i];
                            Vector3 normalizedVertex = originalVertex.normalized;
                            
                            float height = GetEffectiveHeight(normalizedVertex);
                            
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
                            faceSubVerticesChunks[faceIndex][subIndex][i] = normalizedVertex * (radius + height);
                        }
                        
                        // Mettre à jour le mesh du sous-chunk
                        if (faceSubMeshChunks[faceIndex].Count > subIndex && faceSubMeshChunks[faceIndex][subIndex] != null) {
                            faceSubMeshChunks[faceIndex][subIndex].vertices = faceSubVerticesChunks[faceIndex][subIndex].ToArray();
                            faceSubMeshChunks[faceIndex][subIndex].RecalculateNormals();
                            faceSubMeshChunks[faceIndex][subIndex].RecalculateBounds();
                            
                            // Mettre à jour le mesh du MeshFilter
                            if (faceSubMeshFilterChunks[faceIndex].Count > subIndex && faceSubMeshFilterChunks[faceIndex][subIndex] != null) {
                                faceSubMeshFilterChunks[faceIndex][subIndex].mesh = faceSubMeshChunks[faceIndex][subIndex];
                                
                                // Appliquer les matériaux procéduraux au sous-chunk
                                CreateProceduralSubChunkMesh(faceIndex, subIndex);
                                
                                updatedSubChunks++;
                            }
                        }
                    }
                }
            }
        }
        
        if (updatedSubChunks > 0) {
            Debug.Log($"🔄 {updatedSubChunks} sous-chunks mis à jour");
        }
    }
    
    void ApplyProceduralGenerationToMainMesh() {
        if (!useProceduralGeneration || meshFilter == null || meshFilter.mesh == null) return;
        
        // Récupérer les vertices du mesh principal
        Vector3[] vertices = meshFilter.mesh.vertices;
        
        // Appliquer la génération procédurale aux vertices
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 originalVertex = vertices[i];
            Vector3 normalizedVertex = originalVertex.normalized;
            
            float height = GetEffectiveHeight(normalizedVertex);
            
            // Nouveau système d'océans qui préserve la forme de base
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(normalizedVertex, height);
            } else {
                // Ancien système (pour compatibilité)
                float effectiveWaterLevel = GetEffectiveWaterLevel();
                if (useFlatOceans && height <= effectiveWaterLevel) {
                    height = 0f; // Océans plats au niveau 0
                } else if (height > effectiveWaterLevel) {
                    if (forceOceanLevel) {
                        height = height - effectiveWaterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Appliquer la hauteur au vertex
            vertices[i] = normalizedVertex * (radius + height);
        }
        
        // Mettre à jour le mesh
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
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
                    
                    float height = GetEffectiveHeight(normalizedVertex);
                    
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
            float effectiveWaterLevel = GetEffectiveWaterLevel();
            float effectiveMountainLevel = GetEffectiveMountainLevel();
            
            if (avgHeight <= effectiveWaterLevel) { // Océans au niveau de l'eau
                waterTriangles.Add(p1);
                waterTriangles.Add(p2);
                waterTriangles.Add(p3);
            } else if (avgHeight <= effectiveMountainLevel) { // Seuil de montagne normal
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
    
    public float GetVertexHeight(Vector3 vertex) {
        if (disableCacheForPerformance) {
            return GetVertexHeightOriginal(vertex);
        } else if (fixIcosahedronSeams) {
            return GetVertexHeightWithSeamFix(vertex);
        } else {
            return GetVertexHeightOriginal(vertex);
        }
    }
    
    float GetVertexHeightOriginal(Vector3 vertex) {
        Vector3 normalizedVertex = vertex.normalized;
        float height = GetEffectiveHeight(normalizedVertex);
        
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
    
    float GetVertexHeightWithSeamFix(Vector3 vertex) {
        // Normaliser le vertex pour la comparaison
        Vector3 normalizedVertex = vertex.normalized;
        
        if (useFastCache) {
            return GetVertexHeightFastCache(normalizedVertex);
        } else {
            return GetVertexHeightSlowCache(normalizedVertex);
        }
    }
    
    float GetVertexHeightFastCache(Vector3 normalizedVertex) {
        // Utiliser une clé normalisée pour un accès direct au cache
        Vector3 cacheKey = NormalizeVertexForCache(normalizedVertex);
        
        if (vertexHeightCache.ContainsKey(cacheKey)) {
            return vertexHeightCache[cacheKey];
        } else {
            // Calculer la nouvelle hauteur
            float height = GetEffectiveHeight(normalizedVertex);
            
            // Appliquer le système d'océans avancé si activé
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(normalizedVertex, height);
            } else {
                // Ancien système (pour compatibilité)
                float effectiveWaterLevel = GetEffectiveWaterLevel();
                if (useFlatOceans && height <= effectiveWaterLevel) {
                    height = 0f; // Océans plats au niveau 0
                } else if (height > effectiveWaterLevel) {
                    if (forceOceanLevel) {
                        height = height - effectiveWaterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Appliquer le lissage des océans si activé
            if (smoothOceanSeams && height <= waterLevel) {
                height = ApplyOceanSmoothing(normalizedVertex, height);
            }
            
            // Mettre en cache
            vertexHeightCache[cacheKey] = height;
            
            return height;
        }
    }
    
    float GetVertexHeightSlowCache(Vector3 normalizedVertex) {
        // Chercher un vertex existant dans le cache avec une tolérance
        Vector3 cachedVertex = FindCachedVertex(normalizedVertex);
        
        if (cachedVertex != Vector3.zero) {
            // Utiliser la hauteur du vertex en cache
            return vertexHeightCache[cachedVertex];
        } else {
            // Calculer la nouvelle hauteur
            float height = GetEffectiveHeight(normalizedVertex);
            
            // Appliquer le système d'océans avancé si activé
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(normalizedVertex, height);
            } else {
                // Ancien système (pour compatibilité)
                float effectiveWaterLevel = GetEffectiveWaterLevel();
                if (useFlatOceans && height <= effectiveWaterLevel) {
                    height = 0f; // Océans plats au niveau 0
                } else if (height > effectiveWaterLevel) {
                    if (forceOceanLevel) {
                        height = height - effectiveWaterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Appliquer le lissage des océans si activé
            if (smoothOceanSeams && height <= waterLevel) {
                height = ApplyOceanSmoothing(normalizedVertex, height);
            }
            
            // Mettre en cache avec une clé normalisée précise
            Vector3 cacheKey = NormalizeVertexForCache(normalizedVertex);
            vertexHeightCache[cacheKey] = height;
            normalizedVertexCache[cacheKey] = normalizedVertex;
            
            return height;
        }
    }
    
    Vector3 NormalizeVertexForCache(Vector3 vertex) {
        // Normaliser avec une précision fixe pour éviter les erreurs de virgule flottante
        float precision = 10000f; // 4 décimales de précision
        return new Vector3(
            Mathf.Round(vertex.x * precision) / precision,
            Mathf.Round(vertex.y * precision) / precision,
            Mathf.Round(vertex.z * precision) / precision
        );
    }
    
    Vector3 FindCachedVertex(Vector3 targetVertex) {
        // Utiliser une approche plus robuste pour trouver les vertices partagés
        Vector3 bestMatch = Vector3.zero;
        float bestDistance = float.MaxValue;
        
        foreach (var cachedVertex in vertexHeightCache.Keys) {
            float distance = Vector3.Distance(cachedVertex, targetVertex);
            if (distance < vertexTolerance && distance < bestDistance) {
                bestMatch = cachedVertex;
                bestDistance = distance;
            }
        }
        
        return bestMatch;
    }
    
    void ClearVertexHeightCache() {
        vertexHeightCache.Clear();
        normalizedVertexCache.Clear();
    }
    
    float ApplyOceanSmoothing(Vector3 position, float originalHeight) {
        // Pour les océans, utiliser une hauteur uniforme pour éviter les fentes
        if (originalHeight <= waterLevel) {
            return 0f; // Océans parfaitement plats
        }
        return originalHeight;
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
        
        float effectiveWaterLevel = GetEffectiveWaterLevel();
        
        if (originalHeight <= effectiveWaterLevel) {
            // Zone qui devrait être submergée
            // Au lieu de mettre à 0, on aplatit progressivement vers le niveau de l'eau
            float flatteningFactor = Mathf.Clamp01((effectiveWaterLevel - originalHeight) / effectiveWaterLevel);
            float flattenedHeight = Mathf.Lerp(originalHeight, effectiveWaterLevel, flatteningFactor * oceanFlatteningStrength);
            
            // Pour les océans plats, on peut encore les aplatir complètement si souhaité
            if (useFlatOceans) {
                return effectiveWaterLevel; // Niveau constant pour les océans
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
            float effectiveWaterLevel = GetEffectiveWaterLevel();
            float effectiveMountainLevel = GetEffectiveMountainLevel();
            
            if (avgHeight <= effectiveWaterLevel) { // Océans au niveau de l'eau
                waterTriangles.Add(p1);
                waterTriangles.Add(p2);
                waterTriangles.Add(p3);
            } else if (avgHeight <= effectiveMountainLevel) { // Seuil de montagne normal
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
    
    void CreateProceduralFaceChunkMesh(int chunkIndex) {
        // Séparer les triangles par type de terrain pour ce chunk par face
        List<int> waterTriangles = new List<int>();
        List<int> landTriangles = new List<int>();
        List<int> mountainTriangles = new List<int>();
        
        for (int i = 0; i < faceTrianglesChunks[chunkIndex].Count; i += 3) {
            int p1 = faceTrianglesChunks[chunkIndex][i];
            int p2 = faceTrianglesChunks[chunkIndex][i + 1];
            int p3 = faceTrianglesChunks[chunkIndex][i + 2];
            
            // Calculer l'altitude moyenne du triangle
            float avgHeight = (GetVertexHeight(faceVerticesChunks[0][p1]) + 
                              GetVertexHeight(faceVerticesChunks[0][p2]) + 
                              GetVertexHeight(faceVerticesChunks[0][p3])) / 3f;
            
            // Assigner au bon type de terrain
            float effectiveWaterLevel = GetEffectiveWaterLevel();
            float effectiveMountainLevel = GetEffectiveMountainLevel();
            
            if (avgHeight <= effectiveWaterLevel) { // Océans au niveau de l'eau
                waterTriangles.Add(p1);
                waterTriangles.Add(p2);
                waterTriangles.Add(p3);
            } else if (avgHeight <= effectiveMountainLevel) { // Seuil de montagne normal
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
        chunkMesh.name = $"Icosahedron Face Chunk {chunkIndex} Planet";
        chunkMesh.vertices = faceVerticesChunks[0].ToArray();
        chunkMesh.uv = faceUvsChunks[0].ToArray();
        
        // Créer les submeshes
        chunkMesh.subMeshCount = 3;
        chunkMesh.SetTriangles(waterTriangles.ToArray(), 0);
        chunkMesh.SetTriangles(landTriangles.ToArray(), 1);
        chunkMesh.SetTriangles(mountainTriangles.ToArray(), 2);
        
        chunkMesh.RecalculateNormals();
        chunkMesh.RecalculateBounds();
        
        // Appliquer le mesh
        faceMeshFilterChunks[chunkIndex].mesh = chunkMesh;
        
        // Appliquer les matériaux de planète
        ApplyPlanetMaterialsToFaceChunk(chunkIndex);
    }
    
    void CreateProceduralSubChunkMesh(int faceIndex, int subIndex) {
        // Séparer les triangles par type de terrain pour ce sous-chunk
        List<int> waterTriangles = new List<int>();
        List<int> landTriangles = new List<int>();
        List<int> mountainTriangles = new List<int>();
        
        for (int i = 0; i < faceSubTrianglesChunks[faceIndex][subIndex].Count; i += 3) {
            int p1 = faceSubTrianglesChunks[faceIndex][subIndex][i];
            int p2 = faceSubTrianglesChunks[faceIndex][subIndex][i + 1];
            int p3 = faceSubTrianglesChunks[faceIndex][subIndex][i + 2];
            
            // Calculer l'altitude moyenne du triangle
            float avgHeight = (GetVertexHeight(faceSubVerticesChunks[faceIndex][subIndex][p1]) + 
                              GetVertexHeight(faceSubVerticesChunks[faceIndex][subIndex][p2]) + 
                              GetVertexHeight(faceSubVerticesChunks[faceIndex][subIndex][p3])) / 3f;
            
            // Assigner au bon type de terrain
            float effectiveWaterLevel = GetEffectiveWaterLevel();
            float effectiveMountainLevel = GetEffectiveMountainLevel();
            
            if (avgHeight <= effectiveWaterLevel) { // Océans au niveau de l'eau
                waterTriangles.Add(p1);
                waterTriangles.Add(p2);
                waterTriangles.Add(p3);
            } else if (avgHeight <= effectiveMountainLevel) { // Seuil de montagne normal
                landTriangles.Add(p1);
                landTriangles.Add(p2);
                landTriangles.Add(p3);
            } else {
                mountainTriangles.Add(p1);
                mountainTriangles.Add(p2);
                mountainTriangles.Add(p3);
            }
        }
        
        // Utiliser le mesh existant et le modifier
        Mesh subChunkMesh = faceSubMeshChunks[faceIndex][subIndex];
        if (subChunkMesh == null) {
            subChunkMesh = new Mesh();
            subChunkMesh.name = $"Icosahedron Face {faceIndex} SubChunk {subIndex} Planet";
            faceSubMeshChunks[faceIndex][subIndex] = subChunkMesh;
        }
        
        // Mettre à jour les vertices et UVs (déjà modifiés par la génération procédurale)
        subChunkMesh.vertices = faceSubVerticesChunks[faceIndex][subIndex].ToArray();
        subChunkMesh.uv = faceSubUvsChunks[faceIndex][subIndex].ToArray();
        
        // Créer les submeshes
        subChunkMesh.subMeshCount = 3;
        subChunkMesh.SetTriangles(waterTriangles.ToArray(), 0);
        subChunkMesh.SetTriangles(landTriangles.ToArray(), 1);
        subChunkMesh.SetTriangles(mountainTriangles.ToArray(), 2);
        
        subChunkMesh.RecalculateNormals();
        subChunkMesh.RecalculateBounds();
        
        // Appliquer le mesh
        faceSubMeshFilterChunks[faceIndex][subIndex].mesh = subChunkMesh;
        
        // Appliquer les matériaux de planète
        ApplyPlanetMaterialsToSubChunk(faceIndex, subIndex);
    }
    
    void ApplyPlanetMaterialsToSubChunk(int faceIndex, int subIndex) {
        if (faceSubMeshRendererChunks[faceIndex][subIndex] == null) return;
        
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
        faceSubMeshRendererChunks[faceIndex][subIndex].materials = materials;
    }
    
    void ApplyPlanetMaterialsToFaceChunk(int chunkIndex) {
        if (faceMeshRendererChunks[chunkIndex] == null) return;
        
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
        faceMeshRendererChunks[chunkIndex].materials = materials;
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
