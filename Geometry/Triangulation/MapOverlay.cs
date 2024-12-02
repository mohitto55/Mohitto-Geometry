using System.Collections.Generic;
using Monotone;
using NUnit.Framework;
using Swewep_Line_Algorithm;
using UnityEngine;

public static class MapOverlay 
{
    public static void StartMapOverlay(HalfEdgeData D, HalfEdgeFace face1, HalfEdgeFace face2)
    {
        // 1. Copy s1, s2 to D
        List<HalfEdge> s1 = face1.GetConnectedEdges();
        List<HalfEdge> s2 = face2.GetConnectedEdges();

        List<Segment> segments = new List<Segment>();
        Dictionary<int, HalfEdge> halfedgeDic = new Dictionary<int, HalfEdge>();
        int id = 0;
        for (int i = 0; i < s1.Count; i++)
        {
            HalfEdge halfEdge = s1[i];
            halfedgeDic.Add(id, halfEdge);
            Segment segment = new Segment(halfEdge.vertex.Coordinate, halfEdge.prev.vertex.Coordinate, id++); 
            segments.Add(segment);
        }

        for (int i = 0; i < s2.Count; i++)
        {
            HalfEdge halfEdge = s2[i];
            halfedgeDic.Add(id, halfEdge);
            Segment segment = new Segment(halfEdge.vertex.Coordinate, halfEdge.prev.vertex.Coordinate, id++); 
            segments.Add(segment);
        }
        //Debug.Log("총 세그먼트 카운트" + segments.Count);
        // 2. Compute all intersections between edges from S1 and S2


        List<Point> points = Swewep_Line_Algorithm.LS.SweepIntersections(segments, (segment, arg3) =>
        {
            Debug.Log("생성" + arg3.ToVector());
            foreach (var VARIABLE in segment)
            {
                Debug.Log("교차선 : " + VARIABLE.ToString());
            }
            D.vertices.Add(new HalfEdgeVertex( new Vector2(arg3.x, arg3.y)));
        } );
        Debug.Log(points.Count);
    }
}
