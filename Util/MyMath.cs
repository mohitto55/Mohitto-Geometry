using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MyMath
{

    public static float KindaSmallNumber = 0.00005f;
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
    // 유니티 3D에서는 바닥 평면이 X가 X고 Y가 Z다;
    public static float Cross2D(Vector3 v1, Vector3 v2) => v1.x * v2.y - v1.y * v2.x;
    // 왼쪽이면 양수 우측이면 음수
    
    public static bool FloatZero(float x) => Mathf.Abs(x) < KindaSmallNumber;

    /// <summary>
    /// a가 b보다 작으면 -1 크면 1 같으면 0을 반환합니다
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
    /// 1 == 반시계
    /// 0 == 일직선
    /// -1 == 시계
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    public static float CCW(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float ret = Cross2D(p2 - p1, p3 - p2);
        if (ret > 0) return 1; //반시계 방향
        else if (MathUtility.FloatZero(ret)) return 0; //일직선상
        else return -1; //시계 방향
    }
    
    /// <summary>
    /// ccw > 0 반시계 방향
    /// ccw == 0 직선
    /// ccw < 0 시계방향
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
        // 분모  
        float det = Vector3.Cross((Vector3)(b - a), (Vector3)(d - c)).z;  
        // 두 직선이 평행한 경우라면  
        if (Mathf.Abs(det) < 0.00005f) return false;  
        x = a + (b - a) * (Cross2D(c - a, d - c) / det);  
        return true;  
    }  
    
    // 함수: 다각형의 방향을 확인하고 필요한 경우 반대로 정렬
    public static List<Vector2> EnsureClockwise(List<Vector2> points)
    {
        if (IsCounterClockwise(points))
        {
            points.Reverse(); // 반시계 방향이면 시계 방향으로 반대로 정렬
        }

        return points;
    }
    /// <summary>
    /// 반시계 방향으로 정점들 정렬
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static List<Vector2> EnsureCounterClockwise(List<Vector2> points)
    {
        if (!IsCounterClockwise(points))
        {
            points.Reverse(); // 시계 방향이면 반시계 방향으로 정렬
        }
        return points;
    }

    // 함수: 다각형의 방향을 확인 (반시계 방향이면 true, 시계 방향이면 false 반환)
    // https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
    // 간단한 사례에 적용된 미적분학입니다. (그래픽을 게시할 기술이 없습니다.) 선분 아래의 면적은 평균 높이(y2+y1)/2에 가로 길이(x2-x1)를 곱한 값과 같습니다. x의 부호 규칙을 주목하세요. 삼각형으로 시도해보면 곧 어떻게 작동하는지 알 수 있을 겁니다. 
    // 사소한 경고: 이 답변은 일반적인 데카르트 좌표계를 가정합니다. 언급할 만한 이유는 HTML5 캔버스와 같은 일부 일반적인 컨텍스트가 역 Y축을 사용하기 때문입니다. 그런 다음 규칙을 뒤집어야 합니다. 면적이 음수이면 곡선 은 시계 방향입니다
    
    /// <summary>
    /// 다각형의 방향을 확인 (반시계 방향이면 true, 시계 방향이면 false 반환)
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
            // 기준선의 왼편(LP)/오른편(RP)에 놓인 점 분리;
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
