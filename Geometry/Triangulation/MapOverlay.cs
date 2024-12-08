using System.Collections.Generic;
using System.Linq;
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
        // 1. Copy s1, s2 to D

        List<Segment> segments = new List<Segment>();
        Dictionary<int, HalfEdge> halfedgeDic = new Dictionary<int, HalfEdge>();
        int id = 0;
        
        for (int i = 0; i < D.faces.Count; i++)
        {
            List<HalfEdge> edges = D.faces[i].GetOuterEdges();
            for (int j = 0; j < edges.Count; j++)
            {
                HalfEdge halfEdge = edges[j];
                halfedgeDic.Add(id, halfEdge);
                Segment segment = new Segment(halfEdge.vertex.Coordinate, halfEdge.prev.vertex.Coordinate, id);
                id++;
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

        
        
        
        Dictionary<Point, Segment> slTable;
        List<Point> points = Swewep_Line_Algorithm.LS.SweepIntersections(D, segments, out slTable,(segment, arg3) =>
        {
            Debug.Log("생성" + arg3.ToVector());
            foreach (var VARIABLE in segment)
            {
                Debug.Log("교차선 : " + VARIABLE.ToString());
            }
            D.vertices.Add(new HalfEdgeVertex( new Vector2(arg3.x, arg3.y)));
        } );

        Debug.Log("인털섹션 가장 왼쪽 점 찾기" + slTable.Count);
        foreach (var VARIABLE in slTable)
        {
            Debug.Log(VARIABLE.Key.ToVector() + " SEGMENT : " + VARIABLE.Value.Start.ToVector() + " " + VARIABLE.Value.End.ToVector() );
        }
        
        foreach (var VARIABLE in halfedgeDic)
        {
            Debug.Log(VARIABLE.Key.ToString() + "ㅊ자기" );
        }



        // D를 탐색하여 O(S1, S2)의 경계 사이클을 결정한다.
        // 각 경계 사이클에 대해 노드가 생성되는 그래프 G를 생성한다. 
        DisjointSet<HalfEdge> graphG = new DisjointSet<HalfEdge>(D.faces.Count() * 2 + 1);
        Dictionary<HalfEdge, int> boundaryTable = new Dictionary<HalfEdge, int>();
        List<HalfEdge> cycle = new List<HalfEdge>();
        for (int i = 0; i < D.faces.Count; i++)
        {
            HalfEdge innerComponent = D.faces[i].InnerComponent;
            HalfEdge outerComponent = D.faces[i].OuterComponent;
            
            cycle.Add(innerComponent);
            cycle.Add(outerComponent);
            
            graphG.AddItem(innerComponent);
            graphG.AddItem(outerComponent);

            List<HalfEdge> innerEdges = HalfEdgeUtility.GetBoundaryEdges(innerComponent);
            List<HalfEdge> outerEdges = HalfEdgeUtility.GetBoundaryEdges(outerComponent);

            int innerId = graphG.GetItemId(innerComponent);
            int outerId = graphG.GetItemId(outerComponent);
            foreach (HalfEdge edge in innerEdges)
            {
                if (!boundaryTable.ContainsKey(edge))
                {
                    boundaryTable.Add(edge, innerId);
                }
            }
            foreach (HalfEdge edge in outerEdges)
            {
                if (!boundaryTable.ContainsKey(edge))
                {
                    boundaryTable.Add(edge, outerId);
                }
            }
        }
        for (int i = 0; i < D.faces.Count; i++)
        {
            HalfEdge innerComponent = D.faces[i].InnerComponent;

            HalfEdge leftEdge = HalfEdgeUtility.FindLeftmostEdgeInCycle(innerComponent);
            Debug.Log("가장 왼쪽 Inner 정점" + leftEdge);


            // p의 가장 왼쪽 세그먼트를 찾는다.
            var sameVertex = slTable.Where(t =>
            {
                return MathUtility.FloatZero(t.Key.x - leftEdge.vertex.Coordinate.x) &&
                       MathUtility.FloatZero(t.Key.y - leftEdge.vertex.Coordinate.y);
            });
            
            
            if (sameVertex.Count() > 0 && sameVertex.First().Key != null)
            {
                Point point = sameVertex.First().Key;
                Segment segment = sameVertex.First().Value;
                if (halfedgeDic.ContainsKey(segment.num))
                {
                    HalfEdge edge = halfedgeDic[segment.num];
                    
                    Debug.Log(leftEdge.vertex.Coordinate + " 의 왼쪽 세그먼트 : " + sameVertex.First().Value);
                    Debug.Log(leftEdge.vertex.Coordinate + " 의 왼쪽 엣지 : " + edge);
                    Debug.Log("각도 " + HalfEdgeUtility.CheckInnerCycle(edge));
                    float isInner = HalfEdgeUtility.CheckInnerCycle(edge);
                    
                    // 음수면 Inner
                    if (isInner <= 0)
                    {
                        if (boundaryTable.ContainsKey(edge) && boundaryTable.ContainsKey(leftEdge))
                        {
                            int rightID = boundaryTable[edge];
                            int leftID = boundaryTable[leftEdge];
                            Debug.Log("Left ID " + leftID + " / Right ID : " + rightID);
                            graphG.Merge(leftID, rightID);
                            Debug.Log(graphG);
                        }
                        else
                        {
                            if (!boundaryTable.ContainsKey(edge))
                            {
                                Debug.Log("라이트 없음");
                            }
                            if (!boundaryTable.ContainsKey(leftEdge))
                            {
                                Debug.Log("래프트 없음");
                            }
                        }
                    }
                }

            }
        }
        /// 6. for : each connected Component in G
        IEnumerable<List<HalfEdge>> connectedComponentNodes = graphG.GetConnectedItems();
        foreach (var connectedComponents in connectedComponentNodes)
        {
            // C를 해당 구성요소의 유일한 외부 경계 순환이라 하고
            HalfEdge C = null;
            foreach (HalfEdge component in connectedComponents)
            {
                float isOuter = HalfEdgeUtility.CheckInnerCycle(component);
                if (isOuter <= 0)
                {
                    C = component;
                    Debug.LogWarning("Outer 컴포넌트 발견");
                    break;
                }
            }
            if (C == null)
            {
                Debug.LogWarning("맵 오버레이 도중 유일한 Outer Component가 존재하지 않습니다.");
                continue;
            }
            
            // f를 C에 의해 경계 지어진 면이라 합니다
            // f에 대한 레코드를 생성하고
            HalfEdgeFace F = C.incidentFace;
            // C의 임의의 반변을 OuterComponent(f)로 설정하며
            // 구성요소 내의 각 Inner Cycle에 대한 포인터들로 구성된 리스트 InnerComponents(f)를 구성합니다
            // 구성요소의 각 내부 순환에 대해 IncidentFace(e)를 f로 설정합니다 (여기서 e는 순환의 임의의 반변)
            foreach (HalfEdge component in connectedComponents)
            {
                if(component == C)
                    continue;
                List<HalfEdge> innerEdges = HalfEdgeUtility.GetBoundaryEdges(component);
                foreach (HalfEdge innerEdge in innerEdges)
                {
                    innerEdge.incidentFace = F;
                }
            }
        }
        Debug.Log(points.Count);
    }

    public class GraphG
    {
        public int ID = 0;
        public HalfEdge edge = null;
    }
}
