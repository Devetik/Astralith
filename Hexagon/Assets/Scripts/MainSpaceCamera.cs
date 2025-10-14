using UnityEngine;

namespace HexasphereProcedural {
    
    /// <summary>
    /// Contrôleur de caméra pour tourner autour d'une planète sélectionnée
    /// </summary>
    public class MainSpaceCamera : MonoBehaviour {
        
        [Header("🎯 Planète Cible")]
        [SerializeField] public Transform targetPlanet;
        [SerializeField] public bool autoFindPlanet = true;
        
        [Header("📷 Configuration Caméra")]
        [SerializeField] public float rotationSpeed = 2f;
        [SerializeField] public float zoomSpeed = 20f;
        [SerializeField] public float maxDistance = 500f;
        [SerializeField] public float defaultDistance = 3f;
        [SerializeField] public float surfaceMargin = 0.1f; // Marge de sécurité par rapport à la surface
        
        [Header("🔄 Rotation")]
        [SerializeField] public bool keepNorthUp = true;
        [SerializeField] public float smoothRotation = 5f;
        [SerializeField] public float smoothZoom = 5f;
        
        [Header("🎮 Contrôles")]
        [SerializeField] public KeyCode resetCameraKey = KeyCode.R;
        [SerializeField] public KeyCode focusPlanetKey = KeyCode.F;
        [SerializeField] public KeyCode orbitAroundKey = KeyCode.Mouse1; // Clic droit pour tourner autour
        [SerializeField] public KeyCode freeLookKey = KeyCode.Mouse2; // Molette pour orientation libre
        
        [Header("🖱️ Détection de Clic")]
        [SerializeField] public float clickMaxDuration = 0.2f;
        [SerializeField] public float clickMaxMovement = 5f;
        
        [Header("🎬 Animation de Caméra")]
        [SerializeField] public float cameraMoveSpeed = 2f;
        [SerializeField] public bool smoothCameraTransition = true;
        [SerializeField] public float animationDuration = 1f; // Durée fixe de 1 seconde
        [SerializeField] public float acceptableDistanceRange = 2f; // Range de distance acceptable
        
        [Header("🌍 Transition Vue Planétaire")]
        [SerializeField] public float spaceTransitionDistance = 20f; // Distance à partir de laquelle on considère être dans l'espace
        [SerializeField] public float northOrientationSpeed = 2f; // Vitesse d'orientation vers le nord
        [SerializeField] public bool enableNorthTransition = true; // Activer/désactiver la transition vers le nord
        [SerializeField] public bool autoExitFreeLook = true; // Sortir automatiquement du mode orientation libre
        [SerializeField] public float maxNorthAngle = 80f; // Angle maximum vers le nord (plus agressif)
        [SerializeField] public bool onlyApplyWhenNotFreeLooking = true; // Appliquer la transition seulement si pas en mode orientation libre
        
        [Header("🎯 Visualisation")]
        [SerializeField] public bool showCameraToPlanetLine = true;
        [SerializeField] public Color lineColor = Color.cyan;
        [SerializeField] public bool showContactGizmo = true;
        [SerializeField] public Color contactGizmoColor = Color.red;
        [SerializeField] public float contactGizmoSize = 0.1f;
        
        [Header("🌍 Debug Vue Planétaire")]
        [SerializeField] public bool showTransitionZone = true;
        [SerializeField] public Color transitionZoneColor = Color.green;
        [SerializeField] public bool showTransitionDebug = true;
        

        
        [Header("📷 État Caméra")]
        [SerializeField] private Vector3 currentRotation;
        [SerializeField] private float currentDistance;
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private float targetDistance;
        
        // Variables privées
        private Camera cam;
        private bool isOrbiting = false; // Tourner autour de la planète
        private bool isFreeLooking = false; // Orientation libre sur place
        private Vector3 lastMousePosition;
        private Vector3 planetCenter;
        private float planetRadius;
        private Vector3 mousePositionOnClick;
        private Quaternion freeLookRotation; // Rotation libre de la caméra
        private float clickStartTime;
        private bool isClicking = false;
        
        // Animation de caméra
        private bool isMovingToPlanet = false;
        private bool isPointingToPlanet = false;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 targetPosition;
        private Quaternion targetRotationAnimation;
        private float moveStartTime;
        private float moveDuration;
        
