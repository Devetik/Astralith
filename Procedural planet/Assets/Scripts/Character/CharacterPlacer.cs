using UnityEngine;
using System.Collections.Generic;

public class CharacterPlacer : MonoBehaviour
{
    [Header("Configuration")]
    public Transform characterPrefab;
    public PlanetGenerator planetGenerator;
    public PlanetGeneratorNetworked planetGeneratorNetworked;
    public ContinentAnalyzer continentAnalyzer;
    
    [Header("Paramètres de Placement")]
    public int searchAttempts = 1000; // Nombre de tentatives pour trouver une position valide
    public float minHeightAboveWater = 0.1f; // Hauteur minimale au-dessus de l'eau
    public float maxHeightAboveWater = 2.0f; // Hauteur maximale au-dessus de l'eau
    public LayerMask groundLayer = 1; // Layer du sol
    
    [Header("Placement Intelligent")]
    public bool useContinentAnalysis = true; // Utilise l'analyse des continents
    public bool preferMainContinent = true; // Préfère le continent principal
    
    [Header("Tags de Détection")]
    public string landTag = "Land"; // Tag pour la terre
    public string waterTag = "Water"; // Tag pour l'eau
    public string oceanTag = "Ocean"; // Tag pour l'océan
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public Color debugColor = Color.green;
    
    private Transform currentCharacter;
    private List<Vector3> validPositions = new List<Vector3>();
    
    private void Start()
    {
        // Trouve automatiquement les générateurs si non assignés
        if (planetGenerator == null)
        {
            planetGenerator = FindObjectOfType<PlanetGenerator>();
            Debug.Log($"PlanetGenerator trouvé: {planetGenerator != null}");
        }
            
        if (planetGeneratorNetworked == null)
        {
            planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
            Debug.Log($"PlanetGeneratorNetworked trouvé: {planetGeneratorNetworked != null}");
        }
            
        Debug.Log($"CharacterPlacer Start - PlanetGenerator: {planetGenerator != null}, PlanetGeneratorNetworked: {planetGeneratorNetworked != null}");
        
        // Si aucun générateur n'est trouvé, essaie de les chercher dans tous les GameObjects
        if (planetGenerator == null && planetGeneratorNetworked == null)
        {
            Debug.LogWarning("Aucun générateur trouvé au Start, recherche différée...");
            Invoke(nameof(FindGeneratorsDelayed), 1f);
        }
    }
    
    private void FindGeneratorsDelayed()
    {
        if (planetGenerator == null)
        {
            planetGenerator = FindObjectOfType<PlanetGenerator>();
            Debug.Log($"PlanetGenerator trouvé (différé): {planetGenerator != null}");
        }
            
        if (planetGeneratorNetworked == null)
        {
            planetGeneratorNetworked = FindObjectOfType<PlanetGeneratorNetworked>();
            Debug.Log($"PlanetGeneratorNetworked trouvé (différé): {planetGeneratorNetworked != null}");
        }
    }
    
