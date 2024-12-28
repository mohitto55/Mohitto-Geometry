using System.Collections.Generic;
using System.Linq;
using Monotone;
using NUnit.Framework;
using Swewep_Line_Algorithm;
using Unity.VisualScripting;
using UnityEngine;

public static class HalfEdgeUtility
{
    /// <summary>
    /// 연결된 사이클 Edge들을 반환한다.
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public static List<HalfEdge> GetBoundaryEdges(HalfEdge edge)
    {
        if (edge == null) return null;
        List<HalfEdge> BoundaryEdges = new List<HalfEdge>();
        HalfEdge startEdge = edge;
        HalfEdge searchEdge = edge;

        do
        {
            if(searchEdge == null) return null;
            BoundaryEdges.Add(searchEdge);
            searchEdge = searchEdge.next;
        }
        while (startEdge != searchEdge);

        return BoundaryEdges;
    }
    
    public static List<HalfEdgeVertex> GetBoundaryVertices(HalfEdge edge)
    {
        if (edge == null) return null;
        List<HalfEdgeVertex> BoundaryVertex = new List<HalfEdgeVertex>();
        HalfEdge startEdge = edge;
        HalfEdge searchEdge = edge;

        do
        {
            if(searchEdge == null) return null;
            BoundaryVertex.Add(searchEdge.vertex);
            searchEdge = searchEdge.next;
        }
        while (startEdge != searchEdge);

        return BoundaryVertex;
    }
    /// <summary>
    /// 하나의 버텍스에 여러개의 Edge가 연결되어 있을때 원하는 Face에 속한 Edge를 구하는 방법은 Incident Edge와 인접한 Edge를 순회하면된다.
    /// Incident Edge의 Face가 원하는 Face가 아니라면 Edge.twin.prev는 같은 vertex를 가리키고있는 또다른 Edge를 찾아갈 수 있다.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    public static HalfEdge GetEdgeHasSameFace(HalfEdgeVertex vertex, HalfEdgeFace face)
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


    // public static Segment GetSegment(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    // {
    //     
    // }

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
    
