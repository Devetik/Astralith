using UnityEngine;
using System.Collections.Generic;

namespace HexasphereProcedural {

    /// <summary>
    /// Générateur de planète hexagonal complet avec toutes les fonctionnalités
    /// </summary>
    public class HexaAstralithPlanet : MonoBehaviour {
        
        [Header("🌍 Configuration de Base")]
        [SerializeField] public int divisions = 3;
        [SerializeField] public float radius = 1f;
        [SerializeField] public float noiseScale = 1f;
        [SerializeField] public float heightAmplitude = 0.2f;
        [SerializeField] public bool showDebugInfo = true;
        
        [Header("🏷️ Tag de l'Objet")]
        [SerializeField] public string objectTag = "Planet";
        
        [Header("🎨 Matériaux (Assignez dans l'inspector)")]
        [SerializeField] public Material waterMaterial;
        [SerializeField] public Material landMaterial;
        [SerializeField] public Material mountainMaterial;
        
        [Header("🌊 Niveaux de Terrain")]
        [SerializeField] public float waterLevel = 0.0f;
        [SerializeField] public float mountainLevel = 0.3f;
        
        [Header("🌊 Contrôle des Hauteurs")]
        [SerializeField] public bool useHeightControl = true;
        [SerializeField] public float minHeight = -0.3f;
        [SerializeField] public float maxHeight = 0.4f;
        
        [Header("🎵 Bruit Avancé")]
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
        
        [Header("🌊 Océans Plats")]
        [SerializeField] public bool useFlatOceans = true;  
        [SerializeField] public bool forceOceanLevel = true;
        
        [Header("🎯 Système LOD")]
        [SerializeField] public bool enableLOD = true;
        [SerializeField] public Transform cameraTransform;
        [SerializeField] public float lodUpdateInterval = 0.1f;
        [SerializeField] public float lod0Distance = 10f;
        [SerializeField] public float lod1Distance = 25f;
        [SerializeField] public float lod2Distance = 50f;
        [SerializeField] public float lod3Distance = 100f;
        [SerializeField] public float cullDistance = 200f;
        [SerializeField] public int lod0Divisions = 4;
        [SerializeField] public int lod1Divisions = 3;
        [SerializeField] public int lod2Divisions = 2;
        [SerializeField] public int lod3Divisions = 1;
        
        [Header("🌊 LOD Frontières")]
        [SerializeField] public bool enableBoundaryLOD = true;
        [SerializeField] public float boundaryDetectionRadius = 2f;
        [SerializeField] public int boundarySubdivisionLevel = 2;
        [SerializeField] public bool detectWaterLandBoundaries = true;
        [SerializeField] public bool detectMountainBoundaries = true;
        
        [Header("🎨 LOD Transitions")]
        [SerializeField] public bool enableTransitionSmoothing = true;
        [SerializeField] public float transitionWidth = 1f;
        [SerializeField] public int transitionSubdivisions = 3;
        [SerializeField] public bool smoothWaterLandTransitions = true;
        [SerializeField] public bool smoothMountainTransitions = true;
        
        [Header("🌊 LOD Océans")]
        [SerializeField] public bool preserveOceanStructure = true;
        [SerializeField] public float oceanStructureStrength = 0.5f;
        
        [Header("🌊 Système Océans Avancé")]
        [SerializeField] public bool useAdvancedOceanSystem = true;
        [SerializeField] public bool preserveBaseShape = true;
        [SerializeField] public float oceanFlatteningStrength = 1f;
        
        [Header("⚡ Optimisation Haute Résolution")]
        [SerializeField] public bool useAdaptiveSubdivision = true;
        [SerializeField] public int maxSafeDivisions = 5;
        [SerializeField] public bool useChunkedGeneration = true;
        [SerializeField] public int chunkSize = 1000;
        
        [Header("🔺 Subdivision Alternative")]
        [SerializeField] public bool useTriangularSubdivision = true;
        [SerializeField] public int maxTriangularDivisions = 20;
        [SerializeField] public bool useSimpleEdgeSubdivision = false;
        
        [Header("🛡️ Limites de Sécurité")]
        [SerializeField] public int maxVerticesLimit = 200000;
        [SerializeField] public bool useSmartLimits = true;
        
        [Header("🔺 Subdivision Hexasphere")]
        [SerializeField] public bool useHexasphereSubdivision = false; // Désactivé par défaut pour éviter les crashes
        [SerializeField] public int hexasphereMaxDivisions = 10;
        [SerializeField] public bool useSafeMode = true; // Mode sécurisé par défaut
        
        [Header("🎯 Subdivision Intelligente")]
        [SerializeField] public bool useIntelligentSubdivision = true;
        [SerializeField] public int maxIntelligentDivisions = 8;
        [SerializeField] public float subdivisionQuality = 0.8f; // 0.0 = très basse qualité, 1.0 = haute qualité
        
        [Header("🔷 Subdivision Hexagone")]
        [SerializeField] public bool useHexagonSubdivision = false; // Désactivé par défaut pour éviter les crashes
        [SerializeField] public int hexagonMaxDivisions = 8; // Limite réduite
        [SerializeField] public bool useHexagonRows = true; // Ajouter des rangées d'hexagones
        
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        
        // Variables LOD
        private int currentLOD = 0;
        private float lastLODUpdate = 0f;
        private float currentDistance = 0f;
        private Dictionary<int, Mesh> lodMeshes = new Dictionary<int, Mesh>();
        private Dictionary<int, Material[]> lodMaterials = new Dictionary<int, Material[]>();
        
        // Sauvegarde du mesh original
        private Mesh originalMesh;
        private Material[] originalMaterials;
        
        void Start() {
            // Appliquer le tag à l'objet
            ApplyTagToObject();
            
            // Générer la planète
            GeneratePlanet();
        }
        
        void Update() {
            if (Input.GetKeyDown(KeyCode.R)) {
                GeneratePlanet();
            }
            if (Input.GetKeyDown(KeyCode.H)) {
                ToggleHeightControl();
            }
            
            // Mise à jour du système LOD
            if (enableLOD && Time.time - lastLODUpdate > lodUpdateInterval) {
                UpdateLOD();
                lastLODUpdate = Time.time;
            }
        }
        
        public void GeneratePlanet() {
            CreateMesh();
        }
        
        void ApplyTagToObject() {
            if (string.IsNullOrEmpty(objectTag)) {
                Debug.LogWarning("⚠️ Aucun tag défini pour l'objet");
                return;
            }
            
            // Vérifier si le tag existe
            if (!IsTagValid(objectTag)) {
                Debug.LogWarning($"⚠️ Le tag '{objectTag}' n'existe pas. Créez-le dans Edit > Project Settings > Tags and Layers");
                return;
            }
            
            // Appliquer le tag
            gameObject.tag = objectTag;
        }
        
        bool IsTagValid(string tag) {
            // Vérifier si le tag existe dans les tags Unity
            try {
                // Essayer de trouver des objets avec ce tag
                GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
                return true; // Si on arrive ici, le tag existe
            } catch (UnityException) {
                return false; // Le tag n'existe pas
            }
        }
        
        void CreateMesh() {
            if (meshFilter == null) {
                meshFilter = GetComponent<MeshFilter>();
                if (meshFilter == null) {
                    meshFilter = gameObject.AddComponent<MeshFilter>();
                }
            }
            
            if (meshRenderer == null) {
                meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer == null) {
                    meshRenderer = gameObject.AddComponent<MeshRenderer>();
                }
            }
            
            // Générer les vertices et triangles
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            
            // Créer une sphère avec triangulation cohérente
            CreateSphereMesh(vertices, uvs, triangles);
            
            // Créer le mesh avec multi-matériaux
            CreateMultiMaterialMesh(vertices, uvs, triangles);
        }
        
