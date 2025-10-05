using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Crée une sphère géodésique et révèle les hexagones en masquant les 6 bras
/// </summary>
public class GeodesicSphereHexagons : MonoBehaviour
{
    [Header("Paramètres de la Sphère")]
    [Range(1, 10)]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;

    [Header("Contrôle Affichage")]
    [Range(0.1f, 50f)]
    public float hexSize = 5.0f;
    public bool showHexagons = true;
    public bool showOriginalSphere = false;

    [Header("Matériaux")]
    public Material hexagonMaterial;
    public Material pentagonMaterial;

    // Composants
    private PlanetHexWorld hexWorld;
    private List<GameObject> hexagonObjects = new List<GameObject>();

    void Start()
    {
        Debug.Log("=== CRÉATION SPHÈRE GÉODÉSIQUE AVEC HEXAGONES ===");
        CreateGeodesicSphere();
    }

    /// <summary>
    /// Crée la sphère géodésique avec les hexagones révélés
    /// </summary>
    private void CreateGeodesicSphere()
    {
        // Crée la grille géodésique
        CreateBaseGrid();
        
        if (hexWorld == null || hexWorld.cells == null || hexWorld.cells.Count == 0)
        {
            Debug.LogError("Impossible de créer la grille géodésique !");
            return;
        }

        Debug.Log($"Grille géodésique créée : {hexWorld.cells.Count} cellules");

        // Désactive l'affichage de la sphère originale
        hexWorld.drawGizmos = showOriginalSphere;

        // Crée les hexagones révélés
        if (showHexagons)
        {
            CreateRevealedHexagons();
        }
    }

    /// <summary>
    /// Crée la grille de base
    /// </summary>
    private void CreateBaseGrid()
    {
        if (hexWorld == null)
        {
            GameObject hexWorldGO = new GameObject("GeodesicSphere");
            hexWorldGO.transform.SetParent(transform);
            hexWorld = hexWorldGO.AddComponent<PlanetHexWorld>();
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = buildLatitudeDeg;
            hexWorld.drawGizmos = showOriginalSphere;
        }
        
        // S'assure que la grille est générée
        if (hexWorld.cells == null || hexWorld.cells.Count == 0)
        {
            Debug.Log("Génération de la grille géodésique...");
            hexWorld.enabled = false;
            hexWorld.enabled = true;
        }
    }

    /// <summary>
    /// Crée les hexagones révélés (sans les 6 bras)
    /// </summary>
    private void CreateRevealedHexagons()
    {
        Debug.Log("Création des hexagones révélés...");
        
        // Nettoie les hexagones existants
        ClearHexagons();

        int hexCount = 0;
        int pentCount = 0;

        // Parcourt toutes les cellules
        for (int i = 0; i < hexWorld.cells.Count; i++)
        {
            var cell = hexWorld.cells[i];
            
            if (cell.isPentagon)
            {
                // Crée un pentagone
                CreatePentagon(cell);
                pentCount++;
            }
            else
            {
                // Crée un hexagone
                CreateHexagon(cell);
                hexCount++;
            }
        }

        Debug.Log($"✅ Hexagones créés : {hexCount} hexagones, {pentCount} pentagones");
    }

    /// <summary>
    /// Crée un hexagone révélé
    /// </summary>
    private void CreateHexagon(PlanetHexWorld.Cell cell)
    {
        Vector3 planetCenter = transform.position;
        Vector3 normal = (cell.center - planetCenter).normalized;

        // Crée le GameObject
        GameObject hexGO = new GameObject($"Hex_{cell.id}");
        hexGO.transform.SetParent(transform);
        hexGO.transform.position = planetCenter + normal * radius;
        
        // Orientation basée sur la position
        Quaternion baseRotation = Quaternion.LookRotation(normal, Vector3.up);
        float rotation = CalculateHexRotation(cell);
        Quaternion hexRotation = Quaternion.Euler(0, 0, rotation);
        hexGO.transform.rotation = baseRotation * hexRotation;

        // Crée le mesh hexagonal
        Mesh hexMesh = CreateHexMesh(cell);

        // Ajoute les composants
        MeshFilter mf = hexGO.AddComponent<MeshFilter>();
        MeshRenderer mr = hexGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = hexMesh;
        mr.material = hexagonMaterial != null ? hexagonMaterial : GetDefaultMaterial(cell);

        hexagonObjects.Add(hexGO);
    }

    /// <summary>
    /// Crée un pentagone révélé
    /// </summary>
    private void CreatePentagon(PlanetHexWorld.Cell cell)
    {
        Vector3 planetCenter = transform.position;
        Vector3 normal = (cell.center - planetCenter).normalized;

        // Crée le GameObject
        GameObject pentGO = new GameObject($"Pent_{cell.id}");
        pentGO.transform.SetParent(transform);
        pentGO.transform.position = planetCenter + normal * radius;
        
        // Orientation basée sur la position
        Quaternion baseRotation = Quaternion.LookRotation(normal, Vector3.up);
        float rotation = CalculateHexRotation(cell);
        Quaternion pentRotation = Quaternion.Euler(0, 0, rotation);
        pentGO.transform.rotation = baseRotation * pentRotation;

        // Crée le mesh pentagonal
        Mesh pentMesh = CreatePentMesh(cell);

        // Ajoute les composants
        MeshFilter mf = pentGO.AddComponent<MeshFilter>();
        MeshRenderer mr = pentGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = pentMesh;
        mr.material = pentagonMaterial != null ? pentagonMaterial : GetDefaultMaterial(cell);

        hexagonObjects.Add(pentGO);
    }

