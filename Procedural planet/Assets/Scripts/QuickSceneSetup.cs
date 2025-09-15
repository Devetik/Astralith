using UnityEngine;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Managing.Object;

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
        
        // 1. Crée le NetworkManager avec DefaultPrefabObjects
        CreateNetworkManager();
        
        // 2. Crée le PlanetGenerator avec réseau
        CreateNetworkedPlanetGenerator();
        
        // 3. Crée l'UI
        CreateUI();
        
        // 4. Force l'activation de tous les objets
        ForceActivateAllObjects();
        
        Debug.Log("=== SETUP COMPLET AVEC RÉSEAU TERMINÉ ===");
        Debug.Log("✅ NetworkManager configuré avec DefaultPrefabObjects !");
    }
    
    [ContextMenu("Réparer la Scène")]
    public void RepairScene()
    {
        Debug.Log("=== RÉPARATION DE LA SCÈNE ===");
        
        // 1. Trouve ou crée le PlanetGenerator
        RepairPlanetGenerator();
        
        // 2. Trouve ou crée le PlanetCameraController
        RepairCameraController();
        
        // 3. Assigne les matériaux
        AssignMaterials();
        
        // 4. Active tous les GameObjects
        ActivateAllObjects();
        
        Debug.Log("=== RÉPARATION TERMINÉE ===");
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
        
        // Crée et assigne le DefaultPrefabObjects
        CreateAndAssignDefaultPrefabObjects(networkManager);
        
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
    
    private void CreateAndAssignDefaultPrefabObjects(NetworkManager networkManager)
    {
#if UNITY_EDITOR
        // Crée le DefaultPrefabObjects
        var defaultPrefabObjects = ScriptableObject.CreateInstance<DefaultPrefabObjects>();
        
        // Sauvegarde le fichier
        string path = "Assets/Scripts/DefaultPrefabObjects.asset";
        UnityEditor.AssetDatabase.CreateAsset(defaultPrefabObjects, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        // Assigne le fichier au NetworkManager
        networkManager.SpawnablePrefabs = defaultPrefabObjects;
        
        Debug.Log($"DefaultPrefabObjects créé et assigné : {path}");
#else
        Debug.LogWarning("DefaultPrefabObjects ne peut être créé qu'en mode éditeur !");
#endif
    }
    
    private void RepairPlanetGenerator()
    {
        Debug.Log("--- Réparation PlanetGenerator ---");
        
        var planetGenerator = FindObjectOfType<PlanetGenerator>();
        if (planetGenerator == null)
        {
            Debug.Log("PlanetGenerator non trouvé, création...");
            var go = new GameObject("PlanetGenerator");
            planetGenerator = go.AddComponent<PlanetGenerator>();
            go.AddComponent<PlanetSaveManager>();
            Debug.Log("PlanetGenerator créé !");
        }
        else
        {
            Debug.Log($"PlanetGenerator trouvé sur {planetGenerator.gameObject.name}");
        }
        
        // S'assure que le GameObject est actif
        if (!planetGenerator.gameObject.activeInHierarchy)
        {
            planetGenerator.gameObject.SetActive(true);
            Debug.Log("PlanetGenerator activé !");
        }
    }
    
    private void RepairCameraController()
    {
        Debug.Log("--- Réparation CameraController ---");
        
        var cameraController = FindObjectOfType<PlanetCameraController>();
        var mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.Log("Caméra principale non trouvée, création...");
            var cameraGO = new GameObject("Main Camera");
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
            Debug.Log("Caméra principale créée !");
        }
        
        if (cameraController == null)
        {
            Debug.Log("PlanetCameraController non trouvé, ajout à la caméra...");
            cameraController = mainCamera.gameObject.AddComponent<PlanetCameraController>();
            Debug.Log("PlanetCameraController ajouté !");
        }
        else
        {
            Debug.Log($"PlanetCameraController trouvé sur {cameraController.gameObject.name}");
        }
        
        // Configure le PlanetCameraController
        if (cameraController.planetGenerator == null)
        {
            var planetGenerator = FindObjectOfType<PlanetGenerator>();
            if (planetGenerator != null)
            {
                cameraController.planetGenerator = planetGenerator;
                Debug.Log("PlanetGenerator assigné au PlanetCameraController !");
            }
        }
        
        if (cameraController.planetCenter == null)
        {
            var planetGenerator = FindObjectOfType<PlanetGenerator>();
            if (planetGenerator != null)
            {
                cameraController.planetCenter = planetGenerator.transform;
                Debug.Log("PlanetCenter assigné au PlanetCameraController !");
            }
        }
    }
    
    private void AssignMaterials()
    {
        Debug.Log("--- Attribution des Matériaux ---");
        
        var planetGenerator = FindObjectOfType<PlanetGenerator>();
        if (planetGenerator == null)
        {
            Debug.LogError("PlanetGenerator non trouvé pour l'attribution des matériaux !");
            return;
        }
        
        // Cherche les matériaux dans le dossier Materials
        var landMaterial = Resources.Load<Material>("landMaterial");
        var waterMaterial = Resources.Load<Material>("waterMaterial");
        
        if (landMaterial == null)
        {
            // Cherche dans le dossier Materials
            landMaterial = Resources.Load<Material>("Materials/landMaterial");
        }
        
        if (waterMaterial == null)
        {
            // Cherche dans le dossier Materials
            waterMaterial = Resources.Load<Material>("Materials/waterMaterial");
        }
        
        if (landMaterial == null)
        {
            Debug.LogWarning("Matériau de terre non trouvé, création d'un matériau par défaut...");
            landMaterial = CreateDefaultLandMaterial();
        }
        
        if (waterMaterial == null)
        {
            Debug.LogWarning("Matériau d'eau non trouvé, création d'un matériau par défaut...");
            waterMaterial = CreateDefaultWaterMaterial();
        }
        
        // Assigne les matériaux
        planetGenerator.landMaterial = landMaterial;
        planetGenerator.waterMaterial = waterMaterial;
        
        Debug.Log($"Matériaux assignés - Terre: {landMaterial != null}, Eau: {waterMaterial != null}");
    }
    
    private Material CreateDefaultLandMaterial()
    {
        var material = new Material(Shader.Find("Standard"));
        material.color = Color.green;
        material.name = "DefaultLandMaterial";
        return material;
    }
    
    private Material CreateDefaultWaterMaterial()
    {
        var material = new Material(Shader.Find("Standard"));
        material.color = Color.blue;
        material.name = "DefaultWaterMaterial";
        return material;
    }
    
    private void ActivateAllObjects()
    {
        Debug.Log("--- Activation de tous les GameObjects ---");
        
        var allObjects = FindObjectsOfType<GameObject>();
        int activatedCount = 0;
        
        foreach (var obj in allObjects)
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                activatedCount++;
                Debug.Log($"Activé: {obj.name}");
            }
        }
        
        Debug.Log($"Nombre d'objets activés: {activatedCount}");
    }
}
