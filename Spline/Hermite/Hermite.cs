using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class Hermite : Spline
{
    [SerializeField, HideInInspector] private List<Vector2> controlPoints = new List<Vector2>();
    public Vector2 P0     
    {
        get => controlPoints[0];
        set => MovePoint(0, value);
    }
    public Vector2 T0     
    {
        get => controlPoints[1];
        set => MovePoint(1, value);
    }
    public Vector2 P1    
    {
        get => controlPoints[2];
        set => MovePoint(2, value);
    }
    public Vector2 T1     
    {
        get => controlPoints[3];
        set => MovePoint(3, value);
    }
    
    public Hermite(Vector2 centre) : base() {
        controlPoints = new List<Vector2> {
            centre + Vector2.left,
            Vector2.right,
            centre + (Vector2.right + Vector2.down) * 0.5f,
            Vector2.right
        };
    }

    public Hermite(Hermite hermite) : base()
    {
        controlPoints = new List<Vector2>(hermite.controlPoints);
    }

    public Hermite(Vector2 p0, Vector2 t0, Vector2 p1, Vector2 t1) : base()
    {
        this.controlPoints = new List<Vector2>(){p0, t0, p1, t1};
    }
    
    public override Vector2 StartPoint { get => P0; set => MovePoint(0, value); }
    public override Vector2 EndPoint { get => P1; set => MovePoint(2, value); }

    public override Vector2[] Points
    {
        get
        {
            return new Vector2[2] { P0, P1 };
        }
    }

    public override Vector2[] Handles
    {
        get
        {
            return new Vector2[2] { T0, T1 };
        }
    }
    
    public override Vector2[] Segment
    {
        get => controlPoints.ToArray();
    }


    protected override Vector2[] OnCalculateEvenlySpacedPoints(float spacing, float resolution = 1) {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        if (controlPoints == null)
            controlPoints = new List<Vector2>();
        
        if(controlPoints.Count <= 0)
            return evenlySpacedPoints.ToArray();
        
        evenlySpacedPoints.Add(controlPoints[0]);
        Vector2 previousPoint = controlPoints[0];
        float dstSinceLastEvenPoint = 0;


        Vector2[] p = Segment;
        float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
        float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
        int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
        float t = 0;
            
        while (t <= 1)
        {
            t += 1f / divisions;
            Vector2 pointOnCurve = (Vector2)GetPointOnHermiteCurve(p[0], p[1], p[2], p[3], t);
            evenlySpacedPoints.Add(pointOnCurve);
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
        return GetPointOnHermiteCurve(P0, T0, P1, T1, t);
    }

    public override Vector2 GetDerivative(float t)
    {
        return Derivative(P0, T0, P1, T1, t);
    }

    public override float[] GetRoots()
    {
        return GetCubicBezier().GetRoots();
    }

    protected override Spline[] OnSplit(float t)
    {
        Vector3[] points = SplitHermite(P0, T0, P1, T1, t);
        return new Spline[2]
        {
            new Hermite(points[0], points[1], points[2], points[3]),
            new Hermite(points[4], points[5], points[6], points[7])
        };
    }

    public Vector2 GetPoint(int i) {
        return controlPoints[i * 2]; 
    }
    protected override void OnMovePoint(int i, Vector2 pos) {
        Vector2 deltaPos = pos - controlPoints[i];
        controlPoints[i] = pos;
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
        Gizmos.DrawLine(segment[0], segment[1] + segment[0]);
        Gizmos.DrawLine(segment[2], segment[3] + segment[2]);
        for (int i = 0; i < segment.Length; i++)
        {
            Vector2 handlePosition = i % 2 == 0 ? segment[i] : segment[i] + segment[i - 1];
            MyGizmos.DrawWireCircle(handlePosition, 0.1f, 30);
        }
    }

    public override CubicBezier GetCubicBezier()
    {
        return Bezier.HermiteToBezier(P0, T0, P1, T1);
    }

    public override void ConvertCubicBezierToSpline(CubicBezier bezier)
    {
        Hermite hermite = Bezier.BezierToHermite(bezier.P0, bezier.P1, bezier.P2, bezier.P3);
        P0 = hermite.P0;
        P1 = hermite.P1;
        T0 = hermite.T0;
        T1 = hermite.T1;
    }

    public override Spline GetCopySpline()
    {
        return new Hermite(P0, T0, P1, T1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p1">커브의 시작점</param>
    /// <param name="p2">커브의 끝점</param>
    /// <param name="t1">시작점의 tangent</param>
    /// <param name="t2">끝점의 tangent</param>
    /// <param name="t">커브의 0 ~ 1 만큼 사이의 길이</param>
    /// <returns></returns>
    public static Vector3 GetPointOnHermiteCurve(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
    {
        float tPow3 = Mathf.Pow(t, 3);
        float tPow2 = Mathf.Pow(t, 2);
        // 기울기 구하기
        float h1 = 2 * tPow3 - 3 * tPow2 + 1;     // (2 * t^3 - 3 * t^2 + 1)
        float h2 = tPow3 - 2 * tPow2 + t;         // (t^3 - 2 * t^2 + t)
        float h3 = 3 * tPow2 - 2 * tPow3;         // (3 * t^2 - 2 * t^3)
        float h4 = tPow3 - tPow2;                 // (t^3 - t^2)
        
        return (h1 * p0) + (h2 * t0) + (h3 * p1) + (h4 * t1);
    }

    public static Vector3 HermiteCurveMat(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
    {
        Matrix4x4 Xt = GetHermiteCurveMat(p0,t0,p1,t1,t);
        return Xt.GetRow(0) + Xt.GetRow(1) + Xt.GetRow(2) + Xt.GetRow(3);
    }
    
    public static Matrix4x4 GetHermiteCurveMat(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
    {
        // 에르밋 곡선을 수식으로 나타내면 P(u)= a0+a1u+a2u^2+a3u^3 라고 할 수 있다.
        // 3차 곡선으로 표현하기 위해 최대 3차항까지 있다.
        
        // 구하고자 하는 Hermite의 지점을 X(t) 라고 할때 X = T * A
        // T = [ t^3 t^2 t 1 ] 이다.
        
        // X = T * A 이기에 각 정점들을 아래처럼 구할 수 있다.
        // p1 = X(0) = [ t^3 t^2 t 1 ] X A
        Vector4 p0Basis = new Vector4(0, 0, 0, 1);
        // t1 = X'(0) = [ 3t^2 2t 1 0 ] X A
        Vector4 t0Basis = new Vector4(0, 0, 1, 0);
        // p2 = X(1) = [ t^3 t^2 t 1 ] X A
        Vector4 p1Basis = new Vector4(1, 1, 1, 1);
        // t2 = X'(1) = [ 3t^2 2t 1 0 ] X A
        Vector4 t1Basis = new Vector4(3, 2, 1, 0);

        Matrix4x4 T = new Matrix4x4(new Vector4(Mathf.Pow(t, 3), Mathf.Pow(t, 2),t, 1), Vector4.zero,Vector4.zero,Vector4.zero).transpose;
        // 기존 정점들의 t값을 담은 행렬
        Matrix4x4 B = new Matrix4x4(p0Basis, t0Basis, p1Basis, t1Basis).transpose;
        Matrix4x4 G = new Matrix4x4(p0, t0, p1, t1).transpose;
        
        // 즉 P = [p1, t1, p2, t2] = B X A 라는 뜻이다.
        // 반대로 A = B^-1 X G 가 된다.
        
        // X(t) = T * A
        // = T X B^-1 X G
        Matrix4x4 B_I = B.inverse;
        Matrix4x4 Xt = (T * B_I) * G;
        return Xt;
    }
    public static Vector3 Derivative(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
    {
        float tPow2 = Mathf.Pow(t, 2);
        float h1Derivative = 6 * tPow2 - 6 * t;
        float h2Derivative = 3 * tPow2 - 4 * t + 1;
        float h3Derivative = 6 * t - 6 * tPow2;
        float h4Derivative = 3 * tPow2 - 2 * t;

        // Compute derivative
        Vector3 result =
            (h1Derivative * p0) +
            (h2Derivative * t0) +
            (h3Derivative * p1) +
            (h4Derivative * t1);
        return result;
    }
    /// <summary>
    /// 대략적으로 호의 길이를 계산하여 반환합니다.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="t0"></param>
    /// <param name="p1"></param>
    /// <param name="t1"></param>
    /// <returns></returns>
    public static float ApproximateLength(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float segmentCount = 20)
    {
        Vector2 previousPoint = p0;
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++) {
            float division = 1 * segmentIndex / segmentCount;
            Vector2 pointOnCurve = GetPointOnHermiteCurve(p0, t0 , p1, t1, division);
            dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);
            previousPoint = pointOnCurve;
        }
        return dstSinceLastEvenPoint;
    }

    /// <summary>
    /// Hermite Curve를 t비율 만큼 2등분으로 자릅니다.
    /// 총 8개의 Point 정보를 반환합니다.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="t0"></param>
    /// <param name="p1"></param>
    /// <param name="t1"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3[] SplitHermite(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
    {
        Vector3[] splitHermitePoints = new Vector3[8];
        Vector2[] bezier = Bezier.HermiteToBezier(p0, t0, p1, t1).Segment;
        Vector3[] bezierSplit = Bezier.SplitCubicBezier(bezier[0], bezier[1], bezier[2], bezier[3], t);
        Vector2[] hermiteSplit1 = Bezier.BezierToHermite(bezierSplit[0],bezierSplit[1],bezierSplit[2],bezierSplit[3]).Segment;
        Vector2[] hermiteSplit2 = Bezier.BezierToHermite(bezierSplit[4],bezierSplit[5],bezierSplit[6],bezierSplit[7]).Segment;

        splitHermitePoints[0] = hermiteSplit1[0];
        splitHermitePoints[1] = hermiteSplit1[1];
        splitHermitePoints[2] = hermiteSplit1[2];
        splitHermitePoints[3] = hermiteSplit1[3];
        
        splitHermitePoints[4] = hermiteSplit2[0];
        splitHermitePoints[5] = hermiteSplit2[1];
        splitHermitePoints[6] = hermiteSplit2[2];
        splitHermitePoints[7] = hermiteSplit2[3];
        
        return splitHermitePoints;
    }

    public static Vector2[] SplitUsingMatrix(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float t)
    {
        // X(t) = T * M * G    G = [p1 t1 p2 t2]
        // X1(1) = X = T * M * G    G = [p1 t1 x ??]
        // X2(0) = X = T * M * G    G = [x ?? p2 t2]
        // T X B^-1 X G
        // x = TM * G
        
        // 에르밋 곡선을 수식으로 나타내면 P(u)= a0+a1u+a2u^2+a3u^3 라고 할 수 있다.
        // 3차 곡선으로 표현하기 위해 최대 3차항까지 있다.
        
        // 구하고자 하는 Hermite의 지점을 X(t) 라고 할때 X = T * A
        // T = [ t^3 t^2 t 1 ] 이다.
        
        // X = T * A 이기에 각 정점들을 아래처럼 구할 수 있다.
        // p1 = X(0) = [ t^3 t^2 t 1 ] X A
        Vector4 p0Basis = new Vector4(0, 0, 0, 1);
        // t1 = X'(0) = [ 3t^2 2t 1 0 ] X A
        Vector4 t0Basis = new Vector4(0, 0, 1, 0);
        // p2 = X(1) = [ t^3 t^2 t 1 ] X A
        Vector4 p1Basis = new Vector4(1, 1, 1, 1);
        // t2 = X'(1) = [ 3t^2 2t 1 0 ] X A
        Vector4 t1Basis = new Vector4(3, 2, 1, 0);

        Matrix4x4 T = new Matrix4x4(new Vector4(Mathf.Pow(t, 3), Mathf.Pow(t, 2),t, 1), Vector4.zero,Vector4.zero,Vector4.zero).transpose;
        // 기존 정점들의 t값을 담은 행렬
        Matrix4x4 B = new Matrix4x4(p0Basis, t0Basis, p1Basis, t1Basis).transpose;
        Matrix4x4 G = new Matrix4x4(p0, t0, p1, t1).transpose;
        
        // 즉 P = [p1, t1, p2, t2] = B X A 라는 뜻이다.
        // 반대로 A = B^-1 X G 가 된다.

        float z = t;
        // X(t) = T * A
        // = T X B^-1 X G
        Matrix4x4 B_I = B.inverse;
        Matrix4x4 M = B_I;
        
        // 곡선을 어떤 지점에서 분할한다 할때 t = z이면, 곡선의 점 정보를 새로운 행렬 곱셉으로 분리한다.
        // X(t) = T * A
        // = T X M X G
        // T = [ (z*t)^3 (z*t)^2 z*t 1 ]
        // 해당 z를 분리시키면
        // Z = [ Z^3 0 0 0 ]
        //     [ 0 z^2 0 0 ]
        //     [ 0 0 z 0 ]
        //     [ 0 0 0 1 ]
        // X(t) = T * Z * M X G 가 된다.

        Matrix4x4 Z = new Matrix4x4(
            new Vector4(Mathf.Pow(z,3),0,0,0),
            new Vector4(0,Mathf.Pow(z,2),0,0),
            new Vector4(0,0,Mathf.Pow(z,1),0),
            new Vector4(0,0,0,1));
        
        // X(t) = T * Z * M X G = T * M * M^{-1} * Z * M * G
        // = T * M * (Q) * G
        // Q = M^{-1} * Z * M 로 변환한다.
        // M * M^{-1}는 행렬식에 아무 변화를 주지 않지면 계산 순서를 바꾼다.
        // 그래서 최종적으론 T * M * Q * G로 축약해 적절한 행렬 표현이 남게되고
        // Q는 t = 0에서 t = z 까지의 곡선을 나타내는 새로운 좌표 집합이 된다.
        Matrix4x4 Q = M.inverse * Z * M;
        Matrix4x4 QG = Q * G;

        Vector2[] newPoints = new Vector2[6];
        newPoints[0] = QG.GetRow(0);
        newPoints[1] = QG.GetRow(1);
        newPoints[2] = QG.GetRow(2);
        newPoints[3] = QG.GetRow(3);

        // [ z, 1 ] 구간을 다음과 같이 평가한다.
        // T = [ (z+(1-z)*t)^3 (z+(1-z)*t)^2 z+(1-z)*t 1 ]
        // T = [ t^3 t^2 t 1 ]
        // 해당 z를 분리시키면
        // Z = [ (1-z)^3 0 0 0 ]
        //     [ 0 2(1-z)^2 0 0 ]
        //     [ 0 0 (1-z) 0 ]
        //     [ z^3 z^2 z 1 ]
        
        
        Z = new Matrix4x4(
            new Vector4(Mathf.Pow(1 - z, 3), 0, 0, 0),
            new Vector4(3 * z * Mathf.Pow(1 - z, 2),Mathf.Pow(1 - z, 2), 0, 0),
            new Vector4(3 * Mathf.Pow(z, 2) * (1 - z), 2 * z * (1 - z), 1-z, 0),
            new Vector4(Mathf.Pow(z, 3), Mathf.Pow(z, 2), z, 1)
        ).transpose;
        
        Q = M.inverse * Z * M;
        QG = Q * G;
        
        // X = T M G
        newPoints[4] = QG.GetRow(0);
        newPoints[5] = QG.GetRow(1);
        newPoints[6] = QG.GetRow(2);
        newPoints[7] = QG.GetRow(3);
        return newPoints;
    }
    
    public static HermitePath HermiteLerp(HermitePath hermite1, HermitePath hermite2, float t)
    {
        HermitePath copyHermite1 = new HermitePath(hermite1);
        HermitePath copyHermite2 = new HermitePath(hermite2);
        
        float[] pointRatios1 = copyHermite1.GetPointRatiosInCurve();
        float[] pointRatios2 = copyHermite2.GetPointRatiosInCurve();

        if (pointRatios1.Length == 0)
        {
            return copyHermite2;
        }
        if (pointRatios2.Length == 0)
        {
            return copyHermite1;
        }
        
        for (int i = 0; i < pointRatios1.Length; i++)
        {
            float ratio = pointRatios1[i];
            
            if(ratio == 1 || ratio == 0)
                continue;
            copyHermite2.Split(ratio);
        }
        for (int i = 0; i < pointRatios2.Length; i++)
        {
            float ratio = pointRatios2[i];
            
            if(ratio == 1 || ratio == 0)
                continue;
            copyHermite1.Split(ratio);
        }
        
        List<Hermite> newHermiteSegments = new List<Hermite>();
        for (int i = 0; i < copyHermite1.SegmentCount && i < copyHermite2.SegmentCount ; i++)
        {
            Hermite segment1 = (Hermite)copyHermite1.GetSegment(i);
            Vector2 p0 = segment1.P0;
            Vector2 t0 = segment1.T0;
                
            Hermite segment2 = (Hermite)copyHermite2.GetSegment(i);
            Vector2 pp0 = segment2.P0;
            Vector2 tt0 = segment2.T0;
                
            Vector2 lerpPoint0 = Vector2.Lerp(p0, pp0, t);
            Vector2 newTangent0 = Vector2.Lerp(t0, tt0, t);
            
            Vector2 p1 = segment1.P1;
            Vector2 t1 = segment1.T1;
            
            Vector2 pp1 = segment2.P1;
            Vector2 tt1 = segment2.T1;
                
            Vector2 lerpPoint1 = Vector2.Lerp(p1, pp1, t);
            Vector2 newTangent1 = Vector2.Lerp(t1, tt1, t);
            newHermiteSegments.Add(new Hermite(lerpPoint0, newTangent0, lerpPoint1, newTangent1));
        }
        HermitePath newPath = new HermitePath(newHermiteSegments.AsEnumerable());
        
        copyHermite1.DrawOnGizmo(true, false);
        copyHermite2.DrawOnGizmo(true, false);
        return newPath;
    }
    

    // public static void GetBoundingBox(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1)
    // {
    //     Vector3[] hermiteToBezier = Bezier.HermiteToBezier(p0, t0, p1, t1);
    //     Bezier.GetBoundingBox(hermiteToBezier[0], hermiteToBezier[1], hermiteToBezier[2], hermiteToBezier[3]);
    // }
}
