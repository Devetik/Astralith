using UnityEngine;
using System.Collections.Generic;

namespace HexasphereProcedural {
    
    /// <summary>
    /// Générateur de props sur une planète procédurale
    /// Place 3 catégories d'objets : rochers, herbes et autres arbres
    /// Utilise des noise maps séparées pour chaque type d'objet
    /// </summary>
    public class Props : MonoBehaviour {
        
        [Header("🗿 Configuration des Rochers")]
        [SerializeField] public int rockCount = 150;
        [SerializeField] public float rockScale = 1f;
        [SerializeField] public float rockSpacing = 3f;
        [SerializeField] public float rockScaleVariation = 0.4f;
        [SerializeField] public GameObject[] rockPrefabs = new GameObject[2];
        [SerializeField] public float[] rockTypeWeights = {0.6f, 0.4f};
        
        [Header("🌿 Configuration des Herbes")]
        [SerializeField] public int grassCount = 300;
        [SerializeField] public float grassScale = 0.8f;
        [SerializeField] public float grassSpacing = 1.5f;
        [SerializeField] public float grassScaleVariation = 0.3f;
        [SerializeField] public GameObject[] grassPrefabs = new GameObject[2];
        [SerializeField] public float[] grassTypeWeights = {0.5f, 0.5f};
        
        [Header("🌳 Configuration des Autres Arbres")]
        [SerializeField] public int otherTreeCount = 100;
        [SerializeField] public float otherTreeScale = 1.2f;
        [SerializeField] public float otherTreeSpacing = 4f;
        [SerializeField] public float otherTreeScaleVariation = 0.3f;
        [SerializeField] public GameObject[] otherTreePrefabs = new GameObject[2];
        [SerializeField] public float[] otherTreeTypeWeights = {0.4f, 0.6f};
        
        [Header("🌍 Zones Terrestres")]
        [SerializeField] public float landThreshold = 0.3f;
        [SerializeField] public float oceanLevel = 0.0f;
        
        [Header("🗿 Groupement des Rochers")]
        [SerializeField] public float rockPatchNoiseScale = 0.015f;
        [SerializeField] public float rockPatchThreshold = 0.2f;
        [SerializeField] public int minRocksPerPatch = 5;
        [SerializeField] public int maxRocksPerPatch = 20;
        
        [Header("🌿 Groupement des Herbes")]
        [SerializeField] public float grassPatchNoiseScale = 0.025f;
        [SerializeField] public float grassPatchThreshold = 0.15f;
        [SerializeField] public int minGrassPerPatch = 10;
        [SerializeField] public int maxGrassPerPatch = 30;
        
        [Header("🌳 Groupement des Autres Arbres")]
        [SerializeField] public float otherTreePatchNoiseScale = 0.02f;
        [SerializeField] public float otherTreePatchThreshold = 0.18f;
        [SerializeField] public int minOtherTreesPerPatch = 6;
        [SerializeField] public int maxOtherTreesPerPatch = 25;
        
        [Header("🌊 Intensité du Bruit")]
        [SerializeField] public float noiseIntensity = 2.5f;
        [SerializeField] public int noiseOctaves = 3;
        [SerializeField] public float noiseLacunarity = 2.0f;
        [SerializeField] public float noisePersistence = 0.5f;
        
        [Header("🔧 Debug Densitée")]
        [SerializeField] public bool showPlacementStats = true;
        [SerializeField] public bool forceDensePlacement = false;
        [SerializeField] public float maxAttemptsMultiplier = 20f;
        
        [Header("🔧 Debug")]
        [SerializeField] public bool showDebugInfo = true;
        [SerializeField] public bool showPropPositions = false;
        [SerializeField] public Color rockDebugColor = Color.gray;
        [SerializeField] public Color grassDebugColor = Color.green;
        [SerializeField] public Color otherTreeDebugColor = Color.yellow;
        
        [Header("🔄 Rotation des Props")]
        [SerializeField] public bool useRandomRotation = true; // Activer la rotation aléatoire
        [SerializeField] public float maxRotationX = 15f; // Rotation X max en degrés
        [SerializeField] public float maxRotationZ = 15f; // Rotation Z max en degrés
        [SerializeField] public float rotationDistribution = 2f; // Facteur de distribution (plus élevé = plus centré sur 0)
        
