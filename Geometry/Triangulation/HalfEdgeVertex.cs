using System;
using UnityEngine;

namespace Geometry.DCEL
{
    public class HalfEdgeVertex
    {
        public Vector2 Coordinate;
        public HalfEdge IncidentEdge;
        
        public HalfEdgeVertex(Vector2 v)
        {
            Coordinate = v;
        }

        // bool CheckParallel(HalfEdgeVertex v1, HalfEdgeVertex v2)
        // {
        //     if (MyMath.FloatZero(Mathf.Abs(v1.Coordinate.y - this.Coordinate.y)))
        //     {
        //         // 양쪽이 모두 수평이라면?
        //         if(MyMath.FloatZero(Mathf.Abs(v2.Coordinate.y - this.Coordinate.y)))
        //         {
        //             this.type = Vtype.REGULAR;
        //             return true;
        //         }
        //         // 한쪽이 아래쪽으로 향한다면?
        //         if (v2.Coordinate.y < Coordinate.y)
        //         {
        //             this.type = Vtype.START;
        //         }
        //         else if (v2.Coordinate.y > Coordinate.y)
        //         {
        //             this.type = Vtype.END;
        //         }
        //         return true;
        //     }
        //     return false;
        // }
        
        public static bool operator ==(HalfEdgeVertex x, HalfEdgeVertex y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return Math.Abs(x.Coordinate.x - y.Coordinate.x) < 0.00005f && Math.Abs(x.Coordinate.y - y.Coordinate.y) < 0.00005f;
        }
        public static bool operator !=(HalfEdgeVertex x, HalfEdgeVertex y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return Coordinate.ToString();
        }
    }
}
