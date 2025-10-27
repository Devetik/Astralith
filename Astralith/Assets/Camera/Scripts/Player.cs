using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Astralith.RTSCamera
{
    public class Player : MonoBehaviour
    {  
        [Header("Movement")]
        [SerializeField] float MoveSpeed = 20f;
        [SerializeField] AnimationCurve MoveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);
        [SerializeField] float Acceleration = 10f;
        [SerializeField] float Deceleration = 20f;
        float decelerationMultiplier = 1f;
        Vector3 Velocity = Vector3.zero;
        [Space(10)]
        [SerializeField] float SprintSpeedMultiplier = 3f;

        [Space(10)]
        [SerializeField] bool EnableEdgeMovement = false;
        [SerializeField] float EdgeScrollingMargin = 15f;
        Vector2 edgeScrollInput;

        [Header("Orbit")]
        [SerializeField] float OrbitSensivity = 0.5f;
        [SerializeField] float OrbitSmoothing = 5f;

        [Header("Zoom")]
        [SerializeField] float ZoomSpeed = 0.5f;
        [SerializeField] float ZoomSmoothing = 5f;
        float CurrentZoomSpeed = 0f;

        public float ZoomLevel
        {
            get
            {
                InputAxis axis = OrbitalFollow.RadialAxis;
                return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
            }
        }

        [Header("Component")]
        [SerializeField] Transform CameraTarget;
        [SerializeField] Transform CameraTargetPlanet;
        [SerializeField] CinemachineOrbitalFollow OrbitalFollow;

#region Input

        [Header("Input")]
        Vector2 moveInput;
        Vector2 scrollInput;
        Vector2 lookInput;
        bool middleClickInput = false;
        bool sprintInput;

        void OnSprint(InputValue value)
        {
            sprintInput = value.isPressed;
        }

        void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        void OnScrollWheel(InputValue value)
        {
            scrollInput = value.Get<Vector2>();
        }

        void OnMiddleClick(InputValue value)
        {
            middleClickInput = value.isPressed;
        }

#endregion

#region Unity Methods
        void LateUpdate()
        {
            float deltaTime = Time.unscaledDeltaTime;

            Debug.Log(ZoomLevel);

            UpdateEdgeScrolling();
            UpdateOrbit(deltaTime);
            UpdateMovement(deltaTime);
            UpdateZoom(deltaTime);
        }
#endregion

#region Control Methods

        void UpdateEdgeScrolling()
        {
            if(!EnableEdgeMovement){return;}
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            edgeScrollInput = Vector2.zero;

            if(mousePosition.x <= EdgeScrollingMargin)
            {
                edgeScrollInput.x = -1f;
            }
            else if(mousePosition.x >= Screen.width - EdgeScrollingMargin)
            {
                edgeScrollInput.x = 1f;
            }

            if(mousePosition.y <= EdgeScrollingMargin)
            {
                edgeScrollInput.y = -1f;
            }
            else if(mousePosition.y >= Screen.height - EdgeScrollingMargin)
            {
                edgeScrollInput.y = 1f;
            }
        }

        void UpdateMovement(float deltaTime)
        {
            Vector3 forward = Camera.main.transform.forward;   
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = Camera.main.transform.right;   
            right.y = 0f;
            right.Normalize();

            Vector3 inputVector = new Vector3(moveInput.x + edgeScrollInput.x, 0, moveInput.y + edgeScrollInput.y);

            float zoomMultiplier = MoveSpeedZoomCurve.Evaluate(ZoomLevel);

            Vector3 targetVelocity = inputVector * MoveSpeed * zoomMultiplier;

            float sprintFactor = 1f;

            if(sprintInput)
            {
                targetVelocity *= SprintSpeedMultiplier;

                sprintFactor = SprintSpeedMultiplier;
            }

            if(inputVector.sqrMagnitude > 0.01f)
            {
                Velocity = Vector3.MoveTowards(Velocity, targetVelocity, Acceleration * sprintFactor * deltaTime);
                if(sprintInput)
                {
                    decelerationMultiplier = SprintSpeedMultiplier;
                }
            }
            else
            {
                Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, Deceleration * decelerationMultiplier * deltaTime);
            }

            Vector3 motion = Velocity * deltaTime;
            CameraTarget.position += forward * motion.z + right * motion.x;

            if(Velocity.sqrMagnitude <= 0.01f)
            {
                decelerationMultiplier = 1f;
            }
        }
        
        void UpdateOrbit(float deltaTime)

        {
           Vector2 orbitInput = lookInput * (middleClickInput ? 1f : 0f);
           orbitInput *= OrbitSensivity;

           InputAxis horizontalAxis = OrbitalFollow.HorizontalAxis;
           InputAxis verticalAxis = OrbitalFollow.VerticalAxis;

           //horizontalAxis.Value += orbitInput.x;
           //verticalAxis.Value -= orbitInput.y;
           horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + orbitInput.x, OrbitSmoothing * deltaTime);
           verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - orbitInput.y, OrbitSmoothing * deltaTime);

           //horizontalAxis.Value = Mathf.Clamp(horizontalAxis.Value, horizontalAxis.Range.x, horizontalAxis.Range.y);
           verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

           OrbitalFollow.HorizontalAxis = horizontalAxis;
           OrbitalFollow.VerticalAxis = verticalAxis;
        }   

        void UpdateZoom(float deltaTime)
        {
            InputAxis axis = OrbitalFollow.RadialAxis;

            float targetZoomSpeed = 0f;

            if(Mathf.Abs(scrollInput.y) > 0.01f)
            {
                targetZoomSpeed = ZoomSpeed * scrollInput.y;
            }

            CurrentZoomSpeed = Mathf.Lerp(CurrentZoomSpeed, targetZoomSpeed, ZoomSmoothing * deltaTime);

            axis.Value -= CurrentZoomSpeed;
            axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);

            OrbitalFollow.RadialAxis = axis;
        }

#endregion
    }
}
