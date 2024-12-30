using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Swewep_Line_Algorithm
{
    public class DebugPoint
    {
        public Vector3 p;
    }

    public static class LS
    {
        private static SortedSet<Segment> FixDistances(Point p, SortedSet<Segment> T)
        {
            SegmentComparer comp = new SegmentComparer();
            comp.P = p;
            SortedSet<Segment> temp = new SortedSet<Segment>(comp);
            // temp���տ� T��Ҹ� ��� ����ִ´�.
            temp.UnionWith(T);
            return temp;
        }

        public static Segment NearestLeft(Segment s, SortedSet<Segment> T)
        {
            Segment previous = null;
            foreach (var segment in T)
            {
                if (s == segment) return previous;
                previous = segment;
            }

            return null;
        }

        public static Segment NearestRight(Segment s, SortedSet<Segment> T)
        {
            Segment previous = null;
            foreach (var segment in T.Reverse())
            {
                if (s == segment) return previous;
                previous = segment;
            }

            return null;
        }

        public static Segment NearestLeft(Point p, IEnumerable<Segment> T)
        {
            Segment minDistance = null;
            float min = float.MaxValue;
            foreach (var segment in T)
            {
                // ���а� �Ÿ��� ���� ���� ���̸鼭 �������� ������ �� ã��
                // �ٿ� ������ �Ʒ��� ���µ� �������� ���� ���� �ƴѰ�?
                if (min > Mathf.Abs(segment.Distance(p)) && segment.Distance(p) >= 0)
                {
                    min = Mathf.Abs(segment.Distance(p));
                    minDistance = segment;
                }
            }

            return minDistance;
        }

        public static Segment NearestRight(Point p, IEnumerable<Segment> T)
        {
            Segment minDistance = null;
            float min = float.MaxValue;
            foreach (var segment in T)
            {
                if (min > Mathf.Abs(segment.Distance(p)) && segment.Distance(p) <= 0)
                {
                    min = Mathf.Abs(segment.Distance(p));
                    minDistance = segment;
                }
            }

            return minDistance;
        }

        private static Point FindNewEvent(Segment sl, Segment sr, Point point,
            ref SortedDictionary<Point, EventPoint> sortedDictionary)
        {
            if (sl != null && sr != null)
            {
                Point intersection = sl.Intersection(sr);
                if (intersection != null && (intersection.y < point.y ||
                                             MathUtility.FloatZero(intersection.y - point.y) &&
                                             intersection.x > point.x))
                {
                    intersection.Label = "x" + Mathf.Min(sl.num, sr.num) + "" + Mathf.Max(sl.num, sr.num);
                    if (!sortedDictionary.ContainsKey(intersection))
                    {
                        sortedDictionary[intersection] = new EventPoint(sl);
                    }
                }
            }

            return null;
        }

        private static void DebugEventQueue(SortedDictionary<Point, EventPoint> Q)
        {
            Debug.Log(("Event Queue"));
            foreach (var e in Q)
            {
                Debug.Log(e.Key + " ");
            }
        }

        private static void DebugSweep(IEnumerable<Segment> T)
        {
            Debug.Log(("Sweep"));
            foreach (var segment in T)
            {
                Debug.Log(segment + " ");
            }
        }

        private static void DebugIntersection(Segment bRight, Segment bLeft, Point p)
        {
            Debug.Log("Testing Intersection  for " + bLeft + " " + bRight + " = " + p);
        }

                // ���������� �������� ���ϴ� ����
        // Plane�ȿ� �ִ� Segment���� �ִ´�.
        public static List<Point> SweepIntersections(List<Segment> segments, out Dictionary<Point, Segment> slTable, Action<List<Segment>, Point> intersctionCallback = null)
        {
            // �� �������̿��� ��� ����, �ܺ� �Ǻ��� ����Ѵ�.
            // �̺�Ʈ ����Ʈ�� ���� ���ʿ� �ִ� DCEL�� �����Ѵ�.
            // slTable
            slTable = new Dictionary<Point, Segment>();
            List<Point> intersect = new List<Point>();
// �̺�ƮQ
            // �̺�Ʈ ť���ص� ����� ������ �ߺ��� Event Point�� �ȳ������� BST�� �ؾ��Ѵ�.
            // Key Value�� Comparerer �Լ��� �������� ���Ľ�Ų��.

            // �ʱ�ȭ ����� �̷���
            // ���׸�Ʈ�� endpoint�� Q�� �����ϵ�, ��� ������ ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ���Ѵ�.
            SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
            
            
            foreach (var segment in segments)
            {
                // ���׸�Ʈ�� �������� Q�� �����ϵ�, **upper endpoint**�� ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ�� �Ѵ�.
                // ���� ���� ���� �ִ� ��� ��⿭�� ä�� ���� �ش� ��Ͽ� �߰��Ѵ�.
                if (Q.ContainsKey(segment.Start))
                {
                    Q[segment.Start].Segments.Add(segment);
                }
                // �׷��� �ʴٸ� �� Event�� �߰��Ѵ�.
                else
                {
                    Q.Add(segment.Start, new EventPoint(segment));
                }
                // ������ End�� Q�� ��ϵ��� �ʾҴٸ� End�� Key�� ��Ͻ�Ų��.
                if (!Q.ContainsKey(segment.End))
                {
                    Q.Add(segment.End, new EventPoint());
                }
            }
            
            // ���������Ʈ���� �����Ǿ��ְ� top retrieving(�˻�)�� �����ϴ�.
            // SortedSet�� ���� ���Ҵ� �߰����� �ʴ´�.
            // T == Status Struct
            SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer(true));
            
            while (Q.Count > 0)
            {
                Point p = Q.Keys.First();
                EventPoint e = Q[p];
                Q.Remove(p);
                // LowerPoint
                // s�� �����Ѵ�. 
                // sl�� sr�� sweep line �Ʒ����� �����ϴ� ��� T.���� s�� ���ʰ� ������ �̿��� �� ���� �������� Q�� �̺�Ʈ�� �����մϴ�
                List<Segment> lower = new List<Segment>();
                // Intercepting Point
                List<Segment> contain = new List<Segment>();

                // Start Event Handler
                // Status Structure ���θ� ��ȸ�� ���� �������ΰ� �´�� �ִ� Segment�� ã�Ƴ���

                foreach (var segment in T)
                {
                    // End == LowerPoint�� ��� ����Ʈ�� ��� ���߿� �����Ѵ�.
                    if (segment.End == p) lower.Add(segment);
                    // Intercepting Point == �������̰� StartPoint�� ��ġ�� �������
                    // �� ���� ���� ������ �����ϰ� �ִٴ� ���� �ǹ��ϸ� ��Ͽ� �����ϰ� �Ʒ����� Ȯ������ �ʴ´�.
                    // �ֳ��ϸ� ���� �� If������ �װ��� �������ֱ� �����̴�.
                    
                    // �����ϴ� s�� s'�� T���� �ٲ۴�.
                    // ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // �������� �����Ѵ�

                    else if (segment.PointIntercept(p) && segment.Start != p)
                    {
                        contain.Add(segment);
                    }
                }
                // ��ø�Ǵ� ���������� �����Ϸ��� �Ʒ��ɷ� �ٲٸ�ȴ�.
                if (contain.Count > 0)
                {
                    if (intersctionCallback != null)
                    {
                        List<Segment> IntersectSegments = new List<Segment>();
                        IntersectSegments.AddRange(contain);
                        intersctionCallback.Invoke(IntersectSegments, p);   
                    }
                    
                    intersect.Add(p);
                }
                // �� �Ʒ� �̺�Ʈ ����Ʈ��� Status���� �����Ѵ�.
                foreach (var segment in lower)
                {
                    T.Remove(segment);
                }

                // �������׸�Ʈ�� ��� �߰�
                contain.AddRange(e.Segments);
                foreach (var segment in e.Segments)
                {
                    T.Add(segment);
                }

                // �������� ���� ����
                T = FixDistances(p, T);
                
                if (contain.Count == 0)
                {
                    Segment sl = NearestLeft(p, T);
                    Segment sr = NearestRight(p, T);
                    Point i = FindNewEvent(sl, sr, p, ref Q);

                    if (sl != null)
                    {
                        if (!slTable.ContainsKey(p))
                        {
                            slTable.Add(p, sl);
                        }
                    }
                }
                else
                {

                    // - **Intersection point** : swap leaves and update information in search path
                    //     - �����ϴ� s�� s'�� T���� �ٲ۴�.
                    // - ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // - ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // - �������� �����Ѵ�
                    
                    // contain�� X���� �������� ���Ľ�Ų��.
                    contain.Sort((segment, segment1) =>
                    {
                        if (segment.SweepXValue(p) < segment1.SweepXValue(p)) return -1;
                        return 1;
                    });
                    
                    Segment SP = contain[0];
                    Segment SPP = contain[contain.Count - 1];
                    
                    Segment sl = NearestLeft(SP, T);
                    Segment sr = NearestRight(SPP, T);
                    
                    Point i = FindNewEvent(SP, sl, p, ref Q);
                    Point j = FindNewEvent(SPP, sr, p, ref Q);
                    
                    if (sl != null)
                    {
                        if (!slTable.ContainsKey(p))
                        {
                            slTable.Add(p, sl);
                        }
                    }
                }
            }
            return intersect;
        }
        // ���������� �������� ���ϴ� ����
        // Plane�ȿ� �ִ� Segment���� �ִ´�.
        public static IEnumerator SweepIntersections(List<Segment> segments, List<Point> intersect,
            DebugPoint debugPoint, float debugWaitSecond = 0.2f, bool debug = false)
        {
            WaitForSeconds delayWaitForSecond = new WaitForSeconds(debugWaitSecond);
            // �̺�ƮQ
            // �̺�Ʈ ť���ص� ����� ������ �ߺ��� Event Point�� �ȳ������� BST�� �ؾ��Ѵ�.
            // Key Value�� Comparerer �Լ��� �������� ���Ľ�Ų��.

            // �ʱ�ȭ ����� �̷���
            // ���׸�Ʈ�� endpoint�� Q�� �����ϵ�, ��� ������ ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ���Ѵ�.
            SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
            foreach (var segment in segments)
            {
                // ���׸�Ʈ�� �������� Q�� �����ϵ�, **upper endpoint**�� ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ�� �Ѵ�.
                // ���� ���� ���� �ִ� ��� ��⿭�� ä�� ���� �ش� ��Ͽ� �߰��Ѵ�.
                if (Q.ContainsKey(segment.Start))
                {
                    Q[segment.Start].Segments.Add(segment);
                }
                // �׷��� �ʴٸ� �� Event�� �߰��Ѵ�.
                else
                {
                    Q.Add(segment.Start, new EventPoint(segment));
                }
                // ������ End�� Q�� ��ϵ��� �ʾҴٸ� End�� Key�� ��Ͻ�Ų��.
                if (!Q.ContainsKey(segment.End))
                {
                    Q.Add(segment.End, new EventPoint());
                }
            }
            
            // ���������Ʈ���� �����Ǿ��ְ� top retrieving(�˻�)�� �����ϴ�.
            // SortedSet�� ���� ���Ҵ� �߰����� �ʴ´�.
            // T == Status Struct
            SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer(true));
            
            while (Q.Count > 0)
            {
                Point p = Q.Keys.First();
                EventPoint e = Q[p];
                Q.Remove(p);
                debugPoint.p = p.ToVector();
                // LowerPoint
                // s�� �����Ѵ�. 
                // sl�� sr�� sweep line �Ʒ����� �����ϴ� ��� T.���� s�� ���ʰ� ������ �̿��� �� ���� �������� Q�� �̺�Ʈ�� �����մϴ�
                List<Segment> lower = new List<Segment>();
                // Intercepting Point
                List<Segment> contain = new List<Segment>();

                // Start Event Handler
                // Status Structure ���θ� ��ȸ�� ���� �������ΰ� �´�� �ִ� Segment�� ã�Ƴ���

                foreach (var segment in T)
                {
                    // End == LowerPoint�� ��� ����Ʈ�� ��� ���߿� �����Ѵ�.
                    if (segment.End == p) lower.Add(segment);
                    // Intercepting Point == �������̰� StartPoint�� ��ġ�� �������
                    // �� ���� ���� ������ �����ϰ� �ִٴ� ���� �ǹ��ϸ� ��Ͽ� �����ϰ� �Ʒ����� Ȯ������ �ʴ´�.
                    // �ֳ��ϸ� ���� �� If������ �װ��� �������ֱ� �����̴�.
                    
                    // �����ϴ� s�� s'�� T���� �ٲ۴�.
                    // ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // �������� �����Ѵ�

                    else if (segment.PointIntercept(p) && segment.Start != p)
                    {
                        contain.Add(segment);
                    }
                }
                // ��ø�Ǵ� ���������� �����Ϸ��� �Ʒ��ɷ� �ٲٸ�ȴ�.
                if (e.Segments.Count + lower.Count + contain.Count > 1)
                {
                    intersect.Add(p);
                    debugPoint.p = p.ToVector();
                }
                // �� �Ʒ� �̺�Ʈ ����Ʈ��� Status���� �����Ѵ�.
                foreach (var segment in lower)
                {
                    T.Remove(segment);
                }

                // �������׸�Ʈ�� ��� �߰�
                contain.AddRange(e.Segments);
                foreach (var segment in e.Segments)
                {
                    T.Add(segment);
                }

                // �������� ���� ����
                T = FixDistances(p, T);
                
                if (contain.Count == 0)
                {
                    Segment sl = NearestLeft(p, T);
                    Segment sr = NearestRight(p, T);
                    Point i = FindNewEvent(sl, sr, p, ref Q);
                    if (debug)
                    {
                        Debug.Log("Event: " + p);
                        DebugEventQueue(Q);
                        DebugSweep(T);
                        //DebugIntersection(sl, sr, i);
                    }
                }
                else
                {

                    // - **Intersection point** : swap leaves and update information in search path
                    //     - �����ϴ� s�� s'�� T���� �ٲ۴�.
                    // - ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // - ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // - �������� �����Ѵ�
                    
                    // contain�� X���� �������� ���Ľ�Ų��.
                    contain.Sort((segment, segment1) =>
                    {
                        if (segment.SweepXValue(p) < segment1.SweepXValue(p)) return -1;
                        return 1;
                    });
                    
                    Segment SP = contain[0];
                    Segment SPP = contain[contain.Count - 1];
                    
                    Segment sl = NearestLeft(SP, T);
                    Segment sr = NearestRight(SPP, T);
                    
                    Point i = FindNewEvent(SP, sl, p, ref Q);
                    Point j = FindNewEvent(SPP, sr, p, ref Q);

                    if (debug)
                    {
                        Debug.Log("Event: " + p);
                        DebugEventQueue(Q);
                        DebugSweep(T);
                        DebugIntersection(SP, sl, i);
                        DebugIntersection(SPP, sr, j);
                    }
                }

                yield return delayWaitForSecond;
            }
            debugPoint.p.y = 0;
            //return intersect;
        }

        public static void SweepIntersections(List<Segment> segments, List<Point> intersect)
        {
            // �̺�ƮQ
            // �̺�Ʈ ť���ص� ����� ������ �ߺ��� Event Point�� �ȳ������� BST�� �ؾ��Ѵ�.
            // Key Value�� Comparerer �Լ��� �������� ���Ľ�Ų��.

            // �ʱ�ȭ ����� �̷���
            // ���׸�Ʈ�� endpoint�� Q�� �����ϵ�, ��� ������ ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ���Ѵ�.
            SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
            foreach (var segment in segments)
            {
                // ���׸�Ʈ�� �������� Q�� �����ϵ�, **upper endpoint**�� ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ�� �Ѵ�.
                // ���� ���� ���� �ִ� ��� ��⿭�� ä�� ���� �ش� ��Ͽ� �߰��Ѵ�.
                if (Q.ContainsKey(segment.Start)) Q[segment.Start].Segments.Add(segment);
                // �׷��� �ʴٸ� �� Event�� �߰��Ѵ�.
                else Q.Add(segment.Start, new EventPoint(segment));
                // ������ End�� Q�� ��ϵ��� �ʾҴٸ� End�� Key�� ��Ͻ�Ų��.
                if (!Q.ContainsKey(segment.End)) Q.Add(segment.End, new EventPoint());
            }

            // ���������Ʈ���� �����Ǿ��ְ� top retrieving(�˻�)�� �����ϴ�.
            // T == Status Struct
            SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer());
            while (Q.Count > 0)
            {
                Point p = Q.Keys.First();
                EventPoint e = Q[p];
                Q.Remove(p);

                // LowerPoint
                // s�� �����Ѵ�. 
                // sl�� sr�� sweep line �Ʒ����� �����ϴ� ��� T.���� s�� ���ʰ� ������ �̿��� �� ���� �������� Q�� �̺�Ʈ�� �����մϴ�
                List<Segment> lower = new List<Segment>();
                // Intercepting Point
                List<Segment> contain = new List<Segment>();

                // Start Event Handler
                // Status Structure ���θ� ��ȸ�� ���� �������ΰ� �´�� �ִ� Segment�� ã�Ƴ���
                foreach (var segment in T)
                {
                    // End == LowerPoint�� ��� ����Ʈ�� ��� ���߿� �����Ѵ�.
                    if (segment.End == p) lower.Add(segment);
                    // Intercepting Point == �������̰� StartPoint�� ��ġ�� �������
                    // �� ���� ���� ������ �����ϰ� �ִٴ� ���� �ǹ��ϸ� �̹� ��Ͽ� �����ϰ� �Ʒ����� Ȯ������ �ʴ´�.
                    // �ֳ��ϸ� �� If������ �װ��� �������ֱ� ??���̴�.
                    // �����ϴ� s�� s'�� T���� �ٲ۴�.
                    // ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // �������� �����Ѵ�
                    else if (segment.PointIntercept(p) && segment.Start != p) contain.Add(segment);
                }

                // �̺�Ʈ ����Ʈ�� ����Ű�� 
                if (e.Segments.Count + lower.Count + contain.Count > 1) intersect.Add(p);

                // �� �Ʒ� �̺�Ʈ ����Ʈ��� Status���� �����Ѵ�.
                foreach (var segment in lower)
                {
                    T.Remove(segment);
                }

                // �������׸�Ʈ�� ��� �߰�
                contain.AddRange(e.Segments);
                foreach (var segment in e.Segments)
                {
                    T.Add(segment);
                }

                // �������� ���� ����
                T = FixDistances(p, T);

                if (contain.Count == 0)
                {
                    Segment sl = NearestLeft(p, T);
                    Segment sr = NearestRight(p, T);
                    Point i = FindNewEvent(sl, sr, p, ref Q);
                }
                else
                {
                    // contain�� X���� �������� ���Ľ�Ų��.
                    contain.Sort((segment, segment1) =>
                    {
                        if (segment.SweepXValue(p) < segment1.SweepXValue(p)) return -1;
                        return 1;
                    });

                    // - **Intersection point** : swap leaves and update information in search path
                    //     - �����ϴ� s�� s'�� T���� �ٲ۴�.
                    // - ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // - ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
                    // - �������� �����Ѵ�
                    Segment SP = contain[0];
                    Segment SPP = contain[contain.Count - 1];

                    Segment sl = NearestLeft(SP, T);
                    Segment sr = NearestRight(SPP, T);

                    Point i = FindNewEvent(SP, sl, p, ref Q);
                    Point j = FindNewEvent(SPP, sr, p, ref Q);
                }
                
            }
        }


        //         // ���������� �������� ���ϴ� ����
        // // Plane�ȿ� �ִ� Segment���� �ִ´�.
        // public static IEnumerator SweepIntersections(List<Segment> segments, List<Point> intersect, DebugPoint debugPoint, bool debug = false)
        // {
        //     // �̺�ƮQ
        //     // �̺�Ʈ ť���ص� ����� ������ �ߺ��� Event Point�� �ȳ������� BST�� �ؾ��Ѵ�.
        //     // Ket Value�� Comparerer �Լ��� �������� ���Ľ�Ų��.
        //     
        //     // �ʱ�ȭ ����� �̷���
        //     // ���׸�Ʈ�� endpoint�� Q�� �����ϵ�, ��� ������ ���Ե� ���� �ش� ���׸�Ʈ�� �Բ� ����Ǿ���Ѵ�.
        //     SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
        //     foreach (var segment in segments)
        //     {
        //         // ���� ���� ���� �ִ� ��� ��⿭�� ä�� ���� �ش� ��Ͽ� �߰��Ѵ�.
        //         if(Q.ContainsKey(segment.Start)) Q[segment.Start].Segments.Add(segment);
        //         // �׷��� �ʴٸ� �� Event�� �߰��Ѵ�.
        //         else Q.Add(segment.Start, new EventPoint(segment));
        //         // ������ End�� Q�� ��ϵ��� �ʾҴٸ� End�� Key�� ��Ͻ�Ų��.
        //         if(!Q.ContainsKey(segment.End)) Q.Add(segment.End, new EventPoint());
        //     }
        //
        //     // ���������Ʈ���� �����Ǿ��ְ� top retrieving(�˻�)�� �����ϴ�.
        //     // T == Status Struct
        //     SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer());
        //     Debug.Log("===================");
        //     while (Q.Count > 0)
        //     {
        //         Debug.Log("Last Q : " + Q.Count);
        //         Point p = Q.Keys.First();
        //         EventPoint e = Q[p];
        //         Q.Remove(p);
        //
        //         // LowerPoint
        //         // s�� �����Ѵ�. 
        //         // sl�� sr�� sweep line �Ʒ����� �����ϴ� ��� T.���� s�� ���ʰ� ������ �̿��� �� ���� �������� Q�� �̺�Ʈ�� �����մϴ�
        //         List<Segment> lower = new List<Segment>();
        //         // Intercepting Point
        //         List<Segment> contain = new List<Segment>();
        //
        //         // EventHandle ����
        //         // ���������� �ϸ鼭 ���׸�Ʈ�� � ���� �ִ��� Ȯ���Ѵ�.
        //         foreach (var segment in T)
        //         {
        //             // End == LowerPoint�� ��� ����Ʈ�� ��� ���߿� �����Ѵ�.
        //             if(segment.End == p) lower.Add(segment);
        //             // Intercepting Point == �������̰� StartPoint�� ��ġ�� �������
        //             // �� ���� ���� ������ �����ϰ� �ִٴ� ���� �ǹ��ϸ� �̹� ��Ͽ� �����ϰ� �Ʒ����� Ȯ������ �ʴ´�.
        //             // �ֳ��ϸ� �� If������ �װ��� �������ֱ� �����̴�.
        //             // �����ϴ� s�� s'�� T���� �ٲ۴�.
        //             // ���� s'�� �� ���� �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
        //             // ���� s'�� �� ������ �̿��� �������� �Ʒ��� �����Ѵٸ� �������� Q�� �ִ´�.
        //             // �������� �����Ѵ�
        //             else if(segment.PointIntercept(p) && segment.Start != p) contain.Add(segment);
        //         }
        //
        //         if (e.Segments.Count + lower.Count + contain.Count > 1) intersect.Add(p);
        //         
        //         // �� �Ʒ� �̺�Ʈ ����Ʈ��� Status���� �����Ѵ�.
        //         foreach(var segment in lower)
        //         {
        //             T.Remove(segment);
        //         }
        //
        //         // �������׸�Ʈ�� ��� �߰�
        //         contain.AddRange(e.Segments);
        //         foreach (var segment in e.Segments)
        //         {
        //             T.Add(segment);
        //         }
        //
        //         // �������� ���� ����
        //         yield return new WaitForSeconds(1);
        //         T = FixDistances(p, T);
        //
        //         if (contain.Count == 0)
        //         {
        //             Debug.Log("contain.Count == 0");
        //             Segment sl = NearestLeft(p, T);
        //             Segment sr = NearestRight(p, T);
        //             Point i = FindNewEvent(sl, sr, p, ref Q);
        //             if (debug)
        //             {
        //                 Debug.Log("Event: " + p);
        //                 DebugEventQueue(Q);
        //                 DebugSweep(T);
        //                 DebugIntersection(sl, sr, i);
        //             }
        //         }
        //         else
        //         {
        //             Debug.Log("contain.Count > 0");
        //             // contain ������ 1�̻��̶�� X���� �������� ���Ľ�Ų��.
        //             contain.Sort((segment, segment1) =>
        //             {
        //                 if (segment.SweepXValue(p) < segment1.SweepXValue(p)) return -1;
        //                 return 1;
        //             });
        //
        //             Segment SP = contain[0];
        //             Segment SPP = contain[contain.Count - 1];
        //
        //             Segment sl = NearestLeft(SP, T);
        //             Segment sr = NearestRight(SPP, T);
        //
        //             Point i = FindNewEvent(SP, sl, p, ref Q);
        //             Point j = FindNewEvent(SPP, sr, p, ref Q);
        //             if (debug)
        //             {
        //                 Debug.Log("Event: " + p);
        //                 DebugEventQueue(Q);
        //                 DebugSweep(T);
        //                 DebugIntersection(SP, sl, i);
        //                 DebugIntersection(SPP, sr, j);
        //             }
        //         }
        //     }
        //     //return intersect;
        // }
    }
}