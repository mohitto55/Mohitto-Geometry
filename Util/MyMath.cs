using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MyMath
{
    public static float SignedAngle(Vector2 to, Vector2 from)
    {
        float angle = Vector2.Angle(to, from);
        // ����
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
    // ����Ƽ 3D������ �ٴ� ����� X�� X�� Y�� Z��;
    public static float Cross2D(Vector3 v1, Vector3 v2) => v1.x * v2.y - v1.y * v2.x;
    // �����̸� ��� �����̸� ����
    
    public static bool FloatZero(float x) => Mathf.Abs(x) < 0.00005f;

    public static float CCW(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float ret = Cross2D(p2 - p1, p3 - p2);
        if (ret > 0) return 1; //�ݽð� ����
        else if (MathUtility.FloatZero(ret)) return 0; //��������
        else return -1; //�ð� ����
    }
    
    public static bool LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 x)  
    {  
        // �и�  
        float det = Vector3.Cross((Vector3)(b - a), (Vector3)(d - c)).z;  
        // �� ������ ������ �����  
        if (Mathf.Abs(det) < 0.00005f) return false;  
        x = a + (b - a) * (Cross2D(c - a, d - c) / det);  
        return true;  
    }  
}
