using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

/// <summary>
/// Générateur de planète avec synchronisation réseau
/// </summary>
public class PlanetGeneratorNetworked : NetworkBehaviour
{
    [Header("Paramètres de la Planète")]
    public int resolution = 64;
    public float radius = 5f;
    public int seed = 12345;
    public int octaves = 5;
    public float lacunarity = 2f;
    [Range(0, 1)]
    public float persistence = 0.5f;
    public float continentFreq = 0.15f;  // Plus bas = continents plus larges
    [Range(0f,1f)]
    public float continentAmp = 0.25f;   // Plus haut = plus de relief
    public AnimationCurve continentCurve = AnimationCurve.EaseInOut(0,0,1,1);
    public float mountainFreq = 2.0f;
    [Range(0f,1f)]
    public float mountainAmp = 0.15f;    // Plus haut = montagnes plus hautes
    public float mountainMaskFreq = 0.6f;
    [Range(0.5f,3f)]
    public float mountainMaskPower = 1.4f;
    public bool useWarp = true;
    public float warpFreq = 0.5f;
    [Range(0f, 1f)]
    public float warpStrength = 0.2f;
    public Material landMaterial;
    public Material waterMaterial;
    [Range(0f,1f)]
    public float oceanLevel = 0.2f;      // Plus bas = plus de terre

    [Header("Synchronisation Réseau")]
    private PlanetSettingsData _currentSettings = new PlanetSettingsData();

    private PlanetGenerator _planetGenerator;

