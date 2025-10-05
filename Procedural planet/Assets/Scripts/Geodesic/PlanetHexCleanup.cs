using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script de nettoyage complet pour résoudre les erreurs de ressources
/// </summary>
public class PlanetHexCleanup : MonoBehaviour
{
    [Header("Nettoyage Complet")]
    public bool autoCleanupOnStart = true;
    public bool showDebugInfo = true;

    void Start()
    {
        if (autoCleanupOnStart)
        {
            PerformCompleteCleanup();
        }
    }

    /// <summary>
    /// Nettoyage complet de toutes les ressources
    /// </summary>
    [ContextMenu("Nettoyage Complet")]
    public void PerformCompleteCleanup()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== NETTOYAGE COMPLET DES RESSOURCES ===");
        }

        try
        {
            // 1. Nettoie tous les GameObjects avec des meshes
            CleanupAllMeshObjects();

            // 2. Nettoie tous les meshes orphelins
            CleanupOrphanMeshes();

            // 3. Nettoie tous les matériaux temporaires
            CleanupTemporaryMaterials();

            // 4. Force le garbage collection
            ForceGarbageCollection();

            // 5. Nettoie les ressources Unity
            CleanupUnityResources();

            if (showDebugInfo)
            {
                Debug.Log("✅ Nettoyage complet terminé !");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors du nettoyage: {e.Message}");
        }
    }

    /// <summary>
    /// Nettoie tous les GameObjects avec des meshes
    /// </summary>
    private void CleanupAllMeshObjects()
    {
        // Trouve tous les GameObjects avec des meshes
        MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
        
        if (showDebugInfo)
        {
            Debug.Log($"Nettoyage de {meshFilters.Length} meshes...");
        }

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter != null && meshFilter.mesh != null)
            {
                // Nettoie le mesh
                if (Application.isPlaying)
                {
                    DestroyImmediate(meshFilter.mesh);
                }
                else
                {
                    DestroyImmediate(meshFilter.mesh);
                }
            }
        }

        // Trouve tous les GameObjects de tuiles
        GameObject[] tileObjects = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in tileObjects)
        {
            if (obj.name.StartsWith("HexTile_"))
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }
    }

    /// <summary>
    /// Nettoie tous les meshes orphelins
    /// </summary>
    private void CleanupOrphanMeshes()
    {
        // Trouve tous les meshes dans la scène
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();
        
        foreach (MeshRenderer renderer in meshRenderers)
        {
            if (renderer != null && renderer.materials != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat != null && mat.name.Contains("(Instance)"))
                    {
                        if (Application.isPlaying)
                        {
                            DestroyImmediate(mat);
                        }
                        else
                        {
                            DestroyImmediate(mat);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Nettoie tous les matériaux temporaires
    /// </summary>
    private void CleanupTemporaryMaterials()
    {
        // Trouve tous les matériaux temporaires
        Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
        
        foreach (Material mat in materials)
        {
            if (mat != null && mat.name.Contains("(Instance)"))
            {
                if (Application.isPlaying)
                {
                    DestroyImmediate(mat);
                }
                else
                {
                    DestroyImmediate(mat);
                }
            }
        }
    }

    /// <summary>
    /// Force le garbage collection
    /// </summary>
    private void ForceGarbageCollection()
    {
        // Force le garbage collection
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        if (showDebugInfo)
        {
            Debug.Log("Garbage collection forcé");
        }
    }

    /// <summary>
    /// Nettoie les ressources Unity
    /// </summary>
    private void CleanupUnityResources()
    {
        // Nettoie les ressources non utilisées
        Resources.UnloadUnusedAssets();

        // Nettoie le cache des assets
        if (Application.isPlaying)
        {
            Resources.UnloadUnusedAssets();
        }

        if (showDebugInfo)
        {
            Debug.Log("Ressources Unity nettoyées");
        }
    }

    /// <summary>
    /// Nettoie spécifiquement les tuiles hexagonales
    /// </summary>
    [ContextMenu("Nettoyer Tuiles Hexagonales")]
    public void CleanupHexTiles()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== NETTOYAGE TUILES HEXAGONALES ===");
        }

        try
        {
            // Trouve tous les composants de tuiles
            PlanetHexTilesWithDistanceControl[] tileComponents = FindObjectsOfType<PlanetHexTilesWithDistanceControl>();
            
            foreach (PlanetHexTilesWithDistanceControl tileComponent in tileComponents)
            {
                if (tileComponent != null)
                {
                    tileComponent.ClearTiles();
                }
            }

            // Nettoie les GameObjects de tuiles
            GameObject[] hexTiles = GameObject.FindGameObjectsWithTag("Untagged");
            foreach (GameObject obj in hexTiles)
            {
                if (obj.name.StartsWith("HexTile_") || obj.name.Contains("HexTiles"))
                {
                    if (Application.isPlaying)
                    {
                        Destroy(obj);
                    }
                    else
                    {
                        DestroyImmediate(obj);
                    }
                }
            }

            // Force le nettoyage
            ForceGarbageCollection();
            CleanupUnityResources();

            if (showDebugInfo)
            {
                Debug.Log("✅ Tuiles hexagonales nettoyées !");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors du nettoyage des tuiles: {e.Message}");
        }
    }

    /// <summary>
    /// Nettoie les composants de planète
    /// </summary>
    [ContextMenu("Nettoyer Composants Planète")]
    public void CleanupPlanetComponents()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== NETTOYAGE COMPOSANTS PLANÈTE ===");
        }

        try
        {
            // Trouve tous les composants de planète
            PlanetHexWorld[] hexWorlds = FindObjectsOfType<PlanetHexWorld>();
            foreach (PlanetHexWorld hexWorld in hexWorlds)
            {
                if (hexWorld != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(hexWorld.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(hexWorld.gameObject);
                    }
                }
            }

            // Nettoie les sélecteurs
            PlanetHexTileSelector[] selectors = FindObjectsOfType<PlanetHexTileSelector>();
            foreach (PlanetHexTileSelector selector in selectors)
            {
                if (selector != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(selector);
                    }
                    else
                    {
                        DestroyImmediate(selector);
                    }
                }
            }

            // Force le nettoyage
            ForceGarbageCollection();
            CleanupUnityResources();

            if (showDebugInfo)
            {
                Debug.Log("✅ Composants planète nettoyés !");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors du nettoyage des composants: {e.Message}");
        }
    }

    /// <summary>
    /// Nettoyage d'urgence - nettoie tout
    /// </summary>
    [ContextMenu("Nettoyage d'Urgence")]
    public void EmergencyCleanup()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== NETTOYAGE D'URGENCE ===");
        }

        try
        {
            // Nettoie tout
            CleanupAllMeshObjects();
            CleanupOrphanMeshes();
            CleanupTemporaryMaterials();
            CleanupHexTiles();
            CleanupPlanetComponents();
            
            // Force le nettoyage multiple
            for (int i = 0; i < 3; i++)
            {
                ForceGarbageCollection();
                CleanupUnityResources();
            }

            if (showDebugInfo)
            {
                Debug.Log("✅ Nettoyage d'urgence terminé !");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors du nettoyage d'urgence: {e.Message}");
        }
    }
}
