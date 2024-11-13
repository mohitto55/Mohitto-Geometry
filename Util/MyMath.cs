using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MyMath
{
    public static float SignedAngle(Vector2 to, Vector2 from)
    {
        float angle = Vector2.Angle(to, from);
        // 외적
        if (to.x * from.y - to.y * from.x > 0)
        {
            return (float)(-angle);
        }
        return angle;
    }

    public static List<Vector2> GetVector2ListFromTransform(List<Transform> list)
    {
        return list.Select(t => (Vector2)t.position).ToList();
    }
    
    public static List<Vector3> GetVector3ListFromTransform(List<Transform> list)
    {
        return list.Select(t => (Vector3)t.position).ToList();
    }
    // 유니티 3D에서는 바닥 평면이 X가 X고 Y가 Z다;
    public static float Cross2D(Vector3 v1, Vector3 v2) => v1.x * v2.y - v1.y * v2.x;
    // 왼쪽이면 양수 우측이면 음수
    
    public static bool FloatZero(float x) => Mathf.Abs(x) < 0.00005f;

    public static float CCW(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float ret = Cross2D(p2 - p1, p3 - p2);
        if (ret > 0) return 1; //반시계 방향
        else if (MathUtility.FloatZero(ret)) return 0; //일직선상
        else return -1; //시계 방향
    }
    
    public static bool LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 x)  
    {  
        // 분모  
        float det = Vector3.Cross((Vector3)(b - a), (Vector3)(d - c)).z;  
        // 두 직선이 평행한 경우라면  
        if (Mathf.Abs(det) < 0.00005f) return false;  
        x = a + (b - a) * (Cross2D(c - a, d - c) / det);  
        return true;  
    }  
}
