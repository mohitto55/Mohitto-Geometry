using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Monotone
{
    public class HalfEdgeData
    {
        public List<HalfEdgeVertex> vertices = new List<HalfEdgeVertex>();
        public List<HalfEdge> edges = new List<HalfEdge>();
        public List<HalfEdgeFace> faces = new List<HalfEdgeFace>();

        public HalfEdgeData(List<Vector2> points)
        {
            int size = points.Count;
            HalfEdgeFace face = new HalfEdgeFace();
            faces.Add(face);

            HalfEdge prevLeftEdge = null;
            HalfEdge prevRightEdge = null;

            for (int i = 0; i < size; i++)
            {
                Vector2 point = points[i];

                // 이거는 버텍스틑 하나만 만드네? 원래 같은 버텍스를 여러개 만들어야하는데
                // 다른 자료들 보니 꼭 같은 정점을 두개 만들 필요는 없어보인다 어떻게 쓰느냐가 중요한듯
                HalfEdgeVertex vertex = new HalfEdgeVertex(point);
                HalfEdge left = new HalfEdge(vertex);
                HalfEdge right = new HalfEdge(vertex);

                left.incidentFace = face;
                left.next = null;
                left.vertex = vertex;
                left.twin = right;
                
                right.incidentFace = null;
                right.next = prevRightEdge;
                right.vertex = null;
                right.twin = left;
                
                edges.Add(left);
                edges.Add(right);

                vertex.IncidentEdge = left;
                vertices.Add(vertex);
                
                if (prevLeftEdge != null)
                {
                    prevLeftEdge.next = left;
                    left.prev = prevLeftEdge;
                }
                
                if (prevRightEdge != null)
                {
                    prevRightEdge.vertex = vertex;
                    prevRightEdge.prev = right;
                }
                
                prevLeftEdge = left;
                prevRightEdge = right;
            }
            
            HalfEdge firstLeftEdge = edges[0];
            prevLeftEdge.next = firstLeftEdge;
            firstLeftEdge.prev = prevLeftEdge;
                
            HalfEdge firstRightEdge = edges[1];
            firstRightEdge.next = prevRightEdge;
            prevRightEdge.prev = firstRightEdge;

            prevRightEdge.vertex = vertices[0];
            face.OuterComponent = firstLeftEdge;
            
            // 완성된 버텍스들을 순회하며 타입을 정한다.
            for (int i = 0; i < vertices.Count; i++)
            {
                int indexPrev = (i - 1 + vertices.Count) % vertices.Count;
                int indexNext = (i + 1) % vertices.Count;
                vertices[i].DetermineType(vertices[indexPrev], vertices[indexNext]);
            }
        }

        // 대각선추가
        public void AddDiagonal(HalfEdgeVertex upper, HalfEdgeVertex lower)
        {
            if (upper == null || lower == null)
                return;
            if (upper.IncidentEdge.vertex.Coordinate.y < lower.IncidentEdge.vertex.Coordinate.y)
            {
                HalfEdgeVertex temp = lower;
                lower = upper;
                upper = temp;
            }
            // 왼쪽 원점을 아래쪽 오른쪽을 위로해서 각각 자체를 쌍둥이로 표현할 수 있다.
            // 대각선으로 할 새로운 edge를 만든다
            HalfEdge leftEdge = new HalfEdge();
            HalfEdge rightEdge = new HalfEdge();

            leftEdge.vertex = lower;
            rightEdge.vertex = upper;

            leftEdge.twin = rightEdge;
            rightEdge.twin = leftEdge;

            // 새롭게 대각선을 만드니 이전 엣지들의 다음 vertex와 edge를 업데이트 해줘야한다.
            HalfEdge prevLowerEdge = lower.IncidentEdge.prev;
            HalfEdge prevUpperEdge = upper.IncidentEdge.prev;

            prevLowerEdge.next = leftEdge;
            leftEdge.prev = prevLowerEdge;
            leftEdge.next = upper.IncidentEdge;
            upper.IncidentEdge.prev = leftEdge;

            prevUpperEdge.next = rightEdge;
            rightEdge.prev = prevUpperEdge;
            rightEdge.next = lower.IncidentEdge;
            lower.IncidentEdge.prev = rightEdge;

            leftEdge.incidentFace = leftEdge.next.incidentFace;
            // 이전에 저장한 Edge가 outerComponent일 가능성이 있기 떄문에 데이터 보장을 위해 이렇게 해준다.
            leftEdge.incidentFace.OuterComponent = leftEdge;

            HalfEdgeFace rightFace = new HalfEdgeFace { OuterComponent = rightEdge };
            HalfEdge current = rightEdge;
            do
            {
                current.incidentFace = rightFace;
                current = current.next;
            } while (current != rightEdge);
            faces.Add(rightFace);
            edges.Add(leftEdge);
            edges.Add(rightEdge);
        }
    }
}