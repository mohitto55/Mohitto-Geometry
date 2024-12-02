using System;
using UnityEngine;

namespace Swewep_Line_Algorithm
{
    public class Segment
    {
        public Point Start;
        public Point End;

        public readonly string ID;
        public readonly int num;

        public Segment(Point start, Point end, int id = -1)
        { 
            // y가 위에 있는 것이 Start
            if (start.y > end.y || MathUtility.FloatZero(start.y - end.y) && start.x < end.x)
            {
                this.Start = start;
                this.End = end;
            }
            else
            {
                this.Start = end;
                this.End = start;
            }

            if (id > -1)
            {
                this.Start.Label = "a" + id;
                this.End.Label = "b" + id;
                this.ID = "S" + id;
                this.num = id;
            }
        }
        
        public Segment(Vector3 vec1, Vector3 vec2, int id = -1)
        {
            Point start = new Point(vec1.x, vec1.y);
            Point end = new Point(vec2.x, vec2.y);
            // y가 위에 있는 것이 Start
            if (start.y > end.y || MathUtility.FloatZero(start.y - end.y) && start.x < end.x)
            {
                this.Start = start;
                this.End = end;
            }
            else
            {
                this.Start = end;
                this.End = start;
            }
            if (id > -1)
            {
                this.Start.Label = "a" + id;
                this.End.Label = "b" + id;
                this.ID = "S" + id;
                this.num = id;
            }
        }

        public Segment(float x1, float y1, float x2, float y2, int id = -1)
        {
            Point start = new Point(x1, y1), end = new Point(x2, y2);
            if (start.y > end.y || MathUtility.FloatZero(start.y - end.y) && start.x < end.x)
            {
                this.Start = start;
                this.End = end;
            }
            else
            {
                this.Start = end;
                this.End = start;
            }

            if (id > -1)
            {
                this.Start.Label = "a" + id;
                this.End.Label = "b" + id;
                this.ID = "S" + id;
                this.num = id;
            }
        }

        // 선분과 점사이의 거리를 계산한다.
        public float Distance(Point p)
        {
            // 외적은 두 선분의 평행사변형의 넓이가 나오는데 거기에 밑변을 나누면 높이가 나오고 그 높이가 점과의 거리다.
            Point p1 = End - Start, p2 = p - Start;
            return MathUtility.Cross2D(p1, p2) / p1.Magnitude();
        }

        public bool IsIntersectingSegments(Segment s, bool includeEndPoints = true)
        {
            Point p1 = End - Start, p2 = s.End - s.Start;
            float rxs = MathUtility.Cross2D(p1, p2);
            // 두 선분이 평행하면 false다
            if (MathUtility.FloatZero(rxs)) return false;
            
            // 두 선분의 교차점은 일반 방정식으로 표현하기 보단 벡터로 표현하고 연립방정식으로 풀어 외적으로 간결히 표현할 수 있다.
            // a_x + ub_x = c_x + td_x
            // a_y + ub_y = c_y + td_y
            // 이 식을 u나 t에대해서 풀고 외적으로 표현할 수도 있고
            
            // a + ub = c + td
            // (a + ub) X b = (c + td) X b : X는 외적
            // 각각 b나 d를 외적해서 u나 b 하나를 없애고 나머지 하나에 대해서 연립해서 풀수도 있다.
            float u = MathUtility.Cross2D(this.Start - s.Start, p1) / -rxs;
            float t = MathUtility.Cross2D(s.Start - this.Start, p2) / rxs;

            // u랑 t는 각각의 선분 내부에서 Lerp Alpha값이다.
            if (includeEndPoints) return u >= 0 && u <=  1 && t >= 0 && t <= 1;
            // 교차점이 끝에 걸려있는지 체크한다.
            else return u > 0 && u < 1 && t > 0 && t < 1;
        }

        public bool PointIntercept(Point p)
        {
            Point direction = End - Start, result = p - Start;
            // 이 뜻은 두 y 벡터의 사이에 있다는 뜻이다.
            if (MathUtility.FloatZero(result.x) && MathUtility.FloatZero(direction.x) && direction.y > p.y ||
               MathUtility.FloatZero(result.y) && MathUtility.FloatZero(direction.y) && direction.y > p.y)
                return true;

            return MathUtility.FloatZero(result.x / direction.x - result.y / direction.y);
        }
        
        // 교차점을 구하는 함수
        public Point Intersection(Segment s)
        {
            if (s is null || !IsIntersectingSegments(s)) return null;
            // 겹쳐져 있다면 결국 두 선분 사이에 있다는 뜻
            // 그러니 교차점의 t(alpha) 값을 구하는 공식을 써서 교차점을 반환해준다.
            Point p1 = End - Start, p2 = s.End - s.Start;
            float rxs = Cross(p1, p2);
            float t = Cross(s.Start - Start, p2) / rxs;
            return Start + new Point(t * p1.x, t * p1.y);
        }

