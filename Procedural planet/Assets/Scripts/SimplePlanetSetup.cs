using UnityEngine;

/// <summary>
/// Configuration simple et automatique du système de planète procédurale
/// </summary>
public class SimplePlanetSetup : MonoBehaviour
{
    [Header("Configuration")]
    public bool setupOnStart = true;
    
    [Header("Références (optionnelles)")]
    public PlanetGenerator planetGenerator;
    public PlanetGeneratorNetworked planetGeneratorNetworked;
    public PlanetNetworkManager networkManager;
    public SimplePlanetUI simpleUI;
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupSystem();
        }
    }
    
    /// <summary>
    /// Configure automatiquement tout le système
    /// </summary>
    public void SetupSystem()
    {
        // Trouve ou crée les composants nécessaires
        FindOrCreateComponents();
        
        // Configure les références
        SetupReferences();
        
        // Configure l'interface utilisateur
        SetupUI();
    }
    
    private void FindOrCreateComponents()
    {
        // Trouve le PlanetGenerator
        if (planetGenerator == null)
            planetGenerator = FindObjectOfType<PlanetGenerator>();
            
        // Trouve le PlanetGeneratorNetworked
        if (planetGeneratorNetworked == null)
            planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
            
        // Trouve le PlanetNetworkManager
        if (networkManager == null)
            networkManager = FindObjectOfType<PlanetNetworkManager>();
            
        // Trouve le SimplePlanetUI
        if (simpleUI == null)
            simpleUI = FindObjectOfType<SimplePlanetUI>();
    }
    
    private void SetupReferences()
    {
        // Configure les références dans SimplePlanetUI
        if (simpleUI != null)
        {
            if (simpleUI.planetGenerator == null)
                simpleUI.planetGenerator = planetGenerator;
                
            if (simpleUI.planetGeneratorNetworked == null)
                simpleUI.planetGeneratorNetworked = planetGeneratorNetworked;
                
            if (simpleUI.networkManager == null)
                simpleUI.networkManager = networkManager;
        }
    }
    
    private void SetupUI()
    {
        // Crée l'interface utilisateur si elle n'existe pas
        if (simpleUI == null)
        {
            GameObject uiGO = new GameObject("Simple Planet UI");
            simpleUI = uiGO.AddComponent<SimplePlanetUI>();
            
            // Configure les références
            simpleUI.planetGenerator = planetGenerator;
            simpleUI.planetGeneratorNetworked = planetGeneratorNetworked;
            simpleUI.networkManager = networkManager;
        }
    }
    
    /// <summary>
    /// Reconfigure le système
    /// </summary>
    public void ReconfigureSystem()
    {
        SetupSystem();
    }
}

