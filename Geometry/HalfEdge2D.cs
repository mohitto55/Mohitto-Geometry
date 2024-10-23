using System.Collections.Generic;
using UnityEngine;

// https://github.com/Habrador/Computational-geometry/blob/master/Assets/_Habrador%20Computational%20Geometry%20Library/_Utility%20scripts/Data%20structures/Half-edge/HalfEdgeData2.cs#L74
public class HalfEdgeData2
{
    //From https://www.openmesh.org/media/Documentations/OpenMesh-6.3-Documentation/a00010.html
    public HashSet<HalfEdgeVertex2> vertices;
    public HashSet<HalfEdgeFace2> faces;
    public HashSet<HalfEdge2> edges;
    
    public HalfEdgeData2()
    {
        this.vertices = new HashSet<HalfEdgeVertex2>();

        this.faces = new HashSet<HalfEdgeFace2>();

        this.edges = new HashSet<HalfEdge2>();
    }

    
    public HashSet<HalfEdge2> GetUniqueEdges()
    {
        HashSet<HalfEdge2> uniqueEdges = new HashSet<HalfEdge2>();

        foreach (HalfEdge2 e in edges)
        {
            Vector2 p1 = e.v.position;
            Vector2 p2 = e.prevEdge.v.position;

            bool isInList = false;

            //TODO: Put these in a lookup dictionary to improve performance
            foreach (HalfEdge2 eUnique in uniqueEdges)
            {
                Vector2 p1_test = eUnique.v.position;
                Vector2 p2_test = eUnique.prevEdge.v.position;

                if ((p1.Equals(p1_test) && p2.Equals(p2_test)) || (p2.Equals(p1_test) && p1.Equals(p2_test)))
                {
                    isInList = true;

                    break;
                }
            }

            if (!isInList)
            {
                uniqueEdges.Add(e);
            }
        }

        return uniqueEdges;
    }
}

public class HalfEdgeVertex2
{
    public Vector2 position;
    public HalfEdge2 edge;

    public HalfEdgeVertex2(Vector2 position)
    {
        this.position = position;
    }
}


public class HalfEdgeFace2
{
    public HalfEdge2 edge;

    public HalfEdgeFace2(HalfEdge2 edge)
    {
        this.edge = edge;
    }
}
public class HalfEdge2
{
    public HalfEdgeVertex2 v;
    public HalfEdgeFace2 face;
    public HalfEdge2 nextEdge;
    public HalfEdge2 oppositeEdge;
    public HalfEdge2 prevEdge;

    public HalfEdge2(HalfEdgeVertex2 v)
    {
        this.v = v;
    }
}