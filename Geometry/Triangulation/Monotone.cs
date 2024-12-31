using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.DCEL;
using UnityEditor.Build;
using UnityEngine;

namespace Geometry.Monotone
{
    public static class Monotone
    {
        public static IEnumerator MonotoneTriangulation(HalfEdgeData polygon, List<HalfEdgeDebugValue> debugHalfEdgeDebug, float monotoneDealy = 0.5f, float travelDelay = 0.1f,  float travelWaitDelay = 1f, float triangulationDelay = 0.3f)
        {
            MapOverlay.MonotoneOverlay(polygon);
            
             yield return SplitPolygonIntoMonotone(polygon, debugHalfEdgeDebug, monotoneDealy);
            
             List<HalfEdgeFace> faces = new List<HalfEdgeFace>();
            
             for (int i = 0; i < polygon.faces.Count; i++)
             {
                 HalfEdgeFace face = polygon.faces[i];
                 faces.Add(face);
             }
             //
             for (int i = 0; i < faces.Count; i++)
             {
                 yield return HalfEdgeDebug.TravelFaceVertex(polygon, faces[i], debugHalfEdgeDebug, travelDelay);
                 yield return new WaitForSeconds(travelWaitDelay);
             }
             for (int i = 0; i < faces.Count; i++)
             {
                 Debug.Log("트라이 앵글");
                 yield return MonotoneTriangulation(polygon, faces[i], debugHalfEdgeDebug, triangulationDelay);
             } 
             
             for (int i = 0; i < polygon.faces.Count; i++)
             {
                 yield return HalfEdgeDebug.TravelFaceVertex(polygon, polygon.faces[i], debugHalfEdgeDebug, travelDelay);
                 yield return new WaitForSeconds(travelWaitDelay);
             }
            HalfEdgeDebug.DebugHalfEdgeData(polygon);
            yield return null;
        }

