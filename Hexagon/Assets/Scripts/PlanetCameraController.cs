using UnityEngine;

namespace HexasphereProcedural {
    
    /// <summary>
    /// Contrôleur de caméra pour tourner autour d'une planète sélectionnée
    /// </summary>
    public class PlanetCameraController : MonoBehaviour {
        
        [Header("🎯 Planète Cible")]
        [SerializeField] public Transform targetPlanet;
        [SerializeField] public bool autoFindPlanet = true;
        
        [Header("📷 Configuration Caméra")]
        [SerializeField] public float rotationSpeed = 2f;
        [SerializeField] public float zoomSpeed = 20f;
        [SerializeField] public float minDistance = 0.5f;
        [SerializeField] public float maxDistance = 500f;
        [SerializeField] public float defaultDistance = 3f;
        
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
        
        [Header("🎨 Interface")]
        [SerializeField] public bool showDebugInfo = true;
        
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
            RaycastHit hit;
            
            // Chercher spécifiquement les planètes
            HexaAstralithPlanet[] planets = FindObjectsByType<HexaAstralithPlanet>(FindObjectsSortMode.None);
            
            foreach (HexaAstralithPlanet planet in planets) 
            {
                
                // Essayer le raycast avec une distance plus grande
                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    
                    // Vérifier si c'est une planète (par nom, tag ou composant)
                    bool isPlanet = false;
                    
                    // Vérifier par tag
                    if (hit.collider.CompareTag("Planet")) {
                        isPlanet = true;
                    }
                    
                    // Vérifier par nom
                    if (hit.collider.name.Contains("Planet") || hit.collider.name.Contains("HexaAstralith")) {
                        isPlanet = true;
                    }
                    
                    // Vérifier par composant HexaAstralithPlanet
                    if (hit.collider.GetComponent<HexaAstralithPlanet>() != null) {
                        isPlanet = true;
                    }
                    
                    if (isPlanet) {
                        // Vérifier si c'est la même planète déjà sélectionnée
                        if (hasSelectedPlanet && targetPlanet != null && targetPlanet == hit.collider.transform) {
                            return true; // Retourner true mais sans changer la configuration
                        }
                        
                        SetTargetPlanet(hit.collider.transform);
                        return true; // Planète sélectionnée
                    } else {
                    }
                } 
                else 
                {
                    
                    // Essayer de sélectionner la planète la plus proche si le raycast échoue
                    if (planets.Length > 0) {
                        Transform closestPlanet = planets[0].transform;
                        float closestDistance = Vector3.Distance(transform.position, closestPlanet.position);
                        
                        for (int i = 1; i < planets.Length; i++) {
                            float distance = Vector3.Distance(transform.position, planets[i].transform.position);
                            if (distance < closestDistance) {
                                closestDistance = distance;
                                closestPlanet = planets[i].transform;
                            }
                        }
                        
                        // Vérifier si c'est la même planète déjà sélectionnée
                        if (hasSelectedPlanet && targetPlanet != null && targetPlanet == closestPlanet) {
                            return true; // Retourner true mais sans changer la configuration
                        }
                        
                        SetTargetPlanet(closestPlanet);
                        return true;
                    }
                }
            }
            
