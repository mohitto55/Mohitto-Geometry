using System.Collections;
using System.Collections.Generic;
using Monotone;
using UnityEngine;

public static class HalfEdgeDebug
{
    public static IEnumerator CheckEdgeCycle(HalfEdgeData data, HalfEdgeFace face, Monotone.Monotone.HalfEdgeDebugValue point, float delay = 1)
    {
        Debug.Log("여행 시작" );

        yield return TravelFaceVertex(data, face, point, delay);
        HalfEdge searchEdge = face.OuterComponent;
        searchEdge = searchEdge.twin;
        int safe = 0;
        Debug.Log("반대 확인 시작");
        do
        {
            point.value = searchEdge.vertex.Coordinate;
            searchEdge = searchEdge.next;
            safe++;
            if (safe > 100000000000)
            {
                Debug.LogError("세이프아웃");
                break;
            }

            if (searchEdge == null)
            {
                Debug.LogError("엣지가 끊겨 있습니다.");
                break;
            }
            yield return new WaitForSeconds(delay);
        } while (searchEdge != face.OuterComponent.twin);
        Debug.Log("반대편 갯수 " + safe);
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
