using UnityEngine;

namespace HexSphere
{
    /// <summary>
    /// Script utilitaire pour configurer rapidement une sphère hexagonale
    /// </summary>
    public class HexSphereSetup : MonoBehaviour
    {
        [Header("Configuration Rapide")]
        [Range(1, 5)]
        public int subdivisionLevel = 2;
        
        [Range(0.1f, 10f)]
        public float radius = 1f;
        
        [Range(0.1f, 2f)]
        public float hexSize = 0.3f;
        
        [Header("Matériaux")]
        public Material defaultMaterial;
        public bool createMaterial = true;
        
        [Header("Composants")]
        public bool addManager = true;
        public bool addDemo = false;
        public bool addCollider = true;
        
        [Header("Position")]
        public Vector3 spawnPosition = Vector3.zero;
        public bool useCurrentPosition = true;
        
        [ContextMenu("Créer Sphère Hexagonale")]
        public void CreateHexSphere()
        {
            // Créer le GameObject principal
            GameObject hexSphere = new GameObject("HexSphere");
            
            // Positionner
            if (useCurrentPosition)
            {
                hexSphere.transform.position = transform.position;
            }
            else
            {
                hexSphere.transform.position = spawnPosition;
            }
            
            // Ajouter le générateur
            HexSphereGenerator generator = hexSphere.AddComponent<HexSphereGenerator>();
            generator.subdivisionLevel = subdivisionLevel;
            generator.radius = radius;
            generator.hexSize = hexSize;
            
            // Créer le matériau si nécessaire
            if (createMaterial && defaultMaterial == null)
            {
                defaultMaterial = CreateDefaultMaterial();
            }
            
            if (defaultMaterial != null)
            {
                generator.hexMaterial = defaultMaterial;
            }
            
            // Ajouter le gestionnaire
            if (addManager)
            {
                HexSphereManager manager = hexSphere.AddComponent<HexSphereManager>();
                manager.sphereGenerator = generator;
            }
            
            // Ajouter le script de démonstration
            if (addDemo)
            {
                HexSphereDemo demo = hexSphere.AddComponent<HexSphereDemo>();
                demo.sphereManager = hexSphere.GetComponent<HexSphereManager>();
                demo.sphereGenerator = generator;
            }
            
            // Ajouter un collider
            if (addCollider)
            {
                SphereCollider collider = hexSphere.AddComponent<SphereCollider>();
                collider.radius = radius;
                collider.isTrigger = false;
            }
            
            // Générer la sphère
            generator.GenerateHexSphere();
            
            Debug.Log($"Sphère hexagonale créée: {hexSphere.name}");
        }
        
        /// <summary>
        /// Crée un matériau par défaut
        /// </summary>
        private Material CreateDefaultMaterial()
        {
            // Essayer d'utiliser URP d'abord
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            
            if (shader == null)
            {
                Debug.LogWarning("Aucun shader approprié trouvé, utilisation du shader par défaut");
                shader = Shader.Find("Legacy Shaders/Diffuse");
            }
            
            Material material = new Material(shader);
            material.name = "HexSphere Material";
            material.color = new Color(0.2f, 0.6f, 1f, 1f); // Bleu clair
            
            return material;
        }
        
        /// <summary>
        /// Crée une sphère hexagonale avec des paramètres prédéfinis
        /// </summary>
        [ContextMenu("Créer Sphère Simple")]
        public void CreateSimpleHexSphere()
        {
            subdivisionLevel = 1;
            radius = 1f;
            hexSize = 0.4f;
            CreateHexSphere();
        }
        
        /// <summary>
        /// Crée une sphère hexagonale détaillée
        /// </summary>
        [ContextMenu("Créer Sphère Détaillée")]
        public void CreateDetailedHexSphere()
        {
            subdivisionLevel = 3;
            radius = 1f;
            hexSize = 0.2f;
            CreateHexSphere();
        }
        
        /// <summary>
        /// Crée une sphère hexagonale pour la planète
        /// </summary>
        [ContextMenu("Créer Planète Hexagonale")]
        public void CreatePlanetHexSphere()
        {
            subdivisionLevel = 2;
            radius = 2f;
            hexSize = 0.3f;
            addDemo = true;
            CreateHexSphere();
        }
        
        /// <summary>
        /// Configure les paramètres pour une sphère de test
        /// </summary>
        [ContextMenu("Configurer pour Test")]
        public void ConfigureForTesting()
        {
            subdivisionLevel = 1;
            radius = 1f;
            hexSize = 0.5f;
            addManager = true;
            addDemo = true;
            addCollider = true;
        }
        
        /// <summary>
        /// Configure les paramètres pour une sphère de production
        /// </summary>
        [ContextMenu("Configurer pour Production")]
        public void ConfigureForProduction()
        {
            subdivisionLevel = 3;
            radius = 1f;
            hexSize = 0.2f;
            addManager = true;
            addDemo = false;
            addCollider = true;
        }
    }
}
