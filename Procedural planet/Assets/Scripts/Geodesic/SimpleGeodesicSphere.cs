using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sphère géodésique simple avec hexagones révélés
/// </summary>
public class SimpleGeodesicSphere : MonoBehaviour
{
    [Header("Paramètres Sphère")]
    [Range(1, 10)]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;

    [Header("Affichage")]
    public bool showHexagons = true;
    public bool showOriginalTriangles = false;

    // Composants
    private PlanetHexWorld hexWorld;
    private List<GameObject> hexObjects = new List<GameObject>();

    void Start()
    {
        CreateSphere();
    }

    /// <summary>
    /// Crée la sphère géodésique
    /// </summary>
    [ContextMenu("Créer Sphère")]
    public void CreateSphere()
    {
        Debug.Log("=== CRÉATION SPHÈRE GÉODÉSIQUE SIMPLE ===");
        
        // Nettoie d'abord
        Cleanup();
        
        // Crée la grille
        CreateGrid();
        
        if (hexWorld == null || hexWorld.cells == null || hexWorld.cells.Count == 0)
        {
            Debug.LogError("Impossible de créer la grille !");
            return;
        }

        Debug.Log($"Grille créée : {hexWorld.cells.Count} cellules");

        // Désactive l'affichage des triangles originaux
        hexWorld.drawGizmos = showOriginalTriangles;

        // Crée les hexagones si demandé
        if (showHexagons)
        {
            CreateHexagons();
        }
    }

    /// <summary>
    /// Crée la grille géodésique
    /// </summary>
    private void CreateGrid()
    {
        // Supprime l'ancienne grille si elle existe
        if (hexWorld != null)
        {
            DestroyImmediate(hexWorld.gameObject);
        }

        // Crée une nouvelle grille
        GameObject gridGO = new GameObject("GeodesicGrid");
        gridGO.transform.SetParent(transform);
        hexWorld = gridGO.AddComponent<PlanetHexWorld>();
        
        // Configure les paramètres
        hexWorld.frequency = frequency;
        hexWorld.radius = radius;
        hexWorld.seed = seed;
        hexWorld.buildLatitudeDeg = 70f;
        hexWorld.drawGizmos = showOriginalTriangles;
        
        // Force la génération
        hexWorld.enabled = false;
        hexWorld.enabled = true;
    }

    /// <summary>
    /// Crée les hexagones révélés
    /// </summary>
    private void CreateHexagons()
    {
        Debug.Log("Création des hexagones révélés...");
        
        int hexCount = 0;
        int pentCount = 0;

        foreach (var cell in hexWorld.cells)
        {
            if (cell.isPentagon)
            {
                CreatePentagon(cell);
                pentCount++;
            }
            else
            {
                CreateHexagon(cell);
                hexCount++;
            }
        }

        Debug.Log($"✅ Créé : {hexCount} hexagones, {pentCount} pentagones");
    }

    /// <summary>
    /// Crée un hexagone
    /// </summary>
    private void CreateHexagon(PlanetHexWorld.Cell cell)
    {
        Vector3 planetCenter = transform.position;
        Vector3 normal = (cell.center - planetCenter).normalized;

        // Crée le GameObject
        GameObject hexGO = new GameObject($"Hex_{cell.id}");
        hexGO.transform.SetParent(transform);
        hexGO.transform.position = planetCenter + normal * radius;
        
        // Orientation
        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
        hexGO.transform.rotation = rotation;

        // Crée le mesh hexagonal
        Mesh hexMesh = CreateHexMesh(cell);

        // Ajoute les composants
        MeshFilter mf = hexGO.AddComponent<MeshFilter>();
        MeshRenderer mr = hexGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = hexMesh;
        mr.material = GetHexMaterial();

        hexObjects.Add(hexGO);
    }

