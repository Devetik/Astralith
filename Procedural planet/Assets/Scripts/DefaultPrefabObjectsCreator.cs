using UnityEngine;
using FishNet.Managing;
using FishNet.Managing.Object;

public class DefaultPrefabObjectsCreator : MonoBehaviour
{
    [Header("Configuration")]
    public bool createOnStart = true;
    public bool addPlanetGeneratorToPrefabs = true;

    private void Start()
    {
        if (createOnStart)
        {
            CreateDefaultPrefabObjects();
        }
    }

    /// <summary>
    /// Configure le NetworkManager avec DefaultPrefabObjects
    /// </summary>
    [ContextMenu("Configurer NetworkManager")]
    public void CreateDefaultPrefabObjects()
    {
        // Trouve le NetworkManager
        var networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager non trouvé dans la scène !");
            return;
        }

        // Vérifie si le fichier DefaultPrefabObjects existe
        var existingPrefabs = Resources.Load<DefaultPrefabObjects>("DefaultPrefabObjects");
        if (existingPrefabs != null)
        {
            // Configure le NetworkManager avec le fichier existant
            networkManager.SpawnablePrefabs = existingPrefabs;
            Debug.Log("NetworkManager configuré avec DefaultPrefabObjects existant");
        }
        else
        {
            Debug.LogWarning("DefaultPrefabObjects non trouvé. Vous devrez le configurer manuellement dans le NetworkManager.");
            Debug.Log("Instructions :");
            Debug.Log("1. Sélectionnez le NetworkManager dans la scène");
            Debug.Log("2. Dans l'inspecteur, assignez DefaultPrefabObjects au champ 'Spawnable Prefabs'");
        }
    }

    /// <summary>
    /// Instructions pour configurer manuellement le NetworkManager
    /// </summary>
    public void ShowInstructions()
    {
        Debug.Log("=== Instructions de Configuration NetworkManager ===");
        Debug.Log("1. Sélectionnez le NetworkManager dans la scène");
        Debug.Log("2. Dans l'inspecteur, trouvez le champ 'Spawnable Prefabs'");
        Debug.Log("3. Assignez le fichier 'DefaultPrefabObjects' à ce champ");
        Debug.Log("4. Si le fichier n'existe pas, créez-le via le menu Assets > Create > Fish-Networking > Default Prefab Objects");
    }

    // Interface utilisateur
    private void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 110));
        GUILayout.Label("NetworkManager Configurator", GUI.skin.box);
        
        if (GUILayout.Button("Configurer NetworkManager"))
        {
            CreateDefaultPrefabObjects();
        }
        
        if (GUILayout.Button("Afficher Instructions"))
        {
            ShowInstructions();
        }
        
        GUILayout.EndArea();
    }
}