        // États
        private bool hasSelectedPlanet = false;
        
        // Variables pour la visualisation
        private Vector3 contactPoint;
        private bool hasContactPoint = false;
        private float dynamicMinDistance = 0.5f; // Distance minimale calculée dynamiquement
        
        // Variables pour la transition vue planétaire
        private Quaternion targetNorthRotation;
        
        void Start() {
            cam = GetComponent<Camera>();
            if (cam == null) {
                cam = gameObject.AddComponent<Camera>();
            }
            
            // Initialiser les valeurs seulement si elles n'ont pas été définies dans l'inspector
            if (currentDistance == 0f) {
                currentDistance = defaultDistance;
            }
            if (targetDistance == 0f) {
                targetDistance = defaultDistance;
            }
            if (currentRotation == Vector3.zero) {
                currentRotation = transform.eulerAngles;
            }
            if (targetRotation == Vector3.zero) {
                targetRotation = currentRotation;
            }
            
            // Positionner la caméra initialement
            SetupInitialCamera();
        }
        
        void Update() {
            HandleInput();
            UpdateCamera();
            UpdateContactPoint();
            
            // Test de sélection automatique si pas de planète sélectionnée
            if (!hasSelectedPlanet && Input.GetKeyDown(KeyCode.S)) {
                FindNearestPlanet();
            }
        }
        
        void HandleInput() {
            // Détection de clic gauche simple
            if (Input.GetMouseButtonDown(0)) {
                mousePositionOnClick = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                clickStartTime = Time.time;
                isClicking = true;
            }
            
            // Vérifier si c'est un clic simple (pas un mouvement)
            if (Input.GetMouseButtonUp(0) && isClicking) {
                float clickDuration = Time.time - clickStartTime;
                Vector3 clickMovement = Input.mousePosition - mousePositionOnClick;
                
                
                // Vérifier si c'est un clic simple
                if (clickDuration <= clickMaxDuration && clickMovement.magnitude <= clickMaxMovement) {
                    SelectPlanetAtMouse();
                }
                
                isClicking = false;
            }
            
            // Reset caméra
            if (Input.GetKeyDown(resetCameraKey)) {
                ResetCamera();
            }
            
            // Focus sur planète
            if (Input.GetKeyDown(focusPlanetKey)) {
                FocusOnPlanet();
            }
            
            // Tourner autour de la planète (clic droit)
            if (Input.GetMouseButtonDown(1)) {
                StartOrbiting();
            }
            if (Input.GetMouseButton(1)) {
                UpdateOrbiting();
            }
            if (Input.GetMouseButtonUp(1)) {
                StopOrbiting();
            }
            
            // Orientation libre sur place (clic gauche maintenu) - seulement si pas un clic simple
            if (Input.GetMouseButton(0) && hasSelectedPlanet && !isClicking) {
                if (!isFreeLooking) {
                    StartFreeLook();
                }
                UpdateFreeLook();
            }
            
            // Détecter le début d'un mouvement (clic gauche maintenu après un clic simple)
            if (Input.GetMouseButton(0) && hasSelectedPlanet && isClicking) {
                float clickDuration = Time.time - clickStartTime;
                Vector3 clickMovement = Input.mousePosition - mousePositionOnClick;
                
                // Si c'est un mouvement (pas un clic simple)
                if (clickDuration > clickMaxDuration || clickMovement.magnitude > clickMaxMovement) {
                    isClicking = false; // Arrêter la détection de clic
                    if (!isFreeLooking) {
                        StartFreeLook();
                    }
                }
            }
            
            if (Input.GetMouseButtonUp(0) && isFreeLooking) {
                StopFreeLook();
            }
            
            // Zoom avec molette
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f) {
                UpdateZoom(scroll);
            }
            
            // L'orientation libre avec clic gauche est gérée plus haut
        }
        
