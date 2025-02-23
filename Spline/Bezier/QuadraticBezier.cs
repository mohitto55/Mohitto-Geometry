using System.Collections.Generic;
using UnityEngine;

public class QuadraticBezier : Bezier
{
    [SerializeField, HideInInspector] protected List<Vector2> controlPoints = new List<Vector2>();
    public QuadraticBezier(Vector2 centre)
    {
        controlPoints = new List<Vector2> {
            centre + Vector2.left,
            centre + (Vector2.left + Vector2.up) * 0.5f,
            centre + Vector2.right
        };
    }

    public QuadraticBezier(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        controlPoints = new List<Vector2> {
            p1,p2,p3
        };
    }
    
    public QuadraticBezier(CubicBezier cubic)
    {
        CubicToQuadratic(cubic.P0, cubic.P1, cubic.P2, cubic.P3, out var q0, out var q1, out var q2);
        controlPoints = new List<Vector2>(){q0,q1,q2};
    }

    public override Vector2 StartPoint { get => P0; set => controlPoints[0] = value; }
    public override Vector2 EndPoint { get => P2; set => controlPoints[2] = value; }

    public Vector2 P0 
    {
        get => controlPoints[0];
        set => controlPoints[0] = value;
    }
    public Vector2 P1 
    {
        get => controlPoints[1];
        set => controlPoints[1] = value;
    }
    public Vector2 P2 
    {
        get => controlPoints[2];
        set => controlPoints[2] = value;
    }

    public override Vector2[] Points
    {
        get
        {
            return new Vector2[2] { controlPoints[0], controlPoints[controlPoints.Count-1] };
        }
    }

    public override Vector2[] Handles
    {
        get
        {
            return new Vector2[1] { controlPoints[1] };
        }
    }
    
    public override Vector2[] Segment
    {
        get => controlPoints.ToArray();
    }

    protected override Vector2[] OnCalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        if (controlPoints == null || controlPoints.Count <= 0)
            return new Vector2[0];
        
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(controlPoints[0]);
        Vector2 previousPoint = controlPoints[0];
        float dstSinceLastEvenPoint = 0;
        
        Vector2[] p = Segment;
        float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]);
        float estimatedCurveLength = Vector2.Distance(p[0], p[2]) + controlNetLength / 2f;
        int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
        float t = 0;
        while (t <= 1) {
            t += 1f / divisions;
            Vector2 pointOnCurve = EvaluateQuadratic(p[0], p[1], p[2], t);
            dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

            while (dstSinceLastEvenPoint >= spacing) {
                float overshootDst = dstSinceLastEvenPoint - spacing;
                Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                evenlySpacedPoints.Add(newEvenlySpacedPoint);
                dstSinceLastEvenPoint = overshootDst;
                previousPoint = newEvenlySpacedPoint;
            }

            previousPoint = pointOnCurve;
        }
        
        return evenlySpacedPoints.ToArray();
    }

    // TODO : 구현 필요
    public override Vector2 GetPoint(float t)
    {
        return EvaluateQuadratic(P0, P1, P2, t);
    }

    // TODO : 구현 필요
    public override float GetLength(float t)
    {
        return ApproximateLength(P0, P1, P2, t);
    }

    // TODO : 구현 필요
    public override Vector2 GetDerivative(float t)
    {
        return Vector2.zero;
    }
    // TODO : 구현 필요
    public override float[] GetRoots()
    {
        return new float[0];
    }
    // TODO : 구현 필요
    protected override Spline[] OnSplit(float t)
    {
        return new Spline[0];
    }
    // TODO : 구현 필요
    public Vector2 Derivative(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        return Vector2.zero;
    }

    /// <summary>
    /// 대략적으로 호의 길이를 계산하여 반환합니다.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="t0"></param>
    /// <param name="p1"></param>
    /// <param name="t1"></param>
    /// <returns></returns>
    public static float ApproximateLength(Vector3 p0, Vector3 p1, Vector3 p2, float segmentCount = 20)
    {
        Vector2 previousPoint = p0;
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++) {
            float division = 1 * segmentIndex / segmentCount;
            Vector2 pointOnCurve = EvaluateQuadratic(p0, p1 , p2, division);
            dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);
            previousPoint = pointOnCurve;
        }
        return dstSinceLastEvenPoint;
    }

    
    protected override void OnMovePoint(int i, Vector2 pos) {
        Vector2 deltaPos = pos - controlPoints[i];
        controlPoints[i] = pos;
        if(i % 3 == 0) {
            if(i + 1 < controlPoints.Count) {
                controlPoints[i + 1] += deltaPos;
            }
            if(i - 1 >= 0) {
                controlPoints[i - 1] += deltaPos;
            }
        }
    }

    public override void DrawOnGizmo(bool drawLerpHermiteLine = true, bool drawPoint = true)
    {
        Vector2[] path = CalculateEvenlySpacedPoints(0.03f);
        if (drawLerpHermiteLine)
        {
            for (int pathIndex = 0; pathIndex < path.Length - 1; pathIndex++)
            {
                int curIndex = pathIndex;
                int nextIndex = pathIndex + 1;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(path[curIndex], path[nextIndex]);
            }
        }

        if (!drawPoint)
            return;
        Gizmos.color = Color.red;
        Vector2[] segment = Segment;
        Gizmos.DrawLine(segment[0], segment[1]);
        Gizmos.DrawLine(segment[1], segment[2]);
        for (int i = 0; i < segment.Length; i++)
        {
            MyGizmos.DrawWireCircle(segment[i], 0.5f, 30);
        }
    }

    public override CubicBezier GetCubicBezier()
    {
        return new CubicBezier(P0,P1,P2,P2);
    }

    public override void ConvertCubicBezierToSpline(CubicBezier bezier)
    {
        CubicToQuadratic(bezier.P0, bezier.P1, bezier.P2, bezier.P3, out Vector2 q0, out Vector2 q1, out Vector2 q2);
        P0 = q0;
        P1 = q1;
        P2 = q2;
    }

    public override Spline GetCopySpline()
    {
        return new QuadraticBezier(P0, P1, P2);
    }

    public static Vector2 GetPointOnBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, float t) {
        return EvaluateQuadratic(p0,p1,p2,t);
    }
    
}
