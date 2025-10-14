using UnityEngine;
using System.Collections.Generic;

namespace HexasphereProcedural {
    
    /// <summary>
    /// Générateur de forêts sur une planète procédurale
    /// Utilise la noise map 3D existante pour déterminer les zones terrestres
    /// et une noise map supplémentaire pour grouper les arbres par patch
    /// </summary>
    public class Forest : MonoBehaviour {
        
        [Header("🌳 Configuration des Arbres")]
        [SerializeField] public int treeCount = 200;
        [SerializeField] public float TreeScale = 1f;
        [SerializeField] public float treeSpacing = 2f;
        [SerializeField] public float treeScaleVariation = 0.3f;
        
        [Header("🌍 Zones Terrestres")]
        [SerializeField] public float landThreshold = 0.3f; // Seuil pour considérer une zone comme terrestre
        [SerializeField] public float oceanLevel = 0.0f; // Niveau de l'océan
        
        [Header("🌲 Groupement des Arbres")]
        [SerializeField] public float patchNoiseScale = 0.02f; // Échelle plus fine pour des patches plus denses
        [SerializeField] public float patchThreshold = 0.15f; // Seuil plus strict pour des forêts plus compactes
        [SerializeField] public int minTreesPerPatch = 8;
        [SerializeField] public int maxTreesPerPatch = 25;
        
        [Header("🌊 Intensité du Bruit")]
        [SerializeField] public float noiseIntensity = 2.5f; // Multiplicateur d'intensité du bruit
        [SerializeField] public int noiseOctaves = 3; // Nombre d'octaves pour un bruit plus complexe
        [SerializeField] public float noiseLacunarity = 2.0f; // Facteur de lacunarité
        [SerializeField] public float noisePersistence = 0.5f; // Persistance du bruit
        
        [Header("🎲 Variété des Arbres")]
        [SerializeField] public GameObject[] treePrefabs = new GameObject[3]; // Tree Type 1, 2, 3
        [SerializeField] public float[] treeTypeWeights = {0.4f, 0.35f, 0.25f}; // Probabilités pour chaque type
        
        [Header("🔧 Debug")]
        [SerializeField] public bool showDebugInfo = true;
        [SerializeField] public bool showTreePositions = false;
        [SerializeField] public Color debugColor = Color.green;
        
        [Header("🎮 Contrôles")]
        [SerializeField] public bool regenerateOnStart = true;
        
        // Références
        private HexasphereFill hexasphereFill;
        private Transform planetTransform;
        private Vector3 planetCenter;
        private float planetRadius;
        
        // Données des arbres
        [System.NonSerialized] public List<GameObject> spawnedTrees = new List<GameObject>();
        [System.NonSerialized] public List<Vector3> treePositions = new List<Vector3>();
        [System.NonSerialized] public List<Quaternion> treeRotations = new List<Quaternion>();
        
        // Noise maps
        private float[,] patchNoiseMap;
        
        void Start() {
            // Obtenir la référence vers HexasphereFill
            hexasphereFill = GetComponent<HexasphereFill>();
            if (hexasphereFill == null) {
                Debug.LogError("Forest: HexasphereFill component not found on the same GameObject!");
                return;
            }
            
            planetTransform = transform;
            planetCenter = planetTransform.position;
            planetRadius = hexasphereFill.radius;
            
            // Générer les arbres après un court délai pour s'assurer que la planète est générée
            if (regenerateOnStart) {
                Invoke(nameof(GenerateForest), 0.5f);
            }
        }
        
        void GenerateForest() {
            if (hexasphereFill == null) {
                Debug.LogError("Forest: HexasphereFill reference is null!");
                return;
            }
            
            Debug.Log($"Forest: Starting forest generation with {treeCount} trees...");
            
            // Nettoyer les arbres existants
            ClearForest();
            
            // Générer les noise maps
            GenerateNoiseMaps();
            
            // Placer les arbres
            PlaceTrees();
            
            Debug.Log($"Forest: Generated {spawnedTrees.Count} trees successfully!");
        }
        
        void GenerateNoiseMaps() {
            int mapSize = 256; // Taille de la noise map
            patchNoiseMap = new float[mapSize, mapSize];
            
            // Générer une noise map multi-octaves plus intense pour les patches d'arbres
            for (int x = 0; x < mapSize; x++) {
                for (int y = 0; y < mapSize; y++) {
                    // Générer un bruit fractal multi-octaves plus intense
                    float patchNoise = GenerateFractalNoise(x, y);
                    patchNoiseMap[x, y] = patchNoise;
                }
            }
        }
        
