using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class Hermite : Spline
{
    private List<Vector2> points = new List<Vector2>();
    
    public Hermite(Vector2 centre) {
        points = new List<Vector2> {
            centre + Vector2.left,
            centre + (Vector2.left + Vector2.up) * 0.5f,
            centre + (Vector2.right + Vector2.down) * 0.5f,
            centre + Vector2.right
        };
    }

    public Hermite(List<Vector2> points)
    {
        this.points = new List<Vector2>(points);
    }
    public override Vector2 this[int i] {
        get { return points[i]; }
    }
    public override int NumPoints {
        get { return points.Count; }
    }
    public override int NumSegments {
        get { return (points.Count / 2) - 1; }
    }

    public void AddPoint(int pointIndex, Vector2 point, Vector2 tangent)
    {
        points.Insert(pointIndex, tangent + point);
        points.Insert(pointIndex, point);
    }

    public override void AddPoint(Vector2 anchorPos) {
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);
    }
    public override Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1) {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        if (points == null)
            points = new List<Vector2>();
        
        if(points.Count <= 0)
            return evenlySpacedPoints.ToArray();
        
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++) {
            Vector2[] p = GetSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = (Vector2)HermiteCurve(p[0], p[1] - p[0], p[2], p[3] - p[2] , t);
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
        }

        return evenlySpacedPoints.ToArray();
    }

    public override Vector2[] GetSegment(int i) {
        return new Vector2[] { points[i * 2], points[i * 2 + 1], 
            points[i * 2 + 2], points[i * 2 + 3] }; 
    }
    
    public Vector2 GetPoint(int i) {
        return points[i * 2]; 
    }
    public override void MovePoint(int i, Vector2 pos) {
        Vector2 deltaPos = pos - points[i];
        points[i] = pos;
        if(i % 2 == 0) {
            if(i + 1 < points.Count) {
                points[i + 1] += deltaPos;
            }
        } 
    }

    public override void DeletePoint(int anchorIndex) {
        if(anchorIndex == 0) {
            points.RemoveRange(0, 2);
        }
        else
        if (anchorIndex == NumSegments) {
            points.RemoveRange(points.Count - 2, 2);
        }
        else
            if(anchorIndex > 0 && anchorIndex < NumSegments) {
            points.RemoveRange(anchorIndex * 2, 2);
        }
    }
    
    public void Split(float t)
    {
        int segmentIndex = GetSegmentIndex(t);
        if (segmentIndex == -1)
            return;
        
        Vector2[] segment = GetSegment(segmentIndex);
        Vector3[] splitHermite = SplitHermite(segment[0], segment[1] - segment[0], segment[2], segment[3] - segment[2], t);
        AddPoint(segmentIndex * 2 + 2, splitHermite[2], splitHermite[3]);
    }
    
    public int GetSegmentIndex(float t)
    {
        float[] pointRatios = GetPointRatiosInCurve();
        for (int i = 0; i < pointRatios.Length; i++)
        {
            float ratio = pointRatios[i];
            if (ratio >= t)
            {
                return i - 1;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p1">Ŀ���� ������</param>
    /// <param name="p2">Ŀ���� ����</param>
    /// <param name="t1">�������� tangent</param>
    /// <param name="t2">������ tangent</param>
    /// <param name="t">Ŀ���� 0 ~ 1 ��ŭ ������ ����</param>
    /// <returns></returns>
    public static Vector3 HermiteCurve(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float t)
    {
        float tPow3 = Mathf.Pow(t, 3);
        float tPow2 = Mathf.Pow(t, 2);
        // ���� ���ϱ�
        float h1 = 2 * tPow3 - 3 * tPow2 + 1;     // (2 * t^3 - 3 * t^2 + 1)
        float h2 = tPow3 - 2 * tPow2 + t;         // (t^3 - 2 * t^2 + t)
        float h3 = 3 * tPow2 - 2 * tPow3;         // (3 * t^2 - 2 * t^3)
        float h4 = tPow3 - tPow2;                 // (t^3 - t^2)
        
        return (h1 * p1) + (h2 * t1) + (h3 * p2) + (h4 * t2);
    }

    public static Vector3 HermiteCurveMat(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float t)
    {
        Matrix4x4 Xt = GetHermiteCurveMat(p1,t1,p2,t2,t);
        return Xt.GetRow(0) + Xt.GetRow(1) + Xt.GetRow(2) + Xt.GetRow(3);
    }
    
    public static Matrix4x4 GetHermiteCurveMat(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float t)
    {
        // ������ ��� �������� ��Ÿ���� P(u)= a0+a1u+a2u^2+a3u^3 ��� �� �� �ִ�.
        // 3�� ����� ǥ���ϱ� ���� �ִ� 3���ױ��� �ִ�.
        
        // ���ϰ��� �ϴ� Hermite�� ������ X(t) ��� �Ҷ� X = T * A
        // T = [ t^3 t^2 t 1 ] �̴�.
        
        // X = T * A �̱⿡ �� �������� �Ʒ�ó�� ���� �� �ִ�.
        // p1 = X(0) = [ t^3 t^2 t 1 ] X A
        Vector4 p1Basis = new Vector4(0, 0, 0, 1);
        // t1 = X'(0) = [ 3t^2 2t 1 0 ] X A
        Vector4 t1Basis = new Vector4(0, 0, 1, 0);
        // p2 = X(1) = [ t^3 t^2 t 1 ] X A
        Vector4 p2Basis = new Vector4(1, 1, 1, 1);
        // t2 = X'(1) = [ 3t^2 2t 1 0 ] X A
        Vector4 t2Basis = new Vector4(3, 2, 1, 0);

        Matrix4x4 T = new Matrix4x4(new Vector4(Mathf.Pow(t, 3), Mathf.Pow(t, 2),t, 1), Vector4.zero,Vector4.zero,Vector4.zero).transpose;
        // ���� �������� t���� ���� ���
        Matrix4x4 B = new Matrix4x4(p1Basis, t1Basis, p2Basis, t2Basis).transpose;
        Matrix4x4 G = new Matrix4x4(p1, t1, p2, t2).transpose;
        
        // �� P = [p1, t1, p2, t2] = B X A ��� ���̴�.
        // �ݴ�� A = B^-1 X G �� �ȴ�.
        
        // X(t) = T * A
        // = T X B^-1 X G
        Matrix4x4 B_I = B.inverse;
        Matrix4x4 Xt = (T * B_I) * G;
        return Xt;
    }
    public static Vector3 HermiteSplineDerivative(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float t)
    {
        float tPow2 = Mathf.Pow(t, 2);
        float h1Derivative = 6 * tPow2 - 6 * t;
        float h2Derivative = 3 * tPow2 - 4 * t + 1;
        float h3Derivative = 6 * t - 6 * tPow2;
        float h4Derivative = 3 * tPow2 - 2 * t;

        // Compute derivative
        Vector3 result =
            (h1Derivative * p1) +
            (h2Derivative * t1) +
            (h3Derivative * p2) +
            (h4Derivative * t2);
        return result;
    }
    /// <summary>
    /// �뷫������ ȣ�� ���̸� ����Ͽ� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="t1"></param>
    /// <param name="p2"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static float ApproximateLength(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float segmentCount = 10)
    {
        Vector2 previousPoint = p1;
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++) {
            float division = 1 * segmentIndex / segmentCount;
            Vector2 pointOnCurve = HermiteCurve(p1, t1 , p2, t2, division);
            dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);
            previousPoint = pointOnCurve;
        }
        return dstSinceLastEvenPoint;
    }
    /// <summary>
    /// Ŀ�긦 �����ϴ� ����Ʈ���� ��ġ ������ 0~1�� ��ȯ�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns></returns>
    public float[] GetPointRatiosInCurve()
    {
        if (NumSegments == 0)
            return new float[0];

        // 0���� ����Ʈ ������ 0
        float[] pointsRatio = new float[NumSegments + 1];
        float totalLength = 0;
        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] segments = GetSegment(segmentIndex);
            Vector2 p1 = segments[0];
            Vector2 t1 = segments[1] - p1;
            Vector2 p2 = segments[2];
            Vector2 t2 = segments[3] - p2;
            float curveLength = ApproximateLength(p1, t1, p2, t2);
            totalLength += curveLength;
            pointsRatio[segmentIndex + 1] = totalLength;
        }

        for (int pointIndex = 0; pointIndex < pointsRatio.Length; pointIndex++)
        {
            float pointLength = pointsRatio[pointIndex];
            pointsRatio[pointIndex] = Mathf.Clamp(pointLength / totalLength, 0, 1);
        }

        return pointsRatio;
    }

    /// <summary>
    /// Hermite Curve�� t���� ��ŭ 2������� �ڸ��ϴ�.
    /// �� 8���� Point ������ ��ȯ�մϴ�.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="t1"></param>
    /// <param name="p2"></param>
    /// <param name="t2"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3[] SplitHermite(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float t)
    {
        //return SplitUsingMatrix(p1, t1, p2 ,t2, t);
        Vector3[] splitHermitePoints = new Vector3[8];
        Vector3[] bezier = Bezier.HermiteToBezier(p1, t1, p2, t2);
        Vector3[] bezierSplit = Bezier.SplitBezier(bezier[0], bezier[1], bezier[2], bezier[3], t);
        
        Vector3[] hermiteSplit1 = Bezier.BezierToHermite(bezierSplit[0],bezierSplit[1],bezierSplit[2],bezierSplit[3]);
        Vector3[] hermiteSplit2 = Bezier.BezierToHermite(bezierSplit[4],bezierSplit[5],bezierSplit[6],bezierSplit[7]);

        splitHermitePoints[0] = hermiteSplit1[0];
        splitHermitePoints[1] = hermiteSplit1[1];

        Vector3 X = HermiteCurve(p1, t1, p2, t2, t);
        Vector3 der = HermiteSplineDerivative(p1, t1, p2, t2, t);
        splitHermitePoints[2] = hermiteSplit1[2];
        splitHermitePoints[3] = der.normalized;
        
        splitHermitePoints[4] = hermiteSplit2[0];
        splitHermitePoints[5] = hermiteSplit2[1];
        splitHermitePoints[6] = hermiteSplit2[2];
        splitHermitePoints[7] = hermiteSplit2[3];
        
        return splitHermitePoints;
    }

    public static Vector2[] SplitUsingMatrix(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2, float t)
    {
        // X(t) = T * M * G    G = [p1 t1 p2 t2]
        // X1(1) = X = T * M * G    G = [p1 t1 x ??]
        // X2(0) = X = T * M * G    G = [x ?? p2 t2]
        // T X B^-1 X G
        // x = TM * G
        
        // ������ ��� �������� ��Ÿ���� P(u)= a0+a1u+a2u^2+a3u^3 ��� �� �� �ִ�.
        // 3�� ����� ǥ���ϱ� ���� �ִ� 3���ױ��� �ִ�.
        
        // ���ϰ��� �ϴ� Hermite�� ������ X(t) ��� �Ҷ� X = T * A
        // T = [ t^3 t^2 t 1 ] �̴�.
        
        // X = T * A �̱⿡ �� �������� �Ʒ�ó�� ���� �� �ִ�.
        // p1 = X(0) = [ t^3 t^2 t 1 ] X A
        Vector4 p1Basis = new Vector4(0, 0, 0, 1);
        // t1 = X'(0) = [ 3t^2 2t 1 0 ] X A
        Vector4 t1Basis = new Vector4(0, 0, 1, 0);
        // p2 = X(1) = [ t^3 t^2 t 1 ] X A
        Vector4 p2Basis = new Vector4(1, 1, 1, 1);
        // t2 = X'(1) = [ 3t^2 2t 1 0 ] X A
        Vector4 t2Basis = new Vector4(3, 2, 1, 0);

        Matrix4x4 T = new Matrix4x4(new Vector4(Mathf.Pow(t, 3), Mathf.Pow(t, 2),t, 1), Vector4.zero,Vector4.zero,Vector4.zero).transpose;
        // ���� �������� t���� ���� ���
        Matrix4x4 B = new Matrix4x4(p1Basis, t1Basis, p2Basis, t2Basis).transpose;
        Matrix4x4 G = new Matrix4x4(p1, t1, p2, t2).transpose;
        
        // �� P = [p1, t1, p2, t2] = B X A ��� ���̴�.
        // �ݴ�� A = B^-1 X G �� �ȴ�.

        float z = t;
        // X(t) = T * A
        // = T X B^-1 X G
        Matrix4x4 B_I = B.inverse;
        Matrix4x4 M = B_I;
        
        // ��� � �������� �����Ѵ� �Ҷ� t = z�̸�, ��� �� ������ ���ο� ��� �������� �и��Ѵ�.
        // X(t) = T * A
        // = T X M X G
        // T = [ (z*t)^3 (z*t)^2 z*t 1 ]
        // �ش� z�� �и���Ű��
        // Z = [ Z^3 0 0 0 ]
        //     [ 0 z^2 0 0 ]
        //     [ 0 0 z 0 ]
        //     [ 0 0 0 1 ]
        // X(t) = T * Z * M X G �� �ȴ�.

        Matrix4x4 Z = new Matrix4x4(
            new Vector4(Mathf.Pow(z,3),0,0,0),
            new Vector4(0,Mathf.Pow(z,2),0,0),
            new Vector4(0,0,Mathf.Pow(z,1),0),
            new Vector4(0,0,0,1));
        
        // X(t) = T * Z * M X G = T * M * M^{-1} * Z * M * G
        // = T * M * (Q) * G
        // Q = M^{-1} * Z * M �� ��ȯ�Ѵ�.
        // M * M^{-1}�� ��ĽĿ� �ƹ� ��ȭ�� ���� ������ ��� ������ �ٲ۴�.
        // �׷��� ���������� T * M * Q * G�� ����� ������ ��� ǥ���� ���Եǰ�
        // Q�� t = 0���� t = z ������ ��� ��Ÿ���� ���ο� ��ǥ ������ �ȴ�.
        Matrix4x4 Q = M.inverse * Z * M;
        Matrix4x4 QG = Q * G;

        Vector2[] newPoints = new Vector2[6];
        newPoints[0] = QG.GetRow(0);
        newPoints[1] = QG.GetRow(1);
        newPoints[2] = QG.GetRow(2);
        newPoints[3] = QG.GetRow(3);

        // [ z, 1 ] ������ ������ ���� ���Ѵ�.
        // T = [ (z+(1-z)*t)^3 (z+(1-z)*t)^2 z+(1-z)*t 1 ]
        // T = [ t^3 t^2 t 1 ]
        // �ش� z�� �и���Ű��
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
    
    public void DrawOnGizmo(bool drawLerpHermiteLine, bool drawPoint)
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
        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] segment = GetSegment(segmentIndex);
            Gizmos.DrawLine(segment[0], segment[1]);
            Gizmos.DrawLine(segment[2], segment[3]);
            for (int i = 0; i < segment.Length; i++)
            {
                MyGizmos.DrawWireCicle(segment[i], 0.5f, 30);
            }
        }
    }
    
   
    public static Hermite HermiteLerp(Hermite hermite1, Hermite hermite2, float t)
    {
        Hermite copyHermite1 = new Hermite(hermite1.points);
        Hermite copyHermite2 = new Hermite(hermite2.points);
        
        float[] pointRatios1 = copyHermite1.GetPointRatiosInCurve();
        float[] pointRatios2 = copyHermite2.GetPointRatiosInCurve();
        
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
        
        List<Vector2> newSegment = new List<Vector2>();
        for (int i = 0; i < copyHermite1.NumPoints -1 && i < copyHermite2.NumPoints -1 ; i += 2)
        {
            Vector2 p1 = copyHermite1[i];
            Vector2 t1 = copyHermite1[i + 1] - p1;
                
            Vector2 p2 = copyHermite2[i];
            Vector2 t2 = copyHermite2[i + 1] - p2;
                
            Vector2 lerpPoint = Vector2.Lerp(p1, p2, t);
            
            Vector2 newTangent = Vector2.Lerp(t1, t2, t);
            newSegment.Add(lerpPoint);
            newSegment.Add(newTangent + lerpPoint);
        }
        copyHermite1.DrawOnGizmo(true, true);
        copyHermite2.DrawOnGizmo(true, true);
        return new Hermite(newSegment);
    }

    public static void GetBoundingBox(Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2)
    {
        Vector3[] hermiteToBezier = Bezier.HermiteToBezier(p1, v1, p2, v2);
        Bezier.GetBoundingBox(hermiteToBezier[0], hermiteToBezier[1], hermiteToBezier[2], hermiteToBezier[3]);
    }
}
