using UnityEngine;

public class PlanetUI : MonoBehaviour
{
    [Header("Références")]
    public PlanetGenerator planetGenerator;
    public PlanetSaveManager saveManager;

    [Header("Interface")]
    public bool showUI = true;
    public KeyCode toggleUIKey = KeyCode.Tab;

    private bool uiVisible = true;

    private void Start()
    {
        // Trouve automatiquement les composants si non assignés
        if (planetGenerator == null)
            planetGenerator = FindObjectOfType<PlanetGenerator>();
        
        if (saveManager == null)
            saveManager = FindObjectOfType<PlanetSaveManager>();
    }

    private void Update()
    {
        // Toggle de l'interface avec la touche
        if (Input.GetKeyDown(toggleUIKey))
        {
            uiVisible = !uiVisible;
        }
    }

    // Interface utilisateur (désactivée pour simplifier)
    // private void OnGUI() { ... }
}
