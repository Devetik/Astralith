using UnityEngine;

public class CharacterEventManager : MonoBehaviour
{
    [Header("Références")]
    public CharacterPlacer characterPlacer;
    public PlanetGenerator planetGenerator;
    public PlanetGeneratorNetworked planetGeneratorNetworked;
    
    private void Start()
    {
        // Trouve automatiquement les composants si non assignés
        if (characterPlacer == null)
            characterPlacer = FindFirstObjectByType<CharacterPlacer>();
            
        if (planetGenerator == null)
            planetGenerator = FindFirstObjectByType<PlanetGenerator>();
            
        if (planetGeneratorNetworked == null)
            planetGeneratorNetworked = FindFirstObjectByType<PlanetGeneratorNetworked>();
        
        // S'abonne aux événements de génération
        SubscribeToGenerationEvents();
    }
    
    private void OnDestroy()
    {
        // Se désabonne des événements
        UnsubscribeFromGenerationEvents();
    }
    
    /// <summary>
    /// S'abonne aux événements de génération de planète
    /// </summary>
    private void SubscribeToGenerationEvents()
    {
        // S'abonne aux événements du PlanetGenerator (mode local)
        if (planetGenerator != null)
        {
            // Note: Il faudra ajouter un événement dans PlanetGenerator
            Debug.Log("CharacterEventManager: Abonné aux événements PlanetGenerator");
        }
        
        // S'abonne aux événements du PlanetGeneratorNetworked (mode réseau)
        if (planetGeneratorNetworked != null)
        {
            // Note: Il faudra ajouter un événement dans PlanetGeneratorNetworked
            Debug.Log("CharacterEventManager: Abonné aux événements PlanetGeneratorNetworked");
        }
    }
    
    /// <summary>
    /// Se désabonne des événements de génération
    /// </summary>
    private void UnsubscribeFromGenerationEvents()
    {
        // Se désabonne des événements
        Debug.Log("CharacterEventManager: Désabonné des événements");
    }
    
    /// <summary>
    /// Appelé quand une nouvelle planète est générée
    /// </summary>
    public void OnPlanetGenerated()
    {
        if (characterPlacer != null)
        {
            characterPlacer.OnPlanetGenerated();
        }
    }
    
    /// <summary>
    /// Appelé quand une nouvelle seed est générée
    /// </summary>
    public void OnNewSeedGenerated()
    {
        if (characterPlacer != null)
        {
            characterPlacer.OnPlanetGenerated();
        }
    }
}
