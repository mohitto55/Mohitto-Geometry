using UnityEngine;

namespace Monotone
{
    public class HalfEdge
    {
        // Edge의 앞쪽 방향에 있는 버텍스
        public HalfEdgeVertex vertex;
        public HalfEdge twin;
        public HalfEdgeFace incidentFace;
        public HalfEdge next;
        public HalfEdge prev;
        public Color edgeColor = Color.white;

        public HalfEdge()
        {
            this.vertex = null;
        }
        // public HalfEdge(HalfEdgeVertex vertex)
        // {
        //     this.vertex = vertex;
        // }
        public HalfEdgeVertex GetDestination()
        {
            return twin.vertex;
        }

        public override string ToString()
        {
            return prev?.vertex.Coordinate.ToString() + vertex?.Coordinate.ToString() + next?.vertex.Coordinate.ToString();
        }
    }
}