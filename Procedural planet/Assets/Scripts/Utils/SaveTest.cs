using UnityEngine;
using System.IO;

/// <summary>
/// Script de test pour diagnostiquer les problèmes de sauvegarde
/// </summary>
public class SaveTest : MonoBehaviour
{
    [ContextMenu("Test Complet de Sauvegarde")]
    public void TestCompleteSave()
    {
        Debug.Log("=== TEST COMPLET DE SAUVEGARDE ===");
        
        // 1. Trouve les composants
        var planetGenerator = GetComponent<PlanetGenerator>();
        var planetGeneratorNetworked = GetComponent<PlanetGeneratorNetworked>();
        var saveManager = GetComponent<PlanetSaveManager>();
        
        Debug.Log($"PlanetGenerator trouvé: {planetGenerator != null}");
        Debug.Log($"PlanetGeneratorNetworked trouvé: {planetGeneratorNetworked != null}");
        Debug.Log($"PlanetSaveManager trouvé: {saveManager != null}");
        
        if (planetGeneratorNetworked != null)
        {
            Debug.Log($"PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
        }
        
        if (planetGenerator != null)
        {
            Debug.Log($"PlanetGenerator - Radius: {planetGenerator.radius}, Seed: {planetGenerator.seed}");
        }
        
        // 2. Test de sauvegarde
        if (saveManager != null)
        {
            Debug.Log("--- TEST DE SAUVEGARDE ---");
            saveManager.SaveSettings();
            
            // Vérifie si le fichier existe
            string savePath = Path.Combine(Application.persistentDataPath, "planet_settings.json");
            bool fileExists = File.Exists(savePath);
            Debug.Log($"Fichier de sauvegarde existe: {fileExists}");
            
            if (fileExists)
            {
                string content = File.ReadAllText(savePath);
                Debug.Log($"Contenu du fichier: {content}");
            }
        }
        
        Debug.Log("=== FIN DU TEST ===");
    }
    
    [ContextMenu("Test de Chargement")]
    public void TestLoad()
    {
        Debug.Log("=== TEST DE CHARGEMENT ===");
        
        var saveManager = GetComponent<PlanetSaveManager>();
        if (saveManager != null)
        {
            Debug.Log("--- AVANT CHARGEMENT ---");
            var planetGeneratorNetworked = GetComponent<PlanetGeneratorNetworked>();
            if (planetGeneratorNetworked != null)
            {
                Debug.Log($"PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
            }
            
            Debug.Log("--- CHARGEMENT ---");
            saveManager.LoadSettings();
            
            Debug.Log("--- APRÈS CHARGEMENT ---");
            if (planetGeneratorNetworked != null)
            {
                Debug.Log($"PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
            }
        }
        
        Debug.Log("=== FIN DU TEST DE CHARGEMENT ===");
    }
    
    [ContextMenu("Modifier Radius et Tester")]
    public void ModifyRadiusAndTest()
    {
        Debug.Log("=== MODIFICATION ET TEST ===");
        
        var planetGeneratorNetworked = GetComponent<PlanetGeneratorNetworked>();
        if (planetGeneratorNetworked != null)
        {
            // Modifie le radius
            planetGeneratorNetworked.radius = 2.5f;
            Debug.Log($"Radius modifié à: {planetGeneratorNetworked.radius}");
            
            // Sauvegarde
            var saveManager = GetComponent<PlanetSaveManager>();
            if (saveManager != null)
            {
                saveManager.SaveFromNetworked();
                Debug.Log("Sauvegarde effectuée");
            }
        }
        
        Debug.Log("=== FIN DE LA MODIFICATION ===");
    }
    
    [ContextMenu("Désactiver Auto-Load et Tester")]
    public void DisableAutoLoadAndTest()
    {
        Debug.Log("=== DÉSACTIVATION AUTO-LOAD ===");
        
        var saveManager = GetComponent<PlanetSaveManager>();
        if (saveManager != null)
        {
            saveManager.autoLoadOnStart = false;
            Debug.Log("Auto-load désactivé");
            
            // Test de chargement manuel
            Debug.Log("Test de chargement manuel...");
            saveManager.LoadToNetworked();
        }
        
        Debug.Log("=== FIN DU TEST AUTO-LOAD ===");
    }
    
    [ContextMenu("Test Radius 12 et Sauvegarde")]
    public void TestRadius12AndSave()
    {
        Debug.Log("=== TEST RADIUS 12 ===");
        
        var planetGeneratorNetworked = GetComponent<PlanetGeneratorNetworked>();
        if (planetGeneratorNetworked != null)
        {
            // Modifie le radius à 12
            planetGeneratorNetworked.radius = 12.0f;
            Debug.Log($"Radius modifié à: {planetGeneratorNetworked.radius}");
            
            // Sauvegarde
            var saveManager = GetComponent<PlanetSaveManager>();
            if (saveManager != null)
            {
                saveManager.SaveFromNetworked();
                Debug.Log("Sauvegarde effectuée avec radius 12");
            }
        }
        
        Debug.Log("=== FIN DU TEST RADIUS 12 ===");
    }
    
    [ContextMenu("Activer Auto-Load et Tester")]
    public void EnableAutoLoadAndTest()
    {
        Debug.Log("=== ACTIVATION AUTO-LOAD ===");
        
        var saveManager = GetComponent<PlanetSaveManager>();
        if (saveManager != null)
        {
            saveManager.autoLoadOnStart = true;
            Debug.Log("Auto-load activé");
            
            // Test de chargement manuel
            Debug.Log("Test de chargement manuel...");
            saveManager.LoadToNetworked();
        }
        
        Debug.Log("=== FIN DU TEST AUTO-LOAD ===");
    }
    
    [ContextMenu("Test Sauvegarde Simple")]
    public void TestSimpleSave()
    {
        Debug.Log("=== TEST SAUVEGARDE SIMPLE ===");
        
        var saveManager = GetComponent<PlanetSaveManager>();
        if (saveManager == null)
        {
            Debug.LogError("PlanetSaveManager non trouvé !");
            return;
        }
        
        Debug.Log("PlanetSaveManager trouvé, test de sauvegarde...");
        saveManager.SaveSettings();
        
        Debug.Log("=== FIN TEST SAUVEGARDE SIMPLE ===");
    }
}
