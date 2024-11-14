using System.Collections;
using System.Collections.Generic;
using Monotone;
using UnityEngine;

public static class HalfEdgeDebug
{
    public static IEnumerator CheckEdgeCycle(HalfEdgeData data, HalfEdgeFace face, Monotone.Monotone.HalfEdgeDebugValue point, float delay = 1)
    {
        Debug.Log("���� ����" );

        yield return TravelFaceVertex(data, face, point, delay);
        HalfEdge searchEdge = face.OuterComponent;
        searchEdge = searchEdge.twin;
        int safe = 0;
        Debug.Log("�ݴ� Ȯ�� ����");
        do
        {
            point.value = searchEdge.vertex.Coordinate;
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
    public static IEnumerator TravelFaceVertex(HalfEdgeData data, HalfEdgeFace face, Monotone.Monotone.HalfEdgeDebugValue point, float delay = 1)
    {
        List<HalfEdge> edges = face.GetConnectedEdges();
        for (int i = 0; i < edges.Count; i++)
        {
            point.value = edges[i].vertex.Coordinate;
            Debug.Log("<color=green>"+ edges[i].vertex.Coordinate + " "+ edges[i].next.vertex.Coordinate +  " " + edges[i].twin.vertex.Coordinate +"</color>");

            Debug.Log("<color=green>"+ edges[i].incidentFace.GetHashCode() +"</color>");

            yield return new WaitForSeconds(delay);
        }
    }
}
