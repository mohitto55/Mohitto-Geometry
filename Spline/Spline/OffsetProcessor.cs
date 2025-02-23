using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;

public partial class Spline
{
    public SplinePath Offset(float distance)
    {
        SplinePath offsetPath = SubdivideUntilCircular();
        offsetPath.DrawOnGizmo();
        for(int i = 0; i < offsetPath.SegmentCount; i++)
        {
            CubicBezier cubic = offsetPath.GetSegment(i).GetCubicBezier();
            Vector2 startPoint = cubic.StartPoint;
            Vector2 endPoint = cubic.EndPoint;
            
            Gizmos.color = Color.blue;
            Vector2 n0 = cubic.GetDerivative(0);
            Vector2 n1 = cubic.GetDerivative(1);
        
            Vector2 d0 = new Vector2(n0.y, -n0.x).normalized * distance;
            Vector2 d1 = new Vector2(n1.y, -n1.x).normalized * distance;
            Gizmos.DrawLine(startPoint, startPoint + d0);
            Gizmos.DrawLine(endPoint, endPoint + d1);

            Gizmos.DrawLine(startPoint, startPoint + n0);
            Gizmos.DrawLine(endPoint, endPoint + n1);
            
            float a = Tension(cubic.P0, cubic.P1, cubic.P2, cubic.P3);
            float beta = 0.5132216f*a - 0.025407f * distance + 0.296638f;

            Vector2 nextStart = cubic.StartPoint + d0;
            Vector2 nextEnd = cubic.EndPoint + d1;
            Vector2 defaultDir = cubic.EndPoint - cubic.StartPoint;
            Vector2 nextDir = nextEnd - nextStart;
            
            float angle = Vector2.Dot(defaultDir, nextDir);
            
            if (angle < 0)
            {
                offsetPath.RemoveSegment(i);
                i--;
            }
            else
            {
                cubic.P0 += d0;
                cubic.P1 += d0;
                
                cubic.P3 += d1;
                cubic.P2 += d1;
                Vector2 intersectP = Vector2.zero;
                intersectP = LineIntersection(cubic.P0, cubic.P1, cubic.P2, cubic.P3);

                if (intersectP != Vector2.zero)
                {
                    // 장력 B를 사용하여 새로운 제어점 계산
                    cubic.P1 = Vector2.Lerp(cubic.P1, intersectP, Mathf.Clamp(beta,0,1)); //cubic.P0 + beta * (intersectP - cubic.P0);

                    cubic.P2 = Vector2.Lerp(cubic.P2, intersectP,
                        Mathf.Clamp(1-beta,0,1)); //cubic.P3 + (1 - beta) * (intersectP - cubic.P3);
                }

                offsetPath.GetSegment(i).ConvertCubicBezierToSpline(cubic);
            }
        }
        
        return offsetPath;
    }
    
