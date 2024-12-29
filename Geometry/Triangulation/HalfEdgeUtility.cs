using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Monotone;
using NUnit.Framework;
using Swewep_Line_Algorithm;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public static class HalfEdgeUtility
{
    /// <summary>
    /// ����� ����Ŭ Edge���� ��ȯ�Ѵ�.
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public static List<HalfEdge> GetBoundaryEdges(HalfEdge edge)
    {
        if (edge == null) return null;
        List<HalfEdge> BoundaryEdges = new List<HalfEdge>();
        HalfEdge startEdge = edge;
        HalfEdge searchEdge = edge;
        Debug.Log(startEdge + " �ٿ����");

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
    /// �ϳ��� ���ؽ��� �������� Edge�� ����Ǿ� ������ ���ϴ� Face�� ���� Edge�� ���ϴ� ����� Incident Edge�� ������ Edge�� ��ȸ�ϸ�ȴ�.
    /// Incident Edge�� Face�� ���ϴ� Face�� �ƴ϶�� Edge.twin.prev�� ���� vertex�� ����Ű���ִ� �Ǵٸ� Edge�� ã�ư� �� �ִ�.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="face"></param>
    /// <returns></returns>
    public static HalfEdge GetEdgeHasSameFace(HalfEdgeVertex vertex, HalfEdgeFace face)
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
            if (searchEdge.IncidentFace == face)
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


    // public static Segment GetSegment(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    // {
    //     
    // }

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
            if (edge.IncidentFace != null)
            {
                //Debug.Log("vertex1 ������ ���̽� : " + edge.incidentFace.GetHashCode());
                faceSet.Add(edge.IncidentFace);
                edgeMap[edge.IncidentFace] = edge; // face�� �ش��ϴ� edge�� ����
            }
        }

        // vertex2Edges���� ���� face�� ���� edge�� ã��
        foreach (var edge in vertex2Edges)
        {
           //  if(edge.incidentFace != null)
           // // Debug.Log("vertex2 ������ ���̽� : " + edge.incidentFace.GetHashCode());

            if (edge.IncidentFace != null && faceSet.Contains(edge.IncidentFace))
            {
                return new HalfEdge[] {edgeMap[edge.IncidentFace], edge};
            }
        }
        return null;
    }
    
    /// <summary>
    /// vertex2�� ����Ű�� Edge�� �� �����ϱ� ������ Edge�� ��ȯ�Ѵ�. ���ʹ���
    /// </summary>
    /// <param name="vertex1"></param>
    /// <param name="vertex2"></param>
    /// <returns></returns>
    public static HalfEdge GetEdgeBetweenVertices(HalfEdgeVertex vertex1, HalfEdgeVertex vertex2)
    {
        List<HalfEdge> v2Edges = GetEdgesAdjacentToAVertex(vertex2);
        Vector2 v1Tov2 = (vertex2.Coordinate - vertex1.Coordinate);

        float minAngle = 100000;
        HalfEdge answerEdge = null;
        foreach (var edge in v2Edges)
        {
            // ����� v2�� v1�� ���� ������ v2�� v1�� �������̶�� ���̴�. 
            float angle = MyMath.SignedAngle(edge.ToVector2(), v1Tov2);
            angle = angle <= 0 ? 360 + angle : angle;
            if (angle < minAngle)
            {
                minAngle = angle;
                answerEdge = edge;
            }
        }
        return answerEdge;
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
        if (vertex.IncidentEdge == null)
            return list;
        
        HalfEdge searchEdge = vertex.IncidentEdge;
        HalfEdge startEdge = searchEdge;
        Debug.Log(vertex + " ��ƽ��");
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

        } while (searchEdge != null && searchEdge.vertex == vertex && searchEdge != startEdge);
       // Debug.Log("�������� ã�� ����");
        return list;
    }

    /// <summary>
    /// vertex�κ��� ���������� Edge���� ���T����.
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
            // Debug.Log("���� : " + searchEdge.prev.vertex.Coordinate +  " " + searchEdge.vertex.Coordinate + " "
            //           + searchEdge.next.vertex.Coordinate);
            searchEdge = searchEdge.prev.twin;
            // if(searchEdge == vertex.IncidentEdge)
            //     Debug.Log("�������� ���� ����");

        } while (searchEdge != startEdge);
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

    public static bool IsBoundaryCounterClockwise(HalfEdge edge)
    {
        List<Vector2> vector2s = EdgesToVec(GetBoundaryEdges(edge));
        return MyMath.IsCounterClockwise(vector2s);
    }

    /// <summary>
    /// face�� �����ϴ� edge���� CCW �������� ����Ǿ� �ִ��� Ȯ���Ѵ�.
    /// </summary>
    /// <param name="face"></param>
    /// <returns></returns>
    public static bool IsFaceCounterClockwise(HalfEdgeFace face)
    {
        List<Vector2> vector2s = VerticesToVec2(face.GetAdjacentVertices());
        return MyMath.IsCounterClockwise(vector2s);
    }

    public static List<Vector2> VerticesToVec2(List<HalfEdgeVertex> vertices)
    {
        List<Vector2> vector2s = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++)
        {
            vector2s.Add(vertices[i].Coordinate);
        }
        return vector2s;
    }
    public static List<Vector3> VerticesToVec3(List<HalfEdgeVertex> vertices)
    {
        List<Vector3> vector2s = new List<Vector3>();
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
    public static Dictionary<HalfEdge, VertexHandleType> DetermineVertexType(HalfEdgeFace face, ref Dictionary<HalfEdge, VertexHandleType> typeTable)
    {
        if (typeTable == null)
            typeTable = new Dictionary<HalfEdge, VertexHandleType>();
        
        List<HalfEdge> ConnectedEdges = face.IncidentEdges.ToList();
        Debug.Log("ConnectedEdges : " + ConnectedEdges.Count);

        // �ϼ��� ���ؽ����� ��ȸ�ϸ� Ÿ���� ���Ѵ�.
        for (int i = 0; i < ConnectedEdges.Count; i++)
        {
            HalfEdge centerEdge = ConnectedEdges[i];
            HalfEdge prevEdge = centerEdge.prev;
            HalfEdge nextEdge = centerEdge.next;

            float innerCycle = HalfEdgeUtility.CheckInnerCycle(centerEdge);
            bool isOuter = innerCycle > 0;
            if (centerEdge.IncidentFace == null || centerEdge.twin.IncidentFace != null)
            {
                isOuter = false;
            }
            Debug.Log("Type : " + centerEdge + " / " + isOuter.ToString() + " / " + innerCycle);
            if (!typeTable.ContainsKey(centerEdge))
            {
                VertexHandleType type = DetermineType(prevEdge.vertex, ConnectedEdges[i].vertex, nextEdge.vertex, isOuter);
                typeTable.Add(centerEdge, type);
            }
        }
        return typeTable;
    }
    
    /// <summary>
    /// ����Ŭ�� Inner���� Outer���� �Ǻ��Ѵ�.
    /// �ܺ� ��ȯ(outer cycle)�� ���: ���(> 0)
    /// ���� ��ȯ(inner cycle)�� ���: ����(< 0)
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public static float CheckInnerCycle(HalfEdge edge)
    {
        if (edge == null) return 0;
        
        return IsBoundaryCounterClockwise(edge) ? 1 : -1;

        // ���� �ڵ��ε� ������������ ����ɱ� �ƹ� �ǹ� ���� �ڵ忴��.
        // HalfEdge leftEdge = FindLeftmostEdgeInCycle(edge);
        // //IsBoundaryCounterClockwise()
        //
        // Debug.Log(edge + "�� ���� ����" + leftEdge);
        // if (leftEdge == null) return 0;
        
        Vector2 e1 =  edge.vertex.Coordinate - edge.prev.vertex.Coordinate;
        Vector2 e2 =  edge.next.vertex.Coordinate - edge.vertex.Coordinate;
        return (e1.x * e2.y) - (e1.y * e2.x);
    }

    /// <summary>
    /// ����Ŭ���� ���� ���ʿ� �ִ� Edge�� ��ȯ�Ѵ�.
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
                // if (MyMath.CompareFloat(searchEdge.vertex.Coordinate.x, leftEdge.vertex.Coordinate.x) == 0 && 
                //     MyMath.CompareFloat(searchEdge.vertex.Coordinate.y, leftEdge.vertex.Coordinate.y) > 0)
                // {
                //     continue;
                // }
                leftEdge = searchEdge;
            }
            searchEdge = searchEdge.next;
        }
        while (startEdge != searchEdge);

        return leftEdge;
    }
    public static VertexHandleType DetermineType(HalfEdgeVertex prev, HalfEdgeVertex cur, HalfEdgeVertex next, bool outer = true)
    {
        VertexHandleType vertexType;
        Vector2 v1 = prev.Coordinate - cur.Coordinate;
        Vector2 v2 = next.Coordinate - cur.Coordinate;
        // ������ ���ο� �ִٸ� �ݴ�� �����Ų��.
        float angle =  outer ? MyMath.SignedAngle(v1, v2) : -MyMath.SignedAngle(v1, v2);

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
            if (angle >= 0) vertexType = VertexHandleType.START;
            else if (angle <= 0) vertexType = VertexHandleType.SPLIT;
            else
            {
                vertexType = VertexHandleType.START;
            }
        }
        // ����� ������ �ڽź��� ���ٸ�
        else if ((compareBiggerPrevCur > 0 && compareBiggerNextCur > 0))
        {
            // ���ΰ��� 180�� �̸��̰� 
            if (angle >= 0) vertexType = VertexHandleType.END;
            else if (angle <= 0) vertexType = VertexHandleType.MERGE;
            else
            {
                vertexType = VertexHandleType.END;
            }
        }
        // �ϳ��� �Ʒ�, �ϳ��� ���� ���� ���ؽ��� ����ġ���� �ʴ´�.
        else
        {
            vertexType = VertexHandleType.REGULAR;
        }

        if (!outer)
        {
            switch (vertexType)
            {
                // case VertexHandleType.SPLIT:
                //     vertexType = VertexHandleType.START;
                //     break;
                // case VertexHandleType.MERGE:
                //     vertexType = VertexHandleType.END;
                //     break;
                case VertexHandleType.START:
                    vertexType = VertexHandleType.SPLIT;
                    break;
                case VertexHandleType.END:
                    vertexType = VertexHandleType.MERGE;
                    break;
            }
        }
        return vertexType;
    }

    public static Mesh GetMesh(HalfEdgeData data, GeometryOperationType operationType)
    {
        Mesh mesh = new Mesh();
        Dictionary<HalfEdgeVertex, int> verticesIndexTable = new Dictionary<HalfEdgeVertex, int>();
        for (int i = 0; i < data.vertices.Count; i++)
        {
            verticesIndexTable.Add(data.vertices[i], i);
        }
        
        List<int> trianglesIndexList = new List<int>();
        List<HalfEdgeFace> faces = data.faces;
        for (int i = 0; i < faces.Count; i++)
        {
            HalfEdgeFace face = faces[i];
            List<HalfEdge> outerEdges = face.GetOuterEdges();
            if (outerEdges.Count == 3)
            {
                bool oper = false;
                switch (operationType)
                {
                    case GeometryOperationType.INTERSECTION:
                        if (face.shapes.Count % 2 == 0)
                            oper = true;
                        break;
                    case GeometryOperationType.UNION:
                        oper = true;
                        break;
                    case GeometryOperationType.DIFFERENCE:
                        if (face.shapes.Count % 2 == 1)
                            oper = true;
                        break;
                    case GeometryOperationType.XOR:
                        if (face.shapes.Count == 1)
                            oper = true;
                        break;
                    case GeometryOperationType.NONE:
                        oper = false;
                        break;
                }

                if (oper)
                {
                    foreach (var edge in outerEdges)
                    {
                        trianglesIndexList.Add(verticesIndexTable[edge.vertex]);
                    }
                }
            }
        }
        
        mesh.vertices = VerticesToVec3(data.vertices).ToArray();
        if (trianglesIndexList.Count % 3 != 0)
            return mesh;

        List<Color> colorList = new List<Color>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            float value = (float)i / mesh.vertices.Length;
            Color randColor = new Color(value, value, value);
            colorList.Add(randColor);
        }
        mesh.colors = colorList.ToArray();
        mesh.triangles = trianglesIndexList.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        return mesh;
    }
    public enum VertexHandleType
    {
        START,
        END,
        REGULAR,
        SPLIT,
        MERGE,
    }
}

