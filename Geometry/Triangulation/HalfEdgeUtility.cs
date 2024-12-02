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
       // Debug.LogWarning("GetEdgesHasSameFace 페이스의 해쉬 : " + vertex.Coordinate + " " + face.GetHashCode());
        if (face == null)
        {
            Debug.LogWarning("비교 대상 face가 없습니다.");
            return vertex.IncidentEdge;
        }

        HalfEdge searchEdge = vertex.IncidentEdge;
        if (searchEdge == null)
        {
            //Debug.LogWarning("Vertex의 IncidentEdge가 없습니다.");
            return null;
        }

        do
        {
            if (searchEdge.incidentFace == face)
            {
                //Debug.Log(vertex.Coordinate + " 똑같은 face를 찾았습니다");
                return searchEdge;
            }

            // -THINKING-
            // searchEdge.twin.prev로 하면 안되는 이유가 뭘까?
            if (searchEdge.twin.prev == null)
            {
                return null;
            }
            // Debug.Log("순회");
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
        //Debug.LogWarning("맞는 Edge를 찾지 못했습니다.");

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
        //Debug.Log("같은 페이스를 가진 엣지들 찾기 vertex1 : " + vertex1.Coordinate + " vertex2 : " + vertex2.Coordinate);
        List<HalfEdge> vertex1Edges = GetEdgesAdjacentToAVertex(vertex1);
        List<HalfEdge> vertex2Edges = GetEdgesAdjacentToAVertex(vertex2);
        //Debug.Log("인접한 엣지들 갯수 vertex1 : " + vertex1Edges.Count + " vertex2 : " + vertex2Edges.Count);

        // vertex1Edges의 각 face를 HashSet에 추가
        var faceSet = new HashSet<HalfEdgeFace>();
        var edgeMap = new Dictionary<HalfEdgeFace, HalfEdge>();

        foreach (var edge in vertex1Edges)
        {
            if (edge.incidentFace != null)
            {
                //Debug.Log("vertex1 엣지의 페이스 : " + edge.incidentFace.GetHashCode());
                faceSet.Add(edge.incidentFace);
                edgeMap[edge.incidentFace] = edge; // face에 해당하는 edge를 저장
            }
        }

        // vertex2Edges에서 같은 face를 가진 edge를 찾음
        foreach (var edge in vertex2Edges)
        {
           //  if(edge.incidentFace != null)
           // // Debug.Log("vertex2 엣지의 페이스 : " + edge.incidentFace.GetHashCode());

            if (edge.incidentFace != null && faceSet.Contains(edge.incidentFace))
            {
                return new HalfEdge[] {edgeMap[edge.incidentFace], edge};
            }
        }
        return null;
    }
    
    // 두 버텍스가 이미 연결되어 있는지 확인한다.
    public static bool IsConnectedVertex(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        List<HalfEdge> vertex1Edges = GetEdgesAdjacentToAVertex(vertex1);

        // vertex1Edges의 각 face를 HashSet에 추가
        var faceSet = new HashSet<HalfEdgeFace>();
        var edgeMap = new Dictionary<HalfEdgeFace, HalfEdge>();

        foreach (var edge in vertex1Edges)
        {
            if (edge.prev.vertex == vertex2 || edge.next.vertex == vertex2)
                return true;
        }

        List<HalfEdge> vertex2Edges = GetEdgesAdjacentToAVertex(vertex2);
        // vertex2Edges에서 같은 face를 가진 edge를 찾음
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
        //Debug.Log(vertex.Coordinate + " " + searchEdge.prev.vertex.Coordinate + " 인접 엣지 찾기 시작");
        do
        {
            list.Add(searchEdge);
            if (searchEdge.next.twin == null)
            {
                break;
            }
            // Debug.Log("현재 : " + searchEdge.prev.vertex.Coordinate +  " " + searchEdge.vertex.Coordinate + " "
            //           + searchEdge.next.vertex.Coordinate);
            searchEdge = searchEdge.next.twin;
            // if(searchEdge == vertex.IncidentEdge)
            //     Debug.Log("인접엣지 같은 엣지");

        } while (searchEdge != vertex.IncidentEdge);
       // Debug.Log("인접엣지 찾기 종료");
        return list;
    }

    /// <summary>
    /// 두 vertex에 모두 접근하고 있는 Edge들을 반환한다.
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
    /// face를 참조하는 edge들이 CCW 방향으로 연결되어 있느지 확인한다.
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
        // 완성된 버텍스들을 순회하며 타입을 정한다.
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
        
        // Monotone Composite를 할때는 Y가 큰순, X가 작은순으로 정렬된다.
        // 그러므로 위에 있으면서 왼쪽부터 차례로 스윕라인을 하면서 시도된다는 뜻인데
        // 그러니까 같은 Y면서 X가 작은게 더 높이 있다고 판단을 할 수 있다.
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
        
        // 양방향 노드들이 자신보다 낮다면
        if ((compareBiggerPrevCur < 0 && compareBiggerNextCur < 0))
        {
            if (angle >= 0) cur.type = HalfEdgeVertex.Vtype.START;
            else if (angle <= 0) cur.type = HalfEdgeVertex.Vtype.SPLIT;
            else
            {
                cur.type = HalfEdgeVertex.Vtype.START;
            }
        }
        // 양방향 노드들이 자신보다 높다면
        else if ((compareBiggerPrevCur > 0 && compareBiggerNextCur > 0))
        {
            // 내부각이 180도 미만이고 
            if (angle >= 0) cur.type = HalfEdgeVertex.Vtype.END;
            else if (angle <= 0) cur.type = HalfEdgeVertex.Vtype.MERGE;
            else
            {
                cur.type = HalfEdgeVertex.Vtype.END;
            }
        }
        // 하나는 아래, 하나는 위로 가는 버텍스로 스위치하지 않는다.
        else
        {
            cur.type = HalfEdgeVertex.Vtype.REGULAR;
        }
    }
}

