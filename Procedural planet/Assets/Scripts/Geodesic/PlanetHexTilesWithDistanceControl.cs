using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Version simplifiée avec contrôle de taille pour les tuiles hexagonales
/// </summary>
public class PlanetHexTilesWithDistanceControl : MonoBehaviour
{
    [Header("Configuration")]
    public int frequency = 7;
    public float radius = 150f;
    public int seed = 12345;
    public float buildLatitudeDeg = 70f;

    [Header("Contrôle Taille")]
    [Range(0.1f, 50f)]
    public float hexSize = 5.0f;
    
    [Header("Contrôle Automatique")]
    public bool autoDetectGroups = true;
    
    [Header("Contrôle Pavage")]
    public bool useHexagonalTiling = true;
    
    // Propriétés pour compatibilité avec les autres scripts
    public bool showDebugInfo = true;
    public float hexDistance 
    { 
        get { return hexSize; } 
        set { hexSize = value; } 
    }
    public float hexScale = 1.0f;
    
    [Header("Matériaux")]
    public Material landMaterial;
    public Material waterMaterial;
    public Material buildableMaterial;

    [Header("Composants")]
    public PlanetHexWorld hexWorld;
    public List<GameObject> tileObjects = new List<GameObject>();

    [Header("Interface")]
    public Slider sizeSlider;
    public Text sizeText;

    void Start()
    {
        Debug.Log("=== DÉBUT TUILES HEXAGONALES SIMPLIFIÉES ===");
        
        // Crée l'interface si elle n'existe pas
        CreateUI();
        
        // Crée la grille de base
        CreateBaseGrid();
        
        // Attendre un frame pour que la grille se génère
        StartCoroutine(CreateTilesAfterDelay());
    }

