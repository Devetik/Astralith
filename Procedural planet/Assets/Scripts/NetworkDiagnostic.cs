using UnityEngine;
using FishNet.Object;

/// <summary>
/// Diagnostic pour vérifier la configuration réseau
/// </summary>
public class NetworkDiagnostic : MonoBehaviour
{
    private void Start()
    {
        Invoke(nameof(CheckNetworkSetup), 2f); // Attendre 2 secondes
    }
    
    private void CheckNetworkSetup()
    {
        Debug.Log("=== DIAGNOSTIC RÉSEAU ===");
        
        // Vérifie PlanetGeneratorNetworked
        var pgn = FindObjectOfType<PlanetGeneratorNetworked>();
        if (pgn != null)
        {
            Debug.Log($"✓ PlanetGeneratorNetworked trouvé sur: {pgn.gameObject.name}");
            
            // Vérifie NetworkObject
            var netObj = pgn.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Debug.Log($"✓ NetworkObject trouvé sur: {pgn.gameObject.name}");
                Debug.Log($"  - IsNetworked: {netObj.IsNetworked}");
                Debug.Log($"  - IsSpawned: {netObj.IsSpawned}");
            }
            else
            {
                Debug.LogWarning("✗ NetworkObject MANQUANT sur PlanetGeneratorNetworked");
            }
            
            // Vérifie l'état réseau
            Debug.Log($"  - IsServer: {pgn.IsServer}");
            Debug.Log($"  - IsClient: {pgn.IsClient}");
            Debug.Log($"  - IsNetworked: {pgn.IsNetworked}");
        }
        else
        {
            Debug.LogWarning("✗ PlanetGeneratorNetworked NON TROUVÉ");
        }
        
        // Vérifie NetworkManager
        var nm = FindObjectOfType<FishNet.Managing.NetworkManager>();
        if (nm != null)
        {
            Debug.Log($"✓ NetworkManager trouvé sur: {nm.gameObject.name}");
            Debug.Log($"  - Server Started: {nm.ServerManager.Started}");
            Debug.Log($"  - Client Started: {nm.ClientManager.Started}");
        }
        else
        {
            Debug.LogWarning("✗ NetworkManager NON TROUVÉ");
        }
        
        Debug.Log("=== FIN DIAGNOSTIC RÉSEAU ===");
    }
    
    [ContextMenu("Relancer le diagnostic réseau")]
    public void RelancerDiagnostic()
    {
        CheckNetworkSetup();
    }
}
