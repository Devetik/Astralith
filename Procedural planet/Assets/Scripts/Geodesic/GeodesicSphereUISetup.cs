using UnityEngine;
using UnityEngine.UI;

namespace Geodesic
{
    /// <summary>
    /// Setup automatique pour l'interface utilisateur du système sphérique
    /// </summary>
    public class GeodesicSphereUISetup : MonoBehaviour
    {
        [Header("Configuration")]
        public bool setupOnStart = true;
        public bool showDebugInfo = true;
        
        [Header("Références")]
        public GeodesicSphereUI sphereUI;
        public Canvas canvas;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupUI();
            }
        }
        
        /// <summary>
        /// Configure l'interface utilisateur
        /// </summary>
        [ContextMenu("Setup Interface Utilisateur")]
        public void SetupUI()
        {
            if (showDebugInfo)
            {
                Debug.Log("=== CONFIGURATION INTERFACE SPHÉRIQUE ===");
            }
            
            // Trouve ou crée le canvas
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasGO = new GameObject("Canvas");
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<CanvasScaler>();
                    canvasGO.AddComponent<GraphicRaycaster>();
                    
                    if (showDebugInfo)
                    {
                        Debug.Log("Canvas créé !");
                    }
                }
            }
            
            // Trouve ou crée l'interface
            if (sphereUI == null)
            {
                sphereUI = FindObjectOfType<GeodesicSphereUI>();
                if (sphereUI == null)
                {
                    GameObject uiGO = new GameObject("GeodesicSphereUI");
                    uiGO.transform.SetParent(canvas.transform, false);
                    sphereUI = uiGO.AddComponent<GeodesicSphereUI>();
                    
                    if (showDebugInfo)
                    {
                        Debug.Log("Interface sphérique créée !");
                    }
                }
            }
            
            // Crée les éléments UI
            CreateUIElements();
            
            if (showDebugInfo)
            {
                Debug.Log("=== INTERFACE CONFIGURÉE ===");
            }
        }
        
        /// <summary>
        /// Crée les éléments de l'interface utilisateur
        /// </summary>
        private void CreateUIElements()
        {
            if (sphereUI == null || canvas == null) return;
            
            // Panel principal
            GameObject mainPanel = CreatePanel("MainPanel", canvas.transform);
            RectTransform panelRect = mainPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0.3f, 1);
            panelRect.offsetMin = new Vector2(10, 10);
            panelRect.offsetMax = new Vector2(-10, -10);
            
            // Titre
            CreateText("Title", "Planète Sphérique Géodésique", mainPanel.transform, 24, Color.white);
            
            // Dropdown configuration
            CreateDropdown("CellConfigDropdown", "Nombre de cellules:", mainPanel.transform, sphereUI);
            
            // Input fréquence personnalisée
            CreateInputField("CustomFrequencyInput", "Fréquence personnalisée:", mainPanel.transform, sphereUI);
            
            // Slider taille cellules
            CreateSlider("CellSizeSlider", "Taille cellules:", mainPanel.transform, 0.1f, 5f, 1f, sphereUI);
            
            // Slider ratio terre
            CreateSlider("LandRatioSlider", "Ratio terre:", mainPanel.transform, 0f, 1f, 0.3f, sphereUI);
            
            // Boutons
            CreateButton("GenerateButton", "Générer Planète", mainPanel.transform, sphereUI);
            CreateButton("NewSeedButton", "Nouvelle Seed", mainPanel.transform, sphereUI);
            
            // Texte d'information
            CreateText("InfoText", "Cellules: 1212\nTaille: 1.0\nTerre: 30.0%", mainPanel.transform, 14, Color.cyan);
            
            // Assigne les références
            AssignUIReferences();
        }
        
        /// <summary>
        /// Assigne les références aux composants UI
        /// </summary>
        private void AssignUIReferences()
        {
            if (sphereUI == null) return;
            
            // Trouve les éléments par nom
            sphereUI.cellConfigDropdown = GameObject.Find("CellConfigDropdown")?.GetComponent<Dropdown>();
            sphereUI.customFrequencyInput = GameObject.Find("CustomFrequencyInput")?.GetComponent<InputField>();
            sphereUI.cellSizeSlider = GameObject.Find("CellSizeSlider")?.GetComponent<Slider>();
            sphereUI.landRatioSlider = GameObject.Find("LandRatioSlider")?.GetComponent<Slider>();
            sphereUI.generateButton = GameObject.Find("GenerateButton")?.GetComponent<Button>();
            sphereUI.newSeedButton = GameObject.Find("NewSeedButton")?.GetComponent<Button>();
            sphereUI.infoText = GameObject.Find("InfoText")?.GetComponent<Text>();
            
            if (showDebugInfo)
            {
                Debug.Log("Références UI assignées !");
            }
        }
        
        /// <summary>
        /// Crée un panel
        /// </summary>
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            return panel;
        }
        
        /// <summary>
        /// Crée un texte
        /// </summary>
        private GameObject CreateText(string name, string text, Transform parent, int fontSize, Color color)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            Text textComponent = textGO.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleLeft;
            
            RectTransform rect = textGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.8f);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(10, 0);
            rect.offsetMax = new Vector2(-10, 0);
            
            return textGO;
        }
        
        /// <summary>
        /// Crée un dropdown
        /// </summary>
        private GameObject CreateDropdown(string name, string label, Transform parent, GeodesicSphereUI ui)
        {
            // Label
            CreateText(name + "Label", label, parent, 14, Color.white);
            
            // Dropdown
            GameObject dropdownGO = new GameObject(name);
            dropdownGO.transform.SetParent(parent, false);
            
            Dropdown dropdown = dropdownGO.AddComponent<Dropdown>();
            dropdown.AddOptions(new System.Collections.Generic.List<string>
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
            dropdown.value = 2; // 1212 par défaut
            
            RectTransform rect = dropdownGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.7f);
            rect.anchorMax = new Vector2(1, 0.8f);
            rect.offsetMin = new Vector2(10, 0);
            rect.offsetMax = new Vector2(-10, 0);
            
            return dropdownGO;
        }
        
        /// <summary>
        /// Crée un input field
        /// </summary>
        private GameObject CreateInputField(string name, string label, Transform parent, GeodesicSphereUI ui)
        {
            // Label
            CreateText(name + "Label", label, parent, 14, Color.white);
            
            // Input
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent, false);
            
            InputField input = inputGO.AddComponent<InputField>();
            input.text = "11";
            
            RectTransform rect = inputGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.6f);
            rect.anchorMax = new Vector2(1, 0.7f);
            rect.offsetMin = new Vector2(10, 0);
            rect.offsetMax = new Vector2(-10, 0);
            
            return inputGO;
        }
        
        /// <summary>
        /// Crée un slider
        /// </summary>
        private GameObject CreateSlider(string name, string label, Transform parent, float min, float max, float value, GeodesicSphereUI ui)
        {
            // Label
            CreateText(name + "Label", label, parent, 14, Color.white);
            
            // Slider
            GameObject sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent, false);
            
            Slider slider = sliderGO.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            
            RectTransform rect = sliderGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(1, 0.6f);
            rect.offsetMin = new Vector2(10, 0);
            rect.offsetMax = new Vector2(-10, 0);
            
            return sliderGO;
        }
        
        /// <summary>
        /// Crée un bouton
        /// </summary>
        private GameObject CreateButton(string name, string text, Transform parent, GeodesicSphereUI ui)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);
            
            Button button = buttonGO.AddComponent<Button>();
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
            
            GameObject buttonTextGO = new GameObject("Text");
            buttonTextGO.transform.SetParent(buttonGO.transform, false);
            
            Text buttonText = buttonTextGO.AddComponent<Text>();
            buttonText.text = text;
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 16;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0.3f);
            buttonRect.anchorMax = new Vector2(1, 0.4f);
            buttonRect.offsetMin = new Vector2(10, 0);
            buttonRect.offsetMax = new Vector2(-10, 0);
            
            RectTransform textRect = buttonTextGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return buttonGO;
        }
    }
}
