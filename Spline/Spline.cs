using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Spline
{
    /// <summary>
    /// 고정된 거리 간격으로 곡선을 지나는 Point들을 반환한다.
    /// </summary>
    /// <param name="spacing"></param>
    /// <param name="resolution"></param>
    /// <returns></returns>
    public abstract Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1);
    public abstract Vector2 this[int i] { get; }
    public abstract int NumPoints { get; }
    public abstract int NumSegments { get; }
    public abstract void AddPoint(Vector2 anchorPos);
    public abstract Vector2[] GetSegment(int i);
    public abstract void MovePoint(int i, Vector2 pos);
    public abstract void DeletePoint(int anchorIndex);
}
