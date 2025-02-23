using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public abstract partial class SplinePathBase : ISpline
{
    public bool EditLine = false;
    
    public float NodeSize = 0.5f;

    public abstract void AddPoint(Vector2 point);
    public abstract int ControlPointsCounts { get; }
    public abstract int SegmentCount { get; }
    public abstract Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1);

    /// <summary>
    /// 커브를 구성하는 포인트들의 위치 비율을 0~1로 변환해 반환합니다.
    /// </summary>
    /// <returns></returns>
    public abstract float[] GetPointRatiosInCurve();
    
    public abstract float GetLength(float t);
    public abstract Vector2 GetDerivative(float t);
    
    public float[] GetRoots()
    {
        List<float> roots = new List<float>();
        float[] pointRatios = GetPointRatiosInCurve();
        for (int i = 0; i < SegmentCount; i++)
        {
            float start = pointRatios[i];
            float end = pointRatios[i+1];
            
            Spline segment = GetSegment(i);
            float[] segmentRoots = segment.GetRoots();
            foreach (var root in segmentRoots)
            {
                if (0 <= root && root <= 1)
                {
                    roots.Add(Mathf.Lerp(start, end, root));
                }
            }
        }
        return roots.ToArray();
    }

    public SplinePath Offset(float distance)
    {
        SplinePath offsetPaths = new SplinePath();
        // 내부 Spline들을 Offset 합니다.
        for (int i = 0; i < SegmentCount; i++)
        {
            SplinePath offsetPath = GetSegment(i).Offset(distance);
            offsetPaths.AddPath(offsetPath);
        }
        
        /// 원본 Curve들과 거리비교를 해서 너무 가까운 Segment는 삭제합니다.
        float minDistance = Mathf.Abs(distance);
        Spline startPClosestSpline = null;
        for (int i = 0; i < offsetPaths.SegmentCount; i++)
        {
            Spline offsetSegment = offsetPaths.GetSegment(i);
            Vector2 endP = offsetSegment.EndPoint;
            Vector2 startP = offsetSegment.StartPoint;
            
            bool startPClosest = false;
            bool endPClosest = false;
            for (int k = 0; k < SegmentCount; k++)
            {
                Spline originalSegment = GetSegment(k);
                float startDst = Vector2.Distance(SplineUtility.PointProjection(originalSegment, startP).p, startP);
                float endDst = Vector2.Distance(SplineUtility.PointProjection(originalSegment, endP).p, endP);
                startDst += 0.00005f;
                endDst += 0.00005f;
   
                if(endDst < minDistance)
                {
                    endPClosest = true;
                }
                if (startDst < minDistance)
                {
                    startPClosest = true;
                }
                if (startPClosest && endPClosest)
                {
                    break;
                }
            }

            if (startPClosest && endPClosest)
            {
                offsetPaths.RemoveSegment(i);
                i -= 1;
            }
            else if (endPClosest)
            {
                startPClosestSpline = offsetSegment;
            }
            else if(startPClosestSpline != null && startPClosest)
            {

                Vector2 interV = Vector2.zero;
                if (Sweep_Line_Algorithm.Segment.StraightLineIntersection(startPClosestSpline.StartPoint, startPClosestSpline.EndPoint,
                        offsetSegment.StartPoint, offsetSegment.EndPoint, ref interV))
                {
                    Gizmos.color = Color.magenta;

                    MyGizmos.DrawWireCircle(startPClosestSpline.StartPoint, 0.1f);
                    MyGizmos.DrawWireCircle(startPClosestSpline.EndPoint, 0.1f);
                    Gizmos.color = Color.yellow;

                    MyGizmos.DrawWireCircle(offsetSegment.StartPoint, 0.1f);
                    MyGizmos.DrawWireCircle(offsetSegment.EndPoint, 0.1f);
                    
                    SplinePoint point = SplineUtility.PointProjection(startPClosestSpline, interV);
                    startPClosestSpline.ConvertCubicBezierToSpline(startPClosestSpline.Split(point.t)[0].GetCubicBezier());
                    startPClosestSpline.EndPoint = interV;
                        
                    point = SplineUtility.PointProjection(offsetSegment, interV);
                    offsetSegment.ConvertCubicBezierToSpline(offsetSegment.Split(point.t)[1].GetCubicBezier());
                    offsetSegment.StartPoint = interV;
                    
                    MyGizmos.DrawWireCircle(interV, 0.2f);
                    startPClosestSpline = null;
                }
                else
                {
                    offsetPaths.RemoveSegment(i);
                    i -= 1;
                }
                
            }
        }
        return offsetPaths;
    }

    public abstract Vector2 GetPoint(float t);
    public abstract void Split(float ratio);

    public abstract void Split(int segmentIndex, float t);
    
    public abstract Spline GetSegment(int i);
    public SplinePathBase(){}
}

