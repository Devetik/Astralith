using UnityEngine;
using System.Collections;

/// <summary>
/// Version de debug pour identifier les problèmes de performance
/// </summary>
public class PlanetHexDebug : MonoBehaviour
{
    [Header("Configuration Debug")]
    public int frequency = 5; // Commence petit pour tester
    public float radius = 100f;
    public int seed = 12345;
    public bool showDebugInfo = true;
    public bool stepByStep = true; // Génération étape par étape

    [Header("Matériaux")]
    public Material planetMaterial;

    void Start()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== DÉBUT DEBUG PLANÈTE HEXAGONALE ===");
        }
        
        StartCoroutine(GeneratePlanetStepByStep());
    }

    /// <summary>
    /// Génération étape par étape pour identifier le problème
    /// </summary>
    IEnumerator GeneratePlanetStepByStep()
    {
        if (showDebugInfo)
        {
            Debug.Log("Étape 1: Création du GameObject...");
        }
        
        // Étape 1: Créer le GameObject
        GameObject hexPlanet = new GameObject("HexPlanet");
        hexPlanet.transform.position = Vector3.zero;
        
        if (stepByStep) yield return new WaitForSeconds(0.1f);

        if (showDebugInfo)
        {
            Debug.Log("Étape 2: Ajout des composants...");
        }
        
        // Étape 2: Ajouter les composants
        var hexWorld = hexPlanet.AddComponent<PlanetHexWorld>();
        var tileSelector = hexPlanet.AddComponent<PlanetTileSelector>();
        
        if (stepByStep) yield return new WaitForSeconds(0.1f);

        if (showDebugInfo)
        {
            Debug.Log("Étape 3: Configuration des paramètres...");
        }
        
        // Étape 3: Configurer les paramètres
        hexWorld.frequency = frequency;
        hexWorld.radius = radius;
        hexWorld.seed = seed;
        hexWorld.buildLatitudeDeg = 70f;
        hexWorld.excludePentagonsFromBuild = true;
        hexWorld.generateOnStart = false;
        hexWorld.drawGizmos = showDebugInfo;
        // hexWorld.showDebugInfo = showDebugInfo; // Cette propriété n'existe pas
        
        if (stepByStep) yield return new WaitForSeconds(0.1f);

        if (showDebugInfo)
        {
            Debug.Log("Étape 4: Configuration du sélecteur...");
        }
        
        // Étape 4: Configurer le sélecteur
        tileSelector.cam = Camera.main;
        tileSelector.highlightColor = Color.yellow;
        
        if (stepByStep) yield return new WaitForSeconds(0.1f);

        if (showDebugInfo)
        {
            Debug.Log("Étape 5: Assignation du matériau...");
        }
        
        // Étape 5: Assigner le matériau
        if (planetMaterial != null)
        {
            var renderer = hexPlanet.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = planetMaterial;
            }
        }
        
        if (stepByStep) yield return new WaitForSeconds(0.1f);

        if (showDebugInfo)
        {
            Debug.Log("Étape 6: Génération de la planète...");
        }
        
        // Étape 6: Générer la planète
        try
        {
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Planète générée avec succès: {hexWorld.cells.Count} cellules");
                int hexCount = hexWorld.cells.FindAll(c => !c.isPentagon).Count;
                int pentCount = hexWorld.cells.FindAll(c => c.isPentagon).Count;
                int buildableCount = hexWorld.cells.FindAll(c => c.canBuild).Count;
                
                Debug.Log($"- Hexagones: {hexCount}");
                Debug.Log($"- Pentagones: {pentCount}");
                Debug.Log($"- Constructibles: {buildableCount}");
                Debug.Log("=== DEBUG TERMINÉ ===");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la génération: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Test de génération simple
    /// </summary>
    [ContextMenu("Test Génération Simple")]
    public void TestSimpleGeneration()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST GÉNÉRATION SIMPLE ===");
        }

        // Crée un GameObject simple
        GameObject testPlanet = new GameObject("TestPlanet");
        testPlanet.transform.position = Vector3.zero;

        // Ajoute seulement PlanetHexWorld
        var hexWorld = testPlanet.AddComponent<PlanetHexWorld>();
        
        // Configure avec des paramètres très petits
        hexWorld.frequency = 3; // Très petit
        hexWorld.radius = 50f;   // Très petit
        hexWorld.seed = 12345;
        hexWorld.generateOnStart = false;
        // hexWorld.showDebugInfo = true; // Cette propriété n'existe pas
        hexWorld.drawGizmos = false; // Désactive les gizmos

        try
        {
            if (showDebugInfo)
            {
                Debug.Log("Tentative de génération...");
            }
            
            hexWorld.Generate();
            
            if (showDebugInfo)
            {
                Debug.Log($"Génération réussie: {hexWorld.cells.Count} cellules");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur: {e.Message}");
        }
    }

    /// <summary>
    /// Test de l'icosaèdre de base
    /// </summary>
    [ContextMenu("Test Icosaèdre de Base")]
    public void TestBasicIcosahedron()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== TEST ICOSAÈDRE DE BASE ===");
        }

        try
        {
            // Test de l'icosaèdre de base sans subdivision
            var tri = BuildIcosahedron();
            
            if (showDebugInfo)
            {
                Debug.Log($"Icosaèdre créé: {tri.vertices.Count} sommets, {tri.indices.Count/3} triangles");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur icosaèdre: {e.Message}");
        }
    }

    /// <summary>
    /// Construit un icosaèdre de base (copié de PlanetHexWorld)
    /// </summary>
    private TriMesh BuildIcosahedron()
    {
        TriMesh m = new TriMesh();
        float t = (1f + Mathf.Sqrt(5f)) * 0.5f;

        var verts = new System.Collections.Generic.List<Vector3> {
            new Vector3(-1,  t,  0), new Vector3( 1,  t,  0), new Vector3(-1, -t,  0), new Vector3( 1, -t,  0),
            new Vector3( 0, -1,  t), new Vector3( 0,  1,  t), new Vector3( 0, -1, -t), new Vector3( 0,  1, -t),
            new Vector3( t,  0, -1), new Vector3( t,  0,  1), new Vector3(-t,  0, -1), new Vector3(-t,  0,  1)
        };
        
        // Normalize to unit sphere
        for (int i=0;i<verts.Count;i++) verts[i] = verts[i].normalized;
        m.vertices.AddRange(verts);

        int[] tris = {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
            1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
            4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };
        m.indices.AddRange(tris);
        return m;
    }

    /// <summary>
    /// Structure pour le mesh triangulaire
    /// </summary>
    class TriMesh
    {
        public System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();
        public System.Collections.Generic.List<int> indices = new System.Collections.Generic.List<int>();
    }
}
