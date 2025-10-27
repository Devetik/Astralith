using UnityEngine;
using UnityEngine.UI;

public class ResolutionSlider : MonoBehaviour
{
    [Header("Références")]
    public Planet planet; // ← Assigne ta planète ici
    public Slider slider;
    public Text valueText; // Optionnel : pour afficher la valeur
    
    void Start()
    {
        // Vérifier que les références sont assignées
        if (planet == null)
        {
            Debug.LogError("ResolutionSlider : La référence Planet n'est pas assignée !");
            return;
        }
        
        if (slider == null)
        {
            Debug.LogError("ResolutionSlider : La référence Slider n'est pas assignée !");
            return;
        }
        
        // Initialiser le slider avec la résolution actuelle
        slider.value = planet.resolution;
        slider.minValue = 2;
        slider.maxValue = 256;
        slider.wholeNumbers = true; // Forcer les nombres entiers
        
        // Connecter l'événement
        slider.onValueChanged.AddListener(OnSliderChanged);
        
        // Mettre à jour le texte si présent
        UpdateValueText();
    }
    
    void OnSliderChanged(float value)
    {
        int newResolution = Mathf.RoundToInt(value);
        
        // Appeler la méthode SetResolution de la planète
        if (planet != null)
        {
            planet.SetResolution(newResolution);
        }
        
        // Mettre à jour le texte si présent
        UpdateValueText();
    }
    
    void UpdateValueText()
    {
        if (valueText != null && planet != null)
        {
            valueText.text = "Résolution: " + planet.resolution.ToString();
        }
    }
    
    // Méthode publique pour changer la résolution depuis l'extérieur
    public void SetResolutionFromSlider(float value)
    {
        OnSliderChanged(value);
    }
}