    /// <summary>
    /// 안전한 커브가 될때까지 분할
    /// </summary>
    /// <returns></returns>
    private SplinePath SubdivideUntilCircular()
    {
        // 첫 번째 패스: 극값을 기준으로 분할
        List<float> roots = GetRoots().ToList();
        roots.Sort();
        SplinePath path = new SplinePath(GetCopySpline());
        for (int i = 0; i < roots.Count; i++)
        {
            if(MyMath.FloatZero(1- roots[i]) || MyMath.FloatZero(roots[i]))
                continue;
            
            path.Split(roots[i]);
        }
        
        // 두 번째 패스: 더 간단한 세그먼트로 축소
        float minDst = 0.002f;
        int safe = 200;
        for (int i = 0; i < path.SegmentCount; i++)
        { 
            CubicBezier cubic = path.GetSegment(i).GetCubicBezier();
            
            if (cubic.GetLength(1) < minDst)
            {
                continue;
            }

            Vector2 v = Vector2.zero;
            bool isIntersect = Sweep_Line_Algorithm.Segment.StraightLineIntersection(cubic.P0, cubic.P1, cubic.P3, cubic.P2, ref v);
            if(!isIntersect)
                continue;

            Vector2 triangleCenter = MyGeometry.GetCenter(new List<Vector2>() { cubic.P0, cubic.P3, v });
            Vector2 start = cubic.P0;
            Vector2 end = cubic.P3;
            float d1 = Vector2.Distance(start, triangleCenter);
            float d2 = Vector2.Distance(triangleCenter, end);
            float t = d1 / (d1 + d2);
            
            Vector2 curveMid = cubic.GetPoint(t);
            Vector2 circleCenter;
            
            if(safe <=0)
                break;
            bool isHit = false;
            
            
            isHit = CheckCircle(cubic.P0, cubic.P1, curveMid, out circleCenter);
            
            if (isHit)
            {
                // 0~t
                Vector2 leftCenter = cubic.GetPoint(t / 2);
                float distance = Vector2.Distance(leftCenter, circleCenter);
                Debug.DrawLine(circleCenter, leftCenter);
                
                if (distance >= minDst)
                {
                    Gizmos.color = Color.magenta;
                    MyGizmos.DrawWireCircle(leftCenter, 0.1f);
                    path.Split(i, t / 2f);
                    safe--;
                }
            }
            
            isHit = CheckCircle(cubic.P3, cubic.P2, curveMid, out circleCenter);
            if (isHit)
            {
                // t~1
                Vector2 rightCenter = cubic.GetPoint((t+1) / 2);
                float distance = Vector2.Distance(rightCenter, circleCenter);
                Gizmos.color = Color.magenta;
                Debug.DrawLine(circleCenter, rightCenter);
                if (distance >= minDst)
                {
                    path.Split(i,(t+1) / 2f);
                    safe--;
                    i-=1;
                }
            }
        }
        
        return path;
    }

    private static bool CheckCircle(Vector2 start, Vector2 t0, Vector2 end, out Vector2 circleCenter)
    {
        circleCenter = Vector2.zero;
        Vector2 startToT0 = (t0 - start).normalized;
        Vector2 perpenStartToP1 = new Vector2(-startToT0.y, startToT0.x);

        Vector2 startMidMid = Vector2.Lerp(start, end, 0.5f);
        Vector2 startToMid = (end - start).normalized;
        Vector2 perpenStartMidMid = new Vector2(-startToMid.y, startToMid.x).normalized;

        Vector2 intersectP = Vector2.zero;
        bool isIntersect = Sweep_Line_Algorithm.Segment.StraightLineIntersection(start, start + perpenStartToP1, startMidMid, startMidMid + perpenStartMidMid, ref intersectP);
            
        if (isIntersect)
        {
            float radius = Vector2.Distance(start, intersectP);
            circleCenter = intersectP +(startMidMid - intersectP).normalized * radius;
            
            return true;
        }
        return false;
    }
    
    // p0p1, p2p3 직선이 교차하는 지점T의 장력을 구한다.
    private float Tension(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 intersectP = Vector2.zero;
        if (Sweep_Line_Algorithm.Segment.StraightLineIntersection(p0, p1, p2, p3, ref intersectP))
        {
            float percentageP0P1 = Vector2.Distance(p0, p1) / Vector2.Distance(p0, intersectP);
            float percentageP2P3 = Vector2.Distance(p2, p3) / Vector2.Distance(p3, intersectP);
            return (percentageP0P1 + percentageP2P3) / 2;
        }
        return 0;
    }
    
    private Vector2 LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Vector2 xdiff = new Vector2(a.x - b.x, c.x - d.x);
        Vector2 ydiff = new Vector2(a.y - b.y, c.y - d.y);

        float div = Det(xdiff, ydiff);
        if (div == 0)
            return Vector2.zero;

        d = new Vector2(Det(a, b), Det(c, d));
        float x = Det(d, xdiff) / div;
        float y = Det(d, ydiff) / div;
        return new Vector2(x, y);
    }
    
    private float Det(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
}