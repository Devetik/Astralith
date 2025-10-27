using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{

    ShapeGenerator shapeGenerator;
    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    Planet.NormalCalculationMode normalMode;
    int subdivisionLevel;
    int faceIndex;

    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp, Planet.NormalCalculationMode normalMode, int subdivisionLevel = 0, int faceIndex = 0)
    {
        this.shapeGenerator = shapeGenerator;
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.normalMode = normalMode;
        this.subdivisionLevel = subdivisionLevel;
        this.faceIndex = faceIndex;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        Vector2[] uv = (mesh.uv.Length == vertices.Length) ? mesh.uv : new Vector2[vertices.Length];

        // Calculer les offsets pour la subdivision hiérarchique
        Vector2 subdivisionOffset = Vector2.zero;
        Vector2 subdivisionScale = Vector2.one;
        
        if (subdivisionLevel > 0)
        {
            int facesPerDirection = (int)Mathf.Pow(4, subdivisionLevel);
            int subIndex = faceIndex % facesPerDirection;
            
            // Calculer la position dans la grille de subdivision
            int gridSize = (int)Mathf.Sqrt(facesPerDirection);
            int subX = subIndex % gridSize;
            int subY = subIndex / gridSize;
            
            // Calculer l'offset et l'échelle corrects
            // Chaque subdivision couvre 1/gridSize de la face
            subdivisionOffset = new Vector2(
                (float)subX / gridSize,
                (float)subY / gridSize
            );
            subdivisionScale = Vector2.one / gridSize;
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                
                // Appliquer la subdivision hiérarchique
                percent = subdivisionOffset + percent * subdivisionScale;
                
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
                uv[i].y = unscaledElevation;
                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        // Calculer les normales selon le mode choisi
        if (normalMode == Planet.NormalCalculationMode.Smooth)
        {
            mesh.RecalculateNormals();
        }
        else // LowPoly
        {
            CalculateLowPolyNormals();
        }
        
        mesh.uv = uv;
    }

    public void UpdateUVs(ColourGenerator colourGenerator)
    {
        Vector2[] uv = mesh.uv;

        // Calculer les offsets pour la subdivision hiérarchique
        Vector2 subdivisionOffset = Vector2.zero;
        Vector2 subdivisionScale = Vector2.one;
        
        if (subdivisionLevel > 0)
        {
            int facesPerDirection = (int)Mathf.Pow(4, subdivisionLevel);
            int subIndex = faceIndex % facesPerDirection;
            
            int gridSize = (int)Mathf.Sqrt(facesPerDirection);
            int subX = subIndex % gridSize;
            int subY = subIndex / gridSize;
            
            subdivisionOffset = new Vector2(
                (float)subX / gridSize,
                (float)subY / gridSize
            );
            subdivisionScale = Vector2.one / gridSize;
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                
                // Appliquer la subdivision hiérarchique
                percent = subdivisionOffset + percent * subdivisionScale;
                
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                uv[i].x = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
            }
        }
        mesh.uv = uv;
    }

    private void CalculateLowPolyNormals()
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3[] normals = new Vector3[vertices.Length];
        
        // Calculer les normales pour chaque triangle
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];
            
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
            
            // Calculer la normale du triangle
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
            
            // Assigner la même normale aux 3 sommets du triangle
            normals[i1] = normal;
            normals[i2] = normal;
            normals[i3] = normal;
        }
        
        mesh.normals = normals;
    }

}
