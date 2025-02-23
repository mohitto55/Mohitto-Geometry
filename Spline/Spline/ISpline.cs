using UnityEngine;

public interface ISpline
{
    /// <summary>
    /// ������ �Ÿ� �������� ��� ������ Point���� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="spacing"></param>
    /// <param name="resolution"></param>
    /// <returns></returns>
    public Vector2[] CalculateEvenlySpacedPoints(float spacing = 0.01f, float resolution = 1);

    /// <summary>
    /// 0~1������ ��ġ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector2 GetPoint(float t);
    /// <summary>
    /// Spline�� 0~t ���̸� ��ȯ�մϴ�.
    /// </summary>
    /// <returns></returns>
    public float GetLength(float t);

    /// <summary>
    /// Spline�� Ư�� ������ ���̸� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector2 GetDerivative(float t);

    public float[] GetRoots();

    public SplinePath Offset(float distance);
}
