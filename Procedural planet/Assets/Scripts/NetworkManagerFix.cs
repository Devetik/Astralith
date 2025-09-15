using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Object;

/// <summary>
/// Script pour corriger rapidement les erreurs de NetworkManager
/// </summary>
public class NetworkManagerFix : MonoBehaviour
{
    [ContextMenu("Corriger NetworkManager")]
    public void FixNetworkManager()
    {
        Debug.Log("=== CORRECTION DU NETWORKMANAGER ===");
        
        // Trouve le NetworkManager
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager non trouvé ! Créez d'abord un NetworkManager.");
            return;
        }
        
        // Vérifie si SpawnablePrefabs est null
        if (networkManager.SpawnablePrefabs == null)
        {
            Debug.Log("SpawnablePrefabs est null, création du DefaultPrefabObjects...");
            
#if UNITY_EDITOR
            // Crée le DefaultPrefabObjects
            var defaultPrefabObjects = ScriptableObject.CreateInstance<DefaultPrefabObjects>();
            
            // Sauvegarde le fichier
            string path = "Assets/Scripts/DefaultPrefabObjects.asset";
            UnityEditor.AssetDatabase.CreateAsset(defaultPrefabObjects, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            // Assigne le fichier au NetworkManager
            networkManager.SpawnablePrefabs = defaultPrefabObjects;
            
            Debug.Log($"✅ DefaultPrefabObjects créé et assigné : {path}");
            Debug.Log("✅ NetworkManager corrigé !");
#else
            Debug.LogError("Ce script ne peut être exécuté qu'en mode éditeur !");
#endif
        }
        else
        {
            Debug.Log("✅ SpawnablePrefabs est déjà configuré !");
        }
        
        Debug.Log("=== CORRECTION TERMINÉE ===");
    }
}
