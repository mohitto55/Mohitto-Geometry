using System.Collections.Generic;
using System.Linq;
using Monotone;
using Unity.VisualScripting;
using UnityEngine;

public static class HalfEdgeUtility
{
    /// <summary>
    /// �ϳ��� ���ؽ��� �������� Edge�� ����Ǿ� ������ ���ϴ� Face�� ���� Edge�� ���ϴ� ����� Incident Edge�� ������ Edge�� ��ȸ�ϸ�ȴ�.
    /// Incident Edge�� Face�� ���ϴ� Face�� �ƴ϶�� Edge.twin.prev�� ���� vertex�� ����Ű���ִ� �Ǵٸ� Edge�� ã�ư� �� �ִ�.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    public static HalfEdge GetEdgesHasSameFace(HalfEdgeVertex vertex, HalfEdgeFace face)
    {
        Debug.LogWarning("GetEdgesHasSameFace ���̽��� �ؽ� : " + face.GetHashCode());
        if (face == null)
        {
            Debug.LogWarning("�� ��� face�� �����ϴ�.");
            return vertex.IncidentEdge;
        }

        HalfEdge searchEdge = vertex.IncidentEdge;
        if (searchEdge == null)
        {
            Debug.LogWarning("Vertex�� IncidentEdge�� �����ϴ�.");
            return null;
        }

        do
        {
            if (searchEdge.incidentFace == face)
            {
                return searchEdge;
            }

            if (searchEdge.twin == null)
            {
                return null;
            }
            Debug.Log("��ȸ");
            if (searchEdge.incidentFace != null)
            {
                Debug.Log(searchEdge.vertex.Coordinate);
                Debug.Log(searchEdge.incidentFace.GetHashCode());
            }

            searchEdge = searchEdge.twin.prev;
        } while (searchEdge != vertex.IncidentEdge && vertex != null);
        Debug.LogWarning("�´� Edge�� ã�� ���߽��ϴ�.");

        return null;
    }
    
    /// <summary>
    /// �� vertex�� ����Ű�� Edge�� �� ���� face�� ������ Edge �ΰ��� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    /// <returns></returns>
    public static HalfEdge[] GetEdgesHasSameFace(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        Debug.Log("���� ���̽��� ���� ������ ã�� vertex1 : " + vertex1.Coordinate + " vertex2 : " + vertex2.Coordinate);
        List<HalfEdge> vertex1Edges = GetEdgesAdjacentToAVertex(vertex1);
        List<HalfEdge> vertex2Edges = GetEdgesAdjacentToAVertex(vertex2);
        Debug.Log("������ ������ ���� vertex1 : " + vertex1Edges.Count + " vertex2 : " + vertex1Edges.Count);

        // vertex1Edges�� �� face�� HashSet�� �߰�
        var faceSet = new HashSet<HalfEdgeFace>();
        var edgeMap = new Dictionary<HalfEdgeFace, HalfEdge>();

        foreach (var edge in vertex1Edges)
        {
            if (edge.incidentFace != null)
            {
                Debug.Log("vertex1 ������ ���̽� : " + edge.incidentFace.GetHashCode());
                faceSet.Add(edge.incidentFace);
                edgeMap[edge.incidentFace] = edge; // face�� �ش��ϴ� edge�� ����
            }
        }

        // vertex2Edges���� ���� face�� ���� edge�� ã��
        foreach (var edge in vertex2Edges)
        {
            if(edge.incidentFace != null)
            Debug.Log("vertex2 ������ ���̽� : " + edge.incidentFace.GetHashCode());

            if (edge.incidentFace != null && faceSet.Contains(edge.incidentFace))
            {
                return new HalfEdge[] {edgeMap[edge.incidentFace], edge};
            }
        }

        return null;
    }
    
    public static List<HalfEdgeVertex2> MakeHalfEdge(List<Vector2> positions)
    {
        List<HalfEdgeVertex2> halfEdgeVertexs = new List<HalfEdgeVertex2>();
        
        
        for (int i = 0; i < positions.Count; i++)
        {
            HalfEdgeVertex2 halfEdgeVertex2 = new HalfEdgeVertex2(positions[i]);
            halfEdgeVertexs.Add(halfEdgeVertex2);
            //halfEdgeVertexs[i].edge
        }
        return halfEdgeVertexs;
    }

    /// <summary>
    /// vertex�� �����ϰ� �ִ� Edge���� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    public static List<HalfEdge> GetEdgesAdjacentToAVertex(HalfEdgeVertex vertex)
    {
        List<HalfEdge> list = new List<HalfEdge>();
        HalfEdge searchEdge = vertex.IncidentEdge;
        if (vertex.IncidentEdge == null)
            return list;
        Debug.Log(vertex.Coordinate + " " + searchEdge.prev.vertex.Coordinate + " ���� ���� ã�� ����");
        int i = 0;
        do
        {
            Debug.Log(i);
            i++;
            list.Add(searchEdge);
            if (searchEdge.next.twin == null)
            {
                Debug.Log("Ʈ������");
                break;
            }
            Debug.Log("���� : " + searchEdge.prev.vertex.Coordinate +  " " + searchEdge.vertex.Coordinate + " "
                      + searchEdge.next.vertex.Coordinate + " " + searchEdge.next.twin.vertex.Coordinate + " " + searchEdge.next.twin.prev.vertex.Coordinate);
            searchEdge = searchEdge.next.twin;
            if(searchEdge == vertex.IncidentEdge)
                Debug.Log("�������� ���� ����");

        } while (searchEdge != vertex.IncidentEdge);
        Debug.Log("�������� ã�� ����");
        return list;
    }

    public static List<HalfEdge> GetEdgesAdjacentToAVertex(HalfEdgeData polygon, HalfEdgeVertex vertex)
    {
        return polygon.edges.Where(t=>
        {
            return t.vertex == vertex;
        }).ToList();
    }
}