        void CreateSphereMesh(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
            // Créer un icosaèdre de base
            List<Vector3> baseVertices = CreateIcosahedronVertices();
            List<int> baseTriangles = CreateIcosahedronTriangles();
            
            // Choisir la méthode de subdivision (priorité à la subdivision intelligente)
            if (useIntelligentSubdivision && divisions > maxSafeDivisions) {
                Debug.Log($"🎯 Utilisation de la subdivision intelligente pour {divisions} divisions");
                CreateIntelligentSubdivision(baseVertices, baseTriangles, divisions);
            } else if (useHexagonSubdivision && divisions > maxSafeDivisions) {
                Debug.Log($"🔷 Utilisation de la subdivision hexagone pour {divisions} divisions");
                CreateHexagonSubdivision(baseVertices, baseTriangles, divisions);
            } else if (useHexasphereSubdivision && !useSafeMode && divisions > maxSafeDivisions) {
                Debug.Log($"🔺 Utilisation de la subdivision Hexasphere pour {divisions} divisions");
                CreateHexasphereSubdivision(baseVertices, baseTriangles, divisions);
            } else {
                // Subdivision classique avec limites de sécurité intelligentes
                Debug.Log($"🔧 Génération de {divisions} divisions avec subdivision classique contrôlée");
                
                // Calculer le nombre de vertices attendu pour cette division
                int expectedVertices = CalculateExpectedVertices(divisions);
                
                if (expectedVertices > maxVerticesLimit) {
                    Debug.LogWarning($"⚠️ Trop de vertices attendus ({expectedVertices:N0}). Limitation à {maxVerticesLimit:N0} vertices.");
                }
                
                for (int division = 0; division < divisions; division++) {
                    // Vérifier la limite avant chaque subdivision
                    if (baseVertices.Count > maxVerticesLimit) {
                        Debug.LogWarning($"⚠️ Limite de vertices atteinte à la division {division}. Arrêt de la subdivision.");
                        break;
                    }
                    
                    // Vérification intelligente si activée
                    if (useSmartLimits) {
                        // Vérifier si la prochaine subdivision dépassera la limite
                        int nextVertices = baseVertices.Count * 4; // Chaque division multiplie par ~4
                        if (nextVertices > maxVerticesLimit) {
                            Debug.LogWarning($"⚠️ Prochaine subdivision dépasserait la limite. Arrêt à la division {division}.");
                            break;
                        }
                    }
                    
                    SubdivideSphere(baseVertices, baseTriangles);
                    Debug.Log($"📊 Division {division}: {baseVertices.Count} vertices, {baseTriangles.Count/3} triangles");
                }
            }
            
            // Appliquer les hauteurs et créer le mesh final
            ApplyHeightsToMesh(baseVertices, baseTriangles, vertices, uvs, triangles);
            
            // Vérification du mesh généré
            Debug.Log($"✅ Mesh généré: {vertices.Count} vertices, {triangles.Count/3} triangles");
            if (vertices.Count == 0) {
                Debug.LogError("❌ ERREUR: Aucun vertex généré !");
            }
            if (triangles.Count == 0) {
                Debug.LogError("❌ ERREUR: Aucun triangle généré !");
            }
        }
        
        // === MÉTHODES DE SUBDIVISION HEXAGONE ===
        
        void CreateHexagonSubdivision(List<Vector3> vertices, List<int> triangles, int targetDivisions) {
            Debug.Log($"🔷 Début subdivision hexagone pour {targetDivisions} divisions");
            
            // Subdivision classique jusqu'à la limite sûre
            int safeDivisions = Mathf.Min(targetDivisions, maxSafeDivisions);
            for (int division = 0; division < safeDivisions; division++) {
                SubdivideSphere(vertices, triangles);
                Debug.Log($"📊 Division classique {division}: {vertices.Count} vertices, {triangles.Count/3} triangles");
            }
            
            // Subdivision hexagone pour les divisions restantes
            int remainingDivisions = targetDivisions - safeDivisions;
            if (remainingDivisions > 0) {
                Debug.Log($"🔷 Subdivision hexagone pour {remainingDivisions} divisions restantes");
                for (int i = 0; i < remainingDivisions; i++) {
                    // Vérifications de sécurité strictes
                    if (vertices.Count > 50000) {
                        Debug.LogWarning($"⚠️ Limite de vertices atteinte dans hexagone. Arrêt à la division {i}.");
                        break;
                    }
                    
                    if (triangles.Count > 200000) {
                        Debug.LogWarning($"⚠️ Limite de triangles atteinte dans hexagone. Arrêt à la division {i}.");
                        break;
                    }
                    
                    // Vérifier si on peut continuer sans crash
                    int expectedVertices = vertices.Count * 4;
                    int expectedTriangles = triangles.Count * 4;
                    
                    if (expectedVertices > 100000 || expectedTriangles > 400000) {
                        Debug.LogWarning($"⚠️ Prochaine subdivision dépasserait les limites. Arrêt à la division {i}.");
                        break;
                    }
                    
                    SubdivideHexagonStyle(vertices, triangles);
                    Debug.Log($"📊 Division hexagone {i}: {vertices.Count} vertices, {triangles.Count/3} triangles");
                }
            }
        }
        
        void SubdivideHexagonStyle(List<Vector3> vertices, List<int> triangles) {
            // Approche simplifiée : subdivision classique mais contrôlée
            Debug.Log($"🔷 Subdivision hexagone simplifiée pour {triangles.Count/3} triangles");
            
            // Créer une nouvelle liste de triangles
            List<int> newTriangles = new List<int>();
            
            // Pour chaque triangle, appliquer une subdivision simple
            for (int i = 0; i < triangles.Count; i += 3) {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                // Vérifier les indices
                if (v1 >= vertices.Count || v2 >= vertices.Count || v3 >= vertices.Count) {
                    Debug.LogError($"❌ Indice de vertex hors limites: v1={v1}, v2={v2}, v3={v3}, vertices.Count={vertices.Count}");
                    continue;
                }
                
                // Subdivision simple : diviser chaque triangle en 4 (comme subdivision classique)
                Vector3 mid12 = ((vertices[v1] + vertices[v2]) / 2f).normalized;
                Vector3 mid23 = ((vertices[v2] + vertices[v3]) / 2f).normalized;
                Vector3 mid31 = ((vertices[v3] + vertices[v1]) / 2f).normalized;
                
                // Ajouter les nouveaux vertices
                int mid12Index = vertices.Count;
                vertices.Add(mid12);
                int mid23Index = vertices.Count;
                vertices.Add(mid23);
                int mid31Index = vertices.Count;
                vertices.Add(mid31);
                
                // Créer 4 triangles
                newTriangles.Add(v1);
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid31Index);
                
                newTriangles.Add(v2);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid12Index);
                
