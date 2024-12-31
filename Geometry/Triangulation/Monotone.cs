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
                 Debug.Log("Ʈ���� �ޱ�");
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

            // Stack���δ� �߰��� �ִ� ��Ҹ� ���� �� ��� LinkedList�� �Ѵ�.
            LinkedList<HalfEdge> s = new LinkedList<HalfEdge>();
            
             Dictionary<HalfEdge, int> chainDic = new Dictionary<HalfEdge, int>();
            
             HalfEdge searchEdge = null;
             HalfEdge lastEdge = sortedEdges.Last();
            
            
             // Fisrt Vertex�� next�� RightChain�ϰ��
             Vector2 topVertex = sortedEdges.First().vertex.Coordinate;
             Vector2 prevVertex = sortedEdges.First().prev.vertex.Coordinate;
             Vector2 nextVertex = sortedEdges.First().next.vertex.Coordinate;
             int safeCount = 0;
             if (MyMath.CCW(prevVertex, topVertex, nextVertex) < 0)
             {
                 searchEdge = sortedEdges.First().next;
                 while (lastEdge != searchEdge && safeCount <= sortedEdges.Count)
                 {
                     // �������̸� 1 �����̸� 0
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
             // Fisrt Vertex�� next�� LeftChain�ϰ��
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
             
             // �� �Ʒ� ������ y������ �ι�°�� ���� ���� ������ chain�� ����.
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
                     // ������ ������ �ƴϸ� internal�̴�.
                     float internalAngle = 0;
                     do
                     {
                         if (s.Count <= 2)
                             break;
                         HalfEdge top0 = s.ElementAt(0);
                         HalfEdge top1 = s.ElementAt(1);
                         HalfEdge top2 = s.ElementAt(2);
                         
                         int chain = chainDic[uj];
                         // ���� ü���̳� ������ ü���̳Ŀ�����
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
                         
                         // ������ ������ �ƴϸ� internal�̴�.
                         if (internalAngle >= 0)
                         {
                             // Chain�� �ٲ�� ������ �� ������ ��Ҵ� �̾��� ���� ���̱� ������ �����ش�.
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
                     // Q�� ���� �ϳ� ���� vertex�� �����ϱ�
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
            // Q : v�� �����Ѵ�.
            SortedSet<HalfEdge> sortedVertexs = new SortedSet<HalfEdge>(monotoneEdgeComparer);
            List<HalfEdge> ConnectedEdges = new List<HalfEdge>();
            Dictionary<HalfEdge, HalfEdgeUtility.VertexHandleType> typeTable = new Dictionary<HalfEdge, HalfEdgeUtility.VertexHandleType>();
            // polygon�� vertices���� �ݴ��� vertex�� ���Ե����� compare�� ���ؼ� ��ġ�� ���� vertex�� ������Ų��.

            sortedVertexs.UnionWith(face.IncidentEdges);
            ConnectedEdges.AddRange(face.IncidentEdges);
            HalfEdgeUtility.DetermineVertexType(face, ref typeTable);
            
            // T : SweepLine�� �������� Edge�鸸�� �ִ´�
            SortedSet<HalfEdge> statusEdges = new SortedSet<HalfEdge>(monotoneEdgeComparer);
            
            // ���۷� ����� ��ųʸ�
            Dictionary<HalfEdge, HalfEdge> helperDic = new Dictionary<HalfEdge, HalfEdge>();
            foreach (HalfEdge edge in ConnectedEdges)
            {
                // ���� ������ ���ؽ��� 0���� �����ϱ�
                helperDic.Add(edge, null);
            }
            
            // ���ؽ� �ϳ��� Ž��
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
                // �̹� ����� ���ؽ��� ������Ų��.
                sortedVertexs.Remove(edge);
                //Handle
                HandleVertex(edge, statusEdges, helperDic, typeTable, halfEdgeData);
            }
        }
        
        /// <summary>
        /// ��� Face���� Monotone�����Ѵ�.
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
            // ���ۺκ��̴� �����߰�
            // LeftEdge�� �θ� ã�ƾ��Ѵ�.

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
            
            // �������� �ؾ��Ѵ�.
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
            
            // ������ �ʿ��� ����
            if (type == HalfEdgeUtility.VertexHandleType.SPLIT)
            {
                // �밢�� �߰��� vertex�� ���� �밢���� ����� �� �����Ƿ� �̸� ���س��´�.
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

                // right Edge�� ���� ����� helper�� ������� �Z���� rightEdge�� ���̻� �ǵ帱 �ʿ� ����
                statusEdges.Remove(rightEdge);

                HalfEdge leftEdge = FindAlmostLeftEdge(vertex, statusEdges);
                HalfEdge leftHelperVertex = FindHelper(leftEdge, helperDic);
                if (leftHelperVertex != null && typeTable[leftHelperVertex] == HalfEdgeUtility.VertexHandleType.MERGE)
                {
                    halfEdgeData.AddDiagonal(leftHelperVertex.vertex, vertex);
                }
                helperDic[leftEdge] = vertexEdge;
            }
            
            // Refular��
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

        // ���� ���� �����������̺��� �����ϰ� Edge�� ���� ������� �����ϴµ� ������ �ȴ�.
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

        // ���� ���� ���ʿ� �ִ��� Ȯ���Ѵ�
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
        
        // ���� ���� ���ʿ� �ִ��� Ȯ���Ѵ�
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
        /// �������� ���� �������� Ray�� �� ���� ����� Edge�� ��ȯ�Ѵ�.
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
                Debug.Log("����Ʈã��" + vertex.Coordinate + " " + sortedEdges.Count);

                Debug.Log(p1.Coordinate + " " + p2.Coordinate);
                // Ž������ Edge�� vertex�� �����ϸ� ��������
                if (p1.Coordinate == vertex.Coordinate || p2.Coordinate == vertex.Coordinate)
                    continue;

                // p1�� ������ �����ִ� ������ �����ϱ�
                if (p1.Coordinate.y <= p2.Coordinate.y)
                {
                    HalfEdgeVertex temp = p2;
                    p2 = p1;
                    p1 = temp;
                }

                // p1�� p2 ���̿� vertex�� �����ϴ���
                if (p1.Coordinate.y >= vertex.Coordinate.y && vertex.Coordinate.y >= p2.Coordinate.y)
                {
                    float ccwValue = MyMath.CCW(p1.Coordinate, p2.Coordinate, vertex.Coordinate);
                    // ������ �Ʒ��� �������� ���� ���������� ���ʹ��⿡ ���� ������ �������ΰ���.
                    if (ccwValue < 0)
                        continue;
                    Vector2 intersect = Vector2.one;

                    // ������ ������ �������� �̿��Ѵ�
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

        // �Ȱ��� ���� ���� Edge�� ���� vertex���� �ڽź��� ���������鼭 ���� �����ִ� vertex�� ��ȯ�Ѵ�.
        private static HalfEdge FindHelper(HalfEdge leftEdge, Dictionary<HalfEdge, HalfEdge> dic)
        {
            if (leftEdge == null)
            {
                Debug.LogWarning("LeftEdge�� �����ϴ�.");
                return null;
            }

            if (!dic.ContainsKey(leftEdge))
            {
                Debug.LogWarning("LeftEdge�� ��ϵ��� �ʾҽ��ϴ�.");
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