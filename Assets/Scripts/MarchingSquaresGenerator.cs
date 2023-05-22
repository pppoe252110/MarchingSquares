using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MarchingSquaresGenerator : MonoBehaviour
{
    public MarchingSquaresChunk chunkPrefab;
    public MarchingSquaresPreset preset;
    public int2 size = new int2(8, 8);
    public Dictionary<int2, MarchingSquaresChunk> chunks = new Dictionary<int2, MarchingSquaresChunk>();

    private void Start()
    {
        GenerateChanks();
        preset.onChanged.AddListener(() => GenerateChanks());
    }


    public void GenerateChanks()
    {
        foreach (var item in chunks.Values)
        {
            Destroy(item.gameObject);
        }
        chunks = new Dictionary<int2, MarchingSquaresChunk>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var chunk = Instantiate(chunkPrefab);
                chunk.transform.position = new Vector3(x * preset.gridSize.x, 0, y * preset.gridSize.y);
                chunk.Generate(preset, new Vector2(x * preset.gridSize.x, y * preset.gridSize.y));
                chunks.Add(new int2(x, y), chunk);
            }
        }

    }

}
