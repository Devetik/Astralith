using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Managing.Client;
using FishNet.Transporting;

public class PlanetNetworkManager : MonoBehaviour
{
    [Header("Configuration Réseau")]
    public string serverIP = "127.0.0.1";
    public ushort serverPort = 7772; // Changé pour éviter les conflits
    public bool autoConnectAsClient = true; // Si true, tente de se connecter automatiquement en tant que client
    public float autoConnectInterval = 1.0f; // Intervalle entre les tentatives de connexion

    [Header("Références")]
    public PlanetGeneratorNetworked planetGeneratorNetworked;

    private NetworkManager _networkManager;
    [HideInInspector] public bool _isAutoConnecting = false;
    [HideInInspector] public bool _isHostMode = false;
    private float _lastAutoConnectAttempt = 0f;
    private bool _isReconnectingHost = false;

    private void Start()
    {
        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager non trouvé ! Assurez-vous d'avoir un NetworkManager dans la scène.");
            return;
        }

        // Trouve le générateur de planète networké
        if (planetGeneratorNetworked == null)
        {
            // Cherche d'abord sur le même objet
            planetGeneratorNetworked = GetComponent<PlanetGeneratorNetworked>();
            
            // Si pas trouvé, cherche dans la scène
            if (planetGeneratorNetworked == null)
                planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
        }

        // Configure les callbacks
        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

        // Démarrer l'auto-connexion en tant que client si activé
        if (autoConnectAsClient)
        {
            _isAutoConnecting = true;
            Debug.Log("Auto-connexion activée - tentative de connexion en tant que client...");
        }
    }

    private void Update()
    {
        // Auto-connexion en tant que client seulement si :
        // - Auto-connexion activée
        // - Pas en mode Host/Serveur
        // - Client pas déjà connecté
        // - Serveur pas démarré
        if (_isAutoConnecting && 
            !_isHostMode && 
            !_networkManager.ClientManager.Started && 
            !_networkManager.ServerManager.Started)
        {
            if (Time.time - _lastAutoConnectAttempt >= autoConnectInterval)
            {
                _lastAutoConnectAttempt = Time.time;
                Debug.Log("Tentative de connexion en tant que client...");
                _networkManager.ClientManager.StartConnection(serverIP, serverPort);
            }
        }
    }

    /// <summary>
    /// Démarre le serveur
    /// </summary>
    [ContextMenu("Démarrer Serveur")]
    public void StartServer()
    {
        if (_networkManager == null) return;

        // Arrêter l'auto-connexion et passer en mode Serveur
        _isAutoConnecting = false;
        _isHostMode = true;
        
        Debug.Log($"Passage en mode Serveur - tentative de démarrage sur le port {serverPort}...");
        
        try
        {
            _networkManager.ServerManager.StartConnection();
            Debug.Log($"Serveur démarré avec succès sur le port {serverPort}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erreur lors du démarrage du serveur: {e.Message}");
            Debug.LogError("Essayez de changer le port dans PlanetNetworkManager");
        }
    }

    /// <summary>
    /// Démarre le client
    /// </summary>
    [ContextMenu("Démarrer Client")]
    public void StartClient()
    {
        if (_networkManager == null) return;

        // Arrêter l'auto-connexion et se connecter manuellement
        _isAutoConnecting = false;
        
        Debug.Log("Connexion manuelle en tant que client...");
        _networkManager.ClientManager.StartConnection(serverIP, serverPort);
        // Client connecté
    }

    /// <summary>
    /// Démarre comme hôte (serveur + client)
    /// </summary>
    [ContextMenu("Démarrer Hôte")]
    public void StartHost()
    {
        if (_networkManager == null) return;

        // Arrêter l'auto-connexion et passer en mode Host
        _isAutoConnecting = false;
        _isHostMode = true;
        
        Debug.Log("Passage en mode Host - démarrage du serveur...");
        
        // Pour le mode Host, on démarre d'abord le serveur, puis on attend un peu avant de connecter le client
        _networkManager.ServerManager.StartConnection();
        
        // Attendre que le serveur soit prêt avant de connecter le client
        StartCoroutine(DelayedClientConnection());
    }
    
    /// <summary>
    /// Connecte le client avec un délai pour laisser le serveur se stabiliser
    /// </summary>
    private System.Collections.IEnumerator DelayedClientConnection()
    {
        // Attendre que le serveur soit complètement démarré
        yield return new WaitForSeconds(1.0f);
        
        // Connecter le client local
        _networkManager.ClientManager.StartConnection(serverIP, serverPort);
        Debug.Log("Client local connecté au serveur");
    }
    
    /// <summary>
    /// Reconnexion du client Host avec un délai plus long pour éviter les boucles
    /// </summary>
    private System.Collections.IEnumerator DelayedHostReconnection()
    {
        // Attendre plus longtemps pour éviter les reconnexions en boucle
        yield return new WaitForSeconds(3.0f);
        
        // Vérifier que le serveur est toujours actif et qu'on est toujours en mode Host
        if (_isHostMode && _networkManager.ServerManager.Started && !_networkManager.ClientManager.Started)
        {
            Debug.Log("Reconnexion du client Host après délai...");
            _networkManager.ClientManager.StartConnection(serverIP, serverPort);
        }
        
        // Réinitialiser le flag de reconnexion
        _isReconnectingHost = false;
    }

    /// <summary>
    /// Arrête toutes les connexions
    /// </summary>
    [ContextMenu("Arrêter")]
    public void StopAll()
    {
        if (_networkManager == null) return;

        _networkManager.ServerManager.StopConnection(true);
        _networkManager.ClientManager.StopConnection();
        
        // Réinitialiser les états
        _isHostMode = false;
        _isAutoConnecting = false;
        _isReconnectingHost = false;
        
        Debug.Log("Toutes les connexions arrêtées - retour en mode auto-connexion");
        
        // Redémarrer l'auto-connexion si activée
        if (autoConnectAsClient)
        {
            _isAutoConnecting = true;
        }
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        Debug.Log($"État du serveur: {args.ConnectionState}");
        
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("Serveur démarré avec succès !");
            
            // Arrêter l'auto-connexion si le serveur démarre
            if (_isAutoConnecting)
            {
                _isAutoConnecting = false;
                Debug.Log("Auto-connexion arrêtée - serveur démarré");
            }
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Serveur arrêté.");
        }
    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        Debug.Log($"État du client: {args.ConnectionState}");
        
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Debug.Log("Client connecté avec succès !");
            
            // Arrêter l'auto-connexion si on s'est connecté automatiquement
            if (_isAutoConnecting && !_isHostMode)
            {
                _isAutoConnecting = false;
                Debug.Log("Auto-connexion arrêtée - client connecté");
            }
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.Log("Client déconnecté.");
            
            // Si on est en mode Host et que le client se déconnecte inopinément, on le reconnecte
            // MAIS seulement si ce n'est pas une déconnexion normale et qu'on n'est pas déjà en train de reconnecter
            if (_isHostMode && _networkManager.ServerManager.Started && !_isReconnectingHost)
            {
                // Éviter la reconnexion en boucle - attendre un peu
                _isReconnectingHost = true;
                StartCoroutine(DelayedHostReconnection());
            }
            // Si on est en mode auto-connexion et qu'on se déconnecte, on continue à essayer
            else if (!_isHostMode && !_networkManager.ServerManager.Started)
            {
                Debug.Log("Auto-connexion - nouvelle tentative dans 1 seconde...");
                _isAutoConnecting = true;
            }
        }
    }
    

    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }

    // Interface utilisateur (désactivée pour simplifier)
    // private void OnGUI() { ... }
}
