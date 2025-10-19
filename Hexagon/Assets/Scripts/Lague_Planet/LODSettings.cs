using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LODSettings : ScriptableObject
{
    public Transform cameraTransform;
    [Header("LOD Configuration")]
    public bool enableLOD = false;
    [Tooltip("Active le système de Level of Detail pour optimiser les performances")]
    public bool debugLOD = false;
}
