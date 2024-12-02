using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monotone;
using Sirenix.Utilities;
using UnityEditor.Build;
using UnityEngine;

namespace Monotone
{
    public static class Monotone
    {
        public static IEnumerator MonotoneTriangulation(HalfEdgeData polygon, HalfEdgeDebugValue debugHalfEdgeDebug)
        {
            //MapOverlay.StartMapOverlay(polygon, polygon.faces[0], polygon.faces[1]);

            //List<HalfEdgeVertex> vertices = polygon.faces[1].GetConnectedVertices();

            //MapOverlay.StartMapOverlay(polygon, polygon.faces[0], polygon.faces[1]);
             yield return MakeMonotone(polygon, debugHalfEdgeDebug, 0.5f);
            HalfEdgeData monotone = polygon;
            List<HalfEdgeFace> faces = new List<HalfEdgeFace>();
            yield return new WaitForSeconds(0);
            Debug.Log("페이스 갯수 " + monotone.faces.Count);
            for (int i = 0; i < monotone.faces.Count; i++)
            {
                faces.Add(monotone.faces[i]);
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Debug.Log("페이스 " + i + " " + faces[i].GetConnectedEdges().Count);
                //yield return HalfEdgeDebug.TravelFaceVertex(polygon, faces[i], debugHalfEdgeDebug, 0.2f);
                yield return MonotoneTriangulation(polygon, faces[i], debugHalfEdgeDebug, 0.3f);
            }
            //yield return MonotoneTriangulation(polygon, faces, debugHalfEdgeDebug, 0.3f);

            //HalfEdgeDebug.DebugHalfEdgeData(polygon);
            yield return null;
        }

