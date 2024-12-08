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
    /// �� �������̸� �ϴ� �Լ�
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
        //Debug.Log("�� ���׸�Ʈ ī��Ʈ" + segments.Count);
        // 2. Compute all intersections between edges from S1 and S2

        // ���� 2.1�� plane sweep �˰����� ����Ͽ� S1�� S2�� ���� ������ ��� ������ ����Ѵ�.
        // �̺�Ʈ ����Ʈ���� �ʿ��� T�� Q�� ���� �۾� �ܿ��� ������ �����Ѵ�:
        //    S1�� S2�� ������ ��� ���õ� �̺�Ʈ�� ��� D�� ������Ʈ�Ѵ�.
        //    (�̴� S1�� ������ S2�� ������ ����ϴ� ��쿡 ���� ����Ǿ���.)
        //- �̺�Ʈ ����Ʈ�� ���ʿ� �ִ� �ݿ����� D�� �ش� ������ �����Ѵ�.

        
        
        
        Dictionary<Point, Segment> slTable;
        List<Point> points = Swewep_Line_Algorithm.LS.SweepIntersections(D, segments, out slTable,(segment, arg3) =>
        {
            Debug.Log("����" + arg3.ToVector());
            foreach (var VARIABLE in segment)
            {
                Debug.Log("������ : " + VARIABLE.ToString());
            }
            D.vertices.Add(new HalfEdgeVertex( new Vector2(arg3.x, arg3.y)));
        } );

        Debug.Log("���м��� ���� ���� �� ã��" + slTable.Count);
        foreach (var VARIABLE in slTable)
        {
            Debug.Log(VARIABLE.Key.ToVector() + " SEGMENT : " + VARIABLE.Value.Start.ToVector() + " " + VARIABLE.Value.End.ToVector() );
        }
        
        foreach (var VARIABLE in halfedgeDic)
        {
            Debug.Log(VARIABLE.Key.ToString() + "���ڱ�" );
        }



        // D�� Ž���Ͽ� O(S1, S2)�� ��� ����Ŭ�� �����Ѵ�.
        // �� ��� ����Ŭ�� ���� ��尡 �����Ǵ� �׷��� G�� �����Ѵ�. 
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
            Debug.Log("���� ���� Inner ����" + leftEdge);


            // p�� ���� ���� ���׸�Ʈ�� ã�´�.
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
                    
                    Debug.Log(leftEdge.vertex.Coordinate + " �� ���� ���׸�Ʈ : " + sameVertex.First().Value);
                    Debug.Log(leftEdge.vertex.Coordinate + " �� ���� ���� : " + edge);
                    Debug.Log("���� " + HalfEdgeUtility.CheckInnerCycle(edge));
                    float isInner = HalfEdgeUtility.CheckInnerCycle(edge);
                    
                    // ������ Inner
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
                                Debug.Log("����Ʈ ����");
                            }
                            if (!boundaryTable.ContainsKey(leftEdge))
                            {
                                Debug.Log("����Ʈ ����");
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
            // C�� �ش� ��������� ������ �ܺ� ��� ��ȯ�̶� �ϰ�
            HalfEdge C = null;
            foreach (HalfEdge component in connectedComponents)
            {
                float isOuter = HalfEdgeUtility.CheckInnerCycle(component);
                if (isOuter <= 0)
                {
                    C = component;
                    Debug.LogWarning("Outer ������Ʈ �߰�");
                    break;
                }
            }
            if (C == null)
            {
                Debug.LogWarning("�� �������� ���� ������ Outer Component�� �������� �ʽ��ϴ�.");
                continue;
            }
            
            // f�� C�� ���� ��� ������ ���̶� �մϴ�
            // f�� ���� ���ڵ带 �����ϰ�
            HalfEdgeFace F = C.incidentFace;
            // C�� ������ �ݺ��� OuterComponent(f)�� �����ϸ�
            // ������� ���� �� Inner Cycle�� ���� �����͵�� ������ ����Ʈ InnerComponents(f)�� �����մϴ�
            // ��������� �� ���� ��ȯ�� ���� IncidentFace(e)�� f�� �����մϴ� (���⼭ e�� ��ȯ�� ������ �ݺ�)
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
