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
        
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        
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
        }
        
        public void GeneratePlanet() {
            Debug.Log("🌍 Génération de la planète HexaAstralith...");
            
            CreateMesh();
            
            Debug.Log("✅ Planète HexaAstralith générée !");
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
            
            if (showDebugInfo) {
                Debug.Log($"🏷️ Tag '{objectTag}' appliqué à {gameObject.name}");
            }
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
            
            // Subdiviser
            for (int division = 0; division < divisions; division++) {
                SubdivideSphere(baseVertices, baseTriangles);
            }
            
            // Appliquer les hauteurs et créer le mesh final
            for (int i = 0; i < baseVertices.Count; i++) {
                Vector3 vertex = baseVertices[i];
                float height = GenerateHeight(vertex);
                
                // Gestion des océans plats
                if (useFlatOceans && height <= waterLevel) {
                    height = 0f; // Océans parfaitement plats
                } else if (height > waterLevel) {
                    // Pour les terres, ajuster la hauteur
                    if (forceOceanLevel) {
                        height = height - waterLevel; // Ajuster pour que les terres partent du niveau de la mer
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
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
