using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    [Header("Placement")]
    public Transform spawnPoint;
    public Transform parent;

    [Header("Géométrie")]
    [Min(2)] public int resolution = 64;
    public float radius = 5f;
    public bool addMeshCollider = true;

    [Header("Bruit – Graine & FBM")]
    public int seed = 12345;
    [Min(1)] public int octaves = 5;
    public float lacunarity = 2f;
    [Range(0f, 1f)] public float persistence = 0.5f;

    [Header("Continents (basse fréquence)")]
    public float continentFreq = 0.25f;     // plus petit = masses continentales larges
    [Range(0f,1f)] public float continentAmp = 0.15f; // amplitude relative au rayon
    public AnimationCurve continentCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Montagnes (haute fréquence, sur la terre)")]
    public float mountainFreq = 2.0f;
    [Range(0f,1f)] public float mountainAmp = 0.10f;
    public float mountainMaskFreq = 0.6f;   // où placer les chaînes
    [Range(0.5f,3f)] public float mountainMaskPower = 1.4f;

    [Header("Domain warping (optionnel)")]
    public bool useWarp = true;
    public float warpFreq = 0.5f;
    [Range(0f, 1f)] public float warpStrength = 0.2f;

    [Header("Matériaux")]
    public Material landMaterial;
    public Material waterMaterial;
    [Range(0f,1f)] public float oceanLevel = 0.2f; // 20% au-dessus du rayon

    private GameObject currentPlanetGO;
    private PlanetSaveManager saveManager;
    private PlanetCameraController cameraController;

    // Offsets pré-calculés (corrige les "pics")
    private Vector3[] octaveOffsets;   // pour FBM principal
    private Vector3 warpOffset;        // pour warp
    private Vector3 continentOffset;   // pour continents
    private Vector3 mountainOffset;    // pour montagnes
    private Vector3 mountainMaskOffset;// pour masque montagnes

    // --- PUBLIC API ---
    public void GeneratePlanet()
    {
        if (currentPlanetGO) Destroy(currentPlanetGO);
        InitNoise(seed);

        currentPlanetGO = new GameObject("Planet");
        if (parent) currentPlanetGO.transform.SetParent(parent, true);
        currentPlanetGO.transform.position = spawnPoint ? spawnPoint.position : transform.position;

        var land = new GameObject("LandMesh");
        land.transform.SetParent(currentPlanetGO.transform, false);

        var mf = land.AddComponent<MeshFilter>();
        var mr = land.AddComponent<MeshRenderer>();
        if (landMaterial) mr.sharedMaterial = landMaterial;

        Mesh landMesh = BuildLowPolySphere();
        mf.sharedMesh = landMesh;

        if (addMeshCollider)
        {
            var col = land.AddComponent<MeshCollider>();
            col.sharedMesh = landMesh;
        }

        if (waterMaterial)
        {
            var ocean = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ocean.name = "Ocean";
            ocean.transform.SetParent(currentPlanetGO.transform, false);
            float oceanR = radius * (1f + oceanLevel);
            ocean.transform.localScale = Vector3.one * (oceanR * 2f);
            var rend = ocean.GetComponent<Renderer>();
            rend.sharedMaterial = waterMaterial;
            DestroyImmediate(ocean.GetComponent<Collider>());
        }

        // Sauvegarde automatique après génération
        if (saveManager == null)
            saveManager = GetComponent<PlanetSaveManager>();
        
        if (saveManager != null)
            saveManager.OnPlanetGenerated();

        // Notifie le contrôleur de caméra
        if (cameraController == null)
            cameraController = FindFirstObjectByType<PlanetCameraController>();
        
        if (cameraController != null)
            cameraController.OnPlanetGenerated();
    }

    /// <summary>
    /// Sauvegarde manuelle des paramètres actuels
    /// </summary>
    public void SaveCurrentSettings()
    {
        if (saveManager == null)
            saveManager = GetComponent<PlanetSaveManager>();
        
        if (saveManager != null)
            saveManager.SaveSettings();
    }

    /// <summary>
    /// Charge les paramètres sauvegardés
    /// </summary>
    public void LoadSavedSettings()
    {
        if (saveManager == null)
            saveManager = GetComponent<PlanetSaveManager>();
        
        if (saveManager != null)
            saveManager.LoadSettings();
    }

    // --- BUILD ---
    Mesh BuildLowPolySphere()
    {
        var verts = new List<Vector3>();
        var tris  = new List<int>();

        Vector3[] ups = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        foreach (var up in ups)
            BuildFace(up, verts, tris);

        // Flat-shading : duplique sommets par triangle, normales par face
        var flatVerts = new Vector3[tris.Count];
        var flatNormals = new Vector3[tris.Count];

        for (int i = 0; i < tris.Count; i += 3)
        {
            Vector3 v0 = verts[tris[i]];
            Vector3 v1 = verts[tris[i+1]];
            Vector3 v2 = verts[tris[i+2]];
            Vector3 n  = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            flatVerts[i]   = v0; flatNormals[i]   = n;
            flatVerts[i+1] = v1; flatNormals[i+1] = n;
            flatVerts[i+2] = v2; flatNormals[i+2] = n;

            tris[i] = i; tris[i+1] = i+1; tris[i+2] = i+2;
        }

        Mesh m = new Mesh();
        m.indexFormat = (flatVerts.Length > 65000)
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        m.SetVertices(flatVerts);
        m.SetTriangles(tris, 0, true);
        m.SetNormals(flatNormals);
        m.RecalculateBounds();
        return m;
    }

    void BuildFace(Vector3 localUp, List<Vector3> verts, List<int> tris)
    {
        Vector3 axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        Vector3 axisB = Vector3.Cross(localUp, axisA);

        for (int y = 0; y < resolution; y++)
        {
            float v0 = (float)y / resolution;
            float v1 = (float)(y + 1) / resolution;

            for (int x = 0; x < resolution; x++)
            {
                float u0 = (float)x / resolution;
                float u1 = (float)(x + 1) / resolution;

                Vector3 p00 = ApplyElevation(ProjectToSphere(PointOnUnitCube(localUp, axisA, axisB, u0, v0)));
                Vector3 p10 = ApplyElevation(ProjectToSphere(PointOnUnitCube(localUp, axisA, axisB, u1, v0)));
                Vector3 p01 = ApplyElevation(ProjectToSphere(PointOnUnitCube(localUp, axisA, axisB, u0, v1)));
                Vector3 p11 = ApplyElevation(ProjectToSphere(PointOnUnitCube(localUp, axisA, axisB, u1, v1)));

                int i00 = verts.Count; verts.Add(p00);
                int i11 = verts.Count; verts.Add(p11);
                int i10 = verts.Count; verts.Add(p10);
                int i01 = verts.Count; verts.Add(p01);

                // deux triangles
                tris.Add(i00); tris.Add(i11); tris.Add(i10);
                tris.Add(i00); tris.Add(i01); tris.Add(i11);
            }
        }
    }

    Vector3 PointOnUnitCube(Vector3 up, Vector3 axisA, Vector3 axisB, float u, float v)
    {
        Vector2 uv = new Vector2(u * 2f - 1f, v * 2f - 1f);
        return up + uv.x * axisA + uv.y * axisB;
    }

    Vector3 ProjectToSphere(Vector3 p) => p.normalized;

    Vector3 ApplyElevation(Vector3 unit)
    {
        // Optionnel : domain warping
        Vector3 q = unit;
        if (useWarp)
        {
            Vector3 w = new Vector3(
                Perlin3D(q * warpFreq + warpOffset),
                Perlin3D(q * warpFreq * 1.3f + warpOffset * 1.7f),
                Perlin3D(q * warpFreq * 0.8f + warpOffset * 2.1f)
            );
            q += w * warpStrength;
            q.Normalize();
        }

        // --- Continents ---
        float cont = FBM(q * continentFreq + continentOffset);
        cont = Mathf.InverseLerp(-1f, 1f, cont);    // -> [0,1]
        cont = continentCurve.Evaluate(cont);       // façonne bords des continents
        float landHeight = (cont - 0.5f) * 2f;      // recentre autour de 0 -> [-1,1]
        landHeight *= continentAmp;

        // --- Masque montagnes (où ?) ---
        float mMask = FBM(q * mountainMaskFreq + mountainMaskOffset);
        mMask = Mathf.InverseLerp(-1f, 1f, mMask);
        mMask = Mathf.Pow(mMask, mountainMaskPower);

        // --- Montagnes ridgées ---
        float ridge = RidgedFBM(q * mountainFreq + mountainOffset);
        ridge = Mathf.Max(0f, ridge);              // éviter d'éroder sous la mer
        float mountains = ridge * mountainAmp * cont * mMask; // seulement sur la terre

        float height = radius * (1f + landHeight + mountains);
        return unit * height;
    }

    // --- Noise core (corrigé : offsets figés) ---
    void InitNoise(int s)
    {
        var rng = new System.Random(s);
        octaveOffsets = new Vector3[octaves];
        for (int i = 0; i < octaves; i++)
            octaveOffsets[i] = new Vector3(
                (float)rng.NextDouble() * 1000f,
                (float)rng.NextDouble() * 1000f,
                (float)rng.NextDouble() * 1000f
            );

        continentOffset     = new Vector3((float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f);
        mountainOffset      = new Vector3((float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f);
        mountainMaskOffset  = new Vector3((float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f);
        warpOffset          = new Vector3((float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f, (float)rng.NextDouble()*1000f);
    }

    float FBM(Vector3 p)
    {
        float amp = 1f, freq = 1f, sum = 0f, norm = 0f;
        for (int i = 0; i < octaves; i++)
        {
            sum  += Perlin3D(p * freq + octaveOffsets[i]) * amp;
            norm += amp;
            amp  *= persistence;
            freq *= lacunarity;
        }
        return sum / Mathf.Max(0.0001f, norm); // [-1,1]
    }

    float RidgedFBM(Vector3 p)
    {
        // 1 - |noise| puis accentuation par carré pour des crêtes
        float amp = 0.5f, freq = 1f, sum = 0f, norm = 0f;
        for (int i = 0; i < octaves; i++)
        {
            float n = Perlin3D(p * freq + octaveOffsets[i]);   // [-1,1]
            n = 1f - Mathf.Abs(n);                             // [0,1]
            n *= n;                                            // accentue les crêtes
            sum  += n * amp;
            norm += amp;
            amp  *= persistence;
            freq *= lacunarity;
        }
        return (sum / Mathf.Max(0.0001f, norm)) * 2f - 1f;     // -> [-1,1]
    }

    // Perlin "3D" rapide via moyenne de 3 plans (continu sur la sphère)
    float Perlin3D(Vector3 p)
    {
        float n =
            (Mathf.PerlinNoise(p.x, p.y) +
             Mathf.PerlinNoise(p.y, p.z) +
             Mathf.PerlinNoise(p.z, p.x)) / 3f;
        return n * 2f - 1f; // -> [-1,1]
    }

    // Gizmo
    private void OnDrawGizmosSelected()
    {
        Vector3 p = spawnPoint ? spawnPoint.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p, 0.25f);
    }
}
