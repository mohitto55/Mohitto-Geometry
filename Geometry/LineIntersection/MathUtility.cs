using Swewep_Line_Algorithm;
using UnityEngine;

public static class MathUtility
{
    public static int Orientation(Vector3 vertex, Vector3 a, Vector3 b)
    {
        Vector3 v1 = a - vertex, v2 = b - vertex;
        float area = Cross2D(v1, v2);
        if (area > 0) return 1;
        if (area < 0) return -1;
        return 0;
    }

    // 유니티 3D에서는 바닥 평면이 X가 X고 Y가 Z다;
    public static float Cross2D(Vector3 v1, Vector3 v2) => v1.x * v2.y - v1.y * v2.x;
    // 왼쪽이면 양수 우측이면 음수
    public static float Cross2D(Point v1, Point v2) => v1.x * v2.y - v1.y * v2.x;
    
    public static bool FloatZero(float x) => Mathf.Abs(x) < 0.00005f;
}
