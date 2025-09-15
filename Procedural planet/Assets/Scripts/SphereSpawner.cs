using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    [Header("Taille (diamètre)")]
    public float minDiameter = 0.5f;
    public float maxDiameter = 3f;

    [Header("Spawn fixe")]
    public Transform spawnPoint;   // Si null, utilisera la position de ce GameObject
    public Transform parent;       // Optionnel : pour ranger les sphères dans la hiérarchie
    public bool addRigidbody = false;

    private GameObject currentSphere;
    private MaterialPropertyBlock mpb;

    // À brancher sur le bouton
    public void GenerateSphere()
    {
        // 1) Supprimer l'ancienne sphère
        if (currentSphere != null)
        {
            Destroy(currentSphere);
            currentSphere = null;
        }

        // 2) Créer la nouvelle sphère
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        currentSphere = go;

        if (parent != null) go.transform.SetParent(parent, true);
        go.transform.position = spawnPoint ? spawnPoint.position : transform.position;

        // 3) Diamètre aléatoire
        float d = Random.Range(minDiameter, maxDiameter);
        go.transform.localScale = Vector3.one * d;

        // 4) Couleur aléatoire (sans créer de nouveau matériau)
        var r = go.GetComponent<Renderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        Color c = Random.ColorHSV();
        if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseColor"))
            mpb.SetColor("_BaseColor", c); // URP/HDRP
        else
            mpb.SetColor("_Color", c);     // Built-in/Standard

        r.SetPropertyBlock(mpb);

        // 5) Option physique
        if (addRigidbody) go.AddComponent<Rigidbody>();
    }

    // Optionnel : bouton "Clear" si besoin
    public void Clear()
    {
        if (currentSphere != null)
        {
            Destroy(currentSphere);
            currentSphere = null;
        }
    }

    // Petit repère visuel dans l’éditeur
    private void OnDrawGizmosSelected()
    {
        Vector3 p = spawnPoint ? spawnPoint.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, 0.25f);
    }
}
