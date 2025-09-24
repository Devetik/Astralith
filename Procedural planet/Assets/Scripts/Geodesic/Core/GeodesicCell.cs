using UnityEngine;
using System.Collections.Generic;

namespace Geodesic
{
    /// <summary>
    /// Représente une cellule de la grille géodésique
    /// </summary>
    [System.Serializable]
    public class GeodesicCell
    {
        [Header("Identification")]
        public int id;
        public Vector3 centerPosition;
        public Vector3[] vertices;
        
        [Header("Propriétés")]
        public CellType cellType;
        public float altitude;
        public float temperature;
        public float humidity;
        
        [Header("Navigation")]
        public List<int> neighborIds;
        public bool isVisited;
        public int continentId;
        
        [Header("Debug")]
        public bool showDebugInfo;
        
        public enum CellType
        {
            Ocean,      // Océan profond
            Sea,        // Mer peu profonde
            Coast,      // Côte
            Land,       // Terre
            Mountain,   // Montagne
            Desert,     // Désert
            Forest,     // Forêt
            Tundra      // Toundra
        }
        
        public GeodesicCell(int id, Vector3 centerPosition, Vector3[] vertices)
        {
            this.id = id;
            this.centerPosition = centerPosition;
            this.vertices = vertices;
            this.neighborIds = new List<int>();
            this.cellType = CellType.Ocean;
            this.altitude = 0f;
            this.temperature = 20f;
            this.humidity = 0.5f;
            this.isVisited = false;
            this.continentId = -1;
            this.showDebugInfo = false;
        }
        
        /// <summary>
        /// Détermine le type de cellule basé sur l'altitude
        /// </summary>
        public void DetermineCellType()
        {
            if (altitude < -1000f)
            {
                cellType = CellType.Ocean;
            }
            else if (altitude < -500f)
            {
                cellType = CellType.Sea;
            }
            else if (altitude < 0f)
            {
                cellType = CellType.Coast;
            }
            else if (altitude < 500f)
            {
                cellType = CellType.Land;
            }
            else if (altitude < 1000f)
            {
                cellType = CellType.Mountain;
            }
            else
            {
                cellType = CellType.Mountain;
            }
        }
        
        /// <summary>
        /// Vérifie si la cellule est de la terre
        /// </summary>
        public bool IsLand()
        {
            return cellType == CellType.Land || 
                   cellType == CellType.Mountain || 
                   cellType == CellType.Desert || 
                   cellType == CellType.Forest || 
                   cellType == CellType.Tundra;
        }
        
        /// <summary>
        /// Vérifie si la cellule est de l'eau
        /// </summary>
        public bool IsWater()
        {
            return cellType == CellType.Ocean || 
                   cellType == CellType.Sea || 
                   cellType == CellType.Coast;
        }
        
        /// <summary>
        /// Obtient la couleur de debug pour la cellule
        /// </summary>
        public Color GetDebugColor()
        {
            switch (cellType)
            {
                case CellType.Ocean: return Color.blue;
                case CellType.Sea: return Color.cyan;
                case CellType.Coast: return Color.yellow;
                case CellType.Land: return Color.green;
                case CellType.Mountain: return Color.gray;
                case CellType.Desert: return Color.yellow;
                case CellType.Forest: return Color.green;
                case CellType.Tundra: return Color.white;
                default: return Color.magenta;
            }
        }
        
        /// <summary>
        /// Obtient une position de spawn valide sur la cellule
        /// </summary>
        public Vector3 GetSpawnPosition(float planetRadius)
        {
            if (!IsLand())
            {
                return centerPosition.normalized * planetRadius;
            }
            
            // Position sur la surface de la terre
            Vector3 surfacePosition = centerPosition.normalized * (planetRadius + altitude);
            
            if (showDebugInfo)
            {
                Debug.Log($"Cellule {id}: Position de spawn = {surfacePosition} (altitude: {altitude})");
            }
            
            return surfacePosition;
        }
        
        /// <summary>
        /// Obtient les informations de debug
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Cellule {id}: {cellType} (alt: {altitude:F1}m, temp: {temperature:F1}°C)";
        }
    }
}
