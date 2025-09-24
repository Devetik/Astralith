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
        
        public enum CellType
        {
            Water,  // Eau
            Land    // Terre
        }
        
        public GeodesicSphereCell(int id, Vector3 centerPosition, Vector3[] vertices)
        {
            this.id = id;
            this.centerPosition = centerPosition;
            this.vertices = vertices;
            this.sides = vertices.Length;
            this.neighborIds = new List<int>();
            this.cellType = CellType.Water;
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
            return cellType == CellType.Land;
        }
        
        /// <summary>
        /// Vérifie si la cellule est de l'eau
        /// </summary>
        public bool IsWater()
        {
            return cellType == CellType.Water;
        }
        
        /// <summary>
        /// Obtient la couleur de debug
        /// </summary>
        public Color GetDebugColor()
        {
            switch (cellType)
            {
                case CellType.Water: return Color.blue;
                case CellType.Land: return Color.green;
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