        [Header("🎮 Contrôles")]
        [SerializeField] public bool regenerateOnStart = true;
        [SerializeField] public bool generateRocks = true;
        [SerializeField] public bool generateGrass = true;
        [SerializeField] public bool generateOtherTrees = true;
        
        // Références
        private HexasphereFill hexasphereFill;
        private Transform planetTransform;
        private Vector3 planetCenter;
        private float planetRadius;
        
        // Données des props
        [System.NonSerialized] public List<GameObject> spawnedRocks = new List<GameObject>();
        [System.NonSerialized] public List<GameObject> spawnedGrass = new List<GameObject>();
        [System.NonSerialized] public List<GameObject> spawnedOtherTrees = new List<GameObject>();
        
        [System.NonSerialized] public List<Vector3> rockPositions = new List<Vector3>();
        [System.NonSerialized] public List<Vector3> grassPositions = new List<Vector3>();
        [System.NonSerialized] public List<Vector3> otherTreePositions = new List<Vector3>();
        
        [System.NonSerialized] public List<Quaternion> rockRotations = new List<Quaternion>();
        [System.NonSerialized] public List<Quaternion> grassRotations = new List<Quaternion>();
        [System.NonSerialized] public List<Quaternion> otherTreeRotations = new List<Quaternion>();
        
        // Noise maps séparées pour chaque type
        private float[,] rockPatchNoiseMap;
        private float[,] grassPatchNoiseMap;
        private float[,] otherTreePatchNoiseMap;
        
        void Start() {
            // Obtenir la référence vers HexasphereFill
            hexasphereFill = GetComponent<HexasphereFill>();
            if (hexasphereFill == null) {
                Debug.LogError("Props: HexasphereFill component not found on the same GameObject!");
                return;
            }
            
            planetTransform = transform;
            planetCenter = planetTransform.position;
            planetRadius = hexasphereFill.radius;
            
            // Générer les props après un court délai pour s'assurer que la planète est générée
            if (regenerateOnStart) {
                Invoke(nameof(GenerateProps), 0.5f);
            }
        }
        
        void GenerateProps() {
            if (hexasphereFill == null) {
                Debug.LogError("Props: HexasphereFill reference is null!");
                return;
            }
            
            Debug.Log("Props: Starting props generation...");
            
            // Nettoyer les props existants
            ClearAllProps();
            
            // Générer les noise maps
            GenerateNoiseMaps();
            
            // Placer les props selon les catégories activées
            if (generateRocks) {
                PlaceRocks();
            }
            
            if (generateGrass) {
                PlaceGrass();
            }
            
            if (generateOtherTrees) {
                PlaceOtherTrees();
            }
            
            int totalProps = spawnedRocks.Count + spawnedGrass.Count + spawnedOtherTrees.Count;
            Debug.Log($"Props: Generated {totalProps} props successfully! (Rocks: {spawnedRocks.Count}, Grass: {spawnedGrass.Count}, Other Trees: {spawnedOtherTrees.Count})");
        }
        
        void GenerateNoiseMaps() {
            int mapSize = 256;
            
            // Générer des noise maps séparées pour chaque type de prop
            rockPatchNoiseMap = new float[mapSize, mapSize];
            grassPatchNoiseMap = new float[mapSize, mapSize];
            otherTreePatchNoiseMap = new float[mapSize, mapSize];
            
            // Noise map pour les rochers
            for (int x = 0; x < mapSize; x++) {
                for (int y = 0; y < mapSize; y++) {
                    float rockNoise = GenerateFractalNoise(x, y, rockPatchNoiseScale, 1000f);
                    rockPatchNoiseMap[x, y] = rockNoise;
                }
            }
            
            // Noise map pour les herbes
            for (int x = 0; x < mapSize; x++) {
                for (int y = 0; y < mapSize; y++) {
                    float grassNoise = GenerateFractalNoise(x, y, grassPatchNoiseScale, 2000f);
                    grassPatchNoiseMap[x, y] = grassNoise;
                }
            }
            
            // Noise map pour les autres arbres
            for (int x = 0; x < mapSize; x++) {
                for (int y = 0; y < mapSize; y++) {
                    float otherTreeNoise = GenerateFractalNoise(x, y, otherTreePatchNoiseScale, 3000f);
                    otherTreePatchNoiseMap[x, y] = otherTreeNoise;
                }
            }
        }
        
