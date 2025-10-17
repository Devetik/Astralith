using UnityEngine;
using System.Collections;

public class ActionBar : MonoBehaviour
{
    [Header("🏗️ Configuration Générale")]
    [SerializeField] private LayerMask planetLayerMask = -1; // Layer de la planète
    [SerializeField] private float previewOpacity = 0.5f; // Opacité de prévisualisation
    [SerializeField] private Color validPlacementColor = Color.white; // Couleur pour placement valide
    [SerializeField] private Color invalidPlacementColor = Color.red; // Couleur pour placement invalide
    
    [Header("🌍 Validation de Zone")]
    [SerializeField] private float oceanThreshold = 0.1f; // Seuil pour détecter les océans
    
    [Header("🔄 Rotation")]
    [SerializeField] private float rotationSensitivity = 2f; // Sensibilité de rotation
    [SerializeField] private bool enableRotation = true; // Activer la rotation
    
    [Header("🏠 Exemple - Maison Simple")]
    [SerializeField] private GameObject housePrefab; // Prefab de la maison à placer
    [SerializeField] private float houseScale = 0.25f; // Échelle de la maison
    [SerializeField] private GameObject smithyPrefab; 
    [SerializeField] private float smithyScale = 0.25f; 
    [SerializeField] private GameObject lumberPrefab; 
    [SerializeField] private float lumberScale = 0.25f; 
    
    // Variables privées
    private Camera playerCamera;
    private GameObject previewObject;
    private bool isPlacingObject = false;
    private bool isRotatingObject = false;
    private Vector3 fixedPlacementPosition;
    private float baseRotationY = 0f;
    private float currentRotationY = 0f;
    private HexasphereFill hexasphereFill;
    private Transform planetTransform;
    
    // Variables pour la construction actuelle
    private GameObject currentPrefab;
    private float currentScale = 1f;
    private string currentObjectName = "Object";
    
    void Start()
    {
        // Trouver la caméra du joueur
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Trouver la planète
        hexasphereFill = FindObjectOfType<HexasphereFill>();
        if (hexasphereFill != null)
        {
            planetTransform = hexasphereFill.transform;
        }
    }
    
    void Update()
    {
        if (isPlacingObject)
        {
            if (!isRotatingObject)
            {
                UpdateObjectPreview();
            }
            HandleObjectPlacement();
            HandleObjectRotation();
        }
    }
    
    // ===== TEMPLATE POUR CRÉER VOS FONCTIONS =====
    // Copiez et modifiez ces exemples pour créer vos propres fonctions de placement
    
    // Exemple 1: Maison simple
    public void PlaceHouse()
    {
        StartConstruction(housePrefab, houseScale, "Simple House");
    }
    
    // Exemple 2: Tour (vous devez créer les variables dans l'inspecteur)
    public void PlaceSmithy()
    {
        StartConstruction(smithyPrefab, smithyScale, "Simple Smithy");
    }
    
    // Exemple 3: Usine
    public void PlaceLumberHouse()
    {
        StartConstruction(lumberPrefab, lumberScale, "LumberHouse");
    }
    
    // Exemple 4: Pont
    public void PlaceBridge()
    {
        // Remplacez par votre prefab et échelle
        // StartConstruction(bridgePrefab, bridgeScale, "Pont");
        Debug.Log("PlaceBridge: Configurez bridgePrefab et bridgeScale dans l'inspecteur");
    }
    
    // ===== TEMPLATE SIMPLE =====
    // Pour créer une nouvelle fonction, copiez ce template :
    /*
    public void PlaceMonObjet()
    {
        StartConstruction(monPrefab, monScale, "Mon Objet");
    }
    */
    
    // Méthode générique pour tester
    public void OnButtonClick()
    {
        Debug.Log("TEST");
    }
    
    // Méthode générique pour démarrer une construction
    public void StartConstruction(GameObject prefab, float scale, string objectName)
    {
        if (prefab == null)
        {
            Debug.LogError($"ActionBar: Aucun prefab assigné pour {objectName} !");
            return;
        }
        
        if (isPlacingObject)
        {
            // Annuler le placement actuel
            CancelObjectPlacement();
        }
        else
        {
            // Configurer la construction actuelle
            currentPrefab = prefab;
            currentScale = scale;
            currentObjectName = objectName;
            
            // Commencer le placement
            StartObjectPlacement();
        }
    }
    
    void StartObjectPlacement()
    {
        isPlacingObject = true;
        Debug.Log($"ActionBar: Mode placement de {currentObjectName} activé. Cliquez pour placer ou ESC pour annuler.");
        
        // Créer la prévisualisation
        CreateObjectPreview();
    }
    
    void CancelObjectPlacement()
    {
        isPlacingObject = false;
        isRotatingObject = false;
        
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }
        
