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
    
    [Header("🌊 Zoom Fluide")]
    [SerializeField] public bool enableSmoothZoom = true; // Activer le zoom fluide
    [SerializeField] public float smoothZoomTime = 0.5f; // Durée de lissage du zoom
    [SerializeField] public float zoomDeceleration = 8f; // Vitesse de décélération
    
    [Header("📐 Orientation")]
    [SerializeField] public float maxLookUpAngle = 45f; // Angle maximum vers le haut (en degrés)
    
    [Header("🌐 Sphère Intérieure")]
    [SerializeField] public bool enableInnerSphere = true; // Activer le système de sphère intérieure
    [SerializeField] public float sphereStartDistance = 10f; // Distance à partir de laquelle la sphère commence à grossir
    [SerializeField] public float sphereMaxSizePercent = 0.9f; // Taille maximale de la sphère (90% de la planète)
    [SerializeField] public bool lockToSphereTop = true; // Verrouiller l'orientation vers le haut de la sphère
    [SerializeField] public float manualRotationSpeed = 2f; // Vitesse de rotation manuelle
    [SerializeField] public float rotationTransitionSpeed = 5f; // Vitesse de transition de la rotation
    
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
    
    // Variables de la sphère intérieure
    private float currentSphereRadius = 0f;
    private Vector3 sphereCenter;
    private Vector3 sphereTargetPoint;
    private Quaternion manualRotationOffset = Quaternion.identity;
    private bool isManualRotation = false;
    
    // Variables de zoom fluide
    private float zoomBuffer = 0f; // Accumulation des crans de molette
    private float zoomVelocity = 0f; // Vitesse actuelle du zoom
    private float lastZoomTime = 0f; // Temps du dernier cran de molette
    
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
        UpdateInnerSphere();
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
        
        // Gestion de la rotation manuelle
        if (enableInnerSphere && !lockToSphereTop && currentTarget != null)
        {
            HandleManualRotation();
        }
    }
    
    void HandleManualRotation()
    {
        // Rotation avec la souris (clic droit maintenu)
        if (Input.GetMouseButton(1))
        {
            isManualRotation = true;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // Rotation horizontale (Yaw)
            Quaternion yawRotation = Quaternion.AngleAxis(mouseX * manualRotationSpeed, Vector3.up);
            
            // Rotation verticale (Pitch) - limitée pour éviter le retournement
            Vector3 right = transform.right;
            Quaternion pitchRotation = Quaternion.AngleAxis(-mouseY * manualRotationSpeed, right);
            
            // Appliquer les rotations
            manualRotationOffset = yawRotation * pitchRotation * manualRotationOffset;
        }
        else
        {
            isManualRotation = false;
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
        
        // Ne pas forcer la rotation si le système de sphère intérieure est actif
        if (enableInnerSphere)
        {
            // Marquer comme terminé pour éviter les conflits
            isLookingAtTarget = false;
            return;
        }
        
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
                // Ne pas forcer la rotation si le système de sphère intérieure est actif
                if (!enableInnerSphere)
                {
                    Quaternion newTargetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = newTargetRotation;
                }
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
        
        if (enableSmoothZoom)
        {
            HandleSmoothZoom(scroll);
        }
        else
        {
            HandleDirectZoom(scroll);
        }
    }
    
    void HandleSmoothZoom(float scroll)
    {
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Accumuler les crans de molette dans le buffer
            zoomBuffer += scroll;
            lastZoomTime = Time.time;
        }
        
        // Appliquer le zoom fluide si on a du buffer ou si on est en cours de décélération
        if (zoomBuffer != 0f || zoomVelocity != 0f)
        {
            // Calculer la distance actuelle à la surface de la planète
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position) - planetaryArea;
            
            // Calculer la vitesse de zoom
            float zoomSpeed = CalculateZoomSpeed(currentDistance);
            
            // Calculer la vitesse cible basée sur le buffer
            float targetVelocity = zoomBuffer * zoomSpeed;
            
            // Lisser la vitesse vers la cible
            zoomVelocity = Mathf.Lerp(zoomVelocity, targetVelocity, Time.deltaTime * 10f);
            
            // Appliquer le mouvement
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 zoomMovement = directionToTarget * zoomVelocity * Time.deltaTime;
            
            // Vérifier qu'on ne va pas trop près de la surface
            float newDistanceToCenter = Vector3.Distance(transform.position + zoomMovement, currentTarget.position);
            float newDistanceToSurface = newDistanceToCenter - planetaryArea;
            if (newDistanceToSurface >= contactBuffer)
            {
                transform.position += zoomMovement;
            }
            
            // Décélération du buffer
            if (Time.time - lastZoomTime > 0.1f) // Si pas de nouveau cran depuis 0.1s
            {
                zoomBuffer = Mathf.Lerp(zoomBuffer, 0f, Time.deltaTime * zoomDeceleration);
            }
            
            // Décélération de la vitesse
            if (Mathf.Abs(zoomBuffer) < 0.01f)
            {
                zoomVelocity = Mathf.Lerp(zoomVelocity, 0f, Time.deltaTime * zoomDeceleration);
            }
        }
    }
    
    void HandleDirectZoom(float scroll)
    {
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // Calculer la distance actuelle à la surface de la planète
            float currentDistance = Vector3.Distance(transform.position, currentTarget.position) - planetaryArea;
            
            // Calculer la vitesse de zoom exponentielle
            float zoomSpeed = CalculateZoomSpeed(currentDistance);
            
            // Appliquer le zoom
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 zoomMovement = directionToTarget * scroll * zoomSpeed * Time.deltaTime;
            
            // Vérifier qu'on ne va pas trop près de la surface
            float newDistanceToCenter = Vector3.Distance(transform.position + zoomMovement, currentTarget.position);
            float newDistanceToSurface = newDistanceToCenter - planetaryArea;
            if (newDistanceToSurface >= contactBuffer)
            {
                transform.position += zoomMovement;
            }
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
        float t = 1f -(Mathf.InverseLerp(planetaryArea / 2f, 0f, currentDistance));
        float ActualzoomSpeed = Mathf.Lerp(1f, 350f, t);
        Debug.Log($"ZoomSpeed: {ActualzoomSpeed} / currentDistance:{currentDistance} / planetaryArea:{planetaryArea} t:{t}");
        return ActualzoomSpeed;
    }
    
    void UpdateInnerSphere()
    {
        if (!enableInnerSphere || currentTarget == null) return;
        
        // Calculer la distance actuelle à la surface de la planète
        float currentDistance = Vector3.Distance(transform.position, currentTarget.position) - planetaryArea;
        
        // Calculer le rayon de la sphère intérieure
        float targetSphereRadius = CalculateInnerSphereRadius(currentDistance);
        
        // Mise à jour immédiate du rayon (pas de délai)
        currentSphereRadius = targetSphereRadius;
        
        // Mettre à jour le centre de la sphère (centre de la planète)
        sphereCenter = currentTarget.position;
        
        // Calculer le point cible sur la sphère
        UpdateSphereTargetPoint();
        
        // Appliquer la rotation (toujours, même si la sphère est petite)
        transform.rotation = CalculateInnerSphereRotation(currentDistance);
    }
    
    float CalculateInnerSphereRadius(float currentDistance)
    {
        if (currentDistance >= sphereStartDistance)
        {
            // Sphère de taille 0 quand on est loin
            return 0f;
        }
        else
        {
            // Calculer le facteur de transition (0 = très près, 1 = loin)
            float t = Mathf.InverseLerp(0f, sphereStartDistance, currentDistance);
            
            // Taille maximale de la sphère (pourcentage du rayon de la planète)
            float maxSphereRadius = planetaryArea * sphereMaxSizePercent;
            
            // Interpoler entre 0 et la taille maximale
            return Mathf.Lerp(maxSphereRadius, 0f, t);
        }
    }
    
    void UpdateSphereTargetPoint()
    {
        if (currentSphereRadius <= 0.01f)
        {
            // Si la sphère est trop petite, utiliser le centre de la planète
            sphereTargetPoint = sphereCenter;
            return;
        }
        
        // Calculer la direction de la caméra vers le centre de la sphère
        Vector3 cameraToCenter = (sphereCenter - transform.position).normalized;
        
        if (lockToSphereTop || !isManualRotation)
        {
            // Pour une vraie tangente, nous devons calculer le point sur la sphère
            // qui est perpendiculaire à la direction caméra-centre
            // Utiliser la direction "vers le haut" de la caméra comme référence
            Vector3 cameraUp = transform.up;
            
            // Projeter le vecteur "up" de la caméra sur le plan perpendiculaire à cameraToCenter
            Vector3 projectedUp = Vector3.ProjectOnPlane(cameraUp, cameraToCenter).normalized;
            
            // Si la projection est trop petite, utiliser une direction par défaut
            if (projectedUp.magnitude < 0.1f)
            {
                // Utiliser une direction perpendiculaire arbitraire
                Vector3 right = Vector3.Cross(cameraToCenter, Vector3.up);
                if (right.magnitude < 0.1f)
                    right = Vector3.Cross(cameraToCenter, Vector3.forward);
                projectedUp = right.normalized;
            }
            
            // Le point cible est sur la sphère, perpendiculaire à la direction vers le centre
            sphereTargetPoint = sphereCenter + projectedUp * currentSphereRadius;
        }
        else
        {
            // Orientation manuelle - utiliser l'offset de rotation
            Vector3 rotatedDirection = manualRotationOffset * cameraToCenter;
            sphereTargetPoint = sphereCenter + rotatedDirection * currentSphereRadius;
        }
    }
    
    Quaternion CalculateInnerSphereRotation(float currentDistance)
    {
        // Si la sphère est trop petite, utiliser l'ancien système mais seulement si enableInnerSphere est désactivé
        if (currentSphereRadius <= 0.01f && !enableInnerSphere)
        {
            return CalculateRotation(currentDistance);
        }
        
        // Si la sphère est petite mais le système est activé, utiliser une transition douce
        if (currentSphereRadius <= 0.01f)
        {
            // Utiliser l'ancien système comme fallback mais avec une transition douce
            Quaternion oldSystemRotation = CalculateRotation(currentDistance);
            return Quaternion.Lerp(transform.rotation, oldSystemRotation, Time.deltaTime * 2f);
        }
        
        // Calculer la direction vers le point cible sur la sphère
        Vector3 directionToTarget = (sphereTargetPoint - transform.position).normalized;
        
        if (directionToTarget == Vector3.zero)
        {
            return transform.rotation; // Garder la rotation actuelle si pas de direction valide
        }
        
        // Créer la rotation pour regarder vers le point cible
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        // Si on est en rotation manuelle, appliquer l'offset
        if (isManualRotation && !lockToSphereTop)
        {
            targetRotation = targetRotation * manualRotationOffset;
        }
        
        // Vérifier si la rotation change trop brusquement pour éviter le dédoublement
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        
        // Si l'angle de changement est trop important, utiliser une interpolation plus lente
        float adjustedSpeed = rotationTransitionSpeed;
        if (angleDifference > 90f)
        {
            adjustedSpeed = rotationTransitionSpeed * 0.3f; // Ralentir pour les gros changements
        }
        else if (angleDifference > 45f)
        {
            adjustedSpeed = rotationTransitionSpeed * 0.6f; // Ralentir modérément
        }
        
        // Utiliser une interpolation douce pour éviter les saccades
        return Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * adjustedSpeed);
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
        
        // Réinitialiser les variables de la sphère intérieure
        currentSphereRadius = 0f;
        sphereCenter = Vector3.zero;
        sphereTargetPoint = Vector3.zero;
        manualRotationOffset = Quaternion.identity;
        isManualRotation = false;
        
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
        
        // Dessiner la sphère intérieure si activée
        if (enableInnerSphere && currentTarget != null && currentSphereRadius > 0.01f)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sphereCenter, currentSphereRadius);
            
            // Dessiner le point cible sur la sphère
            if (sphereTargetPoint != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(sphereTargetPoint, 0.1f);
                
                // Dessiner une ligne vers le point cible
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, sphereTargetPoint);
            }
        }
    }
}
