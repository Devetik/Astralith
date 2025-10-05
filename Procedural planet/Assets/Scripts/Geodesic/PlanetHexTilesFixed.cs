using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Version corrigée des tuiles hexagonales avec orientation correcte
/// </summary>
public class PlanetHexTilesFixed : MonoBehaviour
{
    [Header("Configuration Tuiles")]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;
    public bool showDebugInfo = true;

    [Header("Tuiles")]
    public Material landMaterial;
    public Material waterMaterial;
    public Material buildableMaterial;

    [Header("Composants")]
    public PlanetHexWorld hexWorld;
    public List<GameObject> tileObjects = new List<GameObject>();
    public Transform tilesParent;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT TUILES HEXAGONALES CORRIGÉES ===");
        }
        
        CreateHexTiles();
    }

    /// <summary>
    /// Crée les tuiles hexagonales
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
        hexWorld.drawGizmos = false;

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

        // Crée le mesh hexagonal avec orientation correcte
        Mesh hexMesh = CreateHexMeshWithCorrectOrientation(cell);
        
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

        // Ajoute un tag pour la sélection (utilise des tags existants)
        if (cell.canBuild)
        {
            tileGO.tag = "Untagged"; // Utilise le tag par défaut
        }
        else if (cell.isPentagon)
        {
            tileGO.tag = "Untagged"; // Utilise le tag par défaut
        }
        else
        {
            tileGO.tag = "Untagged"; // Utilise le tag par défaut
        }

        // Stocke la référence
        tileObjects.Add(tileGO);
    }

    /// <summary>
    /// Crée le mesh hexagonal avec orientation correcte vers le centre
    /// </summary>
    private Mesh CreateHexMeshWithCorrectOrientation(PlanetHexWorld.Cell cell)
    {
        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Centre de la cellule
        Vector3 center = cell.center;
        vertices.Add(center);
        normals.Add(center.normalized);

        // Calcule les points du périmètre basés sur les voisins
        List<Vector3> perimeterPoints = new List<Vector3>();
        
        if (cell.neighbors != null && cell.neighbors.Count > 0)
        {
            // Utilise les voisins pour créer le périmètre
            for (int i = 0; i < cell.neighbors.Count; i++)
            {
                int neighborId = cell.neighbors[i];
                if (neighborId < hexWorld.cells.Count)
                {
                    var neighbor = hexWorld.cells[neighborId];
                    
                    // Point de périmètre au milieu entre le centre et le voisin
                    Vector3 direction = (neighbor.center - center).normalized;
                    Vector3 perimeterPoint = center + direction * 0.3f; // Distance réduite pour les tuiles
                    perimeterPoints.Add(perimeterPoint);
                }
            }
        }
        
        // Si pas de voisins, crée un hexagone basique orienté vers le centre
        if (perimeterPoints.Count == 0)
        {
            int hexSides = cell.isPentagon ? 5 : 6;
            for (int i = 0; i < hexSides; i++)
            {
                float angle = (i * 2f * Mathf.PI) / hexSides;
                
                // Crée un hexagone dans le plan tangent à la sphère
                Vector3 tangent1 = Vector3.Cross(center, Vector3.up).normalized;
                if (tangent1.magnitude < 0.1f)
                {
                    tangent1 = Vector3.Cross(center, Vector3.right).normalized;
                }
                Vector3 tangent2 = Vector3.Cross(center, tangent1).normalized;
                
                // Crée le point du périmètre
                Vector3 offset = (tangent1 * Mathf.Cos(angle) + tangent2 * Mathf.Sin(angle)) * 0.1f;
                Vector3 perimeterPoint = center + offset;
                perimeterPoints.Add(perimeterPoint);
            }
        }

        // Ajoute les sommets du périmètre
        foreach (Vector3 point in perimeterPoints)
        {
            vertices.Add(point);
            normals.Add(point.normalized);
        }

        // Crée les triangles (fan triangulation depuis le centre)
        int triangleSides = perimeterPoints.Count;
        for (int i = 0; i < triangleSides; i++)
        {
            triangles.Add(0); // Centre
            triangles.Add(i + 1);
            triangles.Add(((i + 1) % triangleSides) + 1);
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
