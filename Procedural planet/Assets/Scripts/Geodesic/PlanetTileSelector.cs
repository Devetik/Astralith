using UnityEngine;

/// PlanetTileSelector
/// - Récupère la cellule cliquée (raycast caméra) auprès de PlanetHexWorld.
/// - Montre un gizmo sur la cellule sélectionnée.
/// - Expose un event-like hook OnSelectCell(int cellId).
[RequireComponent(typeof(PlanetHexWorld))]
public class PlanetTileSelector : MonoBehaviour
{
    public Camera cam;
    public int selectedCellId = -1;
    public Color highlightColor = Color.yellow;
    public float highlightScale = 1.015f; // léger "halo"

    PlanetHexWorld world;

    void Awake()
    {
        world = GetComponent<PlanetHexWorld>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!cam || world == null || world.cells == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            int id = world.GetCellFromRay(ray, out Vector3 hit);
            if (id >= 0)
            {
                selectedCellId = id;
                OnSelectCell(id);
            }
        }
    }

    // Ici, branche ton UI (panneau d'info, bouton "Choisir ce site", etc.)
    void OnSelectCell(int cellId)
    {
        var c = world.cells[cellId];
        Debug.Log($"Selected cell {cellId} | lat={c.latitudeDeg:F1} lon={c.longitudeDeg:F1} " +
                  $"build={c.canBuild} pent={c.isPentagon} alt={c.altitude:F2} temp={c.temperature:F2} hum={c.humidity:F2}");
    }

    void OnRenderObject()
    {
        if (selectedCellId < 0 || world == null || world.cells == null) return;
        var c = world.cells[selectedCellId];

        // Simple halo: on dessine une petite sphère transparente au centre
        var mat = GetLineMaterial();
        mat.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(world.transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(highlightColor);

        // "rosace" de petits rayons
        Vector3 center = world.transform.InverseTransformPoint(c.center);
        float r = world.radius * 0.01f;
        int seg = 24;
        for (int i=0;i<seg;i++)
        {
            float a0 = (i    / (float)seg) * Mathf.PI * 2f;
            float a1 = ((i+1)/ (float)seg) * Mathf.PI * 2f;
            Vector3 p0 = center + (Quaternion.AngleAxis(a0*Mathf.Rad2Deg, center.normalized) * Vector3.Cross(center.normalized, Vector3.right)).normalized * r * 0.5f;
            Vector3 p1 = center + (Quaternion.AngleAxis(a1*Mathf.Rad2Deg, center.normalized) * Vector3.Cross(center.normalized, Vector3.right)).normalized * r * 0.5f;
            GL.Vertex(p0); GL.Vertex(p1);
        }
        GL.End();
        GL.PopMatrix();
    }

    static Material lineMat;
    static Material GetLineMaterial()
    {
        if (!lineMat)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZWrite", 0);
        }
        return lineMat;
    }
}
