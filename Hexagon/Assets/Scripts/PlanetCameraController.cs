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
        [SerializeField] public float zoomSpeed = 2f;
        [SerializeField] public float minDistance = 1.5f;
        [SerializeField] public float maxDistance = 10f;
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
        
        [Header("🎨 Interface")]
        [SerializeField] public bool showDebugInfo = true;
        
        // Variables privées
        private Camera cam;
        private Vector3 currentRotation;
        private float currentDistance;
        private Vector3 targetRotation;
        private float targetDistance;
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
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 targetPosition;
        private Quaternion targetRotationAnimation;
        private float moveStartTime;
        private float moveDuration;
        
        // États
        private bool hasSelectedPlanet = false;
        
        void Start() {
            Debug.Log("🚀 DÉMARRAGE du contrôleur de caméra");
            
            cam = GetComponent<Camera>();
            if (cam == null) {
                Debug.Log("📷 Ajout d'un composant Camera");
                cam = gameObject.AddComponent<Camera>();
            }
            
            Debug.Log($"📷 Caméra trouvée: {cam.name}");
            
            // Initialiser les valeurs
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;
            currentRotation = transform.eulerAngles;
            targetRotation = currentRotation;
            
            Debug.Log($"📊 Valeurs initiales - Distance: {currentDistance}, Rotation: {currentRotation}");
            
            // Ne plus chercher automatiquement une planète
            Debug.Log("🔍 Pas de recherche automatique de planète");
            
            // Positionner la caméra initialement
            SetupInitialCamera();
            
            Debug.Log("✅ Contrôleur de caméra initialisé");
        }
        
        void Update() {
            HandleInput();
            UpdateCamera();
            
            // Test de sélection automatique si pas de planète sélectionnée
            if (!hasSelectedPlanet && Input.GetKeyDown(KeyCode.S)) {
                Debug.Log("🔍 Sélection automatique de planète (touche S)");
                FindNearestPlanet();
            }
        }
        
        void HandleInput() {
            // Log des inputs pour debug
            if (Input.GetMouseButtonDown(0)) {
                Debug.Log("🖱️ CLIC GAUCHE DÉTECTÉ");
            }
            if (Input.GetMouseButtonDown(1)) {
                Debug.Log("🖱️ CLIC DROIT DÉTECTÉ");
            }
            
            // Détection de clic gauche simple
            if (Input.GetMouseButtonDown(0)) {
                Debug.Log("🖱️ Début clic gauche...");
                mousePositionOnClick = Input.mousePosition;
                lastMousePosition = Input.mousePosition;
                clickStartTime = Time.time;
                isClicking = true;
            }
            
            // Vérifier si c'est un clic simple (pas un mouvement)
            if (Input.GetMouseButtonUp(0) && isClicking) {
                float clickDuration = Time.time - clickStartTime;
                Vector3 clickMovement = Input.mousePosition - mousePositionOnClick;
                
                Debug.Log($"🖱️ Fin clic - Durée: {clickDuration:F2}s, Mouvement: {clickMovement.magnitude:F1}px");
                
                // Vérifier si c'est un clic simple
                if (clickDuration <= clickMaxDuration && clickMovement.magnitude <= clickMaxMovement) {
                    Debug.Log("🖱️ Clic simple détecté - recherche de planète");
                    SelectPlanetAtMouse();
                } else {
                    Debug.Log("🖱️ Mouvement détecté - pas de sélection de planète");
                }
                
                isClicking = false;
            }
            
            // Reset caméra
            if (Input.GetKeyDown(resetCameraKey)) {
                Debug.Log("⌨️ RESET CAMÉRA");
                ResetCamera();
            }
            
            // Focus sur planète
            if (Input.GetKeyDown(focusPlanetKey)) {
                Debug.Log("⌨️ FOCUS PLANÈTE");
                FocusOnPlanet();
            }
            
            // Tourner autour de la planète (clic droit)
            if (Input.GetMouseButtonDown(1)) {
                Debug.Log("🖱️ Début orbite autour de la planète (clic droit)");
                StartOrbiting();
            }
            if (Input.GetMouseButton(1)) {
                UpdateOrbiting();
            }
            if (Input.GetMouseButtonUp(1)) {
                Debug.Log("🖱️ Fin orbite autour de la planète (clic droit)");
                StopOrbiting();
            }
            
            // Orientation libre sur place (clic gauche maintenu) - seulement si pas un clic simple
            if (Input.GetMouseButton(0) && hasSelectedPlanet && !isClicking) {
                if (!isFreeLooking) {
                    Debug.Log("🖱️ Début orientation libre (clic gauche maintenu)");
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
                    Debug.Log($"🖱️ Mouvement détecté - début orientation libre (durée: {clickDuration:F2}s, mouvement: {clickMovement.magnitude:F1}px)");
                    isClicking = false; // Arrêter la détection de clic
                    if (!isFreeLooking) {
                        StartFreeLook();
                    }
                }
            }
            
            if (Input.GetMouseButtonUp(0) && isFreeLooking) {
                Debug.Log("🖱️ Fin orientation libre (clic gauche relâché)");
                StopFreeLook();
            }
            
            // Zoom avec molette
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f) {
                Debug.Log($"🖱️ ZOOM: {scroll}");
                UpdateZoom(scroll);
            }
            
            // L'orientation libre avec clic gauche est gérée plus haut
        }
        
        bool SelectPlanetAtMouse() {
            Debug.Log("🔍 Recherche de planète au clic...");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            Debug.Log($"📷 Rayon depuis: {ray.origin}, direction: {ray.direction}");
            Debug.Log($"🖱️ Position souris: {Input.mousePosition}");
            
            // Vérifier d'abord s'il y a des objets dans la scène
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            Debug.Log($"🌍 Nombre d'objets dans la scène: {allObjects.Length}");
            
            // Chercher spécifiquement les planètes
            HexaAstralithPlanet[] planets = FindObjectsByType<HexaAstralithPlanet>(FindObjectsSortMode.None);
            Debug.Log($"🌍 Nombre de planètes HexaAstralith: {planets.Length}");
            
            foreach (HexaAstralithPlanet planet in planets) {
                Debug.Log($"🌍 Planète trouvée: {planet.name}, position: {planet.transform.position}, tag: {planet.gameObject.tag}");
            }
            
            // Essayer le raycast avec une distance plus grande
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                Debug.Log($"🎯 Objet touché: {hit.collider.name}, tag: {hit.collider.tag}, distance: {hit.distance}");
                
                // Vérifier si c'est une planète (par nom, tag ou composant)
                bool isPlanet = false;
                
                // Vérifier par tag
                if (hit.collider.CompareTag("Planet")) {
                    Debug.Log("✅ Planète détectée par TAG");
                    isPlanet = true;
                }
                
                // Vérifier par nom
                if (hit.collider.name.Contains("Planet") || hit.collider.name.Contains("HexaAstralith")) {
                    Debug.Log("✅ Planète détectée par NOM");
                    isPlanet = true;
                }
                
                // Vérifier par composant HexaAstralithPlanet
                if (hit.collider.GetComponent<HexaAstralithPlanet>() != null) {
                    Debug.Log("✅ Planète détectée par COMPOSANT");
                    isPlanet = true;
                }
                
                if (isPlanet) {
                    // Vérifier si c'est la même planète déjà sélectionnée
                    if (hasSelectedPlanet && targetPlanet != null && targetPlanet == hit.collider.transform) {
                        Debug.Log($"🔄 Même planète cliquée: {hit.collider.name} - Aucun changement");
                        return true; // Retourner true mais sans changer la configuration
                    }
                    
                    SetTargetPlanet(hit.collider.transform);
                    Debug.Log($"🎯 Planète sélectionnée: {hit.collider.name}");
                    return true; // Planète sélectionnée
                } else {
                    Debug.Log("❌ Objet touché mais pas une planète");
                }
            } else {
                Debug.Log("❌ Aucun objet touché par le rayon");
                
                // Essayer de sélectionner la planète la plus proche si le raycast échoue
                if (planets.Length > 0) {
                    Debug.Log("🔄 Tentative de sélection de la planète la plus proche...");
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
                        Debug.Log($"🔄 Même planète la plus proche: {closestPlanet.name} - Aucun changement");
                        return true; // Retourner true mais sans changer la configuration
                    }
                    
                    Debug.Log($"🎯 Sélection de la planète la plus proche: {closestPlanet.name} (distance: {closestDistance:F2})");
                    SetTargetPlanet(closestPlanet);
                    return true;
                }
            }
            
            return false; // Aucune planète sélectionnée
        }
        
        void SetTargetPlanet(Transform planet) {
            // Vérifier si c'est la même planète déjà sélectionnée
            if (hasSelectedPlanet && targetPlanet != null && targetPlanet == planet) {
                Debug.Log($"🔄 Même planète sélectionnée: {planet.name} - Aucun changement");
                return; // Ne pas reset le focus si c'est la même planète
            }
            
            Debug.Log($"🎯 Configuration de la planète cible: {planet.name}");
            targetPlanet = planet;
            hasSelectedPlanet = true;
            
            // Calculer le centre et le rayon de la planète
            CalculatePlanetInfo();
            
            // Démarrer l'animation vers la planète
            if (smoothCameraTransition) {
                StartCameraMoveToPlanet();
            } else {
                // Positionnement instantané
                SetupCameraAroundPlanet();
            }
            
            Debug.Log($"✅ Planète configurée - Centre: {planetCenter}, Rayon: {planetRadius}");
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
            
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;
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
        
        void StartCameraMoveToPlanet() {
            if (targetPlanet == null) return;
            
            Debug.Log("🎬 Début animation vers la planète");
            
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
            
            Debug.Log($"🎬 Animation: Direction={directionToPlanet:F2}, Durée={moveDuration:F2}s, Position cible={targetPosition:F2}");
        }
        
        void UpdateCameraMoveToPlanet() {
            if (!isMovingToPlanet || targetPlanet == null) return;
            
            float elapsed = Time.time - moveStartTime;
            float progress = elapsed / moveDuration;
            
            if (progress >= 1f) {
                // Animation terminée
                isMovingToPlanet = false;
                SetupCameraAroundPlanet();
                Debug.Log("🎬 Animation terminée - caméra positionnée");
                return;
            }
            
            // Interpolation fluide
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Appliquer l'animation de position
            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothProgress);
            
            // Appliquer l'animation de rotation (sauf si en orientation libre)
            if (isFreeLooking) {
                // Garder la rotation libre pendant l'animation
                transform.rotation = freeLookRotation;
            } else {
                // Animation normale de rotation
                transform.rotation = Quaternion.Lerp(startRotation, targetRotationAnimation, smoothProgress);
            }
            
            // Garder le nord en haut si activé
            if (keepNorthUp) {
                KeepNorthUp();
            }
            
            if (showDebugInfo && Time.frameCount % 30 == 0) { // Log toutes les 30 frames
                Debug.Log($"🎬 Animation: Progress={progress:F2}, Position={transform.position:F2}, Rotation={transform.rotation.eulerAngles:F1}");
            }
        }
        
        void HandleRotationDuringAnimation() {
            // Permettre l'orientation libre pendant l'animation
            if (Input.GetMouseButton(0) && hasSelectedPlanet) {
                if (!isFreeLooking) {
                    Debug.Log("🔄 Début orientation libre pendant animation");
                    StartFreeLook();
                }
                UpdateFreeLook();
            }
            if (Input.GetMouseButtonUp(0) && isFreeLooking) {
                Debug.Log("🔄 Fin orientation libre pendant animation");
                StopFreeLook();
            }
        }
        
        // ===== TOURNER AUTOUR DE LA PLANÈTE =====
        void StartOrbiting() {
            isOrbiting = true;
            lastMousePosition = Input.mousePosition;
            Debug.Log("🔄 Début de l'orbite autour de la planète");
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
            
            Debug.Log($"🔄 Orbite: Y={yRotation:F3}, X={xRotation:F3}, Target={targetRotation:F1}");
            
            lastMousePosition = Input.mousePosition;
        }
        
        void StopOrbiting() {
            isOrbiting = false;
            Debug.Log("🔄 Fin de l'orbite autour de la planète");
        }
        
        // ===== ORIENTATION LIBRE SUR PLACE =====
        void StartFreeLook() {
            if (!hasSelectedPlanet) {
                Debug.Log("❌ StartFreeLook: Pas de planète sélectionnée");
                return;
            }
            
            Debug.Log("🔄 StartFreeLook: Début orientation libre");
            isFreeLooking = true;
            lastMousePosition = Input.mousePosition;
            freeLookRotation = transform.rotation; // Sauvegarder la rotation actuelle
            Debug.Log($"🔄 StartFreeLook: Rotation initiale: {freeLookRotation.eulerAngles}");
        }
        
        void UpdateFreeLook() {
            if (!isFreeLooking) {
                Debug.Log("❌ UpdateFreeLook: Pas en mode orientation libre");
                return;
            }
            
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // Rotation horizontale (autour de l'axe Y) - orientation libre
            float yRotation = mouseDelta.x * rotationSpeed * 0.1f;
            
            // Rotation verticale (autour de l'axe X) - orientation libre
            float xRotation = -mouseDelta.y * rotationSpeed * 0.1f;
            
            // Appliquer la rotation libre à la caméra
            freeLookRotation *= Quaternion.Euler(xRotation, yRotation, 0f);
            
            Debug.Log($"🔄 UpdateFreeLook: Delta=({mouseDelta.x:F1}, {mouseDelta.y:F1}), Rot=({xRotation:F3}, {yRotation:F3}), Final={freeLookRotation.eulerAngles}");
            
            lastMousePosition = Input.mousePosition;
        }
        
        void StopFreeLook() {
            isFreeLooking = false;
            Debug.Log("🔄 Fin de l'orientation libre");
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
                if (showDebugInfo && Time.frameCount % 60 == 0) { // Log toutes les 60 frames
                    Debug.Log($"📷 Caméra libre - hasSelectedPlanet: {hasSelectedPlanet}, targetPlanet: {(targetPlanet != null ? "OK" : "NULL")}");
                }
                return;
            }
            
            // Gérer l'animation vers la planète
            if (isMovingToPlanet) {
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
                Debug.Log($"🔄 UpdateCamera: Application rotation libre: {freeLookRotation.eulerAngles}");
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
                Debug.Log($"📷 Caméra - Position: {transform.position}, Distance: {currentDistance:F2}, Rotation: {currentRotation:F1}");
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
            
            Debug.Log("🔄 Caméra réinitialisée");
        }
        
        void FocusOnPlanet() {
            if (!hasSelectedPlanet) return;
            
            // Focus sur la planète avec une rotation douce
            Vector3 direction = (planetCenter - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(direction).eulerAngles;
            
            Debug.Log("🎯 Focus sur la planète");
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
            
            Debug.LogWarning("❌ Aucune planète trouvée. Créez d'abord une planète avec HexaAstralithPlanet.");
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