        float GenerateFractalNoise(float x, float y, float noiseScale, float offset) {
            float noise = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float maxValue = 0f;
            
            for (int i = 0; i < noiseOctaves; i++) {
                float sampleX = x * noiseScale * frequency + offset;
                float sampleY = y * noiseScale * frequency + offset;
                
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noise += perlinValue * amplitude;
                maxValue += amplitude;
                
                amplitude *= noisePersistence;
                frequency *= noiseLacunarity;
            }
            
            noise = noise / maxValue;
            noise = Mathf.Pow(noise, noiseIntensity);
            
            return noise;
        }
        
        float GetVertexHeight(Vector3 position) {
            if (hexasphereFill == null) return 0f;
            Vector3 normalizedPosition = position.normalized;
            return CalculateHeightAtPosition(normalizedPosition);
        }
        
        float CalculateHeightAtPosition(Vector3 normalizedPosition) {
            if (hexasphereFill == null) return 0f;
            return hexasphereFill.GetVertexHeight(normalizedPosition * planetRadius);
        }
        
        void PlaceRocks() {
            int rocksPlaced = 0;
            int attempts = 0;
            int maxAttempts = Mathf.RoundToInt(rockCount * maxAttemptsMultiplier);
            int landAreaAttempts = 0;
            int patchAttempts = 0;
            int spacingAttempts = 0;
            
            Debug.Log($"Props: Starting rock placement. Target: {rockCount} rocks");
            
            while (rocksPlaced < rockCount && attempts < maxAttempts) {
                attempts++;
                
                Vector3 randomPosition = GenerateRandomSpherePosition();
                
                if (!IsLandArea(randomPosition)) {
                    landAreaAttempts++;
                    continue;
                }
                
                if (!IsInRockPatch(randomPosition)) {
                    patchAttempts++;
                    continue;
                }
                
                if (!IsValidRockPosition(randomPosition)) {
                    spacingAttempts++;
                    continue;
                }
                
                PlaceRockAtPosition(randomPosition);
                rocksPlaced++;
            }
            
            if (showPlacementStats) {
                Debug.Log($"Props: Rock placement complete. Rocks placed: {rocksPlaced}, Total attempts: {attempts}");
                Debug.Log($"Props: Failed due to land area: {landAreaAttempts}, patch: {patchAttempts}, spacing: {spacingAttempts}");
            }
        }
        
        void PlaceGrass() {
            int grassPlaced = 0;
            int attempts = 0;
            int maxAttempts = Mathf.RoundToInt(grassCount * maxAttemptsMultiplier);
            int landAreaAttempts = 0;
            int patchAttempts = 0;
            int spacingAttempts = 0;
            
            Debug.Log($"Props: Starting grass placement. Target: {grassCount} grass");
            
            while (grassPlaced < grassCount && attempts < maxAttempts) {
                attempts++;
                
                Vector3 randomPosition = GenerateRandomSpherePosition();
                
                if (!IsLandArea(randomPosition)) {
                    landAreaAttempts++;
                    continue;
                }
                
                if (!IsInGrassPatch(randomPosition)) {
                    patchAttempts++;
                    continue;
                }
                
                if (!IsValidGrassPosition(randomPosition)) {
                    spacingAttempts++;
                    continue;
                }
                
                PlaceGrassAtPosition(randomPosition);
                grassPlaced++;
            }
            
            if (showPlacementStats) {
                Debug.Log($"Props: Grass placement complete. Grass placed: {grassPlaced}, Total attempts: {attempts}");
                Debug.Log($"Props: Failed due to land area: {landAreaAttempts}, patch: {patchAttempts}, spacing: {spacingAttempts}");
            }
        }
        
