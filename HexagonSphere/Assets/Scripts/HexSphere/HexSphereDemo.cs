using UnityEngine;
using System.Collections;

namespace HexSphere
{
    /// <summary>
    /// Script de démonstration pour le système HexSphere
    /// </summary>
    public class HexSphereDemo : MonoBehaviour
    {
        [Header("Références")]
        public HexSphereManager sphereManager;
        public HexSphereGenerator sphereGenerator;
        
        [Header("Démonstration")]
        public bool autoDemo = false;
        public float demoInterval = 2f;
        
        [Header("Effets")]
        public bool enableColorCycling = false;
        public Gradient colorCycleGradient;
        public float colorCycleSpeed = 1f;
        
        [Header("Animation")]
        public bool enablePulseAnimation = false;
        public float pulseSpeed = 1f;
        public float pulseIntensity = 0.1f;
        
        private float demoTimer = 0f;
        private float colorCycleTimer = 0f;
        private float pulseTimer = 0f;
        private Vector3 originalScale;
        
        private void Start()
        {
            // Initialiser les références
            if (sphereManager == null)
                sphereManager = GetComponent<HexSphereManager>();
            
            if (sphereGenerator == null)
                sphereGenerator = GetComponent<HexSphereGenerator>();
            
            originalScale = transform.localScale;
            
            // Démarrer la démonstration automatique si activée
            if (autoDemo)
            {
                StartCoroutine(AutoDemoCoroutine());
            }
        }
        
        private void Update()
        {
            HandleInput();
            UpdateColorCycling();
            UpdatePulseAnimation();
        }
        
        /// <summary>
        /// Gère les entrées pour la démonstration
        /// </summary>
        private void HandleInput()
        {
            // Touches de démonstration
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                DemoRandomHighlight();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DemoColorGradient();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                DemoElevationEffect();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                DemoNeighborHighlight();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                DemoRadiusHighlight();
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleAutoDemo();
            }
        }
        
        /// <summary>
        /// Met à jour le cycle de couleurs
        /// </summary>
        private void UpdateColorCycling()
        {
            if (!enableColorCycling || sphereGenerator == null) return;
            
            colorCycleTimer += Time.deltaTime * colorCycleSpeed;
            float t = (Mathf.Sin(colorCycleTimer) + 1f) * 0.5f;
            Color cycleColor = colorCycleGradient.Evaluate(t);
            
            // Appliquer la couleur à toutes les cellules
            foreach (HexCell cell in sphereGenerator.hexCells)
            {
                cell.color = cycleColor;
            }
        }
        
        /// <summary>
        /// Met à jour l'animation de pulsation
        /// </summary>
        private void UpdatePulseAnimation()
        {
            if (!enablePulseAnimation) return;
            
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * pulseIntensity;
            transform.localScale = originalScale * pulse;
        }
        