        public static IEnumerator MonotoneTriangulation(HalfEdgeData halfEdgeData, HalfEdgeFace monotoneFace, HalfEdgeDebugValue debugHalfEdgeDebug, float delay = 1)
        {
             SortedSet<HalfEdge> sortedEdges = new SortedSet<HalfEdge>(new MonotoneEdgeTriangulationComparer());

            List<HalfEdge> edges = new List<HalfEdge>();
            edges.AddRange(monotoneFace.GetConnectedEdges());
            sortedEdges.UnionWith(edges);

            // Stack으로는 중간에 있는 요소를 없앨 수 없어서 LinkedList로 한다.
            LinkedList<HalfEdge> s = new LinkedList<HalfEdge>();
            
             Dictionary<HalfEdge, int> chainDic = new Dictionary<HalfEdge, int>();
            
             HalfEdge searchEdge = null;
             HalfEdge lastEdge = sortedEdges.Last();
            
            
             // Fisrt Vertex의 next가 RightChain일경우
             Vector2 topVertex = sortedEdges.First().vertex.Coordinate;
             Vector2 prevVertex = sortedEdges.First().prev.vertex.Coordinate;
             Vector2 nextVertex = sortedEdges.First().next.vertex.Coordinate;
             int safeCount = 0;
             if (MyMath.CCW(prevVertex, topVertex, nextVertex) < 0)
             {
                 searchEdge = sortedEdges.First().next;
                 while (lastEdge != searchEdge && safeCount <= sortedEdges.Count)
                 {
                     // 오른쪽이면 1 왼쪽이면 0
                     if (!chainDic.ContainsKey(searchEdge))
                     {
                         chainDic.Add(searchEdge, 1);
                     }
                     searchEdge = searchEdge.next;
                     safeCount++;
                 }
                 searchEdge = sortedEdges.First().prev;
                 safeCount = 0;
                 while (lastEdge != searchEdge && safeCount <= sortedEdges.Count)
                 {
                     if (!chainDic.ContainsKey(searchEdge))
                     {
                         chainDic.Add(searchEdge, 0);
                     }
                     searchEdge = searchEdge.prev;
                     safeCount++;
                 }
             }
             // Fisrt Vertex의 next가 LeftChain일경우
             else
             {
                 lastEdge = sortedEdges.Last();
                 searchEdge = sortedEdges.First().next;
                 safeCount = 0;
            
                // Debug.LogWarning(lastEdge.vertex.Coordinate);
                 while (lastEdge != searchEdge && safeCount <= sortedEdges.Count)
                 {
                     if (!chainDic.ContainsKey(searchEdge))
                     {
                         chainDic.Add(searchEdge, 0);
                     }
                     searchEdge = searchEdge.next;
                     safeCount++;
                 }
                 searchEdge = sortedEdges.First().prev;
                 safeCount = 0;
            
                 while (lastEdge != searchEdge && safeCount <= sortedEdges.Count)
                 {
                     if (!chainDic.ContainsKey(searchEdge))
                     {
                         chainDic.Add(searchEdge, 1);
                     }
                     searchEdge = searchEdge.prev;
                     safeCount++;
                 }
             }
            
             foreach (var pair in chainDic)
             {
                 float length = 3;
                 if (pair.Value == 0)
                 {
                     Debug.DrawLine(pair.Key.vertex.Coordinate, pair.Key.vertex.Coordinate + Vector2.left * length,Color.magenta);
                 }
                 else
                 {
                     Debug.DrawLine(pair.Key.vertex.Coordinate, pair.Key.vertex.Coordinate + Vector2.right * length, Color.red);
                 }
             }
             Debug.DrawLine(sortedEdges.Last().vertex.Coordinate, sortedEdges.Last().vertex.Coordinate + Vector2.down * 3, Color.red);
             chainDic.Add(sortedEdges.First(), chainDic[sortedEdges.ElementAt(1)]);
             
             // 맨 아래 정점은 y축으로 두번째로 가장 낮은 정점의 chain과 같다.
             chainDic.Add(sortedEdges.Last(), chainDic[sortedEdges.ElementAt(chainDic.Count - 2)]);
             s.AddFirst(sortedEdges.ElementAt(0));
             s.AddFirst(sortedEdges.ElementAt(1));
             for (int j = 2; j < sortedEdges.Count; j++)
             {
                 HalfEdge uj = sortedEdges.ElementAt(j);
                 debugHalfEdgeDebug.value = uj.vertex.Coordinate;
                 Debug.Log("===========" + uj.vertex.Coordinate + "===========");
                 int sortedVertexChain = chainDic[uj];
                 Debug.Log("===========" +s.First.Value.vertex.Coordinate + "===========");
                 Debug.Log("===========" +s.Count + "===========");
                 yield return new WaitForSeconds(delay);
                 int stackVertexChain = chainDic[s.First.Value];
                 Debug.Log("J : " + j+ " S : " + s.Count);
                 foreach (var VARIABLE in s)
                 {
                     Debug.Log("SV : " + VARIABLE.vertex.Coordinate);
                 }
                 if (uj != null && sortedVertexChain == stackVertexChain && j < sortedEdges.Count-1)
                 {
                     s.AddFirst(uj);
                     // 각도가 음수가 아니면 internal이다.
                     float internalAngle = 0;
                     do
                     {
                         if (s.Count <= 2)
                             break;
                         HalfEdge top0 = s.ElementAt(0);
                         HalfEdge top1 = s.ElementAt(1);
                         HalfEdge top2 = s.ElementAt(2);
                         
                         int chain = chainDic[uj];
                         // 왼쪽 체인이냐 오른쪽 체인이냐에따라
                         if (chain == 0)
                         {
                             internalAngle = MyMath.SignedAngle(top2.vertex.Coordinate - top1.vertex.Coordinate,
                                 top0.vertex.Coordinate - top1.vertex.Coordinate);
                         }
                         else
                         {
                             internalAngle = MyMath.SignedAngle(top0.vertex.Coordinate - top1.vertex.Coordinate,
                                 top2.vertex.Coordinate - top1.vertex.Coordinate);
                         }
                         
                         // 각도가 음수가 아니면 internal이다.
                         if (internalAngle >= 0)
                         {
                             // Chain이 바뀌면 어차피 맨 마지막 요소는 이어져 있을 것이기 떄문에 없애준다.
                             halfEdgeData.AddDiagonal(top0.vertex, top2.vertex);
                             
                             s.RemoveFirst();
                             s.RemoveFirst();
                             s.AddFirst(top0);
                             Debug.Log("s " + s.Count);
                             Debug.Log("인터널의 마지막 " + top0.vertex.Coordinate);

                         }
                         debugHalfEdgeDebug.value = uj.vertex.Coordinate;
                         yield return new WaitForSeconds(delay);
                     } while (internalAngle >= 0 && s.Count > 2);
                 }
                 else
                 {
                     foreach (var VARIABLE in s)
                     {
                         Debug.Log("SV2 : " + VARIABLE.vertex.Coordinate);
                     }
                     s.RemoveLast();
                     HalfEdge prev = s.First.Value;
                     while (s.Count >= 2)
                     {
                         Debug.Log("진짜 갯수 " + s.First.Value.vertex.Coordinate + " " + HalfEdgeUtility.GetEdgesAdjacentToAVertexBruteForce(halfEdgeData, s.First.Value.vertex).Count);
                             halfEdgeData.AddDiagonal(s.Last.Value.vertex, uj.vertex);
                         s.RemoveLast();
                         yield return new WaitForSeconds(delay);
                     }
                     // Q의 오직 하나 남은 vertex랑 연결하기
                         halfEdgeData.AddDiagonal(s.First.Value.vertex, uj.vertex);
                     s.AddFirst(uj);
                     foreach (var VARIABLE in s)
                     {
                         Debug.Log("SV3 : " + VARIABLE.vertex.Coordinate);
                     }
                 }
             }

            //return halfEdgeData;
            yield return null;
        }

