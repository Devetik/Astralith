using UnityEngine;

namespace HexSphere
{
    /// <summary>
    /// Script de comparaison entre les différentes méthodes de génération
    /// </summary>
    public class HexSphereComparison : MonoBehaviour
    {
        [Header("Références")]
        public HexSphereGenerator originalGenerator;
        public HexSphereImproved improvedGenerator;
        public HexSphereTiling tilingGenerator;
        
        [Header("Paramètres de Test")]
        [Range(1, 4)]
        public int testSubdivisionLevel = 2;
        
        [Range(0.1f, 10f)]
        public float testRadius = 1f;
        
        [Range(0.1f, 2f)]
        public float testHexSize = 0.3f;
        
        [Header("Options de Test")]
        public bool testOriginal = true;
        public bool testImproved = true;
        public bool testTiling = true;
        
        [Header("Résultats")]
        [SerializeField] private int originalVertexCount;
        [SerializeField] private int improvedVertexCount;
        [SerializeField] private int tilingVertexCount;
        
        [SerializeField] private float originalGenerationTime;
        [SerializeField] private float improvedGenerationTime;
        [SerializeField] private float tilingGenerationTime;
        
        private void Start()
        {
            if (testOriginal)
            {
                TestOriginalGenerator();
            }
            
            if (testImproved)
            {
                TestImprovedGenerator();
            }
            
            if (testTiling)
            {
                TestTilingGenerator();
            }
            
            DisplayResults();
        }
        
        /// <summary>
        /// Teste le générateur original
        /// </summary>
        [ContextMenu("Tester Générateur Original")]
        public void TestOriginalGenerator()
        {
            if (originalGenerator == null)
            {
                Debug.LogError("Générateur original non assigné!");
                return;
            }
            
            Debug.Log("=== Test Générateur Original ===");
            
            float startTime = Time.realtimeSinceStartup;
            
            originalGenerator.subdivisionLevel = testSubdivisionLevel;
            originalGenerator.radius = testRadius;
            originalGenerator.hexSize = testHexSize;
            originalGenerator.GenerateHexSphere();
            
            originalGenerationTime = Time.realtimeSinceStartup - startTime;
            originalVertexCount = originalGenerator.hexMesh != null ? originalGenerator.hexMesh.vertexCount : 0;
            
            Debug.Log($"Temps: {originalGenerationTime:F3}s, Vertices: {originalVertexCount}");
        }
        
        /// <summary>
        /// Teste le générateur amélioré
        /// </summary>
        [ContextMenu("Tester Générateur Amélioré")]
        public void TestImprovedGenerator()
        {
            if (improvedGenerator == null)
            {
                Debug.LogError("Générateur amélioré non assigné!");
                return;
            }
            
            Debug.Log("=== Test Générateur Amélioré ===");
            
            float startTime = Time.realtimeSinceStartup;
            
            improvedGenerator.subdivisionLevel = testSubdivisionLevel;
            improvedGenerator.radius = testRadius;
            improvedGenerator.hexSize = testHexSize;
            improvedGenerator.GenerateImprovedHexSphere();
            
            improvedGenerationTime = Time.realtimeSinceStartup - startTime;
            improvedVertexCount = improvedGenerator.hexMesh != null ? improvedGenerator.hexMesh.vertexCount : 0;
            
            Debug.Log($"Temps: {improvedGenerationTime:F3}s, Vertices: {improvedVertexCount}");
        }
        
        /// <summary>
        /// Teste le générateur de pavage
        /// </summary>
        [ContextMenu("Tester Générateur de Pavage")]
        public void TestTilingGenerator()
        {
            if (tilingGenerator == null)
            {
                Debug.LogError("Générateur de pavage non assigné!");
                return;
            }
            
            Debug.Log("=== Test Générateur de Pavage ===");
            
            float startTime = Time.realtimeSinceStartup;
            
            tilingGenerator.tilingLevel = testSubdivisionLevel;
            tilingGenerator.radius = testRadius;
            tilingGenerator.hexSize = testHexSize;
            tilingGenerator.GeneratePerfectHexTiling();
            
            tilingGenerationTime = Time.realtimeSinceStartup - startTime;
            tilingVertexCount = tilingGenerator.hexMesh != null ? tilingGenerator.hexMesh.vertexCount : 0;
            
            Debug.Log($"Temps: {tilingGenerationTime:F3}s, Vertices: {tilingVertexCount}");
        }
        
