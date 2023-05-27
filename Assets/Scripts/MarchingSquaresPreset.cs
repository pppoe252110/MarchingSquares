using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Preset", menuName = "ScriptableObjects/MarchingSquaresPreset", order = 1)]
public class MarchingSquaresPreset : ScriptableObject
{
    public int2 gridSize = new int2(8, 8);
    public float noiseSize = 0.3f;
    [Range(0f, 1f)]
    public float isoValue = 0.3f;
    [Range(1f, 8f)]
    public int subdivision = 1;
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