        public class HalfEdgeDebugValue
        {
            public Vector2 value;
            public Color color;
        }
        public static IEnumerator MakeMonotone(HalfEdgeData halfEdgeData, HalfEdgeDebugValue debugHalfEdgeDebug, float debugDelay = 0.5f)
        {
            MonotoneVertexComparer monotoneVertexComparer = new MonotoneVertexComparer();
            SortedSet<HalfEdgeVertex> sortedVertexs = new SortedSet<HalfEdgeVertex>(monotoneVertexComparer);
            List<HalfEdge> ConnectedEdges = new List<HalfEdge>();
            // polygon의 vertices들은 반대쪽 vertex도 포함되지만 compare에 의해서 위치가 같은 vertex는 삭제시킨다.
            for (int i = 0; i < halfEdgeData.faces.Count; i++)
            {
                HalfEdgeFace face = halfEdgeData.faces[i];
                sortedVertexs.UnionWith(face.GetConnectedVertices());
                ConnectedEdges.AddRange(HalfEdgeUtility.DetermineVertexType(face));
            }

            MonotoneEdgeComparer monotoneEdgeComparer = new MonotoneEdgeComparer();
            // SweepLine과 교차중인 Edge들만을 넣는다
            SortedSet<HalfEdge> statusEdges = new SortedSet<HalfEdge>(monotoneEdgeComparer);

            // 헬퍼로 사용할 딕셔너리
            Dictionary<HalfEdge, HalfEdgeVertex> helperDic = new Dictionary<HalfEdge, HalfEdgeVertex>();
            foreach (HalfEdge edge in ConnectedEdges)
            {
                // 가장 왼쪽인 버텍스가 0으로 가게하기
                helperDic.Add(edge, null);
            }
            
            // 버텍스 하나씨 탐색
            while (sortedVertexs.Count > 0)
            {
                HalfEdgeVertex vertex = sortedVertexs.First();
                yield return new WaitForSeconds(debugDelay);
                // 이미 사용한 버텍스는 삭제시킨다.
                sortedVertexs.Remove(vertex);
                debugHalfEdgeDebug.value = vertex.Coordinate;
                //Handle
                HandleVertex(vertex, statusEdges, helperDic, halfEdgeData);
            }
        }

