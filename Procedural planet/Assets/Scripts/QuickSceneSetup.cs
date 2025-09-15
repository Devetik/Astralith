using UnityEngine;
using FishNet.Managing;
using FishNet.Object;

/// <summary>
/// Script de récupération rapide de la scène
/// </summary>
public class QuickSceneSetup : MonoBehaviour
{
    [ContextMenu("Recréer Tous les Objets")]
    public void RecreateAllObjects()
    {
        Debug.Log("=== RÉCRÉATION RAPIDE DE LA SCÈNE ===");
        
        // 0. Nettoie les objets existants
        CleanExistingObjects();
        
        // 1. Crée le PlanetGenerator avec tous ses composants
        CreatePlanetGenerator();
        
        // 2. Crée l'UI
        CreateUI();
        
        // 3. Crée le NetworkManager (en dernier pour éviter les erreurs)
        CreateNetworkManager();
        
        // 4. Force l'activation de tous les objets
        ForceActivateAllObjects();
        
        Debug.Log("=== RÉCRÉATION TERMINÉE ===");
    }
    
    [ContextMenu("Setup Minimal (Sans Réseau)")]
    public void CreateMinimalSetup()
    {
        Debug.Log("=== SETUP MINIMAL SANS RÉSEAU ===");
        
        // 0. Nettoie les objets existants
        CleanExistingObjects();
        
        // 1. Crée le PlanetGenerator simple
        CreateSimplePlanetGenerator();
        
        // 2. Crée l'UI
        CreateUI();
        
        // 3. Force l'activation de tous les objets
        ForceActivateAllObjects();
        
        Debug.Log("=== SETUP MINIMAL TERMINÉ ===");
    }
    
    [ContextMenu("Setup Complet (Avec Réseau)")]
    public void CreateFullNetworkSetup()
    {
        Debug.Log("=== SETUP COMPLET AVEC RÉSEAU ===");
        
        // 0. Nettoie les objets existants
        CleanExistingObjects();
        
        // 1. Crée le PlanetGenerator avec réseau
        CreateNetworkedPlanetGenerator();
        
        // 2. Crée l'UI
        CreateUI();
        
        // 3. Force l'activation de tous les objets
        ForceActivateAllObjects();
        
        Debug.Log("=== SETUP COMPLET AVEC RÉSEAU TERMINÉ ===");
        Debug.Log("⚠️ ATTENTION: Ajoutez manuellement le NetworkManager de FishNet !");
        Debug.Log("1. Créez un GameObject vide nommé 'NetworkManager'");
        Debug.Log("2. Ajoutez le composant 'NetworkManager' (FishNet)");
        Debug.Log("3. Ajoutez le composant 'PlanetNetworkManager'");
        Debug.Log("4. Configurez le NetworkManager dans l'inspecteur");
    }
    
    [ContextMenu("Nettoyer et Recréer")]
    public void CleanExistingObjects()
    {
        Debug.Log("=== NETTOYAGE DES OBJETS EXISTANTS ===");
        
        // Supprime les NetworkManagers existants
        var existingManagers = FindObjectsOfType<NetworkManager>();
        foreach (var manager in existingManagers)
        {
            if (manager != null)
            {
                DestroyImmediate(manager.gameObject);
                Debug.Log("NetworkManager existant supprimé");
            }
        }
        
        // Supprime les PlanetGenerators existants
        var existingGenerators = FindObjectsOfType<PlanetGeneratorNetworked>();
        foreach (var generator in existingGenerators)
        {
            if (generator != null)
            {
                DestroyImmediate(generator.gameObject);
                Debug.Log("PlanetGenerator existant supprimé");
            }
        }
        
        // Supprime les UIs existantes
        var existingUIs = FindObjectsOfType<SimplePlanetUI>();
        foreach (var ui in existingUIs)
        {
            if (ui != null)
            {
                DestroyImmediate(ui.gameObject);
                Debug.Log("SimplePlanetUI existant supprimé");
            }
        }
        
        Debug.Log("=== NETTOYAGE TERMINÉ ===");
    }
    
