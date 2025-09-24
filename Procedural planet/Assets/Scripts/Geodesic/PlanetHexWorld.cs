using System;
using System.Collections.Generic;
using UnityEngine;

/// PlanetHexWorld
/// - Génère une planète "RimWorld-like" en tuiles (dual d'un icosa subdivisé).
/// - Place les 12 pentagones dans les calottes polaires (orientation automatique).
/// - Marque les tuiles constructibles (bande de latitudes |lat| <= buildLatitudeDeg).
/// - Calcule des champs simples (altitude/temperature/humidity) via bruit.
/// - Expose GetCellFromRay pour sélectionner une tuile avec un Ray (caméra).
/// NOTE: Ce script crée un Mesh de sphère et garde les cellules (centres + voisins).
[ExecuteAlways]
public class PlanetHexWorld : MonoBehaviour
{
    [Header("Resolution (Class I geodesic)")]
    [Tooltip("n=11 ≈ 1212 cellules (≈1200 hex + 12 pentas).")]
    public int frequency = 11;                 // n
    [Tooltip("Rayon 'scaled' (rendu from orbit). Pour la vue de sélection, c'est parfait.")]
    public float radius = 10_000f;

    [Header("Build band (hex-only)")]
    [Range(0f, 85f)] public float buildLatitudeDeg = 70f; // bande jouable ≈ 94% de surface
    public bool excludePentagonsFromBuild = true;

    [Header("Orientation (polariser pentas)")]
    public bool autoOrientPentagonsToPoles = true;
    [Range(0f, 1f)] public float orientBlend = 1f; // 1 = immédiat, <1 = interpolation douce à l'Update

    [Header("Noise / Biomes")]
    public int seed = 12345;
    public float altitudeScale = 1f;
    public float temperatureBase = 0.5f; // 0..1
    public float humidityBase = 0.5f;    // 0..1

    [Header("Runtime")]
    public bool generateOnStart = true;
    public bool drawGizmos = true;
    public Color gizmoHexColor = new Color(1f, 1f, 1f, 0.25f);
    public Color gizmoPentColor = new Color(1f, 0.5f, 0.2f, 0.8f);
    public Color gizmoBuildableColor = new Color(0.2f, 1f, 0.3f, 0.8f);

    // ---------------- Data structures ----------------
    [Serializable]
    public class Cell
    {
        public int id;                   // index
        public Vector3 center;           // position sur la sphère (monde local)
        public List<int> neighbors;      // indices des cellules voisines (5 pour pentas, 6 pour hex)
        public bool isPentagon;          // 5 voisins ?
        public float latitudeDeg;        // lat en degrés (-90..+90)
        public float longitudeDeg;       // lon en degrés (-180..+180)
        public bool canBuild;            // bande jouable + pas pentagon (si excludePentagonsFromBuild)
        // Champs "biome-like"
        public float altitude;           // -1..+1
        public float temperature;        // 0..1
        public float humidity;           // 0..1
    }

    public List<Cell> cells;             // Dual vertex-graph cells
    public Mesh sphereMesh;              // Maillage de la sphère (rendu)
    public Bounds forcedBounds = new Bounds(Vector3.zero, Vector3.one * 1e12f);

    // -- internals
    private System.Random prng;
    private Vector3 targetUp = Vector3.up; // orientation visée pour pousser pentas aux pôles

    void Start()
    {
        if (generateOnStart) Generate();
    }