        // 점점 스윕라인이 이동한다.
        // 같은 y레벨의 X value를 반환한다.
        // 어느 세그먼트가 다른 세그먼트 앞에 있는지 결정하는 방법이 필요하다
        // 사람은 딱 보면 뭐가 앞인지 알 수 있지만 컴퓨터는 모르기 때문에 이런 처리를 해줘야한다.
        public float SweepXValue(Point p, float belowLine = 0.0005f)
        {
            float y = p.y - belowLine;
            Point direction = End - Start;
            
            // y 값이 같으면 x 값만 반환 (수평선 처리)
            if (MathUtility.FloatZero(direction.y))
            {
                // 수평선에서는 x 값은 y와 무관하게 일정
                return p.x; 
            }
            // y축을 기준으로 p의 위치가 어느 비율인지 확인한다.
            var t = (y - Start.y) / direction.y;
            // y비율을 통해서 p의 x 위치도 구해준다.
            return t * direction.x + Start.x;
        }
        
        // 두 세그먼트가 완젼히 겹치면 true 아니면 false
        public static bool operator ==(Segment t, Segment seg)
        {
            if (t is null && seg is null) return true;
            if (t is null || seg is null) return false;
            return t.Start == seg.Start && t.End == seg.End;
        }

        public static bool operator !=(Segment t, Segment seg)
        {
            return !(t == seg);
        }

        public override string ToString()
        {
            return ID + " : [" + Start.ToString() + " -> " + End.ToString() + "]";
        }

        // 점 a, b를 지나는 직선과 점 c, d를 지나는 직선의 교차점 x를 반환한다.
        // 두 직선이 평행하면 false, 아니면 true를 반환한다.
        public bool lineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 x)
        {
            // 부모
            float det = Vector3.Cross((Vector3)(b - a), (Vector3)(d - c)).z;
            // 두 직선이 평행한 경우라면
            if (Mathf.Abs(det) < 0.00005f) return false;
            x = a + (b - a) * (Cross(c - a, d - c) / det);
            return true;
        }

        public float Cross(Vector2 a, Vector2 b)
        {
            return Vector3.Cross((Vector3)a, (Vector3)b).z;
        }
        
        public float Cross(Point a, Point b)
        {
            return Vector3.Cross(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0)).z;
        }
        
        

        // 원점을 기준으로 b가 a의 왼쪽(반시계)이면 양수, 오른쪽(시계)이면 음수, 평행이면 0을 반환한다.
        float ccw(Vector2 a, Vector2 b)
        {
            return Vector3.Cross(a, b).z;
        }

        // p, a, b 가 있을때 p를 기준으로 b가 a의 왼쪽(반시계)이면 양수, 오른쪽(시계)이면 음수, 평행이면 0을 반환한다.
        float ccw(Vector2 p, Vector2 a, Vector2 b)
        {
            return ccw(a - p, b - p);
        }

        // 선분 a, b 선분 c, d가 평행하면 한 점에서 겹치는지 확인한다.
        bool paralleSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p)
        {
            if (b.x < a.x && b.y < a.y) Swap(ref a, ref b);
            if (d.x < c.x && d.y < c.y) Swap(ref c, ref d);

            // 한 직선위에 없거나 두 선분이 겹치지 않는 경우를 우선 걸러낸다.
            if (ccw(a, b, c) != 0 || (b.x < c.x && b.y < c.y) || (d.x < a.x && d.y < a.y)) return false;
            
            // 두 선분이 겹친다면 교차점 하나를 찾는다.
            if (a.x < c.x && a.y < c.y) p = c;
            else p = a;
            return true;
        }

        // - p가 두 점 a, b를 감싸면서 각 변이 x, y축에 평행한 최소사각형 내부에 있는지 확인한다.
        // a, b, p는 일직선 상에 있다고 가정한다.
        bool inBoundingRectangle(Vector2 p, Vector2 a, Vector2 b)
        {
            if (b.x < a.x && b.y < a.y) Swap(ref a, ref b);
            return p == a || p == b || ((a.x < p.x && a.y < p.y) && (p.x < b.x && p.y < b.y));
        }
        
        // - 두 점 a, b를 지나는 선분과 두 점 c, b를 지나는 선분을 p에 반환한다.
        // - 교점이 여러개일 경우 아무점이나 반환한다.
        bool segmentIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p){    
            //두 직선이 평행인 경우를 우선 예외로 처리한다.
            if(!lineIntersection(a, b, c, d, ref p))        
                return paralleSegments(a, b, c, d, ref p);    
            //p가 두 선분에 포함되어 있는 경우에만 참을 반환한다.
            return inBoundingRectangle(p, a, b) && inBoundingRectangle(p, c, d);
        }

        void Swap(ref Vector2 a, ref Vector2 b)
        {
            // tuple 값교환
            (b, a) = (a, b);
        }
        
    }
}
