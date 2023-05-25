using UnityEngine;

public static class Multilerp
{
    public static float MultilerpFunction(float t, params float[] points)
    {
        if (t >= 1)
        {
            return points[points.Length - 1];
        }
        if (t <= 0)
        {
            return points[0];
        }

        int v = Mathf.FloorToInt(t * (points.Length - 1f));

        float from = points[v];
        float to = points[v + 1];

        float m = t * (points.Length - 1f) - v;

        return Mathf.Lerp(from, to, m);
    }
}