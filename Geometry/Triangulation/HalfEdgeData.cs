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


        public void UpdateEdge(HalfEdge edge, HalfEdgeVertex prevVertex, HalfEdgeVertex newVertex)
        {
            Debug.Log("������Ʈ ����" + edge);
            HalfEdgeVertex lower = prevVertex;
            HalfEdgeVertex upper = newVertex;
            if (lower.Coordinate.y > upper.Coordinate.y)
            {
                HalfEdgeVertex temp = upper;
                upper = lower;
                lower = temp;
            }
            
            // ���� ������ �Ʒ��� �������� �����ؼ� ���� ��ü�� �ֵ��̷� ǥ���� �� �ִ�.
            // �밢������ �� ���ο� edge�� �����
            // leftEdge�� ���� rigtEdge�� �Ʒ��ΰ���
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
                Debug.LogWarning(upper.Coordinate + " " + lower.Coordinate + " ������ �����ϴ�");
                isSameCycle = true;
            }
            else
            {
                // �� ���� ��� inner �Ǵ� outer�ε��� face�� ���ٸ� �Ȱ��� �������� �����ٴ� ���̹Ƿ� face�� �ϳ��� �������Ѵ�.
                 isSameCycle = HalfEdgeUtility.IsBoundaryCounterClockwise(upperEdge) ==
                                     HalfEdgeUtility.IsBoundaryCounterClockwise(lowerEdge);
            }
            lowerEdge = lowerEdge == null ? leftEdge.prev : lowerEdge;
            upperEdge = upperEdge == null ? rightEdge.prev : upperEdge;
            
            leftEdge.vertex = upper;
            leftEdge.twin = rightEdge;
            leftEdge.prev = lowerEdge;
            leftEdge.next = upperEdge.next;
            leftEdge.IncidentFace = leftEdge.next.IncidentFace;
            
            rightEdge.vertex = lower;
            rightEdge.twin = leftEdge;
            rightEdge.prev = upperEdge;
            rightEdge.next = lowerEdge.next;
            rightEdge.IncidentFace = rightEdge.next.IncidentFace;

            // ���Ӱ� �밢���� ����� ���� �������� ���� vertex�� edge�� ������Ʈ ������Ѵ�.

            rightEdge.next.prev = rightEdge;
            rightEdge.next.twin.vertex = lower;
            lowerEdge.next = leftEdge;
            
            leftEdge.next.prev = leftEdge;
            leftEdge.next.twin.vertex = upper;
            upperEdge.next = rightEdge;

            

            // incidentEdge�� ������ �ִ��� �����ϰ� �Ѵ�.
            lower.IncidentEdge = rightEdge;
            upper.IncidentEdge = leftEdge;

            leftEdge.IncidentFace = leftEdge.next.IncidentFace;
            // ������ ������ Edge�� InnerComponent�� ���ɼ��� �ֱ� ������ ������ ������ ���� �̷��� ���ش�.
            if(leftEdge.IncidentFace != null)
                leftEdge.IncidentFace.OuterComponent = leftEdge;

            HalfEdgeFace rightFace = leftEdge.IncidentFace;
            if (isSameCycle)
            {
                rightFace = new HalfEdgeFace(leftEdge.IncidentFace);
                rightFace.OuterComponent = rightEdge;
            }
            
            //Debug.Log("���� ���� Face : <color=green>"+ rightFace.GetHashCode() +"</color>");
            HalfEdge current = rightEdge;
            do
            {
                current.IncidentFace = rightFace;
                current = current.next;
            } while (current != null && current != rightEdge);
            
            
            if(isSameCycle)
                faces.Add(rightFace);
                
            edges.Add(leftEdge);
            edges.Add(rightEdge);
            
            Debug.LogWarning("���� �߰�1 / " + leftEdge.prev + " / " +leftEdge + " / " + leftEdge.next);
            Debug.LogWarning("���� �߰�2 / " + rightEdge.prev + " / " +rightEdge + " / " + rightEdge.next);
        }
        
        public void InsertEdge(HalfEdge edge, HalfEdgeVertex newVertex)
        {
            // leftEdge�� ���� rigtEdge�� �Ʒ��ΰ���
            HalfEdge rightEdge = edge;
            HalfEdge leftEdge = edge.twin;

            if (rightEdge.vertex.Coordinate.y > leftEdge.vertex.Coordinate.y)
            {
                HalfEdge temp = rightEdge;
                rightEdge = leftEdge;
                leftEdge = temp;
            }
            HalfEdgeVertex upperVertex = leftEdge.vertex;
            HalfEdgeVertex lowerVertex = rightEdge.vertex;
            Debug.Log("�μ�Ʈ " + newVertex + " / " + upperVertex);
            AddDiagonal(newVertex, upperVertex);
            UpdateEdge(rightEdge, upperVertex, lowerVertex);
        }

       

        // �밢���߰�
        // ���� v �ֺ��� ó��: v �ֺ��� �ݿ������� ��ȯ ������ �����ϱ� ���� e'�� e"�� ��ġ�� �׽�Ʈ�մϴ�
        // �� ���� �ݿ��� ���� �����, v���� �ð�������� ����˴ϴ�
        // e'�� ���� �ݿ����� �ð� �ݴ� ������ ù ��° �ݿ����� ����Ǿ�� �մϴ�
        // ���⵵ �м�: ��κ��� �ܰ�� ��� �ð��� �ɸ��ϴ�
        // v �ֺ��� e'�� e"�� ��ȯ ������ ã�� ���� v�� ������ ����ϴ� �ð��� �ɸ��ϴ�
        // ���������� �߻��� �� �ִ� �ٸ� ����(�� ������ ���� ��)�� O(m) �ð��� �ɸ��ϴ�
        // (���⼭ m�� �̺�Ʈ ����Ʈ�� ������ ������ ��) 
        public void AddDiagonal(HalfEdgeVertex v1, HalfEdgeVertex v2)
        {
            if (v1 == null || v2 == null)
                return;

            if (HalfEdgeUtility.IsConnectedVertex(v1, v2))
            {
                Debug.Log("�̹� �����" + v1 + " " + v2);
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

            // ���� ������ �Ʒ��� �������� �����ؼ� ���� ��ü�� �ֵ��̷� ǥ���� �� �ִ�.
            // �밢������ �� ���ο� edge�� �����
            // leftEdge�� ���� rigtEdge�� �Ʒ��ΰ���
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
        }
    }
}