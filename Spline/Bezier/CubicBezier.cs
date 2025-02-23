using System.Collections.Generic;
using System.Linq;
using CustomCollision;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CubicBezier : Bezier
{
    [SerializeField, HideInInspector] protected List<Vector2> controlPoints = new List<Vector2>();
    public CubicBezier(Vector2 centre)
    {
        controlPoints = new List<Vector2> {
            centre + Vector2.left,
            centre + (Vector2.left + Vector2.up) * 0.5f,
            centre + (Vector2.right + Vector2.down) * 0.5f,
            centre + Vector2.right
        };
    }

    public CubicBezier(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        controlPoints = new List<Vector2> {
            p1,p2,p3,p4
        };
    }

    public override Vector2 StartPoint { get => P0; set => MovePoint(0, value); }
    public override Vector2 EndPoint { get => P3; set => MovePoint(3, value); }

    public Vector2 P0
    {
        get => controlPoints[0];
        set => MovePoint(0, value);
    }

    
    public Vector2 P1
    {
        get => controlPoints[1];
        set => MovePoint(1, value);
    }
    public Vector2 P2 
    {
        get => controlPoints[2];
        set => MovePoint(2, value);
    }
    public Vector2 P3
    {
        get => controlPoints[3];
        set => MovePoint(3, value);
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
            return new Vector2[2] { controlPoints[1], controlPoints[2] };
        }
    }
    
    public override Vector2[] Segment
    {
        get => controlPoints.ToArray();
    }

    public bool IsLinear
    {
        get
        {
            float angle1 = MyMath.UnsignedAngle(P1 - P0, P2 - P0);
            float angle2 = MyMath.UnsignedAngle(P3 - P0, P2 - P0);
            return MyMath.FloatZero(angle1) && MyMath.FloatZero(angle2);
        }
    }

    public bool IsSimple
    {
        get
        {
            if (IsLinear) return true;

            // 3. 곡선의 바운딩 박스 크기에 비해 너무 구부러져 있으면 단순하지 않음
            float len = GetLength(1);
            AABB2D box = GetBoundingBox();
            float w = box.Width;
            float h = box.Height;
            return len < 2 * (w + h);
        }
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
        float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
        float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
        int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
        float t = 0;
        while (t <= 1) {
            t += 1f / divisions;
            Vector2 pointOnCurve = EvaluateCubic(p[0], p[1], p[2], p[3], t);
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

    public override Vector2 GetPoint(float t)
    {
        return GetPointOnBezierCurve(P0, P1, P2, P3, t);
    }

    public override float GetLength(float t)
    {
        return ApproximateLength(P0, P1, P2, P3);
    }

    public override Vector2 GetDerivative(float t)
    {
        return Derivative(P0, P1, P2, P3, t);
    }

    public override float[] GetRoots()
    {
        return GetRoots(P0, P1, P2, P3);
    }

    protected override Spline[] OnSplit(float t)
    {
        Vector3[] points = SplitCubicBezier(P0, P1, P2, P3, t);
        return new Spline[2]
        {
            new CubicBezier(points[0], points[1], points[2], points[3]),
            new CubicBezier(points[4], points[5], points[6], points[7])
        };
    }

    public Vector2 Derivative(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        Vector2 d0 = p1 - p0;
        Vector2 d1 = p2 - p1;
        Vector2 d2 = p3 - p2;

        float MT = 1 - t;
        float A = MT * MT;
        float B = 2 * MT * t;
        float C = t * t;

        return 3 * (A * d0 + B * d1 + C * d2);
    }

    /// <summary>
    /// 대략적으로 호의 길이를 계산하여 반환합니다.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="t0"></param>
    /// <param name="p1"></param>
    /// <param name="t1"></param>
    /// <returns></returns>
    public static float ApproximateLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float segmentCount = 20)
    {
        Vector2 previousPoint = p0;
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++) {
            float division = 1 * segmentIndex / segmentCount;
            Vector2 pointOnCurve = GetPointOnBezierCurve(p0, p1 , p2, p3, division);
            dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);
            previousPoint = pointOnCurve;
        }
        return dstSinceLastEvenPoint;
    }

    
    protected override void OnMovePoint(int i, Vector2 pos) {
        Vector2 deltaPos = pos - controlPoints[i];
        controlPoints[i] = pos;
        // if(i % 3 == 0) {
        //     if(i + 1 < controlPoints.Count) {
        //         controlPoints[i + 1] += deltaPos;
        //     }
        //     if(i - 1 >= 0) {
        //         controlPoints[i - 1] += deltaPos;
        //     }
        // }
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
        Gizmos.DrawLine(segment[3], segment[2]);
        for (int i = 0; i < segment.Length; i++)
        {
            MyGizmos.DrawWireCircle(segment[i], 0.5f, 30);
        }
    }

    public override CubicBezier GetCubicBezier()
    {
        return new CubicBezier(P0,P1,P2,P3);
    }

    public override void ConvertCubicBezierToSpline(CubicBezier bezier)
    {
        P0 = bezier.P0;
        P1 = bezier.P1;
        P2 = bezier.P2;
        P3 = bezier.P3;
    }

    public override Spline GetCopySpline()
    {
        return new CubicBezier(P0, P1, P2, P3);
    }

    public static Vector2 GetPointOnBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
        // Vector3 a = Vector2.Lerp(p0, p1, t);
        // Vector3 b = Vector2.Lerp(p1, p2, t);
        // Vector3 c = Vector2.Lerp(p2, p3, t);
        //
        // Vector3 d = Vector2.Lerp(a, b, t);
        // Vector3 e = Vector2.Lerp(b, c, t);
        //
        // Vector2 pointOnCurve = Vector2.Lerp(d, e, t);
        //
        // return pointOnCurve;

        float t2 = t * t;
        float t3 = t2 * t;
        float mt = 1 - t;
        float mt2 = mt * mt;
        float mt3 = mt2 * mt;
        return p0 * mt3 + p1 * 3 * mt2 * t + p2 * 3 * mt * t2 + p3 * t3;
    }
    
    public static float[] GetRoots(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        List<float> calRoots = new List<float>(); 
        calRoots.AddRange(GetCubicRoots(p0.x, p1.x, p2.x, p3.x));
        calRoots.AddRange(GetCubicRoots(p0.y, p1.y, p2.y, p3.y));
        
        List<float> clampRoots = new List<float>();
        for (int i = 0; i < calRoots.Count; i++)
        {
            if(0 < calRoots[i] && calRoots[i] < 1)
                clampRoots.Add(calRoots[i]);
        }
        return clampRoots.ToArray();
    }
    
    // Cubic Bezier
    protected static float[] GetCubicRoots(float p0, float p1, float p2,float p3)
    {
        float v1 = 3 * (p1 - p0);
        float v2 = 3 * (p2 - p1);
        float v3 = 3 * (p3 - p2);
        
        if (v3 == float.NaN) return  new float[1] {-v1 / (v2 - v1)};

        // quadratic root finding is not super complex.
        float a = 3 * (-p0 + 3 * p1 - 3 * p2 + p3);

        // no root:
        if (a == float.NaN) return  new float[0];

        float b = 6 * (p0 - 2 * p1 + p2),
            c = 3 * (p1 - p0),
            d = b*b - 4*a*c;

        // no root:
        if (d < 0) return new float[0];

        // one root:
        float f = -b / (2*a);
        if (d == 0) return new float[1] { f };

        // two roots:
        float l = Mathf.Sqrt(d) / (2*a);
        return new float[2] {f-l, f+l};
    }
    public static QuadraticBezier CreateQuadraticFromThreePoint(Vector2 start, Vector2 mid, Vector2 end)
    {
        float d1 = Vector2.Distance(start, mid);
        float d2 = Vector2.Distance(mid, end);
        float t = d1 / (d1 + d2);
        
        float a = t * t + Mathf.Pow(1f - t, 2) - 1;
        float b = t * t + Mathf.Pow(1 - t, 2);
        float ratioT= Mathf.Abs(a / b);

        float oneMinusTPow2 = Mathf.Pow(1 - t, 2);
        float ut = oneMinusTPow2 / (t * t + oneMinusTPow2);

        Vector2 B = mid;
        Vector2 C = ut * start + (1-ut) * end;
        Vector2 A = B + ((B - C) / ratioT);
        return new QuadraticBezier(start, A, end);
    }
    
    public ABC GetQuadraticABC(Vector2 start, Vector2 mid, Vector2 end)
    {
        float d1 = Vector2.Distance(start, mid);
        float d2 = Vector2.Distance(mid, end);
        float t = d1 / (d1 + d2);
        
        float a = t * t + Mathf.Pow(1f - t, 2) - 1;
        float b = t * t + Mathf.Pow(1 - t, 2);
        float ratioT= Mathf.Abs(a / b);

        float oneMinusTPow2 = Mathf.Pow(1 - t, 2);
        float ut = oneMinusTPow2 / (t * t + oneMinusTPow2);

        Vector2 B = mid;
        Vector2 C = ut * start + (1-ut) * end;
        Vector2 A = B + ((B - C) / ratioT);
        return new ABC(A, B, C);
    }
    
    public ABC GetABC(Vector2 start, Vector2 mid, Vector2 end, int degree)
    {
        float d1 = Vector2.Distance(start, mid);
        float d2 = Vector2.Distance(mid, end);
        float t = d1 / (d1 + d2);

        float tD = Mathf.Pow(t, degree);
        
        float a = tD + Mathf.Pow(1f - t, degree) - 1;
        float b = tD + Mathf.Pow(1 - t, degree);
        float ratioT= Mathf.Abs(a / b);

        float oneMinusTPowD = Mathf.Pow(1 - t, degree);
        float ut = oneMinusTPowD / (t * t * t + oneMinusTPowD);

        Vector2 B = mid;
        Vector2 C = ut * start + (1-ut) * end;
        Vector2 A = B + ((B - C) / ratioT);
        return new ABC(A, B, C);
    }
    
    public struct ABC
    {
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;

        public ABC(Vector2 a, Vector2 b, Vector2 c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }
    }
}