    /// <summary>
    /// Crée un pentagone
    /// </summary>
    private void CreatePentagon(PlanetHexWorld.Cell cell)
    {
        Vector3 planetCenter = transform.position;
        Vector3 normal = (cell.center - planetCenter).normalized;

        // Crée le GameObject
        GameObject pentGO = new GameObject($"Pent_{cell.id}");
        pentGO.transform.SetParent(transform);
        pentGO.transform.position = planetCenter + normal * radius;
        
        // Orientation
        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
        pentGO.transform.rotation = rotation;

        // Crée le mesh pentagonal
        Mesh pentMesh = CreatePentMesh(cell);

        // Ajoute les composants
        MeshFilter mf = pentGO.AddComponent<MeshFilter>();
        MeshRenderer mr = pentGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = pentMesh;
        mr.material = GetPentMaterial();

        hexObjects.Add(pentGO);
    }

    /// <summary>
    /// Crée le mesh hexagonal basé sur les voisins
    /// </summary>
    private Mesh CreateHexMesh(PlanetHexWorld.Cell cell)
    {
        Mesh mesh = new Mesh();
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Utilise les positions des voisins pour créer l'hexagone
        if (cell.neighbors != null && cell.neighbors.Count >= 6)
        {
            for (int i = 0; i < 6; i++)
            {
                int neighborId = cell.neighbors[i];
                if (neighborId < hexWorld.cells.Count)
                {
                    var neighbor = hexWorld.cells[neighborId];
                    Vector3 neighborPos = neighbor.center - cell.center;
                    
                    // Projette sur le plan tangent
                    Vector3 tangentPos = neighborPos - Vector3.Dot(neighborPos, Vector3.up) * Vector3.up;
                    tangentPos = tangentPos.normalized * 0.5f; // Taille fixe basée sur la grille
                    
                    vertices.Add(tangentPos);
                    
                    // UV
                    float angle = Mathf.Atan2(tangentPos.z, tangentPos.x);
                    float u = 0.5f + 0.5f * Mathf.Cos(angle);
                    float v = 0.5f + 0.5f * Mathf.Sin(angle);
                    uvs.Add(new Vector2(u, v));
                }
            }
        }
        else
        {
            // Fallback : hexagone régulier
            for (int i = 0; i < 6; i++)
            {
                float angle = (i * 2f * Mathf.PI) / 6;
                Vector3 vertex = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0.1f);
                vertices.Add(vertex);
                
                float u = 0.5f + 0.5f * Mathf.Cos(angle);
                float v = 0.5f + 0.5f * Mathf.Sin(angle);
                uvs.Add(new Vector2(u, v));
            }
        }

        // Triangulation
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(((i + 1) % (vertices.Count - 1)) + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Crée le mesh pentagonal
    /// </summary>
    private Mesh CreatePentMesh(PlanetHexWorld.Cell cell)
    {
        Mesh mesh = new Mesh();
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Pentagone régulier
        for (int i = 0; i < 5; i++)
        {
            float angle = (i * 2f * Mathf.PI) / 5;
            Vector3 vertex = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0.1f);
            vertices.Add(vertex);
            
            float u = 0.5f + 0.5f * Mathf.Cos(angle);
            float v = 0.5f + 0.5f * Mathf.Sin(angle);
            uvs.Add(new Vector2(u, v));
        }

        // Triangulation
        for (int i = 0; i < 5; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(((i + 1) % 5) + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Obtient le matériau pour les hexagones
    /// </summary>
    private Material GetHexMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.blue;
        return mat;
    }

    /// <summary>
    /// Obtient le matériau pour les pentagones
    /// </summary>
    private Material GetPentMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        return mat;
    }

    /// <summary>
    /// Nettoie tout
    /// </summary>
    [ContextMenu("Nettoyer")]
    public void Cleanup()
    {
        // Nettoie les hexagones
        foreach (var obj in hexObjects)
        {
            if (obj != null)
            {
                if (obj.GetComponent<MeshFilter>() != null && obj.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    DestroyImmediate(obj.GetComponent<MeshFilter>().sharedMesh);
                }
                DestroyImmediate(obj);
            }
        }
        hexObjects.Clear();

        // Nettoie la grille
        if (hexWorld != null)
        {
            DestroyImmediate(hexWorld.gameObject);
            hexWorld = null;
        }

        Debug.Log("Nettoyage terminé");
    }

    void OnDestroy()
    {
        Cleanup();
    }
}

