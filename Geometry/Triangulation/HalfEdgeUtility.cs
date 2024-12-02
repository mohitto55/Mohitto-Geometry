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
       // Debug.LogWarning("GetEdgesHasSameFace ���̽��� �ؽ� : " + vertex.Coordinate + " " + face.GetHashCode());
        if (face == null)
        {
            Debug.LogWarning("�� ��� face�� �����ϴ�.");
            return vertex.IncidentEdge;
        }

        HalfEdge searchEdge = vertex.IncidentEdge;
        if (searchEdge == null)
        {
            //Debug.LogWarning("Vertex�� IncidentEdge�� �����ϴ�.");
            return null;
        }

        do
        {
            if (searchEdge.incidentFace == face)
            {
                //Debug.Log(vertex.Coordinate + " �Ȱ��� face�� ã�ҽ��ϴ�");
                return searchEdge;
            }

            // -THINKING-
            // searchEdge.twin.prev�� �ϸ� �ȵǴ� ������ ����?
            if (searchEdge.twin.prev == null)
            {
                return null;
            }
            // Debug.Log("��ȸ");
            // Debug.Log(searchEdge.prev.vertex.Coordinate);
            // Debug.Log(searchEdge.vertex.Coordinate);
            // Debug.Log(searchEdge.next.vertex.Coordinate);

            // if (searchEdge.incidentFace != null)
            // {
            //     Debug.Log(searchEdge.vertex.Coordinate);
            //     Debug.Log(searchEdge.incidentFace.GetHashCode());
            // }

            searchEdge = searchEdge.twin.prev;
        } while (searchEdge != vertex.IncidentEdge);
        //Debug.LogWarning("�´� Edge�� ã�� ���߽��ϴ�.");

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
        //Debug.Log("���� ���̽��� ���� ������ ã�� vertex1 : " + vertex1.Coordinate + " vertex2 : " + vertex2.Coordinate);
        List<HalfEdge> vertex1Edges = GetEdgesAdjacentToAVertex(vertex1);
        List<HalfEdge> vertex2Edges = GetEdgesAdjacentToAVertex(vertex2);
        //Debug.Log("������ ������ ���� vertex1 : " + vertex1Edges.Count + " vertex2 : " + vertex2Edges.Count);

        // vertex1Edges�� �� face�� HashSet�� �߰�
        var faceSet = new HashSet<HalfEdgeFace>();
        var edgeMap = new Dictionary<HalfEdgeFace, HalfEdge>();

        foreach (var edge in vertex1Edges)
        {
            if (edge.incidentFace != null)
            {
                //Debug.Log("vertex1 ������ ���̽� : " + edge.incidentFace.GetHashCode());
                faceSet.Add(edge.incidentFace);
                edgeMap[edge.incidentFace] = edge; // face�� �ش��ϴ� edge�� ����
            }
        }

        // vertex2Edges���� ���� face�� ���� edge�� ã��
        foreach (var edge in vertex2Edges)
        {
           //  if(edge.incidentFace != null)
           // // Debug.Log("vertex2 ������ ���̽� : " + edge.incidentFace.GetHashCode());

            if (edge.incidentFace != null && faceSet.Contains(edge.incidentFace))
            {
                return new HalfEdge[] {edgeMap[edge.incidentFace], edge};
            }
        }
        return null;
    }
    
    // �� ���ؽ��� �̹� ����Ǿ� �ִ��� Ȯ���Ѵ�.
    public static bool IsConnectedVertex(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        List<HalfEdge> vertex1Edges = GetEdgesAdjacentToAVertex(vertex1);

        // vertex1Edges�� �� face�� HashSet�� �߰�
        var faceSet = new HashSet<HalfEdgeFace>();
        var edgeMap = new Dictionary<HalfEdgeFace, HalfEdge>();

        foreach (var edge in vertex1Edges)
        {
            if (edge.prev.vertex == vertex2 || edge.next.vertex == vertex2)
                return true;
        }

        List<HalfEdge> vertex2Edges = GetEdgesAdjacentToAVertex(vertex2);
        // vertex2Edges���� ���� face�� ���� edge�� ã��
        foreach (var edge in vertex2Edges)
        {
            if (edge.prev.vertex == vertex1 || edge.next.vertex == vertex1)
                return true;
        }
        return false;
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
        //Debug.Log(vertex.Coordinate + " " + searchEdge.prev.vertex.Coordinate + " ���� ���� ã�� ����");
        do
        {
            list.Add(searchEdge);
            if (searchEdge.next.twin == null)
            {
                break;
            }
            // Debug.Log("���� : " + searchEdge.prev.vertex.Coordinate +  " " + searchEdge.vertex.Coordinate + " "
            //           + searchEdge.next.vertex.Coordinate);
            searchEdge = searchEdge.next.twin;
            // if(searchEdge == vertex.IncidentEdge)
            //     Debug.Log("�������� ���� ����");

        } while (searchEdge != vertex.IncidentEdge);
       // Debug.Log("�������� ã�� ����");
        return list;
    }

    /// <summary>
    /// �� vertex�� ��� �����ϰ� �ִ� Edge���� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    /// <returns></returns>
    public static List<HalfEdge> GetEdgesAdjacentToAVertex(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        List<HalfEdge> list = new List<HalfEdge>();
        HalfEdge searchEdge = vertex1.IncidentEdge;
        if (vertex1.IncidentEdge == null)
            return list;
        
        do
        {
            if((searchEdge.vertex == vertex1 && searchEdge.prev.vertex == vertex2) ||
               (searchEdge.vertex == vertex2 && searchEdge.prev.vertex == vertex1))
                list.Add(searchEdge);
            if (searchEdge.next.twin == null)
            {
                break;
            }
            searchEdge = searchEdge.next.twin;
        } while (searchEdge != vertex1.IncidentEdge);

        return list;
    }

    public static List<HalfEdge> GetEdgesAdjacentToAVertexBruteForce(HalfEdgeData polygon, HalfEdgeVertex vertex)
    {
        return polygon.edges.Where(t=>
        {
            return t.vertex == vertex;
        }).ToList();
    }

    /// <summary>
    /// face�� �����ϴ� edge���� CCW �������� ����Ǿ� �ִ��� Ȯ���Ѵ�.
    /// </summary>
    /// <param name="face"></param>
    /// <returns></returns>
    public static bool IsFaceCounterClockwise(HalfEdgeFace face)
    {
        List<Vector2> vector2s = VerticesToVec(face.GetConnectedVertices());
        return MyMath.IsCounterClockwise(vector2s);
    }

    public static List<Vector2> VerticesToVec(List<HalfEdgeVertex> vertices)
    {
        List<Vector2> vector2s = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++)
        {
            vector2s.Add(vertices[i].Coordinate);
        }

        return vector2s;
    }

    public static List<HalfEdge> DetermineVertexType(HalfEdgeFace face)
    {
        List<HalfEdge> ConnectedEdges = face.GetConnectedEdges();
        // �ϼ��� ���ؽ����� ��ȸ�ϸ� Ÿ���� ���Ѵ�.
        for (int i = 0; i < ConnectedEdges.Count; i++)
        {
            int indexPrev = (i - 1 + ConnectedEdges.Count) % ConnectedEdges.Count;
            int indexNext = (i + 1) % ConnectedEdges.Count;
            DetermineType(ConnectedEdges[indexPrev].vertex, ConnectedEdges[i].vertex, ConnectedEdges[indexNext].vertex);
        }
        return ConnectedEdges;
    }
    
    public static void DetermineType(HalfEdgeVertex prev, HalfEdgeVertex cur, HalfEdgeVertex next)
    {
        Vector2 v1 = prev.Coordinate - cur.Coordinate;
        Vector2 v2 = next.Coordinate - cur.Coordinate;
        float angle = MyMath.SignedAngle(v1, v2);

        Vector2 prevP = prev.Coordinate;
        Vector2 curP = cur.Coordinate;
        Vector2 nextP = next.Coordinate;

        int compareBiggerPrevCur = MyMath.CompareFloat(prevP.y, curP.y);
        int compareBiggerNextCur = MyMath.CompareFloat(nextP.y, curP.y);
        
        // Monotone Composite�� �Ҷ��� Y�� ū��, X�� ���������� ���ĵȴ�.
        // �׷��Ƿ� ���� �����鼭 ���ʺ��� ���ʷ� ���������� �ϸ鼭 �õ��ȴٴ� ���ε�
        // �׷��ϱ� ���� Y�鼭 X�� ������ �� ���� �ִٰ� �Ǵ��� �� �� �ִ�.
        if (compareBiggerPrevCur == 0)
        {
            if(MyMath.CompareFloat(prevP.x, curP.x) < 0)
                prevP.y += 1;
            else
                prevP.y -= 1;
            compareBiggerPrevCur = MyMath.CompareFloat(prevP.y, curP.y);
        }
        if (compareBiggerNextCur == 0)
        {
            if(MyMath.CompareFloat(nextP.x, curP.x) < 0)
                nextP.y += 1;
            else
                nextP.y -= 1;
            compareBiggerNextCur = MyMath.CompareFloat(nextP.y, curP.y);

        }
        
        // ����� ������ �ڽź��� ���ٸ�
        if ((compareBiggerPrevCur < 0 && compareBiggerNextCur < 0))
        {
            if (angle >= 0) cur.type = HalfEdgeVertex.Vtype.START;
            else if (angle <= 0) cur.type = HalfEdgeVertex.Vtype.SPLIT;
            else
            {
                cur.type = HalfEdgeVertex.Vtype.START;
            }
        }
        // ����� ������ �ڽź��� ���ٸ�
        else if ((compareBiggerPrevCur > 0 && compareBiggerNextCur > 0))
        {
            // ���ΰ��� 180�� �̸��̰� 
            if (angle >= 0) cur.type = HalfEdgeVertex.Vtype.END;
            else if (angle <= 0) cur.type = HalfEdgeVertex.Vtype.MERGE;
            else
            {
                cur.type = HalfEdgeVertex.Vtype.END;
            }
        }
        // �ϳ��� �Ʒ�, �ϳ��� ���� ���� ���ؽ��� ����ġ���� �ʴ´�.
        else
        {
            cur.type = HalfEdgeVertex.Vtype.REGULAR;
        }
    }
}