        void PlaceOtherTrees() {
            int otherTreesPlaced = 0;
            int attempts = 0;
            int maxAttempts = Mathf.RoundToInt(otherTreeCount * maxAttemptsMultiplier);
            int landAreaAttempts = 0;
            int patchAttempts = 0;
            int spacingAttempts = 0;
            
            Debug.Log($"Props: Starting other tree placement. Target: {otherTreeCount} other trees");
            
            while (otherTreesPlaced < otherTreeCount && attempts < maxAttempts) {
                attempts++;
                
                Vector3 randomPosition = GenerateRandomSpherePosition();
                
                if (!IsLandArea(randomPosition)) {
                    landAreaAttempts++;
                    continue;
                }
                
                if (!IsInOtherTreePatch(randomPosition)) {
                    patchAttempts++;
                    continue;
                }
                
                if (!IsValidOtherTreePosition(randomPosition)) {
                    spacingAttempts++;
                    continue;
                }
                
                PlaceOtherTreeAtPosition(randomPosition);
                otherTreesPlaced++;
            }
            
            if (showPlacementStats) {
                Debug.Log($"Props: Other tree placement complete. Other trees placed: {otherTreesPlaced}, Total attempts: {attempts}");
                Debug.Log($"Props: Failed due to land area: {landAreaAttempts}, patch: {patchAttempts}, spacing: {spacingAttempts}");
            }
        }
        
        Vector3 GenerateRandomSpherePosition() {
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 basePosition = planetCenter + randomDirection * planetRadius;
            float height = GetVertexHeight(basePosition);
            return planetCenter + randomDirection * (planetRadius + height);
        }
        
        bool IsLandArea(Vector3 position) {
            Vector3 normalizedPosition = (position - planetCenter).normalized;
            float height = GetVertexHeight(normalizedPosition * planetRadius);
            
            float effectiveWaterLevel = hexasphereFill.GetEffectiveWaterLevel();
            float effectiveMountainLevel = hexasphereFill.GetEffectiveMountainLevel();
            
            bool isLand = height > effectiveWaterLevel && height <= effectiveMountainLevel;
            
            if (showDebugInfo && Random.value < 0.1f) {
                Debug.Log($"Props: Position={position}, Height={height}, WaterLevel={effectiveWaterLevel}, MountainLevel={effectiveMountainLevel}, IsLand={isLand}");
            }
            
            return isLand;
        }
        
        bool IsInRockPatch(Vector3 position) {
            if (forceDensePlacement) return true;
            
            Vector2 noiseCoords = WorldToNoiseMap(position);
            float patchValue = GetInterpolatedNoiseValue(noiseCoords, rockPatchNoiseMap);
            bool isInPatch = patchValue > rockPatchThreshold;
            
            if (showDebugInfo && Random.value < 0.05f) {
                Debug.Log($"Props: Rock patch check - Position={position}, NoiseValue={patchValue:F3}, Threshold={rockPatchThreshold:F3}, IsInPatch={isInPatch}");
            }
            
            return isInPatch;
        }
        
        bool IsInGrassPatch(Vector3 position) {
            if (forceDensePlacement) return true;
            
            Vector2 noiseCoords = WorldToNoiseMap(position);
            float patchValue = GetInterpolatedNoiseValue(noiseCoords, grassPatchNoiseMap);
            bool isInPatch = patchValue > grassPatchThreshold;
            
            if (showDebugInfo && Random.value < 0.05f) {
                Debug.Log($"Props: Grass patch check - Position={position}, NoiseValue={patchValue:F3}, Threshold={grassPatchThreshold:F3}, IsInPatch={isInPatch}");
            }
            
            return isInPatch;
        }
        
        bool IsInOtherTreePatch(Vector3 position) {
            if (forceDensePlacement) return true;
            
            Vector2 noiseCoords = WorldToNoiseMap(position);
            float patchValue = GetInterpolatedNoiseValue(noiseCoords, otherTreePatchNoiseMap);
            bool isInPatch = patchValue > otherTreePatchThreshold;
            
            if (showDebugInfo && Random.value < 0.05f) {
                Debug.Log($"Props: Other tree patch check - Position={position}, NoiseValue={patchValue:F3}, Threshold={otherTreePatchThreshold:F3}, IsInPatch={isInPatch}");
            }
            
            return isInPatch;
        }
        
