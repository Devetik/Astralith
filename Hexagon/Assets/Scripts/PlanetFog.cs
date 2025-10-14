using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlanetFog : MonoBehaviour
{
    [Header("🌫️ Configuration du Brouillard")]
    [SerializeField] public bool enableFog = true;
    [SerializeField] public float fogDensity = 0.02f;
    [SerializeField] public Color fogColor = new Color(0.8f, 0.9f, 1f, 0.3f);
    [SerializeField] public float fogStartDistance = 0f;
    [SerializeField] public float fogEndDistance = 100f;
    
    [Header("☁️ Configuration des Nuages")]
    [SerializeField] public bool enableClouds = true;
    [SerializeField] public int cloudCount = 50;
    [SerializeField] public float cloudRadius = 1.2f; // Rayon autour de la planète
    [SerializeField] public float cloudSizeMin = 0.1f;
    [SerializeField] public float cloudSizeMax = 0.3f;
    [SerializeField] public Color cloudColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] public float cloudSpeed = 0.1f;
    [SerializeField] public bool useProceduralClouds = true;
    [SerializeField] public bool fixCloudPosition = true; // Corriger la position des nuages
    
    [Header("🌍 Paramètres de Planète")]
    [SerializeField] public Transform planetTransform;
    [SerializeField] public float planetRadius = 1f;
    [SerializeField] public bool autoDetectPlanet = true;
    
    [Header("🎨 Matériaux")]
    [SerializeField] public Material cloudMaterial;
    [SerializeField] public Material fogMaterial;
    
    [Header("⚙️ Performance")]
    [SerializeField] public bool useLOD = true;
    [SerializeField] public float lodDistance = 50f;
    [SerializeField] public int maxCloudsPerFrame = 5;
    [SerializeField] public bool forceCloudsActive = true; // Forcer l'activation des nuages
    
    // Variables privées
    private List<GameObject> cloudObjects = new List<GameObject>();
    private List<CloudData> cloudDataList = new List<CloudData>();
    private Camera playerCamera;
    private bool isInitialized = false;
    
    // Structure pour les données des nuages
    [System.Serializable]
    public class CloudData
    {
        public Vector3 position;
        public Vector3 rotation;
        public float size;
        public float speed;
        public float opacity;
        public GameObject gameObject;
    }
    
    void Start()
    {
        InitializeFogSystem();
    }
    
    void InitializeFogSystem()
    {
        // Détecter automatiquement la planète si activé
        if (autoDetectPlanet && planetTransform == null)
        {
            GameObject planet = GameObject.FindGameObjectWithTag("Planet");
            if (planet != null)
            {
                planetTransform = planet.transform;
                planetRadius = planet.GetComponent<HexasphereFill>()?.radius ?? 1f;
            }
        }
        
        // Trouver la caméra du joueur
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Configurer le brouillard Unity
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogColor = fogColor;
        }
        
        // Générer les nuages
        if (enableClouds)
        {
            GenerateClouds();
        }
        
        isInitialized = true;
    }
    
    void GenerateClouds()
    {
        Debug.Log("☁️ Génération des nuages...");
        
        // Nettoyer les nuages existants
        ClearClouds();
        
        if (useProceduralClouds)
        {
            GenerateProceduralClouds();
        }
        else
        {
            GenerateSimpleClouds();
        }
        
        // Forcer l'activation immédiate
        ForceActivateAllClouds();
        
        Debug.Log($"☁️ {cloudObjects.Count} nuages générés et activés");
    }
    
    void GenerateProceduralClouds()
    {
        for (int i = 0; i < cloudCount; i++)
        {
            // Position aléatoire sur une sphère autour de la planète
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 cloudPosition;
            
            if (fixCloudPosition && planetTransform != null)
            {
                // Position relative à la planète
                cloudPosition = planetTransform.position + randomDirection * (planetRadius + cloudRadius);
            }
            else
            {
                // Ancienne méthode
                cloudPosition = randomDirection * (planetRadius + cloudRadius);
                if (planetTransform != null)
                {
                    cloudPosition += planetTransform.position;
                }
            }
            
            Debug.Log($"☁️ Position planète: {(planetTransform != null ? planetTransform.position.ToString() : "null")}");
            Debug.Log($"☁️ Rayon planète: {planetRadius}, Rayon nuage: {cloudRadius}");
            Debug.Log($"☁️ Position nuage calculée: {cloudPosition}");
            
            // Créer le nuage
            GameObject cloud = CreateCloudObject(cloudPosition, i);
            
            // Données du nuage
            CloudData cloudData = new CloudData
            {
                position = cloudPosition,
                rotation = new Vector3(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                ),
                size = Random.Range(cloudSizeMin, cloudSizeMax),
                speed = Random.Range(cloudSpeed * 0.5f, cloudSpeed * 1.5f),
                opacity = Random.Range(0.3f, 0.8f),
                gameObject = cloud
            };
            
            cloudDataList.Add(cloudData);
            cloudObjects.Add(cloud);
        }
    }
    
    void GenerateSimpleClouds()
    {
        // Génération de nuages plus simples pour les performances
        for (int i = 0; i < cloudCount / 2; i++)
        {
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 cloudPosition = randomDirection * (planetRadius + cloudRadius);
            
            if (planetTransform != null)
            {
                cloudPosition += planetTransform.position;
            }
            
            GameObject cloud = CreateSimpleCloudObject(cloudPosition, i);
            cloudObjects.Add(cloud);
        }
    }
    
    GameObject CreateCloudObject(Vector3 position, int index)
    {
        GameObject cloud = new GameObject($"Cloud_{index}");
        cloud.transform.position = position;
        cloud.transform.parent = transform;
        
        // Créer un mesh de nuage (sphère déformée)
        Mesh cloudMesh = CreateCloudMesh();
        MeshFilter meshFilter = cloud.AddComponent<MeshFilter>();
        meshFilter.mesh = cloudMesh;
        
        // Ajouter le renderer
        MeshRenderer renderer = cloud.AddComponent<MeshRenderer>();
        
        // Créer le matériau si nécessaire
        if (cloudMaterial == null)
        {
            cloudMaterial = CreateCloudMaterial();
        }
        renderer.material = cloudMaterial;
        
        // S'assurer que le nuage est actif
        cloud.SetActive(true);
        
        // Ajouter un script de mouvement
        CloudMovement cloudMovement = cloud.AddComponent<CloudMovement>();
        cloudMovement.Initialize(this, index);
        
        Debug.Log($"☁️ Nuage {index} créé à la position {position}");
        
        return cloud;
    }
    
    GameObject CreateSimpleCloudObject(Vector3 position, int index)
    {
        GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cloud.name = $"SimpleCloud_{index}";
        cloud.transform.position = position;
        cloud.transform.parent = transform;
        
        // Ajuster la taille
        float size = Random.Range(cloudSizeMin, cloudSizeMax);
        cloud.transform.localScale = Vector3.one * size;
        
        // Configurer le matériau
        MeshRenderer renderer = cloud.GetComponent<MeshRenderer>();
        if (cloudMaterial == null)
        {
            cloudMaterial = CreateCloudMaterial();
        }
        renderer.material = cloudMaterial;
        
        // Supprimer le collider
        Collider collider = cloud.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
        
        // S'assurer que le nuage est actif
        cloud.SetActive(true);
        
        Debug.Log($"☁️ Nuage simple {index} créé à la position {position} avec taille {size}");
        
        return cloud;
    }
    
    Mesh CreateCloudMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "CloudMesh";
        
        // Créer une sphère avec des déformations pour ressembler à un nuage
        int segments = 16;
        int rings = 8;
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Générer les vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            float v = (float)ring / rings;
            float latitude = Mathf.PI * v;
            
            for (int segment = 0; segment <= segments; segment++)
            {
                float u = (float)segment / segments;
                float longitude = 2f * Mathf.PI * u;
                
                // Position de base
                Vector3 pos = new Vector3(
                    Mathf.Sin(latitude) * Mathf.Cos(longitude),
                    Mathf.Cos(latitude),
                    Mathf.Sin(latitude) * Mathf.Sin(longitude)
                );
                
                // Ajouter de la déformation pour ressembler à un nuage
                float noise = Mathf.PerlinNoise(pos.x * 2f, pos.z * 2f);
                pos += pos * noise * 0.3f;
                
                vertices.Add(pos);
                uvs.Add(new Vector2(u, v));
            }
        }
        
        // Générer les triangles
        for (int ring = 0; ring < rings; ring++)
        {
            for (int segment = 0; segment < segments; segment++)
            {
                int current = ring * (segments + 1) + segment;
                int next = current + segments + 1;
                
                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);
                
                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    Material CreateCloudMaterial()
    {
        // Essayer d'abord un shader plus simple
        Material material = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        if (material.shader.name == "Hidden/InternalErrorShader")
        {
            // Fallback vers Standard si Legacy n'est pas disponible
            material = new Material(Shader.Find("Standard"));
            material.name = "CloudMaterial";
            
            // Configurer les propriétés du matériau
            material.SetFloat("_Mode", 3); // Mode Transparent
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
        else
        {
            material.name = "CloudMaterial";
        }
        
        // Couleur et transparence - rendre plus visible
        Color finalColor = new Color(cloudColor.r, cloudColor.g, cloudColor.b, 0.8f);
        material.color = finalColor;
        material.SetFloat("_Metallic", 0f);
        material.SetFloat("_Smoothness", 0.1f);
        
        // Forcer la couleur blanche si le matériau est rose
        if (material.color.r > 0.8f && material.color.g < 0.5f && material.color.b > 0.5f)
        {
            material.color = new Color(1f, 1f, 1f, 0.8f); // Blanc opaque
            Debug.Log("☁️ Couleur corrigée vers blanc");
        }
        
        Debug.Log($"☁️ Matériau créé avec shader: {material.shader.name}");
        
        return material;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Mettre à jour le brouillard
        if (enableFog)
        {
            UpdateFogSettings();
        }
        
        // Mettre à jour les nuages
        if (enableClouds)
        {
            UpdateClouds();
        }
    }
    
    void UpdateFogSettings()
    {
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;
    }
    
    void UpdateClouds()
    {
        // Forcer l'activation des nuages si demandé
        if (forceCloudsActive)
        {
            foreach (GameObject cloud in cloudObjects)
            {
                if (cloud != null && !cloud.activeSelf)
                {
                    cloud.SetActive(true);
                }
            }
        }
        
        if (useLOD && playerCamera != null)
        {
            UpdateCloudLOD();
        }
        
        // Mettre à jour les nuages procéduraux
        for (int i = 0; i < cloudDataList.Count && i < maxCloudsPerFrame; i++)
        {
            UpdateCloudData(cloudDataList[i]);
        }
    }
    
    void UpdateCloudLOD()
    {
        if (playerCamera == null) 
        {
            // Si pas de caméra, activer tous les nuages
            foreach (GameObject cloud in cloudObjects)
            {
                if (cloud != null && !cloud.activeSelf)
                {
                    cloud.SetActive(true);
                }
            }
            return;
        }
        
        Vector3 cameraPosition = playerCamera.transform.position;
        
        foreach (GameObject cloud in cloudObjects)
        {
            if (cloud == null) continue;
            
            float distance = Vector3.Distance(cameraPosition, cloud.transform.position);
            bool shouldBeActive = distance < lodDistance;
            
            if (cloud.activeSelf != shouldBeActive)
            {
                cloud.SetActive(shouldBeActive);
            }
        }
    }
    
    void UpdateCloudData(CloudData cloudData)
    {
        if (cloudData.gameObject == null) return;
        
        // Rotation lente
        cloudData.rotation += Vector3.up * cloudData.speed * Time.deltaTime;
        cloudData.gameObject.transform.rotation = Quaternion.Euler(cloudData.rotation);
        
        // Mouvement orbital léger
        if (planetTransform != null)
        {
            Vector3 direction = (cloudData.gameObject.transform.position - planetTransform.position).normalized;
            cloudData.gameObject.transform.position += direction * cloudData.speed * 0.1f * Time.deltaTime;
        }
    }
    
    void ClearClouds()
    {
        foreach (GameObject cloud in cloudObjects)
        {
            if (cloud != null)
            {
                DestroyImmediate(cloud);
            }
        }
        cloudObjects.Clear();
        cloudDataList.Clear();
    }
    
    // Méthodes publiques pour le contrôle
    public void SetFogEnabled(bool enabled)
    {
        enableFog = enabled;
        RenderSettings.fog = enabled;
    }
    
    public void SetCloudsEnabled(bool enabled)
    {
        enableClouds = enabled;
        foreach (GameObject cloud in cloudObjects)
        {
            if (cloud != null)
            {
                cloud.SetActive(enabled);
            }
        }
    }
    
    public void RegenerateClouds()
    {
        if (enableClouds)
        {
            GenerateClouds();
            // Forcer l'activation après génération
            if (forceCloudsActive)
            {
                ForceActivateAllClouds();
            }
        }
    }
    
    public void SetFogDensity(float density)
    {
        fogDensity = Mathf.Clamp01(density);
        if (enableFog)
        {
            RenderSettings.fogDensity = fogDensity;
        }
    }
    
    public void SetCloudCount(int count)
    {
        cloudCount = Mathf.Max(0, count);
        if (enableClouds)
        {
            RegenerateClouds();
        }
    }
    
    // Fonction de debug pour vérifier l'état des nuages
    public void DebugCloudStatus()
    {
        Debug.Log($"☁️ Debug Nuages:");
        Debug.Log($"   - Nuages créés: {cloudObjects.Count}");
        Debug.Log($"   - Nuages actifs: {cloudObjects.Count(c => c != null && c.activeSelf)}");
        Debug.Log($"   - Nuages inactifs: {cloudObjects.Count(c => c != null && !c.activeSelf)}");
        Debug.Log($"   - LOD activé: {useLOD}");
        Debug.Log($"   - Distance LOD: {lodDistance}");
        Debug.Log($"   - Caméra: {(playerCamera != null ? "Trouvée" : "Non trouvée")}");
    }
    
    // Forcer l'activation de tous les nuages
    public void ForceActivateAllClouds()
    {
        foreach (GameObject cloud in cloudObjects)
        {
            if (cloud != null)
            {
                cloud.SetActive(true);
            }
        }
        Debug.Log("☁️ Tous les nuages ont été forcés à s'activer");
    }
    
    // Créer un nuage de test visible
    public void CreateTestCloud()
    {
        Debug.Log("☁️ Création d'un nuage de test...");
        
        // Position de test devant la caméra
        Vector3 testPosition = Vector3.forward * 5f;
        if (playerCamera != null)
        {
            testPosition = playerCamera.transform.position + playerCamera.transform.forward * 5f;
        }
        
        GameObject testCloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        testCloud.name = "TestCloud";
        testCloud.transform.position = testPosition;
        testCloud.transform.localScale = Vector3.one * 0.5f;
        testCloud.transform.parent = transform;
        
        // Matériau de test très visible
        MeshRenderer renderer = testCloud.GetComponent<MeshRenderer>();
        Material testMaterial = new Material(Shader.Find("Standard"));
        testMaterial.color = Color.red; // Rouge pour être très visible
        renderer.material = testMaterial;
        
        // Supprimer le collider
        Collider collider = testCloud.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
        
        testCloud.SetActive(true);
        
        Debug.Log($"☁️ Nuage de test créé à la position {testPosition}");
        
        // Détruire après 5 secondes
        Destroy(testCloud, 5f);
    }
    
    // Corriger la position de tous les nuages
    public void FixCloudPositions()
    {
        Debug.Log("☁️ Correction des positions des nuages...");
        
        if (planetTransform == null)
        {
            Debug.LogWarning("☁️ Pas de planète définie pour corriger les positions");
            return;
        }
        
        for (int i = 0; i < cloudObjects.Count; i++)
        {
            if (cloudObjects[i] != null)
            {
                // Recalculer la position relative à la planète
                Vector3 randomDirection = Random.onUnitSphere;
                Vector3 newPosition = planetTransform.position + randomDirection * (planetRadius + cloudRadius);
                cloudObjects[i].transform.position = newPosition;
                
                Debug.Log($"☁️ Nuage {i} repositionné à {newPosition}");
            }
        }
    }
    
    // Corriger la couleur de tous les nuages
    public void FixCloudColors()
    {
        Debug.Log("☁️ Correction des couleurs des nuages...");
        
        foreach (GameObject cloud in cloudObjects)
        {
            if (cloud != null)
            {
                MeshRenderer renderer = cloud.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.material != null)
                {
                    renderer.material.color = new Color(1f, 1f, 1f, 0.8f); // Blanc opaque
                }
            }
        }
    }
    
    void OnDestroy()
    {
        ClearClouds();
    }
}

// Script pour le mouvement des nuages
public class CloudMovement : MonoBehaviour
{
    private PlanetFog planetFog;
    private int cloudIndex;
    private Vector3 basePosition;
    private float timeOffset;
    
    public void Initialize(PlanetFog fog, int index)
    {
        planetFog = fog;
        cloudIndex = index;
        basePosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    
    void Update()
    {
        if (planetFog == null) return;
        
        // Mouvement de flottement
        float time = Time.time + timeOffset;
        Vector3 offset = new Vector3(
            Mathf.Sin(time * 0.5f) * 0.1f,
            Mathf.Cos(time * 0.3f) * 0.05f,
            Mathf.Sin(time * 0.7f) * 0.08f
        );
        
        transform.position = basePosition + offset;
        
        // Rotation lente
        transform.Rotate(Vector3.up, planetFog.cloudSpeed * Time.deltaTime);
    }
}