            return false; // Aucune planète sélectionnée
        }
        
        void SetTargetPlanet(Transform planet) {
            // Vérifier si c'est la même planète déjà sélectionnée
            if (hasSelectedPlanet && targetPlanet != null && targetPlanet == planet) {
                return; // Ne pas reset le focus si c'est la même planète
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
            
            // Utiliser la position de la planète comme centre
            planetCenter = targetPlanet.position;
            
            // Estimer le rayon de la planète
            Renderer renderer = targetPlanet.GetComponent<Renderer>();
            if (renderer != null) {
                planetRadius = renderer.bounds.size.magnitude / 2f;
            } else {
                // Rayon par défaut basé sur l'échelle
                planetRadius = targetPlanet.localScale.magnitude / 2f;
            }
            
            // Ajuster la distance minimale et maximale basée sur la taille de la planète
            minDistance = planetRadius * 1.5f;
            maxDistance = planetRadius * 5f;
            defaultDistance = planetRadius * 3f;
            
            // Ne pas changer la distance actuelle si elle est déjà définie
            if (currentDistance == 0f) {
                currentDistance = defaultDistance;
                targetDistance = defaultDistance;
            }
            // Sinon, garder la distance actuelle
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
            isFreeLooking = false;
        }
        
        // Méthodes de drag supprimées - remplacées par l'orientation libre
        
        void UpdateZoom(float scroll) {
            if (!hasSelectedPlanet) return;
            
            targetDistance -= scroll * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        
        void UpdateCamera() {
            // Si pas de planète sélectionnée, laisser la caméra libre
            if (!hasSelectedPlanet || targetPlanet == null) {
                return;
            }
            
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
                // Mode orientation libre : pas de changement de position, seulement rotation
                transform.rotation = freeLookRotation;
            } else {
                // Mode orbite ou normal : calculer la position autour de la planète
                Vector3 direction = Quaternion.Euler(currentRotation) * Vector3.back;
                Vector3 newPosition = planetCenter + direction * currentDistance;
                transform.position = newPosition;
                
                if (isOrbiting) {
                    // Mode orbite : regarder la planète
                    transform.LookAt(planetCenter);
                } else {
                    // Mode normal : regarder la planète
                    transform.LookAt(planetCenter);
                }
            }
            
            // Garder le nord en haut si activé (seulement si pas en orientation libre)
            if (keepNorthUp && !isFreeLooking) {
                KeepNorthUp();
            }
            
            // Log de debug pour la position
            if (showDebugInfo && Time.frameCount % 120 == 0) { // Log toutes les 120 frames
            }
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
            foreach (GameObject obj in allObjects) {
                if (obj.name.Contains("Planet") || obj.name.Contains("HexaAstralith")) {
                    SetTargetPlanet(obj.transform);
                    return;
                }
            }
            
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
        
        void OnGUI() {
            // Interface minimale pour le debug
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 250, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("📷 Caméra Planète", GUI.skin.box);
            GUILayout.Space(5);
            
            // Statut de la planète
            GUILayout.Label($"Planète: {(hasSelectedPlanet ? "✅" : "❌")}");
            if (hasSelectedPlanet && targetPlanet != null) {
                GUILayout.Label($"Nom: {targetPlanet.name}");
                GUILayout.Label($"Distance: {currentDistance:F1}");
            }
            
            GUILayout.Space(5);
            
            // Contrôles
            GUILayout.Label("Contrôles:");
            GUILayout.Label("🖱️ Clic simple: Sélectionner planète");
            GUILayout.Label("🖱️ Clic gauche maintenu: Orientation libre");
            GUILayout.Label("🖱️ Clic droit: Orbite autour");
            GUILayout.Label("🖱️ Molette: Zoom");
            GUILayout.Label("⌨️ R: Reset | F: Focus");
            GUILayout.Label("⌨️ S: Sélection auto");
            
            GUILayout.Space(5);
            
            // Animation
            GUILayout.Label("Animation:");
            smoothCameraTransition = GUILayout.Toggle(smoothCameraTransition, "Transition fluide");
            if (GUILayout.Button("Durée: " + animationDuration.ToString("F1") + "s")) {
                animationDuration = animationDuration >= 2f ? 0.5f : animationDuration + 0.5f;
            }
            if (GUILayout.Button("Range: " + acceptableDistanceRange.ToString("F1"))) {
                acceptableDistanceRange = acceptableDistanceRange >= 5f ? 1f : acceptableDistanceRange + 1f;
            }
            if (GUILayout.Button("Vitesse: " + cameraMoveSpeed.ToString("F1"))) {
                cameraMoveSpeed = cameraMoveSpeed >= 5f ? 1f : cameraMoveSpeed + 1f;
            }
            
            GUILayout.Space(5);
            
            // Debug
            GUILayout.Label("Debug:");
            GUILayout.Label($"Orbite: {(isOrbiting ? "ON" : "OFF")}");
            GUILayout.Label($"FreeLook: {(isFreeLooking ? "ON" : "OFF")}");
            GUILayout.Label($"Clicking: {(isClicking ? "ON" : "OFF")}");
            GUILayout.Label($"Moving: {(isMovingToPlanet ? "ON" : "OFF")}");
            GUILayout.Label($"Pointing: {(isPointingToPlanet ? "ON" : "OFF")}");
            GUILayout.Label($"Planète: {(hasSelectedPlanet ? "Sélectionnée" : "Aucune")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        void OnDrawGizmos() {
            if (!hasSelectedPlanet || targetPlanet == null) return;
            
            // Dessiner la sphère de la planète
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(planetCenter, planetRadius);
            
            // Dessiner la distance actuelle
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(planetCenter, currentDistance);
            
            // Dessiner la ligne vers la planète
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, planetCenter);
        }
    }
}