        float GetInterpolatedNoiseValue(Vector2 noiseCoords, float[,] noiseMap) {
            float x = noiseCoords.x * (noiseMap.GetLength(0) - 1);
            float y = noiseCoords.y * (noiseMap.GetLength(1) - 1);
            
            int x1 = Mathf.FloorToInt(x);
            int y1 = Mathf.FloorToInt(y);
            int x2 = Mathf.Min(x1 + 1, noiseMap.GetLength(0) - 1);
            int y2 = Mathf.Min(y1 + 1, noiseMap.GetLength(1) - 1);
            
            float fx = x - x1;
            float fy = y - y1;
            
            float n1 = noiseMap[x1, y1];
            float n2 = noiseMap[x2, y1];
            float n3 = noiseMap[x1, y2];
            float n4 = noiseMap[x2, y2];
            
            float i1 = Mathf.Lerp(n1, n2, fx);
            float i2 = Mathf.Lerp(n3, n4, fx);
            
            return Mathf.Lerp(i1, i2, fy);
        }
        
        Vector2 WorldToNoiseMap(Vector3 worldPosition) {
            Vector3 direction = (worldPosition - planetCenter).normalized;
            float u = 0.5f + Mathf.Atan2(direction.z, direction.x) / (2f * Mathf.PI);
            float v = 0.5f - Mathf.Asin(direction.y) / Mathf.PI;
            return new Vector2(u, v);
        }
        
        bool IsValidRockPosition(Vector3 position) {
            foreach (Vector3 rockPos in rockPositions) {
                if (Vector3.Distance(position, rockPos) < rockSpacing) {
                    return false;
                }
            }
            return true;
        }
        
        bool IsValidGrassPosition(Vector3 position) {
            foreach (Vector3 grassPos in grassPositions) {
                if (Vector3.Distance(position, grassPos) < grassSpacing) {
                    return false;
                }
            }
            return true;
        }
        
        bool IsValidOtherTreePosition(Vector3 position) {
            foreach (Vector3 otherTreePos in otherTreePositions) {
                if (Vector3.Distance(position, otherTreePos) < otherTreeSpacing) {
                    return false;
                }
            }
            return true;
        }
        
        void PlaceRockAtPosition(Vector3 position) {
            GameObject rockPrefab = SelectRockType();
            if (rockPrefab == null) {
                Debug.LogWarning("Props: No rock prefab available!");
                return;
            }
            
            GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity, planetTransform);
            Vector3 directionToCenter = (planetCenter - position).normalized;
            Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
            
            // Appliquer une rotation aléatoire si activée
            if (useRandomRotation) {
                Quaternion randomRotation = GenerateRandomRotation();
                rock.transform.rotation = baseRotation * randomRotation;
            } else {
                rock.transform.rotation = baseRotation;
            }
            
            float scaleVariation = rockScale + Random.Range(-rockScaleVariation, rockScaleVariation);
            rock.transform.localScale = Vector3.one * scaleVariation;
            
            spawnedRocks.Add(rock);
            rockPositions.Add(position);
            rockRotations.Add(rock.transform.rotation);
        }
        
        void PlaceGrassAtPosition(Vector3 position) {
            GameObject grassPrefab = SelectGrassType();
            if (grassPrefab == null) {
                Debug.LogWarning("Props: No grass prefab available!");
                return;
            }
            
            GameObject grass = Instantiate(grassPrefab, position, Quaternion.identity, planetTransform);
            Vector3 directionToCenter = (planetCenter - position).normalized;
            Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
            
            // Appliquer une rotation aléatoire si activée
            if (useRandomRotation) {
                Quaternion randomRotation = GenerateRandomRotation();
                grass.transform.rotation = baseRotation * randomRotation;
            } else {
                grass.transform.rotation = baseRotation;
            }
            
            float scaleVariation = grassScale + Random.Range(-grassScaleVariation, grassScaleVariation);
            grass.transform.localScale = Vector3.one * scaleVariation;
            
            spawnedGrass.Add(grass);
            grassPositions.Add(position);
            grassRotations.Add(grass.transform.rotation);
        }
        
