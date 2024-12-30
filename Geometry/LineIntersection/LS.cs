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
            // temp집합에 T요소를 모두 집어넣는다.
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
                // 선분과 거리가 가장 작은 점이면서 음수여서 왼쪽인 것 찾기
                // 근에 위에서 아래로 가는데 왼쪽으로 가면 음수 아닌가?
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

                // 스윕라인의 교차점을 구하는 문제
        // Plane안에 있는 Segment들을 넣는다.
        public static List<Point> SweepIntersections(List<Segment> segments, out Dictionary<Point, Segment> slTable, Action<List<Segment>, Point> intersctionCallback = null)
        {
            // 맵 오버레이에서 사용 내부, 외부 판별시 사용한다.
            // 이벤트 포인트의 가장 왼쪽에 있는 DCEL을 저장한다.
            // slTable
            slTable = new Dictionary<Point, Segment>();
            List<Point> intersect = new List<Point>();
// 이벤트Q
            // 이벤트 큐로해도 상관은 없지만 중복된 Event Point를 안넣으려면 BST로 해야한다.
            // Key Value를 Comparerer 함수를 기준으로 정렬시킨다.

            // 초기화 방법은 이렇다
            // 세그먼트의 endpoint를 Q에 삽입하되, 상단 끝점이 삽입될 때는 해당 세그먼트가 함께 저장되어야한다.
            SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
            
            
            foreach (var segment in segments)
            {
                // 세그먼트의 끝점들을 Q에 삽입하되, **upper endpoint**가 삽입될 때는 해당 세그먼트가 함께 저장되어야 한다.
                // 기존 시작 값이 있는 경우 대기열을 채운 다음 해당 목록에 추가한다.
                if (Q.ContainsKey(segment.Start))
                {
                    Q[segment.Start].Segments.Add(segment);
                }
                // 그렇지 않다면 새 Event를 추가한다.
                else
                {
                    Q.Add(segment.Start, new EventPoint(segment));
                }
                // 선분의 End가 Q에 등록되지 않았다면 End도 Key에 등록시킨다.
                if (!Q.ContainsKey(segment.End))
                {
                    Q.Add(segment.End, new EventPoint());
                }
            }
            
            // 레드블랙이진트리로 구현되어있고 top retrieving(검색)이 가능하다.
            // SortedSet은 같은 원소는 추가하지 않는다.
            // T == Status Struct
            SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer(true));
            
            while (Q.Count > 0)
            {
                Point p = Q.Keys.First();
                EventPoint e = Q[p];
                Q.Remove(p);
                // LowerPoint
                // s를 삭제한다. 
                // sl과 sr이 sweep line 아래에서 교차하는 경우 T.에서 s의 왼쪽과 오른쪽 이웃이 된 다음 교차점을 Q의 이벤트로 삽입합니다
                List<Segment> lower = new List<Segment>();
                // Intercepting Point
                List<Segment> contain = new List<Segment>();

                // Start Event Handler
                // Status Structure 내부를 순회해 현재 스윕라인과 맞닿고 있는 Segment를 찾아낸다

                foreach (var segment in T)
                {
                    // End == LowerPoint일 경우 리스트에 담고 나중에 삭제한다.
                    if (segment.End == p) lower.Add(segment);
                    // Intercepting Point == 교차점이고 StartPoint와 겹치지 않을경우
                    // 이 뜻은 위쪽 끝에서 교차하고 있다는 것을 의미하며 목록에 포함하고 아래쪽을 확인하지 않는다.
                    // 왜냐하면 여기 위 If문에서 그것을 검증해주기 때문이다.
                    
                    // 교차하는 s와 s'를 T에서 바꾼다.
                    // 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // 교차점을 저장한다

                    else if (segment.PointIntercept(p) && segment.Start != p)
                    {
                        contain.Add(segment);
                    }
                }
                // 중첩되는 시작점까지 포함하려면 아래걸로 바꾸면된다.
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
                // 맨 아래 이벤트 포인트라면 Status에서 삭제한다.
                foreach (var segment in lower)
                {
                    T.Remove(segment);
                }

                // 상위세그먼트는 모두 추가
                contain.AddRange(e.Segments);
                foreach (var segment in e.Segments)
                {
                    T.Add(segment);
                }

                // 스윕라인 순서 수정
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
                    //     - 교차하는 s와 s'를 T에서 바꾼다.
                    // - 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // - 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // - 교차점을 저장한다
                    
                    // contain을 X값을 기준으로 정렬시킨다.
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
        // 스윕라인의 교차점을 구하는 문제
        // Plane안에 있는 Segment들을 넣는다.
        public static IEnumerator SweepIntersections(List<Segment> segments, List<Point> intersect,
            DebugPoint debugPoint, float debugWaitSecond = 0.2f, bool debug = false)
        {
            WaitForSeconds delayWaitForSecond = new WaitForSeconds(debugWaitSecond);
            // 이벤트Q
            // 이벤트 큐로해도 상관은 없지만 중복된 Event Point를 안넣으려면 BST로 해야한다.
            // Key Value를 Comparerer 함수를 기준으로 정렬시킨다.

            // 초기화 방법은 이렇다
            // 세그먼트의 endpoint를 Q에 삽입하되, 상단 끝점이 삽입될 때는 해당 세그먼트가 함께 저장되어야한다.
            SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
            foreach (var segment in segments)
            {
                // 세그먼트의 끝점들을 Q에 삽입하되, **upper endpoint**가 삽입될 때는 해당 세그먼트가 함께 저장되어야 한다.
                // 기존 시작 값이 있는 경우 대기열을 채운 다음 해당 목록에 추가한다.
                if (Q.ContainsKey(segment.Start))
                {
                    Q[segment.Start].Segments.Add(segment);
                }
                // 그렇지 않다면 새 Event를 추가한다.
                else
                {
                    Q.Add(segment.Start, new EventPoint(segment));
                }
                // 선분의 End가 Q에 등록되지 않았다면 End도 Key에 등록시킨다.
                if (!Q.ContainsKey(segment.End))
                {
                    Q.Add(segment.End, new EventPoint());
                }
            }
            
            // 레드블랙이진트리로 구현되어있고 top retrieving(검색)이 가능하다.
            // SortedSet은 같은 원소는 추가하지 않는다.
            // T == Status Struct
            SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer(true));
            
            while (Q.Count > 0)
            {
                Point p = Q.Keys.First();
                EventPoint e = Q[p];
                Q.Remove(p);
                debugPoint.p = p.ToVector();
                // LowerPoint
                // s를 삭제한다. 
                // sl과 sr이 sweep line 아래에서 교차하는 경우 T.에서 s의 왼쪽과 오른쪽 이웃이 된 다음 교차점을 Q의 이벤트로 삽입합니다
                List<Segment> lower = new List<Segment>();
                // Intercepting Point
                List<Segment> contain = new List<Segment>();

                // Start Event Handler
                // Status Structure 내부를 순회해 현재 스윕라인과 맞닿고 있는 Segment를 찾아낸다

                foreach (var segment in T)
                {
                    // End == LowerPoint일 경우 리스트에 담고 나중에 삭제한다.
                    if (segment.End == p) lower.Add(segment);
                    // Intercepting Point == 교차점이고 StartPoint와 겹치지 않을경우
                    // 이 뜻은 위쪽 끝에서 교차하고 있다는 것을 의미하며 목록에 포함하고 아래쪽을 확인하지 않는다.
                    // 왜냐하면 여기 위 If문에서 그것을 검증해주기 때문이다.
                    
                    // 교차하는 s와 s'를 T에서 바꾼다.
                    // 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // 교차점을 저장한다

                    else if (segment.PointIntercept(p) && segment.Start != p)
                    {
                        contain.Add(segment);
                    }
                }
                // 중첩되는 시작점까지 포함하려면 아래걸로 바꾸면된다.
                if (e.Segments.Count + lower.Count + contain.Count > 1)
                {
                    intersect.Add(p);
                    debugPoint.p = p.ToVector();
                }
                // 맨 아래 이벤트 포인트라면 Status에서 삭제한다.
                foreach (var segment in lower)
                {
                    T.Remove(segment);
                }

                // 상위세그먼트는 모두 추가
                contain.AddRange(e.Segments);
                foreach (var segment in e.Segments)
                {
                    T.Add(segment);
                }

                // 스윕라인 순서 수정
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
                    //     - 교차하는 s와 s'를 T에서 바꾼다.
                    // - 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // - 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // - 교차점을 저장한다
                    
                    // contain을 X값을 기준으로 정렬시킨다.
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
            // 이벤트Q
            // 이벤트 큐로해도 상관은 없지만 중복된 Event Point를 안넣으려면 BST로 해야한다.
            // Key Value를 Comparerer 함수를 기준으로 정렬시킨다.

            // 초기화 방법은 이렇다
            // 세그먼트의 endpoint를 Q에 삽입하되, 상단 끝점이 삽입될 때는 해당 세그먼트가 함께 저장되어야한다.
            SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
            foreach (var segment in segments)
            {
                // 세그먼트의 끝점들을 Q에 삽입하되, **upper endpoint**가 삽입될 때는 해당 세그먼트가 함께 저장되어야 한다.
                // 기존 시작 값이 있는 경우 대기열을 채운 다음 해당 목록에 추가한다.
                if (Q.ContainsKey(segment.Start)) Q[segment.Start].Segments.Add(segment);
                // 그렇지 않다면 새 Event를 추가한다.
                else Q.Add(segment.Start, new EventPoint(segment));
                // 선분의 End가 Q에 등록되지 않았다면 End도 Key에 등록시킨다.
                if (!Q.ContainsKey(segment.End)) Q.Add(segment.End, new EventPoint());
            }

            // 레드블랙이진트리로 구현되어있고 top retrieving(검색)이 가능하다.
            // T == Status Struct
            SortedSet<Segment> T = new SortedSet<Segment>(new SegmentComparer());
            while (Q.Count > 0)
            {
                Point p = Q.Keys.First();
                EventPoint e = Q[p];
                Q.Remove(p);

                // LowerPoint
                // s를 삭제한다. 
                // sl과 sr이 sweep line 아래에서 교차하는 경우 T.에서 s의 왼쪽과 오른쪽 이웃이 된 다음 교차점을 Q의 이벤트로 삽입합니다
                List<Segment> lower = new List<Segment>();
                // Intercepting Point
                List<Segment> contain = new List<Segment>();

                // Start Event Handler
                // Status Structure 내부를 순회해 현재 스윕라인과 맞닿고 있는 Segment를 찾아낸다
                foreach (var segment in T)
                {
                    // End == LowerPoint일 경우 리스트에 담고 나중에 삭제한다.
                    if (segment.End == p) lower.Add(segment);
                    // Intercepting Point == 교차점이고 StartPoint와 겹치지 않을경우
                    // 이 뜻은 위쪽 끝에서 교차하고 있다는 것을 의미하며 이미 목록에 포함하고 아래쪽을 확인하지 않는다.
                    // 왜냐하면 위 If문에서 그것을 검증해주기 ??문이다.
                    // 교차하는 s와 s'를 T에서 바꾼다.
                    // 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // 교차점을 저장한다
                    else if (segment.PointIntercept(p) && segment.Start != p) contain.Add(segment);
                }

                // 이벤트 포인트가 가리키는 
                if (e.Segments.Count + lower.Count + contain.Count > 1) intersect.Add(p);

                // 맨 아래 이벤트 포인트라면 Status에서 삭제한다.
                foreach (var segment in lower)
                {
                    T.Remove(segment);
                }

                // 상위세그먼트는 모두 추가
                contain.AddRange(e.Segments);
                foreach (var segment in e.Segments)
                {
                    T.Add(segment);
                }

                // 스윕라인 순서 수정
                T = FixDistances(p, T);

                if (contain.Count == 0)
                {
                    Segment sl = NearestLeft(p, T);
                    Segment sr = NearestRight(p, T);
                    Point i = FindNewEvent(sl, sr, p, ref Q);
                }
                else
                {
                    // contain을 X값을 기준으로 정렬시킨다.
                    contain.Sort((segment, segment1) =>
                    {
                        if (segment.SweepXValue(p) < segment1.SweepXValue(p)) return -1;
                        return 1;
                    });

                    // - **Intersection point** : swap leaves and update information in search path
                    //     - 교차하는 s와 s'를 T에서 바꾼다.
                    // - 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // - 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
                    // - 교차점을 저장한다
                    Segment SP = contain[0];
                    Segment SPP = contain[contain.Count - 1];

                    Segment sl = NearestLeft(SP, T);
                    Segment sr = NearestRight(SPP, T);

                    Point i = FindNewEvent(SP, sl, p, ref Q);
                    Point j = FindNewEvent(SPP, sr, p, ref Q);
                }
                
            }
        }


        //         // 스윕라인의 교차점을 구하는 문제
        // // Plane안에 있는 Segment들을 넣는다.
        // public static IEnumerator SweepIntersections(List<Segment> segments, List<Point> intersect, DebugPoint debugPoint, bool debug = false)
        // {
        //     // 이벤트Q
        //     // 이벤트 큐로해도 상관은 없지만 중복된 Event Point를 안넣으려면 BST로 해야한다.
        //     // Ket Value를 Comparerer 함수를 기준으로 정렬시킨다.
        //     
        //     // 초기화 방법은 이렇다
        //     // 세그먼트의 endpoint를 Q에 삽입하되, 상단 끝점이 삽입될 때는 해당 세그먼트가 함께 저장되어야한다.
        //     SortedDictionary<Point, EventPoint> Q = new SortedDictionary<Point, EventPoint>(new PointComparer());
        //     foreach (var segment in segments)
        //     {
        //         // 기존 시작 값이 있는 경우 대기열을 채운 다음 해당 목록에 추가한다.
        //         if(Q.ContainsKey(segment.Start)) Q[segment.Start].Segments.Add(segment);
        //         // 그렇지 않다면 새 Event를 추가한다.
        //         else Q.Add(segment.Start, new EventPoint(segment));
        //         // 선분의 End가 Q에 등록되지 않았다면 End도 Key에 등록시킨다.
        //         if(!Q.ContainsKey(segment.End)) Q.Add(segment.End, new EventPoint());
        //     }
        //
        //     // 레드블랙이진트리로 구현되어있고 top retrieving(검색)이 가능하다.
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
        //         // s를 삭제한다. 
        //         // sl과 sr이 sweep line 아래에서 교차하는 경우 T.에서 s의 왼쪽과 오른쪽 이웃이 된 다음 교차점을 Q의 이벤트로 삽입합니다
        //         List<Segment> lower = new List<Segment>();
        //         // Intercepting Point
        //         List<Segment> contain = new List<Segment>();
        //
        //         // EventHandle 시작
        //         // 스윕라인을 하면서 세그먼트에 어떤 값이 있는지 확인한다.
        //         foreach (var segment in T)
        //         {
        //             // End == LowerPoint일 경우 리스트에 담고 나중에 삭제한다.
        //             if(segment.End == p) lower.Add(segment);
        //             // Intercepting Point == 교차점이고 StartPoint와 겹치지 않을경우
        //             // 이 뜻은 위쪽 끝에서 교차하고 있다는 것을 의미하며 이미 목록에 포함하고 아래쪽을 확인하지 않는다.
        //             // 왜냐하면 위 If문에서 그것을 검증해주기 떄문이다.
        //             // 교차하는 s와 s'를 T에서 바꾼다.
        //             // 만약 s'의 새 왼쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
        //             // 만약 s'의 새 오른쪽 이웃이 스윕라인 아래서 교차한다면 교차점을 Q에 넣는다.
        //             // 교차점을 저장한다
        //             else if(segment.PointIntercept(p) && segment.Start != p) contain.Add(segment);
        //         }
        //
        //         if (e.Segments.Count + lower.Count + contain.Count > 1) intersect.Add(p);
        //         
        //         // 맨 아래 이벤트 포인트라면 Status에서 삭제한다.
        //         foreach(var segment in lower)
        //         {
        //             T.Remove(segment);
        //         }
        //
        //         // 상위세그먼트는 모두 추가
        //         contain.AddRange(e.Segments);
        //         foreach (var segment in e.Segments)
        //         {
        //             T.Add(segment);
        //         }
        //
        //         // 스윕라인 순서 수정
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
        //             // contain 갯수가 1이상이라면 X값을 기준으로 정렬시킨다.
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