using UnityEngine;
using System.Collections.Generic;

namespace HexSphere
{
    /// <summary>
    /// Gestionnaire principal du système de sphère hexagonale
    /// </summary>
    public class HexSphereManager : MonoBehaviour
    {
        [Header("Références")]
        public HexSphereGenerator sphereGenerator;
        
        [Header("Contrôles")]
        [Range(0.1f, 5f)]
        public float rotationSpeed = 1f;
        
        public bool autoRotate = false;
        public bool showDebugInfo = true;
        
        [Header("Interaction")]
        public Camera playerCamera;
        public LayerMask hexLayerMask = -1;
        
        [Header("Effets Visuels")]
        public bool enableHoverEffect = true;
        public Color hoverColor = Color.red;
        public float hoverScale = 1.1f;
        
        // État interne
        private HexCell hoveredCell;
        private HexCell selectedCell;
        private List<HexCell> highlightedCells = new List<HexCell>();
        
        private void Start()
        {
            // Initialiser les références si elles ne sont pas assignées
            if (sphereGenerator == null)
            {
                sphereGenerator = GetComponent<HexSphereGenerator>();
            }
            
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
            
            // Générer la sphère si elle n'existe pas
            if (sphereGenerator != null && sphereGenerator.hexCells.Count == 0)
            {
                sphereGenerator.GenerateHexSphere();
            }
        }
        
        private void Update()
        {
            HandleInput();
            HandleAutoRotation();
            UpdateHoverEffect();
        }
        
        /// <summary>
        /// Gère les entrées utilisateur
        /// </summary>
        private void HandleInput()
        {
            // Rotation manuelle avec la souris
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                
                transform.Rotate(Vector3.up, mouseX * rotationSpeed * 10f, Space.World);
                transform.Rotate(Vector3.right, -mouseY * rotationSpeed * 10f, Space.Self);
            }
            
            // Zoom avec la molette
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                transform.localScale *= (1f + scroll * 0.1f);
                transform.localScale = Vector3.ClampMagnitude(transform.localScale, 5f);
            }
            
            // Clic pour sélectionner une cellule
            if (Input.GetMouseButtonDown(0))
            {
                HexCell clickedCell = GetHexCellUnderMouse();
                if (clickedCell != null)
                {
                    SelectHexCell(clickedCell);
                }
            }
            
