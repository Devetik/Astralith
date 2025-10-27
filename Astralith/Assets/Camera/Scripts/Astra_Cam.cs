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

        [SerializeField] float ZoomSpeed = 0.5f;
        
        [Header("🌍 Drag Organique")]
        [SerializeField] float dragSensitivity = 1f; // Sensibilité de base du drag
        [SerializeField] float minDragSensitivity = 0.1f; // Sensibilité minimale (très proche)
        [SerializeField] float maxDragSensitivity = 5f; // Sensibilité maximale (très loin)
        
        [Header("📷 Orientation de la Caméra")]
        [SerializeField] Vector3 lookAtOffset = Vector3.zero; // Offset du point de regard (X, Y, Z)
        [SerializeField] bool useLookAtOffset = false; // Activer l'offset du point de regard
        
        [Header("🌍 Rayon d'Orbite")]
        [Range(0f, 1000f)]
        [SerializeField] float orbitRadius = 400f; // Rayon de l'orbite (0-1000)
        
        // Variables pour l'offset
        private Transform lookAtOffsetTransform;
        private bool lookAtOffsetInitialized = false;


        bool middleClickInput = false;
        Vector2 scrollInput;
        Vector2 lookInput;
        
        // Variables pour le drag organique
        private Vector2 lastMousePosition;
        private bool isDragging = false;


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
            UpdateLookAtOffset();
            UpdateOrbitRadius();
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
            if (Mathf.Abs(scrollInput.y) > 0.01f)
            {
                InputAxis axis = OrbitalFollow.RadialAxis;
                axis.Value -= scrollInput.y * ZoomSpeed;
                axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);
                OrbitalFollow.RadialAxis = axis;
            }
        }
        
        void UpdateLookAtOffset()
        {
            if (!useLookAtOffset || CinemachineCamera == null || CameraTarget == null) 
            {
                // Si l'offset est désactivé, nettoyer
                if (lookAtOffsetInitialized && lookAtOffsetTransform != null)
                {
                    CinemachineCamera.LookAt = CameraTarget; // Retourner au target original
                    DestroyImmediate(lookAtOffsetTransform.gameObject);
                    lookAtOffsetTransform = null;
                    lookAtOffsetInitialized = false;
                }
                return;
            }
            
            // Créer l'objet LookAt une seule fois
            if (!lookAtOffsetInitialized)
            {
                GameObject lookAtObject = new GameObject("LookAtOffset");
                lookAtOffsetTransform = lookAtObject.transform;
                CinemachineCamera.LookAt = lookAtOffsetTransform;
                lookAtOffsetInitialized = true;
                Debug.Log("LookAtOffset créé");
            }
            
            // Mettre à jour la position du point de regard
            if (lookAtOffsetTransform != null)
            {
                Vector3 lookAtPosition = CameraTarget.position + lookAtOffset;
                lookAtOffsetTransform.position = lookAtPosition;
            }
        }
        
        void UpdateOrbitRadius()
        {
            if (OrbitalFollow == null) return;
            
            // Mettre à jour directement la propriété Radius de CinemachineOrbitalFollow
            OrbitalFollow.Radius = orbitRadius;
        }
            
    }
}
