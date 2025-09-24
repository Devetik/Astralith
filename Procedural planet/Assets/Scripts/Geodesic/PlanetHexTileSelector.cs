using UnityEngine;

/// <summary>
/// Sélecteur optimisé pour les tuiles hexagonales
/// </summary>
public class PlanetHexTileSelector : MonoBehaviour
{
    [Header("Configuration")]
    public Camera cam;
    public int selectedTileId = -1;
    public Color highlightColor = Color.yellow;
    public float highlightScale = 1.1f;

    [Header("Composants")]
    public PlanetHexTiles hexTiles;
    public GameObject selectedTileGO;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        hexTiles = GetComponent<PlanetHexTiles>();
    }

    void Update()
    {
        if (!cam || hexTiles == null || hexTiles.hexWorld == null || hexTiles.hexWorld.cells == null) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            SelectTile();
        }
    }

    /// <summary>
    /// Sélectionne une tuile au clic
    /// </summary>
    private void SelectTile()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Vérifie si c'est une tuile
            if (hitObject.name.StartsWith("HexTile_"))
            {
                // Extrait l'ID de la tuile
                string tileName = hitObject.name;
                string idString = tileName.Replace("HexTile_", "");
                
                if (int.TryParse(idString, out int tileId))
                {
                    SelectTileById(tileId);
                }
            }
        }
    }

    /// <summary>
    /// Sélectionne une tuile par son ID
    /// </summary>
    private void SelectTileById(int tileId)
    {
        selectedTileId = tileId;

        // Trouve la tuile dans la liste
        GameObject tileGO = hexTiles.tileObjects.Find(t => t.name == $"HexTile_{tileId}");
        
        if (tileGO != null)
        {
            // Désélectionne l'ancienne tuile
            if (selectedTileGO != null)
            {
                DeselectTile();
            }

            // Sélectionne la nouvelle tuile
            selectedTileGO = tileGO;
            HighlightTile(tileGO);

            // Affiche les informations
            if (hexTiles.hexWorld.cells.Count > tileId)
            {
                var cell = hexTiles.hexWorld.cells[tileId];
                OnSelectTile(tileId, cell);
            }
        }
    }

    /// <summary>
    /// Met en surbrillance une tuile
    /// </summary>
    private void HighlightTile(GameObject tileGO)
    {
        // Change l'échelle pour la surbrillance
        tileGO.transform.localScale = Vector3.one * highlightScale;

        // Change la couleur du matériau
        var renderer = tileGO.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Crée un nouveau matériau pour la surbrillance
            Material highlightMaterial = new Material(renderer.material);
            highlightMaterial.color = highlightColor;
            renderer.material = highlightMaterial;
        }
    }

    /// <summary>
    /// Désélectionne la tuile actuelle
    /// </summary>
    private void DeselectTile()
    {
        if (selectedTileGO != null)
        {
            // Remet l'échelle normale
            selectedTileGO.transform.localScale = Vector3.one;

            // Remet le matériau original
            var renderer = selectedTileGO.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Remet le matériau selon le type de tuile
                var cell = hexTiles.hexWorld.cells[selectedTileId];
                Material originalMaterial = hexTiles.GetTileMaterial(cell);
                renderer.material = originalMaterial;
            }

            selectedTileGO = null;
        }
    }

    /// <summary>
    /// Appelé quand une tuile est sélectionnée
    /// </summary>
    private void OnSelectTile(int tileId, PlanetHexWorld.Cell cell)
    {
        Debug.Log($"Tuile sélectionnée: {tileId}");
        Debug.Log($"  - Position: {cell.center}");
        Debug.Log($"  - Latitude: {cell.latitudeDeg:F1}°");
        Debug.Log($"  - Longitude: {cell.longitudeDeg:F1}°");
        Debug.Log($"  - Constructible: {cell.canBuild}");
        Debug.Log($"  - Pentagon: {cell.isPentagon}");
        Debug.Log($"  - Altitude: {cell.altitude:F2}");
        Debug.Log($"  - Température: {cell.temperature:F2}");
        Debug.Log($"  - Humidité: {cell.humidity:F2}");
    }

    /// <summary>
    /// Désélectionne la tuile actuelle
    /// </summary>
    [ContextMenu("Désélectionner Tuile")]
    public void DeselectCurrentTile()
    {
        DeselectTile();
        selectedTileId = -1;
    }

    /// <summary>
    /// Sélectionne une tuile aléatoire
    /// </summary>
    [ContextMenu("Sélectionner Tuile Aléatoire")]
    public void SelectRandomTile()
    {
        if (hexTiles != null && hexTiles.hexWorld != null && hexTiles.hexWorld.cells.Count > 0)
        {
            int randomId = Random.Range(0, hexTiles.hexWorld.cells.Count);
            SelectTileById(randomId);
        }
    }

    /// <summary>
    /// Sélectionne une tuile constructible aléatoire
    /// </summary>
    [ContextMenu("Sélectionner Tuile Constructible Aléatoire")]
    public void SelectRandomBuildableTile()
    {
        if (hexTiles != null && hexTiles.hexWorld != null)
        {
            var buildableCells = hexTiles.hexWorld.cells.FindAll(c => c.canBuild);
            if (buildableCells.Count > 0)
            {
                var randomCell = buildableCells[Random.Range(0, buildableCells.Count)];
                SelectTileById(randomCell.id);
            }
        }
    }
}