            // Touches pour contrôles
            if (Input.GetKeyDown(KeyCode.R))
            {
                RegenerateSphere();
            }
            
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleAutoRotation();
            }
            
            if (Input.GetKeyDown(KeyCode.G))
            {
                ToggleGizmos();
            }
        }
        
        /// <summary>
        /// Gère la rotation automatique
        /// </summary>
        private void HandleAutoRotation()
        {
            if (autoRotate)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
        }
        
        /// <summary>
        /// Met à jour l'effet de survol
        /// </summary>
        private void UpdateHoverEffect()
        {
            if (!enableHoverEffect) return;
            
            HexCell cellUnderMouse = GetHexCellUnderMouse();
            
            // Retirer l'effet de survol de la cellule précédente
            if (hoveredCell != null && hoveredCell != cellUnderMouse)
            {
                RemoveHoverEffect(hoveredCell);
            }
            
            // Appliquer l'effet de survol à la nouvelle cellule
            if (cellUnderMouse != null && cellUnderMouse != hoveredCell)
            {
                ApplyHoverEffect(cellUnderMouse);
            }
            
            hoveredCell = cellUnderMouse;
        }
        
        /// <summary>
        /// Obtient la cellule hexagonale sous la souris
        /// </summary>
        private HexCell GetHexCellUnderMouse()
        {
            if (playerCamera == null || sphereGenerator == null) return null;
            
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, hexLayerMask))
            {
                // Trouver la cellule la plus proche du point d'impact
                return sphereGenerator.GetNearestHexCell(hit.point);
            }
            
            return null;
        }
        
        /// <summary>
        /// Applique l'effet de survol à une cellule
        /// </summary>
        private void ApplyHoverEffect(HexCell cell)
        {
            if (cell == null) return;
            
            // Changer la couleur
            cell.color = hoverColor;
            
            // Appliquer un léger scale (sera géré par le mesh)
            // Note: Pour un effet de scale plus avancé, il faudrait modifier le mesh
        }
        
        /// <summary>
        /// Retire l'effet de survol d'une cellule
        /// </summary>
        private void RemoveHoverEffect(HexCell cell)
        {
            if (cell == null) return;
            
            // Restaurer la couleur originale
            cell.color = Color.white;
        }
        
        /// <summary>
        /// Sélectionne une cellule hexagonale
        /// </summary>
        public void SelectHexCell(HexCell cell)
        {
            if (cell == null) return;
            
            // Désélectionner la cellule précédente
            if (selectedCell != null)
            {
                DeselectHexCell(selectedCell);
            }
            
            selectedCell = cell;
            cell.color = Color.blue;
            
            Debug.Log($"Cellule sélectionnée: Index {cell.index}, Position {cell.center}");
            
            // Déclencher l'événement de sélection
            OnHexCellSelected(cell);
        }
        
        /// <summary>
        /// Désélectionne une cellule
        /// </summary>
        public void DeselectHexCell(HexCell cell)
        {
            if (cell == null) return;
            
            cell.color = Color.white;
            
            if (selectedCell == cell)
            {
                selectedCell = null;
            }
        }
        
        /// <summary>
        /// Surligne plusieurs cellules
        /// </summary>
        public void HighlightCells(List<HexCell> cells, Color highlightColor)
        {
            // Retirer le surlignage précédent
            ClearHighlights();
            
            highlightedCells = new List<HexCell>(cells);
            
            foreach (HexCell cell in cells)
            {
                cell.color = highlightColor;
            }
        }
        
        /// <summary>
        /// Retire tous les surlignages
        /// </summary>
        public void ClearHighlights()
        {
            foreach (HexCell cell in highlightedCells)
            {
                cell.color = Color.white;
            }
            
            highlightedCells.Clear();
        }
        
        /// <summary>
        /// Régénère la sphère
        /// </summary>
        public void RegenerateSphere()
        {
            if (sphereGenerator != null)
            {
                sphereGenerator.GenerateHexSphere();
                Debug.Log("Sphère régénérée");
            }
        }
        
        /// <summary>
        /// Active/désactive la rotation automatique
        /// </summary>
        public void ToggleAutoRotation()
        {
            autoRotate = !autoRotate;
            Debug.Log($"Rotation automatique: {(autoRotate ? "Activée" : "Désactivée")}");
        }
        
        /// <summary>
        /// Active/désactive les gizmos
        /// </summary>
        public void ToggleGizmos()
        {
            if (sphereGenerator != null)
            {
                sphereGenerator.showGizmos = !sphereGenerator.showGizmos;
                Debug.Log($"Gizmos: {(sphereGenerator.showGizmos ? "Activés" : "Désactivés")}");
            }
        }
        
        /// <summary>
        /// Obtient toutes les cellules dans un rayon
        /// </summary>
        public List<HexCell> GetCellsInRadius(Vector3 center, float radius)
        {
            List<HexCell> cellsInRadius = new List<HexCell>();
            
            if (sphereGenerator == null) return cellsInRadius;
            
            foreach (HexCell cell in sphereGenerator.hexCells)
            {
                if (Vector3.Distance(cell.center, center) <= radius)
                {
                    cellsInRadius.Add(cell);
                }
            }
            
            return cellsInRadius;
        }
        
        /// <summary>
        /// Obtient les cellules voisines d'une cellule
        /// </summary>
        public List<HexCell> GetNeighborCells(HexCell centerCell)
        {
            List<HexCell> neighbors = new List<HexCell>();
            
            if (centerCell == null || sphereGenerator == null) return neighbors;
            
            foreach (HexCell cell in sphereGenerator.hexCells)
            {
                if (cell == centerCell) continue;
                
                float distance = Vector3.Distance(cell.center, centerCell.center);
                if (distance <= centerCell.hexSize * 2f) // Distance approximative pour les voisins
                {
                    neighbors.Add(cell);
                }
            }
            
            return neighbors;
        }
        
        /// <summary>
        /// Événement déclenché lors de la sélection d'une cellule
        /// </summary>
        private void OnHexCellSelected(HexCell cell)
        {
            // Ici vous pouvez ajouter des actions personnalisées
            // Par exemple: afficher des informations, changer la couleur, etc.
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== HexSphere Manager ===");
            GUILayout.Label($"Cellules: {sphereGenerator?.hexCells.Count ?? 0}");
            GUILayout.Label($"Rotation auto: {(autoRotate ? "ON" : "OFF")}");
            GUILayout.Label($"Vitesse: {rotationSpeed:F1}");
            
            if (selectedCell != null)
            {
                GUILayout.Label($"Sélectionnée: {selectedCell.index}");
            }
            
            if (hoveredCell != null)
            {
                GUILayout.Label($"Survol: {hoveredCell.index}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Contrôles:");
            GUILayout.Label("Clic: Sélectionner");
            GUILayout.Label("R: Régénérer");
            GUILayout.Label("T: Rotation auto");
            GUILayout.Label("G: Gizmos");
            
            GUILayout.EndArea();
        }
    }
}