        void PlaceOtherTreeAtPosition(Vector3 position) {
            GameObject otherTreePrefab = SelectOtherTreeType();
            if (otherTreePrefab == null) {
                Debug.LogWarning("Props: No other tree prefab available!");
                return;
            }
            
            GameObject otherTree = Instantiate(otherTreePrefab, position, Quaternion.identity, planetTransform);
            Vector3 directionToCenter = (planetCenter - position).normalized;
            Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
            
            // Appliquer une rotation aléatoire si activée
            if (useRandomRotation) {
                Quaternion randomRotation = GenerateRandomRotation();
                otherTree.transform.rotation = baseRotation * randomRotation;
            } else {
                otherTree.transform.rotation = baseRotation;
            }
            
            float scaleVariation = otherTreeScale + Random.Range(-otherTreeScaleVariation, otherTreeScaleVariation);
            otherTree.transform.localScale = Vector3.one * scaleVariation;
            
            spawnedOtherTrees.Add(otherTree);
            otherTreePositions.Add(position);
            otherTreeRotations.Add(otherTree.transform.rotation);
        }
        
        GameObject SelectRockType() {
            if (rockPrefabs == null || rockPrefabs.Length == 0) return null;
            
            float randomValue = Random.value;
            float cumulativeWeight = 0f;
            
            for (int i = 0; i < rockPrefabs.Length; i++) {
                cumulativeWeight += rockTypeWeights[i];
                if (randomValue <= cumulativeWeight) {
                    return rockPrefabs[i];
                }
            }
            
            return rockPrefabs[0];
        }
        
        GameObject SelectGrassType() {
            if (grassPrefabs == null || grassPrefabs.Length == 0) return null;
            
            float randomValue = Random.value;
            float cumulativeWeight = 0f;
            
            for (int i = 0; i < grassPrefabs.Length; i++) {
                cumulativeWeight += grassTypeWeights[i];
                if (randomValue <= cumulativeWeight) {
                    return grassPrefabs[i];
                }
            }
            
            return grassPrefabs[0];
        }
        
        Quaternion GenerateRandomRotation() {
            // Rotation Y complète (360°)
            float rotationY = Random.Range(0f, 360f);
            
            // Rotations X et Z avec distribution centrée sur 0
            float rotationX = GenerateCenteredRandomValue(maxRotationX, rotationDistribution);
            float rotationZ = GenerateCenteredRandomValue(maxRotationZ, rotationDistribution);
            
            return Quaternion.Euler(rotationX, rotationY, rotationZ);
        }
        
        float GenerateCenteredRandomValue(float maxValue, float distribution) {
            // Utiliser une distribution normale pour centrer les valeurs sur 0
            // Plus le facteur de distribution est élevé, plus les valeurs sont centrées sur 0
            float randomValue = Random.Range(-maxValue, maxValue);
            
            // Appliquer une fonction de distribution pour centrer sur 0
            float sign = randomValue >= 0 ? 1f : -1f;
            float normalizedValue = Mathf.Abs(randomValue) / maxValue;
            float distributedValue = Mathf.Pow(normalizedValue, distribution);
            
            return sign * distributedValue * maxValue;
        }
        
        GameObject SelectOtherTreeType() {
            if (otherTreePrefabs == null || otherTreePrefabs.Length == 0) return null;
            
            float randomValue = Random.value;
            float cumulativeWeight = 0f;
            
            for (int i = 0; i < otherTreePrefabs.Length; i++) {
                cumulativeWeight += otherTreeTypeWeights[i];
                if (randomValue <= cumulativeWeight) {
                    return otherTreePrefabs[i];
                }
            }
            
            return otherTreePrefabs[0];
        }
        
        public void ClearAllProps() {
            // Détruire tous les rochers
            foreach (GameObject rock in spawnedRocks) {
                if (rock != null) DestroyImmediate(rock);
            }
            spawnedRocks.Clear();
            rockPositions.Clear();
            rockRotations.Clear();
            
            // Détruire toutes les herbes
            foreach (GameObject grass in spawnedGrass) {
                if (grass != null) DestroyImmediate(grass);
            }
            spawnedGrass.Clear();
            grassPositions.Clear();
            grassRotations.Clear();
            
            // Détruire tous les autres arbres
            foreach (GameObject otherTree in spawnedOtherTrees) {
                if (otherTree != null) DestroyImmediate(otherTree);
            }
            spawnedOtherTrees.Clear();
            otherTreePositions.Clear();
            otherTreeRotations.Clear();
        }
        
        [ContextMenu("🗿 Regénérer les Rochers")]
        public void RegenerateRocks() {
            Debug.Log("Props: Regenerating rocks...");
            ClearRocks();
            if (generateRocks) PlaceRocks();
        }
        
