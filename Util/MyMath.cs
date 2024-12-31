using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MyMath
{

    public static float KindaSmallNumber = 0.00005f;
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
    
    public static float UnsignedAngle(Vector2 to, Vector2 from)
    {
        return Vector2.Angle(to, from);
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
    
    public static bool FloatZero(float x) => Mathf.Abs(x) < KindaSmallNumber;

    /// <summary>
    /// a�� b���� ������ -1 ũ�� 1 ������ 0�� ��ȯ�մϴ�
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int CompareFloat(float a, float b)
    {
        if (FloatZero(a - b)) return 0;
        if (a > b) return 1;
        if (a < b) return -1;
        return 0;
    }
    
    /// <summary>
    /// 1 == �ݽð�
    /// 0 == ������
    /// -1 == �ð�
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    public static float CCW(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float ret = Cross2D(p2 - p1, p3 - p2);
        if (ret > 0) return 1; //�ݽð� ����
        else if (MathUtility.FloatZero(ret)) return 0; //��������
        else return -1; //�ð� ����
    }
    
    /// <summary>
    /// ccw > 0 �ݽð� ����
    /// ccw == 0 ����
    /// ccw < 0 �ð����
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    public static float CCWValue(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return Cross2D(p2 - p1, p3 - p2);
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
    
    // �Լ�: �ٰ����� ������ Ȯ���ϰ� �ʿ��� ��� �ݴ�� ����
    public static List<Vector2> EnsureClockwise(List<Vector2> points)
    {
        if (IsCounterClockwise(points))
        {
            points.Reverse(); // �ݽð� �����̸� �ð� �������� �ݴ�� ����
        }

        return points;
    }
    /// <summary>
    /// �ݽð� �������� ������ ����
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static List<Vector2> EnsureCounterClockwise(List<Vector2> points)
    {
        if (!IsCounterClockwise(points))
        {
            points.Reverse(); // �ð� �����̸� �ݽð� �������� ����
        }
        return points;
    }

    // �Լ�: �ٰ����� ������ Ȯ�� (�ݽð� �����̸� true, �ð� �����̸� false ��ȯ)
    // https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
    // ������ ��ʿ� ����� ���������Դϴ�. (�׷����� �Խ��� ����� �����ϴ�.) ���� �Ʒ��� ������ ��� ����(y2+y1)/2�� ���� ����(x2-x1)�� ���� ���� �����ϴ�. x�� ��ȣ ��Ģ�� �ָ��ϼ���. �ﰢ������ �õ��غ��� �� ��� �۵��ϴ��� �� �� ���� �̴ϴ�. 
    // ����� ���: �� �亯�� �Ϲ����� ��ī��Ʈ ��ǥ�踦 �����մϴ�. ����� ���� ������ HTML5 ĵ������ ���� �Ϻ� �Ϲ����� ���ؽ�Ʈ�� �� Y���� ����ϱ� �����Դϴ�. �׷� ���� ��Ģ�� ������� �մϴ�. ������ �����̸� � �� �ð� �����Դϴ�
    
    /// <summary>
    /// �ٰ����� ������ Ȯ�� (�ݽð� �����̸� true, �ð� �����̸� false ��ȯ)
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static bool IsCounterClockwise(List<Vector2> points)
    {
        float area = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];
            area += (p2.x - p1.x) * (p2.y + p1.y);
        }
        return area < 0;
    }
    
    // https://kipl.tistory.com/284
    // https://stackoverflow.com/questions/12446766/order-concave-polygon-vertices-counterclockwise
    private class VertexCompair : IComparer<Vector2>
    {
        public int Compare(Vector2 a, Vector2 b)
        {
            if (a.x == b.x)
            {
                if (a.y < b.y)
                    return -1;
                else
                    return 0;
            }
            if (a.x < b.x)
                return -1;
            else
                return 0;
        }
    }
    public static List<Vector2> MakeSimplePolygon(List<Vector2> pts) {
        if (pts.Count < 1) return new List<Vector2>(); //null_vector;
        int rightId = 0, leftId = 0;
        for (int i = pts.Count; i-->0;) {
            Debug.LogWarning(i);
            if (pts[i].x > pts[rightId].x) rightId = i;
            if (pts[i].x < pts[leftId].x) leftId = i;
        }
        List<Vector2> LP = new List<Vector2>(), RP = new List<Vector2>();
        for (int i = 0; i < pts.Count; i++) {
            if (i == rightId || i == leftId) continue;
            // ���ؼ��� ����(LP)/������(RP)�� ���� �� �и�;
            if (MyMath.CCW(pts[leftId], pts[rightId], pts[i]) >= 0) LP.Add(pts[i]); 
            else RP.Add(pts[i]);
        }
        if (LP.Count > 0) LP.Sort(new VertexCompair());
        if (RP.Count > 0) RP.Sort(new VertexCompair());

        List<Vector2> spoly = new List<Vector2>();
        spoly.Add(pts[leftId]);
        for (int i = 0; i < LP.Count(); i++) spoly.Add(LP[i]);
        spoly.Add(pts[rightId]);
        for (int i = RP.Count(); i-->0;) spoly.Add(RP[i]);
        //spoly = clockwise simple polygon;
        return spoly;
    }
}
