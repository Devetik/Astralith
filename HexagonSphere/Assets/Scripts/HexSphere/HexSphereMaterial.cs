using UnityEngine;

namespace HexSphere
{
    /// <summary>
    /// Gestionnaire de matériaux pour la sphère hexagonale
    /// </summary>
    [CreateAssetMenu(fileName = "HexSphereMaterial", menuName = "HexSphere/Material Settings")]
    public class HexSphereMaterial : ScriptableObject
    {
        [Header("Matériau de Base")]
        public Material baseMaterial;
        
        [Header("Couleurs")]
        public Color baseColor = Color.white;
        public Color highlightColor = Color.yellow;
        public Color selectedColor = Color.blue;
        public Color hoverColor = Color.red;
        
        [Header("Propriétés du Matériau")]
        [Range(0f, 1f)]
        public float metallic = 0f;
        
        [Range(0f, 1f)]
        public float smoothness = 0.5f;
        
        [Range(0f, 1f)]
        public float emission = 0f;
        
        [Header("Textures")]
        public Texture2D mainTexture;
        public Texture2D normalMap;
        public Texture2D emissionMap;
        
        [Header("Paramètres UV")]
        public Vector2 textureScale = Vector2.one;
        public Vector2 textureOffset = Vector2.zero;
        
        /// <summary>
        /// Crée un matériau dynamique basé sur les paramètres
        /// </summary>
        public Material CreateDynamicMaterial()
        {
            if (baseMaterial == null)
            {
                // Créer un matériau URP par défaut
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader == null)
                {
                    urpShader = Shader.Find("Standard");
                }
                
                baseMaterial = new Material(urpShader);
            }
            
            Material dynamicMaterial = new Material(baseMaterial);
            
            // Appliquer les propriétés
            dynamicMaterial.color = baseColor;
            dynamicMaterial.SetFloat("_Metallic", metallic);
            dynamicMaterial.SetFloat("_Smoothness", smoothness);
            
            if (mainTexture != null)
            {
                dynamicMaterial.mainTexture = mainTexture;
                dynamicMaterial.SetTextureScale("_MainTex", textureScale);
                dynamicMaterial.SetTextureOffset("_MainTex", textureOffset);
            }
            
            if (normalMap != null)
            {
                dynamicMaterial.SetTexture("_BumpMap", normalMap);
            }
            
            if (emissionMap != null)
            {
                dynamicMaterial.SetTexture("_EmissionMap", emissionMap);
                dynamicMaterial.SetColor("_EmissionColor", Color.white * emission);
                dynamicMaterial.EnableKeyword("_EMISSION");
            }
            
            return dynamicMaterial;
        }
        
        /// <summary>
        /// Applique une couleur à un matériau existant
        /// </summary>
        public void ApplyColorToMaterial(Material material, Color color)
        {
            if (material != null)
            {
                material.color = color;
            }
        }
        
        /// <summary>
        /// Obtient la couleur appropriée selon l'état de la cellule
        /// </summary>
        public Color GetColorForCellState(HexCellState state)
        {
            switch (state)
            {
                case HexCellState.Normal:
                    return baseColor;
                case HexCellState.Highlighted:
                    return highlightColor;
                case HexCellState.Selected:
                    return selectedColor;
                case HexCellState.Hovered:
                    return hoverColor;
                default:
                    return baseColor;
            }
        }
    }
    
    /// <summary>
    /// États possibles d'une cellule hexagonale
    /// </summary>
    public enum HexCellState
    {
        Normal,
        Highlighted,
        Selected,
        Hovered
    }
}
