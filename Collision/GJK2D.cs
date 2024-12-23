using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GJK2D
{
    public static Vector2 SupportPoint(Vector2 dir, Vector2 center,  IEnumerable<Vector2> points)
    {
        float maxValue = 0;
        Vector2 answerPoint = points.First();
        foreach (var point in points)
        {
            float value = Vector2.Dot(point - center, dir);
            if (maxValue < value)
            {
                answerPoint = point;
                maxValue = value;
            }
        }
        return answerPoint;
    }
}
