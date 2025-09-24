using UnityEngine;
using System.Collections.Generic;

namespace Geodesic
{
    /// <summary>
    /// Représente une cellule de la grille géodésique dual
    /// </summary>
    [System.Serializable]
    public class GeodesicDualCell
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
        
        [Header("Construction")]
        public bool isBuildable;
        public bool hasStructure;
        
        public enum CellType
        {
            Ocean,      // Océan profond
            Lake,       // Lac
            Clearing,   // Clairière (constructible)
            Forest,     // Forêt
            Mountain,   // Montagne
            Desert,     // Désert
            Tundra,     // Toundra
            Coast       // Côte
        }
        
        public GeodesicDualCell(int id, Vector3 centerPosition, Vector3[] vertices)
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
            this.isBuildable = false;
            this.hasStructure = false;
        }
        
        /// <summary>
        /// Vérifie si la cellule est constructible
        /// </summary>
        public bool IsBuildable()
        {
            return isBuildable && !hasStructure;
        }
        
        /// <summary>
        /// Vérifie si la cellule est de la terre
        /// </summary>
        public bool IsLand()
        {
            return cellType == CellType.Clearing || 
                   cellType == CellType.Forest || 
                   cellType == CellType.Mountain || 
                   cellType == CellType.Desert || 
                   cellType == CellType.Tundra;
        }
        
        /// <summary>
        /// Vérifie si la cellule est de l'eau
        /// </summary>
        public bool IsWater()
        {
            return cellType == CellType.Ocean || 
                   cellType == CellType.Lake || 
                   cellType == CellType.Coast;
        }
        
        /// <summary>
        /// Obtient la couleur de debug
        /// </summary>
        public Color GetDebugColor()
        {
            switch (cellType)
            {
                case CellType.Ocean: return Color.blue;
                case CellType.Lake: return Color.cyan;
                case CellType.Clearing: return Color.green;
                case CellType.Forest: return Color.green * 0.7f;
                case CellType.Mountain: return Color.gray;
                case CellType.Desert: return Color.yellow;
                case CellType.Tundra: return Color.white;
                case CellType.Coast: return Color.yellow * 0.8f;
                default: return Color.magenta;
            }
        }
        
        /// <summary>
        /// Obtient les informations de debug
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Cellule {id}: {cellType} (alt: {altitude:F1}m, buildable: {isBuildable})";
        }
    }
}