    private void Awake()
    {
        // S'assure que le GameObject reste activé
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("GameObject désactivé dans Awake, réactivation...");
            gameObject.SetActive(true);
        }
    }
    
    private void OnEnable()
    {
        // S'assure que le GameObject reste activé quand il est activé
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("GameObject désactivé dans OnEnable, réactivation...");
            gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // S'assure que le GameObject est activé
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log("GameObject désactivé détecté, réactivation...");
            gameObject.SetActive(true);
        }
        
        // Trouve le générateur de planète local
        _planetGenerator = GetComponent<PlanetGenerator>();
        if (_planetGenerator == null)
        {
            _planetGenerator = gameObject.AddComponent<PlanetGenerator>();
        }
        
        // S'assure que le générateur de planète est activé
        if (_planetGenerator != null && !_planetGenerator.gameObject.activeInHierarchy)
        {
            Debug.Log("Activation du PlanetGenerator...");
            _planetGenerator.gameObject.SetActive(true);
        }

        // Désactive le chargement automatique des sauvegardes
        var saveManager = GetComponent<PlanetSaveManager>();
        if (saveManager != null)
        {
            saveManager.autoLoadOnStart = false;
        }

        // Initialise les paramètres
        UpdateSettingsFromInspector();
        
        // Ne génère PAS automatiquement au démarrage - attend une connexion réseau
        // La génération se fera via OnStartServer() ou manuellement
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("PlanetGeneratorNetworked: Serveur démarré");
        
        // Initialise les paramètres sur le serveur
        UpdateSettingsFromInspector();
        
        // Ne génère PAS automatiquement - attend une action manuelle
        Debug.Log("Serveur prêt - utilisez le bouton 'Générer Planète' pour créer la planète");
    }
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("PlanetGeneratorNetworked: Client démarré");
    }

    private void OnDestroy()
    {
        // Nettoyage si nécessaire
    }

    /// <summary>
    /// Met à jour les paramètres depuis l'inspecteur
    /// </summary>
    [ContextMenu("Mettre à jour les paramètres")]
    public void UpdateSettingsFromInspector()
    {
        _currentSettings = CreateSettingsData();
    }
    
    /// <summary>
    /// Force la mise à jour des paramètres et génère la planète
    /// </summary>
    [ContextMenu("Générer avec paramètres actuels")]
    public void GenerateWithCurrentSettings()
    {
        UpdateSettingsFromInspector();
        
        // Attendre un frame pour que le réseau soit initialisé
        StartCoroutine(DelayedGeneratePlanet());
    }
    
    /// <summary>
    /// Génère la planète avec un délai pour laisser le temps au réseau de s'initialiser
    /// </summary>
    private System.Collections.IEnumerator DelayedGeneratePlanet()
    {
        // Attendre 2 frames pour que le réseau soit complètement initialisé
        yield return null;
        yield return null;
        
        GeneratePlanet();
    }
    
    /// <summary>
    /// Force la génération immédiate (pour diagnostic)
    /// </summary>
    [ContextMenu("Forcer Génération Immédiate")]
    public void ForceGenerateImmediate()
    {
        Debug.Log("Génération forcée immédiate");
        UpdateSettingsFromInspector();
        GeneratePlanetInternal();
    }
    
    /// <summary>
    /// Force la génération en mode local (sans réseau)
    /// </summary>
    [ContextMenu("Générer en Mode Local")]
    public void GenerateLocalOnly()
    {
        Debug.Log("Génération en mode local uniquement");
        UpdateSettingsFromInspector();
        GeneratePlanetInternal();
    }
    
    /// <summary>
    /// Force la synchronisation des paramètres aux clients
    /// </summary>
    [ContextMenu("Synchroniser avec Clients")]
    public void ForceSyncToClients()
    {
        if (IsServerStarted && NetworkManager.ServerManager.Clients.Count > 0)
        {
            Debug.Log("Synchronisation forcée avec les clients");
            UpdateSettingsFromInspector();
            SendSettingsToClientsRpc(_currentSettings);
        }
        else
        {
            Debug.LogWarning("Impossible de synchroniser : serveur non actif ou aucun client");
        }
    }
    
    /// <summary>
    /// Change la seed de la planète et régénère
    /// </summary>
    [ContextMenu("Nouvelle Seed")]
    public void GenerateNewSeed()
    {
        // Génère une nouvelle seed aléatoire
        int newSeed = Random.Range(0, int.MaxValue);
        
        // Met à jour la seed dans les paramètres networkés (c'est la source de vérité)
        this.seed = newSeed;
        
        Debug.Log($"Nouvelle seed générée : {newSeed}");
        
        // Génère la planète avec la nouvelle seed
        GeneratePlanet();
    }
    
    /// <summary>
    /// Génère la planète directement (sans réseau)
    /// </summary>
    [ContextMenu("Générer Directement")]
    public void GenerateDirectly()
    {
        UpdateSettingsFromInspector();
        GeneratePlanetInternal();
    }
    
    /// <summary>
    /// Supprime le fichier de sauvegarde pour éviter les conflits
    /// </summary>
    [ContextMenu("Supprimer Sauvegarde")]
    public void DeleteSaveFile()
    {
        var saveManager = GetComponent<PlanetSaveManager>();
        if (saveManager != null)
        {
            saveManager.DeleteSaveFile();
            Debug.Log("Fichier de sauvegarde supprimé");
        }
    }
    
    /// <summary>
    /// Synchronise les paramètres depuis PlanetGenerator
    /// </summary>
    [ContextMenu("Synchroniser depuis PlanetGenerator")]
    public void SyncFromPlanetGenerator()
    {
        if (_planetGenerator == null) return;
        
        this.resolution = _planetGenerator.resolution;
        this.radius = _planetGenerator.radius;
        this.seed = _planetGenerator.seed;
        this.octaves = _planetGenerator.octaves;
        this.persistence = _planetGenerator.persistence;
        this.lacunarity = _planetGenerator.lacunarity;
        this.continentFreq = _planetGenerator.continentFreq;
        this.continentAmp = _planetGenerator.continentAmp;
        this.continentCurve = _planetGenerator.continentCurve;
        this.mountainFreq = _planetGenerator.mountainFreq;
        this.mountainAmp = _planetGenerator.mountainAmp;
        this.mountainMaskFreq = _planetGenerator.mountainMaskFreq;
        this.mountainMaskPower = _planetGenerator.mountainMaskPower;
        this.useWarp = _planetGenerator.useWarp;
        this.warpFreq = _planetGenerator.warpFreq;
        this.warpStrength = _planetGenerator.warpStrength;
        this.oceanLevel = _planetGenerator.oceanLevel;
        
        Debug.Log("Paramètres synchronisés depuis PlanetGenerator");
    }

    /// <summary>
    /// Crée les données de paramètres depuis les valeurs actuelles
    /// </summary>
    private PlanetSettingsData CreateSettingsData()
    {
        return new PlanetSettingsData
        {
            resolution = this.resolution,
            radius = this.radius,
            seed = this.seed,
            octaves = this.octaves,
            lacunarity = this.lacunarity,
            persistence = this.persistence,
            continentFreq = this.continentFreq,
            continentAmp = this.continentAmp,
            mountainFreq = this.mountainFreq,
            mountainAmp = this.mountainAmp,
            mountainMaskFreq = this.mountainMaskFreq,
            mountainMaskPower = this.mountainMaskPower,
            useWarp = this.useWarp,
            warpFreq = this.warpFreq,
            warpStrength = this.warpStrength,
            oceanLevel = this.oceanLevel
        };
    }

    /// <summary>
    /// Génère la planète (appelé par le serveur)
    /// </summary>
    [ContextMenu("Générer Planète")]
    public void GeneratePlanet()
    {
        // Met à jour les paramètres depuis l'inspecteur avant de générer
        UpdateSettingsFromInspector();
        
        // Si le réseau n'est pas actif ou pas connecté, génère directement
        if (!IsNetworked || !IsServerStarted)
        {
            GeneratePlanetInternal();
        }
        else
        {
            GeneratePlanetServerRpc();
        }
    }
    
    /// <summary>
    /// Génère la planète avec synchronisation réseau
    /// </summary>
    [ContextMenu("Générer avec Réseau")]
    public void GenerateWithNetwork()
    {
        UpdateSettingsFromInspector();
        
        if (IsServerStarted)
        {
            Debug.Log("Génération avec réseau (serveur actif)");
            GeneratePlanetInternal();
        }
        else
        {
            Debug.Log("Génération en local (pas de serveur)");
            // Si le serveur n'est pas démarré, génère en local
            GeneratePlanetInternal();
        }
    }

    /// <summary>
    /// RPC pour demander la génération au serveur
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void GeneratePlanetServerRpc()
    {
        if (!IsServer)
        {
            Debug.LogWarning("GeneratePlanetServerRpc appelé mais pas serveur");
            return;
        }
        
        GeneratePlanetInternal();
    }

    /// <summary>
    /// Génère la planète en interne
    /// </summary>
    private void GeneratePlanetInternal()
    {
        if (_planetGenerator == null) return;

        // Met à jour les paramètres du générateur local
        ApplyCurrentSettingsToGenerator();

        // Désactive temporairement la sauvegarde automatique
        var saveManager = GetComponent<PlanetSaveManager>();
        bool wasAutoSaveEnabled = false;
        if (saveManager != null)
        {
            wasAutoSaveEnabled = saveManager.autoSaveOnGenerate;
            saveManager.autoSaveOnGenerate = false;
        }

        // Génère la planète
        _planetGenerator.GeneratePlanet();

        // Réactive la sauvegarde automatique si elle était activée
        if (saveManager != null && wasAutoSaveEnabled)
        {
            saveManager.autoSaveOnGenerate = true;
            // Sauvegarde manuelle après génération
            saveManager.SaveSettings();
        }

        // Envoie les paramètres aux clients seulement si le serveur est actif et a des clients
        if (IsServerStarted && NetworkManager.ServerManager.Clients.Count > 0)
        {
            Debug.Log($"Envoi des paramètres à {NetworkManager.ServerManager.Clients.Count} clients");
            SendSettingsToClientsRpc(_currentSettings);
        }
        else if (IsServerStarted)
        {
            Debug.Log("Serveur actif mais aucun client connecté");
        }
    }

    /// <summary>
    /// RPC pour envoyer les paramètres aux clients
    /// </summary>
    [ObserversRpc]
    private void SendSettingsToClientsRpc(PlanetSettingsData settings)
    {
        if (IsServer) return; // Le serveur ne doit pas exécuter ce RPC

        Debug.Log("Paramètres reçus du serveur");
        
        // Applique les paramètres
        ApplySettings(settings);
        
        // Met à jour le générateur local
        ApplyCurrentSettingsToGenerator();
        
        // Génère la planète
        if (_planetGenerator != null)
        {
            _planetGenerator.GeneratePlanet();
        }
    }

    /// <summary>
    /// Applique les paramètres actuels au générateur local
    /// </summary>
    private void ApplyCurrentSettingsToGenerator()
    {
        if (_planetGenerator == null) return;
        
        // S'assure que le générateur est activé
        if (!_planetGenerator.gameObject.activeInHierarchy)
        {
            Debug.Log("Activation du PlanetGenerator avant application des paramètres...");
            _planetGenerator.gameObject.SetActive(true);
        }

        // Copie les paramètres de PlanetGeneratorNetworked vers PlanetGenerator
        _planetGenerator.resolution = this.resolution;
        _planetGenerator.radius = this.radius;
        _planetGenerator.seed = this.seed;
        _planetGenerator.octaves = this.octaves;
        _planetGenerator.persistence = this.persistence;
        _planetGenerator.lacunarity = this.lacunarity;
        _planetGenerator.continentFreq = this.continentFreq;
        _planetGenerator.continentAmp = this.continentAmp;
        _planetGenerator.continentCurve = this.continentCurve;
        _planetGenerator.mountainFreq = this.mountainFreq;
        _planetGenerator.mountainAmp = this.mountainAmp;
        _planetGenerator.mountainMaskFreq = this.mountainMaskFreq;
        _planetGenerator.mountainMaskPower = this.mountainMaskPower;
        _planetGenerator.useWarp = this.useWarp;
        _planetGenerator.warpFreq = this.warpFreq;
        _planetGenerator.warpStrength = this.warpStrength;
        _planetGenerator.oceanLevel = this.oceanLevel;
        
        // Utilise les matériaux de PlanetGenerator si ils sont assignés
        if (_planetGenerator.landMaterial != null)
            _planetGenerator.landMaterial = _planetGenerator.landMaterial;
        if (_planetGenerator.waterMaterial != null)
            _planetGenerator.waterMaterial = _planetGenerator.waterMaterial;
            
    }

    /// <summary>
    /// Applique des paramètres spécifiques
    /// </summary>
    private void ApplySettings(PlanetSettingsData settings)
    {
        this.resolution = settings.resolution;
        this.radius = settings.radius;
        this.seed = settings.seed;
        this.octaves = settings.octaves;
        this.persistence = settings.persistence;
        this.lacunarity = settings.lacunarity;
        this.continentFreq = settings.continentFreq;
        this.continentAmp = settings.continentAmp;
        this.mountainFreq = settings.mountainFreq;
        this.mountainAmp = settings.mountainAmp;
        this.mountainMaskFreq = settings.mountainMaskFreq;
        this.mountainMaskPower = settings.mountainMaskPower;
        this.useWarp = settings.useWarp;
        this.warpFreq = settings.warpFreq;
        this.warpStrength = settings.warpStrength;
        this.oceanLevel = settings.oceanLevel;
    }


    /// <summary>
    /// Structure de données pour la synchronisation des paramètres
    /// </summary>
    [System.Serializable]
    public struct PlanetSettingsData
    {
        public int resolution;
        public float radius;
        public int seed;
        public int octaves;
        public float lacunarity;
        public float persistence;
        public float continentFreq;
        public float continentAmp;
        public float mountainFreq;
        public float mountainAmp;
        public float mountainMaskFreq;
        public float mountainMaskPower;
        public bool useWarp;
        public float warpFreq;
        public float warpStrength;
        public float oceanLevel;
    }
}
