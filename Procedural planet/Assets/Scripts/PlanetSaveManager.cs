using System;
using System.IO;
using UnityEngine;

public class PlanetSaveManager : MonoBehaviour
{
    [Header("Configuration")]
    public string saveFileName = "planet_settings.json";
    public bool autoSaveOnGenerate = true;
    public bool autoLoadOnStart = true;

    private PlanetGenerator planetGenerator;
    private PlanetGeneratorNetworked planetGeneratorNetworked;
    private string savePath;

    private void Awake()
    {
        // Cherche les deux composants
        planetGenerator = GetComponent<PlanetGenerator>();
        planetGeneratorNetworked = GetComponent<PlanetGeneratorNetworked>();
        
        if (planetGeneratorNetworked == null && planetGenerator == null)
        {
            Debug.LogError("PlanetSaveManager nécessite un composant PlanetGenerator ou PlanetGeneratorNetworked sur le même GameObject !");
            return;
        }

        // Définit le chemin de sauvegarde dans le dossier persistant
        savePath = Path.Combine(Application.persistentDataPath, saveFileName);
        
        // Log de debug pour voir ce qui est trouvé
        if (planetGeneratorNetworked != null)
            Debug.Log("PlanetSaveManager: PlanetGeneratorNetworked trouvé - mode réseau activé");
        if (planetGenerator != null)
            Debug.Log("PlanetSaveManager: PlanetGenerator trouvé - mode local disponible");
    }

    private void Start()
    {
        Debug.Log($"PlanetSaveManager.Start() - autoLoadOnStart: {autoLoadOnStart}");
        
        // Force le chargement au démarrage (même si autoLoadOnStart est désactivé)
        Debug.Log("Démarrage du chargement différé (forcé)...");
        StartCoroutine(DelayedLoad());
    }
    
    private System.Collections.IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(1.0f); // Attendre 1 seconde
        Debug.Log("Chargement différé des paramètres...");
        
