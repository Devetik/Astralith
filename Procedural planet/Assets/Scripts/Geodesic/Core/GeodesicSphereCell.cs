using UnityEngine;
using System.Collections.Generic;

namespace Geodesic
{
    /// <summary>
    /// Représente une cellule de la grille sphérique géodésique
    /// </summary>
    [System.Serializable]
    public class GeodesicSphereCell
    {
        [Header("Identification")]
        public int id;
        public Vector3 centerPosition;
        public Vector3[] vertices;
        public int sides; // Nombre de côtés (5 ou 6)
        public List<int> neighborIds; // IDs des cellules voisines
        
        [Header("Propriétés")]
        public CellType cellType;
        public bool isBuildable;
        public bool hasStructure;
        
        [Header("Génération Procédurale")]
        public float altitude;        // Altitude (0-1)
        public float temperature;     // Température (-1 à 1)
        public float humidity;        // Humidité (0-1)
        public float fertility;       // Fertilité (0-1)
        
        public enum CellType
        {
            Ocean,      // Océan profond
            Lake,       // Lac
            Coast,      // Côte
            Desert,     // Désert
            Grassland,  // Prairie
            Forest,     // Forêt
            Mountain,   // Montagne
            Tundra,     // Toundra
            Ice         // Glace
        }
        
        public GeodesicSphereCell(int id, Vector3 centerPosition, Vector3[] vertices)
        {
            this.id = id;
            this.centerPosition = centerPosition;
            this.vertices = vertices;
            this.sides = vertices.Length;
            this.neighborIds = new List<int>();
            this.cellType = CellType.Ocean;
            this.isBuildable = false;
            this.hasStructure = false;
            this.altitude = 0f;
            this.temperature = 0f;
            this.humidity = 0f;
            this.fertility = 0f;
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
            return cellType != CellType.Ocean && cellType != CellType.Lake;
        }
        
        /// <summary>
        /// Vérifie si la cellule est de l'eau
        /// </summary>
        public bool IsWater()
        {
            return cellType == CellType.Ocean || cellType == CellType.Lake;
        }
        
        /// <summary>
        /// Vérifie si la cellule est constructible
        /// </summary>
        public bool CanBuild()
        {
            return IsLand() && isBuildable && !hasStructure;
        }
        
        /// <summary>
        /// Obtient la couleur de debug
        /// </summary>
        public Color GetDebugColor()
        {
            switch (cellType)
            {
                case CellType.Ocean: return new Color(0f, 0.2f, 0.8f);      // Bleu océan
                case CellType.Lake: return new Color(0.2f, 0.4f, 1f);       // Bleu lac
                case CellType.Coast: return new Color(0.8f, 0.8f, 0.4f);    // Jaune sable
                case CellType.Desert: return new Color(0.9f, 0.7f, 0.3f);   // Jaune désert
                case CellType.Grassland: return new Color(0.4f, 0.8f, 0.2f); // Vert prairie
                case CellType.Forest: return new Color(0.2f, 0.6f, 0.2f);   // Vert forêt
                case CellType.Mountain: return new Color(0.5f, 0.4f, 0.3f);  // Brun montagne
                case CellType.Tundra: return new Color(0.6f, 0.7f, 0.8f);   // Gris toundra
                case CellType.Ice: return new Color(0.9f, 0.9f, 1f);        // Blanc glace
                default: return Color.magenta;
            }
        }
        
        /// <summary>
        /// Obtient les informations de debug
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Cellule {id}: {cellType} ({sides} côtés, buildable: {isBuildable})";
        }
        
        /// <summary>
        /// Obtient le nombre de voisins
        /// </summary>
        public int GetNeighborCount()
        {
            return neighborIds != null ? neighborIds.Count : 0;
        }
        
        /// <summary>
        /// Vérifie si une cellule est voisine
        /// </summary>
        public bool IsNeighbor(int cellId)
        {
            return neighborIds != null && neighborIds.Contains(cellId);
        }
    }
}
