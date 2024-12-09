using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Monotone;
using UnityEngine;

public static class HalfEdgeDebug
{
    public static IEnumerator CheckEdgeCycle(HalfEdgeData data, HalfEdgeFace face, List<Monotone.Monotone.HalfEdgeDebugValue> point, float delay = 1)
    {
        Debug.Log("여행 시작" );

        yield return TravelFaceVertex(data, face, point, delay);
        HalfEdge searchEdge = face.OuterComponent;
        searchEdge = searchEdge.twin;
        int safe = 0;
        Debug.Log("반대 확인 시작");
        do
        {
            point.Clear();
            point.Add(new Monotone.Monotone.HalfEdgeDebugValue(searchEdge.vertex.Coordinate));
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
        Debug.Log("============Half Edge Data 출력============");
        Debug.Log("총 Vertex : " + data.vertices.Count);
        Debug.Log("총 Edge : " + data.edges.Count);
        Debug.Log("총 face : " + data.faces.Count);

        Debug.Log("============Face 정보 출력============");
        List<HalfEdgeFace> faces = data.faces;
        for (int i = 0; i < faces.Count(); i++)
        {
            HalfEdgeFace f = faces[i];

            Debug.Log(i+"번째 Face");
            Debug.Log("해쉬 : " + f.GetHashCode());
            Debug.Log("Outer Edges들 갯수 : " + f.GetOuterEdges().Count);
            foreach (var VARIABLE in f.GetOuterEdges())
            {
                Debug.Log("Outer Edges : " + VARIABLE.vertex.Coordinate);
            }
            Debug.Log("Inner Edges들 갯수 : " + f.GetInnerEdges().Count);
            foreach (var VARIABLE in f.GetInnerEdges())
            {
                Debug.Log("Inner Edges : " + VARIABLE.vertex.Coordinate);
            }
        }
        
        Debug.Log("============Vertex 정보 출력============");
        List<HalfEdgeVertex> vertices = data.vertices;
        for (int i = 0; i < vertices.Count(); i++)
        {
            HalfEdgeVertex v = vertices[i];
            Debug.Log(i+"번째 Vertex");
            Debug.Log("좌표 : " + v.Coordinate);
            
            
            List<HalfEdge> halfEdges = HalfEdgeUtility.GetEdgesAdjacentToAVertexBruteForce(data, v);
            Debug.Log(i +"번째 Vertex의 인접 Edge 검사");
            Debug.Log("인접 Edge들 브루트포스 탐색 갯수 : " + halfEdges.Count);
            halfEdges = HalfEdgeUtility.GetEdgesAdjacentToAVertex(v);
            Debug.Log("인접 Edge들 인접 검사 탐색 갯수 : " + halfEdges.Count);
            foreach (HalfEdge adjacentEdge in halfEdges)
            {
                Debug.Log("정상 Edge 좌표검사");
                Debug.Log("prev : " + adjacentEdge.prev.vertex.Coordinate + " / cur : " + adjacentEdge.vertex.Coordinate + " / next : " + adjacentEdge.next.vertex.Coordinate);
                Debug.Log("정상 Edge의 face");
                if(adjacentEdge.incidentFace != null)
                    Debug.Log(adjacentEdge.incidentFace.GetHashCode());
                else
                    Debug.Log("Face is Null");

                Debug.Log("반대 Edge 좌표검사");
                Debug.Log("prev : " + adjacentEdge.twin.prev.vertex.Coordinate + " / cur : " + adjacentEdge.twin.vertex.Coordinate + " / next : " + adjacentEdge.twin.next.vertex.Coordinate);
                Debug.Log("반대 Edge의 face");
                if(adjacentEdge.twin.incidentFace != null)
                    Debug.Log(adjacentEdge.twin.incidentFace.GetHashCode());
                else
                    Debug.Log("Face is Null");
            }
        }
    }
}