        public static void HandleVertex(HalfEdgeVertex vertex, SortedSet<HalfEdge> statusEdges,
            Dictionary<HalfEdge, HalfEdgeVertex> helperDic, HalfEdgeData halfEdgeData)
        {
            // 시작부분이니 상태추가
            // LeftEdge의 부모를 찾아야한다.
            if (vertex.type == HalfEdgeVertex.Vtype.START)
            {
                HalfEdge leftEdge = vertex.LeftEdge();
                statusEdges.Add(leftEdge);

                HalfEdgeVertex helperVertex = FindHelper(leftEdge, helperDic);
                if (helperVertex != null)
                {
                    halfEdgeData.AddDiagonal(helperVertex, vertex);
                }

                helperDic[leftEdge] = vertex;
            }
            
            // 마무리를 해야한다.
            if (vertex.type == HalfEdgeVertex.Vtype.END)
            {
                HalfEdge leftEdge = vertex.LeftEdge();
                if (leftEdge == null)
                {
                    return;
                }

                HalfEdgeVertex helperVertex = FindHelper(leftEdge, helperDic);
                if (helperVertex != null && helperVertex.type == HalfEdgeVertex.Vtype.MERGE)
                {
                    halfEdgeData.AddDiagonal(helperVertex, vertex);
                }

                statusEdges.Remove(leftEdge);
            }
            
            // 분할이 필요한 구간
            if (vertex.type == HalfEdgeVertex.Vtype.SPLIT)
            {
                HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                HalfEdgeVertex helperVertex = FindHelper(leftEdge, helperDic);
                halfEdgeData.AddDiagonal(helperVertex, vertex);
                helperDic[leftEdge] = vertex;
                HalfEdge rightEdge = vertex.RightEdge();
                Debug.Log("rightEdge " + rightEdge.vertex.Coordinate + rightEdge.prev.vertex.Coordinate);

                statusEdges.Add(rightEdge);
                helperDic[rightEdge] = vertex;
            }
            else if (vertex.type == HalfEdgeVertex.Vtype.MERGE)
            {
                HalfEdge rightEdge = vertex.RightEdge();
                HalfEdgeVertex rightHelperVertex = FindHelper(rightEdge, helperDic);

                if (rightHelperVertex != null && rightHelperVertex.type == HalfEdgeVertex.Vtype.MERGE)
                {
                    halfEdgeData.AddDiagonal(rightHelperVertex, vertex);
                }

                // right Edge와 가장 가까운 helper를 연결시켜 줫으니 rightEdge는 더이상 건드릴 필요 없다
                statusEdges.Remove(rightEdge);

                HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                HalfEdgeVertex leftHelperVertex = FindHelper(leftEdge, helperDic);
                if (leftHelperVertex != null && leftHelperVertex.type == HalfEdgeVertex.Vtype.MERGE)
                {
                    halfEdgeData.AddDiagonal(leftHelperVertex, vertex);
                }

                helperDic[leftEdge] = vertex;
                // // right Edge와 가장 가까운 helper를 연결시켜 줫으니 rightEdge는 더이상 건드릴 필요 없다
                // if(leftEdge != null)
                // statusEdges.Remove(leftEdge);
            }
            
            // Refular은
            else if (vertex.type == HalfEdgeVertex.Vtype.REGULAR)
            {
                if (vertex.PolygonInteriorLiesToTheRight())
                {
                    HalfEdge prevEdge = vertex.IncidentEdge.prev;
                    HalfEdge nextEdge = vertex.IncidentEdge.next;
                    HalfEdge aboveEdge = prevEdge.vertex.Coordinate.y >= nextEdge.vertex.Coordinate.y ? vertex.IncidentEdge : nextEdge;
                    HalfEdge belowEdge = aboveEdge == vertex.IncidentEdge ? nextEdge : vertex.IncidentEdge;
                    HalfEdgeVertex helper = FindHelper(aboveEdge, helperDic);
                    if (helper != null && helper.type == HalfEdgeVertex.Vtype.MERGE)
                    {
                        halfEdgeData.AddDiagonal(helper, vertex);
                    }
                    statusEdges.Remove(aboveEdge);
                    statusEdges.Add(belowEdge);
                    helperDic[belowEdge] = vertex;
                }
                else
                {
                    HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                    if (leftEdge == null)
                        return;
                    
                    statusEdges.Add(leftEdge);
                    HalfEdgeVertex helper = FindHelper(leftEdge, helperDic);
                    if (helper == null)
                        return;
                    if (helper.type == HalfEdgeVertex.Vtype.MERGE)
                    {
                        halfEdgeData.AddDiagonal(helper, vertex);
                    }

                    helperDic[leftEdge] = vertex;
                }
            }
        }

        // 컴페어를 통해 정점들을높이별로 정렬하고 Edge를 스윕 방법별로 정렬하는데 도움이 된다.
        public class MonotoneVertexComparer : IComparer<HalfEdgeVertex>
        {
            public int Compare(HalfEdgeVertex x, HalfEdgeVertex y)
            {
                if (x.Coordinate == y.Coordinate)
                {
                    return 0;
                }
                if (MyMath.FloatZero(x.Coordinate.y - y.Coordinate.y))
                {
                    if (x.Coordinate.x < y.Coordinate.x)
                    {
                        return -1;
                    }
                    else if(x.Coordinate.x > y.Coordinate.x)
                    {
                        return 1;
                    }
                }
                if (x.Coordinate.y > y.Coordinate.y)
                {
                    return -1;
                }
                return 1;
            }
        }