        float GenerateFractalNoise(float x, float y) {
            float noise = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float maxValue = 0f;
            
            // Générer plusieurs octaves de bruit
            for (int i = 0; i < noiseOctaves; i++) {
                float sampleX = x * patchNoiseScale * frequency;
                float sampleY = y * patchNoiseScale * frequency;
                
                // Utiliser Perlin noise avec des décalages pour éviter la répétition
                float perlinValue = Mathf.PerlinNoise(sampleX + 1000f, sampleY + 1000f);
                noise += perlinValue * amplitude;
                maxValue += amplitude;
                
                amplitude *= noisePersistence;
                frequency *= noiseLacunarity;
            }
            
            // Normaliser et appliquer l'intensité
            noise = noise / maxValue;
            noise = Mathf.Pow(noise, noiseIntensity);
            
            return noise;
        }
        
        float GetVertexHeight(Vector3 position) {
            // Utiliser la même méthode que HexasphereFill pour calculer la hauteur
            if (hexasphereFill == null) return 0f;
            
            // Normaliser la position
            Vector3 normalizedPosition = position.normalized;
            
            // Utiliser la méthode GenerateHeight de HexasphereFill
            // On va accéder à la méthode via reflection ou créer une méthode publique
            return CalculateHeightAtPosition(normalizedPosition);
        }
        
        float CalculateHeightAtPosition(Vector3 normalizedPosition) {
            // Utiliser la vraie méthode de génération de hauteur de HexasphereFill
            if (hexasphereFill == null) return 0f;
            
            // Utiliser directement la méthode GetVertexHeight de HexasphereFill
            return hexasphereFill.GetVertexHeight(normalizedPosition * planetRadius);
        }
        
        void PlaceTrees() {
            int treesPlaced = 0;
            int attempts = 0;
            int maxAttempts = treeCount * 10; // Limite pour éviter les boucles infinies
            int landAreaAttempts = 0;
            int patchAttempts = 0;
            int spacingAttempts = 0;
            
            Debug.Log($"Forest: Starting tree placement. WaterLevel={hexasphereFill.waterLevel}, MountainLevel={hexasphereFill.mountainLevel}");
            
            while (treesPlaced < treeCount && attempts < maxAttempts) {
                attempts++;
                
                // Générer une position aléatoire sur la sphère
                Vector3 randomPosition = GenerateRandomSpherePosition();
                
                // Vérifier si c'est une zone terrestre
                if (!IsLandArea(randomPosition)) {
                    landAreaAttempts++;
                    continue;
                }
                
                // Vérifier si c'est dans une zone de patch d'arbres
                if (!IsInTreePatch(randomPosition)) {
                    patchAttempts++;
                    continue;
                }
                
                // Vérifier la distance avec les autres arbres
                if (!IsValidTreePosition(randomPosition)) {
                    spacingAttempts++;
                    continue;
                }
                
                // Placer l'arbre
                PlaceTreeAtPosition(randomPosition);
                treesPlaced++;
            }
            
            Debug.Log($"Forest: Placement complete. Trees placed: {treesPlaced}, Total attempts: {attempts}");
            Debug.Log($"Forest: Failed due to land area: {landAreaAttempts}, patch: {patchAttempts}, spacing: {spacingAttempts}");
            
            if (treesPlaced < treeCount) {
                Debug.LogWarning($"Forest: Only placed {treesPlaced} out of {treeCount} requested trees. Consider adjusting parameters.");
            }
        }
        
        Vector3 GenerateRandomSpherePosition() {
            // Générer une position aléatoire sur la surface de la sphère
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 basePosition = planetCenter + randomDirection * planetRadius;
            
            // Calculer la vraie hauteur à cette position
            float height = GetVertexHeight(basePosition);
            
            // Ajuster la position avec la vraie hauteur
            return planetCenter + randomDirection * (planetRadius + height);
        }
        
