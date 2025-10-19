using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;


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

    void GenerateMesh()
    {
        int numFaces = GetFaceCount();
        
        for (int i = 0; i < numFaces; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
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
}