        Debug.Log($"ActionBar: Placement de {currentObjectName} annulé.");
    }
    
    void CreateObjectPreview()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
        }
        
        previewObject = Instantiate(currentPrefab);
        previewObject.name = $"{currentObjectName} Preview";
        
        // Appliquer l'échelle
        previewObject.transform.localScale = Vector3.one * currentScale;
        
        // Configurer la prévisualisation
        SetupPreviewObject();
    }
    
    void SetupPreviewObject()
    {
        if (previewObject == null) return;
        
        // Désactiver les colliders de la prévisualisation
        Collider[] colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
        
        // Créer des matériaux temporaires compatibles
        CreateCompatibleMaterials();
        
        // Configurer l'opacité
        SetPreviewOpacity(previewOpacity);
    }
    
    void SetPreviewOpacity(float opacity)
    {
        if (previewObject == null) return;
        
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                // Utiliser directement la propriété _Color du shader Unlit/Color
                Color color = materials[i].color;
                color.a = opacity;
                materials[i].color = color;
                
                // Configurer le mode de rendu transparent
                materials[i].SetFloat("_Mode", 3); // Mode Transparent
                materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                materials[i].SetInt("_ZWrite", 0);
                materials[i].DisableKeyword("_ALPHATEST_ON");
                materials[i].EnableKeyword("_ALPHABLEND_ON");
                materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                materials[i].renderQueue = 3000;
            }
        }
    }
    
    void SetMaterialOpacity(Material material, float opacity)
    {
        if (material == null) return;
        
        // Essayer différentes propriétés de couleur selon le shader
        if (material.HasProperty("_Color"))
        {
            Color color = material.GetColor("_Color");
            color.a = opacity;
            material.SetColor("_Color", color);
        }
        else if (material.HasProperty("_TintColor"))
        {
            Color color = material.GetColor("_TintColor");
            color.a = opacity;
            material.SetColor("_TintColor", color);
        }
        else if (material.HasProperty("_MainColor"))
        {
            Color color = material.GetColor("_MainColor");
            color.a = opacity;
            material.SetColor("_MainColor", color);
        }
        else if (material.HasProperty("_BaseColor"))
        {
            Color color = material.GetColor("_BaseColor");
            color.a = opacity;
            material.SetColor("_BaseColor", color);
        }
        else
        {
            // Pour les shaders sans propriété de couleur, utiliser le mode de rendu transparent
            material.SetFloat("_Mode", 3); // Mode Transparent
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            
            // Essayer de définir l'opacité via l'alpha
            if (material.HasProperty("_Alpha"))
            {
                material.SetFloat("_Alpha", opacity);
            }
        }
    }
    
    void UpdateObjectPreview()
    {
        if (previewObject == null) return;
        
        // Raycast depuis la caméra vers la planète
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, planetLayerMask))
        {
            // Vérifier si on a touché la planète ou un chunk
            if (IsPlanetOrChunk(hit.transform))
            {
                Vector3 placementPosition = hit.point;
                bool isValidPlacement = IsValidPlacement(placementPosition);
                
                // Positionner l'objet
                previewObject.transform.position = placementPosition;
                
                // Orienter l'objet vers l'extérieur de la planète
                Vector3 directionToCenter = (planetTransform.position - placementPosition).normalized;
                previewObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
                
                // Changer la couleur selon la validité
                SetPreviewColor(isValidPlacement ? validPlacementColor : invalidPlacementColor);
            }
        }
    }
    
    void HandleObjectPlacement()
    {
        if (Input.GetMouseButtonDown(0)) // Clic gauche
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, planetLayerMask))
            {
                if (IsPlanetOrChunk(hit.transform))
                {
                    Vector3 placementPosition = hit.point;
                    
                    if (IsValidPlacement(placementPosition))
                    {
                        // Commencer la rotation si activée
                        if (enableRotation)
                        {
                            StartObjectRotation();
                        }
                        else
                        {
                            PlaceObject(placementPosition);
                            CancelObjectPlacement();
                        }
                    }
                    else
                    {
                        Debug.Log("ActionBar: Zone non constructible ! (Océan détecté)");
                    }
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape)) // ESC pour annuler
        {
            CancelObjectPlacement();
        }
    }
    
    void HandleObjectRotation()
    {
        if (!enableRotation) return;
        
        if (Input.GetMouseButton(0) && isRotatingObject) // Maintien du clic
        {
            // Calculer le mouvement horizontal de la souris
            float mouseX = Input.GetAxis("Mouse X");
            
            if (Mathf.Abs(mouseX) > 0.01f) // Seuil pour éviter les micro-mouvements
            {
                // Appliquer la rotation
                currentRotationY += mouseX * rotationSensitivity;
                
                // Mettre à jour la rotation de la prévisualisation
                UpdateObjectRotation();
            }
        }
        else if (Input.GetMouseButtonUp(0) && isRotatingObject) // Relâchement du clic
        {
            // Placer l'objet avec la rotation finale à la position fixée
            if (IsValidPlacement(fixedPlacementPosition))
            {
                PlaceObject(fixedPlacementPosition);
                CancelObjectPlacement();
            }
            else
            {
                Debug.Log("ActionBar: Zone non constructible ! (Océan détecté)");
                CancelObjectPlacement();
            }
            
            StopObjectRotation();
        }
    }
    
    void StartObjectRotation()
    {
        isRotatingObject = true;
        
        // Fixer la position de l'objet
        fixedPlacementPosition = previewObject.transform.position;
        
        // Calculer la rotation de base (orientation vers l'extérieur de la planète)
        Vector3 directionToCenter = (planetTransform.position - fixedPlacementPosition).normalized;
        Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
        baseRotationY = baseRotation.eulerAngles.y;
        currentRotationY = 0f; // Commencer à 0 pour la rotation relative
        
        Debug.Log($"ActionBar: Position fixée. Maintenez le clic et déplacez la souris horizontalement pour orienter {currentObjectName}. Relâchez pour placer.");
    }
    
    void StopObjectRotation()
    {
        isRotatingObject = false;
    }
    
    void UpdateObjectRotation()
    {
        if (previewObject == null) return;
        
        // Maintenir la position fixe
        previewObject.transform.position = fixedPlacementPosition;
        
        // Calculer la rotation de base (orientation vers l'extérieur de la planète)
        Vector3 directionToCenter = (planetTransform.position - fixedPlacementPosition).normalized;
        Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, -directionToCenter);
        
        // Ajouter la rotation Y personnalisée
        Quaternion customRotation = Quaternion.Euler(0, currentRotationY, 0);
        previewObject.transform.rotation = baseRotation * customRotation;
    }
    
    bool IsValidPlacement(Vector3 position)
    {
        if (hexasphereFill == null) return false;
        
        // Calculer la hauteur à cette position
        Vector3 normalizedPosition = (position - planetTransform.position).normalized;
        float height = hexasphereFill.GetVertexHeight(normalizedPosition * hexasphereFill.radius);
        
        // Vérifier si c'est au-dessus du niveau de l'eau
        float effectiveWaterLevel = hexasphereFill.GetEffectiveWaterLevel();
        bool isAboveWater = height > effectiveWaterLevel + oceanThreshold;
        
        return isAboveWater;
    }
    
    void PlaceObject(Vector3 position)
    {
        // Créer le vrai objet avec la rotation finale
        GameObject obj = Instantiate(currentPrefab, position, previewObject.transform.rotation, planetTransform);
        obj.name = currentObjectName;
        
        // Appliquer l'échelle
        obj.transform.localScale = Vector3.one * currentScale;
        
        Debug.Log($"ActionBar: {currentObjectName} placé à la position {position} avec rotation Y: {currentRotationY:F1}° et échelle: {currentScale}");
    }
    
    void SetPreviewColor(Color color)
    {
        if (previewObject == null) return;
        
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                // Utiliser directement la propriété _Color du shader Unlit/Color
                materials[i].color = color;
            }
        }
    }
    
    void SetMaterialColor(Material material, Color color)
    {
        if (material == null) return;
        
        // Gestion spécifique pour le shader Unlit/Texture
        if (material.shader.name == "Unlit/Texture")
        {
            // Pour Unlit/Texture, on peut forcer l'utilisation de _Color
            // même si elle n'apparaît pas dans les propriétés listées
            try
            {
                material.SetColor("_Color", color);
            }
            catch
            {
                // Si ça ne marche pas, utiliser un shader alternatif temporaire
                CreateTemporaryColoredMaterial(material, color);
            }
        }
        // Essayer différentes propriétés de couleur selon le shader
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        else if (material.HasProperty("_TintColor"))
        {
            material.SetColor("_TintColor", color);
        }
        else if (material.HasProperty("_MainColor"))
        {
            material.SetColor("_MainColor", color);
        }
        else if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Tint"))
        {
            material.SetColor("_Tint", color);
        }
        else
        {
            // Pour les shaders sans propriété de couleur, essayer de définir la couleur via d'autres propriétés
            Debug.LogWarning($"ActionBar: Impossible de définir la couleur pour le shader '{material.shader.name}'. Propriétés disponibles: {string.Join(", ", material.GetTexturePropertyNames())}");
        }
    }
    
    void CreateCompatibleMaterials()
    {
        if (previewObject == null) return;
        
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] originalMaterials = renderer.materials;
            Material[] newMaterials = new Material[originalMaterials.Length];
            
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                // Créer un matériau temporaire avec un shader simple qui supporte les couleurs
                Material tempMaterial = new Material(Shader.Find("Unlit/Color"));
                tempMaterial.color = Color.white;
                
                newMaterials[i] = tempMaterial;
            }
            
            renderer.materials = newMaterials;
        }
    }
    
    void CreateTemporaryColoredMaterial(Material originalMaterial, Color color)
    {
        // Créer un nouveau matériau avec un shader qui supporte les couleurs
        Material tempMaterial = new Material(Shader.Find("Unlit/Color"));
        tempMaterial.color = color;
        
        // Remplacer temporairement le matériau
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == originalMaterial)
                {
                    materials[i] = tempMaterial;
                }
            }
            renderer.materials = materials;
        }
    }
    
    bool IsPlanetOrChunk(Transform hitTransform)
    {
        // Vérifier si c'est la planète elle-même
        if (hitTransform == planetTransform)
        {
            return true;
        }
        
        // Vérifier si c'est un enfant de la planète (chunk)
        if (hitTransform.IsChildOf(planetTransform))
        {
            return true;
        }
        
        return false;
    }
}