        // 누가 가장 왼쪽에 있는지 확인한다
        public class MonotoneEdgeComparer : IComparer<HalfEdge>
        {
            public int Compare(HalfEdge e1, HalfEdge e2)
            {

                HalfEdgeVertex x = e1.vertex;
                HalfEdgeVertex y = e2.vertex;
                if (x.Coordinate == y.Coordinate)
                {
                    return 0;
                }
                if (MyMath.FloatZero(x.Coordinate.y - y.Coordinate.y))
                {
                    if (x.Coordinate.x < y.Coordinate.x)
                    {
                        return -1;
                    }
                    else if(x.Coordinate.x > y.Coordinate.x)
                    {
                        return 1;
                    }
                }
                if (x.Coordinate.y > y.Coordinate.y)
                {
                    return -1;
                }
                return 1;
            }
        }
        
        // 누가 가장 왼쪽에 있는지 확인한다
        public class MonotoneEdgeTriangulationComparer : IComparer<HalfEdge>
        {
            public int Compare(HalfEdge x, HalfEdge y)
            {
                if (MyMath.FloatZero(x.vertex.Coordinate.y - y.vertex.Coordinate.y))
                {
                    if (x.vertex.Coordinate.x < y.vertex.Coordinate.x) return -1;
                    else return 1;
                }
                if (x.vertex.Coordinate.y >= y.vertex.Coordinate.y || 
                    (x.vertex.Coordinate.y == y.vertex.Coordinate.y &&
                     x.vertex.Coordinate.x <= y.vertex.Coordinate.x)) return -1;
                return 1;
            }
        }

        public static HalfEdge FindLeftEdge(HalfEdgeVertex vertex, SortedSet<HalfEdge> sortedEdges)
        {
            float minDst = float.MaxValue;
            HalfEdge leftEdge = null;
            foreach (HalfEdge edge in sortedEdges)
            {
                HalfEdgeVertex p1 = edge.vertex;
                HalfEdgeVertex p2 = edge.prev.vertex;
                //Debug.Log("래프트찾기" + vertex.Coordinate + " " + sortedEdges.Count);

                //Debug.Log(p1.Coordinate + " " + p2.Coordinate);
                // 탐색중인 Edge가 vertex를 포함하면 다음으로
                if (p1.Coordinate == vertex.Coordinate || p2.Coordinate == vertex.Coordinate)
                    continue;

                // p1이 무조건 위에있는 것으로 설정하기
                if (p1.Coordinate.y <= p2.Coordinate.y)
                {
                    HalfEdgeVertex temp = p2;
                    p2 = p1;
                    p1 = temp;
                }

                // p1과 p2 사이에 vertex가 존재하는지
                if (p1.Coordinate.y >= vertex.Coordinate.y && vertex.Coordinate.y >= p2.Coordinate.y)
                {
                    float ccwValue = MyMath.CCW(p1.Coordinate, p2.Coordinate, vertex.Coordinate);
                    // 위에서 아래로 내려가는 직선 기준으으로 왼쪽방향에 있지 않으면 다음으로간다.
                    if (ccwValue < 0)
                        continue;
                    Vector2 intersect = Vector2.one;

                    // 직선의 교차점 방정식을 이용한다
                    MyMath.LineIntersection(p2.Coordinate, p1.Coordinate, vertex.Coordinate,
                        vertex.Coordinate + Vector2.left, ref intersect);
                    float distanceToEdge = Vector2.Distance(intersect, vertex.Coordinate);

                    if (distanceToEdge < minDst)
                    {
                        minDst = distanceToEdge;
                        leftEdge = edge;
                    }
                }
            }

            if (leftEdge != null)
            {
                //Debug.DrawLine(vertex.Coordinate, vertex.Coordinate + Vector2.left * minDst, Color.green);
            }

            return leftEdge;
        }

        // 똑같은 가장 왼쪽 Edge를 가진 vertex들중 자신보다 높이있으면서 가장 낮게있는 vertex를 반환한다.
        public static HalfEdgeVertex FindHelper(HalfEdge leftEdge, Dictionary<HalfEdge, HalfEdgeVertex> dic)
        {
            if (leftEdge == null)
            {
                Debug.LogWarning("LeftEdge가 없습니다.");
                return null;
            }

            if (!dic.ContainsKey(leftEdge))
            {
                Debug.LogWarning("LeftEdge가 등록되지 않았습니다.");
                return null;
            }

            HalfEdgeVertex helperVertex = dic[leftEdge];
            return helperVertex;
        }

        public static float DistanceEdgeAndPoint(Vector2 start, Vector2 end, Vector2 p)
        {
            Vector2 edge = end - start;
            Vector2 startTop = p - start;
            return Mathf.Abs(MyMath.Cross2D(edge, startTop)) / edge.magnitude;
        }
        
        
    }
}