        bool SelectPlanetAtMouse() 
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            
            // Trier par distance (le plus proche en premier)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            // Chercher le premier objet avec un tag de focus
            foreach (RaycastHit hit in hits) {
                string objectTag = hit.collider.tag;
                
                // Vérifier si l'objet a un des tags de focus
                string[] focusTags = {"Planet", "Moon", "Sun"};
                foreach (string focusTag in focusTags) {
                    if (objectTag == focusTag) {
                        // Vérifier si c'est la même planète déjà sélectionnée
                        if (hasSelectedPlanet && targetPlanet != null && targetPlanet == hit.collider.transform) {
                            return true; // Retourner true mais sans changer la configuration
                        }
                        
                        SetTargetPlanet(hit.collider.transform);
                        return true; // Objet focusable sélectionné
                    }
                }
            }
            
            // Si aucun objet trouvé par raycast, essayer de trouver par proximité
            return FindNearestPlanetByProximity();
        }
        
        bool FindNearestPlanetByProximity() {
            // Chercher tous les objets avec les tags de focus
            string[] focusTags = {"Planet", "Moon", "Sun"};
            Transform nearestObject = null;
            float nearestDistance = Mathf.Infinity;
            
            foreach (string tag in focusTags) {
                GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
                
                foreach (GameObject obj in objects) {
                    // Calculer la distance à l'écran
                    Vector3 screenPos = cam.WorldToScreenPoint(obj.transform.position);
                    
                    if (screenPos.z > 0) { // Devant la caméra
                        float screenDistance = Vector2.Distance(Input.mousePosition, new Vector2(screenPos.x, screenPos.y));
                        
                        if (screenDistance < nearestDistance && screenDistance < 200f) { // Dans un rayon de 200 pixels
                            nearestDistance = screenDistance;
                            nearestObject = obj.transform;
                        }
                    }
                }
            }
            
            if (nearestObject != null) {
                SetTargetPlanet(nearestObject);
                return true;
            }
            
            return false; // Aucun objet focusable trouvé
        }
        
        void SetTargetPlanet(Transform planet) {
            // Vérifier si c'est la même planète déjà sélectionnée
            if (hasSelectedPlanet && targetPlanet != null && targetPlanet == planet) {
                return; // Ne pas reset le focus si c'est la même planète
            }
            
            // Si c'est un chunk d'HexasphereFill, cibler l'objet parent
            if (planet.name.Contains("Chunk")) {
                Transform parent = planet.parent;
                if (parent != null && parent.GetComponent<HexasphereFill>() != null) {
                    planet = parent;
                }
            }
            
            targetPlanet = planet;
            hasSelectedPlanet = true;
            
            // Calculer le centre et le rayon de la planète
            CalculatePlanetInfo();
            
            // Nouvelle logique : pointer vers la planète et se déplacer si nécessaire
            StartPointingToPlanet();
            
        }
        
        void CalculatePlanetInfo() {
            if (targetPlanet == null) return;
            
            // Simple : utiliser directement la position de l'objet
            planetCenter = targetPlanet.position;
            
            // Pour HexasphereFill, utiliser le radius du script
            HexasphereFill hexasphereFill = targetPlanet.GetComponent<HexasphereFill>();
            if (hexasphereFill != null) {
                planetRadius = hexasphereFill.radius;
            } else {
                // Pour les autres planètes, estimer le rayon
                Renderer renderer = targetPlanet.GetComponent<Renderer>();
                if (renderer != null && renderer.bounds.size.magnitude > 0) {
                    planetRadius = renderer.bounds.size.magnitude / 2f;
                } else {
                    planetRadius = Mathf.Max(targetPlanet.localScale.magnitude / 2f, 1f);
                }
            }
            
            // Ajuster la distance maximale basée sur la taille de la planète
            maxDistance = planetRadius * 5f;
            defaultDistance = planetRadius * 3f;
            
            // Ne pas changer la distance actuelle si elle est déjà définie
            if (currentDistance == 0f) {
                currentDistance = defaultDistance;
                targetDistance = defaultDistance;
            }         
        }
        
        void SetupCameraAroundPlanet() {
            if (targetPlanet == null) return;
            
            // Positionner la caméra à une distance par défaut
            Vector3 direction = (transform.position - planetCenter).normalized;
            if (direction == Vector3.zero) {
                direction = Vector3.back;
            }
            
            transform.position = planetCenter + direction * currentDistance;
            transform.LookAt(planetCenter);
            
            // Garder le nord en haut si activé
            if (keepNorthUp) {
                KeepNorthUp();
            }
        }
        
        void StartPointingToPlanet() {
            if (targetPlanet == null) return;
            
            // Pour HexasphereFill, éviter l'animation qui peut causer des problèmes
            HexasphereFill hexasphereFill = targetPlanet.GetComponent<HexasphereFill>();
            if (hexasphereFill != null) {
                
                // Pas d'animation pour HexasphereFill, juste pointer directement
                Vector3 directionToPlanet = (planetCenter - transform.position).normalized;
                if (directionToPlanet != Vector3.zero) {
                    transform.rotation = Quaternion.LookRotation(directionToPlanet);
                }
                
                // S'assurer que la sélection persiste
                hasSelectedPlanet = true;
                return;
            }
            
            // Pour les autres planètes, utiliser l'animation normale
            // Sauvegarder la position et rotation actuelles AVANT tout calcul
            startPosition = transform.position;
            startRotation = transform.rotation;
            
            // Calculer la distance actuelle à la planète
            float currentDistanceToPlanet = Vector3.Distance(transform.position, planetCenter);
            
            // Rotation cible : regarder directement la planète depuis la position actuelle
            targetRotationAnimation = Quaternion.LookRotation(planetCenter - transform.position);
            
            // Vérifier si on doit se déplacer
            if (currentDistanceToPlanet > acceptableDistanceRange) {
                // Calculer la position cible à une distance acceptable
                Vector3 directionToPlanet = (planetCenter - transform.position).normalized;
                targetPosition = planetCenter - directionToPlanet * acceptableDistanceRange;
                
                // Démarrer l'animation de déplacement
                moveDuration = animationDuration;
                moveStartTime = Time.time;
                isMovingToPlanet = true;
            } else {
                // Juste pointer vers la planète, pas de déplacement
                isPointingToPlanet = true;
                moveDuration = animationDuration * 0.5f; // Plus rapide pour le pointage
                moveStartTime = Time.time;
            }
        }
        
        void StartCameraMoveToPlanet() {
            if (targetPlanet == null) return;
            
            
            // Sauvegarder la position et rotation actuelles
            startPosition = transform.position;
            startRotation = transform.rotation;
            
            // Calculer la direction de la ligne droite vers la planète
            Vector3 directionToPlanet = (planetCenter - startPosition).normalized;
            
            // Position cible : garder la même direction mais à la distance appropriée
            targetPosition = planetCenter - directionToPlanet * currentDistance;
            
            // Rotation cible : regarder la planète depuis la nouvelle position
            targetRotationAnimation = Quaternion.LookRotation(planetCenter - targetPosition);
            
            // Utiliser la durée fixe au lieu de calculer basé sur la distance
            moveDuration = animationDuration;
            moveStartTime = Time.time;
            
            isMovingToPlanet = true;
            
        }
        
        void UpdateCameraMoveToPlanet() {
            if ((!isMovingToPlanet && !isPointingToPlanet) || targetPlanet == null) return;
            
            float elapsed = Time.time - moveStartTime;
            float progress = elapsed / moveDuration;
            
            if (progress >= 1f) {
                // Animation terminée
                isMovingToPlanet = false;
                isPointingToPlanet = false;
                // Ne pas appeler SetupCameraAroundPlanet() pour éviter le saut d'angle
                return;
            }
            
            // Interpolation fluide
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Appliquer l'animation de position (seulement si on se déplace)
            if (isMovingToPlanet) {
                transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            }
            // Si isPointingToPlanet, on ne bouge pas la position
            
            // Préserver le zoom actuel - ne pas changer la distance
            // La distance reste celle de la position de départ
            
            // Appliquer l'animation de rotation (sauf si en orientation libre)
            if (isFreeLooking) {
                // Garder la rotation libre pendant l'animation
                transform.rotation = freeLookRotation;
            } else {
                // Animation normale de rotation
                transform.rotation = Quaternion.Lerp(startRotation, targetRotationAnimation, smoothProgress);
            }
            
            // S'assurer que la rotation finale est bien appliquée à la fin
            if (progress >= 0.99f && !isFreeLooking) {
                transform.rotation = targetRotationAnimation;
            }
            
            // Garder le nord en haut si activé
            if (keepNorthUp) {
                KeepNorthUp();
            }
        }
        
        void HandleRotationDuringAnimation() {
            // Permettre l'orientation libre pendant l'animation
            if (Input.GetMouseButton(0) && hasSelectedPlanet) {
                if (!isFreeLooking) {
                    StartFreeLook();
                }
                UpdateFreeLook();
            }
            if (Input.GetMouseButtonUp(0) && isFreeLooking) {
                StopFreeLook();
            }
        }
        
        // ===== TOURNER AUTOUR DE LA PLANÈTE =====
        void StartOrbiting() {
            isOrbiting = true;
            lastMousePosition = Input.mousePosition;
        }
        
        void UpdateOrbiting() {
            if (!isOrbiting || !hasSelectedPlanet) return;
            
            // Mettre à jour le centre à chaque frame (simple)
            planetCenter = targetPlanet.position;
            
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // Rotation horizontale (autour de l'axe Y) - tourner autour de la planète
            float yRotation = mouseDelta.x * rotationSpeed * 0.1f;
            
            // Rotation verticale (autour de l'axe X) - tourner autour de la planète
            float xRotation = -mouseDelta.y * rotationSpeed * 0.1f;
            
            // Appliquer la rotation pour l'orbite
            targetRotation.y += yRotation;
            targetRotation.x += xRotation;
            
            // Limiter la rotation verticale pour éviter le retournement
            targetRotation.x = Mathf.Clamp(targetRotation.x, -80f, 80f);
            
            lastMousePosition = Input.mousePosition;
        }
        
        void StopOrbiting() {
            isOrbiting = false;
        }
        
        // ===== ORIENTATION LIBRE SUR PLACE =====
        void StartFreeLook() {
            if (!hasSelectedPlanet) {
                return;
            }
            
            isFreeLooking = true;
            lastMousePosition = Input.mousePosition;
            freeLookRotation = transform.rotation; // Sauvegarder la rotation actuelle
        }
        
        void UpdateFreeLook() {
            if (!isFreeLooking) {
                return;
            }
            
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // Rotation horizontale (autour de l'axe Y) - orientation libre
            float yRotation = mouseDelta.x * rotationSpeed * 0.1f;
            
            // Rotation verticale (autour de l'axe X) - orientation libre
            float xRotation = -mouseDelta.y * rotationSpeed * 0.1f;
            
            // Appliquer la rotation libre à la caméra
            freeLookRotation *= Quaternion.Euler(xRotation, yRotation, 0f);
                     
            lastMousePosition = Input.mousePosition;
        }
        
        void StopFreeLook() {
            // Vérifier si on est dans la zone de transition
            if (hasSelectedPlanet && targetPlanet != null) {
                float distanceToPlanet = Vector3.Distance(transform.position, planetCenter);
                
                // Si on est dans la zone de transition, conserver l'orientation
                if (distanceToPlanet <= spaceTransitionDistance) {
                    // Ne pas sortir du mode orientation libre dans la zone
                    // La caméra garde son orientation actuelle
                    return;
                }
            }
            
            // Sortir du mode orientation libre seulement si on est en dehors de la zone
            isFreeLooking = false;
        }
        
        // Méthodes de drag supprimées - remplacées par l'orientation libre
        
        void UpdateZoom(float scroll) {
            if (!hasSelectedPlanet) return;
            
            targetDistance -= scroll * zoomSpeed;
            
            // Utiliser la distance minimale dynamique si un point de contact est disponible
            float effectiveMinDistance;
            if (hasContactPoint) {
                effectiveMinDistance = dynamicMinDistance;
            } else {
                // Fallback : utiliser le rayon de la planète + marge de sécurité
                effectiveMinDistance = planetRadius + surfaceMargin;
            }
            
            // Debug pour voir les valeurs
            Debug.Log($"Zoom: hasContactPoint={hasContactPoint}, dynamicMinDistance={dynamicMinDistance}, planetRadius={planetRadius}, effectiveMinDistance={effectiveMinDistance}, targetDistance={targetDistance}");
            
            targetDistance = Mathf.Clamp(targetDistance, effectiveMinDistance, maxDistance);
            
            // Forcer la mise à jour de la position même en mode freeLook
            if (isFreeLooking) {
                // Recalculer la position de la caméra même en mode orientation libre
                Vector3 direction = (transform.position - planetCenter).normalized;
                if (direction != Vector3.zero) {
                    transform.position = planetCenter + direction * currentDistance;
                }
            }
        }
        
        void UpdateCamera() {
            // Si pas de planète sélectionnée, laisser la caméra libre
            if (!hasSelectedPlanet || targetPlanet == null) {
                return;
            }
            
            // Mettre à jour le centre à chaque frame (simple)
            planetCenter = targetPlanet.position;
            
            // Gérer l'animation vers la planète (déplacement ou pointage)
            if (isMovingToPlanet || isPointingToPlanet) {
                UpdateCameraMoveToPlanet();
                // Permettre les contrôles de rotation pendant l'animation
                HandleRotationDuringAnimation();
                return; // Ne pas appliquer les autres contrôles de position pendant l'animation
            }
            
            // Mise à jour de la distance
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothZoom * Time.deltaTime);
            
            // Mise à jour de la rotation
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, smoothRotation * Time.deltaTime);
            
            // Gérer les différents modes de caméra
            if (isFreeLooking) {
                // Mode orientation libre : rotation libre mais position mise à jour
                transform.rotation = freeLookRotation;
                
                // Mettre à jour la position même en mode orientation libre
                Vector3 direction = (transform.position - planetCenter).normalized;
                if (direction != Vector3.zero) {
                    transform.position = planetCenter + direction * currentDistance;
                }
                
                // Gérer la transition vue planétaire même en mode orientation libre
                UpdatePlanetViewTransition();
            } else {
                // Mode orbite ou normal : calculer la position autour de la planète
                Vector3 direction = Quaternion.Euler(currentRotation) * Vector3.back;
                Vector3 newPosition = planetCenter + direction * currentDistance;
                transform.position = newPosition;
                
                // Appliquer la rotation de base (regarder la planète)
                if (isOrbiting) {
                    // Mode orbite : regarder la planète
                    transform.LookAt(planetCenter);
                } else {
                    // Mode normal : regarder la planète
                    transform.LookAt(planetCenter);
                }
                
                // Gérer la transition vue planétaire APRÈS la rotation de base
                UpdatePlanetViewTransition();
            }
            
            // Garder le nord en haut si activé (seulement si pas en orientation libre)
            if (keepNorthUp && !isFreeLooking) {
                KeepNorthUp();
            }

        }
        
        void UpdatePlanetViewTransition() {
            if (!hasSelectedPlanet || targetPlanet == null) return;
            
            // Calculer la distance actuelle à la planète
            float distanceToPlanet = Vector3.Distance(transform.position, planetCenter);
            
            // Debug de la zone
            if (showTransitionDebug && Random.value < 0.1f) {
                Debug.Log($"Zone: distance={distanceToPlanet:F1}, inZone={distanceToPlanet <= spaceTransitionDistance}, isFreeLooking={isFreeLooking}");
            }
            
            // Dans la zone de transition, on ne fait rien de spécial
            // Le FreeLook conserve automatiquement l'orientation grâce à StopFreeLook()
        }
        
        void KeepNorthUp() {
            // S'assurer que l'axe Y de la caméra reste vers le haut
            Vector3 forward = (planetCenter - transform.position).normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;
            
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
        
        void ResetCamera() {
            if (!hasSelectedPlanet) return;
            
            // Reset à la position par défaut
            targetRotation = Vector3.zero;
            targetDistance = defaultDistance;
            
        }
        
        // Méthode pour réinitialiser aux valeurs par défaut
        public void ResetToDefaults() {
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;
            currentRotation = Vector3.zero;
            targetRotation = Vector3.zero;
        }
        
        void FocusOnPlanet() {
            if (!hasSelectedPlanet) return;
            
            // Focus sur la planète avec une rotation douce
            Vector3 direction = (planetCenter - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(direction).eulerAngles;
            
        }
        
        void SetupInitialCamera() {
            // Position initiale de la caméra
            if (targetPlanet != null) {
                CalculatePlanetInfo();
                SetupCameraAroundPlanet();
            } else {
                // Position par défaut
                transform.position = new Vector3(0, 0, -5);
                transform.LookAt(Vector3.zero);
            }
        }
        
        void FindNearestPlanet() {
            // Chercher par nom d'abord (plus fiable)
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            
            // Si pas trouvé par nom, chercher par tag (si le tag existe)
            try {
                GameObject[] planets = GameObject.FindGameObjectsWithTag("Planet");
                if (planets.Length > 0) {
                    SetTargetPlanet(planets[0].transform);
                    return;
                }
            } catch (UnityException) {
                // Le tag "Planet" n'existe pas, continuer sans erreur
            }
            
        }
        

        
        void UpdateContactPoint() {
            if (!hasSelectedPlanet || targetPlanet == null) {
                hasContactPoint = false;
                return;
            }
            
            // Effectuer un raycast depuis la caméra vers la planète
            Vector3 direction = (planetCenter - transform.position).normalized;
            Ray ray = new Ray(transform.position, direction);
            
            // Raycast vers tous les colliders
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            
            // Trier par distance pour trouver le premier contact
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            // Chercher le premier hit avec le tag "Planet" ou qui appartient à la planète ciblée
            foreach (RaycastHit hit in hits) {
                // Vérifier si c'est un objet avec le tag "Planet"
                if (hit.collider.CompareTag("Planet")) {
                    // Vérifier si c'est la planète ciblée ou un de ses chunks
                    if (IsPlanetOrChunk(hit.collider.transform)) {
                        contactPoint = hit.point;
                        hasContactPoint = true;
                        
                        // Calculer la distance minimale dynamiquement
                        // La distance minimale doit être la distance du centre à la surface + marge
                        float distanceFromCenterToSurface = Vector3.Distance(planetCenter, contactPoint);
                        dynamicMinDistance = distanceFromCenterToSurface + surfaceMargin;
                        
                        // Debug pour voir les valeurs
                        Debug.Log($"Contact détecté: distanceFromCenterToSurface={distanceFromCenterToSurface}, surfaceMargin={surfaceMargin}, dynamicMinDistance={dynamicMinDistance}");
                        
                        // S'assurer que la distance minimale n'est pas négative
                        dynamicMinDistance = Mathf.Max(dynamicMinDistance, 0.1f);
                        
                        return;
                    }
                }
            }
            
            // Si aucun contact trouvé, utiliser le point le plus proche sur la sphère
            Vector3 toPlanet = planetCenter - transform.position;
            float distanceToCenter = toPlanet.magnitude;
            if (distanceToCenter > 0) {
                Vector3 directionToPlanet = toPlanet.normalized;
                contactPoint = planetCenter - directionToPlanet * planetRadius;
                hasContactPoint = true;
                
                // Calculer la distance minimale basée sur le rayon de la planète
                dynamicMinDistance = planetRadius + surfaceMargin;
            } else {
                hasContactPoint = false;
            }
        }
        
        bool IsPlanetOrChunk(Transform hitTransform) {
            // Vérifier si c'est la planète ciblée elle-même
            if (hitTransform == targetPlanet) {
                return true;
            }
            
            // Vérifier si c'est un enfant de la planète ciblée (chunk)
            if (hitTransform.IsChildOf(targetPlanet)) {
                return true;
            }
            
            // Vérifier si c'est un parent de la planète ciblée
            if (targetPlanet.IsChildOf(hitTransform)) {
                return true;
            }
            
            return false;
        }
        
        
        void OnDrawGizmos() {
            if (!hasSelectedPlanet || targetPlanet == null) return;
            
            // Dessiner la sphère de la planète
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(planetCenter, planetRadius);
            
            // Dessiner la distance actuelle
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(planetCenter, currentDistance);
            
            // Dessiner la ligne entre la caméra et la planète si activé
            if (showCameraToPlanetLine) {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(transform.position, planetCenter);
            }
            
            // Dessiner le gizmo de contact si activé et disponible
            if (showContactGizmo && hasContactPoint) {
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
            
            // Debug zone de transition
            if (showTransitionZone) {
                Gizmos.color = transitionZoneColor;
                Gizmos.DrawWireSphere(planetCenter, spaceTransitionDistance);
                
                // Dessiner le pôle nord
                Gizmos.color = Color.yellow;
                Vector3 northPole = planetCenter + Vector3.up * planetRadius * 2f;
                Gizmos.DrawLine(planetCenter, northPole);
                Gizmos.DrawWireSphere(northPole, planetRadius * 0.1f);
            }
        }
    }
}