[System.Serializable]
public abstract class SplinePathBase<T> : SplinePathBase where T : Spline
{
    
    [SerializeReference, SerializeField] protected List<T> path = new List<T>();
    public SplinePathBase(){}

    public SplinePathBase(T segment)
    {
        path.Add(segment);
    }
    public SplinePathBase(SplinePathBase<T> other)
    {
        path = new List<T>(other.path);
    }
    
    public override int ControlPointsCounts
    {
        get
        {
            int count = 0;
            for (int i = 0; i < path.Count; i++)
            {
                count += path[i].ControlPointsCount;
            }

            return count;
        }
    }

    public override int SegmentCount => path.Count;
    
    public override Vector2 GetPoint(float t)
    {
        (int, float) indexRatio = GetSegmentIndexFromRatio(t);
        int SegmentIndex = indexRatio.Item1;
        return GetSegment(SegmentIndex).GetPoint(indexRatio.Item2);
    }

    public override Spline GetSegment(int i) => path[i];
    
    public override Vector2 GetDerivative(float t)
    {
        (int, float) indexRatio = GetSegmentIndexFromRatio(t);
        int SegmentIndex = indexRatio.Item1;
        return GetSegment(SegmentIndex).GetDerivative(indexRatio.Item2);
    }

    public override Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < path.Count; i++)
        {
            points.AddRange(path[i].CalculateEvenlySpacedPoints(spacing, resolution));
        }
        return points.ToArray();
    }
    
    /// <summary>
    /// 커브를 구성하는 포인트들의 위치 비율을 0~1로 변환해 반환합니다.
    /// </summary>
    /// <returns></returns>
    public override float[] GetPointRatiosInCurve()
    {
        if (SegmentCount == 0)
            return new float[0];
        
        // 0번쨰 포인트 비율은 0
        float[] pointsRatio = new float[SegmentCount + 1];
        float totalLength = 0;
        for (int segmentIndex = 0; segmentIndex < SegmentCount; segmentIndex++)
        {
            Spline segment = GetSegment(segmentIndex);
            float curveLength =  segment.GetLength(1);
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

    public override float GetLength(float t)
    {
        float totalLength = 0;
        foreach (var path in path)
        {
            totalLength += path.GetLength(t);
        }
        return totalLength;
    }
    
    public void DrawOnGizmo(bool drawLerpHermiteLine = true, bool drawPoint = true)
    {
        for (int i = 0; i < SegmentCount; i++)
        {
            Spline segment = GetSegment(i);
            segment.DrawOnGizmo(drawLerpHermiteLine, drawPoint);
        }
    }
    
    public (int, float) GetSegmentIndexFromRatio(float ratio)
    {
        float[] pointRatios = GetPointRatiosInCurve();

        for (int i = 1; i < pointRatios.Length; i++)
        {
            if (ratio <= pointRatios[i])
            {
                float a = pointRatios[i] - pointRatios[i - 1];
                float b = ratio - pointRatios[i - 1];
                return (i - 1, b / a);
            }
        }

        return (0, ratio);
    }

    /// <summary>
    /// ratio 비율을 기준으로 Spline Path를 이등분 합니다.
    /// </summary>
    /// <param name="ratio"></param>
    public override void Split(float ratio)
    {
        (int, float) segmentValue = GetSegmentIndexFromRatio(ratio);
        int segmentIndex = segmentValue.Item1;
        float segmentRatio = segmentValue.Item2;
        if(SegmentCount > 0)
            Split(segmentIndex, segmentRatio);
    }

    public override void Split(int segmentIndex, float t)
    {
        Spline segment = GetSegment(segmentIndex);
        Spline[] splitSplines = segment.Split(t);
        
        Spline startToMid = splitSplines[0];
        Spline midToEnd = splitSplines[1];
        path[segmentIndex] = (T)startToMid;
        path.Insert(segmentIndex+1, (T)midToEnd);
    }

    public void RemoveSegment(int index)
    {
        Spline segment = GetSegment(index);
        path.RemoveAt(index);

    }

    public void AddPath(SplinePathBase<T> otherPath)
    {
        path.AddRange(otherPath.path);
    }
}

public class SplinePath : SplinePathBase<Spline>
{
    public SplinePath()
    {
    }
    public SplinePath(Spline segment) :base(segment)
    {
        
    }
    public override void AddPoint(Vector2 point)
    {
    }
}