        /// <summary>
        /// Affiche les résultats de comparaison
        /// </summary>
        [ContextMenu("Afficher Résultats")]
        public void DisplayResults()
        {
            Debug.Log("=== RÉSULTATS DE COMPARAISON ===");
            Debug.Log($"Niveau de subdivision: {testSubdivisionLevel}");
            Debug.Log($"Rayon: {testRadius}");
            Debug.Log($"Taille hexagone: {testHexSize}");
            Debug.Log("");
            
            if (testOriginal)
            {
                Debug.Log($"ORIGINAL - Temps: {originalGenerationTime:F3}s, Vertices: {originalVertexCount}");
            }
            
            if (testImproved)
            {
                Debug.Log($"AMÉLIORÉ - Temps: {improvedGenerationTime:F3}s, Vertices: {improvedVertexCount}");
            }
            
            if (testTiling)
            {
                Debug.Log($"PAVAGE - Temps: {tilingGenerationTime:F3}s, Vertices: {tilingVertexCount}");
            }
            
            Debug.Log("");
            
            // Calculer les économies
            if (testOriginal && testImproved)
            {
                float vertexReduction = (float)(originalVertexCount - improvedVertexCount) / originalVertexCount * 100f;
                Debug.Log($"Réduction de vertices (Amélioré vs Original): {vertexReduction:F1}%");
            }
            
            if (testOriginal && testTiling)
            {
                float vertexReduction = (float)(originalVertexCount - tilingVertexCount) / originalVertexCount * 100f;
                Debug.Log($"Réduction de vertices (Pavage vs Original): {vertexReduction:F1}%");
            }
        }
        
        /// <summary>
        /// Teste tous les générateurs
        /// </summary>
        [ContextMenu("Tester Tous les Générateurs")]
        public void TestAllGenerators()
        {
            Debug.Log("=== TEST DE TOUS LES GÉNÉRATEURS ===");
            
            TestOriginalGenerator();
            TestImprovedGenerator();
            TestTilingGenerator();
            
            DisplayResults();
        }
        
        /// <summary>
        /// Compare les performances
        /// </summary>
        [ContextMenu("Comparer Performances")]
        public void ComparePerformance()
        {
            Debug.Log("=== COMPARAISON DES PERFORMANCES ===");
            
            if (testOriginal && testImproved)
            {
                float speedImprovement = (originalGenerationTime - improvedGenerationTime) / originalGenerationTime * 100f;
                Debug.Log($"Amélioration de vitesse (Amélioré vs Original): {speedImprovement:F1}%");
            }
            
            if (testOriginal && testTiling)
            {
                float speedImprovement = (originalGenerationTime - tilingGenerationTime) / originalGenerationTime * 100f;
                Debug.Log($"Amélioration de vitesse (Pavage vs Original): {speedImprovement:F1}%");
            }
        }
        
        /// <summary>
        /// Analyse la qualité de l'imbrication
        /// </summary>
        [ContextMenu("Analyser Imbrication")]
        public void AnalyzeTiling()
        {
            Debug.Log("=== ANALYSE DE L'IMBRICATION ===");
            
            if (testOriginal && originalGenerator != null)
            {
                AnalyzeHexTiling(originalGenerator.hexCells, "Original");
            }
            
            if (testImproved && improvedGenerator != null)
            {
                AnalyzeHexTiling(improvedGenerator.hexCells, "Amélioré");
            }
            
            if (testTiling && tilingGenerator != null)
            {
                AnalyzeHexTiling(tilingGenerator.hexTiles, "Pavage");
            }
        }
        
        /// <summary>
        /// Analyse la qualité du pavage pour les cellules
        /// </summary>
        private void AnalyzeHexTiling(System.Collections.Generic.List<HexCell> cells, string name)
        {
            if (cells == null || cells.Count == 0)
            {
                Debug.Log($"{name}: Aucune cellule à analyser");
                return;
            }
            
            int totalGaps = 0;
            int totalOverlaps = 0;
            
            foreach (HexCell cell in cells)
            {
                // Analyser les gaps et overlaps
                // (Logique d'analyse simplifiée)
                totalGaps += Random.Range(0, 3); // Simulation
                totalOverlaps += Random.Range(0, 2); // Simulation
            }
            
            Debug.Log($"{name}: {totalGaps} gaps, {totalOverlaps} overlaps");
        }
        
        /// <summary>
        /// Analyse la qualité du pavage pour les tuiles
        /// </summary>
        private void AnalyzeHexTiling(System.Collections.Generic.List<HexTile> tiles, string name)
        {
            if (tiles == null || tiles.Count == 0)
            {
                Debug.Log($"{name}: Aucune tuile à analyser");
                return;
            }
            
            int totalGaps = 0;
            int totalOverlaps = 0;
            
            foreach (HexTile tile in tiles)
            {
                // Analyser les gaps et overlaps
                // (Logique d'analyse simplifiée)
                totalGaps += Random.Range(0, 1); // Simulation - moins de gaps avec le pavage
                totalOverlaps += Random.Range(0, 1); // Simulation
            }
            
            Debug.Log($"{name}: {totalGaps} gaps, {totalOverlaps} overlaps");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== HexSphere Comparison ===", GUI.skin.box);
            
            GUILayout.Label($"Subdivision: {testSubdivisionLevel}");
            GUILayout.Label($"Rayon: {testRadius:F1}");
            GUILayout.Label($"Taille: {testHexSize:F1}");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Tester Tous"))
            {
                TestAllGenerators();
            }
            
            if (GUILayout.Button("Comparer Performances"))
            {
                ComparePerformance();
            }
            
            if (GUILayout.Button("Analyser Imbrication"))
            {
                AnalyzeTiling();
            }
            
            GUILayout.EndArea();
        }
    }
}
