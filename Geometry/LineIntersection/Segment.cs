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
            // y�� ���� �ִ� ���� Start
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
            // y�� ���� �ִ� ���� Start
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

        // ���а� �������� �Ÿ��� ����Ѵ�.
        public float Distance(Point p)
        {
            // ������ �� ������ ����纯���� ���̰� �����µ� �ű⿡ �غ��� ������ ���̰� ������ �� ���̰� ������ �Ÿ���.
            Point p1 = End - Start, p2 = p - Start;
            return MathUtility.Cross2D(p1, p2) / p1.Magnitude();
        }

        public bool IsIntersectingSegments(Segment s, bool includeEndPoints = true)
        {
            Point p1 = End - Start, p2 = s.End - s.Start;
            float rxs = MathUtility.Cross2D(p1, p2);
            // �� ������ �����ϸ� false��
            if (MathUtility.FloatZero(rxs)) return false;
            
            // �� ������ �������� �Ϲ� ���������� ǥ���ϱ� ���� ���ͷ� ǥ���ϰ� �������������� Ǯ�� �������� ������ ǥ���� �� �ִ�.
            // a_x + ub_x = c_x + td_x
            // a_y + ub_y = c_y + td_y
            // �� ���� u�� t�����ؼ� Ǯ�� �������� ǥ���� ���� �ְ�
            
            // a + ub = c + td
            // (a + ub) X b = (c + td) X b : X�� ����
            // ���� b�� d�� �����ؼ� u�� b �ϳ��� ���ְ� ������ �ϳ��� ���ؼ� �����ؼ� Ǯ���� �ִ�.
            float u = MathUtility.Cross2D(this.Start - s.Start, p1) / -rxs;
            float t = MathUtility.Cross2D(s.Start - this.Start, p2) / rxs;

            // u�� t�� ������ ���� ���ο��� Lerp Alpha���̴�.
            if (includeEndPoints) return u >= 0 && u <=  1 && t >= 0 && t <= 1;
            // �������� ���� �ɷ��ִ��� üũ�Ѵ�.
            else return u > 0 && u < 1 && t > 0 && t < 1;
        }

        public bool PointIntercept(Point p)
        {
            Point direction = End - Start, result = p - Start;
            // �� ���� �� y ������ ���̿� �ִٴ� ���̴�.
            if (MathUtility.FloatZero(result.x) && MathUtility.FloatZero(direction.x) && direction.y > p.y ||
               MathUtility.FloatZero(result.y) && MathUtility.FloatZero(direction.y) && direction.y > p.y)
                return true;

            return MathUtility.FloatZero(result.x / direction.x - result.y / direction.y);
        }
        
        // �������� ���ϴ� �Լ�
        public Point Intersection(Segment s)
        {
            if (s is null || !IsIntersectingSegments(s)) return null;
            // ������ �ִٸ� �ᱹ �� ���� ���̿� �ִٴ� ��
            // �׷��� �������� t(alpha) ���� ���ϴ� ������ �Ἥ �������� ��ȯ���ش�.
            Point p1 = End - Start, p2 = s.End - s.Start;
            float rxs = Cross(p1, p2);
            float t = Cross(s.Start - Start, p2) / rxs;
            return Start + new Point(t * p1.x, t * p1.y);
        }

        // ���� ���������� �̵��Ѵ�.
        // ���� y������ X value�� ��ȯ�Ѵ�.
        // ��� ���׸�Ʈ�� �ٸ� ���׸�Ʈ �տ� �ִ��� �����ϴ� ����� �ʿ��ϴ�
        // ����� �� ���� ���� ������ �� �� ������ ��ǻ�ʹ� �𸣱� ������ �̷� ó���� ������Ѵ�.
        public float SweepXValue(Point p, float belowLine = 0.0005f)
        {
            float y = p.y - belowLine;
            Point direction = End - Start;
            
            // y ���� ������ x ���� ��ȯ (���� ó��)
            if (MathUtility.FloatZero(direction.y))
            {
                // ���򼱿����� x ���� y�� �����ϰ� ����
                return p.x; 
            }
            // y���� �������� p�� ��ġ�� ��� �������� Ȯ���Ѵ�.
            var t = (y - Start.y) / direction.y;
            // y������ ���ؼ� p�� x ��ġ�� �����ش�.
            return t * direction.x + Start.x;
        }
        
        // �� ���׸�Ʈ�� ������ ��ġ�� true �ƴϸ� false
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

        // �� a, b�� ������ ������ �� c, d�� ������ ������ ������ x�� ��ȯ�Ѵ�.
        // �� ������ �����ϸ� false, �ƴϸ� true�� ��ȯ�Ѵ�.
        public bool lineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 x)
        {
            // �θ�
            float det = Vector3.Cross((Vector3)(b - a), (Vector3)(d - c)).z;
            // �� ������ ������ �����
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
        
        

        // ������ �������� b�� a�� ����(�ݽð�)�̸� ���, ������(�ð�)�̸� ����, �����̸� 0�� ��ȯ�Ѵ�.
        float ccw(Vector2 a, Vector2 b)
        {
            return Vector3.Cross(a, b).z;
        }

        // p, a, b �� ������ p�� �������� b�� a�� ����(�ݽð�)�̸� ���, ������(�ð�)�̸� ����, �����̸� 0�� ��ȯ�Ѵ�.
        float ccw(Vector2 p, Vector2 a, Vector2 b)
        {
            return ccw(a - p, b - p);
        }

        // ���� a, b ���� c, d�� �����ϸ� �� ������ ��ġ���� Ȯ���Ѵ�.
        bool paralleSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p)
        {
            if (b.x < a.x && b.y < a.y) Swap(ref a, ref b);
            if (d.x < c.x && d.y < c.y) Swap(ref c, ref d);

            // �� �������� ���ų� �� ������ ��ġ�� �ʴ� ��츦 �켱 �ɷ�����.
            if (ccw(a, b, c) != 0 || (b.x < c.x && b.y < c.y) || (d.x < a.x && d.y < a.y)) return false;
            
            // �� ������ ��ģ�ٸ� ������ �ϳ��� ã�´�.
            if (a.x < c.x && a.y < c.y) p = c;
            else p = a;
            return true;
        }

        // - p�� �� �� a, b�� ���θ鼭 �� ���� x, y�࿡ ������ �ּһ簢�� ���ο� �ִ��� Ȯ���Ѵ�.
        // a, b, p�� ������ �� �ִٰ� �����Ѵ�.
        bool inBoundingRectangle(Vector2 p, Vector2 a, Vector2 b)
        {
            if (b.x < a.x && b.y < a.y) Swap(ref a, ref b);
            return p == a || p == b || ((a.x < p.x && a.y < p.y) && (p.x < b.x && p.y < b.y));
        }
        
        // - �� �� a, b�� ������ ���а� �� �� c, b�� ������ ������ p�� ��ȯ�Ѵ�.
        // - ������ �������� ��� �ƹ����̳� ��ȯ�Ѵ�.
        bool segmentIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p){    
            //�� ������ ������ ��츦 �켱 ���ܷ� ó���Ѵ�.
            if(!lineIntersection(a, b, c, d, ref p))        
                return paralleSegments(a, b, c, d, ref p);    
            //p�� �� ���п� ���ԵǾ� �ִ� ��쿡�� ���� ��ȯ�Ѵ�.
            return inBoundingRectangle(p, a, b) && inBoundingRectangle(p, c, d);
        }

        void Swap(ref Vector2 a, ref Vector2 b)
        {
            // tuple ����ȯ
            (b, a) = (a, b);
        }
        
    }
}