        [ContextMenu("🌿 Regénérer les Herbes")]
        public void RegenerateGrass() {
            Debug.Log("Props: Regenerating grass...");
            ClearGrass();
            if (generateGrass) PlaceGrass();
        }
        
        [ContextMenu("🌳 Regénérer les Autres Arbres")]
        public void RegenerateOtherTrees() {
            Debug.Log("Props: Regenerating other trees...");
            ClearOtherTrees();
            if (generateOtherTrees) PlaceOtherTrees();
        }
        
        [ContextMenu("🔄 Regénérer Tous les Props")]
        public void RegenerateAllProps() {
            Debug.Log("Props: Regenerating all props...");
            GenerateProps();
        }
        
        [ContextMenu("🗑️ Effacer Tous les Props")]
        public void ClearAllPropsButton() {
            Debug.Log("Props: Clearing all props...");
            ClearAllProps();
        }
        
        public void ClearRocks() {
            foreach (GameObject rock in spawnedRocks) {
                if (rock != null) DestroyImmediate(rock);
            }
            spawnedRocks.Clear();
            rockPositions.Clear();
            rockRotations.Clear();
        }
        
        public void ClearGrass() {
            foreach (GameObject grass in spawnedGrass) {
                if (grass != null) DestroyImmediate(grass);
            }
            spawnedGrass.Clear();
            grassPositions.Clear();
            grassRotations.Clear();
        }
        
        public void ClearOtherTrees() {
            foreach (GameObject otherTree in spawnedOtherTrees) {
                if (otherTree != null) DestroyImmediate(otherTree);
            }
            spawnedOtherTrees.Clear();
            otherTreePositions.Clear();
            otherTreeRotations.Clear();
        }
        
        void OnDrawGizmos() {
            if (!showDebugInfo) return;
            
            if (showPropPositions) {
                // Dessiner les rochers
                Gizmos.color = rockDebugColor;
                foreach (Vector3 rockPos in rockPositions) {
                    Gizmos.DrawWireSphere(rockPos, 0.1f);
                }
                
                // Dessiner les herbes
                Gizmos.color = grassDebugColor;
                foreach (Vector3 grassPos in grassPositions) {
                    Gizmos.DrawWireSphere(grassPos, 0.05f);
                }
                
                // Dessiner les autres arbres
                Gizmos.color = otherTreeDebugColor;
                foreach (Vector3 otherTreePos in otherTreePositions) {
                    Gizmos.DrawWireSphere(otherTreePos, 0.15f);
                }
            }
            
            // Dessiner le centre de la planète
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(planetCenter, 0.2f);
        }
        
        void OnValidate() {
            // Valider les paramètres des rochers
            rockCount = Mathf.Max(0, rockCount);
            rockSpacing = Mathf.Max(0.0001f, rockSpacing);
            rockScaleVariation = Mathf.Clamp(rockScaleVariation, 0f, 1f);
            
            // Valider les paramètres des herbes
            grassCount = Mathf.Max(0, grassCount);
            grassSpacing = Mathf.Max(0.0001f, grassSpacing);
            grassScaleVariation = Mathf.Clamp(grassScaleVariation, 0f, 1f);
            
            // Valider les paramètres des autres arbres
            otherTreeCount = Mathf.Max(0, otherTreeCount);
            otherTreeSpacing = Mathf.Max(0.0001f, otherTreeSpacing);
            otherTreeScaleVariation = Mathf.Clamp(otherTreeScaleVariation, 0f, 1f);
            
            // Valider les seuils
            landThreshold = Mathf.Clamp(landThreshold, 0f, 1f);
            rockPatchThreshold = Mathf.Clamp(rockPatchThreshold, 0f, 1f);
            grassPatchThreshold = Mathf.Clamp(grassPatchThreshold, 0f, 1f);
            otherTreePatchThreshold = Mathf.Clamp(otherTreePatchThreshold, 0f, 1f);
            
            // Valider les paramètres de bruit
            noiseIntensity = Mathf.Max(0.1f, noiseIntensity);
            noiseOctaves = Mathf.Max(1, Mathf.Min(noiseOctaves, 8));
            noiseLacunarity = Mathf.Max(1.0f, noiseLacunarity);
            noisePersistence = Mathf.Clamp(noisePersistence, 0.1f, 1.0f);
        }
    }
}
