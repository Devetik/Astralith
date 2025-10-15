using UnityEngine;

public class CameraPlanet : MonoBehaviour
{
    [Header("🎯 Configuration")]
    [SerializeField] public float lookAtSpeed = 2f; // Vitesse d'orientation vers l'objet
    [SerializeField] public bool smoothLookAt = true; // Transition fluide
    
    [Header("🔄 Tracking")]
    [SerializeField] public bool enableTracking = true; // Activer le tracking de la cible
    
    [Header("🖱️ Détection de Clic")]
    [SerializeField] public float clickMaxDuration = 0.2f; // Durée max pour un clic simple
    [SerializeField] public float clickMaxMovement = 5f; // Mouvement max pour un clic simple
    
    [Header("🎮 Debug")]
    [SerializeField] public bool showDebugInfo = true;
    [SerializeField] public Color debugColor = Color.cyan;
    
    [Header("🎯 Gizmo de Contact")]
    [SerializeField] public bool showContactGizmo = true;
    [SerializeField] public Color contactGizmoColor = Color.red;
    [SerializeField] public float contactGizmoSize = 0.1f;
    [SerializeField] public float planetRadius = 1f; // Rayon de la planète pour le fallback
    [SerializeField] public float PlanetAreaFactor = 1f; // Facteur de multiplication pour la zone planétaire
    
    [Header("🔍 Zoom Dynamique")]
    [SerializeField] public bool enableZoom = true;
    [SerializeField] public float minZoomSpeed = 0.1f; // Vitesse minimale (près de la planète)
    [SerializeField] public float maxZoomSpeed = 100f; // Vitesse maximale (loin de la planète)
    [SerializeField] public float ZoomSpeed = 1f; // Vitesse maximale (loin de la planète)
    [SerializeField] public float zoomExponent = 2f; // Exposant pour la courbe exponentielle
    [SerializeField] public float contactBuffer = 0.1f; // Zone tampon autour du point de contact
    [SerializeField] public float maxZoomDistance = 5000f; // Distance maximale pour le zoom rapide
    
    [Header("📐 Orientation")]
    [SerializeField] public float maxLookUpAngle = 45f; // Angle maximum vers le haut (en degrés)
    
    // Variables privées
    private float planetaryArea = 0f; // Distance centre-surface de la planète
    private Camera cam;
    private Vector3 mousePositionOnClick;
    private float clickStartTime;
    private bool isClicking = false;
    private Transform currentTarget;
    private bool isLookingAtTarget = false;
    private Quaternion targetRotation;
    private Quaternion startRotation;
    private float lookAtStartTime;
    private float lookAtDuration = 1f; // Durée de la transition
    
    // Variables de tracking
    private Vector3 lastTargetPosition;
    private float lastTrackingCheck = 0f;
    private bool isTracking = false;
    private bool isInTrackingMode = false; // Mode suivi en temps réel
    private float lastMovementTime = 0f; // Temps du dernier mouvement détecté
    private float stabilityDuration = 1f; // Durée de stabilité requise pour repasser au mode check
    
