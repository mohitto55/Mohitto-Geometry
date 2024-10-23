using System.Collections.Generic;
using UnityEngine;

namespace Swewep_Line_Algorithm
{
    public class EventPoint
    {
        public List<Segment> Segments = new List<Segment>();
        public EventPoint(Segment s) => this.Segments.Add(s);

        public EventPoint()
        {
            
        }
    }

    public class PointComparer : IComparer<Point>
    {
        public int Compare(Point x, Point y)
        {
            // 동일한곳에 있으면 0
            if(x==y) return 0;
            // X가 Y보다 왼쪽위에 있으면 -1
            if (x.y > y.y || MathUtility.FloatZero(x.y - y.y) && x.x < y.x) return -1;
            // 그외는 1
            return 1;
        }
    }

    public class SegmentComparer : IComparer<Segment>
    {
        public Point P { get; set; }
        public int Compare(Segment x, Segment y)
        {
            if (P is null) return 0;
            if (x == y) return 0;
            if (x.SweepXValue(P) < y.SweepXValue(P)) return -1;
            return 1;
        }
    }
}
