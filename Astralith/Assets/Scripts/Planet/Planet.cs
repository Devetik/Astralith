using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Planet : MonoBehaviour
{

    [Range(2, 256)]
    public int resolution = 10;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;
    
    public enum NormalCalculationMode { Smooth, LowPoly };
    [Header("Rendering Style")]
    public NormalCalculationMode normalMode = NormalCalculationMode.Smooth;
    
    [Header("Hierarchical Subdivision")]
    [Range(0, 3)]
    public int subdivisionLevel = 0;
    [Tooltip("0 = 6 faces, 1 = 24 faces, 2 = 96 faces, 3 = 384 faces")]
    
    [Header("Texture Quality")]
    [Range(64, 1024)]
    public int textureResolution = 256;
    [Tooltip("Résolution de la texture de couleurs (plus élevé = plus précis)")]
    
    [Header("Collision Settings")]
    public bool generateColliders = true;
    public Transform MainCamera;

    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;
    public LODSettings lodSettings;
    public PropsSettings propsSettings;

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;
    [HideInInspector]
    public bool lodSettingsFoldout;
    [HideInInspector]
    public bool propsSettingsFoldout;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    void Start()
    {
        if (autoUpdate)
        {
            GeneratePlanet();
        }
    }

    void OnValidate()
    {
        if (autoUpdate)
        {
            // Délai pour éviter les appels multiples pendant l'édition
            if (Application.isPlaying)
            {
                GeneratePlanet();
            }
            else
            {
                // En Edit mode, utiliser un délai pour éviter les appels répétés
#if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    if (this != null && autoUpdate)
                    {
                        GeneratePlanet();
                    }
                };
#endif
            }
        }
    }

#if UNITY_EDITOR
    void OnEnable()
    {
        // Se déclenche quand on entre/sort du Play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && autoUpdate)
        {
            // Régénérer la planète quand on revient en Edit mode
            EditorApplication.delayCall += () => {
                if (this != null && autoUpdate)
                {
                    GeneratePlanet();
                }
            };
        }
    }
#endif

    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);
        colourGenerator.SetTextureResolution(textureResolution);

        int numFaces = GetFaceCount();
        
        // Nettoyer les anciens mesh si le nombre de faces a changé
        if (meshFilters != null && meshFilters.Length != numFaces)
        {
            CleanupOldMeshes();
        }
        
        if (meshFilters == null || meshFilters.Length != numFaces)
        {
            meshFilters = new MeshFilter[numFaces];
        }
        terrainFaces = new TerrainFace[numFaces];

        Vector3[] directions = GetFaceDirections();

        for (int i = 0; i < numFaces; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject($"mesh_{i}");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                
                // Ajouter MeshCollider si demandé
                if (generateColliders)
                {
                    MeshCollider meshCollider = meshObj.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilters[i].sharedMesh;
                }
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;

            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i], normalMode, subdivisionLevel, i);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == (i / GetFacesPerDirection());
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    void CleanupOldMeshes()
    {
        if (meshFilters != null)
        {
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i] != null && meshFilters[i].gameObject != null)
                {
                    // Détruire le GameObject et son mesh
                    if (Application.isPlaying)
                    {
                        Destroy(meshFilters[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(meshFilters[i].gameObject);
                    }
                }
            }
        }
    }

    public void SetResolution(int newResolution)
    {
        resolution = Mathf.Clamp(newResolution, 2, 256);
        if (autoUpdate)
        {
            GeneratePlanet();
        }
    }

    int GetFaceCount()
    {
        int facesPerDirection = GetFacesPerDirection();
        return 6 * facesPerDirection;
    }

    int GetFacesPerDirection()
    {
        return (int)Mathf.Pow(4, subdivisionLevel);
    }

    Vector3[] GetFaceDirections()
    {
        int facesPerDirection = GetFacesPerDirection();
        Vector3[] directions = new Vector3[6 * facesPerDirection];
        Vector3[] baseDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        
        for (int face = 0; face < 6; face++)
        {
            Vector3 baseDir = baseDirections[face];
            
            for (int sub = 0; sub < facesPerDirection; sub++)
            {
                int index = face * facesPerDirection + sub;
                directions[index] = baseDir;
            }
        }
        
        return directions;
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColours();
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
        }
    }

    public void OnColourSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateColours();
        }
    }

    public void OnPropsSettingsUpdated()
    {
        if (autoUpdate)
        {
            // Pour l'instant, pas d'implémentation
            // Sera utilisé pour la génération de props
        }
    }
    public void OnlodSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
        }
    }

    [ContextMenu("Force Regenerate Planet")]
    public void ForceRegeneratePlanet()
    {
        GeneratePlanet();
    }

    void GenerateMesh()
    {
        int numFaces = GetFaceCount();
        
        for (int i = 0; i < numFaces; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
                
                // Mettre à jour le collider si nécessaire
                if (generateColliders)
                {
                    UpdateMeshCollider(i);
                }
            }
        }

        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColours()
    {
        colourGenerator.UpdateColours();
        
        int numFaces = GetFaceCount();
        
        for (int i = 0; i < numFaces; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colourGenerator);
            }
        }
    }

    void UpdateMeshCollider(int faceIndex)
    {
        if (meshFilters[faceIndex] != null)
        {
            MeshCollider meshCollider = meshFilters[faceIndex].GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                // Forcer la mise à jour du collider
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = meshFilters[faceIndex].sharedMesh;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (lodSettings != null && lodSettings.debugLOD)
        {
            // Obtenir la caméra
            Transform cam = MainCamera;
            if (cam == null && Camera.main != null)
            {
                cam = Camera.main.transform;
            }
            
            if (cam != null)
            {
                // Dessiner une ligne du centre de la planète vers la caméra
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, cam.position);
                
                // Trouver le point de contact réel avec le mesh
                Vector3 directionToCamera = (cam.position - transform.position).normalized;
                RaycastHit hit;
                
                // Raycast depuis la caméra vers le centre de la planète
                if (Physics.Raycast(cam.position, -directionToCamera, out hit))
                {
                    // Point de contact réel avec le mesh
                    Vector3 contactPoint = hit.point;
                    
                    // Dessiner une sphère de 0.5 unités au point de contact réel
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(contactPoint, 0.5f);
                    
                    // Dessiner la normale du mesh au point de contact
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(contactPoint, contactPoint + hit.normal * 2f);
                }
                else
                {
                    // Fallback : utiliser le rayon de la sphère si pas de hit
                    float planetRadius = shapeGenerator != null ? shapeGenerator.GetScaledElevation(0) : 1f;
                    Vector3 contactPoint = transform.position + directionToCamera * planetRadius;
                    
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(contactPoint, 0.5f);
                }
            }
        }
    }
}