        public static IEnumerator MonotoneTriangulation(HalfEdgeData halfEdgeData, HalfEdgeFace monotoneFace, List<HalfEdgeDebugValue> debugHalfEdgeDebug, float delay = 1)
        {
             SortedSet<HalfEdge> sortedEdges = new SortedSet<HalfEdge>(new MonotoneEdgeTriangulationComparer());

            List<HalfEdge> edges = new List<HalfEdge>();
            edges.AddRange(monotoneFace.GetOuterEdges());
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
                 debugHalfEdgeDebug.Clear();
                 debugHalfEdgeDebug.Add(new HalfEdgeDebugValue(uj.vertex.Coordinate));
                 int sortedVertexChain = chainDic[uj];
                 yield return new WaitForSeconds(delay);
                 int stackVertexChain = chainDic[s.First.Value];
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
                         }
                         debugHalfEdgeDebug.Clear();
                         debugHalfEdgeDebug.Add(new HalfEdgeDebugValue(uj.vertex.Coordinate));
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
                         halfEdgeData.AddDiagonal(s.Last.Value.vertex, uj.vertex);
                         s.RemoveLast();
                         yield return new WaitForSeconds(delay);
                     }
                     // Q의 오직 하나 남은 vertex랑 연결하기
                     halfEdgeData.AddDiagonal(s.First.Value.vertex, uj.vertex);
                     s.AddFirst(uj);
                 }
             }

            //return halfEdgeData;
            yield return null;
        }

        private static IEnumerator SplitFaceIntoMonotone(HalfEdgeData halfEdgeData, HalfEdgeFace face,
            List<HalfEdgeDebugValue> debugHalfEdgeDebug, float debugDelay = 0.5f)
        {
            MonotoneEdgeComparer monotoneEdgeComparer = new MonotoneEdgeComparer();
            // Q : v를 정렬한다.
            SortedSet<HalfEdge> sortedVertexs = new SortedSet<HalfEdge>(monotoneEdgeComparer);
            List<HalfEdge> ConnectedEdges = new List<HalfEdge>();
            Dictionary<HalfEdge, HalfEdgeUtility.VertexHandleType> typeTable = new Dictionary<HalfEdge, HalfEdgeUtility.VertexHandleType>();
            // polygon의 vertices들은 반대쪽 vertex도 포함되지만 compare에 의해서 위치가 같은 vertex는 삭제시킨다.

            sortedVertexs.UnionWith(face.IncidentEdges);
            ConnectedEdges.AddRange(face.IncidentEdges);
            HalfEdgeUtility.DetermineVertexType(face, ref typeTable);
            
            // T : SweepLine과 교차중인 Edge들만을 넣는다
            SortedSet<HalfEdge> statusEdges = new SortedSet<HalfEdge>(monotoneEdgeComparer);
            
            // 헬퍼로 사용할 딕셔너리
            Dictionary<HalfEdge, HalfEdge> helperDic = new Dictionary<HalfEdge, HalfEdge>();
            foreach (HalfEdge edge in ConnectedEdges)
            {
                // 가장 왼쪽인 버텍스가 0으로 가게하기
                helperDic.Add(edge, null);
            }
            
            // 버텍스 하나씩 탐색
            while (sortedVertexs.Count > 0)
            {
                debugHalfEdgeDebug.Clear();

                foreach (var VARIABLE in statusEdges)
                {
                    debugHalfEdgeDebug.Add(new HalfEdgeDebugValue(VARIABLE.prev.vertex.Coordinate));
                    debugHalfEdgeDebug.Add(new HalfEdgeDebugValue(VARIABLE.vertex.Coordinate));
                    Debug.Log(VARIABLE.prev.vertex + " " +VARIABLE.vertex);
                }

                HalfEdge edge = sortedVertexs.First();
                HalfEdgeVertex vertex = edge.vertex;
                debugHalfEdgeDebug.Add(new HalfEdgeDebugValue(vertex.Coordinate));
                yield return new WaitForSeconds(debugDelay);
                // 이미 사용한 버텍스는 삭제시킨다.
                sortedVertexs.Remove(edge);
                //Handle
                HandleVertex(edge, statusEdges, helperDic, typeTable, halfEdgeData);
            }
        }
        
        /// <summary>
        /// 모든 Face들을 Monotone분할한다.
        /// </summary>
        /// <param name="halfEdgeData"></param>
        /// <param name="debugHalfEdgeDebug"></param>
        /// <param name="debugDelay"></param>
        /// <returns></returns>
        private static IEnumerator SplitPolygonIntoMonotone(HalfEdgeData halfEdgeData, List<HalfEdgeDebugValue> debugHalfEdgeDebug, float debugDelay = 0.5f)
        {
            List<HalfEdgeFace> faces = new List<HalfEdgeFace>();
            for (int i = 0; i < halfEdgeData.faces.Count; i++)
            {
                faces.Add(halfEdgeData.faces[i]);
            }
            for (int i = 0; i < faces.Count; i++)
            {
                yield return SplitFaceIntoMonotone(halfEdgeData, faces[i], debugHalfEdgeDebug, debugDelay);
            }
        }

        private static void HandleVertex(HalfEdge vertexEdge, SortedSet<HalfEdge> statusEdges,
            Dictionary<HalfEdge, HalfEdge> helperDic, Dictionary<HalfEdge, HalfEdgeUtility.VertexHandleType> typeTable, HalfEdgeData halfEdgeData)
        {
            // 시작부분이니 상태추가
            // LeftEdge의 부모를 찾아야한다.

            HalfEdgeVertex vertex = vertexEdge.vertex;
            HalfEdgeUtility.VertexHandleType type = typeTable[vertexEdge];
            if (type == HalfEdgeUtility.VertexHandleType.START)
            {
                HalfEdge leftEdge = vertexEdge.LeftEdge();
                statusEdges.Add(leftEdge);

                HalfEdge helperVertex = FindHelper(leftEdge, helperDic);
                if (helperVertex != null)
                {
                    halfEdgeData.AddDiagonal(helperVertex.vertex, vertex);
                }

                helperDic[leftEdge] = vertexEdge;
            }
            
            // 마무리를 해야한다.
            if (type == HalfEdgeUtility.VertexHandleType.END)
            {
                HalfEdge leftEdge = vertexEdge.LeftEdge();
                if (leftEdge == null)
                {
                    return;
                }

                HalfEdge helperVertex = FindHelper(leftEdge, helperDic);
                if (helperVertex != null && typeTable[helperVertex] == HalfEdgeUtility.VertexHandleType.MERGE)
                {
                    halfEdgeData.AddDiagonal(helperVertex.vertex, vertex);
                }

                statusEdges.Remove(leftEdge);
            }
            
            // 분할이 필요한 구간
            if (type == HalfEdgeUtility.VertexHandleType.SPLIT)
            {
                // 대각선 추가시 vertex의 우측 대각선이 변경될 수 있으므로 미리 정해놓는다.
                HalfEdge rightEdge = vertexEdge.RightEdge();
                HalfEdge leftEdge = FindAlmostLeftEdge(vertex, statusEdges);

                HalfEdge leftHelperVertex = FindHelper(leftEdge, helperDic);
                halfEdgeData.AddDiagonal(leftHelperVertex.vertex, vertex);
                helperDic[leftEdge] = vertexEdge;
                
                statusEdges.Add(rightEdge);
                helperDic[rightEdge] = vertexEdge;
            }
            else if (type == HalfEdgeUtility.VertexHandleType.MERGE)
            {
                HalfEdge rightEdge = vertexEdge.RightEdge();
                HalfEdge rightHelperVertex = FindHelper(rightEdge, helperDic);
                if (rightHelperVertex != null && typeTable[rightHelperVertex] == HalfEdgeUtility.VertexHandleType.MERGE)
                {
                    halfEdgeData.AddDiagonal(rightHelperVertex.vertex, vertex);
                }

                // right Edge와 가장 가까운 helper를 연결시켜 줫으니 rightEdge는 더이상 건드릴 필요 없다
                statusEdges.Remove(rightEdge);

                HalfEdge leftEdge = FindAlmostLeftEdge(vertex, statusEdges);
                HalfEdge leftHelperVertex = FindHelper(leftEdge, helperDic);
                if (leftHelperVertex != null && typeTable[leftHelperVertex] == HalfEdgeUtility.VertexHandleType.MERGE)
                {
                    halfEdgeData.AddDiagonal(leftHelperVertex.vertex, vertex);
                }
                helperDic[leftEdge] = vertexEdge;
            }
            
            // Refular은
            else if (type == HalfEdgeUtility.VertexHandleType.REGULAR)
            {
                HalfEdgeFace face = vertexEdge.IncidentFace;
                bool isRight = vertexEdge.PolygonInteriorLiesToTheRight(face);

                if (isRight)
                {
                    HalfEdge prevEdge = vertexEdge.prev;
                    HalfEdge nextEdge = vertexEdge.next;
                    HalfEdge aboveEdge = prevEdge.vertex.Coordinate.y >= nextEdge.vertex.Coordinate.y ? vertexEdge : nextEdge;
                    HalfEdge belowEdge = aboveEdge == vertexEdge ? nextEdge : vertexEdge;
                    HalfEdge helper = FindHelper(aboveEdge, helperDic);
                    if (helper != null && typeTable[helper] == HalfEdgeUtility.VertexHandleType.MERGE)
                    {
                        halfEdgeData.AddDiagonal(helper.vertex, vertex);
                    }
                    statusEdges.Remove(aboveEdge);
                    statusEdges.Add(belowEdge);
                    helperDic[belowEdge] = vertexEdge;
                }
                else
                {
                    HalfEdge leftEdge = FindAlmostLeftEdge(vertex, statusEdges);
                    
                    if (leftEdge == null)
                    {
                        return;
                    }
                    statusEdges.Add(leftEdge);
                    HalfEdge helper = FindHelper(leftEdge, helperDic);
                    if (helper == null)
                        return;

                    if (typeTable[helper] == HalfEdgeUtility.VertexHandleType.MERGE)
                    {
                        halfEdgeData.AddDiagonal(helper.vertex, vertex);
                    }

                    helperDic[leftEdge] = vertexEdge;
                }
            }
        }

        // 컴페어를 통해 정점들을높이별로 정렬하고 Edge를 스윕 방법별로 정렬하는데 도움이 된다.
        private class MonotoneVertexComparer : IComparer<HalfEdgeVertex>
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
        private class MonotoneEdgeComparer : IComparer<HalfEdge>
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

        /// <summary>
        /// 정점에서 가장 왼쪽으로 Ray를 쏠때 가장 가까운 Edge를 반환한다.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="sortedEdges"></param>
        /// <returns></returns>
        private static HalfEdge FindAlmostLeftEdge(HalfEdgeVertex vertex, SortedSet<HalfEdge> sortedEdges)
        {
            float minDst = float.MaxValue;
            HalfEdge leftEdge = null;
            foreach (HalfEdge edge in sortedEdges)
            {
                HalfEdgeVertex p1 = edge.vertex;
                HalfEdgeVertex p2 = edge.prev.vertex;
                Debug.Log("래프트찾기" + vertex.Coordinate + " " + sortedEdges.Count);

                Debug.Log(p1.Coordinate + " " + p2.Coordinate);
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
                Debug.DrawLine(vertex.Coordinate, vertex.Coordinate + Vector2.left * minDst, Color.green);
            }

            return leftEdge;
        }

        // 똑같은 가장 왼쪽 Edge를 가진 vertex들중 자신보다 높이있으면서 가장 낮게있는 vertex를 반환한다.
        private static HalfEdge FindHelper(HalfEdge leftEdge, Dictionary<HalfEdge, HalfEdge> dic)
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

            HalfEdge helperVertex = dic[leftEdge];
            return helperVertex;
        }

        private static float DistanceEdgeAndPoint(Vector2 start, Vector2 end, Vector2 p)
        {
            Vector2 edge = end - start;
            Vector2 startTop = p - start;
            return Mathf.Abs(MyMath.Cross2D(edge, startTop)) / edge.magnitude;
        }
        
        
    }
}