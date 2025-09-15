using UnityEngine;
using FishNet.Managing;

/// <summary>
/// Script de diagnostic rapide pour vérifier l'état de la scène
/// </summary>
public class SceneDiagnostic : MonoBehaviour
{
    [ContextMenu("Diagnostic Complet de la Scène")]
    public void FullSceneDiagnostic()
    {
        Debug.Log("=== DIAGNOSTIC COMPLET DE LA SCÈNE ===");
        
        // Vérifie tous les GameObjects
        var allObjects = FindObjectsOfType<GameObject>();
        Debug.Log($"Nombre total de GameObjects dans la scène: {allObjects.Length}");
        
        foreach (var obj in allObjects)
        {
            Debug.Log($"GameObject: {obj.name} - Active: {obj.activeInHierarchy}");
        }
        
        // Vérifie les composants spécifiques
        var planetGenerator = FindObjectOfType<PlanetGenerator>();
        var planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
        var planetSaveManager = FindObjectOfType<PlanetSaveManager>();
        var simplePlanetUI = FindObjectOfType<SimplePlanetUI>();
        var planetNetworkManager = FindObjectOfType<PlanetNetworkManager>();
        var networkManager = FindObjectOfType<NetworkManager>();
        
        Debug.Log("=== COMPOSANTS TROUVÉS ===");
        Debug.Log($"PlanetGenerator: {planetGenerator != null} {(planetGenerator != null ? $"sur {planetGenerator.gameObject.name}" : "")}");
        Debug.Log($"PlanetGeneratorNetworked: {planetGeneratorNetworked != null} {(planetGeneratorNetworked != null ? $"sur {planetGeneratorNetworked.gameObject.name}" : "")}");
        Debug.Log($"PlanetSaveManager: {planetSaveManager != null} {(planetSaveManager != null ? $"sur {planetSaveManager.gameObject.name}" : "")}");
        Debug.Log($"SimplePlanetUI: {simplePlanetUI != null} {(simplePlanetUI != null ? $"sur {simplePlanetUI.gameObject.name}" : "")}");
        Debug.Log($"PlanetNetworkManager: {planetNetworkManager != null} {(planetNetworkManager != null ? $"sur {planetNetworkManager.gameObject.name}" : "")}");
        Debug.Log($"NetworkManager: {networkManager != null} {(networkManager != null ? $"sur {networkManager.gameObject.name}" : "")}");
        
        // Vérifie les GameObjects actifs
        Debug.Log("=== GAMEOBJECTS ACTIFS ===");
        if (planetGenerator != null)
            Debug.Log($"PlanetGenerator GameObject: {planetGenerator.gameObject.name} - Active: {planetGenerator.gameObject.activeInHierarchy}");
        if (planetGeneratorNetworked != null)
            Debug.Log($"PlanetGeneratorNetworked GameObject: {planetGeneratorNetworked.gameObject.name} - Active: {planetGeneratorNetworked.gameObject.activeInHierarchy}");
        if (simplePlanetUI != null)
            Debug.Log($"SimplePlanetUI GameObject: {simplePlanetUI.gameObject.name} - Active: {simplePlanetUI.gameObject.activeInHierarchy}");
        
        Debug.Log("=== FIN DU DIAGNOSTIC ===");
    }
    
    [ContextMenu("Réparer la Scène")]
    public void FixScene()
    {
        Debug.Log("=== RÉPARATION DE LA SCÈNE ===");
        
        // Vérifie si les composants existent
        var planetGenerator = FindObjectOfType<PlanetGenerator>();
        var planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
        var simplePlanetUI = FindObjectOfType<SimplePlanetUI>();
        
        if (planetGenerator == null && planetGeneratorNetworked == null)
        {
            Debug.Log("Aucun générateur trouvé, création d'un PlanetGenerator...");
            var go = new GameObject("PlanetGenerator");
            go.AddComponent<PlanetGenerator>();
            go.AddComponent<PlanetSaveManager>();
            Debug.Log("PlanetGenerator créé !");
        }
        
        if (simplePlanetUI == null)
        {
            Debug.Log("SimplePlanetUI non trouvé, création...");
            var go = new GameObject("PlanetUI");
            go.AddComponent<SimplePlanetUI>();
            Debug.Log("SimplePlanetUI créé !");
        }
        
        // Active tous les GameObjects
        var allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (!obj.activeInHierarchy)
            {
                Debug.Log($"Activation de {obj.name}");
                obj.SetActive(true);
            }
        }
        
        Debug.Log("=== RÉPARATION TERMINÉE ===");
    }
}
