using System.Collections.Generic;
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
        HalfEdgeFace face1 , face2;
        // 1. Copy s1, s2 to D

        List<Segment> segments = new List<Segment>();
        Dictionary<int, HalfEdge> halfedgeDic = new Dictionary<int, HalfEdge>();
        int id = 0;
        for (int i = 0; i < D.faces.Count; i++)
        {
            List<HalfEdge> edges = D.faces[i].GetInnerEdges();
            for (int j = 0; j < edges.Count; j++)
            {
                HalfEdge halfEdge = edges[j];
                halfedgeDic.Add(id++, halfEdge);
                Segment segment = new Segment(halfEdge.vertex.Coordinate, halfEdge.prev.vertex.Coordinate, id++);
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
        List<Point> points = Swewep_Line_Algorithm.LS.SweepIntersections(segments, (segment, arg3) =>
        {
            Debug.Log("����" + arg3.ToVector());
            foreach (var VARIABLE in segment)
            {
                Debug.Log("������ : " + VARIABLE.ToString());
            }
            D.vertices.Add(new HalfEdgeVertex( new Vector2(arg3.x, arg3.y)));
        } );
        Debug.Log(points.Count);
    }
}