                newTriangles.Add(v3);
                newTriangles.Add(mid31Index);
                newTriangles.Add(mid23Index);
                
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid31Index);
            }
            
            // Remplacer les anciens triangles par les nouveaux
            triangles.Clear();
            triangles.AddRange(newTriangles);
            
            Debug.Log($"🔷 Subdivision hexagone terminée: {vertices.Count} vertices, {triangles.Count/3} triangles");
        }
        
        List<Vector3> SubdivideEdgeHexagon(Vector3 start, Vector3 end, int segments) {
            List<Vector3> points = new List<Vector3>();
            points.Add(start);
            
            for (int i = 1; i < segments; i++) {
                float t = (float)i / segments;
                Vector3 point = Vector3.Lerp(start, end, t);
                point = point.normalized; // Normaliser pour maintenir la forme sphérique
                points.Add(point);
            }
            
            points.Add(end);
            return points;
        }
        
        void CreateTriangleFromVertices(Vector3 p1, Vector3 p2, Vector3 p3, List<Vector3> vertices, List<int> triangles) {
            // Ajouter les vertices s'ils n'existent pas déjà
            int i1 = GetOrAddVertexHexagon(p1, vertices);
            int i2 = GetOrAddVertexHexagon(p2, vertices);
            int i3 = GetOrAddVertexHexagon(p3, vertices);
            
            // Ajouter le triangle
            triangles.Add(i1);
            triangles.Add(i2);
            triangles.Add(i3);
        }
        
        int GetOrAddVertexHexagon(Vector3 vertex, List<Vector3> vertices) {
            // Chercher un vertex existant avec une tolérance plus stricte
            float tolerance = 0.0001f;
            for (int i = 0; i < vertices.Count; i++) {
                if (Vector3.Distance(vertices[i], vertex) < tolerance) {
                    return i;
                }
            }
            
            // Ajouter un nouveau vertex
            vertices.Add(vertex);
            return vertices.Count - 1;
        }
        
        // === MÉTHODES DE SUBDIVISION INTELLIGENTE ===
        
        void CreateIntelligentSubdivision(List<Vector3> vertices, List<int> triangles, int targetDivisions) {
            Debug.Log($"🎯 Début subdivision intelligente pour {targetDivisions} divisions");
            
            // Subdivision classique jusqu'à la limite sûre
            int safeDivisions = Mathf.Min(targetDivisions, maxSafeDivisions);
            for (int division = 0; division < safeDivisions; division++) {
                SubdivideSphere(vertices, triangles);
                Debug.Log($"📊 Division classique {division}: {vertices.Count} vertices, {triangles.Count/3} triangles");
            }
            
            // Subdivision intelligente pour les divisions restantes
            int remainingDivisions = targetDivisions - safeDivisions;
            if (remainingDivisions > 0) {
                Debug.Log($"🎯 Subdivision intelligente pour {remainingDivisions} divisions restantes");
                for (int i = 0; i < remainingDivisions; i++) {
                    // Limite de sécurité basée sur la qualité
                    int qualityLimit = Mathf.RoundToInt(maxVerticesLimit * subdivisionQuality);
                    if (vertices.Count > qualityLimit) {
                        Debug.LogWarning($"⚠️ Limite de qualité atteinte. Arrêt à la division {i}.");
                        break;
                    }
                    
                    SubdivideIntelligent(vertices, triangles);
                    Debug.Log($"📊 Division intelligente {i}: {vertices.Count} vertices, {triangles.Count/3} triangles");
                }
            }
        }
        
        void SubdivideIntelligent(List<Vector3> vertices, List<int> triangles) {
            // Créer une nouvelle liste de triangles
            List<int> newTriangles = new List<int>();
            
            // Pour chaque triangle, appliquer une subdivision intelligente
            for (int i = 0; i < triangles.Count; i += 3) {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                // Vérifier les indices
                if (v1 >= vertices.Count || v2 >= vertices.Count || v3 >= vertices.Count) {
                    Debug.LogError($"❌ Indice de vertex hors limites: v1={v1}, v2={v2}, v3={v3}, vertices.Count={vertices.Count}");
                    continue;
                }
                
                // Calculer la taille du triangle pour décider de la subdivision
                float triangleSize = CalculateTriangleSize(vertices[v1], vertices[v2], vertices[v3]);
                
                if (triangleSize > 0.1f) { // Seulement subdiviser les gros triangles
                    // Subdivision classique mais contrôlée
                    Vector3 mid12 = ((vertices[v1] + vertices[v2]) / 2f).normalized;
                    Vector3 mid23 = ((vertices[v2] + vertices[v3]) / 2f).normalized;
                    Vector3 mid31 = ((vertices[v3] + vertices[v1]) / 2f).normalized;
                    
                    // Ajouter les nouveaux vertices
                    int mid12Index = vertices.Count;
                    vertices.Add(mid12);
                    int mid23Index = vertices.Count;
                    vertices.Add(mid23);
                    int mid31Index = vertices.Count;
                    vertices.Add(mid31);
                    
                    // Créer 4 triangles
                    newTriangles.Add(v1);
                    newTriangles.Add(mid12Index);
                    newTriangles.Add(mid31Index);
                    
                    newTriangles.Add(v2);
                    newTriangles.Add(mid23Index);
                    newTriangles.Add(mid12Index);
                    
                    newTriangles.Add(v3);
                    newTriangles.Add(mid31Index);
                    newTriangles.Add(mid23Index);
                    
                    newTriangles.Add(mid12Index);
                    newTriangles.Add(mid23Index);
                    newTriangles.Add(mid31Index);
                } else {
                    // Triangle trop petit, le garder tel quel
                    newTriangles.Add(v1);
                    newTriangles.Add(v2);
                    newTriangles.Add(v3);
                }
            }
            
            // Remplacer les anciens triangles par les nouveaux
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }
        
        float CalculateTriangleSize(Vector3 p1, Vector3 p2, Vector3 p3) {
            // Calculer l'aire du triangle
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            float area = Vector3.Cross(v1, v2).magnitude * 0.5f;
            return area;
        }
        
        // === MÉTHODES DE SUBDIVISION HEXASPHERE ===
        
        void CreateHexasphereSubdivision(List<Vector3> vertices, List<int> triangles, int targetDivisions) {
            Debug.Log($"🔺 Début subdivision Hexasphere pour {targetDivisions} divisions");
            
            // Subdivision classique jusqu'à la limite sûre
            int safeDivisions = Mathf.Min(targetDivisions, maxSafeDivisions);
            for (int division = 0; division < safeDivisions; division++) {
                SubdivideSphere(vertices, triangles);
                Debug.Log($"📊 Division classique {division}: {vertices.Count} vertices, {triangles.Count/3} triangles");
            }
            
            // Subdivision Hexasphere pour les divisions restantes
            int remainingDivisions = targetDivisions - safeDivisions;
            if (remainingDivisions > 0) {
                Debug.Log($"🔺 Subdivision Hexasphere pour {remainingDivisions} divisions restantes");
                for (int i = 0; i < remainingDivisions; i++) {
                    // Limite de sécurité stricte
                    if (vertices.Count > 50000) {
                        Debug.LogWarning($"⚠️ Limite de vertices atteinte dans Hexasphere. Arrêt à la division {i}.");
                        break;
                    }
                    
                    if (triangles.Count > 100000) {
                        Debug.LogWarning($"⚠️ Limite de triangles atteinte dans Hexasphere. Arrêt à la division {i}.");
                        break;
                    }
                    
                    SubdivideHexasphereStyle(vertices, triangles);
                    Debug.Log($"📊 Division Hexasphere {i}: {vertices.Count} vertices, {triangles.Count/3} triangles");
                }
            }
        }
        
        void SubdivideHexasphereStyle(List<Vector3> vertices, List<int> triangles) {
            // Créer une nouvelle liste de triangles
            List<int> newTriangles = new List<int>();
            
            // Pour chaque triangle, appliquer une subdivision simple mais contrôlée
            for (int i = 0; i < triangles.Count; i += 3) {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                // Vérifier les indices
                if (v1 >= vertices.Count || v2 >= vertices.Count || v3 >= vertices.Count) {
                    Debug.LogError($"❌ Indice de vertex hors limites: v1={v1}, v2={v2}, v3={v3}, vertices.Count={vertices.Count}");
                    continue;
                }
                
                // Subdivision simple : diviser chaque arête en 2 segments
                Vector3 mid12 = ((vertices[v1] + vertices[v2]) / 2f).normalized;
                Vector3 mid23 = ((vertices[v2] + vertices[v3]) / 2f).normalized;
                Vector3 mid31 = ((vertices[v3] + vertices[v1]) / 2f).normalized;
                
                // Ajouter les nouveaux vertices
                int mid12Index = vertices.Count;
                vertices.Add(mid12);
                int mid23Index = vertices.Count;
                vertices.Add(mid23);
                int mid31Index = vertices.Count;
                vertices.Add(mid31);
                
                // Créer 4 triangles (subdivision classique mais contrôlée)
                newTriangles.Add(v1);
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid31Index);
                
                newTriangles.Add(v2);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid12Index);
                
                newTriangles.Add(v3);
                newTriangles.Add(mid31Index);
                newTriangles.Add(mid23Index);
                
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid31Index);
            }
            
            // Remplacer les anciens triangles par les nouveaux
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }
        
        List<Vector3> SubdivideEdge(Vector3 start, Vector3 end, int segments) {
            List<Vector3> points = new List<Vector3>();
            points.Add(start);
            
            for (int i = 1; i < segments; i++) {
                float t = (float)i / segments;
                Vector3 point = Vector3.Lerp(start, end, t);
                point = point.normalized; // Normaliser pour maintenir la forme sphérique
                points.Add(point);
            }
            
            points.Add(end);
            return points;
        }
        
        void CreateTriangleFromPoints(Vector3 p1, Vector3 p2, Vector3 p3, List<Vector3> vertices, List<int> triangles) {
            // Ajouter les vertices s'ils n'existent pas déjà
            int i1 = GetOrAddVertex(p1, vertices);
            int i2 = GetOrAddVertex(p2, vertices);
            int i3 = GetOrAddVertex(p3, vertices);
            
            // Ajouter le triangle
            triangles.Add(i1);
            triangles.Add(i2);
            triangles.Add(i3);
        }
        
        int GetOrAddVertex(Vector3 vertex, List<Vector3> vertices) {
            // Chercher un vertex existant avec une tolérance
            float tolerance = 0.001f;
            for (int i = 0; i < vertices.Count; i++) {
                if (Vector3.Distance(vertices[i], vertex) < tolerance) {
                    return i;
                }
            }
            
            // Ajouter un nouveau vertex
            vertices.Add(vertex);
            return vertices.Count - 1;
        }
        
        // === MÉTHODES DE SUBDIVISION SIMPLE ===
        
        void CreateSimpleEdgeSubdivision(List<Vector3> vertices, List<int> triangles, int targetDivisions) {
            Debug.Log($"🔺 Début subdivision simple par arêtes pour {targetDivisions} divisions");
            
            // Subdivision classique jusqu'à la limite sûre
            int safeDivisions = Mathf.Min(targetDivisions, maxSafeDivisions);
            for (int division = 0; division < safeDivisions; division++) {
                SubdivideSphere(vertices, triangles);
                Debug.Log($"📊 Division classique {division}: {vertices.Count} vertices, {triangles.Count/3} triangles");
            }
            
            // Subdivision simple pour les divisions restantes
            int remainingDivisions = targetDivisions - safeDivisions;
            if (remainingDivisions > 0) {
                Debug.Log($"🔺 Subdivision simple pour {remainingDivisions} divisions restantes");
                for (int i = 0; i < remainingDivisions; i++) {
                    SubdivideSimple(vertices, triangles);
                    Debug.Log($"📊 Division simple {i}: {vertices.Count} vertices, {triangles.Count/3} triangles");
                }
            }
        }
        
        void SubdivideSimple(List<Vector3> vertices, List<int> triangles) {
            // Créer une nouvelle liste de triangles
            List<int> newTriangles = new List<int>();
            
            // Pour chaque triangle, ajouter un vertex au milieu de chaque arête
            for (int i = 0; i < triangles.Count; i += 3) {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                // Calculer les milieux des arêtes
                Vector3 mid12 = ((vertices[v1] + vertices[v2]) / 2f).normalized;
                Vector3 mid23 = ((vertices[v2] + vertices[v3]) / 2f).normalized;
                Vector3 mid31 = ((vertices[v3] + vertices[v1]) / 2f).normalized;
                
                // Ajouter les nouveaux vertices
                int mid12Index = vertices.Count;
                vertices.Add(mid12);
                int mid23Index = vertices.Count;
                vertices.Add(mid23);
                int mid31Index = vertices.Count;
                vertices.Add(mid31);
                
                // Créer 4 nouveaux triangles (subdivision classique mais contrôlée)
                newTriangles.Add(v1);
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid31Index);
                
                newTriangles.Add(v2);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid12Index);
                
                newTriangles.Add(v3);
                newTriangles.Add(mid31Index);
                newTriangles.Add(mid23Index);
                
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid31Index);
            }
            
            // Remplacer les anciens triangles par les nouveaux
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }
        
        // === MÉTHODES DE SUBDIVISION TRIANGULAIRE ===
        
        void CreateTriangularSubdivision(List<Vector3> vertices, List<int> triangles, int targetDivisions) {
            Debug.Log($"🔺 Début subdivision triangulaire pour {targetDivisions} divisions");
            
            // Subdivision classique jusqu'à la limite sûre
            int safeDivisions = Mathf.Min(targetDivisions, maxSafeDivisions);
            for (int division = 0; division < safeDivisions; division++) {
                SubdivideSphere(vertices, triangles);
                Debug.Log($"📊 Division classique {division}: {vertices.Count} vertices, {triangles.Count/3} triangles");
            }
            
            // Subdivision triangulaire pour les divisions restantes
            int remainingDivisions = targetDivisions - safeDivisions;
            if (remainingDivisions > 0) {
                Debug.Log($"🔺 Subdivision triangulaire pour {remainingDivisions} divisions restantes");
                for (int i = 0; i < remainingDivisions; i++) {
                    SubdivideTriangular(vertices, triangles);
                    Debug.Log($"📊 Division triangulaire {i}: {vertices.Count} vertices, {triangles.Count/3} triangles");
                }
            }
        }
        
        void SubdivideTriangular(List<Vector3> vertices, List<int> triangles) {
            // Créer une nouvelle liste de triangles
            List<int> newTriangles = new List<int>();
            
            // Pour chaque triangle existant, le diviser en 4 triangles plus équilibrés
            for (int i = 0; i < triangles.Count; i += 3) {
                int v1 = triangles[i];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 2];
                
                // Calculer les milieux des arêtes
                Vector3 mid12 = ((vertices[v1] + vertices[v2]) / 2f).normalized;
                Vector3 mid23 = ((vertices[v2] + vertices[v3]) / 2f).normalized;
                Vector3 mid31 = ((vertices[v3] + vertices[v1]) / 2f).normalized;
                
                // Ajouter les nouveaux vertices
                int mid12Index = vertices.Count;
                vertices.Add(mid12);
                int mid23Index = vertices.Count;
                vertices.Add(mid23);
                int mid31Index = vertices.Count;
                vertices.Add(mid31);
                
                // Créer 4 nouveaux triangles plus équilibrés
                // Triangle central
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid31Index);
                
                // Triangle 1
                newTriangles.Add(v1);
                newTriangles.Add(mid12Index);
                newTriangles.Add(mid31Index);
                
                // Triangle 2
                newTriangles.Add(v2);
                newTriangles.Add(mid23Index);
                newTriangles.Add(mid12Index);
                
                // Triangle 3
                newTriangles.Add(v3);
                newTriangles.Add(mid31Index);
                newTriangles.Add(mid23Index);
            }
            
            // Remplacer les anciens triangles par les nouveaux
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }
        
        // === MÉTHODES DE SUBDIVISION ADAPTATIVE ===
        
        void CreateAdaptiveSphereMesh(List<Vector3> baseVertices, List<int> baseTriangles, 
            List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
            
            if (useChunkedGeneration) {
                CreateChunkedSphereMesh(baseVertices, baseTriangles, vertices, uvs, triangles);
            } else {
                CreateProgressiveSphereMesh(baseVertices, baseTriangles, vertices, uvs, triangles);
            }
        }
        
        void CreateChunkedSphereMesh(List<Vector3> baseVertices, List<int> baseTriangles, 
            List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
            
            // Subdivision par chunks pour éviter l'explosion mémoire
            int totalDivisions = divisions;
            int safeDivisions = Mathf.Min(totalDivisions, maxSafeDivisions);
            int remainingDivisions = totalDivisions - safeDivisions;
            
            // Subdivision de base sûre
            for (int division = 0; division < safeDivisions; division++) {
                SubdivideSphere(baseVertices, baseTriangles);
            }
            
            // Subdivision progressive par chunks pour les divisions restantes
            if (remainingDivisions > 0) {
                CreateProgressiveSubdivision(baseVertices, baseTriangles, remainingDivisions);
            }
            
            // Appliquer les hauteurs
            ApplyHeightsToMesh(baseVertices, baseTriangles, vertices, uvs, triangles);
        }
        
        void CreateProgressiveSphereMesh(List<Vector3> baseVertices, List<int> baseTriangles, 
            List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
            
            // Subdivision progressive avec optimisation mémoire
            for (int division = 0; division < divisions; division++) {
                if (baseVertices.Count > 50000) { // Limite de sécurité plus stricte
                    Debug.LogWarning($"⚠️ Limite de vertices atteinte à la division {division}. Arrêt de la subdivision progressive.");
                    break;
                }
                SubdivideSphere(baseVertices, baseTriangles);
                Debug.Log($"📊 Division {division}: {baseVertices.Count} vertices, {baseTriangles.Count/3} triangles");
            }
            
            // Appliquer les hauteurs
            ApplyHeightsToMesh(baseVertices, baseTriangles, vertices, uvs, triangles);
        }
        
        void CreateProgressiveSubdivision(List<Vector3> vertices, List<int> triangles, int remainingDivisions) {
            // Subdivision progressive avec gestion mémoire
            for (int i = 0; i < remainingDivisions; i++) {
                if (vertices.Count > 500000) { // Limite de sécurité
                    Debug.LogWarning($"⚠️ Limite de vertices atteinte. Arrêt de la subdivision.");
                    break;
                }
                SubdivideSphere(vertices, triangles);
            }
        }
        
        void ApplyHeightsToMesh(List<Vector3> baseVertices, List<int> baseTriangles, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
            // Appliquer les hauteurs et créer le mesh final
            for (int i = 0; i < baseVertices.Count; i++) {
                Vector3 vertex = baseVertices[i];
                float height = GenerateHeight(vertex);
                
                // Nouveau système d'océans qui préserve la forme de base
                if (useAdvancedOceanSystem && preserveBaseShape) {
                    height = ApplyAdvancedOceanSystem(vertex, height);
                } else {
                    // Ancien système (pour compatibilité)
                    if (useFlatOceans && height <= waterLevel) {
                        height = 0f; // Océans parfaitement plats
                    } else if (height > waterLevel) {
                        // Pour les terres, ajuster la hauteur
                        if (forceOceanLevel) {
                            height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
                        }
                    }
                }
                
                Vector3 finalVertex = vertex * (radius + height);
                
                vertices.Add(finalVertex);
                uvs.Add(new Vector2(vertex.x, vertex.y));
            }
            
            // Copier les triangles
            triangles.AddRange(baseTriangles);
        }
        
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
        
        void CreateMultiMaterialMesh(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
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
                if (avgHeight < waterLevel) {
                    waterTriangles.Add(p1);
                    waterTriangles.Add(p2);
                    waterTriangles.Add(p3);
                } else if (avgHeight < mountainLevel) {
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
            mesh.name = "HexaAstralithPlanet";
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            
            // Créer les submeshes
            mesh.subMeshCount = 3;
            mesh.SetTriangles(waterTriangles.ToArray(), 0);
            mesh.SetTriangles(landTriangles.ToArray(), 1);
            mesh.SetTriangles(mountainTriangles.ToArray(), 2);
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            meshFilter.mesh = mesh;
            
            // Sauvegarder le mesh original pour le LOD 0
            originalMesh = mesh;
            originalMaterials = new Material[3];
            originalMaterials[0] = waterMaterial;
            originalMaterials[1] = landMaterial;
            originalMaterials[2] = mountainMaterial;
            
            // Appliquer les matériaux
            ApplyMultiMaterials();
            
            Debug.Log($"🎨 Planète créée: {waterTriangles.Count/3} triangles eau, {landTriangles.Count/3} triangles terre, {mountainTriangles.Count/3} triangles montagne");
        }
        
        float GetVertexHeight(Vector3 vertex) {
            Vector3 normalizedVertex = vertex.normalized;
            return GenerateHeight(normalizedVertex);
        }
        
        void ApplyMultiMaterials() {
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
            
            Debug.Log("🎨 Matériaux de la planète appliqués !");
        }
        
        // Méthodes de génération de sphère
        List<Vector3> CreateIcosahedronVertices() {
            List<Vector3> vertices = new List<Vector3>();
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            
            Vector3[] icosahedronVertices = {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            
            foreach (Vector3 vertex in icosahedronVertices) {
                vertices.Add(vertex.normalized);
            }
            
            return vertices;
        }
        
        List<int> CreateIcosahedronTriangles() {
            List<int> triangles = new List<int>();
            
            int[][] icosahedronFaces = {
                new int[] {0, 11, 5}, new int[] {0, 5, 1}, new int[] {0, 1, 7}, new int[] {0, 7, 10}, new int[] {0, 10, 11},
                new int[] {1, 5, 9}, new int[] {5, 11, 4}, new int[] {11, 10, 2}, new int[] {10, 7, 6}, new int[] {7, 1, 8},
                new int[] {3, 9, 4}, new int[] {3, 4, 2}, new int[] {3, 2, 6}, new int[] {3, 6, 8}, new int[] {3, 8, 9},
                new int[] {4, 9, 5}, new int[] {2, 4, 11}, new int[] {6, 2, 10}, new int[] {8, 6, 7}, new int[] {9, 8, 1}
            };
            
            foreach (int[] face in icosahedronFaces) {
                triangles.Add(face[0]);
                triangles.Add(face[1]);
                triangles.Add(face[2]);
            }
            
            return triangles;
        }
        
        void SubdivideSphere(List<Vector3> vertices, List<int> triangles) {
            List<int> newTriangles = new List<int>();
            Dictionary<string, int> edgePoints = new Dictionary<string, int>();
            
            for (int i = 0; i < triangles.Count; i += 3) {
                int p1 = triangles[i];
                int p2 = triangles[i + 1];
                int p3 = triangles[i + 2];
                
                // Créer les points du milieu des arêtes
                int mid1 = GetOrCreateMidPoint(p1, p2, vertices, edgePoints);
                int mid2 = GetOrCreateMidPoint(p2, p3, vertices, edgePoints);
                int mid3 = GetOrCreateMidPoint(p3, p1, vertices, edgePoints);
                
                // Créer 4 nouveaux triangles
                newTriangles.Add(p1); newTriangles.Add(mid1); newTriangles.Add(mid3);
                newTriangles.Add(mid1); newTriangles.Add(p2); newTriangles.Add(mid2);
                newTriangles.Add(mid3); newTriangles.Add(mid2); newTriangles.Add(p3);
                newTriangles.Add(mid1); newTriangles.Add(mid2); newTriangles.Add(mid3);
            }
            
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }
        
        int GetOrCreateMidPoint(int p1, int p2, List<Vector3> vertices, Dictionary<string, int> edgePoints) {
            string key1 = $"{p1}-{p2}";
            string key2 = $"{p2}-{p1}";
            
            if (edgePoints.ContainsKey(key1)) return edgePoints[key1];
            if (edgePoints.ContainsKey(key2)) return edgePoints[key2];
            
            Vector3 midPoint = (vertices[p1] + vertices[p2]) / 2f;
            midPoint = midPoint.normalized;
            
            vertices.Add(midPoint);
            int newIndex = vertices.Count - 1;
            edgePoints[key1] = newIndex;
            
            return newIndex;
        }
        
        Material CreateDefaultMaterial(Color color, string name) {
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = name;
            mat.color = color;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Smoothness", 0.5f);
            return mat;
        }
        
        // Méthodes utilitaires
        public void ToggleHeightControl() {
            useHeightControl = !useHeightControl;
            if (useHeightControl) {
                CalculateLevelsFromHeights();
            }
            Debug.Log($"Contrôle de hauteur: {(useHeightControl ? "ON" : "OFF")}");
        }
        
        public void SetObjectTag(string newTag) {
            if (string.IsNullOrEmpty(newTag)) {
                Debug.LogWarning("⚠️ Tag vide fourni");
                return;
            }
            
            objectTag = newTag;
            ApplyTagToObject();
        }
        
        public void ApplyTagNow() {
            ApplyTagToObject();
        }
        
        void CalculateLevelsFromHeights() {
            if (!useHeightControl) return;
            
            float range = maxHeight - minHeight;
            waterLevel = minHeight + (range * 0.3f);
            mountainLevel = minHeight + (range * 0.7f);
            
            Debug.Log($"📊 Niveaux calculés: Min={minHeight:F2}, Max={maxHeight:F2}, Eau={waterLevel:F2}, Montagne={mountainLevel:F2}");
        }
        
        void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 400, 350));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🌍 HexaAstralith Planet", GUI.skin.box);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Générer Planète")) {
                GeneratePlanet();
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("Contrôles:");
            GUILayout.Label("R : Régénérer");
            GUILayout.Label("H : Toggle contrôle hauteur");
            
            GUILayout.Space(10);
            
            GUILayout.Label("Fonctionnalités:");
            GUILayout.Label("✅ Bruit sphérique");
            GUILayout.Label("✅ Multi-matériaux");
            GUILayout.Label("✅ Océans plats");
            GUILayout.Label("✅ Bruit avancé");
            GUILayout.Label("✅ Contrôle hauteurs");
            GUILayout.Label("✅ Ridges et détails");
            
            GUILayout.Space(10);
            
            GUILayout.Label("Statut:");
            GUILayout.Label($"Bruit avancé: {(useAdvancedNoise ? "ON" : "OFF")}");
            GUILayout.Label($"Océans plats: {(useFlatOceans ? "ON" : "OFF")}");
            GUILayout.Label($"Contrôle hauteur: {(useHeightControl ? "ON" : "OFF")}");
            GUILayout.Label($"Ridges: {(useRidgeNoise ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("LOD:");
            GUILayout.Label($"LOD actuel: {currentLOD}");
            GUILayout.Label($"Distance: {currentDistance:F1}");
            GUILayout.Label($"LOD activé: {(enableLOD ? "ON" : "OFF")}");
            GUILayout.Label($"Frontières: {(enableBoundaryLOD ? "ON" : "OFF")}");
            GUILayout.Label($"Transitions: {(enableTransitionSmoothing ? "ON" : "OFF")}");
            GUILayout.Label($"Océans préservés: {(preserveOceanStructure ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Océans avancés:");
            GUILayout.Label($"Système avancé: {(useAdvancedOceanSystem ? "ON" : "OFF")}");
            GUILayout.Label($"Forme préservée: {(preserveBaseShape ? "ON" : "OFF")}");
            GUILayout.Label($"Force aplatissement: {oceanFlatteningStrength:F2}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Optimisation:");
            GUILayout.Label($"Subdivision adaptative: {(useAdaptiveSubdivision ? "ON" : "OFF")}");
            GUILayout.Label($"Divisions sûres max: {maxSafeDivisions}");
            GUILayout.Label($"Génération par chunks: {(useChunkedGeneration ? "ON" : "OFF")}");
            GUILayout.Label($"Taille chunk: {chunkSize}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Subdivision triangulaire:");
            GUILayout.Label($"Triangulaire: {(useTriangularSubdivision ? "ON" : "OFF")}");
            GUILayout.Label($"Max divisions: {maxTriangularDivisions}");
            GUILayout.Label($"Subdivision simple: {(useSimpleEdgeSubdivision ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Limites de sécurité:");
            GUILayout.Label($"Max vertices: {maxVerticesLimit:N0}");
            GUILayout.Label($"Limites intelligentes: {(useSmartLimits ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Subdivision Hexasphere:");
            GUILayout.Label($"Hexasphere: {(useHexasphereSubdivision ? "ON" : "OFF")}");
            GUILayout.Label($"Max divisions: {hexasphereMaxDivisions}");
            GUILayout.Label($"Mode sécurisé: {(useSafeMode ? "ON" : "OFF")}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Subdivision intelligente:");
            GUILayout.Label($"Intelligente: {(useIntelligentSubdivision ? "ON" : "OFF")}");
            GUILayout.Label($"Max divisions: {maxIntelligentDivisions}");
            GUILayout.Label($"Qualité: {subdivisionQuality:F2}");
            
            GUILayout.Space(5);
            
            GUILayout.Label("Subdivision hexagone:");
            GUILayout.Label($"Hexagone: {(useHexagonSubdivision ? "ON" : "OFF")}");
            GUILayout.Label($"Max divisions: {hexagonMaxDivisions}");
            GUILayout.Label($"Rangées: {(useHexagonRows ? "ON" : "OFF")}");
            
            GUILayout.Space(10);
            
            GUILayout.Label("Matériaux:");
            GUILayout.Label($"Eau: {(waterMaterial != null ? "✅" : "❌")}");
            GUILayout.Label($"Terre: {(landMaterial != null ? "✅" : "❌")}");
            GUILayout.Label($"Montagne: {(mountainMaterial != null ? "✅" : "❌")}");
            
            GUILayout.Space(10);
            
            GUILayout.Label("Tag:");
            GUILayout.Label($"Tag actuel: {objectTag}");
            GUILayout.Label($"Tag appliqué: {gameObject.tag}");
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🏷️ Appliquer Tag Maintenant")) {
                ApplyTagNow();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🎯 Forcer LOD 0")) {
                SetLODLevel(0);
            }
            if (GUILayout.Button("🎯 Forcer LOD 1")) {
                SetLODLevel(1);
            }
            if (GUILayout.Button("🎯 Forcer LOD 2")) {
                SetLODLevel(2);
            }
            if (GUILayout.Button("🎯 Forcer LOD 3")) {
                SetLODLevel(3);
            }
            if (GUILayout.Button("🔄 Mise à jour LOD")) {
                ForceLODUpdate();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🌊 Toggle Système Océans Avancé")) {
                useAdvancedOceanSystem = !useAdvancedOceanSystem;
                GeneratePlanet(); // Régénérer avec le nouveau système
            }
            
            if (GUILayout.Button("🔧 Toggle Préservation Forme")) {
                preserveBaseShape = !preserveBaseShape;
                GeneratePlanet(); // Régénérer avec le nouveau système
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔄 Régénérer LOD 0")) {
                RegenerateLODLevel(0);
            }
            if (GUILayout.Button("🔄 Régénérer LOD 1")) {
                RegenerateLODLevel(1);
            }
            if (GUILayout.Button("🔄 Régénérer LOD 2")) {
                RegenerateLODLevel(2);
            }
            if (GUILayout.Button("🔄 Régénérer LOD 3")) {
                RegenerateLODLevel(3);
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🌍 Régénérer Planète Complète")) {
                RegeneratePlanet();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("⚡ Toggle Subdivision Adaptative")) {
                useAdaptiveSubdivision = !useAdaptiveSubdivision;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("📦 Toggle Génération par Chunks")) {
                useChunkedGeneration = !useChunkedGeneration;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("⚡ Forcer Subdivision Adaptative")) {
                ForceAdaptiveSubdivision();
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("📊 Afficher Infos Divisions")) {
                ShowDivisionInfo();
            }
            
            if (GUILayout.Button("🔺 Toggle Subdivision Triangulaire")) {
                useTriangularSubdivision = !useTriangularSubdivision;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🔧 Toggle Subdivision Simple")) {
                useSimpleEdgeSubdivision = !useSimpleEdgeSubdivision;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🛡️ Augmenter Limite Vertices")) {
                maxVerticesLimit = Mathf.Min(maxVerticesLimit * 2, 1000000);
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🛡️ Réduire Limite Vertices")) {
                maxVerticesLimit = Mathf.Max(maxVerticesLimit / 2, 10000);
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🔺 Toggle Subdivision Hexasphere")) {
                useHexasphereSubdivision = !useHexasphereSubdivision;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🛡️ Toggle Mode Sécurisé")) {
                useSafeMode = !useSafeMode;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🎯 Toggle Subdivision Intelligente")) {
                useIntelligentSubdivision = !useIntelligentSubdivision;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🎯 Augmenter Qualité")) {
                subdivisionQuality = Mathf.Min(subdivisionQuality + 0.1f, 1.0f);
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🎯 Réduire Qualité")) {
                subdivisionQuality = Mathf.Max(subdivisionQuality - 0.1f, 0.1f);
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🔷 Toggle Subdivision Hexagone")) {
                useHexagonSubdivision = !useHexagonSubdivision;
                RegeneratePlanet();
            }
            
            if (GUILayout.Button("🔷 Toggle Rangées Hexagone")) {
                useHexagonRows = !useHexagonRows;
                RegeneratePlanet();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        // === SYSTÈME LOD ===
        
        void UpdateLOD() {
            if (cameraTransform == null) {
                Camera mainCamera = Camera.main;
                if (mainCamera != null) {
                    cameraTransform = mainCamera.transform;
                } else {
                    return;
                }
            }
            
            // Calculer la distance à la caméra
            currentDistance = Vector3.Distance(transform.position, cameraTransform.position);
            
            // Déterminer le niveau LOD approprié
            int newLOD = DetermineLODLevel(currentDistance);
            
            // Appliquer le nouveau niveau LOD si nécessaire
            if (newLOD != currentLOD) {
                ApplyLODLevel(newLOD);
            }
            
            // Culling si trop loin
            if (currentDistance > cullDistance) {
                SetVisibility(false);
            } else {
                SetVisibility(true);
            }
        }
        
        int DetermineLODLevel(float distance) {
            if (distance <= lod0Distance) return 0;
            if (distance <= lod1Distance) return 1;
            if (distance <= lod2Distance) return 2;
            if (distance <= lod3Distance) return 3;
            return 3; // LOD le plus bas par défaut
        }
        
        void ApplyLODLevel(int lodLevel) {
            currentLOD = lodLevel;
            
            // Pour le LOD 0, utiliser le mesh original
            if (lodLevel == 0 && originalMesh != null) {
                if (meshFilter != null) {
                    meshFilter.mesh = originalMesh;
                }
                if (meshRenderer != null && originalMaterials != null) {
                    meshRenderer.materials = originalMaterials;
                }
                return;
            }
            
            // Générer le mesh LOD si pas encore en cache
            if (!lodMeshes.ContainsKey(lodLevel)) {
                GenerateLODMesh(lodLevel);
            }
            
            // Appliquer le mesh LOD
            if (meshFilter != null && lodMeshes.ContainsKey(lodLevel)) {
                meshFilter.mesh = lodMeshes[lodLevel];
            }
            
            // Appliquer les matériaux LOD
            if (meshRenderer != null && lodMaterials.ContainsKey(lodLevel)) {
                meshRenderer.materials = lodMaterials[lodLevel];
            }
            
            if (showDebugInfo) {
                Debug.Log($"🎯 LOD changé vers niveau {lodLevel} (distance: {currentDistance:F1})");
            }
        }
        
        void GenerateLODMesh(int lodLevel) {
            // Créer un mesh temporaire pour ce niveau LOD
            Mesh tempMesh = new Mesh();
            tempMesh.name = $"HexaLOD_{lodLevel}";
            
            // Générer les vertices et triangles pour ce niveau LOD avec matériaux
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();
            List<int> waterTriangles = new List<int>();
            List<int> landTriangles = new List<int>();
            List<int> mountainTriangles = new List<int>();
            
            // Utiliser la méthode de génération existante avec moins de divisions
            CreateLODSphereMeshWithMaterials(vertices, uvs, triangles, waterTriangles, landTriangles, mountainTriangles, lodLevel);
            
            // Créer le mesh final avec submeshes
            tempMesh.vertices = vertices.ToArray();
            tempMesh.uv = uvs.ToArray();
            
            // Créer les submeshes pour les différents matériaux
            tempMesh.subMeshCount = 3;
            tempMesh.SetTriangles(waterTriangles, 0);
            tempMesh.SetTriangles(landTriangles, 1);
            tempMesh.SetTriangles(mountainTriangles, 2);
            
            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();
            
            lodMeshes[lodLevel] = tempMesh;
            
            // Créer les matériaux pour ce niveau LOD
            Material[] materials = CreateMaterialsForLOD(lodLevel);
            lodMaterials[lodLevel] = materials;
        }
        
        void CreateLODSphereMeshWithMaterials(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, 
            List<int> waterTriangles, List<int> landTriangles, List<int> mountainTriangles, int lodLevel) {
            // Créer un icosaèdre de base
            List<Vector3> baseVertices = CreateIcosahedronVertices();
            List<int> baseTriangles = CreateIcosahedronTriangles();
            
            // Subdiviser selon le niveau LOD
            int divisions = GetDivisionsForLOD(lodLevel);
            for (int division = 0; division < divisions; division++) {
                SubdivideSphere(baseVertices, baseTriangles);
            }
            
            // Appliquer les hauteurs et créer le mesh final
            for (int i = 0; i < baseVertices.Count; i++) {
                Vector3 vertex = baseVertices[i];
                float height = GenerateHeightForLOD(vertex, lodLevel);
                
                Vector3 finalVertex = vertex * (radius + height);
                vertices.Add(finalVertex);
                uvs.Add(new Vector2(vertex.x, vertex.y));
            }
            
            // Copier les triangles et les assigner aux bons matériaux
            for (int i = 0; i < baseTriangles.Count; i += 3) {
                int v1 = baseTriangles[i];
                int v2 = baseTriangles[i + 1];
                int v3 = baseTriangles[i + 2];
                
                // Déterminer le type de terrain pour ce triangle
                TerrainType terrainType = DetermineTerrainTypeForTriangle(vertices[v1], vertices[v2], vertices[v3]);
                
                // Assigner le triangle au bon submesh
                switch (terrainType) {
                    case TerrainType.Water:
                        waterTriangles.Add(v1);
                        waterTriangles.Add(v2);
                        waterTriangles.Add(v3);
                        break;
                    case TerrainType.Land:
                        landTriangles.Add(v1);
                        landTriangles.Add(v2);
                        landTriangles.Add(v3);
                        break;
                    case TerrainType.Mountain:
                        mountainTriangles.Add(v1);
                        mountainTriangles.Add(v2);
                        mountainTriangles.Add(v3);
                        break;
                }
                
                // Ajouter aussi au mesh principal
                triangles.Add(v1);
                triangles.Add(v2);
                triangles.Add(v3);
            }
        }
        
        void CreateLODSphereMesh(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, int lodLevel) {
            // Créer un icosaèdre de base
            List<Vector3> baseVertices = CreateIcosahedronVertices();
            List<int> baseTriangles = CreateIcosahedronTriangles();
            
            // Subdiviser selon le niveau LOD
            int divisions = GetDivisionsForLOD(lodLevel);
            for (int division = 0; division < divisions; division++) {
                SubdivideSphere(baseVertices, baseTriangles);
            }
            
            // Appliquer les hauteurs et créer le mesh final
            for (int i = 0; i < baseVertices.Count; i++) {
                Vector3 vertex = baseVertices[i];
                float height = GenerateHeightForLOD(vertex, lodLevel);
                
                Vector3 finalVertex = vertex * (radius + height);
                vertices.Add(finalVertex);
                uvs.Add(new Vector2(vertex.x, vertex.y));
            }
            
            // Copier les triangles
            triangles.AddRange(baseTriangles);
        }
        
        float GenerateHeightForLOD(Vector3 position, int lodLevel) {
            // Réduire la complexité du bruit selon le niveau LOD
            float noiseScaleLOD = noiseScale;
            float heightAmplitudeLOD = heightAmplitude;
            
            // Réduire seulement la complexité du bruit, pas l'amplitude globale
            if (lodLevel >= 2) {
                noiseScaleLOD *= 0.5f;
                // Préserver l'amplitude pour maintenir les océans
            }
            if (lodLevel >= 3) {
                noiseScaleLOD *= 0.3f;
                // Préserver l'amplitude pour maintenir les océans
            }
            
            // Générer le bruit avec la complexité réduite mais amplitude préservée
            float height = GeneratePerlinHeightLOD(position, noiseScaleLOD, heightAmplitudeLOD, lodLevel);
            
            // Appliquer le système d'océans avancé pour le LOD aussi
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(position, height);
            } else {
                // Ancien système (pour compatibilité)
                if (useFlatOceans && height <= waterLevel) {
                    height = 0f; // Océans parfaitement plats
                } else if (height > waterLevel) {
                    // Pour les terres, ajuster la hauteur
                    if (forceOceanLevel) {
                        height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            return height;
        }
        
        float GeneratePerlinHeightLOD(Vector3 position, float noiseScaleLOD, float heightAmplitudeLOD, int lodLevel) {
            // Version simplifiée du bruit pour les niveaux LOD élevés
            float latitude = Mathf.Asin(position.y);
            float longitude = Mathf.Atan2(position.z, position.x);
            
            float u = (longitude + Mathf.PI) / (2f * Mathf.PI);
            float v = (latitude + Mathf.PI / 2f) / Mathf.PI;
            
            // Moins d'octaves pour les niveaux LOD élevés, mais garder au moins 2 octaves
            float height = 0f;
            float frequency = 1f;
            float amplitude = heightAmplitudeLOD;
            float maxValue = 0f;
            
            int octaves = Mathf.Max(2, 6 - lodLevel); // Réduire les octaves mais garder au moins 2
            
            for (int i = 0; i < octaves; i++) {
                float noiseValue = Mathf.PerlinNoise(
                    u * noiseScaleLOD * frequency,
                    v * noiseScaleLOD * frequency
                );
                height += noiseValue * amplitude;
                maxValue += amplitude;
                frequency *= 2f;
                amplitude *= 0.5f;
            }
            
            if (maxValue > 0) {
                height = height / maxValue;
            }
            
            // Pour les niveaux LOD élevés, ajouter un bruit de base pour maintenir la structure
            if (lodLevel >= 2 && preserveOceanStructure) {
                float baseNoise = Mathf.PerlinNoise(u * noiseScaleLOD * 0.5f, v * noiseScaleLOD * 0.5f);
                height = Mathf.Lerp(height, baseNoise, oceanStructureStrength); // Mélanger avec un bruit de base
            }
            
            return height;
        }
        
        Material[] CreateMaterialsForLOD(int lodLevel) {
            Material[] materials = new Material[3]; // Eau, Terre, Montagne
            
            // Utiliser les matériaux existants pour tous les niveaux LOD
            materials[0] = waterMaterial;
            materials[1] = landMaterial;
            materials[2] = mountainMaterial;
            
            return materials;
        }
        
        int GetDivisionsForLOD(int lodLevel) {
            switch (lodLevel) {
                case 0: return lod0Divisions;
                case 1: return lod1Divisions;
                case 2: return lod2Divisions;
                case 3: return lod3Divisions;
                default: return lod3Divisions;
            }
        }
        
        void SetVisibility(bool visible) {
            if (meshRenderer != null) {
                meshRenderer.enabled = visible;
            }
        }
        
        // Méthodes publiques LOD
        public void SetLODLevel(int lodLevel) {
            if (lodLevel >= 0 && lodLevel <= 3) {
                ApplyLODLevel(lodLevel);
            }
        }
        
        public int GetCurrentLOD() {
            return currentLOD;
        }
        
        public float GetCurrentDistance() {
            return currentDistance;
        }
        
        public void ForceLODUpdate() {
            UpdateLOD();
        }
        
        // Méthode pour forcer la régénération d'un niveau LOD spécifique
        public void RegenerateLODLevel(int lodLevel) {
            if (lodMeshes.ContainsKey(lodLevel)) {
                lodMeshes.Remove(lodLevel);
            }
            if (lodMaterials.ContainsKey(lodLevel)) {
                lodMaterials.Remove(lodLevel);
            }
            
            // Régénérer le mesh LOD
            GenerateLODMesh(lodLevel);
            
            // Appliquer immédiatement si c'est le niveau actuel
            if (currentLOD == lodLevel) {
                ApplyLODLevel(lodLevel);
            }
        }
        
        // Méthode pour forcer la régénération complète de la planète
        public void RegeneratePlanet() {
            // Vider le cache LOD
            lodMeshes.Clear();
            lodMaterials.Clear();
            
            // Régénérer le mesh original
            CreateMesh();
            
            // Réinitialiser le LOD
            currentLOD = 0;
            ForceLODUpdate();
        }
        
        // Méthode pour forcer l'activation de la subdivision adaptative
        public void ForceAdaptiveSubdivision() {
            useAdaptiveSubdivision = true;
            maxSafeDivisions = 5; // Plus strict
            useChunkedGeneration = true;
            Debug.Log("⚡ Subdivision adaptative forcée - max sûres: 5");
        }
        
        // Méthode pour calculer le nombre de vertices attendu
        public int CalculateExpectedVertices(int divisions) {
            // Icosaèdre de base : 12 vertices, 20 triangles
            int vertices = 12;
            int triangles = 20;
            
            for (int i = 0; i < divisions; i++) {
                // Chaque division multiplie les triangles par 4
                triangles *= 4;
                // Vertices = triangles * 3 / 2 (approximation)
                vertices = triangles * 3 / 2;
            }
            
            return vertices;
        }
        
        // Méthode pour afficher les informations de debug
        public void ShowDivisionInfo() {
            int expectedVertices = CalculateExpectedVertices(divisions);
            Debug.Log($"📊 Informations pour {divisions} divisions:");
            Debug.Log($"   Vertices attendus: {expectedVertices:N0}");
            Debug.Log($"   Triangles attendus: {expectedVertices * 2 / 3:N0}");
            Debug.Log($"   Limite de sécurité: 100,000 vertices");
            Debug.Log($"   Utilisation mémoire estimée: {(expectedVertices * 3 * 4) / 1024 / 1024:F1} MB");
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
        
        // Méthode pour calculer la hauteur de base sans océans (pour debug)
        public float GetBaseHeight(Vector3 vertex) {
            return GenerateHeight(vertex);
        }
        
        // Méthode pour calculer la hauteur avec océans appliqués
        public float GetHeightWithOceans(Vector3 vertex) {
            float baseHeight = GenerateHeight(vertex);
            return ApplyAdvancedOceanSystem(vertex, baseHeight);
        }
        
        // === MÉTHODES LOD AVANCÉES ===
        
        TerrainType DetermineTerrainTypeForTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            // Calculer la hauteur moyenne du triangle
            Vector3 center = (v1 + v2 + v3) / 3f;
            
            // Utiliser la même logique de génération que le mesh principal
            // pour assurer la cohérence
            float height = GenerateHeight(center.normalized);
            
            // Appliquer le système d'océans avancé si activé
            if (useAdvancedOceanSystem && preserveBaseShape) {
                height = ApplyAdvancedOceanSystem(center.normalized, height);
            } else {
                // Ancien système (pour compatibilité)
                if (useFlatOceans && height <= waterLevel) {
                    height = 0f; // Océans parfaitement plats
                } else if (height > waterLevel) {
                    // Pour les terres, ajuster la hauteur
                    if (forceOceanLevel) {
                        height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
                    }
                }
            }
            
            // Déterminer le type de terrain basé sur la hauteur
            if (height <= waterLevel) {
                return TerrainType.Water;
            } else if (height <= mountainLevel) {
                return TerrainType.Land;
            } else {
                return TerrainType.Mountain;
            }
        }
    }
}
