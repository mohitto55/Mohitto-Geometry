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
                    
                    Debug.Log(holeLeftEdge.vertex.Coordinate + " 의 왼쪽 세그먼트 : " + leftVertexEnumerable.First().Value);
                    Debug.Log(holeLeftEdge.vertex.Coordinate + " 의 왼쪽 엣지 : " + nearLeftEdge);
                    Debug.Log("각도 " + HalfEdgeUtility.CheckInnerCycle(nearLeftEdge));
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
                            Debug.Log("반대로");
                            nearLeftEdge = nearLeftEdge.twin;
                            leftID = boundaryTable[nearLeftEdge];
                        }
                        Debug.Log("Left ID " + leftID + " / Right ID : " + rightID);
                        graphG.Merge(leftID, rightID);
                        Debug.Log(graphG);
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
        Debug.Log(graphG);
        IEnumerable<List<HalfEdge>> connectedComponentNodes = graphG.GetConnectedItems();
        int k = 0;
        foreach (var connectedComponents in connectedComponentNodes)
        {
            // C를 해당 구성요소의 유일한 외부 경계 순환이라 하고
            HalfEdge C = null;
            Debug.LogWarning( k + "번쨰 시작");
            k++;
            foreach (HalfEdge component in connectedComponents)
            {
                float isOuter = HalfEdgeUtility.CheckInnerCycle(component);
                if (isOuter >= 0)
                {
                    C = component;
                    Debug.LogWarning("Outer 컴포넌트 발견" + connectedComponents.Count);
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