    // --- API principale ---
    public void Generate()
    {
        prng = new System.Random(seed);

        // 1) Icosa -> subdiv -> normalize (sphère)
        var tri = BuildIcosahedron();
        for (int i = 0; i < frequency - 1; i++) Subdivide(tri);
        NormalizeVertices(tri, radius);

        // 2) Vertex adjacency (graph)
        var neighbors = BuildVertexAdjacency(tri);

        // 3) Dual cells (un sommet = une cellule)
        cells = new List<Cell>(tri.vertices.Count);
        for (int i = 0; i < tri.vertices.Count; i++)
        {
            var v = tri.vertices[i];
            var nbs = neighbors[i];
            var latlon = ToLatLonDeg(v.normalized);
            var cell = new Cell {
                id = i,
                center = v,
                neighbors = nbs,
                isPentagon = (nbs.Count == 5),
                latitudeDeg = latlon.x,
                longitudeDeg = latlon.y
            };
            cells.Add(cell);
        }

        // 4) Orientation: pousse les pentas vers les pôles
        if (autoOrientPentagonsToPoles)
        {
            targetUp = ComputePolarizationUp(cells);
            ApplyUpOrientationImmediate(targetUp); // or blended at Update
        }

        // 5) Marquage buildable (bande de latitudes)
        MarkBuildable();

        // 6) Champs "biome-like"
        BakeSimpleFields();

        // 7) Maillage sphère (pour le rendu)
        sphereMesh = TrianglesToMesh(tri);
        // Evite culling prématuré
        sphereMesh.bounds = forcedBounds;

        // Assure un MeshFilter/MeshRenderer sur l'objet
        var mf = GetComponent<MeshFilter>();
        if (!mf) mf = gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = sphereMesh;
        var mr = GetComponent<MeshRenderer>();
        if (!mr) mr = gameObject.AddComponent<MeshRenderer>();
        // Laisse le matériau au choix utilisateur (ou un mat par défaut)
    }

    void Update()
    {
        // Interpolation douce vers targetUp si on change des paramètres à la volée
        if (autoOrientPentagonsToPoles && orientBlend < 1f)
        {
            var currentUp = transform.up;
            var newUp = Vector3.Slerp(currentUp, targetUp, Mathf.Clamp01(orientBlend) * Time.deltaTime * 5f);
            var q = Quaternion.FromToRotation(currentUp, newUp);
            transform.rotation = q * transform.rotation;
            // Met à jour latitudes
            RecomputeLatLonAfterRotation();
            MarkBuildable();
        }
    }

    // --- Sélection: renvoie l'ID de la cellule la plus proche d'un point (raycast caméra) ---
    public int GetCellFromRay(Ray ray, out Vector3 hitPointWorld)
    {
        // Intersecte une sphère centrée sur transform.position, rayon 'radius'
        var oc = ray.origin - transform.position;
        float b = Vector3.Dot(oc, ray.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float disc = b * b - c;
        if (disc < 0f)
        {
            hitPointWorld = Vector3.zero;
            return -1;
        }
        float t = -b - Mathf.Sqrt(disc);
        if (t < 0f) t = -b + Mathf.Sqrt(disc);
        hitPointWorld = ray.origin + ray.direction * t;

        // Trouve la cellule dont le centre est le plus proche angulairement
        Vector3 local = (hitPointWorld - transform.position);
        if (local.sqrMagnitude < 1e-6f) return -1;
        Vector3 dir = local.normalized;

        int best = -1;
        float bestDot = -2f;
        for (int i = 0; i < cells.Count; i++)
        {
            float d = Vector3.Dot(dir, (cells[i].center - transform.position).normalized);
            if (d > bestDot)
            {
                bestDot = d; best = i;
            }
        }
        return best;
    }

    // ---------------- Generation primitives ----------------
    class TriMesh
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> indices = new List<int>(); // triplets
    }

    TriMesh BuildIcosahedron()
    {
        // Icosa standard centré à l'origine, rayon 1, on rescalera ensuite
        TriMesh m = new TriMesh();
        float t = (1f + Mathf.Sqrt(5f)) * 0.5f;

        var verts = new List<Vector3> {
            new Vector3(-1,  t,  0), new Vector3( 1,  t,  0), new Vector3(-1, -t,  0), new Vector3( 1, -t,  0),
            new Vector3( 0, -1,  t), new Vector3( 0,  1,  t), new Vector3( 0, -1, -t), new Vector3( 0,  1, -t),
            new Vector3( t,  0, -1), new Vector3( t,  0,  1), new Vector3(-t,  0, -1), new Vector3(-t,  0,  1)
        };
        // Normalize to unit sphere
        for (int i=0;i<verts.Count;i++) verts[i] = verts[i].normalized;
        m.vertices.AddRange(verts);

        int[] tris = {
            0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
            1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
            3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
            4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
        };
        m.indices.AddRange(tris);
        return m;
    }

