using UnityEngine;
using FishNet.Managing;
using FishNet.Object;

/// <summary>
/// Setup automatique complet du système
/// </summary>
public class AutoSetup : MonoBehaviour
{
    [Header("Configuration")]
    public bool setupOnStart = true;
    public bool createPlanetGenerator = true;
    public bool createNetworkedVersion = true;
    public bool createUI = true;
    public bool createNetworkManager = true;
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupEverything();
        }
    }
    
    [ContextMenu("Setup Complet")]
    public void SetupEverything()
    {
        Debug.Log("=== SETUP AUTOMATIQUE DÉMARRÉ ===");
        
        // 1. Crée le NetworkManager si nécessaire
        if (createNetworkManager)
        {
            CreateNetworkManager();
        }
        
        // 2. Crée le PlanetGenerator si nécessaire
        if (createPlanetGenerator)
        {
            CreatePlanetGenerator();
        }
        
        // 3. Crée le PlanetGeneratorNetworked si nécessaire
        if (createNetworkedVersion)
        {
            CreatePlanetGeneratorNetworked();
        }
        
        // 4. Crée l'interface utilisateur si nécessaire
        if (createUI)
        {
            CreateUI();
        }
        
        // 5. Configure les références
        SetupReferences();
        
        Debug.Log("=== SETUP AUTOMATIQUE TERMINÉ ===");
    }
    
    private void CreateNetworkManager()
    {
        var nm = FindObjectOfType<NetworkManager>();
        if (nm == null)
        {
            GameObject nmGO = new GameObject("NetworkManager");
            nm = nmGO.AddComponent<NetworkManager>();
            Debug.Log("✓ NetworkManager créé");
        }
        else
        {
            Debug.Log("✓ NetworkManager déjà présent");
        }
    }
    
    private void CreatePlanetGenerator()
    {
        var pg = FindObjectOfType<PlanetGenerator>();
        if (pg == null)
        {
            GameObject pgGO = new GameObject("Planet Generator");
            pg = pgGO.AddComponent<PlanetGenerator>();
            Debug.Log("✓ PlanetGenerator créé");
        }
        else
        {
            Debug.Log("✓ PlanetGenerator déjà présent");
        }
    }
    
    private void CreatePlanetGeneratorNetworked()
    {
        var pgn = FindObjectOfType<PlanetGeneratorNetworked>();
        if (pgn == null)
        {
            // Trouve le PlanetGenerator existant
            var pg = FindObjectOfType<PlanetGenerator>();
            if (pg != null)
            {
                // Ajoute PlanetGeneratorNetworked sur le même objet
                pgn = pg.gameObject.AddComponent<PlanetGeneratorNetworked>();
                Debug.Log("✓ PlanetGeneratorNetworked ajouté sur PlanetGenerator");
            }
            else
            {
                // Crée un nouvel objet
                GameObject pgnGO = new GameObject("Planet Generator Networked");
                pgn = pgnGO.AddComponent<PlanetGeneratorNetworked>();
                Debug.Log("✓ PlanetGeneratorNetworked créé sur nouvel objet");
            }
        }
        else
        {
            Debug.Log("✓ PlanetGeneratorNetworked déjà présent");
        }
        
        // S'assure qu'il a un NetworkObject
        if (pgn != null)
        {
            var netObj = pgn.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                netObj = pgn.gameObject.AddComponent<NetworkObject>();
                Debug.Log("✓ NetworkObject ajouté à PlanetGeneratorNetworked");
            }
            else
            {
                Debug.Log("✓ NetworkObject déjà présent");
            }
        }
    }
    
    private void CreateUI()
    {
        var spui = FindObjectOfType<SimplePlanetUI>();
        if (spui == null)
        {
            GameObject uiGO = new GameObject("Simple Planet UI");
            spui = uiGO.AddComponent<SimplePlanetUI>();
            Debug.Log("✓ SimplePlanetUI créé");
        }
        else
        {
            Debug.Log("✓ SimplePlanetUI déjà présent");
        }
    }
    
    private void SetupReferences()
    {
        // Configure les références dans SimplePlanetUI
        var spui = FindObjectOfType<SimplePlanetUI>();
        if (spui != null)
        {
            if (spui.planetGenerator == null)
                spui.planetGenerator = FindObjectOfType<PlanetGenerator>();
                
            if (spui.planetGeneratorNetworked == null)
                spui.planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
                
            if (spui.networkManager == null)
                spui.networkManager = FindObjectOfType<PlanetNetworkManager>();
                
            Debug.Log("✓ Références configurées dans SimplePlanetUI");
        }
        
        // Configure les références dans PlanetNetworkManager
        var pnm = FindObjectOfType<PlanetNetworkManager>();
        if (pnm != null)
        {
            if (pnm.planetGeneratorNetworked == null)
                pnm.planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
                
            Debug.Log("✓ Références configurées dans PlanetNetworkManager");
        }
    }
}
