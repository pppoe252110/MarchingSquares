using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Preset", menuName = "ScriptableObjects/MarchingSquaresPreset", order = 1)]
public class MarchingSquaresPreset : ScriptableObject
{
    [Header("Noise Properties")]
    public int seed;
    public int2 gridSize = new int2(8, 8);
    public float noiseGain = 1f;
    public float noiseOffset = 0f;
    public float noiseSize = 0.3f;
    [Range(0f, 1f)]
    public float isoValue = 0.3f;
    [Header("Mesh Properties")]
    [Range(1f, 8f)]
    public int subdivision = 1;
    [Header("Border Properties")]
    public bool generateBorder = true;
    public float borderHeight = 1f;
    [Header("Falloff Map")]
    public bool generateFalloff = true;
    public float falloffMultiplier = 1;
    public float falloffRadius;
    public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [HideInInspector]
    public UnityEvent onChanged;

    private void OnValidate()
    {
        onChanged?.Invoke();
    }
}