        // Vérifie si le fichier existe avant de charger
        if (File.Exists(savePath))
        {
            Debug.Log($"Fichier de sauvegarde trouvé: {savePath}");
            LoadSettings();
        }
        else
        {
            Debug.Log($"Aucun fichier de sauvegarde trouvé: {savePath}");
            Debug.Log("Utilisation des paramètres par défaut");
        }
    }

    /// <summary>
    /// Sauvegarde les paramètres actuels de la planète dans un fichier JSON
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            if (planetGeneratorNetworked != null)
            {
                // Sauvegarde UNIQUEMENT les paramètres de PlanetGeneratorNetworked
                PlanetSettings settings = PlanetSettings.FromPlanetGeneratorNetworked(planetGeneratorNetworked);
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(savePath, json);
                Debug.Log($"Sauvegarde des paramètres de PlanetGeneratorNetworked (seed: {planetGeneratorNetworked.seed}, resolution: {planetGeneratorNetworked.resolution})");
                Debug.Log($"Paramètres sauvegardés dans : {savePath}");
            }
            else
            {
                Debug.LogError("PlanetGeneratorNetworked non trouvé pour la sauvegarde !");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde : {e.Message}");
        }
    }

    /// <summary>
    /// Charge les paramètres depuis le fichier JSON et les applique au générateur
    /// </summary>
    public void LoadSettings()
    {
        Debug.Log($"=== DÉBUT DU CHARGEMENT ===");
        Debug.Log($"Chemin du fichier: {savePath}");
        Debug.Log($"Fichier existe: {File.Exists(savePath)}");
        
        if (!File.Exists(savePath))
        {
            Debug.Log("Aucun fichier de sauvegarde trouvé");
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            Debug.Log($"Contenu du fichier JSON: {json}");
            
            PlanetSettings settings = JsonUtility.FromJson<PlanetSettings>(json);
            Debug.Log($"PlanetSettings parsé - Radius: {settings.radius}, Seed: {settings.seed}, Resolution: {settings.resolution}");
            
            if (planetGeneratorNetworked != null)
            {
                Debug.Log($"AVANT chargement - PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
                
                // Charge UNIQUEMENT dans PlanetGeneratorNetworked
                settings.ApplyToPlanetGeneratorNetworked(planetGeneratorNetworked);
                
                Debug.Log($"APRÈS chargement - PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
                Debug.Log($"Paramètres chargés dans PlanetGeneratorNetworked avec succès");
            }
            else
            {
                Debug.LogError("PlanetGeneratorNetworked non trouvé pour le chargement !");
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors du chargement : {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
        
        Debug.Log($"=== FIN DU CHARGEMENT ===");
    }

    /// <summary>
    /// Force la sauvegarde depuis PlanetGeneratorNetworked
    /// </summary>
    public void SaveFromNetworked()
    {
        if (planetGeneratorNetworked == null)
        {
            Debug.LogError("PlanetGeneratorNetworked non trouvé pour la sauvegarde !");
            return;
        }
        
        try
        {
            PlanetSettings settings = PlanetSettings.FromPlanetGeneratorNetworked(planetGeneratorNetworked);
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"Sauvegarde forcée depuis PlanetGeneratorNetworked (seed: {planetGeneratorNetworked.seed}, resolution: {planetGeneratorNetworked.resolution})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors de la sauvegarde forcée : {e.Message}");
        }
    }
    
    /// <summary>
    /// Force le chargement vers PlanetGeneratorNetworked
    /// </summary>
    public void LoadToNetworked()
    {
        Debug.Log($"=== CHARGEMENT FORCÉ VERS PLANETGENERATORNETWORKED ===");
        Debug.Log($"Chemin du fichier: {savePath}");
        
        if (planetGeneratorNetworked == null)
        {
            Debug.LogError("PlanetGeneratorNetworked non trouvé pour le chargement !");
            return;
        }
        
        if (!File.Exists(savePath))
        {
            Debug.Log("Aucun fichier de sauvegarde trouvé");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(savePath);
            Debug.Log($"Contenu du fichier JSON: {json}");
            
            PlanetSettings settings = JsonUtility.FromJson<PlanetSettings>(json);
            Debug.Log($"PlanetSettings parsé - Radius: {settings.radius}, Seed: {settings.seed}");
            
            Debug.Log($"AVANT chargement forcé - PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
            
            settings.ApplyToPlanetGeneratorNetworked(planetGeneratorNetworked);
            
            Debug.Log($"APRÈS chargement forcé - PlanetGeneratorNetworked - Radius: {planetGeneratorNetworked.radius}, Seed: {planetGeneratorNetworked.seed}");
            Debug.Log($"Chargement forcé vers PlanetGeneratorNetworked avec succès");
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors du chargement forcé : {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
        
        Debug.Log($"=== FIN DU CHARGEMENT FORCÉ ===");
    }

    /// <summary>
    /// Supprime le fichier de sauvegarde
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Fichier de sauvegarde supprimé");
        }
        else
        {
            Debug.Log("Aucun fichier de sauvegarde à supprimer");
        }
    }

    /// <summary>
    /// Vérifie si un fichier de sauvegarde existe
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }

    /// <summary>
    /// Retourne le chemin complet du fichier de sauvegarde
    /// </summary>
    public string GetSavePath()
    {
        return savePath;
    }

    /// <summary>
    /// Sauvegarde automatique appelée après génération (si autoSaveOnGenerate est activé)
    /// </summary>
    public void OnPlanetGenerated()
    {
        if (autoSaveOnGenerate)
        {
            SaveSettings();
        }
    }

    // Méthodes pour l'interface utilisateur
    [ContextMenu("Sauvegarder les paramètres")]
    public void SaveSettingsMenu()
    {
        SaveSettings();
    }

    [ContextMenu("Charger les paramètres")]
    public void LoadSettingsMenu()
    {
        LoadSettings();
    }

    [ContextMenu("Supprimer la sauvegarde")]
    public void DeleteSaveFileMenu()
    {
        DeleteSaveFile();
    }

    // Affichage d'informations (désactivé pour simplifier)
    // private void OnGUI() { ... }
}
