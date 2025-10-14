using UnityEngine;

public class MoonRevolution : MonoBehaviour
{
    [Header("🌙 Configuration de la Révolution")]
    [SerializeField] public Transform centerObject; // Objet autour duquel tourner
    [SerializeField] public float revolutionRadius = 5f; // Rayon de la révolution
    [SerializeField] public float revolutionSpeed = 1f; // Vitesse de révolution (tours par seconde)
    [SerializeField] public bool enableSelfRotation = false; // Révolution sur soi-même
    [SerializeField] public float selfRotationSpeed = 1f; // Vitesse de rotation sur soi-même
    
    [Header("🎯 Axe de Révolution")]
    [SerializeField] public Vector3 revolutionAxis = Vector3.up; // Axe autour duquel tourner
    [SerializeField] public bool useWorldAxis = true; // Utiliser l'axe du monde ou local
    
    [Header("🎮 Contrôles")]
    [SerializeField] public bool startOnAwake = true; // Commencer automatiquement
    [SerializeField] public bool pauseOnStart = false; // Pause au démarrage
    
    [Header("🎨 Debug")]
    [SerializeField] public bool showDebugInfo = true;
    [SerializeField] public bool showRevolutionPath = true;
    [SerializeField] public Color pathColor = Color.yellow;
    [SerializeField] public int pathSegments = 32; // Nombre de segments pour dessiner le chemin
    
    // Variables privées
    private float currentAngle = 0f; // Angle actuel de révolution
    private bool isRevolving = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Start()
    {
        // Sauvegarder la position et rotation initiales
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Positionner l'objet sur le rayon de révolution
        if (centerObject != null)
        {
            SetupInitialPosition();
        }
        
        // Démarrer la révolution si activé
        if (startOnAwake && !pauseOnStart)
        {
            StartRevolution();
        }
    }
    
    void Update()
    {
        if (isRevolving && centerObject != null)
        {
            UpdateRevolution();
        }
    }
    
    void SetupInitialPosition()
    {
        // Positionner l'objet à la distance de révolution du centre
        Vector3 direction = (transform.position - centerObject.position).normalized;
        if (direction == Vector3.zero)
        {
            direction = Vector3.right; // Direction par défaut
        }
        
        transform.position = centerObject.position + direction * revolutionRadius;
    }
    
    void UpdateRevolution()
    {
        // Calculer le nouvel angle
        currentAngle += revolutionSpeed * 360f * Time.deltaTime; // Convertir en degrés par seconde
        
        // Normaliser l'angle
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }
        
        // Calculer la nouvelle position
        Vector3 axis = useWorldAxis ? revolutionAxis : centerObject.TransformDirection(revolutionAxis);
        Vector3 newPosition = centerObject.position + Quaternion.AngleAxis(currentAngle, axis) * Vector3.right * revolutionRadius;
        
        // Appliquer la position
        transform.position = newPosition;
        
        // Rotation sur soi-même si activée
        if (enableSelfRotation)
        {
            transform.Rotate(Vector3.up, selfRotationSpeed * 360f * Time.deltaTime);
        }
    }
    
    public void StartRevolution()
    {
        isRevolving = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"MoonRevolution: Started revolving around {centerObject.name}");
        }
    }
    
    public void StopRevolution()
    {
        isRevolving = false;
        
        if (showDebugInfo)
        {
            Debug.Log("MoonRevolution: Stopped revolving");
        }
    }
    
    public void PauseRevolution()
    {
        isRevolving = false;
        
        if (showDebugInfo)
        {
            Debug.Log("MoonRevolution: Paused");
        }
    }
    
    public void ResumeRevolution()
    {
        isRevolving = true;
        
        if (showDebugInfo)
        {
            Debug.Log("MoonRevolution: Resumed");
        }
    }
    
    public void ResetPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        currentAngle = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log("MoonRevolution: Position reset");
        }
    }
    
    public void SetRevolutionSpeed(float newSpeed)
    {
        revolutionSpeed = newSpeed;
        
        if (showDebugInfo)
        {
            Debug.Log($"MoonRevolution: Speed changed to {newSpeed}");
        }
    }
    
    public void SetRevolutionRadius(float newRadius)
    {
        revolutionRadius = newRadius;
        
        if (showDebugInfo)
        {
            Debug.Log($"MoonRevolution: Radius changed to {newRadius}");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showRevolutionPath || centerObject == null) return;
        
        // Dessiner le chemin de révolution
        Gizmos.color = pathColor;
        
        Vector3 axis = useWorldAxis ? revolutionAxis : centerObject.TransformDirection(revolutionAxis);
        
        for (int i = 0; i < pathSegments; i++)
        {
            float angle1 = (float)i / pathSegments * 360f;
            float angle2 = (float)(i + 1) / pathSegments * 360f;
            
            Vector3 pos1 = centerObject.position + Quaternion.AngleAxis(angle1, axis) * Vector3.right * revolutionRadius;
            Vector3 pos2 = centerObject.position + Quaternion.AngleAxis(angle2, axis) * Vector3.right * revolutionRadius;
            
            Gizmos.DrawLine(pos1, pos2);
        }
        
        // Dessiner le centre
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerObject.position, 0.2f);
        
        // Dessiner l'axe de révolution
        Gizmos.color = Color.blue;
        Vector3 axisStart = centerObject.position - axis * revolutionRadius;
        Vector3 axisEnd = centerObject.position + axis * revolutionRadius;
        Gizmos.DrawLine(axisStart, axisEnd);
    }
}
