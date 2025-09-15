using UnityEngine;

/// <summary>
/// Script de diagnostic pour vérifier les composants
/// </summary>
public class Diagnostic : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== DIAGNOSTIC DÉMARRÉ ===");
        
        // Vérifie les composants principaux
        CheckPlanetGenerator();
        CheckPlanetGeneratorNetworked();
        CheckPlanetNetworkManager();
        CheckSimplePlanetUI();
        CheckSimplePlanetSetup();
        CheckNetworkManager();
        
        Debug.Log("=== FIN DU DIAGNOSTIC ===");
    }
    
    private void CheckPlanetGenerator()
    {
        var pg = FindObjectOfType<PlanetGenerator>();
        if (pg != null)
        {
            Debug.Log($"✓ PlanetGenerator trouvé sur: {pg.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ PlanetGenerator NON TROUVÉ");
        }
    }
    
    private void CheckPlanetGeneratorNetworked()
    {
        var pgn = FindObjectOfType<PlanetGeneratorNetworked>();
        if (pgn != null)
        {
            Debug.Log($"✓ PlanetGeneratorNetworked trouvé sur: {pgn.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ PlanetGeneratorNetworked NON TROUVÉ");
        }
    }
    
    private void CheckPlanetNetworkManager()
    {
        var pnm = FindObjectOfType<PlanetNetworkManager>();
        if (pnm != null)
        {
            Debug.Log($"✓ PlanetNetworkManager trouvé sur: {pnm.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ PlanetNetworkManager NON TROUVÉ");
        }
    }
    
    private void CheckSimplePlanetUI()
    {
        var spui = FindObjectOfType<SimplePlanetUI>();
        if (spui != null)
        {
            Debug.Log($"✓ SimplePlanetUI trouvé sur: {spui.gameObject.name}");
            Debug.Log($"  - showUI: {spui.showUI}");
        }
        else
        {
            Debug.LogWarning("✗ SimplePlanetUI NON TROUVÉ");
        }
    }
    
    private void CheckSimplePlanetSetup()
    {
        var sps = FindObjectOfType<SimplePlanetSetup>();
        if (sps != null)
        {
            Debug.Log($"✓ SimplePlanetSetup trouvé sur: {sps.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ SimplePlanetSetup NON TROUVÉ");
        }
    }
    
    private void CheckNetworkManager()
    {
        var nm = FindObjectOfType<FishNet.Managing.NetworkManager>();
        if (nm != null)
        {
            Debug.Log($"✓ NetworkManager trouvé sur: {nm.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ NetworkManager NON TROUVÉ");
        }
    }
    
    [ContextMenu("Relancer le diagnostic")]
    public void RelancerDiagnostic()
    {
        Start();
    }
}
