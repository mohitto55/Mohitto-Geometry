using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Geometry.DCEL
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
            AddPolygon(points);
        }

        public void AddLine(Vector2 point1, Vector2 point2)
        {
            List<HalfEdgeVertex> matchingVertices1 = vertices.Where(t => t.Coordinate == point1).ToList();
            List<HalfEdgeVertex> matchingVertices2 = vertices.Where(t => t.Coordinate == point1).ToList();

            HalfEdgeVertex vertex1;
            HalfEdgeVertex vertex2;
            
            if (matchingVertices1.Count > 0)
            {
                vertex1 = matchingVertices1[0];
            }
            else
            {
                vertex1 = new HalfEdgeVertex(point1);
                vertices.Add(vertex1);
            }
            if (matchingVertices2.Count > 0)
            {
                vertex2 = matchingVertices2[0];
            }
            else
            {
                vertex2 = new HalfEdgeVertex(point2);
                vertices.Add(vertex2);
            }
            
            AddDiagonal(vertex1, vertex2);
        }
        
        public void AddPolygon(List<Vector2> points)
        {
            // CCW 방향으로 리스트 정렬
            int startVertexCount = vertices.Count;
            int startEdgeCount = edges.Count;
            points = MyMath.EnsureCounterClockwise(points);
            int size = points.Count;
            HalfEdgeFace face = new HalfEdgeFace();
            faces.Add(face);

            HalfEdge prevLeftEdge = null;
            HalfEdge prevRightEdge = null;

            for (int i = 0; i < size; i++)
            {
                Vector2 point = points[i];
                
                // 다른 자료들 보니 꼭 같은 정점을 두개 만들 필요는 없어보인다 어떻게 쓰느냐가 중요한듯
                HalfEdgeVertex vertex = new HalfEdgeVertex(point);
                HalfEdge left = new HalfEdge();
                HalfEdge right = new HalfEdge();

                left.IncidentFace = face;
                left.next = null;
                left.vertex = vertex;
                left.twin = right;
                
                right.IncidentFace = null;
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
            
            HalfEdge firstLeftEdge = edges[startEdgeCount];
            prevLeftEdge.next = firstLeftEdge;
            firstLeftEdge.prev = prevLeftEdge;
                
            HalfEdge firstRightEdge = edges[startEdgeCount+1];
            firstRightEdge.next = prevRightEdge;
            firstRightEdge.vertex = prevLeftEdge.vertex;
            prevRightEdge.prev = firstRightEdge;
            face.OuterComponent = firstLeftEdge;
        }

        public void UpdateEdge(HalfEdge edge, HalfEdgeVertex newVertex)
        {
            HalfEdgeVertex originalVertex = edge.vertex;
            if (originalVertex != null)
            {
                originalVertex.IncidentEdge = originalVertex?.IncidentEdge?.next?.twin;
            }
            edge.vertex = newVertex;
            edge.twin.prev.vertex = newVertex;
            HalfEdge rightBetweenEdge = HalfEdgeUtility.GetEdgeBetweenVertices(originalVertex, newVertex);
            HalfEdge leftBetweenEdge = rightBetweenEdge == null ? edge.twin : rightBetweenEdge.next;
            if (rightBetweenEdge == null)
                rightBetweenEdge = edge.twin;
            edge.next.prev = edge.prev;
            edge.prev.prev = edge.prev;

            edge.next = leftBetweenEdge;
        }
        public void UpdateEdge(HalfEdge edge, HalfEdgeVertex prevVertex, HalfEdgeVertex newVertex)
        {
            Debug.Log("업데이트 시작" + edge);
            HalfEdgeVertex lower = prevVertex;
            HalfEdgeVertex upper = newVertex;
            if (lower.Coordinate.y > upper.Coordinate.y)
            {
                HalfEdgeVertex temp = upper;
                upper = lower;
                lower = temp;
            }
            
            // 왼쪽 원점을 아래쪽 오른쪽을 위로해서 각각 자체를 쌍둥이로 표현할 수 있다.
            // 대각선으로 할 새로운 edge를 만든다
            // leftEdge는 위로 rigtEdge는 아래로간다
            HalfEdge leftEdge = edge;
            HalfEdge rightEdge = edge.twin;
            if (rightEdge.vertex.Coordinate.y > leftEdge.vertex.Coordinate.y)
            {
                HalfEdge temp = rightEdge;
                rightEdge = leftEdge;
                leftEdge = temp;
            }
            
            
            HalfEdge upperEdge = HalfEdgeUtility.GetEdgeBetweenVertices(lower, upper);
            HalfEdge lowerEdge = HalfEdgeUtility.GetEdgeBetweenVertices(upper, lower);

            bool isSameCycle = false;
            if (lowerEdge == null || upperEdge == null)
            {
                Debug.LogWarning(upper.Coordinate + " " + lower.Coordinate + " 엣지가 없습니다");
                isSameCycle = false;
            }
            else
            {
                // 두 엣지 모두 inner 또는 outer인데다 face가 같다면 똑같은 폴리곤을 가른다는 뜻이므로 face를 하나더 만들어야한다.
                 isSameCycle = HalfEdgeUtility.IsBoundaryCounterClockwise(upperEdge) ==
                                     HalfEdgeUtility.IsBoundaryCounterClockwise(lowerEdge);
            }

            if (leftEdge.vertex != null)
            {
                leftEdge.vertex.IncidentEdge = leftEdge.vertex?.IncidentEdge?.next?.twin;
            }
            if (rightEdge.vertex != null)
            {
                rightEdge.vertex.IncidentEdge = rightEdge.vertex?.IncidentEdge?.next?.twin;
            }
            
            leftEdge.vertex = upper;
            leftEdge.twin = rightEdge;
            leftEdge.prev = lowerEdge != null ? lowerEdge : leftEdge.prev;
            leftEdge.next = upperEdge != null ? upperEdge.next : leftEdge.next;

            if (leftEdge.next != null)
            {
                leftEdge.next.twin.vertex = upper;
                leftEdge.IncidentFace = leftEdge.next.IncidentFace;
            }

            rightEdge.vertex = lower;
            rightEdge.twin = leftEdge;
            rightEdge.prev = upperEdge != null ? upperEdge : rightEdge.prev;
            rightEdge.next = lowerEdge != null ? lowerEdge.next : rightEdge.next;
            
            if (rightEdge.next != null)
            {
                rightEdge.next.twin.vertex = lower;
                rightEdge.IncidentFace = rightEdge.next.IncidentFace;
            }

            // 새롭게 대각선을 만드니 이전 엣지들의 다음 vertex와 edge를 업데이트 해줘야한다.
            if (lowerEdge != null)
            {
                lowerEdge.next.prev = rightEdge;
                lowerEdge.next = leftEdge;
            }

            if (upperEdge != null)
            {
                upperEdge.next.prev = leftEdge;
                upperEdge.next = rightEdge;
            }

            // incidentEdge의 방향이 최대한 동일하게 한다.
            lower.IncidentEdge = rightEdge;
            upper.IncidentEdge = leftEdge;

            leftEdge.IncidentFace = leftEdge.next.IncidentFace;
            // 이전에 저장한 Edge가 InnerComponent일 가능성이 있기 떄문에 데이터 보장을 위해 이렇게 해준다.
            if(leftEdge.IncidentFace != null)
                leftEdge.IncidentFace.OuterComponent = leftEdge;

            HalfEdgeFace rightFace = leftEdge.IncidentFace;
            if (isSameCycle)
            {
                rightFace = new HalfEdgeFace(leftEdge.IncidentFace);
                rightFace.OuterComponent = rightEdge;
                
                HalfEdge current = rightEdge;
                do
                {
                    current.IncidentFace = rightFace;
                    current = current.next;
                } while (current != null && current != rightEdge);
            }
            
            //Debug.Log("새로 만든 Face : <color=green>"+ rightFace.GetHashCode() +"</color>");

            
            
            if(isSameCycle)
                faces.Add(rightFace);
        }
        
        public void InsertEdge(HalfEdge edge, HalfEdgeVertex newVertex)
        {
            // leftEdge는 위로 rigtEdge는 아래로간다
            HalfEdge rightEdge = edge;
            HalfEdge leftEdge = edge.twin;
            HalfEdgeVertex nextV = edge.next.vertex;
            
            if (rightEdge.vertex.Coordinate.y > leftEdge.vertex.Coordinate.y)
            {
                HalfEdge temp = rightEdge;
                rightEdge = leftEdge;
                leftEdge = temp;
            }
            HalfEdgeVertex upperVertex = leftEdge.vertex;
            HalfEdgeVertex lowerVertex = rightEdge.vertex;
            Debug.Log("인설트 " + newVertex + " / " + upperVertex);
            //AddDiagonal(newVertex, edge.vertex);
            Debug.Log("인설트V " + edge.vertex);

            UpdateEdge(edge, nextV);
        }

       

        // 대각선추가
        // 정점 v 주변의 처리: v 주변의 반에지들의 순환 순서를 결정하기 위해 e'과 e"의 위치를 테스트합니다
        // 네 개의 반에지 쌍이 생기며, v에서 시계방향으로 연결됩니다
        // e'에 대한 반에지는 시계 반대 방향의 첫 번째 반에지와 연결되어야 합니다
        // 복잡도 분석: 대부분의 단계는 상수 시간이 걸립니다
        // v 주변의 e'과 e"의 순환 순서를 찾는 데는 v의 차수에 비례하는 시간이 걸립니다
        // 교차점에서 발생할 수 있는 다른 경우들(두 에지의 교차 등)도 O(m) 시간이 걸립니다
        // (여기서 m은 이벤트 포인트에 인접한 에지의 수) 
        public void AddDiagonal(HalfEdgeVertex v1, HalfEdgeVertex v2)
        {
            if (v1 == null || v2 == null)
                return;

            if (HalfEdgeUtility.IsConnectedVertex(v1, v2))
            {
                Debug.Log("이미 연결됨" + v1 + " " + v2);
                return;
            }

            HalfEdgeVertex lower = v1;
            HalfEdgeVertex upper = v2;
            if (lower.Coordinate.y > upper.Coordinate.y)
            {
                HalfEdgeVertex temp = upper;
                upper = lower;
                lower = temp;
            }

            // 왼쪽 원점을 아래쪽 오른쪽을 위로해서 각각 자체를 쌍둥이로 표현할 수 있다.
            // 대각선으로 할 새로운 edge를 만든다
            // leftEdge는 위로 rigtEdge는 아래로간다
            HalfEdge leftEdge = new HalfEdge();
            HalfEdge rightEdge = new HalfEdge();
            leftEdge.vertex = upper;
            leftEdge.twin = rightEdge;
            leftEdge.next = rightEdge;
            leftEdge.prev = rightEdge;
            
            rightEdge.vertex = lower;
            rightEdge.twin = leftEdge;
            rightEdge.prev = leftEdge;
            rightEdge.next = leftEdge;

            UpdateEdge(rightEdge, upper, lower);
            edges.Add(leftEdge);
            edges.Add(rightEdge);
            Debug.LogWarning("엣지 추가1 / " + leftEdge.prev + " / " +leftEdge + " / " + leftEdge.next);
            Debug.LogWarning("엣지 추가2 / " + rightEdge.prev + " / " +rightEdge + " / " + rightEdge.next);
        }
    }
}