using UnityEngine;
using UnityEditor;
using HexasphereProcedural;

[CustomEditor(typeof(Forest))]
public class ForestEditor : Editor {
    
    public override void OnInspectorGUI() {
        Forest forest = (Forest)target;
        
        // Dessiner l'inspecteur par défaut
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎮 Actions Rapides", EditorStyles.boldLabel);
        
        // Boutons pour les actions
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🌲 Regénérer les Arbres", GUILayout.Height(30))) {
            forest.RegenerateForest();
        }
        
        if (GUILayout.Button("🗑️ Effacer les Arbres", GUILayout.Height(30))) {
            forest.ClearForestButton();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔄 Effacer et Regénérer", GUILayout.Height(30))) {
            forest.ClearAndRegenerate();
        }
        
        if (GUILayout.Button("📊 Statistiques", GUILayout.Height(30))) {
            ShowForestStats(forest);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Informations de debug
        if (forest.showDebugInfo) {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("📈 Informations Debug", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Arbres générés: {forest.spawnedTrees?.Count ?? 0}");
            EditorGUILayout.LabelField($"Positions d'arbres: {forest.treePositions?.Count ?? 0}");
        }
    }
    
    private void ShowForestStats(Forest forest) {
        string message = $"🌲 Statistiques de la Forêt:\n\n";
        message += $"• Nombre d'arbres: {forest.spawnedTrees?.Count ?? 0}\n";
        message += $"• Positions enregistrées: {forest.treePositions?.Count ?? 0}\n";
        message += $"• Rotation enregistrées: {forest.treeRotations?.Count ?? 0}\n";
        message += $"• Seuil de patch: {forest.patchThreshold:F2}\n";
        message += $"• Intensité du bruit: {forest.noiseIntensity:F2}\n";
        message += $"• Octaves de bruit: {forest.noiseOctaves}\n";
        
        EditorUtility.DisplayDialog("📊 Statistiques de la Forêt", message, "OK");
    }
}
