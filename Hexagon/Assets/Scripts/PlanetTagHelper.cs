using UnityEngine;
using HexasphereProcedural;

namespace ProceduralHexasphereDemo {
    
    /// <summary>
    /// Utilitaire pour ajouter automatiquement le tag "Planet" aux planètes
    /// </summary>
    public class PlanetTagHelper : MonoBehaviour {
        
        [Header("🏷️ Configuration Tag")]
        [SerializeField] private bool autoTagOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        void Start() {
            if (autoTagOnStart) {
                TagAllPlanets();
            }
        }
        
        public void TagAllPlanets() {
            // Trouver toutes les planètes HexaAstralith
            HexaAstralithPlanet[] planets = FindObjectsByType<HexaAstralithPlanet>(FindObjectsSortMode.None);
            
            int taggedCount = 0;
            foreach (HexaAstralithPlanet planet in planets) {
                if (planet != null && planet.gameObject != null) {
                    // Ajouter le tag "Planet" si il n'existe pas déjà
                    if (!planet.gameObject.CompareTag("Planet")) {
                        // Créer le tag "Planet" s'il n'existe pas
                        CreatePlanetTagIfNeeded();
                        
                        // Assigner le tag
                        planet.gameObject.tag = "Planet";
                        taggedCount++;
                        
                        if (showDebugInfo) {
                            Debug.Log($"🏷️ Tag 'Planet' ajouté à: {planet.gameObject.name}");
                        }
                    }
                }
            }
            
            if (showDebugInfo) {
                Debug.Log($"✅ {taggedCount} planètes taguées avec 'Planet'");
            }
        }
        
        void CreatePlanetTagIfNeeded() {
            // Cette méthode ne peut pas créer de tags à l'exécution
            // L'utilisateur doit créer le tag "Planet" dans Unity
            // On affiche juste un message d'aide
            if (showDebugInfo) {
                Debug.Log("💡 Pour créer le tag 'Planet': Edit > Project Settings > Tags and Layers > Tags > + > Planet");
            }
        }
        
        void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("🏷️ Planet Tag Helper", GUI.skin.box);
            GUILayout.Space(10);
            
            if (GUILayout.Button("🏷️ Tagger Toutes les Planètes")) {
                TagAllPlanets();
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("Instructions:");
            GUILayout.Label("1. Créez le tag 'Planet' dans Unity");
            GUILayout.Label("2. Cliquez 'Tagger Toutes les Planètes'");
            GUILayout.Label("3. Utilisez la caméra pour sélectionner");
            
            GUILayout.Space(10);
            
            GUILayout.Label("Statut:");
            GUILayout.Label($"Auto-tag: {(autoTagOnStart ? "ON" : "OFF")}");
            GUILayout.Label($"Debug: {(showDebugInfo ? "ON" : "OFF")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
