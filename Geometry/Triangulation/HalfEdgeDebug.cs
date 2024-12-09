using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monotone;
using UnityEngine;

public static class HalfEdgeDebug
{
    public static IEnumerator CheckEdgeCycle(HalfEdgeData data, HalfEdgeFace face, List<Monotone.Monotone.HalfEdgeDebugValue> point, float delay = 1)
    {
        Debug.Log("���� ����" );

        yield return TravelFaceVertex(data, face, point, delay);
        HalfEdge searchEdge = face.OuterComponent;
        searchEdge = searchEdge.twin;
        int safe = 0;
        Debug.Log("�ݴ� Ȯ�� ����");
        do
        {
            point.Clear();
            point.Add(new Monotone.Monotone.HalfEdgeDebugValue(searchEdge.vertex.Coordinate));
            searchEdge = searchEdge.next;
            safe++;
            if (safe > 100000000000)
            {
                Debug.LogError("�������ƿ�");
                break;
            }

            if (searchEdge == null)
            {
                Debug.LogError("������ ���� �ֽ��ϴ�.");
                break;
            }
            yield return new WaitForSeconds(delay);
        } while (searchEdge != face.OuterComponent.twin);
        Debug.Log("�ݴ��� ���� " + safe);
    }
    public static IEnumerator TravelFaceVertex(HalfEdgeData data, HalfEdgeFace face, List<Monotone.Monotone.HalfEdgeDebugValue> point, float delay = 1)
    {
        List<HalfEdge> edges = face.GetOuterEdges();
        for (int i = 0; i < edges.Count; i++)
        {
            point.Clear();
            point.Add(new Monotone.Monotone.HalfEdgeDebugValue(edges[i].vertex.Coordinate, Color.green));
            Debug.Log("<color=green>"+ edges[i].vertex.Coordinate + " "+ edges[i].next.vertex.Coordinate +  " " + edges[i].twin.vertex.Coordinate +"</color>");
            Debug.Log("<color=green>"+ edges[i].incidentFace.GetHashCode() +"</color>");

            yield return new WaitForSeconds(delay);
        }
    }

    public static void DebugHalfEdgeData(HalfEdgeData data)
    {
        Debug.Log("============Half Edge Data ���============");
        Debug.Log("�� Vertex : " + data.vertices.Count);
        Debug.Log("�� Edge : " + data.edges.Count);
        Debug.Log("�� face : " + data.faces.Count);

        Debug.Log("============Face ���� ���============");
        List<HalfEdgeFace> faces = data.faces;
        for (int i = 0; i < faces.Count(); i++)
        {
            HalfEdgeFace f = faces[i];

            Debug.Log(i+"��° Face");
            Debug.Log("�ؽ� : " + f.GetHashCode());
            Debug.Log("Outer Edges�� ���� : " + f.GetOuterEdges().Count);
            foreach (var VARIABLE in f.GetOuterEdges())
            {
                Debug.Log("Outer Edges : " + VARIABLE.vertex.Coordinate);
            }
            Debug.Log("Inner Edges�� ���� : " + f.GetInnerEdges().Count);
            foreach (var VARIABLE in f.GetInnerEdges())
            {
                Debug.Log("Inner Edges : " + VARIABLE.vertex.Coordinate);
            }
        }
        
        Debug.Log("============Vertex ���� ���============");
        List<HalfEdgeVertex> vertices = data.vertices;
        for (int i = 0; i < vertices.Count(); i++)
        {
            HalfEdgeVertex v = vertices[i];
            Debug.Log(i+"��° Vertex");
            Debug.Log("��ǥ : " + v.Coordinate);
            
            
            List<HalfEdge> halfEdges = HalfEdgeUtility.GetEdgesAdjacentToAVertexBruteForce(data, v);
            Debug.Log(i +"��° Vertex�� ���� Edge �˻�");
            Debug.Log("���� Edge�� ���Ʈ���� Ž�� ���� : " + halfEdges.Count);
            halfEdges = HalfEdgeUtility.GetEdgesAdjacentToAVertex(v);
            Debug.Log("���� Edge�� ���� �˻� Ž�� ���� : " + halfEdges.Count);
            foreach (HalfEdge adjacentEdge in halfEdges)
            {
                Debug.Log("���� Edge ��ǥ�˻�");
                Debug.Log("prev : " + adjacentEdge.prev.vertex.Coordinate + " / cur : " + adjacentEdge.vertex.Coordinate + " / next : " + adjacentEdge.next.vertex.Coordinate);
                Debug.Log("���� Edge�� face");
                if(adjacentEdge.incidentFace != null)
                    Debug.Log(adjacentEdge.incidentFace.GetHashCode());
                else
                    Debug.Log("Face is Null");

                Debug.Log("�ݴ� Edge ��ǥ�˻�");
                Debug.Log("prev : " + adjacentEdge.twin.prev.vertex.Coordinate + " / cur : " + adjacentEdge.twin.vertex.Coordinate + " / next : " + adjacentEdge.twin.next.vertex.Coordinate);
                Debug.Log("�ݴ� Edge�� face");
                if(adjacentEdge.twin.incidentFace != null)
                    Debug.Log(adjacentEdge.twin.incidentFace.GetHashCode());
                else
                    Debug.Log("Face is Null");
            }
        }
    }
}
