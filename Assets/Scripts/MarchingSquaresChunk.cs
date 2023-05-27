using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Presets;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingSquaresChunk : MonoBehaviour
{
    public Dictionary<int2, float> noiseMap;
    private List<Vector3> squareVerticies;
    private List<int> triangles;
    private Mesh mesh;
    private List<Vector2> uvs;
    private int2 coordinates;
    private int2 maxSize;
    private MarchingSquaresPreset preset;

    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(squareVerticies);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.SetUVs(0, uvs);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetCoordinates(int x, int y)
    {
        coordinates = new int2(x, y);
    }

    public void SetMaxSize(int x, int y)
    {
        maxSize = new int2(x, y);
    }

    public void Generate(MarchingSquaresPreset preset, Vector2 offset = new Vector2())
    {
        this.preset = preset;
        noiseMap = new Dictionary<int2, float>();
        squareVerticies = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        GenerateNoise(offset);
        GenerateMeshData();
        GenerateMesh();
    }

    public void GenerateNoise(Vector2 offset)
    {
        Vector2 center = new Vector2(maxSize.x * preset.gridSize.x * preset.subdivision, maxSize.y * preset.gridSize.y * preset.subdivision) / 2f;
        float div = (new Vector2(maxSize.x * preset.gridSize.x * preset.subdivision, maxSize.y * preset.gridSize.y * preset.subdivision).magnitude / 2f * sqrt2b2 * preset.falloffRadius);
        for (int x = 0; x < preset.gridSize.x * preset.subdivision + 1; x++)
        {
            for (int y = 0; y < preset.gridSize.y * preset.subdivision + 1; y++)
            {
                float noise = 0;
                if (preset.isoValue < 1)
                {
                    noise = Mathf.PerlinNoise(
                        (x * preset.noiseSize) / preset.subdivision + offset.x * preset.noiseSize,
                        (y * preset.noiseSize) / preset.subdivision + offset.y * preset.noiseSize);
                    noise = Mathf.Clamp01(noise);
                }
                else if (preset.isoValue <= 0)
                {
                    noise = 1;
                }
                if (preset.generateFalloff)
                {
                    var falloff = preset.falloffCurve.Evaluate(Vector2.Distance(
                        new Vector2(x + coordinates.x * preset.gridSize.x * preset.subdivision, y + coordinates.y * preset.gridSize.y * preset.subdivision),
                        center) / div);
                    noise *= falloff * preset.falloffMultiplier;
                }
                noiseMap.Add(new int2(x, y), noise);
            }
        }
    }

    public void GenerateMeshData()
    {
        for (int x = 0; x < preset.gridSize.x * preset.subdivision; x++)
        {
            for (int y = 0; y < preset.gridSize.y * preset.subdivision; y++)
            {
                int config = 0;
                if (noiseMap[new int2(x, y)] > preset.isoValue)//bottom left
                    config += 1;
                if (noiseMap[new int2(x + 1, y)] > preset.isoValue) //bottom right
                    config += 2;
                if (noiseMap[new int2(x + 1, y + 1)] > preset.isoValue) //top right
                    config += 4;
                if (noiseMap[new int2(x, y + 1)] > preset.isoValue) //top left
                    config += 8;
                //if (isoValue >= 1)
                //{
                //    config = 15;
                //}
                //else if (isoValue <= 0)
                //{
                //    config = 0;
                //}
                GenrateMeshVariation(new int2(x, y), config);
                GenerateBorder(new int2(x, y), config, preset.borderHeight);

            }
        }
    }

    public void GenerateBorder(int2 position, int config, float borderHeight)
    {
        switch (config)
        {
            //corners
            case 1:
                WallFromPoints(position, borderHeight, NodePositions.centreLeft, NodePositions.centreBottom);
                break;
            case 2:
                WallFromPoints(position, borderHeight, NodePositions.centreBottom, NodePositions.centreRight);
                break;
            case 4:
                WallFromPoints(position, borderHeight, NodePositions.centreRight, NodePositions.centreTop);
                break;
            case 8:
                WallFromPoints(position, borderHeight, NodePositions.centreTop, NodePositions.centreLeft);
                break;
            //sides
            case 3:
                WallFromPoints(position, borderHeight, NodePositions.centreLeft, NodePositions.centreRight);
                break;
            case 6:
                WallFromPoints(position, borderHeight, NodePositions.centreBottom, NodePositions.centreTop);
                break;
            case 9:
                WallFromPoints(position, borderHeight, NodePositions.centreTop, NodePositions.centreBottom);
                break;
            case 12:
                WallFromPoints(position, borderHeight, NodePositions.centreRight, NodePositions.centreLeft);
                break;
            //double corners
            case 5:
                WallFromPoints(position, borderHeight, NodePositions.centreLeft, NodePositions.centreTop, NodePositions.centreRight, NodePositions.centreBottom);
                break;
            case 10:
                WallFromPoints(position, borderHeight, NodePositions.centreTop, NodePositions.centreRight, NodePositions.centreBottom, NodePositions.centreLeft);
                break;
            //cut corners
            case 7:
                WallFromPoints(position, borderHeight, NodePositions.centreLeft, NodePositions.centreTop);
                break;
            case 11:
                WallFromPoints(position, borderHeight, NodePositions.centreTop, NodePositions.centreRight);
                break;
            case 13:
                WallFromPoints(position, borderHeight, NodePositions.centreRight, NodePositions.centreBottom);
                break;
            case 14:
                WallFromPoints(position, borderHeight, NodePositions.centreBottom, NodePositions.centreLeft);
                break;

        }
    }

    public void WallFromPoints(int2 position, float borderHeight, params float2[] points)
    {
        var arr = new List<float3>();
        for (int i = 0; i < points.Length; i++)
        {
            arr.Add(new float3(points[i].x, 0, points[i].y));
            arr.Add(new float3(points[i].x, borderHeight, points[i].y));
        }
        AddWall(position, arr);
    }

    public void AddWall(int2 position, List<float3> points)
    {
        if (points.Count > 0)
        {
            CreateWallTriangle(position, points[0], points[1], points[3]);
            CreateWallTriangle(position, points[0], points[3], points[2]);
        }
        if (points.Count > 4)
        {
            CreateWallTriangle(position, points[4], points[5], points[7]);
            CreateWallTriangle(position, points[4], points[7], points[6]);
        }
    }

    public void CreateWallTriangle(int2 position, float3 a, float3 b, float3 c)
    {
        var posA = new Vector3((a.x + position.x) / ((float)preset.subdivision), a.y, (a.z + position.y) / ((float)preset.subdivision));
        var posB = new Vector3((b.x + position.x) / ((float)preset.subdivision), b.y, (b.z + position.y) / ((float)preset.subdivision));
        var posC = new Vector3((c.x + position.x) / ((float)preset.subdivision), c.y, (c.z + position.y) / ((float)preset.subdivision));

        AddVerticies(posA, posB, posC);

        var cross = GetCross(posA, posB, posC);
        float crA = Vector3.SignedAngle(Vector3.forward, cross, Vector3.up);

        float t = (crA + 180) / 360;
        var pA = ThiSFMagic(t, posA);
        var pB = ThiSFMagic(t, posB);
        var pC = ThiSFMagic(t, posC);

        AddUVs(
            new Vector2(pA, posA.y),
            new Vector2(pB, posB.y), 
            new Vector2(pC, posC.y));
        //AddUVs(new Vector2(posA.x, posA.y), new Vector2(posB.x, posB.y), new Vector2(posC.x, posC.y));
        AddTriangles(3);

    }

    public const float sqrt2b2 = 0.70710678118f;

    public float ThiSFMagic(float a, Vector3 point)
    {
        return Multilerp.MultilerpFunction(a,
            point.x,
            (point.z - point.x) * sqrt2b2,
            point.z,
            (point.z + point.x) * sqrt2b2,
            point.x,
            (point.z - point.x) * sqrt2b2,
            point.z,
            (point.z + point.x) * sqrt2b2,
            point.x);
    }

    public Vector3 GetCross(Vector3 a, Vector3 b, Vector3 c)
    {
        var dir = Vector3.Cross(b - a, c - a);
        var norm = Vector3.Normalize(dir);
        return norm;
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
        var posA = new Vector3((a.x + position.x) / ((float)preset.subdivision), 0, (a.y + position.y) / ((float)preset.subdivision));
        var posB = new Vector3((b.x + position.x) / ((float)preset.subdivision), 0, (b.y + position.y) / ((float)preset.subdivision));
        var posC = new Vector3((c.x + position.x) / ((float)preset.subdivision), 0, (c.y + position.y) / ((float)preset.subdivision));

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
        squareVerticies.Add(a);
        squareVerticies.Add(b);
        squareVerticies.Add(c);
    }

    private void AddUVs(Vector2 a, Vector2 b, Vector2 c)
    {
        uvs.Add(new Vector2(a.x, a.y));
        uvs.Add(new Vector2(b.x, b.y));
        uvs.Add(new Vector2(c.x, c.y));
    }
}
