using UnityEngine;

public interface ISpline
{
    /// <summary>
    /// 고정된 거리 간격으로 곡선을 지나는 Point들을 반환한다.
    /// </summary>
    /// <param name="spacing"></param>
    /// <param name="resolution"></param>
    /// <returns></returns>
    public Vector2[] CalculateEvenlySpacedPoints(float spacing = 0.01f, float resolution = 1);

    /// <summary>
    /// 0~1사이의 위치를 반환합니다.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector2 GetPoint(float t);
    /// <summary>
    /// Spline의 0~t 길이를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public float GetLength(float t);

    /// <summary>
    /// Spline의 특정 지점의 길이를 반환합니다.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector2 GetDerivative(float t);

    public float[] GetRoots();

    public SplinePath Offset(float distance);
}
