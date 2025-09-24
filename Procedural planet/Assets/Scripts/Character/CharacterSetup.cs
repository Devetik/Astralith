using UnityEngine;

public class CharacterSetup : MonoBehaviour
{
    [Header("Configuration Rapide")]
    public GameObject characterPrefab;
    public bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupCharacterSystem();
        }
    }
    
    /// <summary>
    /// Configure automatiquement le système de personnage
    /// </summary>
    [ContextMenu("Configurer Système Personnage")]
    public void SetupCharacterSystem()
    {
        Debug.Log("=== CONFIGURATION SYSTÈME PERSONNAGE ===");
        
        // 1. Crée le CharacterPlacer
        SetupCharacterPlacer();
        
        // 2. Crée le CharacterEventManager
        SetupCharacterEventManager();
        
        // 3. Configure les événements
        SetupEvents();
        
        Debug.Log("=== CONFIGURATION TERMINÉE ===");
    }
    
    private void SetupCharacterPlacer()
    {
        var characterPlacer = FindObjectOfType<CharacterPlacer>();
        if (characterPlacer == null)
        {
            var placerGO = new GameObject("CharacterPlacer");
            characterPlacer = placerGO.AddComponent<CharacterPlacer>();
            Debug.Log("CharacterPlacer créé !");
        }
        else
        {
            Debug.Log("CharacterPlacer trouvé !");
        }
        
        // Trouve automatiquement les générateurs pour le CharacterPlacer
        if (characterPlacer.planetGenerator == null)
            characterPlacer.planetGenerator = FindObjectOfType<PlanetGenerator>();
            
        if (characterPlacer.planetGeneratorNetworked == null)
            characterPlacer.planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
            
        // Trouve ou crée le ContinentAnalyzer
        if (characterPlacer.continentAnalyzer == null)
        {
            characterPlacer.continentAnalyzer = FindObjectOfType<ContinentAnalyzer>();
            if (characterPlacer.continentAnalyzer == null)
            {
                var analyzerGO = new GameObject("ContinentAnalyzer");
                characterPlacer.continentAnalyzer = analyzerGO.AddComponent<ContinentAnalyzer>();
                Debug.Log("ContinentAnalyzer créé !");
            }
        }
        
        // Assigne le prefab si fourni
        if (characterPrefab != null && characterPlacer.characterPrefab == null)
        {
            characterPlacer.characterPrefab = characterPrefab.transform;
            Debug.Log("Prefab de personnage assigné !");
        }
    }
    
    private void SetupCharacterEventManager()
    {
        var eventManager = FindObjectOfType<CharacterEventManager>();
        if (eventManager == null)
        {
            var eventManagerGO = new GameObject("CharacterEventManager");
            eventManager = eventManagerGO.AddComponent<CharacterEventManager>();
            Debug.Log("CharacterEventManager créé !");
        }
        else
        {
            Debug.Log("CharacterEventManager trouvé !");
        }
    }
    
    private void SetupEvents()
    {
        // S'abonne aux événements de génération
        PlanetGenerator.OnPlanetGenerated += OnPlanetGenerated;
        PlanetGeneratorNetworked.OnPlanetGenerated += OnPlanetGenerated;
        
        Debug.Log("Événements de génération configurés !");
    }
    
    private void OnDestroy()
    {
        // Se désabonne des événements
        PlanetGenerator.OnPlanetGenerated -= OnPlanetGenerated;
        PlanetGeneratorNetworked.OnPlanetGenerated -= OnPlanetGenerated;
    }
    
    private void OnPlanetGenerated()
    {
        Debug.Log("Événement de génération de planète reçu !");
        
        // Trouve le CharacterPlacer et place le personnage
        var characterPlacer = FindObjectOfType<CharacterPlacer>();
        if (characterPlacer != null)
        {
            characterPlacer.OnPlanetGenerated();
        }
        else
        {
            Debug.LogWarning("CharacterPlacer non trouvé pour placer le personnage !");
        }
    }
}
