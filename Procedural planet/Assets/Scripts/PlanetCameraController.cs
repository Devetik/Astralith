using UnityEngine;

public class PlanetCameraController : MonoBehaviour
{
    [Header("Références")]
    public Transform planetCenter; // Centre de la planète (peut être le GameObject de la planète)
    public PlanetGenerator planetGenerator; // Pour récupérer automatiquement la position de la planète

    [Header("Contrôles")]
    public float mouseSensitivity = 1f;
    public float scrollSensitivity = 2f;
    public KeyCode rotateKey = KeyCode.Mouse1; // Clic droit par défaut
    public bool lockCursorWhenRotating = false; // Désactivé par défaut pour éviter les problèmes

    [Header("Limites")]
    public float minDistance = 5f;
    public float maxDistance = 50f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Vitesse")]
    public float rotationSpeed = 5f;
    public float zoomSpeed = 5f;

    // Variables privées
    private float currentDistance;
    private float currentHorizontalAngle;
    private float currentVerticalAngle;
    private Vector3 lastMousePosition;
    private bool isRotating = false;

    private void Start()
    {
        // Trouve automatiquement le générateur de planète si non assigné
        if (planetGenerator == null)
            planetGenerator = FindObjectOfType<PlanetGenerator>();

        // Trouve automatiquement le centre de la planète
        if (planetCenter == null && planetGenerator != null)
        {
            // Cherche un GameObject nommé "Planet" ou utilise le spawnPoint
            GameObject planet = GameObject.Find("Planet");
            if (planet != null)
                planetCenter = planet.transform;
            else if (planetGenerator.spawnPoint != null)
                planetCenter = planetGenerator.spawnPoint;
            else
                planetCenter = planetGenerator.transform;
        }

        // Initialise la distance et les angles
        if (planetCenter != null)
        {
            currentDistance = Vector3.Distance(transform.position, planetCenter.position);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            
            // Calcule les angles initiaux avec la formule corrigée
            Vector3 direction = (transform.position - planetCenter.position).normalized;
            currentHorizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentVerticalAngle = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        }
        else
        {
            currentDistance = 20f;
            currentHorizontalAngle = 0f;
            currentVerticalAngle = 15f; // Angle légèrement incliné pour une meilleure vue
        }

        UpdateCameraPosition();
    }

    private void Update()
    {
        // Vérifie si le centre de la planète est toujours valide
        if (planetCenter == null)
        {
            // Essaie de retrouver le centre de la planète
            if (planetGenerator != null)
            {
                GameObject planet = GameObject.Find("Planet");
                if (planet != null)
                {
                    SetPlanetCenter(planet.transform);
                }
            }
        }
        
        HandleInput();
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        // Gestion du zoom avec la molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDistance -= scroll * scrollSensitivity;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        // Gestion de la rotation avec le clic droit
        if (Input.GetKeyDown(rotateKey))
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
            if (lockCursorWhenRotating)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (Input.GetKeyUp(rotateKey))
        {
            isRotating = false;
            if (lockCursorWhenRotating)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (isRotating)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // Rotation horizontale (autour de l'axe Y) - pas de Time.deltaTime pour éviter les vibrations
            currentHorizontalAngle += mouseDelta.x * mouseSensitivity * 0.1f;
            
            // Rotation verticale (autour de l'axe X local) - pas de Time.deltaTime pour éviter les vibrations
            currentVerticalAngle -= mouseDelta.y * mouseSensitivity * 0.1f;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            
            lastMousePosition = Input.mousePosition;
        }

        // Contrôles clavier alternatifs (optionnels)
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        // Rotation avec les flèches ou WASD
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            currentHorizontalAngle += horizontalInput * rotationSpeed * Time.deltaTime;
        }

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            currentVerticalAngle += verticalInput * rotationSpeed * Time.deltaTime;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        // Zoom avec + et -
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        {
            currentDistance -= zoomSpeed * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            currentDistance += zoomSpeed * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    private void UpdateCameraPosition()
    {
        if (planetCenter == null) return;

        // Convertit les angles en radians
        float horizontalRad = currentHorizontalAngle * Mathf.Deg2Rad;
        float verticalRad = currentVerticalAngle * Mathf.Deg2Rad;

        // Calcule la position de la caméra en coordonnées sphériques
        // Formule corrigée pour une rotation plus naturelle
        Vector3 offset = new Vector3(
            Mathf.Cos(verticalRad) * Mathf.Sin(horizontalRad),
            Mathf.Sin(verticalRad),
            Mathf.Cos(verticalRad) * Mathf.Cos(horizontalRad)
        ) * currentDistance;

        // Applique la position
        Vector3 targetPosition = planetCenter.position + offset;
        transform.position = targetPosition;

        // Oriente la caméra vers le centre de la planète
        Vector3 lookDirection = (planetCenter.position - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    /// <summary>
    /// Centre la caméra sur un nouveau point
    /// </summary>
    public void SetPlanetCenter(Transform newCenter)
    {
        if (newCenter == null)
        {
            return;
        }

        planetCenter = newCenter;
        
        // Recalcule la distance et les angles pour la nouvelle position
        if (planetCenter != null)
        {
            currentDistance = Vector3.Distance(transform.position, planetCenter.position);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            
            // Recalcule les angles pour la nouvelle position
            Vector3 direction = (transform.position - planetCenter.position).normalized;
            currentHorizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentVerticalAngle = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
            
            UpdateCameraPosition();
        }
    }

    /// <summary>
    /// Met à jour automatiquement le centre quand une nouvelle planète est générée
    /// </summary>
    public void OnPlanetGenerated()
    {
        if (planetGenerator != null)
        {
            // Cherche le GameObject "Planet" créé par le générateur
            GameObject planet = GameObject.Find("Planet");
            if (planet != null)
            {
                SetPlanetCenter(planet.transform);
            }
            else
            {
                // Fallback: utilise le spawnPoint ou le transform du générateur
                if (planetGenerator.spawnPoint != null)
                {
                    SetPlanetCenter(planetGenerator.spawnPoint);
                }
                else
                {
                    SetPlanetCenter(planetGenerator.transform);
                }
            }
            
            // Ajuste la distance selon la nouvelle taille de planète
            AdjustDistanceToPlanet();
        }
    }

    /// <summary>
    /// Réinitialise la position de la caméra
    /// </summary>
    public void ResetCamera()
    {
        currentDistance = 20f;
        currentHorizontalAngle = 0f;
        currentVerticalAngle = 0f;
        UpdateCameraPosition();
    }

    /// <summary>
    /// Ajuste automatiquement la distance selon la taille de la planète
    /// </summary>
    public void AdjustDistanceToPlanet()
    {
        if (planetGenerator != null)
        {
            float planetRadius = planetGenerator.radius;
            minDistance = planetRadius * 1.5f;
            maxDistance = planetRadius * 10f;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    // Affichage d'informations dans l'inspecteur (désactivé pour simplifier)
    // private void OnGUI() { ... }

    // Gizmos pour visualiser dans l'éditeur
    private void OnDrawGizmosSelected()
    {
        if (planetCenter == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(planetCenter.position, minDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(planetCenter.position, maxDistance);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, planetCenter.position);
    }
}
