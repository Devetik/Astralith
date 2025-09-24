using UnityEngine;
using UnityEngine.UI;

namespace Geodesic
{
    /// <summary>
    /// Interface utilisateur pour le système de planète sphérique géodésique
    /// </summary>
    public class GeodesicSphereUI : MonoBehaviour
    {
        [Header("Composants UI")]
        public Dropdown cellConfigDropdown;
        public InputField customFrequencyInput;
        public Slider cellSizeSlider;
        public Slider landRatioSlider;
        public Button generateButton;
        public Button newSeedButton;
        public Text infoText;
        
        [Header("Références")]
        public GeodesicSphereSetup sphereSetup;
        public GeodesicSpherePlanetGenerator sphereGenerator;
        
        [Header("Debug")]
        public bool showDebugInfo = true;
        
        private void Start()
        {
            InitializeUI();
            FindComponents();
            UpdateUI();
        }
        
        /// <summary>
        /// Initialise l'interface utilisateur
        /// </summary>
        private void InitializeUI()
        {
            // Configure le dropdown des configurations
            if (cellConfigDropdown != null)
            {
                cellConfigDropdown.ClearOptions();
                cellConfigDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "252 cellules",
                    "642 cellules", 
                    "1212 cellules",
                    "2252 cellules",
                    "4002 cellules",
                    "6002 cellules",
                    "9002 cellules",
                    "16002 cellules",
                    "25002 cellules",
                    "40002 cellules",
                    "Custom"
                });
                cellConfigDropdown.value = 2; // 1212 par défaut
                cellConfigDropdown.onValueChanged.AddListener(OnCellConfigChanged);
            }
            
            // Configure le slider de taille des cellules
            if (cellSizeSlider != null)
            {
                cellSizeSlider.minValue = 0.1f;
                cellSizeSlider.maxValue = 5f;
                cellSizeSlider.value = 1f;
                cellSizeSlider.onValueChanged.AddListener(OnCellSizeChanged);
            }
            
            // Configure le slider de ratio terre
            if (landRatioSlider != null)
            {
                landRatioSlider.minValue = 0f;
                landRatioSlider.maxValue = 1f;
                landRatioSlider.value = 0.3f;
                landRatioSlider.onValueChanged.AddListener(OnLandRatioChanged);
            }
            
            // Configure les boutons
            if (generateButton != null)
            {
                generateButton.onClick.AddListener(OnGenerateClicked);
            }
            
            if (newSeedButton != null)
            {
                newSeedButton.onClick.AddListener(OnNewSeedClicked);
            }
            
            // Configure l'input personnalisé
            if (customFrequencyInput != null)
            {
                customFrequencyInput.text = "11";
                customFrequencyInput.onValueChanged.AddListener(OnCustomFrequencyChanged);
            }
        }
        
        /// <summary>
        /// Trouve les composants automatiquement
        /// </summary>
        private void FindComponents()
        {
            if (sphereSetup == null)
            {
                sphereSetup = FindObjectOfType<GeodesicSphereSetup>();
            }
            
            if (sphereGenerator == null)
            {
                sphereGenerator = FindObjectOfType<GeodesicSpherePlanetGenerator>();
            }
        }
        
        /// <summary>
        /// Met à jour l'interface utilisateur
        /// </summary>
        private void UpdateUI()
        {
            if (sphereSetup == null) return;
            
            // Met à jour le dropdown
            if (cellConfigDropdown != null)
            {
                cellConfigDropdown.value = (int)sphereSetup.cellConfig;
            }
            
            // Met à jour les sliders
            if (cellSizeSlider != null)
            {
                cellSizeSlider.value = sphereSetup.cellSize;
            }
            
            if (landRatioSlider != null)
            {
                landRatioSlider.value = sphereSetup.landRatio;
            }
            
            // Met à jour l'input personnalisé
            if (customFrequencyInput != null)
            {
                customFrequencyInput.text = sphereSetup.customFrequency.ToString();
                customFrequencyInput.gameObject.SetActive(sphereSetup.cellConfig == GeodesicSphereSetup.CellConfiguration.Custom);
            }
            
            // Met à jour le texte d'information
            UpdateInfoText();
        }
        
        /// <summary>
        /// Met à jour le texte d'information
        /// </summary>
        private void UpdateInfoText()
        {
            if (infoText == null || sphereSetup == null) return;
            
            int cellCount = sphereSetup.GetCellCount();
            
            infoText.text = $"Cellules: {cellCount}\n" +
                           $"Taille: {sphereSetup.cellSize:F1}\n" +
                           $"Terre: {sphereSetup.landRatio * 100f:F1}%";
        }
        
        /// <summary>
        /// Appelé quand la configuration de cellules change
        /// </summary>
        private void OnCellConfigChanged(int value)
        {
            if (sphereSetup == null) return;
            
            sphereSetup.cellConfig = (GeodesicSphereSetup.CellConfiguration)value;
            
            // Affiche/masque l'input personnalisé
            if (customFrequencyInput != null)
            {
                customFrequencyInput.gameObject.SetActive(value == 10); // Custom (index 10)
            }
            
            UpdateInfoText();
            
            if (showDebugInfo)
            {
                Debug.Log($"Configuration changée: {sphereSetup.GetConfigName()}");
            }
        }
        
        /// <summary>
        /// Appelé quand la taille des cellules change
        /// </summary>
        private void OnCellSizeChanged(float value)
        {
            if (sphereSetup == null) return;
            
            sphereSetup.cellSize = value;
            UpdateInfoText();
        }
        
        /// <summary>
        /// Appelé quand le ratio terre change
        /// </summary>
        private void OnLandRatioChanged(float value)
        {
            if (sphereSetup == null) return;
            
            sphereSetup.landRatio = value;
            UpdateInfoText();
        }
        
        /// <summary>
        /// Appelé quand la fréquence personnalisée change
        /// </summary>
        private void OnCustomFrequencyChanged(string value)
        {
            if (sphereSetup == null) return;
            
            if (int.TryParse(value, out int freq))
            {
                sphereSetup.customFrequency = freq;
                UpdateInfoText();
            }
        }
        
        /// <summary>
        /// Appelé quand le bouton générer est cliqué
        /// </summary>
        private void OnGenerateClicked()
        {
            if (sphereSetup == null) return;
            
            sphereSetup.SetupSphereSystem();
            
            if (showDebugInfo)
            {
                Debug.Log("Génération de planète lancée !");
            }
        }
        
        /// <summary>
        /// Appelé quand le bouton nouvelle seed est cliqué
        /// </summary>
        private void OnNewSeedClicked()
        {
            if (sphereSetup == null) return;
            
            sphereSetup.GenerateNewSeed();
            
            if (showDebugInfo)
            {
                Debug.Log($"Nouvelle seed: {sphereSetup.seed}");
            }
        }
        
        /// <summary>
        /// Met à jour l'interface (peut être appelé depuis l'extérieur)
        /// </summary>
        public void RefreshUI()
        {
            UpdateUI();
        }
    }
}
