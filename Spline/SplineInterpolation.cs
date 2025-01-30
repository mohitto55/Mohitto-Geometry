using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class SplineInterpolation : MonoBehaviour
{
    [SerializeField] private HermitePath path1;
    [SerializeField] private HermitePath path2;
    [SerializeField] private int divide = 1;
    [SerializeField] private bool drawPoint = false;
    [FormerlySerializedAs("drawHermiteLine")] [SerializeField] private bool drawLerpHermiteLine = true;
    [SerializeField] private bool drawDebugLine = true;
    [SerializeField] private float split = 0;

    public List<List<Vector2>> SplineInterporation(Spline spline1, Spline spline2, float spacing = 0.01f, int divide = 1)
    {
        if (spacing <= 0)
            return new List<List<Vector2>>();
        
        Vector2[] path1 = spline1.CalculateEvenlySpacedPoints(0.01f);
        Vector2[] path2 = spline2.CalculateEvenlySpacedPoints(0.01f);

        List<List<Vector2>> lerpPaths = new List<List<Vector2>>();
        float lerpValue = 0;
        for (int i = 0; i < divide; i++)
        {
            lerpPaths.Add(new List<Vector2>());
            lerpValue += (float)1 / (divide + 1.0f);
            Vector2[] lerpPath = PathInterporation(path1, path2, lerpValue, spacing);
            lerpPaths[i] = lerpPath.ToList();
        }
        return lerpPaths;
    }

    public Vector2[] PathInterporation(Vector2[] path1, Vector2[] path2, float t, float spacing = 0.01f)
    {
        if (spacing <= 0 || path1.Length <= 0 || path2.Length <= 0)
            return new Vector2[0];
        
        List<Vector2> lerpPath = new List<Vector2>();

        float dstLast = 0;
        while (dstLast <= 1)
        {
            int path1LerpIndex = (int)(path1.Length * dstLast);
            int path2LerpIndex = (int)(path2.Length * dstLast);

            path1LerpIndex = Mathf.Clamp(path1LerpIndex, 0, path1.Length - 1);
            path2LerpIndex = Mathf.Clamp(path2LerpIndex, 0, path2.Length - 1);

            Vector2 lerpValue = Vector2.Lerp(path1[path1LerpIndex], path2[path2LerpIndex], t);
            lerpPath.Add(lerpValue);
            dstLast += spacing;
        }

        return lerpPath.ToArray();
    }

    private void OnDrawGizmos()
    {
        if (path1 == null || path2 == null)
            return;
        
        
        List<List<Vector2>> lerpPaths = SplineInterporation(path1.Path, path2.Path, 0.01f, divide);

        if (drawDebugLine)
        {
            for (int i = 0; i < lerpPaths.Count; i++)
            {
                for (int pathIndex = 0; pathIndex < lerpPaths[i].Count - 1; pathIndex++)
                {
                    Debug.DrawLine(lerpPaths[i][pathIndex], lerpPaths[i][pathIndex + 1]);
                }
            }
        }

        float lerpValue = 0;
        for (int i = 0; i < divide; i++)
        {
            lerpValue += (float)1 / (divide + 1.0f);
            DrawHermite(lerpValue);
        }

        //DrawHermite(split);
    }

    void DrawHermite(float t)
    {
        Hermite hermite = Hermite.HermiteLerp((Hermite)path1.Path, (Hermite)path2.Path, t);

        Vector2[] seg = hermite.GetSegment(0);
        Hermite.GetBoundingBox(seg[0], seg[1] - seg[0], seg[2],seg[3] - seg[2]);
        hermite.DrawOnGizmo(drawLerpHermiteLine, drawPoint);
    }

    
    
}