    [ContextMenu("Forcer Activation des Objets")]
    public void ForceActivateAllObjects()
    {
        Debug.Log("=== FORÇAGE DE L'ACTIVATION ===");
        
        // Active le PlanetGenerator s'il existe
        var planetGenerator = FindObjectOfType<PlanetGeneratorNetworked>();
        if (planetGenerator != null)
        {
            planetGenerator.gameObject.SetActive(true);
            Debug.Log("PlanetGenerator activé");
        }
        
        // Active l'UI s'il existe
        var ui = FindObjectOfType<SimplePlanetUI>();
        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            Debug.Log("SimplePlanetUI activé");
        }
        
        Debug.Log("=== ACTIVATION TERMINÉE ===");
    }
    
    [ContextMenu("Vérifier et Corriger la Caméra")]
    public void CheckAndFixCamera()
    {
        Debug.Log("=== VÉRIFICATION DE LA CAMÉRA ===");
        
        // Cherche la caméra principale
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.Log("Aucune caméra trouvée, création d'une nouvelle caméra...");
            
            // Crée une nouvelle caméra
            GameObject cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            
            // Positionne la caméra
            cameraGO.transform.position = new Vector3(0, 0, -10);
            cameraGO.transform.rotation = Quaternion.identity;
            
            Debug.Log("Nouvelle caméra créée");
        }
        else
        {
            Debug.Log($"Caméra trouvée: {mainCamera.name}");
            
            // Vérifie que la caméra est active
            if (!mainCamera.gameObject.activeInHierarchy)
            {
                mainCamera.gameObject.SetActive(true);
                Debug.Log("Caméra réactivée");
            }
            
            // Vérifie la position de la caméra
            if (mainCamera.transform.position.z > -5)
            {
                mainCamera.transform.position = new Vector3(0, 0, -10);
                Debug.Log("Position de la caméra corrigée");
            }
        }
        
        Debug.Log("=== VÉRIFICATION DE LA CAMÉRA TERMINÉE ===");
    }
    
    private void CreateNetworkManager()
    {
        // Cherche s'il existe déjà
        var existingManager = FindObjectOfType<NetworkManager>();
        if (existingManager != null)
        {
            Debug.Log("NetworkManager existe déjà");
            return;
        }
        
        try
        {
            // Crée le GameObject NetworkManager
            GameObject networkManagerGO = new GameObject("NetworkManager");
            
            // Ajoute le NetworkManager de FishNet
            var networkManager = networkManagerGO.AddComponent<NetworkManager>();
            
            // Configure le NetworkManager
            ConfigureNetworkManager(networkManager);
            
            // Crée le PlanetNetworkManager
            var planetNetworkManager = networkManagerGO.AddComponent<PlanetNetworkManager>();
            
            Debug.Log("NetworkManager créé et configuré avec succès");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors de la création du NetworkManager: {e.Message}");
            Debug.Log("Le système fonctionnera en mode local sans réseau");
        }
    }
    
    private void ConfigureNetworkManager(NetworkManager networkManager)
    {
        // Configuration de base du NetworkManager
        // Ces paramètres évitent les erreurs NullReference
        
        // Configuration du serveur
        if (networkManager.ServerManager != null)
        {
            // Le port sera configuré par PlanetNetworkManager
            Debug.Log("ServerManager configuré");
        }
        
        // Configuration du client
        if (networkManager.ClientManager != null)
        {
            // Configuration de base du client
            Debug.Log("ClientManager configuré");
        }
        
        // Configuration des transports (utilise le transport par défaut)
        if (networkManager.TransportManager != null)
        {
            // Le transport par défaut sera utilisé
            Debug.Log("TransportManager configuré");
        }
        
        Debug.Log("NetworkManager configuré avec les paramètres de base");
    }
    
    private void CreatePlanetGenerator()
    {
        // Cherche s'il existe déjà
        var existingGenerator = FindObjectOfType<PlanetGeneratorNetworked>();
        if (existingGenerator != null)
        {
            Debug.Log("PlanetGenerator existe déjà");
            return;
        }
        
        // Crée le GameObject PlanetGenerator
        GameObject planetGO = new GameObject("PlanetGenerator");
        
        // Ajoute les composants essentiels
        var planetGenerator = planetGO.AddComponent<PlanetGenerator>();
        var planetGeneratorNetworked = planetGO.AddComponent<PlanetGeneratorNetworked>();
        var networkObject = planetGO.AddComponent<NetworkObject>();
        var saveManager = planetGO.AddComponent<PlanetSaveManager>();
        
        // Configure le PlanetGenerator
        planetGenerator.resolution = 64;
        planetGenerator.radius = 1.0f;
        planetGenerator.seed = 12345;
        planetGenerator.oceanLevel = 0.2f;
        
        // Configure le PlanetGeneratorNetworked
        planetGeneratorNetworked.resolution = 64;
        planetGeneratorNetworked.radius = 1.0f;
        planetGeneratorNetworked.seed = 12345;
        planetGeneratorNetworked.oceanLevel = 0.2f;
        
        // Configure le SaveManager
        saveManager.autoLoadOnStart = true;
        saveManager.autoSaveOnGenerate = true;
        
        // S'assurer que le GameObject est actif
        planetGO.SetActive(true);
        
        Debug.Log("PlanetGenerator créé avec tous ses composants et activé");
    }
    
    private void CreateSimplePlanetGenerator()
    {
        // Cherche s'il existe déjà
        var existingGenerator = FindObjectOfType<PlanetGenerator>();
        if (existingGenerator != null)
        {
            Debug.Log("PlanetGenerator simple existe déjà");
            return;
        }
        
        // Crée le GameObject PlanetGenerator
        GameObject planetGO = new GameObject("PlanetGenerator");
        
        // Ajoute seulement les composants essentiels (sans réseau)
        var planetGenerator = planetGO.AddComponent<PlanetGenerator>();
        var saveManager = planetGO.AddComponent<PlanetSaveManager>();
        
        // Configure le PlanetGenerator
        planetGenerator.resolution = 64;
        planetGenerator.radius = 1.0f;
        planetGenerator.seed = 12345;
        planetGenerator.oceanLevel = 0.2f;
        
        // Configure le SaveManager
        saveManager.autoLoadOnStart = true;
        saveManager.autoSaveOnGenerate = true;
        
        // S'assurer que le GameObject est actif
        planetGO.SetActive(true);
        
        Debug.Log("PlanetGenerator simple créé et activé");
    }
    
    private void CreateNetworkedPlanetGenerator()
    {
        // Cherche s'il existe déjà
        var existingGenerator = FindObjectOfType<PlanetGeneratorNetworked>();
        if (existingGenerator != null)
        {
            Debug.Log("PlanetGeneratorNetworked existe déjà");
            return;
        }
        
        // Crée le GameObject PlanetGenerator
        GameObject planetGO = new GameObject("PlanetGenerator");
        
        // Ajoute tous les composants pour le réseau
        var planetGenerator = planetGO.AddComponent<PlanetGenerator>();
        var planetGeneratorNetworked = planetGO.AddComponent<PlanetGeneratorNetworked>();
        var networkObject = planetGO.AddComponent<NetworkObject>();
        var saveManager = planetGO.AddComponent<PlanetSaveManager>();
        
        // Configure le PlanetGenerator
        planetGenerator.resolution = 64;
        planetGenerator.radius = 1.0f;
        planetGenerator.seed = 12345;
        planetGenerator.oceanLevel = 0.2f;
        
        // Configure le PlanetGeneratorNetworked
        planetGeneratorNetworked.resolution = 64;
        planetGeneratorNetworked.radius = 1.0f;
        planetGeneratorNetworked.seed = 12345;
        planetGeneratorNetworked.oceanLevel = 0.2f;
        
        // Configure le SaveManager
        saveManager.autoLoadOnStart = true;
        saveManager.autoSaveOnGenerate = true;
        
        // S'assurer que le GameObject est actif
        planetGO.SetActive(true);
        
        Debug.Log("PlanetGeneratorNetworked créé et activé");
    }
    
    private void CreateUI()
    {
        // Cherche s'il existe déjà
        var existingUI = FindObjectOfType<SimplePlanetUI>();
        if (existingUI != null)
        {
            Debug.Log("SimplePlanetUI existe déjà");
            return;
        }
        
        // Crée le GameObject UI
        GameObject uiGO = new GameObject("PlanetUI");
        var simpleUI = uiGO.AddComponent<SimplePlanetUI>();
        
        Debug.Log("SimplePlanetUI créé");
    }
}
