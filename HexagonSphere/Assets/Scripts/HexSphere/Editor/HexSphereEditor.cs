using UnityEngine;
using UnityEditor;

namespace HexSphere.Editor
{
    /// <summary>
    /// Éditeur personnalisé pour HexSphereGenerator
    /// </summary>
    [CustomEditor(typeof(HexSphereGenerator))]
    public class HexSphereEditor : UnityEditor.Editor
    {
        private HexSphereGenerator generator;
        private bool showAdvancedOptions = false;
        
        private void OnEnable()
        {
            generator = (HexSphereGenerator)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // En-tête avec style
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== HexSphere Generator ===", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Paramètres principaux
            DrawMainParameters();
            EditorGUILayout.Space();
            
            // Options avancées
            DrawAdvancedOptions();
            EditorGUILayout.Space();
            
            // Boutons d'action
            DrawActionButtons();
            EditorGUILayout.Space();
            
            // Informations de debug
            DrawDebugInfo();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawMainParameters()
        {
            EditorGUILayout.LabelField("Paramètres Principaux", EditorStyles.boldLabel);
            
            generator.subdivisionLevel = EditorGUILayout.IntSlider("Niveau de Subdivision", generator.subdivisionLevel, 1, 5);
            generator.radius = EditorGUILayout.Slider("Rayon", generator.radius, 0.1f, 10f);
            generator.hexSize = EditorGUILayout.Slider("Taille des Hexagones", generator.hexSize, 0.1f, 2f);
            
            EditorGUILayout.Space();
            generator.hexMaterial = (Material)EditorGUILayout.ObjectField("Matériau", generator.hexMaterial, typeof(Material), false);
        }
        
        private void DrawAdvancedOptions()
        {
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Options Avancées");
            
            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Génération", EditorStyles.boldLabel);
                generator.generateOnStart = EditorGUILayout.Toggle("Générer au Start", generator.generateOnStart);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Élévation", EditorStyles.boldLabel);
                generator.useElevation = EditorGUILayout.Toggle("Utiliser Élévation", generator.useElevation);
                
                if (generator.useElevation)
                {
                    generator.elevationCurve = EditorGUILayout.CurveField("Courbe d'Élévation", generator.elevationCurve);
                    generator.maxElevation = EditorGUILayout.Slider("Élévation Max", generator.maxElevation, 0f, 1f);
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Couleurs", EditorStyles.boldLabel);
                generator.useGradient = EditorGUILayout.Toggle("Utiliser Gradient", generator.useGradient);
                
                if (generator.useGradient)
                {
                    generator.colorGradient = EditorGUILayout.GradientField("Gradient de Couleur", generator.colorGradient);
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
                generator.showGizmos = EditorGUILayout.Toggle("Afficher Gizmos", generator.showGizmos);
                generator.gizmoColor = EditorGUILayout.ColorField("Couleur Gizmos", generator.gizmoColor);
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Générer Sphère", GUILayout.Height(30)))
            {
                generator.GenerateHexSphere();
                EditorUtility.SetDirty(generator);
            }
            
            if (GUILayout.Button("Nettoyer", GUILayout.Height(30)))
            {
                generator.ClearHexSphere();
                EditorUtility.SetDirty(generator);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Régénérer"))
            {
                generator.ClearHexSphere();
                generator.GenerateHexSphere();
                EditorUtility.SetDirty(generator);
            }
            
            if (GUILayout.Button("Appliquer Matériau"))
            {
                if (generator.hexMaterial != null)
                {
                    MeshRenderer renderer = generator.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material = generator.hexMaterial;
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawDebugInfo()
        {
            if (generator.hexCells != null && generator.hexCells.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Informations", EditorStyles.boldLabel);
                
                EditorGUILayout.LabelField($"Nombre d'hexagones: {generator.hexCells.Count}");
                
                int visibleCells = 0;
                foreach (var cell in generator.hexCells)
                {
                    if (cell.isVisible) visibleCells++;
                }
                EditorGUILayout.LabelField($"Hexagones visibles: {visibleCells}");
                
                if (generator.hexMesh != null)
                {
                    EditorGUILayout.LabelField($"Vertices: {generator.hexMesh.vertexCount}");
                    EditorGUILayout.LabelField($"Triangles: {generator.hexMesh.triangles.Length / 3}");
                }
            }
        }
        
        private void OnSceneGUI()
        {
            if (generator == null || !generator.showGizmos) return;
            
            // Dessiner des informations supplémentaires dans la scène
            Handles.color = generator.gizmoColor;
            
            if (generator.hexCells != null)
            {
                foreach (HexCell cell in generator.hexCells)
                {
                    if (!cell.isVisible) continue;
                    
                    // Dessiner le centre de la cellule
                    Handles.DrawWireDisc(cell.center * generator.radius, cell.normal, 0.05f);
                    
                    // Dessiner la normale
                    Handles.DrawLine(cell.center * generator.radius, 
                        cell.center * generator.radius + cell.normal * 0.1f);
                }
            }
        }
    }
    
    /// <summary>
    /// Éditeur personnalisé pour HexSphereManager
    /// </summary>
    [CustomEditor(typeof(HexSphereManager))]
    public class HexSphereManagerEditor : UnityEditor.Editor
    {
        private HexSphereManager manager;
        
        private void OnEnable()
        {
            manager = (HexSphereManager)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== HexSphere Manager ===", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Paramètres de base
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            // Boutons de contrôle
            DrawControlButtons();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawControlButtons()
        {
            EditorGUILayout.LabelField("Contrôles", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Régénérer Sphère"))
            {
                manager.RegenerateSphere();
            }
            
            if (GUILayout.Button("Toggle Rotation Auto"))
            {
                manager.ToggleAutoRotation();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Toggle Gizmos"))
            {
                manager.ToggleGizmos();
            }
            
            if (GUILayout.Button("Nettoyer Sélection"))
            {
                manager.ClearHighlights();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
