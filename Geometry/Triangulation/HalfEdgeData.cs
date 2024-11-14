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

        // - 선체: 선체 외부의 모든 삼각형을 제거하고 시계 반대 방향으로 정렬해야 합니다
        // - 구멍: 구멍 내의 모든 삼각형을 제거하고 시계 방향으로 정렬해야 합니다
        public HalfEdgeData(List<Vector2> points)
        {
            // CCW 방향으로 리스트 정렬
            points = MyMath.EnsureCounterClockwise(points);
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
                HalfEdge left = new HalfEdge();
                HalfEdge right = new HalfEdge();

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
                    right.vertex = prevLeftEdge.vertex;
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
            firstRightEdge.vertex = prevLeftEdge.vertex;
            prevRightEdge.prev = firstRightEdge;
            //prevRightEdge.next = 

            //prevRightEdge.vertex = vertices[0];
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
        public void AddDiagonal(HalfEdgeVertex lower, HalfEdgeVertex upper)
        {
            Debug.Log("대각선 추가 시작");
            if (lower == null || upper == null)
                return;
            
            if (lower.IncidentEdge.vertex.Coordinate.y < upper.IncidentEdge.vertex.Coordinate.y)
            {
                HalfEdgeVertex temp = upper;
                upper = lower;
                lower = temp;
            }

            HalfEdge[] edgesHasSameFace = HalfEdgeUtility.GetEdgesHasSameFace(lower, upper);
            if (edgesHasSameFace == null)
            {
                Debug.LogWarning(lower.Coordinate + " " + upper.Coordinate +  " 같은 Face를 가진 Edge들을 찾지 못했습니다.");
                return;
            }
            AddDiagonal(lower, upper, edgesHasSameFace[0].incidentFace);
        }
        // 대각선추가
        // 시계 방향, 반시계 방향으로 이어졋는지 상관없이 vertex끼리 선을 잇는다.
        private void AddDiagonal(HalfEdgeVertex lower, HalfEdgeVertex upper, HalfEdgeFace face)
        {
            if (lower == null || upper == null)
                return;
            
            if (lower.IncidentEdge.vertex.Coordinate.y > upper.IncidentEdge.vertex.Coordinate.y)
            {
                HalfEdgeVertex temp = upper;
                upper = lower;
                lower = temp;
            }
            
            HalfEdge upperEdge = HalfEdgeUtility.GetEdgesHasSameFace(upper, face);
            HalfEdge lowerEdge = HalfEdgeUtility.GetEdgesHasSameFace(lower, face);
            if (lowerEdge == null || upperEdge == null)
            {
                Debug.LogWarning(upper.Coordinate + " " + lower.Coordinate + " 엣지가 없습니다");
                return;
            }

            if (!HalfEdgeUtility.IsFaceCounterClockwise(face))
            {
                HalfEdge temp = upperEdge;
                upperEdge = lowerEdge;
                lowerEdge = temp;
            }
            
            // 왼쪽 원점을 아래쪽 오른쪽을 위로해서 각각 자체를 쌍둥이로 표현할 수 있다.
            // 대각선으로 할 새로운 edge를 만든다
            HalfEdge leftEdge = new HalfEdge();
            HalfEdge rightEdge = new HalfEdge();
            leftEdge.vertex = upper;
            leftEdge.twin = rightEdge;
            leftEdge.prev = lowerEdge;
            leftEdge.next = upperEdge.next;
            leftEdge.incidentFace = leftEdge.next.incidentFace;
            
            rightEdge.vertex = lower;
            rightEdge.twin = leftEdge;
            rightEdge.prev = upperEdge;
            rightEdge.next = lowerEdge.next;
            rightEdge.incidentFace = rightEdge.next.incidentFace;


            // 새롭게 대각선을 만드니 이전 엣지들의 다음 vertex와 edge를 업데이트 해줘야한다.
            lowerEdge.next.prev = rightEdge;
            upperEdge.next.prev = leftEdge;

            lowerEdge.next = leftEdge;
            upperEdge.next = rightEdge;

            // incidentEdge의 방향이 최대한 동일하게 한다.
            lower.IncidentEdge = rightEdge;
            upper.IncidentEdge = leftEdge;

            leftEdge.incidentFace = leftEdge.next.incidentFace;
            // 이전에 저장한 Edge가 outerComponent일 가능성이 있기 떄문에 데이터 보장을 위해 이렇게 해준다.
            leftEdge.incidentFace.OuterComponent = leftEdge;

            HalfEdgeFace rightFace = new HalfEdgeFace { OuterComponent = rightEdge };
            Debug.Log("새로 만든 Face : <color=green>"+ rightFace.GetHashCode() +"</color>");
            HalfEdge current = rightEdge;
            int i = 0;
            do
            {
                // Debug.Log(i + " " + current.vertex.Coordinate);
                i++;
                current.incidentFace = rightFace;
                current = current.next;
            } while (current != rightEdge);
            faces.Add(rightFace);
            edges.Add(leftEdge);
            edges.Add(rightEdge);
        }
        
    }
}