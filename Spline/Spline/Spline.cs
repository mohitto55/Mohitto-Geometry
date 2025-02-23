using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CustomCollision;
using Geometry;
using Sweep_Line_Algorithm;
using Unity.VisualScripting;
using UnityEngine;

public struct SplinePoint
{
    /// <summary>
    /// Spline���� ������ Ư�� ����
    /// </summary>
    public Vector2 p;
    /// <summary>
    /// Spline ���� ������ ������ ����
    /// </summary>
    public float t;
}

[System.Serializable]
public abstract partial class Spline : ISpline, ISliceSpline
{
    
    public Spline()
    {
        InitAction();
    }
    
    public Spline(Vector2 startPoint, Vector2 endPoint)
    {
        InitAction();
    }
    
    [SerializeReference]
    protected Action onSplineModified;
    protected bool spacedPointsFlag = true;
    
    public abstract Vector2 StartPoint { get; set; }
    public abstract Vector2 EndPoint { get; set; }
    /// <summary>
    /// Handle�� �����ϰ� Spline�� ������ ����Ʈ��
    /// </summary>
    public abstract Vector2[] Points { get; }
    public abstract Vector2[] Handles { get; }
    /// <summary>
    /// Point�� Handle�� ��� ������ Spine�� �����ϴ� ���� ������ �Դϴ�.
    /// </summary>
    public abstract Vector2[] Segment { get; }
    
    protected Vector2[] evenlySpacedPoints;
    

    /// <summary>
    /// ������ �Ÿ� �������� ��� ������ Point���� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="spacing"></param>
    /// <param name="resolution"></param>
    /// <returns></returns>
    public Vector2[] CalculateEvenlySpacedPoints(float spacing = 0.01f, float resolution = 1)
    {
        // if (spacedPointsFlag)
        // {
        //     evenlySpacedPoints = OnCalculateEvenlySpacedPoints(spacing, resolution);
        // }
        evenlySpacedPoints = OnCalculateEvenlySpacedPoints(spacing, resolution);
        spacedPointsFlag = false;
        return evenlySpacedPoints;
    }

    protected abstract Vector2[] OnCalculateEvenlySpacedPoints(float spacing, float resolution = 1);


    public abstract Vector2 GetPoint(float t);

    public virtual float GetLength(float t)
    {
        float z = t * 0.5f;
        float sum = 0.0f;

        for (int i = 0; i < 24; ++i)
        {
            float derivativeT = z * (float)SplineUtility.TTable[i] + z;
            Vector2 derivative = GetDerivative(derivativeT);
            sum += (float)SplineUtility.CTable[i] * Mathf.Sqrt(derivative.x * derivative.x + derivative.y * derivative.y);
        }
        return z * sum;
    }

    public abstract Vector2 GetDerivative(float t);
    public abstract float[] GetRoots();

    /// <summary>
    /// t���� Spline�� 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Spline[] Split(float t)
    {
        Spline[] splines = OnSplit(t);
        
        if (onSplineModified != null)
        {
            onSplineModified();
        }
        else
        {
            InitAction();
        }
        return splines;
    }
    protected abstract Spline[] OnSplit(float t);

    public void MovePoint(int i, Vector2 pos)
    {
        OnMovePoint(i, pos);
        if (onSplineModified != null)
        {
            onSplineModified();
        }
        else
        {
            InitAction();
        }
    }
    protected abstract void OnMovePoint(int i, Vector2 pos);
    /// <summary>
    /// Point, Handle�� ��� ������ �����Դϴ�.
    /// </summary>
    public int ControlPointsCount
    {
        get => Points.Length + Handles.Length;
    }

    public abstract void DrawOnGizmo(bool drawLerpHermiteLine = true, bool drawPoint = true);

    
    public AABB2D GetBoundingBox()
    {
        float[] roots = GetRoots();
        List<Vector2> points = new List<Vector2>();
        points.Add(StartPoint);
        points.Add(EndPoint);

        foreach (var root in roots)
        {
            points.Add(GetPoint(root));
        }
        return new AABB2D(points);
    }
    public abstract CubicBezier GetCubicBezier();
    public abstract void ConvertCubicBezierToSpline(CubicBezier bezier);
    public abstract Spline GetCopySpline();
    
    private void InitAction()
    {
        onSplineModified = () => spacedPointsFlag = true;
    }
}