        bool IsLandArea(Vector3 position) {
            // Utiliser la vraie hauteur calculée par HexasphereFill
            Vector3 normalizedPosition = (position - planetCenter).normalized;
            float height = GetVertexHeight(normalizedPosition * planetRadius);
            
            // Utiliser les niveaux de terrain ajustés de HexasphereFill
            float effectiveWaterLevel = hexasphereFill.GetEffectiveWaterLevel();
            float effectiveMountainLevel = hexasphereFill.GetEffectiveMountainLevel();
            
            bool isLand = height > effectiveWaterLevel && height <= effectiveMountainLevel;
            
            if (showDebugInfo && Random.value < 0.1f) {
                Debug.Log($"Forest: Position={position}, Height={height}, WaterLevel={effectiveWaterLevel}, MountainLevel={effectiveMountainLevel}, IsLand={isLand}");
            }
            
            return isLand;
        }
        
        bool IsInTreePatch(Vector3 position) {
            // Convertir la position en coordonnées de noise map
            Vector2 noiseCoords = WorldToNoiseMap(position);
            int x = Mathf.FloorToInt(noiseCoords.x * (patchNoiseMap.GetLength(0) - 1));
            int y = Mathf.FloorToInt(noiseCoords.y * (patchNoiseMap.GetLength(1) - 1));
            
            x = Mathf.Clamp(x, 0, patchNoiseMap.GetLength(0) - 1);
            y = Mathf.Clamp(y, 0, patchNoiseMap.GetLength(1) - 1);
            
            // Utiliser l'interpolation bilinéaire pour un bruit plus fluide
            float patchValue = GetInterpolatedNoiseValue(noiseCoords);
            
            // Appliquer un seuil plus strict pour des forêts plus compactes
            bool isInPatch = patchValue > patchThreshold;
            
            if (showDebugInfo && Random.value < 0.05f) {
                Debug.Log($"Forest: Patch check - Position={position}, NoiseValue={patchValue:F3}, Threshold={patchThreshold:F3}, IsInPatch={isInPatch}");
            }
            
            return isInPatch;
        }
        
        float GetInterpolatedNoiseValue(Vector2 noiseCoords) {
            // Interpolation bilinéaire pour un bruit plus fluide
            float x = noiseCoords.x * (patchNoiseMap.GetLength(0) - 1);
            float y = noiseCoords.y * (patchNoiseMap.GetLength(1) - 1);
            
            int x1 = Mathf.FloorToInt(x);
            int y1 = Mathf.FloorToInt(y);
            int x2 = Mathf.Min(x1 + 1, patchNoiseMap.GetLength(0) - 1);
            int y2 = Mathf.Min(y1 + 1, patchNoiseMap.GetLength(1) - 1);
            
            float fx = x - x1;
            float fy = y - y1;
            
            float n1 = patchNoiseMap[x1, y1];
            float n2 = patchNoiseMap[x2, y1];
            float n3 = patchNoiseMap[x1, y2];
            float n4 = patchNoiseMap[x2, y2];
            
            float i1 = Mathf.Lerp(n1, n2, fx);
            float i2 = Mathf.Lerp(n3, n4, fx);
            
            return Mathf.Lerp(i1, i2, fy);
        }
        
        Vector2 WorldToNoiseMap(Vector3 worldPosition) {
            // Convertir une position 3D en coordonnées 2D pour la noise map
            Vector3 direction = (worldPosition - planetCenter).normalized;
            
            // Projection sur une sphère unitaire
            float u = 0.5f + Mathf.Atan2(direction.z, direction.x) / (2f * Mathf.PI);
            float v = 0.5f - Mathf.Asin(direction.y) / Mathf.PI;
            
            return new Vector2(u, v);
        }
        
        bool IsValidTreePosition(Vector3 position) {
            // Vérifier la distance avec les autres arbres
            foreach (Vector3 treePos in treePositions) {
                if (Vector3.Distance(position, treePos) < treeSpacing) {
                    return false;
                }
            }
            return true;
        }
        
        void PlaceTreeAtPosition(Vector3 position) {
            // Choisir un type d'arbre basé sur les poids
            GameObject treePrefab = SelectTreeType();
            if (treePrefab == null) {
                Debug.LogWarning("Forest: No tree prefab available!");
                return;
            }
            
            // Créer l'arbre
            GameObject tree = Instantiate(treePrefab, position, Quaternion.identity, planetTransform);
            
            // Orienter l'arbre vers l'extérieur de la planète
            Vector3 directionToCenter = (planetCenter - position).normalized;
            // L'arbre doit pointer vers l'extérieur, donc utiliser la direction inverse
            tree.transform.rotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
            
            // Appliquer une variation d'échelle
            float scaleVariation = TreeScale + Random.Range(-treeScaleVariation, treeScaleVariation);
            tree.transform.localScale = Vector3.one * scaleVariation;
            
            // Ajouter à la liste
            spawnedTrees.Add(tree);
            treePositions.Add(position);
            treeRotations.Add(tree.transform.rotation);
        }
        