    /// <summary>
    /// 두 vertex를 가리키는 Edge들 중 같은 face를 가지는 Edge 두개를 반환한다.
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    /// <returns></returns>
    public static HalfEdge GetEdgeBetweenVertices(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        //Debug.Log("같은 페이스를 가진 엣지들 찾기 vertex1 : " + vertex1.Coordinate + " vertex2 : " + vertex2.Coordinate);
        List<HalfEdge> v2Edges = GetEdgesAdjacentToAVertex(vertex2);
        //v2Edges.AddRange(GetEdgesSpreadingFromVertex(vertex2));
        Vector2 v1Tov2 = (vertex2.Coordinate - vertex1.Coordinate);

        float minAngle = 100000;
        HalfEdge answerEdge = null;
        foreach (var edge in v2Edges)
        {
            //Vector2 adjEdgePrevDir = (edge.next.vertex.Coordinate - edge.vertex.Coordinate).normalized;
            // 양수면 v2가 v1의 왼쪽 음수면 v2가 v1의 오른쪽이라는 뜻이다. 
            float angle = MyMath.SignedAngle(edge.ToVector2(), v1Tov2);
            angle = angle <= 0 ? 360 + angle : angle;
            //Debug.Log("가까이오는 엣지 v1 :" + vertex1 +" v2: "+vertex2 + " / " + edge + " / Angle : " + angle );
            if (angle < minAngle)
            {
                minAngle = angle;
                answerEdge = edge;
            }
        }
        // Debug.Log("사이 엣지 찾기 : " + vertex1 + " / " + vertex2);
        // if (answerEdge != null)
        // {
        //     Debug.Log("사이 엣지 " + answerEdge);
        //     Debug.Log("답 " + answerEdge);
        //
        // }

        return answerEdge;
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
        if (vertex.IncidentEdge == null)
            return list;
        
        HalfEdge searchEdge = vertex.IncidentEdge;
        HalfEdge startEdge = searchEdge;

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

        } while (searchEdge != startEdge);
       // Debug.Log("인접엣지 찾기 종료");
        return list;
    }

    /// <summary>
    /// vertex로부터 퍼져나가는 Edge들을 반홚나다.
    /// </summary>
    /// <param name="vertex"></param>
    /// <returns></returns>
    public static List<HalfEdge> GetEdgesSpreadingFromVertex(HalfEdgeVertex vertex)
    {
        List<HalfEdge> list = new List<HalfEdge>();
        
        if (vertex.IncidentEdge == null)
            return list;
        
        HalfEdge searchEdge = vertex.IncidentEdge;
        searchEdge = searchEdge.twin;
        HalfEdge startEdge = searchEdge;
        do
        {
            list.Add(searchEdge);
            if (searchEdge.prev.twin == null)
            {
                break;
            }
            // Debug.Log("현재 : " + searchEdge.prev.vertex.Coordinate +  " " + searchEdge.vertex.Coordinate + " "
            //           + searchEdge.next.vertex.Coordinate);
            searchEdge = searchEdge.prev.twin;
            // if(searchEdge == vertex.IncidentEdge)
            //     Debug.Log("인접엣지 같은 엣지");

        } while (searchEdge != startEdge);
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

    public static bool IsBoundaryCounterClockwise(HalfEdge edge)
    {
        List<Vector2> vector2s = EdgesToVec(GetBoundaryEdges(edge));
        return MyMath.IsCounterClockwise(vector2s);
    }

    /// <summary>
    /// face를 참조하는 edge들이 CCW 방향으로 연결되어 있는지 확인한다.
    /// </summary>
    /// <param name="face"></param>
    /// <returns></returns>
    public static bool IsFaceCounterClockwise(HalfEdgeFace face)
    {
        List<Vector2> vector2s = VerticesToVec(face.GetAdjacentVertices());
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
    public static List<Vector2> EdgesToVec(List<HalfEdge> edges)
    {
        List<Vector2> vector2s = new List<Vector2>();
        if (edges == null)
            return vector2s;
        for (int i = 0; i < edges.Count; i++)
        {
            vector2s.Add(edges[i].vertex.Coordinate);
        }
        return vector2s;
    }
    public static List<HalfEdge> DetermineVertexType(HalfEdgeFace face)
    {
        List<HalfEdge> ConnectedEdges = face.GetOuterEdges();
        // 완성된 버텍스들을 순회하며 타입을 정한다.
        for (int i = 0; i < ConnectedEdges.Count; i++)
        {
            int indexPrev = (i - 1 + ConnectedEdges.Count) % ConnectedEdges.Count;
            int indexNext = (i + 1) % ConnectedEdges.Count;
            HalfEdge centerEdge = ConnectedEdges[i];
            bool isOuter = true;
            if (centerEdge.incidentFace != null && centerEdge.twin.incidentFace != null)
            {
                isOuter = false;
            } 
            DetermineType(ConnectedEdges[indexPrev].vertex, ConnectedEdges[i].vertex, ConnectedEdges[indexNext].vertex, isOuter);
        }
        return ConnectedEdges;
    }
    
    /// <summary>
    /// 사이클이 Innder인지 Outer인지 판별한다.
    /// 외부 순환(outer cycle)인 경우: 양수(> 0)
    /// 내부 순환(inner cycle)인 경우: 음수(< 0)
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public static float CheckInnerCycle(HalfEdge edge)
    {
        if (edge == null) return 0;

        HalfEdge leftEdge = FindLeftmostEdgeInCycle(edge);

        if (leftEdge == null) return 0;

        Vector2 e1 =  edge.vertex.Coordinate - edge.prev.vertex.Coordinate;
        Vector2 e2 =  edge.next.vertex.Coordinate - edge.vertex.Coordinate;
        return (e1.x * e2.y) - (e1.y * e2.x);
    }

    /// <summary>
    /// 사이클에서 가장 왼쪽에 있는 Edge를 반환한다.
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public static HalfEdge FindLeftmostEdgeInCycle(HalfEdge edge)
    {
        if (edge == null) return null;
        
        HalfEdge startEdge = edge;
        HalfEdge searchEdge = edge;
        HalfEdge leftEdge = searchEdge;
        do
        {
            if(searchEdge == null) return null;
            if(searchEdge.vertex.Coordinate.x < leftEdge.vertex.Coordinate.x)
            {
                leftEdge = searchEdge;
            }
            searchEdge = searchEdge.next;
        }
        while (startEdge != searchEdge);

        return leftEdge;
    }
    public static void DetermineType(HalfEdgeVertex prev, HalfEdgeVertex cur, HalfEdgeVertex next, bool outer = true)
    {
        Vector2 v1 = prev.Coordinate - cur.Coordinate;
        Vector2 v2 = next.Coordinate - cur.Coordinate;
        // 폴리곤 내부에 있다면 반대로 수행시킨다.
        float angle =  outer ? MyMath.SignedAngle(v1, v2) : -MyMath.SignedAngle(v1, v2);

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

        if (!outer)
        {
            // if (cur.type == HalfEdgeVertex.Vtype.START)
            // {
            //     cur.type = HalfEdgeVertex.Vtype.SPLIT;
            // }
            // else if (cur.type == HalfEdgeVertex.Vtype.END)
            // {
            //     cur.type = HalfEdgeVertex.Vtype.MERGE;
            // }
        }
    }
}

