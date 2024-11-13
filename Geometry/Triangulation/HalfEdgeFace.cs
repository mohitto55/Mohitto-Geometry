using System.Collections.Generic;
using UnityEngine;

namespace Monotone
{
    public class HalfEdgeFace
    {
        public HalfEdge OuterComponent;
        public HalfEdge InnerComponent;
        public HalfEdgeFace()
        {
            this.OuterComponent = null;
        }
        public HalfEdgeFace(HalfEdge outerComponent)
        {
            this.OuterComponent = outerComponent;
        }

        public List<HalfEdge> GetConnectedEdges()
        {
            List<HalfEdge> edges = new List<HalfEdge>();
            HalfEdge curEdge = OuterComponent;
            if (curEdge != null)
            {
                edges.Add(curEdge);
                curEdge = curEdge.next;
                while (curEdge != null && curEdge != OuterComponent)
                {
                    edges.Add(curEdge);
                    curEdge = curEdge.next;
                }
            }
            return edges;
        }
        public List<HalfEdgeVertex> GetConnectedVertexs()
        {
            List<HalfEdgeVertex> vertices = new List<HalfEdgeVertex>();
            HalfEdge curEdge = OuterComponent;
            if (curEdge != null)
            {
                vertices.Add(curEdge.vertex);
                curEdge = curEdge.next;
                while (curEdge != null && curEdge != OuterComponent)
                {
                    vertices.Add(curEdge.vertex);
                    curEdge = curEdge.next;
                }
            }
            return vertices;
        }
    }
}