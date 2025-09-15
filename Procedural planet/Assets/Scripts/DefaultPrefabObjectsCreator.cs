using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Object;

/// <summary>
/// Crée automatiquement le fichier DefaultPrefabObjects pour Fish-Networking
/// </summary>
public class DefaultPrefabObjectsCreator : MonoBehaviour
{
    [ContextMenu("Créer DefaultPrefabObjects")]
    public void CreateDefaultPrefabObjects()
    {
        Debug.Log("=== CRÉATION DU DEFAULTPREFABOBJECTS ===");
        
        // Trouve le NetworkManager
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager non trouvé ! Créez d'abord un NetworkManager.");
            return;
        }
        
        // Crée le DefaultPrefabObjects
        var defaultPrefabObjects = ScriptableObject.CreateInstance<DefaultPrefabObjects>();
        
        // Sauvegarde le fichier
        string path = "Assets/Scripts/DefaultPrefabObjects.asset";
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(defaultPrefabObjects, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        // Assigne le fichier au NetworkManager
        networkManager.SpawnablePrefabs = defaultPrefabObjects;
        
        Debug.Log($"DefaultPrefabObjects créé et assigné : {path}");
        Debug.Log("Le NetworkManager est maintenant configuré correctement !");
#else
        Debug.LogError("Ce script ne peut être exécuté qu'en mode éditeur !");
#endif
        
        Debug.Log("=== CRÉATION TERMINÉE ===");
    }
}
