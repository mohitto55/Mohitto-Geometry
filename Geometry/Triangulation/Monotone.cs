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
            Debug.Log("���̽� ���� " + monotone.faces.Count);
            for (int i = 0; i < monotone.faces.Count; i++)
            {
                faces.Add(monotone.faces[i]);
            }

            for (int i = 0; i < faces.Count; i++)
            {
                Debug.Log("���̽� " + i + " " + faces[i].GetConnectedEdges().Count);
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
                             Debug.Log("s " + s.Count);
                             Debug.Log("���ͳ��� ������ " + top0.vertex.Coordinate);

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
                         Debug.Log("��¥ ���� " + s.First.Value.vertex.Coordinate + " " + HalfEdgeUtility.GetEdgesAdjacentToAVertexBruteForce(halfEdgeData, s.First.Value.vertex).Count);
                             halfEdgeData.AddDiagonal(s.Last.Value.vertex, uj.vertex);
                         s.RemoveLast();
                         yield return new WaitForSeconds(delay);
                     }
                     // Q�� ���� �ϳ� ���� vertex�� �����ϱ�
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
            // polygon�� vertices���� �ݴ��� vertex�� ���Ե����� compare�� ���ؼ� ��ġ�� ���� vertex�� ������Ų��.
            for (int i = 0; i < halfEdgeData.faces.Count; i++)
            {
                HalfEdgeFace face = halfEdgeData.faces[i];
                sortedVertexs.UnionWith(face.GetConnectedVertices());
                ConnectedEdges.AddRange(HalfEdgeUtility.DetermineVertexType(face));
            }

            MonotoneEdgeComparer monotoneEdgeComparer = new MonotoneEdgeComparer();
            // SweepLine�� �������� Edge�鸸�� �ִ´�
            SortedSet<HalfEdge> statusEdges = new SortedSet<HalfEdge>(monotoneEdgeComparer);

            // ���۷� ����� ��ųʸ�
            Dictionary<HalfEdge, HalfEdgeVertex> helperDic = new Dictionary<HalfEdge, HalfEdgeVertex>();
            foreach (HalfEdge edge in ConnectedEdges)
            {
                // ���� ������ ���ؽ��� 0���� �����ϱ�
                helperDic.Add(edge, null);
            }
            
            // ���ؽ� �ϳ��� Ž��
            while (sortedVertexs.Count > 0)
            {
                HalfEdgeVertex vertex = sortedVertexs.First();
                yield return new WaitForSeconds(debugDelay);
                // �̹� ����� ���ؽ��� ������Ų��.
                sortedVertexs.Remove(vertex);
                debugHalfEdgeDebug.value = vertex.Coordinate;
                //Handle
                HandleVertex(vertex, statusEdges, helperDic, halfEdgeData);
            }
        }

        public static void HandleVertex(HalfEdgeVertex vertex, SortedSet<HalfEdge> statusEdges,
            Dictionary<HalfEdge, HalfEdgeVertex> helperDic, HalfEdgeData halfEdgeData)
        {
            // ���ۺκ��̴� �����߰�
            // LeftEdge�� �θ� ã�ƾ��Ѵ�.
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
            
            // �������� �ؾ��Ѵ�.
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
            
            // ������ �ʿ��� ����
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

                // right Edge�� ���� ����� helper�� ������� �Z���� rightEdge�� ���̻� �ǵ帱 �ʿ� ����
                statusEdges.Remove(rightEdge);

                HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                HalfEdgeVertex leftHelperVertex = FindHelper(leftEdge, helperDic);
                if (leftHelperVertex != null && leftHelperVertex.type == HalfEdgeVertex.Vtype.MERGE)
                {
                    halfEdgeData.AddDiagonal(leftHelperVertex, vertex);
                }

                helperDic[leftEdge] = vertex;
                // // right Edge�� ���� ����� helper�� ������� �Z���� rightEdge�� ���̻� �ǵ帱 �ʿ� ����
                // if(leftEdge != null)
                // statusEdges.Remove(leftEdge);
            }
            
            // Refular��
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

        // ���� ���� �����������̺��� �����ϰ� Edge�� ���� ������� �����ϴµ� ������ �ȴ�.
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

        // ���� ���� ���ʿ� �ִ��� Ȯ���Ѵ�
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

        public static HalfEdge FindLeftEdge(HalfEdgeVertex vertex, SortedSet<HalfEdge> sortedEdges)
        {
            float minDst = float.MaxValue;
            HalfEdge leftEdge = null;
            foreach (HalfEdge edge in sortedEdges)
            {
                HalfEdgeVertex p1 = edge.vertex;
                HalfEdgeVertex p2 = edge.prev.vertex;
                //Debug.Log("����Ʈã��" + vertex.Coordinate + " " + sortedEdges.Count);

                //Debug.Log(p1.Coordinate + " " + p2.Coordinate);
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
                //Debug.DrawLine(vertex.Coordinate, vertex.Coordinate + Vector2.left * minDst, Color.green);
            }

            return leftEdge;
        }

        // �Ȱ��� ���� ���� Edge�� ���� vertex���� �ڽź��� ���������鼭 ���� �����ִ� vertex�� ��ȯ�Ѵ�.
        public static HalfEdgeVertex FindHelper(HalfEdge leftEdge, Dictionary<HalfEdge, HalfEdgeVertex> dic)
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