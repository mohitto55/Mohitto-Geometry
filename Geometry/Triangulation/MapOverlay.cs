using System.Collections.Generic;
using Monotone;
using NUnit.Framework;
using Swewep_Line_Algorithm;
using Unity.VisualScripting;
using UnityEngine;

public static class MapOverlay 
{
    /// <summary>
    /// 맵 오버레이를 하는 함수
    /// </summary>
    /// <param name="D"></param>
    public static void MonotoneOverlay(HalfEdgeData D)
    {
        HalfEdgeFace face1 , face2;
        // 1. Copy s1, s2 to D

        List<Segment> segments = new List<Segment>();
        Dictionary<int, HalfEdge> halfedgeDic = new Dictionary<int, HalfEdge>();
        int id = 0;
        for (int i = 0; i < D.faces.Count; i++)
        {
            List<HalfEdge> edges = D.faces[i].GetInnerEdges();
            for (int j = 0; j < edges.Count; j++)
            {
                HalfEdge halfEdge = edges[j];
                halfedgeDic.Add(id++, halfEdge);
                Segment segment = new Segment(halfEdge.vertex.Coordinate, halfEdge.prev.vertex.Coordinate, id++);
                segments.Add(segment);
            }
        }
        //Debug.Log("총 세그먼트 카운트" + segments.Count);
        // 2. Compute all intersections between edges from S1 and S2

        // 섹션 2.1의 plane sweep 알고리즘을 사용하여 S1과 S2의 간선 사이의 모든 교점을 계산한다.
        // 이벤트 포인트에서 필요한 T와 Q에 대한 작업 외에도 다음을 수행한다:
            //    S1과 S2의 간선이 모두 관련된 이벤트일 경우 D를 업데이트한다.
            //    (이는 S1의 간선이 S2의 정점을 통과하는 경우에 대해 설명되었다.)
            //- 이벤트 포인트의 왼쪽에 있는 반엣지를 D의 해당 정점에 저장한다.
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
