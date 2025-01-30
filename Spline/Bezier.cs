using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bezier : Spline
{
    private List<Vector2> points = new List<Vector2>();
    
    public Bezier(Vector2 centre) {
        points = new List<Vector2> {
            centre + Vector2.left,
            centre + (Vector2.left + Vector2.up) * 0.5f,
            centre + (Vector2.right + Vector2.down) * 0.5f,
            centre + Vector2.right
        };
    }
    public override Vector2 this[int i] {
        get { return points[i]; }
    }
    public override int NumPoints {
        get { return points.Count; }
    }
    public override int NumSegments {
        get { return (points.Count - 4) / 3 + 1; }
    }
    public override void AddPoint(Vector2 anchorPos) {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);
    }
    public override Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        if (points == null || points.Count <= 0)
            return new Vector2[0];
        
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++) {
            Vector2[] p = GetSegment(segmentIndex);
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
        }

        return evenlySpacedPoints.ToArray();
    }
    public override Vector2[] GetSegment(int i) {
        return new Vector2[] { points[i * 3], points[i * 3 + 1], 
            points[i * 3 + 2], points[i * 3 + 3] }; 
    }
    public override void MovePoint(int i, Vector2 pos) {
        Vector2 deltaPos = pos - points[i];
        points[i] = pos;
        if(i % 3 == 0) {
            if(i + 1 < points.Count) {
                points[i + 1] += deltaPos;
            }
            if(i - 1 >= 0) {
                points[i - 1] += deltaPos;
            }
        } else if(i > 1 && i < points.Count - 2){
            int crossControlIndex = i % 3 == 1 ? i - 2 : i + 2;
            crossControlIndex = i % 3 == 2 ? i + 2 : crossControlIndex;
            int anchorIndex = i % 3 == 1 ? i - 1 : i + 1;
            float ControlDst = Vector2.Distance(points[anchorIndex] , points[crossControlIndex]);
            Vector2 ControlDir = (points[anchorIndex] - points[i]).normalized;

            Vector2 ControlVec = points[anchorIndex] + ControlDir * ControlDst;
            points[crossControlIndex] = ControlVec;
        }
    }

    public override void DeletePoint(int anchorIndex) {
        if(anchorIndex == 0) {
            points.RemoveRange(0, 3);
        }
        else
        if (anchorIndex == NumSegments) {
            points.RemoveRange(points.Count - 1 - 2, 3);
        }
        else
            if(anchorIndex > 0 && anchorIndex < NumSegments) {
            points.RemoveRange(anchorIndex * 3 - 1, 3);
        }
    }
    
    
    public static Vector2 GetPointOnBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t) {
        Vector3 a = Vector2.Lerp(p0, p1, t);
        Vector3 b = Vector2.Lerp(p1, p2, t);
        Vector3 c = Vector2.Lerp(p2, p3, t);

        Vector3 d = Vector2.Lerp(a, b, t);
        Vector3 e = Vector2.Lerp(b, c, t);

        Vector2 pointOnCurve = Vector2.Lerp(d, e, t);

        return pointOnCurve;
    }
    
    /// <summary>
    /// 2차 베지어 곡선
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t) {

        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }
    
    /// <summary>
    /// 3차 베지어 곡선
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t) {
        Vector2 p0 = EvaluateQuadratic(a, b, c, t);
        Vector2 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }

    /// <summary>
    /// 3차 베지어 곡선을 2차 베지어 곡선으로 변환합니다.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <returns></returns>
    public static Vector2[] CubicToQuadratic(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 v1 = 3 * (p2 - p1);
        Vector2 v2 = 3 * (p3 - p2);
        Vector2 v3 = 3 * (p4 - p3);
        return new Vector2[3] { v1, v2, v3 };
    }

    public static void Casteljau(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, int depth, ref List<Vector2> left, ref List<Vector2> right)
    {

        if (depth > 0)
        {
            Vector2 m1 = (p1 + p2) / 2;
            Vector2 m2 = (p2 + p3) / 2;
            Vector2 m3 = (p3 + p4) / 2;
            Vector2 m4 = (m1 + m2) / 2;
            Vector2 m5 = (m2 + m3) / 2;
            Vector2 m6 = (m4 + m5) / 2;
            Casteljau(p1, m1, m4, m6, depth - 1, ref left, ref right);
            Casteljau(m6, m5, m3, p4, depth - 1, ref left, ref right);
        }
        else
        {
            
        }
    }

    public static Vector3[] HermiteToBezier(Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2)
    {
        Vector3[] vec = new Vector3[4];
        vec[0] = p1;
        vec[1] = p1 + (v1 / 3);
        vec[2] = p2 - (v2 / 3);
        vec[3] = p2;
        return vec;
    }

    public static Vector3[] BezierToHermite(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector3[] hermite = new Vector3[4];
        // Hermite 포지션
        hermite[0] = p1;
        hermite[2] = p4;

        // Hermite 접선
        hermite[1] = 3 * (p2 - p1);
        hermite[3] = 3 * (p4 - p3);
        return hermite;
    }
    // https://pomax.github.io/bezierinfo/ko-KR/index.html#catmullconv
    // 11번 공식을 사용했습니다.
    public static Vector3[] SplitBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float z)
    {
        Vector3[] splitPoints = new Vector3[8];
        Vector3 newP1 = p1;
        Vector3 newP2 = z * p2 - (z - 1) * p1;
        Vector3 newP3 = z * z * p3 - 2 * z * (z - 1) * p2 + Mathf.Pow(z - 1, 2) * p1;
        Vector3 newP4 = Mathf.Pow(z, 3) * p4 - 3 * z * z * (z - 1) * p3 + 3 * z * Mathf.Pow(z - 1, 2) * p2 -
                        Mathf.Pow(z - 1, 3) * p1;
        splitPoints[0] = newP1;
        splitPoints[1] = newP2;
        splitPoints[2] = newP3;
        splitPoints[3] = newP4;
        
        newP1 = Mathf.Pow(z, 3) * p4 - 3 * z * z * (z - 1) * p3 + 3 * z * Mathf.Pow(z - 1, 2) * p2 -
                        Mathf.Pow(z - 1, 3) * p1;
        newP2 = z * z * p4 - 2 * z * (z - 1) * p3 + Mathf.Pow(z - 1, 2) * p2;
        newP3 = z * p4 - (z - 1) * p3;
        newP4 = p4;
        
        splitPoints[4] = newP1;
        splitPoints[5] = newP2;
        splitPoints[6] = newP3;
        splitPoints[7] = newP4;
        
        return splitPoints;
    }
    
    // Helper function to check if a value is within [0, 1]
    private static bool Accept(float t)
    {
        return 0 <= t && t <= 1;
    }

    // Cubic root function to handle negative values
    private static float CubeRoot(float v)
    {
        return v < 0 ? -Mathf.Pow(-v, 1f / 3f) : Mathf.Pow(v, 1f / 3f);
    }

    // Main function to find cubic roots
    // Using Cardano algorithm
    public static List<float> GetCubicRoots(float pa, float pb, float pc, float pd)
    {
        float a = 3 * pa - 6 * pb + 3 * pc;
        float b = -3 * pa + 3 * pb;
        float c = pa;
        float d = -pa + 3 * pb - 3 * pc + pd;

        // Check if this is a cubic equation
        float discriminant;
        float root1;
        float root2;
        if (Mathf.Approximately(d, 0))
        {
            // Not a cubic equation
            if (Mathf.Approximately(a, 0))
            {
                // Not a quadratic equation either
                if (Mathf.Approximately(b, 0))
                {
                    // No solutions
                    return new List<float>();
                }
                // Linear equation
                float linearRoot = -c / b;
                return Accept(linearRoot) ? new List<float> { linearRoot } : new List<float>();
            }

            // Quadratic equation
            discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return new List<float>();

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float twoA = 2 * a;

            root1 = (-b + sqrtDiscriminant) / twoA;
            root2 = (-b - sqrtDiscriminant) / twoA;

            List<float> quadraticRoots = new List<float>();
            if (Accept(root1)) quadraticRoots.Add(root1);
            if (Accept(root2)) quadraticRoots.Add(root2);

            return quadraticRoots;
        }

        // Normalize coefficients
        a /= d;
        b /= d;
        c /= d;

        float p = (3 * b - a * a) / 3;
        float q = (2 * a * a * a - 9 * a * b + 27 * c) / 27;
        float p3 = p / 3;
        float q2 = q / 2;

        discriminant = q2 * q2 + p3 * p3 * p3;

        // Variables for roots
        float root3;

        // Three possible real roots
        if (discriminant < 0)
        {
            float mp3 = -p / 3;
            float mp33 = mp3 * mp3 * mp3;
            float r = Mathf.Sqrt(mp33);
            float t = -q / (2 * r);
            float cosphi = Mathf.Clamp(t, -1, 1);
            float phi = Mathf.Acos(cosphi);
            float crtr = CubeRoot(r);
            float t1 = 2 * crtr;

            root1 = t1 * Mathf.Cos(phi / 3) - a / 3;
            root2 = t1 * Mathf.Cos((phi + 2 * Mathf.PI) / 3) - a / 3;
            root3 = t1 * Mathf.Cos((phi + 4 * Mathf.PI) / 3) - a / 3;

            List<float> cubicRoots = new List<float>();
            if (Accept(root1)) cubicRoots.Add(root1);
            if (Accept(root2)) cubicRoots.Add(root2);
            if (Accept(root3)) cubicRoots.Add(root3);

            return cubicRoots;
        }

        // Three real roots, two of them are equal
        if (Mathf.Approximately(discriminant, 0))
        {
            float u1 = q2 < 0 ? CubeRoot(-q2) : -CubeRoot(q2);
            root1 = 2 * u1 - a / 3;
            root2 = -u1 - a / 3;

            List<float> doubleRoots = new List<float>();
            if (Accept(root1)) doubleRoots.Add(root1);
            if (Accept(root2)) doubleRoots.Add(root2);

            return doubleRoots;
        }

        // One real root, two complex roots
        float sd = Mathf.Sqrt(discriminant);
        float u = CubeRoot(sd - q2);
        float v = CubeRoot(sd + q2);
        root1 = u - v - a / 3;

        return Accept(root1) ? new List<float> { root1 } : new List<float>();
    }

    public static Vector2[] GetBoundingBox(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2[] quadratic = CubicToQuadratic(p1, p2, p3, p4);
        Vector2 v1 = quadratic[0];
        Vector2 v2 = quadratic[1];
        Vector2 v3 = quadratic[2];
        Vector2 a = v1 - 2 * v2 + v3;
        Vector2 b = 2 * (v2 - v1);
        Vector2 c = v1;
        // Vector2 f = -b + Vector2.sq
        return null;
    }
}
