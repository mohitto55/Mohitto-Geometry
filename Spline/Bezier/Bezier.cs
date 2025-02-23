using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class Bezier : Spline
{
    /// <summary>
    /// 2차 베지어 곡선
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector2 EvaluateQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float t) 
    {
        Vector2 m0 = Vector2.Lerp(p0, p1, t);
        Vector2 m1 = Vector2.Lerp(p1, p2, t);
        return Vector2.Lerp(m0, m1, t);
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
    public static Vector2 EvaluateCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float mt = 1 - t;
        float mt2 = mt * mt;
        float mt3 = mt2 * mt;
        float t2 = t * t;
        float t3 = t2 * t;

        return mt3 * p0 + 3 * mt2 * p1 * t + 3 * mt * t2 * p2 + t3 * p3;
    }

    /// <summary>
    /// 3차 베지어 곡선을 2차 베지어 곡선으로 변환합니다.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <returns></returns>
    public static void CubicToQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, 
        out Vector2 q0, out Vector2 q1, out Vector2 q2)
    {
        // 2차 베지어 곡선의 제어점 설정
        q0 = p0;
        q2 = p3;
        q1 = Vector2.Lerp((3 * p1 - p0) / 2, (3 * p2 - q2) / 2, 0.5f);
    }

    public static CubicBezier HermiteToBezier(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1)
    {
        Vector3[] vec = new Vector3[4];
        vec[0] = p0;
        vec[1] = p0 + (t0 / 3);
        vec[2] = p1 - (t1 / 3);
        vec[3] = p1;
        return new CubicBezier(vec[0], vec[1], vec[2], vec[3]);
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="start"></param>
    /// <param name="mid"></param>
    /// <param name="end"></param>
    /// <returns></returns>
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

    public static QuadraticBezier CreateCubicFromThreePoint(Vector2 start, Vector2 mid, Vector2 end)
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
    
    public static Hermite BezierToHermite(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector3[] hermite = new Vector3[4];
        // Hermite 포지션
        hermite[0] = p1;
        hermite[2] = p4;

        // Hermite 접선
        hermite[1] = 3 * (p2 - p1);
        hermite[3] = 3 * (p4 - p3);
        return new Hermite(hermite[0], hermite[1], hermite[2], hermite[3]);
    }
    // https://pomax.github.io/bezierinfo/ko-KR/index.html#catmullconv
    // 11번 공식을 사용했습니다.
    public static Vector3[] SplitCubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float z)
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
        CubicToQuadratic(p1, p2, p3, p4, out var q0, out var q1, out var q2);
        Vector2[] quadratic = new Vector2[3]{q0,q1,q2};
        Vector2 v1 = quadratic[0];
        Vector2 v2 = quadratic[1];
        Vector2 v3 = quadratic[2];
        Vector2 a = v1 - 2 * v2 + v3;
        Vector2 b = 2 * (v2 - v1);
        Vector2 c = v1;
        // Vector2 f = -b + Vector2.sq
        return null;
    }
    
    
    // solves cubic for bezier 3 returns first root
    // function solveBezier3(A, B, C, D){  
    //     // There can be 3 roots, u,u1,u2 hold the results;
    //     // Solves 3rd order a+(-2a+3b)t+(2a-6b+3c)t^2+(-a+3b-3c+d)t^3 Cardano method for finding roots
    //     // this function was derived from http://pomax.github.io/bezierinfo/#intersections cube root solver
    //     // Also see https://en.wikipedia.org/wiki/Cubic_function#Cardano.27s_method
    //
    //     function crt(v) {
    //         if(v<0) return -Math.pow(-v,1/3);
    //         return Math.pow(v,1/3);
    //     }                
    //     function sqrt(v) {
    //         if(v<0) return -Math.sqrt(-v);
    //         return Math.sqrt(v);
    //     }        
    //     var a, b, c, d, p, p3, q, q2, discriminant, U, v1, r, t, mp3, cosphi,phi, t1, sd;
    //     u2 = u1 = u = -Infinity;
    //     d = (-A + 3 * B - 3 * C + D);
    //     a = (3 * A - 6 * B + 3 * C) / d;
    //     b = (-3 * A + 3 * B) / d;
    //     c = A / d;
    //     p = (3 * b - a * a) / 3;
    //     p3 = p / 3;
    //     q = (2 * a * a * a - 9 * a * b + 27 * c) / 27;
    //     q2 = q / 2;
    //     a /= 3;
    //     discriminant = q2 * q2 + p3 * p3 * p3;
    //     if (discriminant < 0) {
    //         mp3 = -p / 3;
    //         r = sqrt(mp3 * mp3 * mp3);
    //         t = -q / (2 * r);
    //         cosphi = t < -1 ? -1 : t > 1 ? 1 : t;
    //         phi = Math.acos(cosphi);
    //         t1 = 2 * crt(r);
    //         u = t1 * Math.cos(phi / 3) - a;
    //         u1 = t1 * Math.cos((phi + 2 * Math.PI) / 3) - a;
    //         u2 = t1 * Math.cos((phi + 4 * Math.PI) / 3) - a;
    //         return u;
    //     }
    //     if(discriminant === 0) {
    //         U = q2 < 0 ? crt(-q2) : -crt(q2);
    //         u = 2 * U - a;           
    //         u1 = -U - a;            
    //         return u;
    //     }                
    //     sd = sqrt(discriminant);
    //     u = crt(sd - q2) - crt(sd + q2) - a; 
    //     return u;      
    // }   
}
