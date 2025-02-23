using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.RootFinding;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class SplineInterpolation : MonoBehaviour
{
    [SerializeField] private SplinePathMono path1;
    [SerializeField] private SplinePathMono path2;
    [SerializeField] private int divide = 1;
    [SerializeField] private bool drawPoint = false;
    [FormerlySerializedAs("drawHermiteLine")] [SerializeField] private bool drawLerpHermiteLine = true;
    [SerializeField] private bool drawDebugLine = true;
    [SerializeField] private float offsetDistance = 1;
    [SerializeField] private bool offsetDraw;

    private List<Vector2> intersectPoints = new List<Vector2>();
    private void Awake()
    {
        intersectPoints = Intersection();
    }

    public List<List<Vector2>> SplineInterporation(ISpline spline1, ISpline spline2, float spacing = 0.01f, int divide = 1)
    {
        if (spacing <= 0 || spline1.GetLength(1) == 0 || spline2.GetLength(1) == 0)
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
        
        
        List<List<Vector2>> lerpPaths = SplineInterporation(path1.Spline, path2.Spline, 0.01f, divide);
        
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

        DrawHermite(0);
        DrawHermite(1);
        
        DrawLerpHermite();
        
        //DrawIntersectionPoints();
        DrawCurveOffset();
        //DrawCurveRoots();
        //DrawCurserProjection();
    }

    void DrawLerpHermite()
    {
        float lerpValue = 0;
        for (int i = 0; i < divide; i++)
        {
            lerpValue += (float)1 / (divide + 1.0f);
            DrawHermite(lerpValue);
        }
    }
    void DrawIntersectionPoints()
    {
        intersectPoints = Intersection();
        for (int i = 0; i < intersectPoints.Count; i++)
        {
            Gizmos.color = Color.cyan;
            MyGizmos.DrawWireCircle(intersectPoints[i], 0.5f, 30);
        }
    }
    void DrawHermite(float t)
    {
        HermitePath hermite = Hermite.HermiteLerp((HermitePath)path1.Spline, (HermitePath)path2.Spline, t);
        hermite.DrawOnGizmo(drawLerpHermiteLine, drawPoint);
    }

    void DrawCurveOffset()
    {
        HermitePath hermitePath = (HermitePath)path1.Spline;
        hermitePath.Offset(offsetDistance).DrawOnGizmo(true, offsetDraw);
        hermitePath.Offset(-offsetDistance).DrawOnGizmo(true, offsetDraw);
    }

    void DrawCurserProjection()
    {
        HermitePath hermitePath = (HermitePath)path1.Spline;
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;
        // ¾À ºä¿¡¼­ GUI ÁÂÇ¥°è¸¦ ¿ùµå ÁÂÇ¥·Î º¯È¯
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        Vector2 p = SplineUtility.PointProjection(hermitePath, ray.origin).p;
        Gizmos.color = Color.red;
        MyGizmos.DrawWireCircle(p, 2);
    }
    
    void DrawCurveRoots()
    {
        HermitePath hermitePath = (HermitePath)path1.Spline;
        float[] roots = hermitePath.GetRoots();

        Gizmos.color = Color.cyan;
        for (int i = 0; i < roots.Length; i++)
        {
            MyGizmos.DrawWireCircle(hermitePath.GetPoint(roots[i]), 0.5f, 30);
        }
    }
    List<Vector2> Intersection()
    {
        List<Vector2> IntersectionPoints = new List<Vector2>();
        SplinePathBase splinePath1 = path1.Spline;
        SplinePathBase splinePath2 = path2.Spline;
        for (int i = 0; i < splinePath1.SegmentCount; i++)
        {
            for (int k = 0; k < splinePath2.SegmentCount; k++)
            {
                CubicBezier segment1 = splinePath1.GetSegment(i).GetCubicBezier();
                CubicBezier segment2 = splinePath2.GetSegment(k).GetCubicBezier();
                IntersectionPoints.AddRange(SplineUtility.IntersectionCurve(segment1, segment2));
            }
        }
        return IntersectionPoints;
    }
    
}
