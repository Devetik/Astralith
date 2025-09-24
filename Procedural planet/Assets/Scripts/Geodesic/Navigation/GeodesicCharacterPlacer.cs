using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Geodesic
{
    /// <summary>
    /// Gestionnaire de placement de personnage sur la grille géodésique
    /// </summary>
    public class GeodesicCharacterPlacer : MonoBehaviour
    {
        [Header("Configuration")]
        public Transform characterPrefab;
        public GeodesicPlanetGenerator planetGenerator;
        public GeodesicGrid geodesicGrid;
        
        [Header("Placement")]
        public bool placeOnStart = true;
        public bool preferMainContinent = true;
        public float spawnHeight = 1f;
        
        [Header("Debug")]
        public bool showDebugInfo = true;
        public bool showSpawnGizmos = true;
        
        private Transform currentCharacter;
        private Vector3 lastSpawnPosition;
        
        private void Start()
        {
            if (placeOnStart)
            {
                PlaceCharacterOnPlanet();
            }
        }
        
        /// <summary>
        /// Place le personnage sur la planète
        /// </summary>
        public void PlaceCharacterOnPlanet()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== PLACEMENT PERSONNAGE GÉODÉSIQUE ===");
            }
            
            // Trouve les composants si non assignés
            FindComponents();
            
            if (planetGenerator == null)
            {
                Debug.LogError("Générateur de planète non trouvé !");
                return;
            }
            
            if (geodesicGrid == null)
            {
                Debug.LogError("Grille géodésique non trouvée !");
                return;
            }
            
            // Obtient une position de spawn
            Vector3 spawnPosition = GetSpawnPosition();
            
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning("Aucune position de spawn valide trouvée !");
                return;
            }
            
            // Place le personnage
            PlaceCharacterAtPosition(spawnPosition);
            
            if (showDebugInfo)
            {
                Debug.Log($"Personnage placé à: {spawnPosition}");
                Debug.Log("=== FIN PLACEMENT ===");
            }
        }
        
        /// <summary>
        /// Trouve les composants nécessaires
        /// </summary>
        private void FindComponents()
        {
            if (planetGenerator == null)
            {
                planetGenerator = FindObjectOfType<GeodesicPlanetGenerator>();
            }
            
            if (geodesicGrid == null)
            {
                geodesicGrid = FindObjectOfType<GeodesicGrid>();
            }
            
            if (geodesicGrid == null && planetGenerator != null)
            {
                geodesicGrid = planetGenerator.GetComponent<GeodesicGrid>();
            }
        }
        
        /// <summary>
        /// Obtient une position de spawn valide
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            if (preferMainContinent)
            {
                // Essaie de trouver une position sur le continent principal
                Vector3 mainContinentPosition = geodesicGrid.GetSpawnPositionOnMainContinent();
                if (mainContinentPosition != Vector3.zero)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log("Position trouvée sur le continent principal");
                    }
                    return mainContinentPosition;
                }
            }
            
            // Fallback: trouve une position sur n'importe quelle terre
            List<GeodesicCell> landCells = geodesicGrid.cells.Where(c => c.IsLand()).ToList();
            
            if (landCells.Count == 0)
            {
                Debug.LogWarning("Aucune cellule de terre trouvée !");
                return Vector3.zero;
            }
            
            // Sélectionne une cellule aléatoire
            GeodesicCell randomCell = landCells[Random.Range(0, landCells.Count)];
            Vector3 spawnPosition = randomCell.GetSpawnPosition(geodesicGrid.planetRadius);
            
            if (showDebugInfo)
            {
                Debug.Log($"Position trouvée sur cellule {randomCell.id} (Type: {randomCell.cellType})");
            }
            
            return spawnPosition;
        }
        
        /// <summary>
        /// Place le personnage à une position spécifique
        /// </summary>
        private void PlaceCharacterAtPosition(Vector3 position)
        {
            // Supprime l'ancien personnage
            if (currentCharacter != null)
            {
                DestroyImmediate(currentCharacter.gameObject);
            }
            
            if (characterPrefab == null)
            {
                Debug.LogError("Prefab de personnage non assigné !");
                return;
            }
            
            // Instancie le personnage
            currentCharacter = Instantiate(characterPrefab, position, Quaternion.identity);
            currentCharacter.name = "Character";
            
            // Oriente le personnage vers le haut
            Vector3 directionFromCenter = -position.normalized;
            currentCharacter.rotation = Quaternion.FromToRotation(Vector3.up, directionFromCenter);
            
            // Ajuste la hauteur
            currentCharacter.position = position + Vector3.up * spawnHeight;
            
            lastSpawnPosition = position;
            
            if (showDebugInfo)
            {
                Debug.Log($"Personnage placé à: {position}");
                Debug.Log($"Orientation: {currentCharacter.rotation.eulerAngles}");
            }
        }
        
        /// <summary>
        /// Obtient la cellule la plus proche d'une position
        /// </summary>
        public GeodesicCell GetNearestCell(Vector3 position)
        {
            if (geodesicGrid == null) return null;
            
            GeodesicCell nearestCell = null;
            float nearestDistance = float.MaxValue;
            
            foreach (GeodesicCell cell in geodesicGrid.cells)
            {
                float distance = Vector3.Distance(position, cell.centerPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCell = cell;
                }
            }
            
            return nearestCell;
        }
        
        /// <summary>
        /// Obtient les informations de la cellule à une position
        /// </summary>
        public string GetCellInfo(Vector3 position)
        {
            GeodesicCell cell = GetNearestCell(position);
            if (cell == null) return "Cellule non trouvée";
            
            return cell.GetDebugInfo();
        }
        
        /// <summary>
        /// Test de placement
        /// </summary>
        [ContextMenu("Test Placement")]
        public void TestPlacement()
        {
            PlaceCharacterOnPlanet();
        }
        
        /// <summary>
        /// Test de position aléatoire
        /// </summary>
        [ContextMenu("Test Position Aléatoire")]
        public void TestRandomPosition()
        {
            if (geodesicGrid == null)
            {
                Debug.LogError("Grille géodésique non trouvée !");
                return;
            }
            
            List<GeodesicCell> landCells = geodesicGrid.cells.Where(c => c.IsLand()).ToList();
            if (landCells.Count == 0)
            {
                Debug.LogWarning("Aucune cellule de terre trouvée !");
                return;
            }
            
            GeodesicCell randomCell = landCells[Random.Range(0, landCells.Count)];
            Vector3 spawnPosition = randomCell.GetSpawnPosition(geodesicGrid.planetRadius);
            
            Debug.Log($"Position aléatoire: {spawnPosition}");
            Debug.Log($"Cellule: {randomCell.GetDebugInfo()}");
        }
        
        private void OnDrawGizmos()
        {
            if (!showSpawnGizmos || lastSpawnPosition == Vector3.zero) return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSpawnPosition, 0.5f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lastSpawnPosition, lastSpawnPosition + Vector3.up * 2f);
        }
    }
}
