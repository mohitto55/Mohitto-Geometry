using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Sweep_Line_Algorithm
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
            if(x==y)return 0;
            // X가 Y보다 왼쪽위에 있으면 -1
            if (x.y > y.y || MathUtility.FloatZero(x.y - y.y) && x.x < y.x) return -1;
            // 그외는 1
            return 1;
        }
    }

    public class SegmentComparer : IComparer<Segment>
    {
        public Point P;
        public bool allowOverlap = false;

        public SegmentComparer()
        {
            
        }

        public SegmentComparer(bool allowOverlap)
        {
            this.allowOverlap = allowOverlap;
        }
        public int Compare(Segment x, Segment y)
        {
            if (x == y)
            {
                return allowOverlap ? -1 : 0;
            }

            if (P is null)
            {
                int compareValue = MyMath.CompareFloat(x.Start.x, y.Start.x);
                return allowOverlap ? (compareValue == 0 ? -1 : compareValue) : compareValue;
            }

            if (x.SweepXValue(P) < y.SweepXValue(P))
            {
                return -1;
            }
            return 1;
        }
    }
}
