using UnityEngine;

/// <summary>
/// Interface utilisateur simplifiée pour la génération de planètes et le multijoueur
/// </summary>
public class SimplePlanetUI : MonoBehaviour
{
    [Header("Références")]
    public PlanetGenerator planetGenerator;
    public PlanetGeneratorNetworked planetGeneratorNetworked;
    public PlanetNetworkManager networkManager;
    
    [Header("Configuration UI")]
    public bool showUI = true;
    public int fontSize = 16;
    public Color buttonColor = Color.white;
    public Color textColor = Color.white;
    
    private void Start()
    {
        Debug.Log("=== SIMPLEPLANETUI START ===");
        
        // Délai pour laisser le temps aux autres composants de s'initialiser
        StartCoroutine(DelayedComponentSearch());
    }
    
    private System.Collections.IEnumerator DelayedComponentSearch()
    {
        // Attendre 2 secondes pour laisser le temps aux composants de s'initialiser
        yield return new WaitForSeconds(2.0f);
        
        Debug.Log("=== RECHERCHE DIFFÉRÉE DES COMPOSANTS ===");
        
        // Trouve automatiquement les composants si non assignés
        if (planetGenerator == null)
        {
            planetGenerator = FindObjectOfType<PlanetGenerator>();
            Debug.Log($"PlanetGenerator trouvé: {planetGenerator != null}");
        }
            
        if (planetGeneratorNetworked == null)
        {
            planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
            Debug.Log($"PlanetGeneratorNetworked trouvé: {planetGeneratorNetworked != null}");
        }
            
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<PlanetNetworkManager>();
            Debug.Log($"PlanetNetworkManager trouvé: {networkManager != null}");
        }
        
        // Vérifie l'état final
        Debug.Log($"État final - PlanetGenerator: {planetGenerator != null}, PlanetGeneratorNetworked: {planetGeneratorNetworked != null}, NetworkManager: {networkManager != null}");
        
        if (planetGenerator == null && planetGeneratorNetworked == null)
        {
            Debug.LogError("Aucun générateur de planète trouvé ! Vérifiez que PlanetGenerator ou PlanetGeneratorNetworked est présent dans la scène.");
        }
        else
        {
            Debug.Log("✅ Générateur trouvé ! L'UI devrait maintenant fonctionner.");
        }
        
        Debug.Log("=== FIN RECHERCHE DIFFÉRÉE ===");
    }
    
    private void OnGUI()
    {
        if (!showUI) return;
        
        // Vérifie périodiquement si les composants sont trouvés
        if (planetGenerator == null && planetGeneratorNetworked == null)
        {
            planetGenerator = FindObjectOfType<PlanetGenerator>();
            planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
        }
        
        // Style pour les boutons
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = fontSize;
        buttonStyle.normal.textColor = textColor;
        
        // Style pour les labels
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = fontSize;
        labelStyle.normal.textColor = textColor;
        
        // Zone de l'interface
        float panelWidth = 300;
        float panelHeight = 300;
        float startX = 20;
        float startY = 20;
        
        // Fond semi-transparent
        GUI.Box(new Rect(startX - 10, startY - 10, panelWidth, panelHeight), "");
        
        // Titre
        GUI.Label(new Rect(startX, startY, panelWidth, 30), "Planète Procédurale", labelStyle);
        
        float currentY = startY + 40;
        
        // Section Génération de Planète
        GUI.Label(new Rect(startX, currentY, panelWidth, 25), "=== GÉNÉRATION ===", labelStyle);
        currentY += 30;
        
        if (planetGeneratorNetworked != null)
        {
            // Bouton Générer Planète
            if (GUI.Button(new Rect(startX, currentY, (panelWidth - 30) / 2, 30), "Générer Planète", buttonStyle))
            {
                planetGeneratorNetworked.GenerateWithNetwork();
            }
            
            // Bouton Nouvelle Seed
            if (GUI.Button(new Rect(startX + (panelWidth - 30) / 2 + 10, currentY, (panelWidth - 30) / 2, 30), "Nouvelle Seed", buttonStyle))
            {
                planetGeneratorNetworked.GenerateNewSeed();
            }
            currentY += 40;
            
            // Bouton Sauvegarder
            if (GUI.Button(new Rect(startX, currentY, panelWidth - 20, 25), "Sauvegarder Paramètres", buttonStyle))
            {
                var saveManager = planetGeneratorNetworked.GetComponent<PlanetSaveManager>();
                if (saveManager != null)
                {
                    saveManager.SaveFromNetworked(); // Force la sauvegarde depuis PlanetGeneratorNetworked
                    Debug.Log("Paramètres sauvegardés manuellement depuis PlanetGeneratorNetworked");
                }
            }
            currentY += 35;
        }
        else if (planetGenerator != null)
        {
            // Bouton Générer Planète
            if (GUI.Button(new Rect(startX, currentY, (panelWidth - 30) / 2, 30), "Générer Planète", buttonStyle))
            {
                planetGenerator.GeneratePlanet();
            }
            
            // Bouton Nouvelle Seed (pour le générateur local)
            if (GUI.Button(new Rect(startX + (panelWidth - 30) / 2 + 10, currentY, (panelWidth - 30) / 2, 30), "Nouvelle Seed", buttonStyle))
            {
                planetGenerator.seed = Random.Range(0, int.MaxValue);
                planetGenerator.GeneratePlanet();
            }
            currentY += 40;
            
            // Note: Sauvegarde désactivée pour PlanetGenerator (conflit avec PlanetGeneratorNetworked)
            GUI.Label(new Rect(startX, currentY, panelWidth - 20, 25), "Sauvegarde: Mode réseau uniquement", labelStyle);
            currentY += 35;
        }
        else
        {
            GUI.Label(new Rect(startX, currentY, panelWidth, 25), "Générateur non trouvé", labelStyle);
            currentY += 40;
        }
        
        // Section Multijoueur
        GUI.Label(new Rect(startX, currentY, panelWidth, 25), "=== MULTIJOUEUR ===", labelStyle);
        currentY += 30;
        
        // Indicateur d'état de l'auto-connexion
        if (networkManager != null && networkManager.autoConnectAsClient)
        {
            string autoConnectStatus = networkManager._isAutoConnecting ? "Auto-connexion active" : "Auto-connexion arrêtée";
            GUI.Label(new Rect(startX, currentY, panelWidth, 20), autoConnectStatus, labelStyle);
            currentY += 25;
        }
        
        if (networkManager != null)
        {
            // Boutons réseau
            if (GUI.Button(new Rect(startX, currentY, (panelWidth - 30) / 3, 25), "Hôte", buttonStyle))
            {
                networkManager.StartHost();
            }
            
            if (GUI.Button(new Rect(startX + (panelWidth - 30) / 3 + 5, currentY, (panelWidth - 30) / 3, 25), "Serveur", buttonStyle))
            {
                networkManager.StartServer();
            }
            
            if (GUI.Button(new Rect(startX + 2 * (panelWidth - 30) / 3 + 10, currentY, (panelWidth - 30) / 3, 25), "Client", buttonStyle))
            {
                networkManager.StartClient();
            }
            currentY += 35;
            
            // Bouton d'arrêt
            if (GUI.Button(new Rect(startX, currentY, panelWidth - 20, 25), "Arrêter", buttonStyle))
            {
                networkManager.StopAll();
            }
        }
        else
        {
            GUI.Label(new Rect(startX, currentY, panelWidth, 25), "NetworkManager non trouvé", labelStyle);
        }
    }
}

