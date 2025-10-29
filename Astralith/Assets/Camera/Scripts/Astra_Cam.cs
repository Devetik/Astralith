using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
namespace Astralith.PlanetCam
{

    public class Astra_Cam : MonoBehaviour
    {
        [SerializeField] Transform CameraTarget;
        [SerializeField] CinemachineOrbitalFollow OrbitalFollow;
        [SerializeField] CinemachineCamera CinemachineCamera;
        [SerializeField] CinemachineRotationComposer RotationComposer;

        [SerializeField] float ZoomSpeed = 0.01f;
        [SerializeField] float ZoomSmoothing = 5f;
        float CurrentZoomSpeed = 0f;
        
        [Header("🌍 Drag Organique")]
        [SerializeField] float dragSensitivity = 1f; // Sensibilité de base du drag
        [SerializeField] float minDragSensitivity = 0.1f; // Sensibilité minimale (très proche)
        [SerializeField] float maxDragSensitivity = 5f; // Sensibilité maximale (très loin)
        
        [Header("📷 Orientation de la Caméra")]
        [SerializeField] Vector3 lookAtOffset = Vector3.zero; // Offset du point de regard (X, Y, Z)
        [SerializeField] bool useLookAtOffset = false; // Activer l'offset du point de regard
        
        [Header("🌍 Rayon d'Orbite")]
        [Range(0f, 5f)]
        [SerializeField] float orbitRadius = 5f; // Rayon de l'orbite (0-1000)
        
        // Variables pour l'offset
        private Transform lookAtOffsetTransform;
        private bool lookAtOffsetInitialized = false;


        bool middleClickInput = false;
        Vector2 scrollInput;
        Vector2 lookInput;
        [SerializeField] Vector3 lookTest = Vector3.zero;
        
        // Variables pour le drag organique
        private Vector2 lastMousePosition;
        private bool isDragging = false;

        public float ZoomLevel
        {
            get
            {
                InputAxis axis = OrbitalFollow.RadialAxis;
                return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
            }
        }

        void OnMiddleClick(InputValue value)
        {
            middleClickInput = value.isPressed;
            
            if (middleClickInput)
            {
                // Commencer le drag
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            else
            {
                // Arrêter le drag
                isDragging = false;
            }
        }

        void OnScrollWheel(InputValue value)
        {
            scrollInput = value.Get<Vector2>();
        }

        void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        void LateUpdate()
        {
            float deltaTime = Time.unscaledDeltaTime;
            UpdateOrganicDrag(deltaTime);
            UpdateZoom(deltaTime);
            CalculatePlanetSurfaceDistance();
        }

        void UpdateOrganicDrag(float deltaTime)
        {
            if (!isDragging || OrbitalFollow == null) return;
            
            // Calculer le mouvement de la souris depuis la dernière frame
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            // Calculer la sensibilité basée sur la distance (zoom level)
            float currentDistance = OrbitalFollow.RadialAxis.Value;
            float minDistance = OrbitalFollow.RadialAxis.Range.x;
            float maxDistance = OrbitalFollow.RadialAxis.Range.y;
            
            // Plus on est loin, plus la sensibilité est élevée
            float distanceRatio = Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
            float dynamicSensitivity = Mathf.Lerp(minDragSensitivity, maxDragSensitivity, distanceRatio);
            
            // Appliquer le mouvement directement avec la sensibilité dynamique
            Vector2 dragMovement = mouseDelta * dynamicSensitivity * dragSensitivity;
            
            // Appliquer le mouvement à l'orbite
            InputAxis horizontalAxis = OrbitalFollow.HorizontalAxis;
            InputAxis verticalAxis = OrbitalFollow.VerticalAxis;
            
            // Mouvement horizontal (rotation autour de la planète)
            horizontalAxis.Value += dragMovement.x * 0.01f; // Facteur de conversion pixel -> degrés
            
            // Mouvement vertical (inclinaison)
            verticalAxis.Value -= dragMovement.y * 0.01f;
            verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);
            
            // Appliquer les changements
            OrbitalFollow.HorizontalAxis = horizontalAxis;
            OrbitalFollow.VerticalAxis = verticalAxis;
            
            // Mettre à jour la position de la souris pour la prochaine frame
            lastMousePosition = currentMousePosition;
        }

        void UpdateZoom(float deltaTime)
        {
            InputAxis axis = OrbitalFollow.RadialAxis;

            float targetZoomSpeed = 0f;

            if(Mathf.Abs(scrollInput.y) > 0.01f)
            {
                targetZoomSpeed = ZoomSpeed * scrollInput.y;
            }
            float normalizedZoomSpeed = Mathf.InverseLerp(0f, 0.25f, ZoomLevel);
            CurrentZoomSpeed = Mathf.Lerp(CurrentZoomSpeed*normalizedZoomSpeed, targetZoomSpeed, ZoomSmoothing * deltaTime);

            axis.Value -= CurrentZoomSpeed;
            axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);

            OrbitalFollow.RadialAxis = axis;

            // Normaliser ZoomLevel entre 0.25 et 0 pour la rotation
            float normalizedZoom = Mathf.InverseLerp(0f, 0.25f, ZoomLevel);
            RotationComposer.Composition.ScreenPosition.y = Mathf.Lerp(1.2f, 0f, normalizedZoom);
        }
               
        void CalculatePlanetSurfaceDistance()
        {
            if (CameraTarget == null || CinemachineCamera == null || ZoomLevel > 0.25f) return;
            
            // Obtenir la position de la caméra
            Vector3 cameraPosition = CinemachineCamera.transform.position;
            
            // Calculer la direction depuis la caméra vers le centre de la planète
            Vector3 directionToPlanet = (CameraTarget.position - cameraPosition).normalized;
            
            // Effectuer le raycast depuis la caméra vers la planète
            RaycastHit hit;
            float maxDistance = Vector3.Distance(cameraPosition, CameraTarget.position);
            
            if (Physics.Raycast(cameraPosition, directionToPlanet, out hit, maxDistance))
            {
                // Calculer la distance depuis le centre de la planète jusqu'au point d'impact sur la surface
                float distanceFromCenterToSurface = Vector3.Distance(CameraTarget.position, hit.point);
                OrbitalFollow.Radius = distanceFromCenterToSurface + orbitRadius;
                
                //Debug.Log($"Distance centre-planète → surface: {distanceFromCenterToSurface:F2} unités");
                //Debug.Log($"Objet touché: {hit.collider.name}");
                
                // Dessiner le raycast pour debug visuel
                //Debug.DrawRay(cameraPosition, directionToPlanet * Vector3.Distance(cameraPosition, hit.point), Color.red, 0.1f);
                //Debug.DrawRay(CameraTarget.position, (hit.point - CameraTarget.position), Color.yellow, 0.1f);
            }
            else
            {
                Debug.Log("Aucune surface de planète détectée depuis la caméra");
                
                // Dessiner le raycast complet si aucun objet n'est touché
                Debug.DrawRay(cameraPosition, directionToPlanet * maxDistance, Color.green, 0.1f);
            }
        }
            
    }
}