    void Subdivide(TriMesh m)
    {
        var oldV = m.vertices;
        var oldI = m.indices;
        var newV = new List<Vector3>(oldV);
        var newI = new List<int>(oldI.Count * 4);

        // cache midpoint indices
        Dictionary<ulong, int> midCache = new Dictionary<ulong, int>(oldI.Count);

        Func<int,int,ulong> key = (a,b) => (ulong)Mathf.Min(a,b) << 32 | (uint)Mathf.Max(a,b);
        Func<int,int,int> midpoint = (a,b) =>
        {
            var k = key(a,b);
            if (midCache.TryGetValue(k, out int idx)) return idx;
            Vector3 mid = (oldV[a] + oldV[b]) * 0.5f;
            int id = newV.Count;
            newV.Add(mid);
            midCache[k] = id;
            return id;
        };

        for (int t = 0; t < oldI.Count; t += 3)
        {
            int i0 = oldI[t], i1 = oldI[t+1], i2 = oldI[t+2];
            int a = midpoint(i0, i1);
            int b = midpoint(i1, i2);
            int c = midpoint(i2, i0);
            // 4 faces
            newI.AddRange(new int[]{ i0,a,c,  i1,b,a,  i2,c,b,  a,b,c });
        }

        m.vertices = newV;
        m.indices = newI;
    }

    void NormalizeVertices(TriMesh m, float R)
    {
        for (int i=0;i<m.vertices.Count;i++)
            m.vertices[i] = m.vertices[i].normalized * R;
    }

    List<List<int>> BuildVertexAdjacency(TriMesh m)
    {
        var adj = new List<HashSet<int>>(m.vertices.Count);
        for (int i=0;i<m.vertices.Count;i++) adj.Add(new HashSet<int>());
        for (int t=0;t<m.indices.Count;t+=3)
        {
            int a = m.indices[t], b = m.indices[t+1], c = m.indices[t+2];
            adj[a].Add(b); adj[a].Add(c);
            adj[b].Add(a); adj[b].Add(c);
            adj[c].Add(a); adj[c].Add(b);
        }
        var list = new List<List<int>>(adj.Count);
        for (int i=0;i<adj.Count;i++) list.Add(new List<int>(adj[i]));
        return list;
    }

    Mesh TrianglesToMesh(TriMesh m)
    {
        var mesh = new Mesh();
        mesh.indexFormat = (m.vertices.Count > 65000) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(m.vertices);
        mesh.SetTriangles(m.indices, 0, true);
        mesh.RecalculateNormals();
        // bounds forcés pour éviter culling
        mesh.bounds = forcedBounds;
        return mesh;
    }

    // ---------------- Orientation & lat/lon ----------------
    Vector3 ComputePolarizationUp(List<Cell> cs)
    {
        // Moyenne des directions des pentas (ou médiane approx) => direction polaire
        Vector3 sum = Vector3.zero;
        foreach (var c in cs) if (c.isPentagon) sum += (c.center - transform.position).normalized;
        if (sum.sqrMagnitude < 1e-8f) return Vector3.up;
        Vector3 dir = sum.normalized;

        // Construit une rotation qui aligne 'dir' sur +Y (up local du transform)
        // On retourne simplement le nouveau up world (qui sera appliqué au transform)
        return dir; // cible "up" = direction moyenne des pentas
    }

    void ApplyUpOrientationImmediate(Vector3 newUp)
    {
        // Oriente l'objet pour que transform.up == newUp
        var currentUp = transform.up;
        if (newUp.sqrMagnitude < 1e-8f) return;
        var q = Quaternion.FromToRotation(currentUp, newUp.normalized);
        transform.rotation = q * transform.rotation;

        RecomputeLatLonAfterRotation();
    }

