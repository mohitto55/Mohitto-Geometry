using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monotone;
using UnityEditor.Build;
using UnityEngine;

namespace Monotone
{
    public static class Monotone
    {
        public static IEnumerator MonotoneTriangulation(HalfEdgeData polygon, HalfEdgeDebugValue debugHalfEdgeDebug)
        {
            HalfEdgeData monotone = MakeMonotone(polygon, polygon.faces.First());
            List<HalfEdgeFace> faces = new List<HalfEdgeFace>();
            for (int i = 0; i < monotone.faces.Count; i++)
            {
                faces.Add(monotone.faces[i]);
            }
            // //Debug.Log("���̽� ���� " + monotone.faces.Count);
            for (int i = 0; i < faces.Count; i++)
            {
                Debug.Log("���̽� " + i);
                yield return HalfEdgeDebug.TravelFaceVertex(polygon, faces[i], debugHalfEdgeDebug, 0.1f);
                yield return MonotoneTriangulation(polygon, faces[i], debugHalfEdgeDebug, 1f);
            }
            Debug.Log("�ﰢ���� ��");
            yield return null;
        }

        public static IEnumerator MonotoneTriangulation(HalfEdgeData halfEdgeData, HalfEdgeFace monotoneFace, HalfEdgeDebugValue debugHalfEdgeDebug, float delay = 1)
        {
            //halfEdgeData.AddDiagonal(monotoneFace.OuterComponent.vertex, monotoneFace.OuterComponent.next.next.vertex, monotoneFace);
             SortedSet<HalfEdge> sortedEdges = new SortedSet<HalfEdge>(new MonotoneEdgeTriangulationComparer());
             List<HalfEdge> edges = monotoneFace.GetConnectedEdges();
             //Debug.Log(edges.Count);
             sortedEdges.UnionWith(edges);
             Stack<HalfEdge> s = new Stack<HalfEdge>();
            
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
                     //Debug.Log(searchEdge.vertex.Coordinate + "   " + lastEdge.vertex.Coordinate);
                     
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
             chainDic.Add(sortedEdges.First(), 1);
             chainDic.Add(sortedEdges.Last(), 1);
            
             s.Push(sortedEdges.ElementAt(0));
             s.Push(sortedEdges.ElementAt(1));
             for (int j = 2; j < sortedEdges.Count; j++)
             {
                 HalfEdge uj = sortedEdges.ElementAt(j);
                 debugHalfEdgeDebug.value = uj.vertex.Coordinate;
                 yield return new WaitForSeconds(delay);
                 int sortedVertexChain = chainDic[uj];
                 int stackVertexChain = chainDic[s.Peek()];
                 if (uj != null && sortedVertexChain == stackVertexChain)
                 {
                     s.Push(uj);
                     // ������ ������ �ƴϸ� internal�̴�.
                     float internalAngle = 0;
                     do
                     {
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
                             halfEdgeData.AddDiagonal(top0.vertex, top2.vertex);
                             s.Pop();
                             s.Pop();
                             s.Push(top0);
                         }
                         debugHalfEdgeDebug.value = uj.vertex.Coordinate;
                         yield return new WaitForSeconds(delay);
                     } while (internalAngle >= 0 && s.Count > 2);
                 }
                 else
                 {
                     s.Pop();
                     HalfEdge prev = s.Peek();
                     while (s.Count >= 2)
                     {
                         Debug.Log("��¥ ���� " + s.Peek().vertex.Coordinate + " " + HalfEdgeUtility.GetEdgesAdjacentToAVertex(halfEdgeData, s.Peek().vertex).Count);
                         halfEdgeData.AddDiagonal(s.Peek().vertex, uj.vertex);
                         s.Pop();
                         yield return new WaitForSeconds(delay);
                     }
                     s.Push(prev);
                     s.Push(uj);
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
        public static HalfEdgeData MakeMonotone(HalfEdgeData polygon, HalfEdgeFace face)
        {
            MonotoneVertexComparer monotoneVertexComparer = new MonotoneVertexComparer();
            SortedSet<HalfEdgeVertex> sortedVertexs = new SortedSet<HalfEdgeVertex>(monotoneVertexComparer);
            // polygon�� vertices���� �ݴ��� vertex�� ���Ե����� compare�� ���ؼ� ��ġ�� ���� vertex�� ������Ų��.
            sortedVertexs.UnionWith(polygon.vertices);
            MonotoneEdgeComparer monotoneEdgeComparer = new MonotoneEdgeComparer();
            // SweepLine�� �������� Edge�鸸�� �ִ´�
            SortedSet<HalfEdge> statusEdges = new SortedSet<HalfEdge>(monotoneEdgeComparer);


            // ���۷� ����� ��ųʸ�
            Dictionary<HalfEdge, HalfEdgeVertex> helperDic = new Dictionary<HalfEdge, HalfEdgeVertex>();
            foreach (HalfEdge edge in polygon.edges)
            {
                // ���� ������ ���ؽ��� 0���� �����ϱ�
                helperDic.Add(edge, null);
            }
            
            // ���ؽ� �ϳ��� Ž��
            while (sortedVertexs.Count > 0)
            {
                HalfEdgeVertex vertex = sortedVertexs.First();
                // �̹� ����� ���ؽ��� ������Ų��.
                sortedVertexs.Remove(vertex);

                //Handle
                HandleVertex(vertex, statusEdges, helperDic, polygon, face);
            }
            return polygon;
        }

        public static void HandleVertex(HalfEdgeVertex vertex, SortedSet<HalfEdge> statusEdges,
            Dictionary<HalfEdge, HalfEdgeVertex> helperDic, HalfEdgeData polygon, HalfEdgeFace face)
        {
            // // vertex�� prev vertex�� ���� �ִٸ� �� vertex�� endpoint �ΰ��̴ϱ� �� vertex�� ������ edge�� ���������ְ�
            //    // �ƴ϶�� �Ʒ��ʿ��ִ� edge�ϱ� �߰������ش�.
            //    if (vertex.IncidentEdge.prev.vertex.Coordinate.y > vertex.Coordinate.y)
            //    {
            //        statusEdges.Remove(vertex.IncidentEdge);
            //    }
            //    else
            //    {
            //        statusEdges.Add(vertex.IncidentEdge);
            //    }
            //    // vertex�� ���� edge�� ���� �ִٸ� �� vertex�� endpoint �ΰ��̴ϱ� �� vertex�� next edge�� ���������ְ�
            //    // �ƴ϶�� �Ʒ��ʿ��ִ� edge�ϱ� �߰������ش�.
            //    if (vertex.IncidentEdge.next.vertex.Coordinate.y > vertex.Coordinate.y)
            //    {
            //        statusEdges.Remove(vertex.IncidentEdge.next);
            //    }
            //    else
            //    {
            //        statusEdges.Add(vertex.IncidentEdge.next);
            //    }

            // HalfEdge leftEdge = FindLeftEdge(vertex, sortedEdges);
            // // leftEdge�� �����Ѵٸ�
            // if (leftEdge != null)
            // {
            //     helperDic[leftEdge].Add(vertex);
            // }

            // ���ۺκ��̴� �����߰�
            if (vertex.type == HalfEdgeVertex.Vtype.START)
            {
                HalfEdge leftEdge = vertex.LeftEdge();
                statusEdges.Add(leftEdge);
                helperDic[leftEdge] = vertex;
            }

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
                    polygon.AddDiagonal(helperVertex, vertex);
                }

                statusEdges.Remove(leftEdge);
            }

            if (vertex.type == HalfEdgeVertex.Vtype.SPLIT)
            {
                HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                HalfEdgeVertex helperVertex = FindHelper(leftEdge, helperDic);
                polygon.AddDiagonal(helperVertex, vertex);
                helperDic[leftEdge] = vertex;
                HalfEdge rightEdge = vertex.RightEdge();
                statusEdges.Add(rightEdge);
                helperDic[rightEdge] = vertex;
            }
            else if (vertex.type == HalfEdgeVertex.Vtype.MERGE)
            {
                HalfEdge rightEdge = vertex.RightEdge();
                HalfEdgeVertex rightHelperVertex = FindHelper(rightEdge, helperDic);
                if (rightHelperVertex != null && rightHelperVertex.type == HalfEdgeVertex.Vtype.MERGE)
                {
                    polygon.AddDiagonal(rightHelperVertex, vertex);
                }

                // right Edge�� ���� ����� helper�� ������� �Z���� rightEdge�� ���̻� �ǵ帱 �ʿ� ����
                statusEdges.Remove(rightEdge);

                HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                HalfEdgeVertex leftHelperVertex = FindHelper(leftEdge, helperDic);
                if (leftHelperVertex != null && leftHelperVertex.type == HalfEdgeVertex.Vtype.MERGE)
                {
                    polygon.AddDiagonal(leftHelperVertex, vertex);
                }

                helperDic[leftEdge] = vertex;
                // // right Edge�� ���� ����� helper�� ������� �Z���� rightEdge�� ���̻� �ǵ帱 �ʿ� ����
                // if(leftEdge != null)
                // statusEdges.Remove(leftEdge);
            }
            else if (vertex.type == HalfEdgeVertex.Vtype.REGULAR)
            {
                if (vertex.PolygonInteriorLiesToTheRight())
                {
                    HalfEdge prevEdge = vertex.IncidentEdge.prev;
                    HalfEdge nextEdge = vertex.IncidentEdge.next;
                    HalfEdge aboveEdge = prevEdge.vertex.Coordinate.y > nextEdge.vertex.Coordinate.y ? vertex.IncidentEdge : nextEdge;
                    HalfEdge belowEdge = prevEdge.vertex.Coordinate.y > nextEdge.vertex.Coordinate.y ? nextEdge : vertex.IncidentEdge;
                    HalfEdgeVertex helper = FindHelper(aboveEdge, helperDic);
                    if (helper != null && helper.type == HalfEdgeVertex.Vtype.MERGE)
                    {
                        polygon.AddDiagonal(helper, vertex);
                    }
                    statusEdges.Remove(aboveEdge);
                    statusEdges.Add(belowEdge);
                    helperDic[belowEdge] = vertex;
                }
                else
                {
                    HalfEdge leftEdge = FindLeftEdge(vertex, statusEdges);
                    statusEdges.Add(leftEdge);
                    HalfEdgeVertex helper = helperDic[leftEdge];
                    if (helper.type == HalfEdgeVertex.Vtype.MERGE)
                    {
                        polygon.AddDiagonal(helper, vertex);
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
                if (x.Coordinate == y.Coordinate) return 0;
                if (x.Coordinate.y >= y.Coordinate.y || (x.Coordinate.y == y.Coordinate.y &&
                    x.Coordinate.x <= y.Coordinate.x)) return -1;
                return 1;
            }
        }

        // ���� ���� ���ʿ� �ִ��� Ȯ���Ѵ�
        public class MonotoneEdgeComparer : IComparer<HalfEdge>
        {
            public int Compare(HalfEdge x, HalfEdge y)
            {
                if (x.vertex.Coordinate == y.vertex.Coordinate) return 0;
                if (x.vertex.Coordinate.x < y.vertex.Coordinate.x) return -1;
                return 1;
            }
        }
        
        // ���� ���� ���ʿ� �ִ��� Ȯ���Ѵ�
        public class MonotoneEdgeTriangulationComparer : IComparer<HalfEdge>
        {
            public int Compare(HalfEdge x, HalfEdge y)
            {
                if (x.vertex.Coordinate == y.vertex.Coordinate) return 0;
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
                return null;

            if (!dic.ContainsKey(leftEdge))
                return null;

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