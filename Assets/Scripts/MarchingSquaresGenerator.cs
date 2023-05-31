using System.Collections.Generic;
using System.Linq;
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

    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;
        GenerateChanks();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (!Application.isPlaying)
            return;
        var a = FindObjectOfType<MarchingSquaresGenerator>();
        var b = FindObjectsOfType<MarchingSquaresChunk>();
        foreach (var chunk in b)
        {
            var pos = new int2((int)(chunk.transform.position.x / a.preset.gridSize.x / a.size.x), (int)(chunk.transform.position.z / a.preset.gridSize.y / a.size.y));
            if (!a.chunks.ContainsValue(chunk) && !a.chunks.ContainsKey(pos))
                a.chunks.Add(pos, chunk);
        }
        a.preset?.onChanged?.AddListener(() => a.GenerateChanks());
    }

    public void GenerateChanks()
    {
        if (!Application.isPlaying)
            return;

        foreach (var item in chunks.Values)
        {
            if(item)
                Destroy(item.gameObject);
        }
        chunks = new Dictionary<int2, MarchingSquaresChunk>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var chunk = Instantiate(chunkPrefab);
                chunk.transform.position = new Vector3(x * preset.gridSize.x, 0, y * preset.gridSize.y);
                chunk.SetMaxSize(size.x, size.y);
                chunk.SetCoordinates(x, y);
                chunk.Generate(preset, new Vector2(x * preset.gridSize.x, y * preset.gridSize.y));
                chunks.Add(new int2(x, y), chunk);
            }
        }

    }

}