    void RecomputeLatLonAfterRotation()
    {
        for (int i=0;i<cells.Count;i++)
        {
            var local = (cells[i].center - transform.position).normalized;
            var latlon = ToLatLonDeg(local);
            cells[i].latitudeDeg = latlon.x;
            cells[i].longitudeDeg = latlon.y;
        }
    }

    static Vector2 ToLatLonDeg(Vector3 nrm)
    {
        // lat: asin(y), lon: atan2(x,z) (choix convention)
        float lat = Mathf.Asin(Mathf.Clamp(nrm.y, -1f, 1f)) * Mathf.Rad2Deg;
        float lon = Mathf.Atan2(nrm.x, nrm.z) * Mathf.Rad2Deg;
        return new Vector2(lat, lon);
    }

    // ---------------- Buildable band ----------------
    void MarkBuildable()
    {
        for (int i=0;i<cells.Count;i++)
        {
            bool withinBand = Mathf.Abs(cells[i].latitudeDeg) <= buildLatitudeDeg;
            bool ok = withinBand;
            if (excludePentagonsFromBuild && cells[i].isPentagon) ok = false;
            cells[i].canBuild = ok;
        }
    }

    // ---------------- Simple procedural fields ----------------
    void BakeSimpleFields()
    {
        // NB: Pour un vrai 3D noise, brancher un lib. Ici, on combine 2D Perlin sur différentes projections.
        float altAmp = 1f;
        foreach (var c in cells)
        {
            Vector3 p = (c.center - transform.position).normalized;
            // pseudo 3D: trois projections 2D perlin sur plans orthogonaux
            float nxy = Mathf.PerlinNoise(Hash01(seed)*10f + (p.x*0.5f+0.5f)*2f, Hash01(seed+1)*10f + (p.y*0.5f+0.5f)*2f);
            float nyz = Mathf.PerlinNoise(Hash01(seed+2)*10f + (p.y*0.5f+0.5f)*2f, Hash01(seed+3)*10f + (p.z*0.5f+0.5f)*2f);
            float nzx = Mathf.PerlinNoise(Hash01(seed+4)*10f + (p.z*0.5f+0.5f)*2f, Hash01(seed+5)*10f + (p.x*0.5f+0.5f)*2f);
            float noise = (nxy + nyz + nzx) / 3f; // 0..1
            float altitude = (noise - 0.5f) * 2f * altitudeScale; // -1..1

            // Temperature = base modulée par latitude & altitude
            float lat01 = 1f - Mathf.Abs(c.latitudeDeg) / 90f;     // équateur=1, pôles=0
            float temp = Mathf.Clamp01(temperatureBase * 0.5f + lat01 * 0.5f - Mathf.Max(0f, altitude)*0.25f);

            // Humidity = base modulée par bruit et altitude
            float hum = Mathf.Clamp01(humidityBase * 0.5f + noise * 0.5f - Mathf.Max(0f, altitude)*0.1f);

            c.altitude = altitude;
            c.temperature = temp;
            c.humidity = hum;
        }
    }

    static float Hash01(int s)
    {
        unchecked
        {
            uint x = (uint)s;
            x ^= x << 13; x ^= x >> 17; x ^= x << 5;
            return (x % 1000u) / 999f;
        }
    }

    // ---------------- Gizmos (debug) ----------------
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || cells == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;

        foreach (var c in cells)
        {
            float r = radius * 0.005f;
            Gizmos.color = c.isPentagon ? gizmoPentColor : gizmoHexColor;
            Gizmos.DrawSphere(transform.InverseTransformPoint(c.center), r * 0.6f);

            if (c.canBuild)
            {
                Gizmos.color = gizmoBuildableColor;
                Gizmos.DrawWireSphere(transform.InverseTransformPoint(c.center), r);
            }
        }
    }
}