    // Variables de contact
    private bool hasContactPoint = false;
    private Vector3 contactPoint;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = gameObject.AddComponent<Camera>();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateLookAt();
        UpdateTracking();
        UpdateContactDetection();
        HandleZoom();
    }
    
    void HandleInput()
    {
        // Détection de clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            mousePositionOnClick = Input.mousePosition;
            clickStartTime = Time.time;
            isClicking = true;
        }
        
        // Vérifier si c'est un clic simple (pas un mouvement)
        if (Input.GetMouseButtonUp(0) && isClicking)
        {
            float clickDuration = Time.time - clickStartTime;
            Vector3 clickMovement = Input.mousePosition - mousePositionOnClick;
            
            // Vérifier si c'est un clic simple
            if (clickDuration <= clickMaxDuration && clickMovement.magnitude <= clickMaxMovement)
            {
                SelectObjectAtMouse();
            }
            
            isClicking = false;
        }
    }
    
    bool SelectObjectAtMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        
        // Trier par distance (le plus proche en premier)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        // Chercher le premier objet avec un tag de focus
        foreach (RaycastHit hit in hits)
        {
            string objectTag = hit.collider.tag;
            
            // Vérifier si l'objet a un des tags de focus
            string[] focusTags = {"Planet", "Moon", "Sun"};
            foreach (string focusTag in focusTags)
            {
                if (objectTag == focusTag)
                {
                    StartLookingAt(hit.collider.transform);
                    return true; // Objet focusable sélectionné
                }
            }
        }
        
        return false; // Aucun objet focusable trouvé
    }
    
    void StartLookingAt(Transform target)
    {
        currentTarget = target;
        startRotation = transform.rotation;
        targetRotation = Quaternion.LookRotation(target.position - transform.position);
        lookAtStartTime = Time.time;
        isLookingAtTarget = true;
        
        // Initialiser le tracking
        lastTargetPosition = target.position;
        lastTrackingCheck = Time.time;
        lastMovementTime = Time.time;
        isTracking = true;
        isInTrackingMode = false;
        UpdateContactDetection();
        if (showDebugInfo)
        {
            Debug.Log($"CameraPlanet: Looking at {target.name} (tag: {target.tag})");
        }
    }
    
    void UpdateLookAt()
    {
        if (!isLookingAtTarget || currentTarget == null) return;
        
        float elapsed = Time.time - lookAtStartTime;
        float progress = elapsed / lookAtDuration;
        
        if (progress >= 1f)
        {
            // Transition terminée
            isLookingAtTarget = false;
            transform.rotation = targetRotation;
            return;
        }
        
        // Interpolation fluide
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
        
        if (smoothLookAt)
        {
            // Transition fluide
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, smoothProgress);
        }
        else
        {
            // Transition directe
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
        }
    }
    
    void UpdateTracking()
    {
        if (!enableTracking || !isTracking || currentTarget == null) return;
        
        // Mode suivi en temps réel
        if (isInTrackingMode)
        {
            // Suivi en temps réel - pas de délai
            Vector3 currentTargetPosition = currentTarget.position;
            Vector3 directionToTarget = currentTargetPosition - transform.position;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion newTargetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = newTargetRotation;
            }
            
            // Vérifier si la planète est stable depuis 1 seconde
            if (Time.time - lastMovementTime >= stabilityDuration)
            {
                isInTrackingMode = false;
                if (showDebugInfo)
                {
                    Debug.Log("CameraPlanet: Planet stable for 1s - Switching to check mode");
                }
            }
            return;
        }
        
        // Mode check - vérifier toutes les 0.1s si la planète bouge
        if (Time.time - lastTrackingCheck >= 0.1f)
        {
            Vector3 currentTargetPosition = currentTarget.position;
            float movementDistance = Vector3.Distance(currentTargetPosition, lastTargetPosition);
            
            // Si la planète a bougé, activer le mode suivi en temps réel
            if (movementDistance > 0.01f)
            {
                isInTrackingMode = true;
                lastMovementTime = Time.time; // Mettre à jour le temps du dernier mouvement
                
                if (showDebugInfo)
                {
                    Debug.Log("CameraPlanet: Movement detected - Starting real-time tracking");
                }
            }
            
            // Mettre à jour la position de référence
            lastTargetPosition = currentTargetPosition;
            lastTrackingCheck = Time.time;
        }
    }
    
    void HandleZoom()
    {
        if (!enableZoom || currentTarget == null) return;
        
        // Détecter la molette de la souris
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Calculer la distance actuelle à la surface de la planète
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position) - planetaryArea;
            
            // Calculer la distance minimale (buffer autour de la surface)
            float minDistance = contactBuffer;
                        
            // Calculer la vitesse de zoom exponentielle
            //float zoomSpeed = CalculateExponentialZoomSpeed(currentDistance, minDistance);
            float zoomSpeed = CalculateZoomSpeed(currentDistance);
            
            // Debug pour voir les valeurs
            if (showDebugInfo)
            {
                //Debug.Log($"Zoom: scroll={scroll}, currentDistance={currentDistance:F2}, minDistance={minDistance:F2}, zoomSpeed={zoomSpeed:F2}");
            }
            
            // Appliquer le zoom
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 zoomMovement = directionToTarget * scroll * zoomSpeed * Time.deltaTime;
            
            // Vérifier qu'on ne va pas trop près de la surface
            float newDistanceToCenter = Vector3.Distance(transform.position + zoomMovement, currentTarget.position);
            float newDistanceToSurface = newDistanceToCenter - planetaryArea;
            if (newDistanceToSurface >= minDistance)
            {
                transform.position += zoomMovement;
            }


                transform.rotation = CalculateRotation(currentDistance);
          
        }
    }

    Quaternion CalculateRotation(float currentDistance)
    {
        // Calculer le facteur de transition (0 = très près, 1 = loin)
        float t = Mathf.InverseLerp(0f, planetaryArea / 4f, currentDistance);
        
        // Orientation vers la planète (quand on est loin)
        Vector3 directionToPlanet = (currentTarget.position - transform.position).normalized;
        Quaternion lookAtPlanet = Quaternion.LookRotation(directionToPlanet);
        
        // Orientation vers le haut (avec le bas de l'écran vers la planète) quand on est près
        Vector3 planetCenter = currentTarget.position;
        Vector3 cameraPosition = transform.position;
        
        // Calculer la direction vers la planète
        Vector3 toPlanet = (planetCenter - cameraPosition).normalized;
        
        // Calculer l'angle limité vers le haut
        float angleLimit = maxLookUpAngle * Mathf.Deg2Rad;
        
        // Créer une direction vers le haut limitée par l'angle
        Vector3 planetUp = Vector3.up;
        Vector3 limitedUp = Vector3.Slerp(toPlanet, planetUp, Mathf.Sin(angleLimit));
        
        // Orientation vers le haut limitée
        Quaternion lookAtHorizon = Quaternion.LookRotation(limitedUp, -toPlanet);
        
        // Interpoler entre les deux orientations avec Slerp pour une rotation fluide
        Quaternion finalRotation = Quaternion.Slerp(lookAtHorizon, lookAtPlanet, t);
        
        return finalRotation;
    }
    
    float CalculateZoomSpeed(float currentDistance)
    {
        float t = 1f -(Mathf.InverseLerp(planetaryArea / 4f, 0f, currentDistance));
        float ActualzoomSpeed = Mathf.Lerp(50f, 3500f, t);
        Debug.Log($"ZoomSpeed: {ActualzoomSpeed} / currentDistance:{currentDistance} / planetaryArea:{planetaryArea} t:{t}");
        return ActualzoomSpeed;
    }
    
   
    void UpdateContactDetection()
    {
        if (currentTarget == null)
        {
            hasContactPoint = false;
            return;
        }
        
        // Raycast depuis la caméra vers la planète
        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
        Ray ray = new Ray(transform.position, directionToTarget);
        
        // Raycast vers tous les colliders
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        
        // Trier par distance pour trouver le premier contact
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        // Chercher le premier hit avec la planète ciblée
        foreach (RaycastHit hit in hits)
        {
            // Vérifier si c'est la planète ciblée ou un de ses enfants
            if (IsPlanetOrChunk(hit.collider.transform))
            {
                contactPoint = hit.point;
                hasContactPoint = true;
                
                // Calculer la distance centre-surface pour PlanetaryArea
                planetaryArea = Vector3.Distance(currentTarget.position, contactPoint);
                
                return;
            }
        }
        
        // Si aucun contact trouvé, utiliser le point le plus proche sur la sphère
        Vector3 toPlanet = currentTarget.position - transform.position;
        float distanceToCenter = toPlanet.magnitude;
        if (distanceToCenter > 0)
        {
            Vector3 directionToPlanet = toPlanet.normalized;
            // Utiliser le rayon configuré de la planète
            contactPoint = currentTarget.position - directionToPlanet * planetRadius;
            hasContactPoint = true;
            
            // Utiliser le rayon configuré pour PlanetaryArea
            planetaryArea = planetRadius;
        }
        else
        {
            hasContactPoint = false;
            planetaryArea = 0f;
        }
    }
    
    bool IsPlanetOrChunk(Transform hitTransform)
    {
        // Vérifier si c'est la planète ciblée elle-même
        if (hitTransform == currentTarget)
        {
            return true;
        }
        
        // Vérifier si c'est un enfant de la planète ciblée (chunk)
        if (hitTransform.IsChildOf(currentTarget))
        {
            return true;
        }
        
        // Vérifier si c'est un parent de la planète ciblée
        if (currentTarget.IsChildOf(hitTransform))
        {
            return true;
        }
        
        // Vérifier par tag si c'est une planète
        if (hitTransform.CompareTag("Planet"))
        {
            return true;
        }
        
        // Vérifier si le nom contient des mots-clés de planète
        string hitName = hitTransform.name.ToLower();
        if (hitName.Contains("planet") || hitName.Contains("chunk") || hitName.Contains("hex"))
        {
            return true;
        }
        
        return false;
    }
    
    public void StopTracking()
    {
        isTracking = false;
        isLookingAtTarget = false;
        isInTrackingMode = false;
        currentTarget = null;
        lastMovementTime = 0f;
        hasContactPoint = false;
        planetaryArea = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log("CameraPlanet: Tracking stopped");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Dessiner la ligne vers l'objet ciblé
        if (currentTarget != null)
        {
            Gizmos.color = debugColor;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            
            // Dessiner une sphère sur l'objet ciblé
            Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
        }
        
        // Dessiner le gizmo de contact si activé et disponible
        if (showContactGizmo && hasContactPoint)
        {
            Gizmos.color = contactGizmoColor;
            Gizmos.DrawWireSphere(contactPoint, contactGizmoSize);
            
            // Dessiner une croix au point de contact
            Gizmos.DrawLine(contactPoint + Vector3.up * contactGizmoSize, 
                           contactPoint + Vector3.down * contactGizmoSize);
            Gizmos.DrawLine(contactPoint + Vector3.left * contactGizmoSize, 
                           contactPoint + Vector3.right * contactGizmoSize);
            Gizmos.DrawLine(contactPoint + Vector3.forward * contactGizmoSize, 
                           contactPoint + Vector3.back * contactGizmoSize);
        }
        
        // Dessiner la zone planétaire si activé et disponible
        if (currentTarget != null && planetaryArea > 0)
        {
            Gizmos.color = Color.green;
            float sphereRadius = planetaryArea * PlanetAreaFactor; // 2 fois la distance centre-surface
            Gizmos.DrawWireSphere(currentTarget.position, sphereRadius);
        }
    }
}