        /// <summary>
        /// Démonstration: Surligner des cellules aléatoires
        /// </summary>
        [ContextMenu("Demo: Surligner Aléatoire")]
        public void DemoRandomHighlight()
        {
            if (sphereManager == null || sphereGenerator == null) return;
            
            // Sélectionner 5 cellules aléatoires
            int count = Mathf.Min(5, sphereGenerator.hexCells.Count);
            System.Collections.Generic.List<HexCell> randomCells = new System.Collections.Generic.List<HexCell>();
            
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, sphereGenerator.hexCells.Count);
                randomCells.Add(sphereGenerator.hexCells[randomIndex]);
            }
            
            sphereManager.HighlightCells(randomCells, Color.yellow);
            Debug.Log($"Cellules surlignées: {count}");
        }
        
        /// <summary>
        /// Démonstration: Appliquer un gradient de couleurs
        /// </summary>
        [ContextMenu("Demo: Gradient de Couleurs")]
        public void DemoColorGradient()
        {
            if (sphereGenerator == null) return;
            
            // Appliquer un gradient basé sur la position Y
            foreach (HexCell cell in sphereGenerator.hexCells)
            {
                float normalizedY = (cell.center.y + 1f) / 2f; // Normaliser entre 0 et 1
                Color gradientColor = Color.Lerp(Color.blue, Color.red, normalizedY);
                cell.color = gradientColor;
            }
            
            Debug.Log("Gradient de couleurs appliqué");
        }
        
        /// <summary>
        /// Démonstration: Effet d'élévation
        /// </summary>
        [ContextMenu("Demo: Effet d'Élévation")]
        public void DemoElevationEffect()
        {
            if (sphereGenerator == null) return;
            
            // Appliquer une élévation basée sur la distance du centre
            foreach (HexCell cell in sphereGenerator.hexCells)
            {
                float distance = Vector3.Distance(cell.center, Vector3.zero);
                float elevation = Mathf.Sin(distance * 5f) * 0.1f;
                cell.ApplyElevation(elevation);
            }
            
            // Régénérer le mesh
            sphereGenerator.GenerateMesh();
            Debug.Log("Effet d'élévation appliqué");
        }
        
        /// <summary>
        /// Démonstration: Surligner les voisins
        /// </summary>
        [ContextMenu("Demo: Surligner Voisins")]
        public void DemoNeighborHighlight()
        {
            if (sphereManager == null || sphereGenerator == null) return;
            
            // Sélectionner une cellule aléatoire
            HexCell randomCell = sphereGenerator.hexCells[Random.Range(0, sphereGenerator.hexCells.Count)];
            
            // Trouver ses voisins
            System.Collections.Generic.List<HexCell> neighbors = sphereManager.GetNeighborCells(randomCell);
            
            // Surligner la cellule centrale et ses voisins
            neighbors.Add(randomCell);
            sphereManager.HighlightCells(neighbors, Color.green);
            
            Debug.Log($"Cellule centrale: {randomCell.index}, Voisins: {neighbors.Count - 1}");
        }
        
        /// <summary>
        /// Démonstration: Surligner dans un rayon
        /// </summary>
        [ContextMenu("Demo: Surligner par Rayon")]
        public void DemoRadiusHighlight()
        {
            if (sphereManager == null) return;
            
            // Point central aléatoire
            Vector3 center = Random.insideUnitSphere;
            float radius = 0.5f;
            
            // Trouver les cellules dans le rayon
            System.Collections.Generic.List<HexCell> cellsInRadius = sphereManager.GetCellsInRadius(center, radius);
            
            sphereManager.HighlightCells(cellsInRadius, Color.magenta);
            Debug.Log($"Cellules dans le rayon: {cellsInRadius.Count}");
        }
        
        /// <summary>
        /// Active/désactive la démonstration automatique
        /// </summary>
        public void ToggleAutoDemo()
        {
            autoDemo = !autoDemo;
            
            if (autoDemo)
            {
                StartCoroutine(AutoDemoCoroutine());
            }
            else
            {
                StopAllCoroutines();
            }
            
            Debug.Log($"Démonstration automatique: {(autoDemo ? "Activée" : "Désactivée")}");
        }
        
        /// <summary>
        /// Coroutine pour la démonstration automatique
        /// </summary>
        private IEnumerator AutoDemoCoroutine()
        {
            while (autoDemo)
            {
                yield return new WaitForSeconds(demoInterval);
                
                // Exécuter différentes démonstrations
                int demoType = Random.Range(1, 6);
                
                switch (demoType)
                {
                    case 1:
                        DemoRandomHighlight();
                        break;
                    case 2:
                        DemoColorGradient();
                        break;
                    case 3:
                        DemoElevationEffect();
                        break;
                    case 4:
                        DemoNeighborHighlight();
                        break;
                    case 5:
                        DemoRadiusHighlight();
                        break;
                }
            }
        }
        
        private void OnGUI()
        {
            if (!autoDemo) return;
            
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
            GUILayout.Label("=== HexSphere Demo ===", GUI.skin.box);
            GUILayout.Space(5);
            
            GUILayout.Label("Démonstrations:");
            GUILayout.Label("1 - Surligner Aléatoire");
            GUILayout.Label("2 - Gradient Couleurs");
            GUILayout.Label("3 - Effet Élévation");
            GUILayout.Label("4 - Surligner Voisins");
            GUILayout.Label("5 - Surligner Rayon");
            GUILayout.Label("SPACE - Toggle Auto");
            
            GUILayout.EndArea();
        }
    }
}