        GameObject SelectTreeType() {
            if (treePrefabs == null || treePrefabs.Length == 0) {
                return null;
            }
            
            float randomValue = Random.value;
            float cumulativeWeight = 0f;
            
            for (int i = 0; i < treePrefabs.Length; i++) {
                cumulativeWeight += treeTypeWeights[i];
                if (randomValue <= cumulativeWeight) {
                    return treePrefabs[i];
                }
            }
            
            // Fallback au premier arbre
            return treePrefabs[0];
        }
        
        public void ClearForest() {
            // Détruire tous les arbres existants
            foreach (GameObject tree in spawnedTrees) {
                if (tree != null) {
                    DestroyImmediate(tree);
                }
            }
            
            spawnedTrees.Clear();
            treePositions.Clear();
            treeRotations.Clear();
        }
        
        [ContextMenu("🌲 Regénérer les Arbres")]
        public void RegenerateForest() {
            Debug.Log("Forest: Regenerating forest...");
            GenerateForest();
        }
        
        [ContextMenu("🗑️ Effacer les Arbres")]
        public void ClearForestButton() {
            Debug.Log("Forest: Clearing forest...");
            ClearForest();
        }
        
        [ContextMenu("🔄 Effacer et Regénérer")]
        public void ClearAndRegenerate() {
            Debug.Log("Forest: Clearing and regenerating forest...");
            ClearForest();
            GenerateForest();
        }
        
        void OnDrawGizmos() {
            if (!showDebugInfo) return;
            
            // Dessiner les positions des arbres
            if (showTreePositions) {
                Gizmos.color = debugColor;
                foreach (Vector3 treePos in treePositions) {
                    Gizmos.DrawWireSphere(treePos, 0.1f);
                }
            }
            
            // Dessiner le centre de la planète
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(planetCenter, 0.2f);
        }
        
        bool IsPlanetOrChunk(Transform hitTransform) {
            // Vérifier si c'est la planète ciblée elle-même
            if (hitTransform == planetTransform) {
                return true;
            }
            
            // Vérifier si c'est un enfant de la planète ciblée (chunk)
            if (hitTransform.IsChildOf(planetTransform)) {
                return true;
            }
            
            // Vérifier si c'est un parent de la planète ciblée
            if (planetTransform.IsChildOf(hitTransform)) {
                return true;
            }
            
            return false;
        }
        
        int GetSubmeshIndex(RaycastHit hit) {
            // Calculer quel submesh a été touché
            Mesh mesh = hit.collider.GetComponent<MeshFilter>()?.mesh;
            if (mesh == null) return -1;
            
            // Trouver le triangle touché
            int triangleIndex = hit.triangleIndex;
            
            // Parcourir les submeshes pour trouver lequel contient ce triangle
            int currentTriangle = 0;
            for (int submesh = 0; submesh < mesh.subMeshCount; submesh++) {
                int[] triangles = mesh.GetTriangles(submesh);
                int submeshTriangleCount = triangles.Length / 3;
                
                if (triangleIndex < currentTriangle + submeshTriangleCount) {
                    return submesh;
                }
                
                currentTriangle += submeshTriangleCount;
            }
            
            return 0; // Fallback
        }
        
        void OnValidate() {
            // Valider les paramètres dans l'éditeur
            treeCount = Mathf.Max(0, treeCount);
            treeSpacing = Mathf.Max(0.01f, treeSpacing);
            treeScaleVariation = Mathf.Clamp(treeScaleVariation, 0f, 1f);
            landThreshold = Mathf.Clamp(landThreshold, 0f, 1f);
            patchThreshold = Mathf.Clamp(patchThreshold, 0f, 1f);
            minTreesPerPatch = Mathf.Max(1, minTreesPerPatch);
            maxTreesPerPatch = Mathf.Max(minTreesPerPatch, maxTreesPerPatch);
            
            // Valider les nouveaux paramètres de bruit
            noiseIntensity = Mathf.Max(0.1f, noiseIntensity);
            noiseOctaves = Mathf.Max(1, Mathf.Min(noiseOctaves, 8));
            noiseLacunarity = Mathf.Max(1.0f, noiseLacunarity);
            noisePersistence = Mathf.Clamp(noisePersistence, 0.1f, 1.0f);
        }
    }
}
