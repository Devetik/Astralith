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
    
    // Variables privées
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
    
    public void StopTracking()
    {
        isTracking = false;
        isLookingAtTarget = false;
        isInTrackingMode = false;
        currentTarget = null;
        lastMovementTime = 0f;
        
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
    }
}
