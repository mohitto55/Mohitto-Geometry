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

        
        
        // �� �������̿��� ��� ����, �ܺ� �Ǻ��� ����Ѵ�.
        // �̺�Ʈ ����Ʈ�� ���� ���ʿ� �ִ� DCEL�� �����Ѵ�.
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
                            nearLeftEdge = nearLeftEdge.twin;
                            leftID = boundaryTable[nearLeftEdge];
                        }
                        graphG.Merge(leftID, rightID);
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
        IEnumerable<List<HalfEdge>> connectedComponentNodes = graphG.GetConnectedItems();
        int k = 0;
        Debug.Log("connectedComponentNodes " + connectedComponentNodes.Count());

        foreach (var connectedComponents in connectedComponentNodes)
        {
            // C�� �ش� ��������� ������ �ܺ� ��� ��ȯ�̶� �ϰ�
            HalfEdge onlyOuterComponent = null;
            Debug.LogWarning( k + "���� ����" + connectedComponents.Count());
            k++;
            foreach (HalfEdge component in connectedComponents)
            {
                float isOuter = HalfEdgeUtility.CheckInnerCycle(component);
                Debug.LogWarning("Outer Value " + component + " / " + isOuter);

                if (isOuter >= 0)
                {
                    onlyOuterComponent = component;
                    Debug.LogWarning("Outer ������Ʈ �߰�" + connectedComponents.Count);
                    break;
                }
            }
            if (onlyOuterComponent == null)
            {
                Debug.LogWarning("�� �������� ���� ������ Outer Component�� �������� �ʽ��ϴ�.");
                continue;
            }
            Debug.Log("connectedComponents " + connectedComponents.Count);

            // f�� C�� ���� ��� ������ ���̶� �մϴ�
            // f�� ���� ���ڵ带 �����ϰ�
            HalfEdgeFace F = onlyOuterComponent.IncidentFace;
            // C�� ������ �ݺ��� OuterComponent(f)�� �����ϸ�
            // ������� ���� �� Inner Cycle�� ���� �����͵�� ������ ����Ʈ InnerComponents(f)�� �����մϴ�
            // ��������� �� ���� ��ȯ�� ���� IncidentFace(e)�� f�� �����մϴ� (���⼭ e�� ��ȯ�� ������ �ݺ�)
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
