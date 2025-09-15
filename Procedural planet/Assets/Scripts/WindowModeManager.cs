using UnityEngine;

/// <summary>
/// Gestionnaire du mode fenêtré pour les builds
/// </summary>
public class WindowModeManager : MonoBehaviour
{
    [Header("Configuration Fenêtre")]
    public bool forceWindowedMode = true;
    public int windowWidth = 1280;
    public int windowHeight = 720;
    public bool centerWindow = true;
    
    void Start()
    {
        if (forceWindowedMode)
        {
            SetWindowedMode();
        }
    }
    
    void Update()
    {
        // Bascule avec F11
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }
        
        // Bascule avec Alt+Enter
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
        {
            ToggleFullscreen();
        }
    }
    
    /// <summary>
    /// Force le mode fenêtré
    /// </summary>
    public void SetWindowedMode()
    {
        // Force le mode fenêtré
        Screen.fullScreen = false;
        
        // Définit la taille de la fenêtre
        Screen.SetResolution(windowWidth, windowHeight, false);
        
        // Centre la fenêtre (Windows uniquement)
        if (centerWindow && Application.platform == RuntimePlatform.WindowsPlayer)
        {
            CenterWindow();
        }
        
        Debug.Log($"Mode fenêtré activé : {windowWidth}x{windowHeight}");
    }
    
    /// <summary>
    /// Centre la fenêtre sur l'écran (Windows uniquement)
    /// </summary>
    private void CenterWindow()
    {
        // Cette fonctionnalité nécessite des plugins Windows spécifiques
        // Pour l'instant, on laisse Unity gérer le positionnement
        Debug.Log("Fenêtre centrée automatiquement par Unity");
    }
    
    /// <summary>
    /// Bascule entre mode plein écran et fenêtré
    /// </summary>
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Debug.Log($"Mode plein écran : {Screen.fullScreen}");
    }
}
