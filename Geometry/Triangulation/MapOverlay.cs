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
                segments.Add(segment);
                id++;
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
        //  �� hole ����Ŭ�� ���� ���� ������ ���ʿ� �ִ� ����Ŭ(Inner Component)�� �����ϰ�,
        //  �̵��� ����� ������Ʈ�� ����Ѵ�.  
        //  (2�� �ܰ��� �� ��° �׸񿡼� G�� arc�� �����ϴ� ������ ���Ǿ���.)
        for (int i = 0; i < D.faces.Count; i++)
        {
            HalfEdge innerComponent = D.faces[i].InnerComponent;

            // ���ۿ��� ���� ������ ����
            HalfEdge holeLeftEdge = HalfEdgeUtility.FindLeftmostEdgeInCycle(innerComponent);
            Debug.Log(i + " ���� ���� Inner ����" + holeLeftEdge);


            // p�� ���� ���� ���׸�Ʈ�� ã�´�.
            var leftVertexEnumerable = slTable.Where(t =>
            {
                return MathUtility.FloatZero(t.Key.x - holeLeftEdge.vertex.Coordinate.x) &&
                       MathUtility.FloatZero(t.Key.y - holeLeftEdge.vertex.Coordinate.y);
            });
            
            
            if (leftVertexEnumerable.Count() > 0 && leftVertexEnumerable.First().Key != null)
            {
                Point point = leftVertexEnumerable.First().Key;
                Segment segment = leftVertexEnumerable.First().Value;
                // sl���̺�(������ ���� ����� ���� edge)�� segment�� �ش��ϴ� edge�� �����´�.
                if (halfedgeDic.ContainsKey(segment.num))
                {
                    HalfEdge nearLeftEdge = halfedgeDic[segment.num];
                    
                    Debug.Log(holeLeftEdge.vertex.Coordinate + " �� ���� ���׸�Ʈ : " + leftVertexEnumerable.First().Value);
                    Debug.Log(holeLeftEdge.vertex.Coordinate + " �� ���� ���� : " + nearLeftEdge);
                    Debug.Log("���� " + HalfEdgeUtility.CheckInnerCycle(nearLeftEdge));
                    float isInner = HalfEdgeUtility.CheckInnerCycle(nearLeftEdge);
                    
                    // ����� outer ������ inner
                    // ������ inner�� �����
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
                            Debug.Log("�ݴ��");
                            nearLeftEdge = nearLeftEdge.twin;
                            leftID = boundaryTable[nearLeftEdge];
                        }
                        Debug.Log("Left ID " + leftID + " / Right ID : " + rightID);
                        graphG.Merge(leftID, rightID);
                        Debug.Log(graphG);
                    }
                    else
                    {
                        // ���� edge�� ���ٸ� ���Ѵ� ���� ��ģ��.
                        int leftID = boundaryTable[holeLeftEdge];
                        graphG.Merge(leftID, 0);
                        if (!boundaryTable.ContainsKey(nearLeftEdge))
                        {
                            Debug.Log("����Ʈ ����");
                        }
                        if (!boundaryTable.ContainsKey(holeLeftEdge))
                        {
                            Debug.Log("����Ʈ ����");
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
            // C�� �ش� ��������� ������ �ܺ� ��� ��ȯ�̶� �ϰ�
            HalfEdge C = null;
            Debug.LogWarning( k + "���� ����");
            k++;
            foreach (HalfEdge component in connectedComponents)
            {
                float isOuter = HalfEdgeUtility.CheckInnerCycle(component);
                if (isOuter >= 0)
                {
                    C = component;
                    Debug.LogWarning("Outer ������Ʈ �߰�" + connectedComponents.Count);
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
