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
                segments.Add(segment);
                id++;
            }
        }
        //Debug.Log("총 세그먼트 카운트" + segments.Count);
        // 2. Compute all intersections between edges from S1 and S2

        // 섹션 2.1의 plane sweep 알고리즘을 사용하여 S1과 S2의 간선 사이의 모든 교점을 계산한다.
        // 이벤트 포인트에서 필요한 T와 Q에 대한 작업 외에도 다음을 수행한다:
        //    S1과 S2의 간선이 모두 관련된 이벤트일 경우 D를 업데이트한다.
        //    (이는 S1의 간선이 S2의 정점을 통과하는 경우에 대해 설명되었다.)
        //- 이벤트 포인트의 왼쪽에 있는 반엣지를 D의 해당 정점에 저장한다.

        
        
        // 맵 오버레이에서 사용 내부, 외부 판별시 사용한다.
        // 이벤트 포인트의 가장 왼쪽에 있는 DCEL을 저장한다.
        Dictionary<Point, Segment> slTable;
        List<Point> points = Swewep_Line_Algorithm.LS.SweepIntersections(segments, out slTable,(segments, arg3) =>
        {
            // HalfEdgeVertex newVertex = new HalfEdgeVertex(new Vector2(arg3.x + 1, arg3.y + 5));
            // D.vertices.Add(newVertex);
            // foreach (var segment in segments)
            // {
            //     HalfEdge matchEdge = halfedgeDic[segment.num];
            //
            //     //D.InsertEdge(matchEdge, newVertex);
            //     foreach(var aedge in HalfEdgeUtility.GetEdgesAdjacentToAVertex(newVertex))
            //     {
            //         Debug.Log("Edges Match : " + aedge);
            //     }
            // }
        } );

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
        //  각 hole 사이클을 가장 왼쪽 정점의 왼쪽에 있는 사이클(Inner Component)과 연결하고,
        //  이들의 연결된 컴포넌트를 계산한다.  
        //  (2번 단계의 두 번째 항목에서 G의 arc를 결정하는 정보가 계산되었다.)
        for (int i = 0; i < D.faces.Count; i++)
        {
            HalfEdge innerComponent = D.faces[i].InnerComponent;

            // 구멍에서 가장 왼쪽은 엣지
            HalfEdge holeLeftEdge = HalfEdgeUtility.FindLeftmostEdgeInCycle(innerComponent);
            Debug.Log(i + " 가장 왼쪽 Inner 정점" + holeLeftEdge);


            // p의 가장 왼쪽 세그먼트를 찾는다.
            var leftVertexEnumerable = slTable.Where(t =>
            {
                return MathUtility.FloatZero(t.Key.x - holeLeftEdge.vertex.Coordinate.x) &&
                       MathUtility.FloatZero(t.Key.y - holeLeftEdge.vertex.Coordinate.y);
            });
            
            
            if (leftVertexEnumerable.Count() > 0 && leftVertexEnumerable.First().Key != null)
            {
                Point point = leftVertexEnumerable.First().Key;
                Segment segment = leftVertexEnumerable.First().Value;
                // sl테이블(정점의 가장 가까운 왼쪽 edge)의 segment에 해당하는 edge를 가져온다.
                if (halfedgeDic.ContainsKey(segment.num))
                {
                    HalfEdge nearLeftEdge = halfedgeDic[segment.num];
                    float isInner = HalfEdgeUtility.CheckInnerCycle(nearLeftEdge);

                    // 양수면 outer 음수면 inner
                    // 무조건 inner로 만들기
                    if (isInner > 0)
                    {
                        nearLeftEdge = nearLeftEdge.twin;
                    }

                    if (boundaryTable.ContainsKey(nearLeftEdge) && boundaryTable.ContainsKey(holeLeftEdge))
                    {
                        int rightID = boundaryTable[holeLeftEdge];
                        int leftID = boundaryTable[nearLeftEdge];

                        if (leftID == 0)
                        {
                            nearLeftEdge = nearLeftEdge.twin;
                            leftID = boundaryTable[nearLeftEdge];
                        }
                        graphG.Merge(leftID, rightID);
                    }
                    else
                    {
                        // 왼쪽 edge가 없다면 무한대 노드로 합친다.
                        int leftID = boundaryTable[holeLeftEdge];
                        graphG.Merge(leftID, 0);
                        if (!boundaryTable.ContainsKey(nearLeftEdge))
                        {
                            Debug.Log("라이트 없음");
                        }
                        if (!boundaryTable.ContainsKey(holeLeftEdge))
                        {
                            Debug.Log("래프트 없음");
                        }
                    }
                    
                }

            }
        }
        /// 6. for : each connected Component in G
        IEnumerable<List<HalfEdge>> connectedComponentNodes = graphG.GetConnectedItems();
        int k = 0;
        Debug.Log("connectedComponentNodes " + connectedComponentNodes.Count());

        foreach (var connectedComponents in connectedComponentNodes)
        {
            // C를 해당 구성요소의 유일한 외부 경계 순환이라 하고
            HalfEdge onlyOuterComponent = null;
            Debug.LogWarning( k + "번쨰 시작" + connectedComponents.Count());
            k++;
            foreach (HalfEdge component in connectedComponents)
            {
                float isOuter = HalfEdgeUtility.CheckInnerCycle(component);
                Debug.LogWarning("Outer Value " + component + " / " + isOuter);

                if (isOuter >= 0)
                {
                    onlyOuterComponent = component;
                    Debug.LogWarning("Outer 컴포넌트 발견" + connectedComponents.Count);
                    break;
                }
            }
            if (onlyOuterComponent == null)
            {
                Debug.LogWarning("맵 오버레이 도중 유일한 Outer Component가 존재하지 않습니다.");
                continue;
            }
            Debug.Log("connectedComponents " + connectedComponents.Count);

            // f를 C에 의해 경계 지어진 면이라 합니다
            // f에 대한 레코드를 생성하고
            HalfEdgeFace F = onlyOuterComponent.IncidentFace;
            // C의 임의의 반변을 OuterComponent(f)로 설정하며
            // 구성요소 내의 각 Inner Cycle에 대한 포인터들로 구성된 리스트 InnerComponents(f)를 구성합니다
            // 구성요소의 각 내부 순환에 대해 IncidentFace(e)를 f로 설정합니다 (여기서 e는 순환의 임의의 반변)
            foreach (HalfEdge innerComponent in connectedComponents)
            {
                if(innerComponent == onlyOuterComponent)
                    continue;
                List<HalfEdge> innerEdges = HalfEdgeUtility.GetBoundaryEdges(innerComponent);
                Debug.Log("innerComponent " + innerComponent + " " + innerEdges.Count);

                foreach (HalfEdge innerEdge in innerEdges)
                {
                    innerEdge.IncidentFace = F;
                    innerEdge.twin.IncidentFace.shapes.UnionWith(F.shapes);
                    Debug.Log("IncidentFace " + innerEdge.IncidentFace.IncidentEdges.Count);

                }
            }
        }
        Debug.Log(graphG.ToString());
    }
}
