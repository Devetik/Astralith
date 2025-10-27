using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Unity.Cinemachine;

public class MobileOrbitZoom : MonoBehaviour
{
    public CinemachineCamera vcam;
    public float zoomSpeed = 0.02f;
    public float mouseZoomSpeed = 2f;
    public float minDist = 250f, maxDist = 1500f;

    float lastPinch = -1f;
    CinemachineOrbitalFollow orbitalFollow;

    void Start(){
        // Vérifier que le composant existe
        if (vcam == null){
            Debug.LogError("CinemachineCamera n'est pas assignée !");
            return;
        }
        
        orbitalFollow = vcam.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null){
            Debug.LogError("CinemachineOrbitalFollow n'est pas trouvé sur la caméra !");
        }
    }

    void OnEnable(){
        EnhancedTouchSupport.Enable();
    }
    
    void OnDisable(){
        EnhancedTouchSupport.Disable();
    }

    void Update(){
        if (orbitalFollow == null) return;

        // Support de la molette de souris
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f){
            float delta = scroll * mouseZoomSpeed;
            orbitalFollow.Radius = Mathf.Clamp(orbitalFollow.Radius - delta, minDist, maxDist);
        }

        // Support du pinch sur mobile
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        if (touches.Count >= 2){
            var d = Vector2.Distance(touches[0].screenPosition, touches[1].screenPosition);
            if (lastPinch > 0){
                float delta = (d - lastPinch) * zoomSpeed;
                orbitalFollow.Radius = Mathf.Clamp(orbitalFollow.Radius - delta, minDist, maxDist);
            }
            lastPinch = d;
        } else {
            lastPinch = -1f;
        }
    }
}
