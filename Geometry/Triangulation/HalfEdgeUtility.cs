using System.Collections.Generic;
using System.Linq;
using Monotone;
using Unity.VisualScripting;
using UnityEngine;

public static class HalfEdgeUtility
{
    /// <summary>
    /// 하나의 버텍스에 여러개의 Edge가 연결되어 있을때 원하는 Face에 속한 Edge를 구하는 방법은 Incident Edge와 인접한 Edge를 순회하면된다.
    /// Incident Edge의 Face가 원하는 Face가 아니라면 Edge.twin.prev는 같은 vertex를 가리키고있는 또다른 Edge를 찾아갈 수 있다.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    public static HalfEdge GetEdgesHasSameFace(HalfEdgeVertex vertex, HalfEdgeFace face)
    {
        Debug.LogWarning("GetEdgesHasSameFace 페이스의 해쉬 : " + face.GetHashCode());
        if (face == null)
        {
            Debug.LogWarning("비교 대상 face가 없습니다.");
            return vertex.IncidentEdge;
        }

        HalfEdge searchEdge = vertex.IncidentEdge;
        if (searchEdge == null)
        {
            Debug.LogWarning("Vertex의 IncidentEdge가 없습니다.");
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
            Debug.Log("순회");
            if (searchEdge.incidentFace != null)
            {
                Debug.Log(searchEdge.vertex.Coordinate);
                Debug.Log(searchEdge.incidentFace.GetHashCode());
            }

            searchEdge = searchEdge.twin.prev;
        } while (searchEdge != vertex.IncidentEdge && vertex != null);
        Debug.LogWarning("맞는 Edge를 찾지 못했습니다.");

        return null;
    }
    
    /// <summary>
    /// 두 vertex를 가리키는 Edge들 중 같은 face를 가지는 Edge 두개를 반환한다.
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    /// <returns></returns>
    public static HalfEdge[] GetEdgesHasSameFace(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        Debug.Log("같은 페이스를 가진 엣지들 찾기 vertex1 : " + vertex1.Coordinate + " vertex2 : " + vertex2.Coordinate);
        List<HalfEdge> vertex1Edges = GetEdgesAdjacentToAVertex(vertex1);
        List<HalfEdge> vertex2Edges = GetEdgesAdjacentToAVertex(vertex2);
        Debug.Log("인접한 엣지들 갯수 vertex1 : " + vertex1Edges.Count + " vertex2 : " + vertex1Edges.Count);

        // vertex1Edges의 각 face를 HashSet에 추가
        var faceSet = new HashSet<HalfEdgeFace>();
        var edgeMap = new Dictionary<HalfEdgeFace, HalfEdge>();

        foreach (var edge in vertex1Edges)
        {
            if (edge.incidentFace != null)
            {
                Debug.Log("vertex1 엣지의 페이스 : " + edge.incidentFace.GetHashCode());
                faceSet.Add(edge.incidentFace);
                edgeMap[edge.incidentFace] = edge; // face에 해당하는 edge를 저장
            }
        }

        // vertex2Edges에서 같은 face를 가진 edge를 찾음
        foreach (var edge in vertex2Edges)
        {
            if(edge.incidentFace != null)
            Debug.Log("vertex2 엣지의 페이스 : " + edge.incidentFace.GetHashCode());

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
    /// vertex에 접근하고 있는 Edge들을 반환한다.
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    public static List<HalfEdge> GetEdgesAdjacentToAVertex(HalfEdgeVertex vertex)
    {
        List<HalfEdge> list = new List<HalfEdge>();
        HalfEdge searchEdge = vertex.IncidentEdge;
        if (vertex.IncidentEdge == null)
            return list;
        Debug.Log(vertex.Coordinate + " " + searchEdge.prev.vertex.Coordinate + " 인접 엣지 찾기 시작");
        int i = 0;
        do
        {
            Debug.Log(i);
            i++;
            list.Add(searchEdge);
            if (searchEdge.next.twin == null)
            {
                Debug.Log("트윈없음");
                break;
            }
            Debug.Log("현재 : " + searchEdge.prev.vertex.Coordinate +  " " + searchEdge.vertex.Coordinate + " "
                      + searchEdge.next.vertex.Coordinate + " " + searchEdge.next.twin.vertex.Coordinate + " " + searchEdge.next.twin.prev.vertex.Coordinate);
            searchEdge = searchEdge.next.twin;
            if(searchEdge == vertex.IncidentEdge)
                Debug.Log("인접엣지 같은 엣지");

        } while (searchEdge != vertex.IncidentEdge);
        Debug.Log("인접엣지 찾기 종료");
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

