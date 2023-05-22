using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingSquaresChunk : MonoBehaviour
{
    public Dictionary<int2, float> noiseMap;
    private List<Vector3> verticies;
    private List<int> triangles;
    private Mesh mesh;
    private List<Vector2> uvs;
    private int subdivision;


    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verticies);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.SetUVs(0, uvs);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void Generate(MarchingSquaresPreset preset, Vector2 offset = new Vector2())
    {
        float startTime = Time.realtimeSinceStartup;
        subdivision = preset.subdivision;

        noiseMap = new Dictionary<int2, float>();
        verticies = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        GenerateNoise(preset.gridSize * preset.subdivision, preset.noiseSize, offset);
        GenerateMeshData(preset.gridSize * preset.subdivision, preset.isoValue);
        GenerateMesh();
        //Debug.Log("Successfully generated | " + (Time.realtimeSinceStartup - startTime));
    }

    public void GenerateNoise(int2 gridSize, float noiseSize, Vector2 offset)
    {
        for (int x = 0; x < gridSize.x + 1; x++)
        {
            for (int y = 0; y < gridSize.y + 1; y++)
            {
                float noise = Mathf.PerlinNoise(
                    (x * noiseSize) / subdivision + offset.x * noiseSize,
                    (y * noiseSize) / subdivision + offset.y * noiseSize);
                noise = Mathf.Clamp01(noise);
                if (noise > 1f)
                    Debug.Log("M");
                else if (noise < 0f)
                    Debug.Log("L");

                noiseMap.Add(new int2(x, y), noise);
            }
        }
    }

    public void GenerateMeshData(int2 gridSize, float isoValue)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                int config = 0;
                if (noiseMap[new int2(x, y)] < isoValue)//bottom left
                    config += 1;
                if (noiseMap[new int2(x + 1, y)] < isoValue) //bottom right
                    config += 2;
                if (noiseMap[new int2(x + 1, y + 1)] < isoValue) //top right
                    config += 4;
                if (noiseMap[new int2(x, y + 1)] < isoValue) //top left
                    config += 8;
                if (isoValue >= 1)
                {
                    config = 15;
                }
                else if (isoValue <= 0)
                {
                    config = 0;
                }
                GenrateMeshVariation(new int2(x, y), config);

            }
        }
    }

    public void GenrateMeshVariation(int2 position, int config)
    {
        switch (config)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(position, NodePositions.centreBottom, NodePositions.bottomLeft, NodePositions.centreLeft);
                break;
            case 2:
                MeshFromPoints(position, NodePositions.centreRight, NodePositions.bottomRight, NodePositions.centreBottom);
                break;
            case 4:
                MeshFromPoints(position, NodePositions.centreTop, NodePositions.topRight, NodePositions.centreRight);
                break;
            case 8:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.centreTop, NodePositions.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(position, NodePositions.centreRight, NodePositions.bottomRight, NodePositions.bottomLeft, NodePositions.centreLeft);
                break;
            case 6:
                MeshFromPoints(position, NodePositions.centreTop, NodePositions.topRight, NodePositions.bottomRight, NodePositions.centreBottom);
                break;
            case 9:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.centreTop, NodePositions.centreBottom, NodePositions.bottomLeft);
                break;
            case 12:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.topRight, NodePositions.centreRight, NodePositions.centreLeft);
                break;
            case 5:
                MeshFromPoints(position, NodePositions.centreTop, NodePositions.topRight, NodePositions.centreRight, NodePositions.centreBottom, NodePositions.bottomLeft, NodePositions.centreLeft);
                break;
            case 10:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.centreTop, NodePositions.centreRight, NodePositions.bottomRight, NodePositions.centreBottom, NodePositions.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(position, NodePositions.centreTop, NodePositions.topRight, NodePositions.bottomRight, NodePositions.bottomLeft, NodePositions.centreLeft);
                break;
            case 11:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.centreTop, NodePositions.centreRight, NodePositions.bottomRight, NodePositions.bottomLeft);
                break;
            case 13:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.topRight, NodePositions.centreRight, NodePositions.centreBottom, NodePositions.bottomLeft);
                break;
            case 14:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.topRight, NodePositions.bottomRight, NodePositions.centreBottom, NodePositions.centreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(position, NodePositions.topLeft, NodePositions.topRight, NodePositions.bottomRight, NodePositions.bottomLeft);
                break;

        }
    }

    public void MeshFromPoints(int2 position, params float2[] points)
    {

        if (points.Length > 2)
            CreateTriangle(position, points[0], points[1], points[2]);
        if (points.Length > 3)
            CreateTriangle(position, points[0], points[2], points[3]);
        if (points.Length > 4)
            CreateTriangle(position, points[0], points[3], points[4]);
        if (points.Length > 5)
            CreateTriangle(position, points[0], points[4], points[5]);
    }


    void CreateTriangle(int2 position, float2 a, float2 b, float2 c)
    {
        var posA = new Vector3((a.x + position.x) / ((float)subdivision), 0, (a.y + position.y) / ((float)subdivision));
        var posB = new Vector3((b.x + position.x) / ((float)subdivision), 0, (b.y + position.y) / ((float)subdivision));
        var posC = new Vector3((c.x + position.x) / ((float)subdivision), 0, (c.y + position.y) / ((float)subdivision));

        AddVerticies(posA, posB, posC);
        AddUVs(new Vector2(posA.x, posA.z), new Vector2(posB.x, posB.z), new Vector2(posC.x, posC.z));
        AddTriangles(3);
    }

    public void AddTriangles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            triangles.Add(triangles.Count);
        }
    }

    private void AddVerticies(Vector3 a, Vector3 b, Vector3 c)
    {
        verticies.Add(a);
        verticies.Add(b);
        verticies.Add(c);
    }

    private void AddUVs(Vector2 a, Vector2 b, Vector2 c)
    {
        uvs.Add(new Vector2(a.x, a.y));
        uvs.Add(new Vector2(b.x, b.y));
        uvs.Add(new Vector2(c.x, c.y));
    }
}
