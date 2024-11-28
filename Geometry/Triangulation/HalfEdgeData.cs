using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Monotone
{
    public class HalfEdgeData
    {
        public List<HalfEdgeVertex> vertices = new List<HalfEdgeVertex>();
        public List<HalfEdge> edges = new List<HalfEdge>();
        public List<HalfEdgeFace> faces = new List<HalfEdgeFace>();
        
        // - ��ü: ��ü �ܺ��� ��� �ﰢ���� �����ϰ� �ð� �ݴ� �������� �����ؾ� �մϴ�
        // - ����: ���� ���� ��� �ﰢ���� �����ϰ� �ð� �������� �����ؾ� �մϴ�
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
            // CCW �������� ����Ʈ ����
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
                
                // �ٸ� �ڷ�� ���� �� ���� ������ �ΰ� ���� �ʿ�� ����δ� ��� �����İ� �߿��ѵ�
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
            
            HalfEdge firstLeftEdge = edges[startEdgeCount];
            prevLeftEdge.next = firstLeftEdge;
            firstLeftEdge.prev = prevLeftEdge;
                
            HalfEdge firstRightEdge = edges[startEdgeCount+1];
            firstRightEdge.next = prevRightEdge;
            firstRightEdge.vertex = prevLeftEdge.vertex;
            prevRightEdge.prev = firstRightEdge;
            face.OuterComponent = firstLeftEdge;
        }

        // �밢���߰�
        public void AddDiagonal(HalfEdgeVertex lower, HalfEdgeVertex upper)
        {
            if (lower == null || upper == null)
                return;
            if (HalfEdgeUtility.IsConnectedVertex(lower, upper))
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
                Debug.LogWarning(lower.Coordinate + " " + upper.Coordinate +  " ���� Face�� ���� Edge���� ã�� ���߽��ϴ�.");
                return;
            }
            AddDiagonal(lower, upper, edgesHasSameFace[0].incidentFace);
        }

        protected bool IsIsolationVertex(HalfEdgeVertex vertex)
        {
            return vertex.IncidentEdge == null;
        }
        
        // �밢���߰�
        // �ð� ����, �ݽð� �������� �̾���� ������� vertex���� ���� �մ´�.
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
                Debug.LogWarning(upper.Coordinate + " " + lower.Coordinate + " ������ �����ϴ�");
                return;
            }

            if (!HalfEdgeUtility.IsFaceCounterClockwise(face))
            {
                HalfEdge temp = upperEdge;
                upperEdge = lowerEdge;
                lowerEdge = temp;
            }
            
            // ���� ������ �Ʒ��� �������� �����ؼ� ���� ��ü�� �ֵ��̷� ǥ���� �� �ִ�.
            // �밢������ �� ���ο� edge�� �����
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


            // ���Ӱ� �밢���� ����� ���� �������� ���� vertex�� edge�� ������Ʈ ������Ѵ�.
            lowerEdge.next.prev = rightEdge;
            upperEdge.next.prev = leftEdge;

            lowerEdge.next = leftEdge;
            upperEdge.next = rightEdge;

            // incidentEdge�� ������ �ִ��� �����ϰ� �Ѵ�.
            lower.IncidentEdge = rightEdge;
            upper.IncidentEdge = leftEdge;

            leftEdge.incidentFace = leftEdge.next.incidentFace;
            // ������ ������ Edge�� outerComponent�� ���ɼ��� �ֱ� ������ ������ ������ ���� �̷��� ���ش�.
            leftEdge.incidentFace.OuterComponent = leftEdge;

            HalfEdgeFace rightFace = new HalfEdgeFace { OuterComponent = rightEdge };
            //Debug.Log("���� ���� Face : <color=green>"+ rightFace.GetHashCode() +"</color>");
            HalfEdge current = rightEdge;
            int i = 0;
            do
            {
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