    /// <summary>
    /// Place le personnage sur la surface de la planète
    /// </summary>
    public void PlaceCharacterOnPlanet()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("CharacterPlacer: Aucun prefab de personnage assigné !");
            return;
        }
        
        // Trouve une position sur la surface de la planète (simplifié)
        Vector3 position = FindAnySurfacePosition();
        
        if (position == Vector3.zero)
        {
            Debug.LogWarning("CharacterPlacer: Impossible de trouver une position sur la planète !");
            return;
        }
        
        // Place le personnage
        PlaceCharacterAtPosition(position);
        
        if (showDebugInfo)
        {
            Debug.Log($"Personnage placé à la position: {position}");
        }
    }
    
    /// <summary>
    /// Trouve une position intelligente sur la surface de la planète
    /// </summary>
    private Vector3 FindAnySurfacePosition()
    {
        // Essaie de trouver un générateur
        float planetRadius = 5f; // Valeur par défaut
        bool foundGenerator = false;
        
        if (planetGeneratorNetworked != null)
        {
            planetRadius = planetGeneratorNetworked.radius;
            Debug.Log($"Utilisation de PlanetGeneratorNetworked - Rayon: {planetRadius}");
            foundGenerator = true;
        }
        else if (planetGenerator != null)
        {
            planetRadius = planetGenerator.radius;
            Debug.Log($"Utilisation de PlanetGenerator - Rayon: {planetRadius}");
            foundGenerator = true;
        }
        else
        {
            Debug.LogWarning("Aucun générateur trouvé, recherche dans la scène...");
            
            // Recherche manuelle dans tous les GameObjects
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                var pg = obj.GetComponent<PlanetGenerator>();
                var pgn = obj.GetComponent<PlanetGeneratorNetworked>();
                
                if (pg != null)
                {
                    planetRadius = pg.radius;
                    planetGenerator = pg;
                    Debug.Log($"PlanetGenerator trouvé manuellement - Rayon: {planetRadius}");
                    foundGenerator = true;
                    break;
                }
                else if (pgn != null)
                {
                    planetRadius = pgn.radius;
                    planetGeneratorNetworked = pgn;
                    Debug.Log($"PlanetGeneratorNetworked trouvé manuellement - Rayon: {planetRadius}");
                    foundGenerator = true;
                    break;
                }
            }
            
            if (!foundGenerator)
            {
                Debug.LogWarning("Aucun générateur trouvé dans la scène, utilisation du rayon par défaut: 5f");
            }
        }
        
        // Utilise l'analyse des continents si disponible
        if (useContinentAnalysis && continentAnalyzer != null)
        {
            Debug.Log("Utilisation de l'analyse des continents...");
            var continents = continentAnalyzer.AnalyzeContinents(planetRadius, landTag, waterTag);
            
            if (continents.Count > 0)
            {
                if (preferMainContinent)
                {
                    var mainContinent = continentAnalyzer.GetMainContinent();
                    if (mainContinent != null)
                    {
                        Vector3 position = continentAnalyzer.GetRandomPositionOnMainContinent();
                        if (position != Vector3.zero)
                        {
                            Debug.Log($"Position sur le continent principal: {position}");
                            return position;
                        }
                    }
                }
                
                // Fallback: utilise le plus grand continent
                var largestContinent = continents[0];
                if (largestContinent.points.Count > 0)
                {
                    int randomIndex = Random.Range(0, largestContinent.points.Count);
                    Vector3 position = largestContinent.points[randomIndex];
                    Debug.Log($"Position sur le plus grand continent: {position}");
                    return position;
                }
            }
            else
            {
                Debug.LogWarning("Aucun continent trouvé par l'analyse, utilisation du placement simple...");
            }
        }
        
        // Fallback: génère une position aléatoire sur la sphère
        Vector3 randomDirection = Random.onUnitSphere;
        Vector3 surfacePosition = randomDirection * planetRadius;
        
        Debug.Log($"Position générée sur la surface (rayon {planetRadius}): {surfacePosition}");
        return surfacePosition;
    }
    
    /// <summary>
    /// Trouve une position valide avec les paramètres donnés
    /// </summary>
    private Vector3 FindValidPositionWithParams(float planetRadius, float oceanLevel)
    {
        Debug.Log($"Recherche de position - Rayon: {planetRadius}, Niveau océan: {oceanLevel}");
        
        for (int i = 0; i < searchAttempts; i++)
        {
            // Génère une position aléatoire sur la sphère
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 surfacePosition = randomDirection * planetRadius;
            
            // Utilise un raycast pour vérifier si c'est de la terre
            if (IsPositionOnLand(surfacePosition, planetRadius))
            {
                validPositions.Add(surfacePosition);
                Debug.Log($"Position valide trouvée à l'essai {i + 1}: {surfacePosition}");
                return surfacePosition;
            }
        }
        
        Debug.LogWarning($"Aucune position valide trouvée après {searchAttempts} tentatives");
        return Vector3.zero;
    }
    
    /// <summary>
    /// Trouve une position valide avec des critères plus larges
    /// </summary>
    private Vector3 FindValidLandPositionRelaxed(float planetRadius, float oceanLevel)
    {
        Debug.Log("Tentative avec critères plus larges...");
        
        for (int i = 0; i < searchAttempts; i++)
        {
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 surfacePosition = randomDirection * planetRadius;
            
            // Essaie d'abord avec le raycast
            if (IsPositionOnLand(surfacePosition, planetRadius))
            {
                Debug.Log($"Position trouvée avec raycast à l'essai {i + 1}");
                return surfacePosition;
            }
            
            // Fallback avec la méthode de hauteur
            if (IsPositionAboveWater(surfacePosition, oceanLevel, planetRadius))
            {
                Debug.Log($"Position trouvée avec méthode de hauteur à l'essai {i + 1}");
                return surfacePosition;
            }
        }
        
        // Dernière tentative : position aléatoire sur la sphère
        Debug.Log("Dernière tentative : position aléatoire sur la sphère");
        return Random.onUnitSphere * planetRadius;
    }
    
    /// <summary>
    /// Vérifie si une position est sur la terre en utilisant un raycast avec tags
    /// </summary>
    private bool IsPositionOnLand(Vector3 position, float planetRadius)
    {
        // Raycast depuis la position vers le centre de la planète
        Vector3 directionToCenter = -position.normalized;
        RaycastHit hit;
        
        // Raycast avec une distance suffisante pour traverser la planète
        if (Physics.Raycast(position + directionToCenter * planetRadius * 0.1f, directionToCenter, out hit, planetRadius * 0.2f, groundLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Vérifie les tags pour déterminer le type de surface
            if (IsLandObject(hitObject))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Terre détectée: {hitObject.name} (Tag: {hitObject.tag})");
                }
                return true;
            }
            else if (IsWaterObject(hitObject))
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Eau détectée: {hitObject.name} (Tag: {hitObject.tag})");
                }
                return false;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Vérifie si un objet est de la terre
    /// </summary>
    private bool IsLandObject(GameObject obj)
    {
        return obj.tag == landTag || 
               obj.name.ToLower().Contains("land") || 
               obj.name.ToLower().Contains("terrain");
    }
    
    /// <summary>
    /// Vérifie si un objet est de l'eau
    /// </summary>
    private bool IsWaterObject(GameObject obj)
    {
        return obj.tag == waterTag || 
               obj.tag == oceanTag ||
               obj.name.ToLower().Contains("water") || 
               obj.name.ToLower().Contains("ocean");
    }
    
    /// <summary>
    /// Vérifie si une position est au-dessus du niveau de l'eau (méthode alternative)
    /// </summary>
    private bool IsPositionAboveWater(Vector3 position, float oceanLevel, float planetRadius)
    {
        // Calcule la hauteur relative par rapport au rayon de la planète
        float relativeHeight = position.magnitude / planetRadius;
        return relativeHeight > (1.0f + oceanLevel);
    }
    
    /// <summary>
    /// Calcule la hauteur au-dessus de l'eau
    /// </summary>
    private float GetHeightAboveWater(Vector3 position, float oceanLevel, float planetRadius)
    {
        float relativeHeight = position.magnitude / planetRadius;
        float waterLevel = 1.0f + oceanLevel;
        return relativeHeight - waterLevel;
    }
    
    /// <summary>
    /// Place le personnage à la position spécifiée (simplifié)
    /// </summary>
    private void PlaceCharacterAtPosition(Vector3 position)
    {
        // Supprime l'ancien personnage s'il existe
        if (currentCharacter != null)
        {
            DestroyImmediate(currentCharacter.gameObject);
        }
        
        // Instancie le nouveau personnage
        currentCharacter = Instantiate(characterPrefab, position, Quaternion.identity);
        
        // Oriente le personnage correctement (debout sur la surface)
        Vector3 directionFromCenter = position.normalized;
        
        // Méthode simple : fait que le personnage regarde vers l'extérieur
        // Vector3.up du personnage doit pointer vers l'extérieur (directionFromCenter)
        currentCharacter.rotation = Quaternion.FromToRotation(Vector3.up, directionFromCenter);
        
        if (showDebugInfo)
        {
            Debug.Log($"Personnage placé et orienté correctement à: {position}");
            Debug.Log($"Direction du centre: {directionFromCenter}");
            Debug.Log($"Rotation appliquée: {currentCharacter.rotation.eulerAngles}");
        }
    }
    
    /// <summary>
    /// Ajuste le personnage pour qu'il touche le sol
    /// </summary>
    private void AdjustCharacterToGround()
    {
        if (currentCharacter == null) return;
        
        // Raycast vers le centre de la planète pour trouver le sol
        Vector3 directionToCenter = -currentCharacter.position.normalized;
        RaycastHit hit;
        
        if (Physics.Raycast(currentCharacter.position + directionToCenter * 10f, directionToCenter, out hit, 20f, groundLayer))
        {
            // Ajuste la position pour que les pieds touchent le sol
            Vector3 groundPosition = hit.point;
            currentCharacter.position = groundPosition;
            
            // Oriente le personnage perpendiculairement à la surface
            currentCharacter.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }
    
    /// <summary>
    /// Méthode appelée automatiquement lors de la génération de planète
    /// </summary>
    public void OnPlanetGenerated()
    {
        // Attend un peu que la planète soit complètement générée
        Invoke(nameof(PlaceCharacterOnPlanet), 0.5f);
    }
    
    /// <summary>
    /// Force le placement du personnage
    /// </summary>
    [ContextMenu("Placer Personnage")]
    public void ForcePlaceCharacter()
    {
        Debug.Log("=== FORCE PLACEMENT DU PERSONNAGE ===");
        PlaceCharacterOnPlanet();
    }
    
    /// <summary>
    /// Test simple de placement (pour debug)
    /// </summary>
    [ContextMenu("Test Placement Simple")]
    public void TestSimplePlacement()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("Aucun prefab de personnage assigné !");
            return;
        }
        
        // Position de test simple
        Vector3 testPosition = new Vector3(0, 5, 0);
        
        // Supprime l'ancien personnage
        if (currentCharacter != null)
        {
            DestroyImmediate(currentCharacter.gameObject);
        }
        
        // Place le personnage
        currentCharacter = Instantiate(characterPrefab, testPosition, Quaternion.identity);
        
        Debug.Log($"Test de placement réussi à: {testPosition}");
    }
    
    /// <summary>
    /// Test d'orientation sur la surface
    /// </summary>
    [ContextMenu("Test Orientation Surface")]
    public void TestSurfaceOrientation()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("Aucun prefab de personnage assigné !");
            return;
        }
        
        // Position sur la surface de la planète
        Vector3 surfacePosition = FindAnySurfacePosition();
        
        if (surfacePosition == Vector3.zero)
        {
            Debug.LogError("Impossible de trouver une position sur la surface !");
            return;
        }
        
        // Supprime l'ancien personnage
        if (currentCharacter != null)
        {
            DestroyImmediate(currentCharacter.gameObject);
        }
        
        // Place le personnage avec la bonne orientation
        PlaceCharacterAtPosition(surfacePosition);
        
        Debug.Log($"Test d'orientation sur la surface à: {surfacePosition}");
    }
    
    /// <summary>
    /// Debug des générateurs trouvés
    /// </summary>
    [ContextMenu("Debug Générateurs")]
    public void DebugGenerators()
    {
        Debug.Log("=== DEBUG GÉNÉRATEURS ===");
        Debug.Log($"PlanetGenerator: {planetGenerator != null}");
        Debug.Log($"PlanetGeneratorNetworked: {planetGeneratorNetworked != null}");
        
        if (planetGenerator != null)
        {
            Debug.Log($"PlanetGenerator.radius: {planetGenerator.radius}");
        }
        
        if (planetGeneratorNetworked != null)
        {
            Debug.Log($"PlanetGeneratorNetworked.radius: {planetGeneratorNetworked.radius}");
        }
        
        // Recherche dans tous les GameObjects
        var allObjects = FindObjectsOfType<GameObject>();
        int pgCount = 0;
        int pgnCount = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj.GetComponent<PlanetGenerator>() != null) pgCount++;
            if (obj.GetComponent<PlanetGeneratorNetworked>() != null) pgnCount++;
        }
        
        Debug.Log($"PlanetGenerator trouvés dans la scène: {pgCount}");
        Debug.Log($"PlanetGeneratorNetworked trouvés dans la scène: {pgnCount}");
        Debug.Log("=== FIN DEBUG ===");
    }
    
    /// <summary>
    /// Debug des tags dans la scène
    /// </summary>
    [ContextMenu("Debug Tags")]
    public void DebugTags()
    {
        Debug.Log("=== DEBUG TAGS ===");
        
        var allObjects = FindObjectsOfType<GameObject>();
        int landCount = 0;
        int waterCount = 0;
        
        foreach (var obj in allObjects)
        {
            if (obj.tag == landTag || obj.name.ToLower().Contains("land"))
            {
                landCount++;
                Debug.Log($"Terre trouvée: {obj.name} (Tag: {obj.tag})");
            }
            else if (obj.tag == waterTag || obj.tag == oceanTag || obj.name.ToLower().Contains("water") || obj.name.ToLower().Contains("ocean"))
            {
                waterCount++;
                Debug.Log($"Eau trouvée: {obj.name} (Tag: {obj.tag})");
            }
        }
        
        Debug.Log($"Total objets de terre: {landCount}");
        Debug.Log($"Total objets d'eau: {waterCount}");
        Debug.Log("=== FIN DEBUG TAGS ===");
    }
    
    /// <summary>
    /// Test de l'analyse des continents
    /// </summary>
    [ContextMenu("Test Analyse Continents")]
    public void TestContinentAnalysis()
    {
        if (continentAnalyzer == null)
        {
            Debug.LogError("ContinentAnalyzer non trouvé !");
            return;
        }
        
        float planetRadius = 5f;
        if (planetGeneratorNetworked != null)
            planetRadius = planetGeneratorNetworked.radius;
        else if (planetGenerator != null)
            planetRadius = planetGenerator.radius;
        
        Debug.Log($"Test de l'analyse des continents avec rayon: {planetRadius}");
        var continents = continentAnalyzer.AnalyzeContinents(planetRadius, landTag, waterTag);
        
        Debug.Log($"Continents trouvés: {continents.Count}");
        for (int i = 0; i < continents.Count; i++)
        {
            Debug.Log($"Continent {i}: {continents[i].points.Count} points, principal: {continents[i].isMainContinent}");
        }
    }
    
    /// <summary>
    /// Supprime le personnage actuel
    /// </summary>
    [ContextMenu("Supprimer Personnage")]
    public void RemoveCharacter()
    {
        if (currentCharacter != null)
        {
            DestroyImmediate(currentCharacter.gameObject);
            currentCharacter = null;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugInfo || validPositions.Count == 0) return;
        
        Gizmos.color = debugColor;
        foreach (Vector3 pos in validPositions)
        {
            Gizmos.DrawWireSphere(pos, 0.5f);
        }
    }
}
