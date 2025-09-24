using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ContinentAnalyzer : MonoBehaviour
{
    [Header("Configuration")]
    public int samplePoints = 1000; // Nombre de points à échantillonner
    public float sampleRadius = 0.1f; // Rayon d'échantillonnage autour de chaque point
    public int minContinentSize = 10; // Taille minimale d'un continent
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public Color continentColor = Color.green;
    public Color oceanColor = Color.blue;
    
    private List<ContinentData> continents = new List<ContinentData>();
    
    [System.Serializable]
    public class ContinentData
    {
        public Vector3 center;
        public float size;
        public List<Vector3> points;
        public bool isMainContinent;
        
        public ContinentData(Vector3 center)
        {
            this.center = center;
            this.points = new List<Vector3>();
            this.size = 0;
            this.isMainContinent = false;
        }
    }
    
    /// <summary>
    /// Analyse la planète pour trouver les continents
    /// </summary>
    public List<ContinentData> AnalyzeContinents(float planetRadius, string landTag = "Land", string waterTag = "Water")
    {
        continents.Clear();
        
        if (showDebugInfo)
        {
            Debug.Log($"=== ANALYSE DES CONTINENTS ===");
            Debug.Log($"Rayon de la planète: {planetRadius}");
            Debug.Log($"Points d'échantillonnage: {samplePoints}");
        }
        
        // Échantillonne des points sur la surface de la planète
        List<Vector3> landPoints = SampleLandPoints(planetRadius, landTag, waterTag);
        
        if (landPoints.Count == 0)
        {
            Debug.LogWarning("Aucun point de terre trouvé !");
            return continents;
        }
        
        // Groupe les points en continents
        GroupPointsIntoContinents(landPoints);
        
        // Trouve le continent principal (le plus grand)
        FindMainContinent();
        
        if (showDebugInfo)
        {
            Debug.Log($"Continents trouvés: {continents.Count}");
            if (continents.Count > 0)
            {
                var mainContinent = continents.FirstOrDefault(c => c.isMainContinent);
                if (mainContinent != null)
                {
                    Debug.Log($"Continent principal: {mainContinent.size} points, centre: {mainContinent.center}");
                }
            }
        }
        
        return continents;
    }
    
    /// <summary>
    /// Échantillonne des points de terre sur la surface
    /// </summary>
    private List<Vector3> SampleLandPoints(float planetRadius, string landTag, string waterTag)
    {
        List<Vector3> landPoints = new List<Vector3>();
        
        for (int i = 0; i < samplePoints; i++)
        {
            // Génère un point aléatoire sur la sphère
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 surfacePoint = randomDirection * planetRadius;
            
            // Vérifie si c'est de la terre
            if (IsLandPoint(surfacePoint, landTag, waterTag))
            {
                landPoints.Add(surfacePoint);
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Points de terre trouvés: {landPoints.Count} sur {samplePoints}");
        }
        
        return landPoints;
    }
    
    /// <summary>
    /// Vérifie si un point est de la terre émergée (bonne zone de spawn)
    /// </summary>
    private bool IsLandPoint(Vector3 point, string landTag, string waterTag)
    {
        // 1. Raycast principal depuis le centre vers le point
        RaycastHit hit;
        if (!Physics.Raycast(Vector3.zero, point.normalized, out hit, 10.0f))
        {
            if (showDebugInfo)
            {
                Debug.Log($"Aucun objet touché pour le point: {point}");
            }
            return false;
        }
        
        GameObject hitObject = hit.collider.gameObject;
        
        if (showDebugInfo)
        {
            Debug.Log($"Raycast principal: {hitObject.name} (Tag: {hitObject.tag}) pour le point: {point}");
        }
        
        // 2. Vérifie si c'est une zone inondée (Land en premier = mauvais)
        bool isLandFirst = hitObject.tag == landTag || 
                          hitObject.name.ToLower().Contains("land") ||
                          hitObject.name.ToLower().Contains("terrain");
        
        if (isLandFirst)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Terre détectée en premier → Zone inondée (mauvaise)");
            }
            return false; // Zone inondée
        }
        
        // 3. Vérifie si c'est une zone émergée (Water en premier = bon)
        bool isWaterFirst = hitObject.tag == waterTag || 
                           hitObject.name.ToLower().Contains("water") ||
                           hitObject.name.ToLower().Contains("ocean");
        
        if (!isWaterFirst)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Ni terre ni eau détectée → Zone inconnue");
            }
            return false;
        }
        
        // 4. Vérifie la stabilité de la zone (raycasts alentour)
        if (!IsZoneStable(point, landTag, waterTag))
        {
            if (showDebugInfo)
            {
                Debug.Log($"Zone instable → Pas de spawn");
            }
            return false;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Zone émergée stable trouvée !");
        }
        return true; // Zone émergée stable !
    }
    
    /// <summary>
    /// Vérifie si la zone est stable (plusieurs raycasts alentour)
    /// </summary>
    private bool IsZoneStable(Vector3 centerPoint, string landTag, string waterTag)
    {
        // Raycasts dans différentes directions autour du point
        Vector3[] directions = {
            centerPoint.normalized,
            (centerPoint + Vector3.up * 0.1f).normalized,
            (centerPoint + Vector3.down * 0.1f).normalized,
            (centerPoint + Vector3.left * 0.1f).normalized,
            (centerPoint + Vector3.right * 0.1f).normalized,
            (centerPoint + Vector3.forward * 0.1f).normalized,
            (centerPoint + Vector3.back * 0.1f).normalized
        };
        
        int stableHits = 0;
        int totalHits = 0;
        
        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            if (Physics.Raycast(Vector3.zero, direction, out hit, 10.0f))
            {
                totalHits++;
                GameObject hitObject = hit.collider.gameObject;
                
                // Vérifie si c'est de l'eau (zone émergée)
                bool isWater = hitObject.tag == waterTag || 
                              hitObject.name.ToLower().Contains("water") ||
                              hitObject.name.ToLower().Contains("ocean");
                
                if (isWater)
                {
                    stableHits++;
                }
            }
        }
        
        // Zone stable si au moins 70% des raycasts touchent l'eau
        float stabilityRatio = (float)stableHits / totalHits;
        bool isStable = stabilityRatio >= 0.7f;
        
        if (showDebugInfo)
        {
            Debug.Log($"Stabilité de la zone: {stableHits}/{totalHits} ({stabilityRatio:P}) - Stable: {isStable}");
        }
        
        return isStable;
    }
    
    
    /// <summary>
    /// Groupe les points en continents
    /// </summary>
    private void GroupPointsIntoContinents(List<Vector3> landPoints)
    {
        List<Vector3> unprocessedPoints = new List<Vector3>(landPoints);
        
        while (unprocessedPoints.Count > 0)
        {
            Vector3 startPoint = unprocessedPoints[0];
            ContinentData continent = new ContinentData(startPoint);
            
            // Utilise un algorithme de flood fill pour trouver tous les points connectés
            FloodFillContinent(startPoint, unprocessedPoints, continent);
            
            if (continent.points.Count >= minContinentSize)
            {
                continent.size = continent.points.Count;
                continents.Add(continent);
            }
        }
        
        // Trie les continents par taille
        continents = continents.OrderByDescending(c => c.size).ToList();
    }
    
    /// <summary>
    /// Flood fill pour trouver tous les points connectés d'un continent
    /// </summary>
    private void FloodFillContinent(Vector3 startPoint, List<Vector3> unprocessedPoints, ContinentData continent)
    {
        Queue<Vector3> queue = new Queue<Vector3>();
        queue.Enqueue(startPoint);
        unprocessedPoints.Remove(startPoint);
        continent.points.Add(startPoint);
        
        while (queue.Count > 0)
        {
            Vector3 currentPoint = queue.Dequeue();
            
            // Trouve les points voisins
            List<Vector3> neighbors = FindNeighbors(currentPoint, unprocessedPoints);
            
            foreach (Vector3 neighbor in neighbors)
            {
                if (unprocessedPoints.Contains(neighbor))
                {
                    unprocessedPoints.Remove(neighbor);
                    continent.points.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
    
    /// <summary>
    /// Trouve les points voisins d'un point donné
    /// </summary>
    private List<Vector3> FindNeighbors(Vector3 point, List<Vector3> availablePoints)
    {
        List<Vector3> neighbors = new List<Vector3>();
        
        foreach (Vector3 otherPoint in availablePoints)
        {
            float distance = Vector3.Distance(point, otherPoint);
            if (distance <= sampleRadius)
            {
                neighbors.Add(otherPoint);
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// Trouve le continent principal (le plus grand)
    /// </summary>
    private void FindMainContinent()
    {
        if (continents.Count > 0)
        {
            continents[0].isMainContinent = true;
        }
    }
    
    /// <summary>
    /// Retourne le continent principal
    /// </summary>
    public ContinentData GetMainContinent()
    {
        return continents.FirstOrDefault(c => c.isMainContinent);
    }
    
    /// <summary>
    /// Retourne une position aléatoire sur le continent principal
    /// </summary>
    public Vector3 GetRandomPositionOnMainContinent()
    {
        var mainContinent = GetMainContinent();
        if (mainContinent != null && mainContinent.points.Count > 0)
        {
            int randomIndex = Random.Range(0, mainContinent.points.Count);
            Vector3 randomPoint = mainContinent.points[randomIndex];
            
            // Calcule la hauteur correcte (surface de la terre)
            Vector3 surfacePosition = GetSurfacePosition(randomPoint);
            
            if (showDebugInfo)
            {
                Debug.Log($"Position sur le continent principal: {surfacePosition}");
            }
            
            return surfacePosition;
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Calcule la position de surface correcte pour un point
    /// </summary>
    private Vector3 GetSurfacePosition(Vector3 point)
    {
        // Raycast depuis l'extérieur vers le point pour trouver la surface de la terre
        Vector3 directionToCenter = -point.normalized;
        Vector3 outsidePoint = point + directionToCenter * 2f;
        
        RaycastHit hit;
        if (Physics.Raycast(outsidePoint, -directionToCenter, out hit, 3.0f))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Vérifie si c'est de la terre
            bool isLand = hitObject.tag == "Land" || 
                         hitObject.name.ToLower().Contains("land") ||
                         hitObject.name.ToLower().Contains("terrain");
            
            if (isLand)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Surface de terre trouvée à: {hit.point}");
                }
                return hit.point;
            }
        }
        
        // Fallback: utilise le point original
        if (showDebugInfo)
        {
            Debug.Log($"Surface non trouvée, utilisation du point original: {point}");
        }
        return point;
    }
    
    /// <summary>
    /// Retourne le centre du continent principal
    /// </summary>
    public Vector3 GetMainContinentCenter()
    {
        var mainContinent = GetMainContinent();
        if (mainContinent != null)
        {
            return mainContinent.center;
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Test de détection des objets dans la scène
    /// </summary>
    [ContextMenu("Test Détection Objets")]
    public void TestObjectDetection()
    {
        Debug.Log("=== TEST DÉTECTION OBJETS ===");
        
        var allObjects = FindObjectsOfType<GameObject>();
        int landObjects = 0;
        int waterObjects = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj.tag == "Land" || obj.name.ToLower().Contains("land"))
            {
                landObjects++;
                Debug.Log($"Objet de terre: {obj.name} (Tag: {obj.tag})");
                
                // Vérifie les colliders
                var colliders = obj.GetComponents<Collider>();
                Debug.Log($"  Colliders: {colliders.Length}");
                foreach (var col in colliders)
                {
                    Debug.Log($"    - {col.GetType().Name} (enabled: {col.enabled})");
                }
            }
            else if (obj.tag == "Water" || obj.name.ToLower().Contains("water") || obj.name.ToLower().Contains("ocean"))
            {
                waterObjects++;
                Debug.Log($"Objet d'eau: {obj.name} (Tag: {obj.tag})");
                
                // Vérifie les colliders
                var colliders = obj.GetComponents<Collider>();
                Debug.Log($"  Colliders: {colliders.Length}");
                foreach (var col in colliders)
                {
                    Debug.Log($"    - {col.GetType().Name} (enabled: {col.enabled})");
                }
            }
        }
        
        Debug.Log($"Total objets de terre: {landObjects}");
        Debug.Log($"Total objets d'eau: {waterObjects}");
        Debug.Log("=== FIN TEST ===");
    }
    
    /// <summary>
    /// Test de raycast sur un point spécifique
    /// </summary>
    [ContextMenu("Test Raycast Point")]
    public void TestRaycastPoint()
    {
        Debug.Log("=== TEST RAYCAST POINT ===");
        
        // Test avec un point au hasard
        Vector3 testPoint = Random.onUnitSphere * 5f;
        Debug.Log($"Point de test: {testPoint}");
        
        Vector3 directionToCenter = -testPoint.normalized;
        RaycastHit hit;
        
        // Test avec différentes distances et directions
        Debug.Log("Test 1: Raycast depuis le point vers le centre");
        if (Physics.Raycast(testPoint + directionToCenter * 0.5f, directionToCenter, out hit, 1.0f))
        {
            Debug.Log($"Raycast réussi: {hit.collider.gameObject.name} (Tag: {hit.collider.gameObject.tag})");
        }
        else
        {
            Debug.Log("Raycast échoué - aucun objet touché");
        }
        
        // Test 2: Raycast depuis l'extérieur vers le point
        Debug.Log("Test 2: Raycast depuis l'extérieur vers le point");
        Vector3 outsidePoint = testPoint + directionToCenter * 2f;
        if (Physics.Raycast(outsidePoint, -directionToCenter, out hit, 3.0f))
        {
            Debug.Log($"Raycast réussi: {hit.collider.gameObject.name} (Tag: {hit.collider.gameObject.tag})");
        }
        else
        {
            Debug.Log("Raycast échoué - aucun objet touché");
        }
        
        // Test 3: Raycast depuis le centre vers l'extérieur
        Debug.Log("Test 3: Raycast depuis le centre vers l'extérieur");
        if (Physics.Raycast(Vector3.zero, testPoint.normalized, out hit, 10.0f))
        {
            Debug.Log($"Raycast réussi: {hit.collider.gameObject.name} (Tag: {hit.collider.gameObject.tag})");
        }
        else
        {
            Debug.Log("Raycast échoué - aucun objet touché");
        }
        
        Debug.Log("=== FIN TEST RAYCAST ===");
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugInfo || continents.Count == 0) return;
        
        foreach (var continent in continents)
        {
            Gizmos.color = continent.isMainContinent ? continentColor : oceanColor;
            
            foreach (var point in continent.points)
            {
                Gizmos.DrawWireSphere(point, 0.1f);
            }
        }
    }
}
