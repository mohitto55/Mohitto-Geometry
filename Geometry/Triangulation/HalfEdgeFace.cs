using System.Collections.Generic;
using UnityEngine;

namespace Monotone
{
    public class HalfEdgeFace
    {
        public HalfEdge OuterComponent;
        public HalfEdge InnerComponent => OuterComponent.twin;
        public HalfEdgeFace()
        {
            this.OuterComponent = null;
        }
        public HalfEdgeFace(HalfEdge outerComponent)
        {
            this.OuterComponent = outerComponent;
        }

        public List<HalfEdge> GetOuterEdges()
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
        public List<HalfEdge> GetInnerEdges()
        {
            List<HalfEdge> edges = new List<HalfEdge>();
            HalfEdge curEdge = OuterComponent.twin;
            if (curEdge != null)
            {
                edges.Add(curEdge);
                curEdge = curEdge.next;
                while (curEdge != null && curEdge != OuterComponent.twin)
                {
                    edges.Add(curEdge);
                    curEdge = curEdge.next;
                }
            }
            return edges;
        }
        
        public List<HalfEdgeVertex> GetAdjacentVertices()
        {
            HalfEdge curEdge = OuterComponent;
            return HalfEdgeUtility.GetBoundaryVertices(curEdge);
        }

        // public HalfEdge GetEdgeIncludeVertex(HalfEdgeVertex vertex)
        // {
        //     HalfEdge curEdge = InnerComponent;
        //     if (curEdge != null)
        //     {
        //         curEdge = curEdge.next;
        //         while (curEdge != null && curEdge != InnerComponent)
        //         {
        //             if (curEdge.vertex == vertex)
        //                 return curEdge;
        //             curEdge = curEdge.next;
        //         }
        //     }
        //
        //     return null;
        // }
    }
}