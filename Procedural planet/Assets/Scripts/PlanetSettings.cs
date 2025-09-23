using System;
using UnityEngine;

[Serializable]
public class PlanetSettings
{
    [Header("Paramètres de base")]
    public int resolution = 64;
    public float radius = 1.0f;
    public int seed = 12345;

    [Header("Bruit de base")]
    public int octaves = 4;
    public float lacunarity = 2.0f;
    public float persistence = 0.5f;

    [Header("Continents (basse fréquence)")]
    public float continentFreq = 0.25f;
    public float continentAmp = 0.15f;
    // AnimationCurve temporairement désactivée
    // public AnimationCurveData continentCurve = new AnimationCurveData();

    [Header("Montagnes (haute fréquence, sur la terre)")]
    public float mountainFreq = 2.0f;
    public float mountainAmp = 0.10f;
    public float mountainMaskFreq = 0.6f;
    public float mountainMaskPower = 2.0f;

    [Header("Déformation (optionnel)")]
    public bool useWarp = false;
    public float warpFreq = 0.1f;
    public float warpStrength = 0.5f;

    [Header("Océans")]
    public float oceanLevel = 0.05f;
    
    [Header("Configuration Réseau")]
    public string serverIP = "127.0.0.1";
    public ushort serverPort = 7772;

    // Constructeur par défaut
    public PlanetSettings()
    {
        // Initialise la courbe par défaut (temporairement désactivée)
        // continentCurve = new AnimationCurveData();
        // continentCurve.keys = new KeyframeData[]
        // {
        //     new KeyframeData(0f, 0f, 0f, 0f),
        //     new KeyframeData(1f, 1f, 0f, 0f)
        // };
    }

    // Méthodes pour PlanetGenerator supprimées pour éviter les conflits
    // Seul PlanetGeneratorNetworked est utilisé pour la sauvegarde
    
    /// <summary>
    /// Crée un PlanetSettings à partir d'un PlanetGeneratorNetworked
    /// </summary>
    public static PlanetSettings FromPlanetGeneratorNetworked(PlanetGeneratorNetworked generator)
    {
        var settings = new PlanetSettings();
        
        settings.resolution = generator.resolution;
        settings.radius = generator.radius;
        settings.seed = generator.seed;
        settings.octaves = generator.octaves;
        settings.lacunarity = generator.lacunarity;
        settings.persistence = generator.persistence;
        settings.continentFreq = generator.continentFreq;
        settings.continentAmp = generator.continentAmp;
        // AnimationCurve temporairement désactivée
        // settings.continentCurve = AnimationCurveData.FromAnimationCurve(generator.continentCurve);
        settings.mountainFreq = generator.mountainFreq;
        settings.mountainAmp = generator.mountainAmp;
        settings.mountainMaskFreq = generator.mountainMaskFreq;
        settings.mountainMaskPower = generator.mountainMaskPower;
        settings.useWarp = generator.useWarp;
        settings.warpFreq = generator.warpFreq;
        settings.warpStrength = generator.warpStrength;
        settings.oceanLevel = generator.oceanLevel;
        
        // Configuration réseau (récupérée depuis PlanetNetworkManager)
        var networkManager = UnityEngine.Object.FindFirstObjectByType<PlanetNetworkManager>();
        if (networkManager != null)
        {
            settings.serverIP = networkManager.serverIP;
            settings.serverPort = networkManager.serverPort;
        }
        
        settings.landMaterialPath = "";
        settings.waterMaterialPath = "";
        
        return settings;
    }
    
    /// <summary>
    /// Applique les paramètres à un PlanetGeneratorNetworked
    /// </summary>
    public void ApplyToPlanetGeneratorNetworked(PlanetGeneratorNetworked generator)
    {
        generator.resolution = this.resolution;
        generator.radius = this.radius;
        generator.seed = this.seed;
        generator.octaves = this.octaves;
        generator.lacunarity = this.lacunarity;
        generator.persistence = this.persistence;
        generator.continentFreq = this.continentFreq;
        generator.continentAmp = this.continentAmp;
        // AnimationCurve temporairement désactivée
        // generator.continentCurve = this.continentCurve.ToAnimationCurve();
        generator.mountainFreq = this.mountainFreq;
        generator.mountainAmp = this.mountainAmp;
        generator.mountainMaskFreq = this.mountainMaskFreq;
        generator.mountainMaskPower = this.mountainMaskPower;
        generator.useWarp = this.useWarp;
        generator.warpFreq = this.warpFreq;
        generator.warpStrength = this.warpStrength;
        generator.oceanLevel = this.oceanLevel;
        
        // Configuration réseau (appliquée au PlanetNetworkManager)
        var networkManager = UnityEngine.Object.FindFirstObjectByType<PlanetNetworkManager>();
        if (networkManager != null)
        {
            networkManager.serverIP = this.serverIP;
            networkManager.serverPort = this.serverPort;
        }
    }

    // Chemins des matériaux (pour la sérialisation)
    public string landMaterialPath = "";
    public string waterMaterialPath = "";

    // Utilitaires pour les matériaux
    private static string GetMaterialPath(Material material)
    {
        if (material == null) return "";
        
#if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GetAssetPath(material);
        return path;
#else
        return material.name;
#endif
    }

    private static Material LoadMaterial(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
#else
        // En build, on essaie de charger depuis les Resources
        string resourcePath = path.Replace("Assets/Resources/", "").Replace(".mat", "");
        return Resources.Load<Material>(resourcePath);
#endif
    }
}