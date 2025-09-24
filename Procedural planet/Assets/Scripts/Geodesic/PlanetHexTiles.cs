using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Système de tuiles hexagonales individuelles comme RimWorld
/// </summary>
public class PlanetHexTiles : MonoBehaviour
{
    [Header("Configuration Tuiles")]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;
    public bool showDebugInfo = true;

    [Header("Tuiles")]
    public float tileSize = 1f; // Taille d'une tuile hexagonale
    public Material landMaterial;
    public Material waterMaterial;
    public Material buildableMaterial;

    [Header("Composants")]
    public PlanetHexWorld hexWorld;
    public PlanetTileSelector tileSelector;

    [Header("Données")]
    public List<GameObject> tileObjects = new List<GameObject>();
    public Transform tilesParent;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT SYSTÈME TUILES HEXAGONALES ===");
        }
        
        CreateHexTiles();
    }

    /// <summary>
    /// Crée les tuiles hexagonales individuelles
    /// </summary>
    [ContextMenu("Créer Tuiles Hexagonales")]
    public void CreateHexTiles()
    {
        if (showDebugInfo)
        {
            Debug.Log("Création des tuiles hexagonales...");
        }

        try
        {
            // Crée le parent pour les tuiles
            if (tilesParent == null)
            {
                GameObject parentGO = new GameObject("HexTiles");
                tilesParent = parentGO.transform;
                tilesParent.SetParent(transform);
            }

            // Nettoie les anciennes tuiles
            ClearTiles();

            // Crée d'abord la grille de base
            CreateBaseGrid();

            // Crée les tuiles individuelles
            CreateIndividualTiles();

            if (showDebugInfo)
            {
                Debug.Log($"Tuiles créées: {tileObjects.Count}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la création des tuiles: {e.Message}");
        }
    }

    /// <summary>
    /// Crée la grille de base
    /// </summary>
    private void CreateBaseGrid()
    {
        // Crée un GameObject temporaire pour la grille
        GameObject gridGO = new GameObject("TempGrid");
        gridGO.transform.position = Vector3.zero;

        // Ajoute PlanetHexWorld
        hexWorld = gridGO.AddComponent<PlanetHexWorld>();
        
        // Configure la grille
        hexWorld.frequency = frequency;
        hexWorld.radius = radius;
        hexWorld.seed = seed;
        hexWorld.buildLatitudeDeg = buildLatitudeDeg;
        hexWorld.excludePentagonsFromBuild = true;
        hexWorld.generateOnStart = false;
        hexWorld.drawGizmos = false; // Pas de gizmos !

        // Génère la grille
        hexWorld.Generate();

        if (showDebugInfo)
        {
            Debug.Log($"Grille générée: {hexWorld.cells.Count} cellules");
        }
    }

    /// <summary>
    /// Crée les tuiles individuelles
    /// </summary>
    private void CreateIndividualTiles()
    {
        foreach (var cell in hexWorld.cells)
        {
            CreateHexTile(cell);
        }
    }

    /// <summary>
    /// Crée une tuile hexagonale individuelle
    /// </summary>
    private void CreateHexTile(PlanetHexWorld.Cell cell)
    {
        // Crée le GameObject de la tuile
        GameObject tileGO = new GameObject($"HexTile_{cell.id}");
        tileGO.transform.SetParent(tilesParent);
        tileGO.transform.position = cell.center;

        // Crée le mesh hexagonal
        Mesh hexMesh = CreateHexMesh(cell);
        
        // Ajoute les composants
        MeshFilter meshFilter = tileGO.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = tileGO.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = tileGO.AddComponent<MeshCollider>();

        // Assigne le mesh
        meshFilter.mesh = hexMesh;
        meshCollider.sharedMesh = hexMesh;

        // Assigne le matériau selon le type
        Material tileMaterial = GetTileMaterial(cell);
        meshRenderer.material = tileMaterial;

        // Ajoute un tag pour la sélection
        if (cell.canBuild)
        {
            tileGO.tag = "BuildableTile";
        }
        else if (cell.isPentagon)
        {
            tileGO.tag = "PentagonTile";
        }
        else
        {
            tileGO.tag = "HexTile";
        }

        // Stocke la référence
        tileObjects.Add(tileGO);
    }

    /// <summary>
    /// Crée le mesh hexagonal pour une cellule
    /// </summary>
    private Mesh CreateHexMesh(PlanetHexWorld.Cell cell)
    {
        Mesh mesh = new Mesh();
        
        // Calcule les sommets du hexagone
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Centre de la cellule
        Vector3 center = Vector3.zero;
        vertices.Add(center);
        normals.Add(cell.center.normalized);

        // Sommets du hexagone (6 côtés)
        int sides = cell.isPentagon ? 5 : 6;
        for (int i = 0; i < sides; i++)
        {
            float angle = (i * 2f * Mathf.PI) / sides;
            Vector3 vertex = new Vector3(
                Mathf.Cos(angle) * tileSize,
                0f,
                Mathf.Sin(angle) * tileSize
            );
            vertices.Add(vertex);
            normals.Add(cell.center.normalized);
        }

        // Crée les triangles (fan triangulation)
        for (int i = 0; i < sides; i++)
        {
            triangles.Add(0); // Centre
            triangles.Add(i + 1);
            triangles.Add(((i + 1) % sides) + 1);
        }

        // Assigne les données au mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Obtient le matériau pour une tuile
    /// </summary>
    public Material GetTileMaterial(PlanetHexWorld.Cell cell)
    {
        if (cell.canBuild)
        {
            return buildableMaterial != null ? buildableMaterial : CreateDefaultBuildableMaterial();
        }
        else if (cell.isPentagon)
        {
            return CreateDefaultPentagonMaterial();
        }
        else
        {
            // Détermine le type de terrain basé sur l'altitude
            if (cell.altitude < 0f)
            {
                return waterMaterial != null ? waterMaterial : CreateDefaultWaterMaterial();
            }
            else
            {
                return landMaterial != null ? landMaterial : CreateDefaultLandMaterial();
            }
        }
    }

    /// <summary>
    /// Crée un matériau par défaut pour la terre
    /// </summary>
    private Material CreateDefaultLandMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.2f, 0.8f, 0.2f); // Vert
        return material;
    }

    /// <summary>
    /// Crée un matériau par défaut pour l'eau
    /// </summary>
    private Material CreateDefaultWaterMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.2f, 0.4f, 0.8f); // Bleu
        return material;
    }

    /// <summary>
    /// Crée un matériau par défaut pour les tuiles constructibles
    /// </summary>
    private Material CreateDefaultBuildableMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.8f, 0.8f, 0.2f); // Jaune
        return material;
    }

    /// <summary>
    /// Crée un matériau par défaut pour les pentagones
    /// </summary>
    private Material CreateDefaultPentagonMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.8f, 0.2f, 0.2f); // Rouge
        return material;
    }

    /// <summary>
    /// Nettoie les tuiles existantes
    /// </summary>
    [ContextMenu("Nettoyer Tuiles")]
    public void ClearTiles()
    {
        if (tilesParent != null)
        {
            foreach (Transform child in tilesParent)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        tileObjects.Clear();

        if (showDebugInfo)
        {
            Debug.Log("Tuiles nettoyées");
        }
    }

    /// <summary>
    /// Génère une nouvelle seed
    /// </summary>
    [ContextMenu("Nouvelle Seed")]
    public void GenerateNewSeed()
    {
        seed = Random.Range(0, int.MaxValue);
        CreateHexTiles();

        if (showDebugInfo)
        {
            Debug.Log($"Nouvelle seed générée: {seed}");
        }
    }

    /// <summary>
    /// Obtient les statistiques des tuiles
    /// </summary>
    public string GetTileStats()
    {
        if (hexWorld == null || hexWorld.cells == null)
        {
            return "Générateur non initialisé";
        }

        int totalTiles = tileObjects.Count;
        int buildableTiles = hexWorld.cells.FindAll(c => c.canBuild).Count;
        int pentagonTiles = hexWorld.cells.FindAll(c => c.isPentagon).Count;
        int hexTiles = hexWorld.cells.FindAll(c => !c.isPentagon).Count;

        return $"Tuiles: {totalTiles}\n" +
               $"Hexagones: {hexTiles}\n" +
               $"Pentagones: {pentagonTiles}\n" +
               $"Constructibles: {buildableTiles}\n" +
               $"Ratio constructible: {(float)buildableTiles / totalTiles * 100f:F1}%";
    }
}
