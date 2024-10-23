using System.Collections.Generic;
using UnityEngine;

public class HalfEdgeUtility : MonoBehaviour
{
    public List<HalfEdgeVertex2> MakeHalfEdge(List<Vector2> positions)
    {
        List<HalfEdgeVertex2> halfEdgeVertexs = new List<HalfEdgeVertex2>();
        
        
        for (int i = 0; i < positions.Count; i++)
        {
            HalfEdgeVertex2 halfEdgeVertex2 = new HalfEdgeVertex2(positions[i]);
            halfEdgeVertexs.Add(halfEdgeVertex2);
            //halfEdgeVertexs[i].edge
        }
        return halfEdgeVertexs;
    }
}