    /// <summary>
    /// Crée le mesh hexagonal
    /// </summary>
    private Mesh CreateHexMesh(PlanetHexWorld.Cell cell)
    {
        Mesh mesh = new Mesh();
        
        // Calcule la taille basée sur les voisins
        float hexRadius = CalculateHexRadius(cell);
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Crée l'hexagone basé sur les positions des voisins
        if (cell.neighbors != null && cell.neighbors.Count >= 6)
        {
            // Utilise les positions réelles des voisins
            for (int i = 0; i < 6; i++)
            {
                int neighborId = cell.neighbors[i];
                if (neighborId < hexWorld.cells.Count)
                {
                    var neighbor = hexWorld.cells[neighborId];
                    Vector3 neighborPos = neighbor.center - cell.center;
                    
                    // Projette sur le plan tangent
                    Vector3 tangentPos = neighborPos - Vector3.Dot(neighborPos, Vector3.up) * Vector3.up;
                    tangentPos = tangentPos.normalized * hexRadius;
                    
                    vertices.Add(tangentPos);
                    
                    // UV basé sur l'angle
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
                Vector3 vertex = new Vector3(Mathf.Cos(angle) * hexRadius, Mathf.Sin(angle) * hexRadius, 0.1f);
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
        
        // Calcule la taille basée sur les voisins
        float pentRadius = CalculateHexRadius(cell);
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Crée le pentagone
        for (int i = 0; i < 5; i++)
        {
            float angle = (i * 2f * Mathf.PI) / 5;
            Vector3 vertex = new Vector3(Mathf.Cos(angle) * pentRadius, Mathf.Sin(angle) * pentRadius, 0.1f);
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
    /// Calcule le rayon de l'hexagone basé sur les voisins
    /// </summary>
    private float CalculateHexRadius(PlanetHexWorld.Cell cell)
    {
        if (cell.neighbors == null || cell.neighbors.Count == 0)
            return hexSize;
        
        // Calcule la distance moyenne aux voisins
        float totalDistance = 0f;
        int validNeighbors = 0;
        
        foreach (var neighborId in cell.neighbors)
        {
            if (neighborId < hexWorld.cells.Count)
            {
                var neighbor = hexWorld.cells[neighborId];
                float distance = Vector3.Distance(cell.center, neighbor.center);
                totalDistance += distance;
                validNeighbors++;
            }
        }
        
        if (validNeighbors > 0)
        {
            float averageDistance = totalDistance / validNeighbors;
            return averageDistance * hexSize * 0.5f;
        }
        
        return hexSize;
    }

    /// <summary>
    /// Calcule la rotation optimale de l'hexagone
    /// </summary>
    private float CalculateHexRotation(PlanetHexWorld.Cell cell)
    {
        // Rotation basée sur la latitude
        float latitude = Mathf.Asin(cell.center.y / cell.center.magnitude) * Mathf.Rad2Deg;
        float longitude = Mathf.Atan2(cell.center.z, cell.center.x) * Mathf.Rad2Deg;
        
        // Rotation de base
        float baseRotation = 30f;
        
        // Ajustement basé sur la position
        float latitudeRotation = latitude * 0.1f;
        float longitudeRotation = longitude * 0.05f;
        
        return (baseRotation + latitudeRotation + longitudeRotation) % 360f;
    }

    /// <summary>
    /// Obtient le matériau par défaut
    /// </summary>
    private Material GetDefaultMaterial(PlanetHexWorld.Cell cell)
    {
        // Crée un matériau par défaut
        Material mat = new Material(Shader.Find("Standard"));
        
        if (cell.isPentagon)
        {
            mat.color = Color.red; // Pentagones en rouge
        }
        else
        {
            mat.color = Color.blue; // Hexagones en bleu
        }
        
        return mat;
    }

    /// <summary>
    /// Nettoie tous les hexagones
    /// </summary>
    private void ClearHexagons()
    {
        foreach (var hex in hexagonObjects)
        {
            if (hex != null)
            {
                if (hex.GetComponent<MeshFilter>() != null && hex.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    DestroyImmediate(hex.GetComponent<MeshFilter>().sharedMesh);
                }
                DestroyImmediate(hex);
            }
        }
        hexagonObjects.Clear();
    }

    /// <summary>
    /// Met à jour l'affichage
    /// </summary>
    public void UpdateDisplay()
    {
        if (hexWorld != null)
        {
            hexWorld.drawGizmos = showOriginalSphere;
        }
        
        if (showHexagons)
        {
            CreateRevealedHexagons();
        }
        else
        {
            ClearHexagons();
        }
    }

    void OnDestroy()
    {
        ClearHexagons();
    }

    // Fonctions pour l'interface
    [ContextMenu("Créer Sphère")]
    public void CreateSphere()
    {
        CreateGeodesicSphere();
    }

    [ContextMenu("Nettoyer")]
    public void Cleanup()
    {
        ClearHexagons();
    }
}

