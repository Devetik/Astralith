using UnityEngine;

public class PlanetSetup : MonoBehaviour
{
    [Header("Configuration automatique")]
    public bool setupOnStart = true;
    public bool createCameraIfMissing = true;
    public bool setupCameraController = true;

    [Header("Références")]
    public Camera mainCamera;
    public PlanetGenerator planetGenerator;
    public PlanetCameraController cameraController;

    private void Start()
    {
        if (setupOnStart)
        {
            SetupPlanetSystem();
        }
    }

    /// <summary>
    /// Configure automatiquement le système de planète
    /// </summary>
    public void SetupPlanetSystem()
    {
        // Trouve ou crée les composants nécessaires
        FindOrCreateComponents();
        
        // Configure la caméra
        if (setupCameraController && mainCamera != null && cameraController != null)
        {
            SetupCamera();
        }
        
        // Génère la planète initiale
        if (planetGenerator != null)
        {
            planetGenerator.GeneratePlanet();
        }
    }

    private void FindOrCreateComponents()
    {
        // Trouve le générateur de planète
        if (planetGenerator == null)
            planetGenerator = FindObjectOfType<PlanetGenerator>();

        // Trouve la caméra principale
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();

        // Crée une caméra si nécessaire
        if (createCameraIfMissing && mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
            
            // Position par défaut
            cameraGO.transform.position = new Vector3(0, 0, -20);
            cameraGO.transform.LookAt(Vector3.zero);
        }

        // Trouve ou crée le contrôleur de caméra
        if (cameraController == null)
            cameraController = FindObjectOfType<PlanetCameraController>();

        if (setupCameraController && cameraController == null && mainCamera != null)
        {
            cameraController = mainCamera.gameObject.AddComponent<PlanetCameraController>();
        }
    }

    private void SetupCamera()
    {
        if (cameraController == null || mainCamera == null) return;

        // Configure le contrôleur de caméra
        cameraController.planetGenerator = planetGenerator;
        
        // Position initiale de la caméra
        if (planetGenerator != null && planetGenerator.spawnPoint != null)
        {
            Vector3 planetPos = planetGenerator.spawnPoint.position;
            mainCamera.transform.position = planetPos + new Vector3(0, 0, -20);
            mainCamera.transform.LookAt(planetPos);
        }
        else
        {
            mainCamera.transform.position = new Vector3(0, 0, -20);
            mainCamera.transform.LookAt(Vector3.zero);
        }

        // Ajuste la distance selon la taille de la planète
        if (planetGenerator != null)
        {
            cameraController.AdjustDistanceToPlanet();
        }
    }

    /// <summary>
    /// Méthode publique pour reconfigurer le système
    /// </summary>
    [ContextMenu("Reconfigurer le système")]
    public void ReconfigureSystem()
    {
        SetupPlanetSystem();
    }

    /// <summary>
    /// Crée un setup complet pour une nouvelle scène
    /// </summary>
    [ContextMenu("Créer setup complet")]
    public void CreateCompleteSetup()
    {
        // Crée le générateur de planète
        if (planetGenerator == null)
        {
            GameObject planetGenGO = new GameObject("Planet Generator");
            planetGenerator = planetGenGO.AddComponent<PlanetGenerator>();
            
            // Ajoute les composants de sauvegarde
            planetGenGO.AddComponent<PlanetSaveManager>();
        }

        // Crée la caméra et son contrôleur
        if (mainCamera == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            mainCamera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
            cameraController = cameraGO.AddComponent<PlanetCameraController>();
        }

        // Configure tout
        SetupPlanetSystem();
    }

    // Affichage d'informations (désactivé pour simplifier)
    // private void OnGUI() { ... }
}