    /// <summary>
    /// Crée les tuiles après un délai pour s'assurer que la grille est générée
    /// </summary>
    private System.Collections.IEnumerator CreateTilesAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Tentative de création des tuiles...");
        CreateAllHexTiles();
    }

    void Update()
    {
        // Met à jour l'affichage
        UpdateDisplay();
    }

    /// <summary>
    /// Crée l'interface utilisateur simple
    /// </summary>
    private void CreateUI()
    {
        // Crée un Canvas si il n'existe pas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("HexCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Crée le panneau de contrôle
        GameObject panel = new GameObject("HexControlPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.8f);
        panelRect.anchorMax = new Vector2(0.3f, 1);
        panelRect.offsetMin = new Vector2(10, 10);
        panelRect.offsetMax = new Vector2(-10, -10);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        // Crée le slider de taille
        GameObject sliderGO = new GameObject("SizeSlider");
        sliderGO.transform.SetParent(panel.transform, false);
        
        RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0.6f);
        sliderRect.anchorMax = new Vector2(0.95f, 0.8f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        sizeSlider = sliderGO.AddComponent<Slider>();
        sizeSlider.minValue = 0.1f;
        sizeSlider.maxValue = 50f;
        sizeSlider.value = hexSize;
        sizeSlider.onValueChanged.AddListener(OnSizeChanged);
        
        // Crée le background du slider
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderGO.transform, false);
        background.AddComponent<RectTransform>();
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1);
        
        // Crée le fill du slider
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderGO.transform, false);
        fill.AddComponent<RectTransform>();
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1);
        
        // Crée le handle du slider
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderGO.transform, false);
        handle.AddComponent<RectTransform>();
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        sizeSlider.targetGraphic = handleImage;
        sizeSlider.fillRect = fill.GetComponent<RectTransform>();
        sizeSlider.handleRect = handle.GetComponent<RectTransform>();

        // Crée le texte de taille
        GameObject textGO = new GameObject("SizeText");
        textGO.transform.SetParent(panel.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.4f);
        textRect.anchorMax = new Vector2(0.95f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        sizeText = textGO.AddComponent<Text>();
        sizeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        sizeText.fontSize = 16;
        sizeText.color = Color.white;
        sizeText.alignment = TextAnchor.MiddleCenter;


    }

    /// <summary>
    /// Callback pour le changement de taille
    /// </summary>
    private void OnSizeChanged(float value)
    {
        hexSize = value;
        UpdateAllTiles();
    }



    /// <summary>
    /// Met à jour l'affichage
    /// </summary>
    private void UpdateDisplay()
    {
        if (sizeText != null)
        {
            sizeText.text = $"Taille Hexagones: {hexSize:F1}";
        }
    }

    /// <summary>
    /// Crée la grille de base
    /// </summary>
    private void CreateBaseGrid()
    {
        if (hexWorld == null)
        {
            GameObject hexWorldGO = new GameObject("PlanetHexWorld");
            hexWorldGO.transform.SetParent(transform);
            hexWorld = hexWorldGO.AddComponent<PlanetHexWorld>();
            hexWorld.frequency = frequency;
            hexWorld.radius = radius;
            hexWorld.seed = seed;
            hexWorld.buildLatitudeDeg = buildLatitudeDeg;
            hexWorld.drawGizmos = false; // Désactive l'affichage de la sphère originale
        }
        
        // S'assure que la grille est générée
        if (hexWorld.cells == null || hexWorld.cells.Count == 0)
        {
            Debug.Log("Génération de la grille géodésique...");
            // Force la génération de la grille en activant/désactivant le composant
            hexWorld.enabled = false;
            hexWorld.enabled = true;
        }
        
        // Désactive l'affichage de la sphère géodésique originale
        hexWorld.drawGizmos = false;
    }

    /// <summary>
    /// Crée toutes les tuiles hexagonales
    /// </summary>
    private void CreateAllHexTiles()
    {
        Debug.Log("=== DÉBUT CRÉATION TUILES ===");
        
        // S'assure que la grille est créée et générée
        CreateBaseGrid();
        
        // Désactive l'affichage de la sphère géodésique originale
        if (hexWorld != null)
        {
            hexWorld.drawGizmos = false; // Désactive l'affichage des triangles
            Debug.Log("Affichage de la sphère géodésique originale désactivé");
        }
        
        Debug.Log($"HexWorld: {hexWorld != null}");
        if (hexWorld != null)
        {
            Debug.Log($"Cells: {hexWorld.cells != null}");
            if (hexWorld.cells != null)
            {
                Debug.Log($"Cells count: {hexWorld.cells.Count}");
            }
        }
        
        if (hexWorld == null || hexWorld.cells == null || hexWorld.cells.Count == 0)
        {
            Debug.LogError("HexWorld ou cells non trouvés ! Tentative de régénération...");
            
            // Essaie de régénérer la grille
            if (hexWorld != null)
            {
                // Force la génération en activant/désactivant le composant
                hexWorld.enabled = false;
                hexWorld.enabled = true;
                
                // Attendre un peu pour que la grille se génère
                System.Threading.Thread.Sleep(100);
                
                if (hexWorld.cells == null || hexWorld.cells.Count == 0)
                {
                    Debug.LogError("Impossible de générer la grille !");
                    return;
                }
            }
            else
            {
                Debug.LogError("HexWorld est null !");
                return;
            }
        }

        Debug.Log($"Création de {hexWorld.cells.Count} tuiles hexagonales...");

        // Nettoie les tuiles existantes
        ClearTiles();

        if (useHexagonalTiling)
        {
            // Affiche les hexagones naturels de la sphère géodésique
            Debug.Log($"Affichage des hexagones naturels de la sphère géodésique sur {hexWorld.cells.Count} cellules...");
            ShowGeodesicHexagons();
        }
        else
        {
            // Crée des tuiles individuelles (ancien système)
            Debug.Log($"Création de tuiles individuelles sur {hexWorld.cells.Count} cellules...");
            CreateIndividualTiles();
        }

        Debug.Log($"✅ {tileObjects.Count} tuiles créées !");
    }

    /// <summary>
    /// Affiche les hexagones naturels de la sphère géodésique
    /// </summary>
    private void ShowGeodesicHexagons()
    {
        int hexCount = 0;
        
        // Parcourt toutes les cellules pour afficher les hexagones
        for (int i = 0; i < hexWorld.cells.Count; i++)
        {
            var cell = hexWorld.cells[i];
            
            // Ignore les pentagones (ils ont 5 voisins, pas 6)
            if (cell.isPentagon)
                continue;
            
            // Affiche l'hexagone naturel de cette cellule
            ShowNaturalHexagon(cell);
            hexCount++;
            
            if (hexCount % 100 == 0)
            {
                Debug.Log($"Affiché {hexCount} hexagones...");
            }
        }
        
        Debug.Log($"✅ Affichage terminé : {hexCount} hexagones naturels de la sphère géodésique");
    }
    
    /// <summary>
    /// Affiche un hexagone naturel basé sur une cellule et ses voisins
    /// </summary>
    private void ShowNaturalHexagon(PlanetHexWorld.Cell centerCell)
    {
        Vector3 planetCenter = transform.position;
        Vector3 normal = (centerCell.center - planetCenter).normalized;

        // Crée le GameObject pour l'affichage
        GameObject hexGO = new GameObject($"GeodesicHex_{centerCell.id}");
        hexGO.transform.SetParent(transform);
        hexGO.transform.position = planetCenter + normal * radius;
        
        // Orientation basée sur la position
        Quaternion baseRotation = Quaternion.LookRotation(normal, Vector3.up);
        float rotation = CalculateOptimalRotation(centerCell);
        Quaternion hexRotation = Quaternion.Euler(0, 0, rotation);
        hexGO.transform.rotation = baseRotation * hexRotation;

        // Crée le mesh hexagonal naturel
        Mesh hexMesh = CreateNaturalHexMesh(centerCell);

        // Ajoute les composants pour l'affichage
        MeshFilter mf = hexGO.AddComponent<MeshFilter>();
        MeshRenderer mr = hexGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = hexMesh;
        mr.material = GetTileMaterial(centerCell);

        // Ajoute à la liste pour le nettoyage
        tileObjects.Add(hexGO);
    }
    
    /// <summary>
    /// Crée le mesh hexagonal naturel basé sur la cellule et ses voisins
    /// </summary>
    private Mesh CreateNaturalHexMesh(PlanetHexWorld.Cell centerCell)
    {
        Mesh mesh = new Mesh();
        
        // Calcule la taille basée sur la distance aux voisins
        float hexRadius = CalculateNaturalHexRadius(centerCell);
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Crée l'hexagone basé sur les positions des voisins
        if (centerCell.neighbors != null && centerCell.neighbors.Count >= 6)
        {
            // Utilise les positions réelles des voisins pour créer l'hexagone
            for (int i = 0; i < 6; i++)
            {
                int neighborId = centerCell.neighbors[i];
                if (neighborId < hexWorld.cells.Count)
                {
                    var neighbor = hexWorld.cells[neighborId];
                    Vector3 neighborPos = neighbor.center - centerCell.center;
                    
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
    /// Calcule le rayon naturel de l'hexagone basé sur les voisins
    /// </summary>
    private float CalculateNaturalHexRadius(PlanetHexWorld.Cell centerCell)
    {
        if (centerCell.neighbors == null || centerCell.neighbors.Count == 0)
            return hexSize;
        
        // Calcule la distance moyenne aux voisins
        float totalDistance = 0f;
        int validNeighbors = 0;
        
        foreach (var neighborId in centerCell.neighbors)
        {
            if (neighborId < hexWorld.cells.Count)
            {
                var neighbor = hexWorld.cells[neighborId];
                float distance = Vector3.Distance(centerCell.center, neighbor.center);
                totalDistance += distance;
                validNeighbors++;
            }
        }
        
        if (validNeighbors > 0)
        {
            float averageDistance = totalDistance / validNeighbors;
            return averageDistance * hexSize * 0.5f; // Ajuste la taille
        }
        
        return hexSize;
    }
    
    /// <summary>
    /// Crée des tuiles individuelles (ancien système)
    /// </summary>
    private void CreateIndividualTiles()
    {
        int createdCount = 0;
        
        for (int i = 0; i < hexWorld.cells.Count; i++)
        {
            var cell = hexWorld.cells[i];
            
            // Ignore les pentagones pour éviter les problèmes d'orientation
            if (cell.isPentagon)
                continue;
                
            CreateHexTile(cell);
            createdCount++;
            
            if (createdCount % 50 == 0)
            {
                Debug.Log($"Créé {createdCount} tuiles...");
            }
        }
    }
    
    /// <summary>
    /// Trouve un groupe de 6 cellules pour former un hexagone
    /// </summary>
    private List<PlanetHexWorld.Cell> FindHexGroup(PlanetHexWorld.Cell centerCell, HashSet<int> usedCells)
    {
        var group = new List<PlanetHexWorld.Cell> { centerCell };
        
        // Vérifie que la cellule centrale a exactement 6 voisins
        if (centerCell.neighbors == null || centerCell.neighbors.Count != 6)
            return new List<PlanetHexWorld.Cell>();
        
        // Prend les 6 voisins dans l'ordre
        foreach (var neighborId in centerCell.neighbors)
        {
            if (neighborId < hexWorld.cells.Count && !usedCells.Contains(neighborId))
            {
                var neighbor = hexWorld.cells[neighborId];
                if (!neighbor.isPentagon) // Évite les pentagones
                {
                    group.Add(neighbor);
                }
            }
        }
        
        // Vérifie qu'on a exactement 6 cellules (1 centrale + 5 voisins)
        if (group.Count != 6)
        {
            return new List<PlanetHexWorld.Cell>(); // Retourne un groupe vide si pas exactement 6
        }
        
        return group;
    }
    
    /// <summary>
    /// Crée une tuile hexagonale à partir d'un groupe de cellules
    /// </summary>
    private void CreateHexTileFromGroup(List<PlanetHexWorld.Cell> group)
    {
        if (group.Count != 6) return;
        
        // Calcule le centre du groupe
        Vector3 groupCenter = Vector3.zero;
        foreach (var cell in group)
        {
            groupCenter += cell.center;
        }
        groupCenter /= group.Count;
        
        Vector3 planetCenter = transform.position;
        Vector3 normal = (groupCenter - planetCenter).normalized;

        // Crée le GameObject
        GameObject tileGO = new GameObject($"HexTile_{group[0].id}");
        tileGO.transform.SetParent(transform);
        tileGO.transform.position = planetCenter + normal * radius;
        
        // Orientation basée sur la position du centre du groupe
        Quaternion baseRotation = Quaternion.LookRotation(normal, Vector3.up);
        float rotation = CalculateOptimalRotation(group[0]);
        Quaternion hexRotation = Quaternion.Euler(0, 0, rotation);
        tileGO.transform.rotation = baseRotation * hexRotation;

        // Crée le mesh hexagonal
        Mesh hexMesh = CreateHexMeshForGroup(group);

        // Ajoute les composants
        MeshFilter mf = tileGO.AddComponent<MeshFilter>();
        MeshRenderer mr = tileGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = hexMesh;
        mr.material = GetTileMaterial(group[0]);

        tileObjects.Add(tileGO);
    }
    
    /// <summary>
    /// Crée le mesh hexagonal pour un groupe
    /// </summary>
    private Mesh CreateHexMeshForGroup(List<PlanetHexWorld.Cell> group)
    {
        Mesh mesh = new Mesh();
        
        // Calcule la taille basée sur l'étendue du groupe
        float groupSize = CalculateGroupSize(group);
        float r = groupSize * hexSize;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Crée un hexagone
        for (int i = 0; i < 6; i++)
        {
            float angle = (i * 2f * Mathf.PI) / 6;
            Vector3 vertex = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0.1f);
            vertices.Add(vertex);
            
            float u = 0.5f + 0.5f * Mathf.Cos(angle);
            float v = 0.5f + 0.5f * Mathf.Sin(angle);
            uvs.Add(new Vector2(u, v));
        }

        // Triangulation
        for (int i = 0; i < 6; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(((i + 1) % 6) + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
    
    /// <summary>
    /// Calcule la taille d'un groupe de cellules
    /// </summary>
    private float CalculateGroupSize(List<PlanetHexWorld.Cell> group)
    {
        if (group.Count == 0) return 1f;
        
        // Calcule la distance maximale entre les cellules du groupe
        float maxDistance = 0f;
        for (int i = 0; i < group.Count; i++)
        {
            for (int j = i + 1; j < group.Count; j++)
            {
                float distance = Vector3.Distance(group[i].center, group[j].center);
                maxDistance = Mathf.Max(maxDistance, distance);
            }
        }
        
        return Mathf.Max(1f, maxDistance * 1.5f); // Légèrement plus grand pour couvrir le groupe
    }


    /// <summary>
    /// Crée une tuile hexagonale
    /// </summary>
    private void CreateHexTile(PlanetHexWorld.Cell cell)
    {
        Vector3 planetCenter = transform.position;
        Vector3 normal = (cell.center - planetCenter).normalized;

        // Crée le GameObject
        GameObject tileGO = new GameObject($"HexTile_{cell.id}");
        tileGO.transform.SetParent(transform); // Placé dans l'objet contenant le script
        tileGO.transform.position = planetCenter + normal * radius;
        
        // Orientation basée sur la position
        Quaternion baseRotation = Quaternion.LookRotation(normal, Vector3.up);
        float rotation = CalculateOptimalRotation(cell);
        Quaternion hexRotation = Quaternion.Euler(0, 0, rotation);
        tileGO.transform.rotation = baseRotation * hexRotation;

        // Crée le mesh hexagonal
        Mesh hexMesh = CreateHexMesh(cell);

        // Ajoute les composants
        MeshFilter mf = tileGO.AddComponent<MeshFilter>();
        MeshRenderer mr = tileGO.AddComponent<MeshRenderer>();

        mf.sharedMesh = hexMesh;
        mr.material = GetTileMaterial(cell);

        tileObjects.Add(tileGO);
        
        // Log pour les premières tuiles
        if (cell.id < 5)
        {
            Debug.Log($"Tuile {cell.id} créée à la position {tileGO.transform.position}");
        }
    }

    /// <summary>
    /// Calcule l'orientation optimale
    /// </summary>
    private float CalculateOptimalRotation(PlanetHexWorld.Cell cell)
    {
        float latitude = cell.latitudeDeg;
        float longitude = cell.longitudeDeg;
        
        float latitudeRotation = latitude * 0.5f;
        float longitudeRotation = longitude * 0.3f;
        float baseRotation = 30f;
        
        return (baseRotation + latitudeRotation + longitudeRotation) % 360f;
    }


    /// <summary>
    /// Crée le mesh hexagonal
    /// </summary>
    private Mesh CreateHexMesh(PlanetHexWorld.Cell cell)
    {
        Mesh mesh = new Mesh();

        int sides = cell.isPentagon ? 5 : 6;
        float r = hexSize;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Centre
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        // Sommets du périmètre
        for (int i = 0; i < sides; i++)
        {
            float angle = (i * 2f * Mathf.PI) / sides;
            Vector3 vertex = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0.1f);
            vertices.Add(vertex);
            
            float u = 0.5f + 0.5f * Mathf.Cos(angle);
            float v = 0.5f + 0.5f * Mathf.Sin(angle);
            uvs.Add(new Vector2(u, v));
        }

        // Triangulation
        for (int i = 0; i < sides; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(((i + 1) % sides) + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// Obtient le matériau pour une tuile
    /// </summary>
    private Material GetTileMaterial(PlanetHexWorld.Cell cell)
    {
        if (cell.canBuild && buildableMaterial != null)
            return buildableMaterial;
        else if (cell.altitude < 0 && waterMaterial != null)
            return waterMaterial;
        else if (landMaterial != null)
            return landMaterial;
        else
            return CreateDefaultMaterial();
    }

    /// <summary>
    /// Crée un matériau par défaut
    /// </summary>
    private Material CreateDefaultMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        return mat;
    }

    /// <summary>
    /// Met à jour toutes les tuiles
    /// </summary>
    private void UpdateAllTiles()
    {
        foreach (var tile in tileObjects)
        {
            if (tile != null)
            {
                // Met à jour la taille
                MeshFilter mf = tile.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    // Recrée le mesh avec la nouvelle taille
                    PlanetHexWorld.Cell cell = GetCellFromTile(tile);
                    if (cell != null)
                    {
                        mf.sharedMesh = CreateHexMesh(cell);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Obtient la cellule à partir d'une tuile
    /// </summary>
    private PlanetHexWorld.Cell GetCellFromTile(GameObject tile)
    {
        string tileName = tile.name;
        if (tileName.StartsWith("HexTile_"))
        {
            string idStr = tileName.Substring(8);
            if (int.TryParse(idStr, out int id))
            {
                if (hexWorld != null && hexWorld.cells != null && id < hexWorld.cells.Count)
                {
                    return hexWorld.cells[id];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Nettoie toutes les tuiles
    /// </summary>
    public void ClearTiles()
    {
        foreach (var tile in tileObjects)
        {
            if (tile != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(tile);
                }
                else
                {
                    DestroyImmediate(tile);
                }
            }
        }
        tileObjects.Clear();
    }

    /// <summary>
    /// Fonction de test pour recréer les tuiles
    /// </summary>
    [ContextMenu("Recréer Tuiles")]
    public void RecreateTiles()
    {
        CreateAllHexTiles();
    }

    /// <summary>
    /// Méthode pour compatibilité avec les autres scripts
    /// </summary>
    public void CreateHexTiles()
    {
        CreateAllHexTiles();
    }

    /// <summary>
    /// Obtient les statistiques des tuiles
    /// </summary>
    public string GetTileStats()
    {
        return $"Tuiles: {tileObjects.Count}, Taille: {hexSize:F1}";
    }
}
