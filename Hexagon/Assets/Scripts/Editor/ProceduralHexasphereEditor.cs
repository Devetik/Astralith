using UnityEngine;
using UnityEditor;
using HexasphereProcedural;

namespace ProceduralHexasphereEditor {

    /// <summary>
    /// Éditeur personnalisé pour le système de terrain procédural
    /// </summary>
    [CustomEditor(typeof(ProceduralHexasphere))]
    public class ProceduralHexasphereEditor : Editor {
        
        private ProceduralHexasphere hexasphere;
        private bool showAdvancedSettings = false;
        
        void OnEnable() {
            hexasphere = (ProceduralHexasphere)target;
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Système de Terrain Procédural", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Configuration de base
            EditorGUILayout.LabelField("Configuration de base", EditorStyles.boldLabel);
            hexasphere.numDivisions = EditorGUILayout.IntSlider("Divisions", hexasphere.numDivisions, 1, 20);
            hexasphere.radius = EditorGUILayout.FloatField("Rayon", hexasphere.radius);
            
            EditorGUILayout.Space();
            
            // Configuration du terrain
            EditorGUILayout.LabelField("Configuration du terrain", EditorStyles.boldLabel);
            hexasphere.noiseScale = EditorGUILayout.Slider("Échelle du bruit", hexasphere.noiseScale, 0.1f, 5f);
            hexasphere.heightAmplitude = EditorGUILayout.Slider("Amplitude de hauteur", hexasphere.heightAmplitude, 0.01f, 1f);
            hexasphere.noiseOctaves = EditorGUILayout.IntSlider("Octaves", hexasphere.noiseOctaves, 1, 8);
            hexasphere.noisePersistence = EditorGUILayout.Slider("Persistance", hexasphere.noisePersistence, 0.1f, 1f);
            hexasphere.noiseLacunarity = EditorGUILayout.Slider("Lacunarité", hexasphere.noiseLacunarity, 1f, 4f);
            
            EditorGUILayout.Space();
            
            // Seuils de terrain
            EditorGUILayout.LabelField("Seuils de terrain", EditorStyles.boldLabel);
            hexasphere.waterLevel = EditorGUILayout.Slider("Niveau d'eau", hexasphere.waterLevel, 0f, 0.5f);
            hexasphere.mountainLevel = EditorGUILayout.Slider("Niveau montagne", hexasphere.mountainLevel, 0.1f, 1f);
            
            EditorGUILayout.Space();
            
            // Matériaux
            EditorGUILayout.LabelField("Matériaux", EditorStyles.boldLabel);
            hexasphere.waterMaterial = (Material)EditorGUILayout.ObjectField("Matériau eau", hexasphere.waterMaterial, typeof(Material), false);
            hexasphere.landMaterial = (Material)EditorGUILayout.ObjectField("Matériau terre", hexasphere.landMaterial, typeof(Material), false);
            hexasphere.mountainMaterial = (Material)EditorGUILayout.ObjectField("Matériau montagne", hexasphere.mountainMaterial, typeof(Material), false);
            
            EditorGUILayout.Space();
            
            // Paramètres avancés
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Paramètres avancés");
            if (showAdvancedSettings) {
                EditorGUI.indentLevel++;
                hexasphere.showDebugInfo = EditorGUILayout.Toggle("Afficher debug", hexasphere.showDebugInfo);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Boutons d'action
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Générer")) {
                hexasphere.GenerateHexasphere();
            }
            if (GUILayout.Button("Régénérer")) {
                hexasphere.RegenerateTerrain();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Couleurs aléatoires")) {
                ApplyRandomColors();
            }
            if (GUILayout.Button("Couleurs par type")) {
                ApplyTerrainColors();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Informations
            if (hexasphere.points != null && hexasphere.tiles != null) {
                EditorGUILayout.LabelField("Informations", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Points : {hexasphere.points.Length}");
                EditorGUILayout.LabelField($"Tiles : {hexasphere.tiles.Length}");
                
                // Statistiques de hauteur
                if (hexasphere.points.Length > 0) {
                    float minHeight = float.MaxValue;
                    float maxHeight = float.MinValue;
                    float totalHeight = 0f;
                    
                    foreach (ProceduralPoint point in hexasphere.points) {
                        minHeight = Mathf.Min(minHeight, point.height);
                        maxHeight = Mathf.Max(maxHeight, point.height);
                        totalHeight += point.height;
                    }
                    
                    float avgHeight = totalHeight / hexasphere.points.Length;
                    
                    EditorGUILayout.LabelField($"Hauteur min : {minHeight:F3}");
                    EditorGUILayout.LabelField($"Hauteur max : {maxHeight:F3}");
                    EditorGUILayout.LabelField($"Hauteur moyenne : {avgHeight:F3}");
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        void ApplyRandomColors() {
            if (hexasphere.tiles == null) return;
            
            Color[] colors = { Color.blue, Color.green, Color.yellow, Color.red, Color.cyan, Color.magenta };
            
            foreach (ProceduralTile tile in hexasphere.tiles) {
                Color randomColor = colors[Random.Range(0, colors.Length)];
                // Ici on pourrait appliquer la couleur au tile
                Debug.Log($"Tile {tile.index} : {randomColor}");
            }
        }
        
        void ApplyTerrainColors() {
            if (hexasphere.tiles == null) return;
            
            int waterCount = 0;
            int landCount = 0;
            int mountainCount = 0;
            
            foreach (ProceduralTile tile in hexasphere.tiles) {
                // Déterminer le type de terrain basé sur la hauteur moyenne des vertices
                float averageHeight = 0f;
                foreach (ProceduralPoint vertex in tile.vertices) {
                    averageHeight += vertex.height;
                }
                averageHeight /= tile.vertices.Length;
                
                TerrainType terrainType;
                if (averageHeight < hexasphere.waterLevel) {
                    terrainType = TerrainType.Water;
                    waterCount++;
                } else if (averageHeight > hexasphere.mountainLevel) {
                    terrainType = TerrainType.Mountain;
                    mountainCount++;
                } else {
                    terrainType = TerrainType.Land;
                    landCount++;
                }
                
                Debug.Log($"Tile {tile.index} : {terrainType} (hauteur: {averageHeight:F2})");
            }
            
            Debug.Log($"Répartition : Eau={waterCount}, Terre={landCount}, Montagne={mountainCount}");
        }
    }
}
