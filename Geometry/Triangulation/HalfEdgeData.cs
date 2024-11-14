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

        // - ��ü: ��ü �ܺ��� ��� �ﰢ���� �����ϰ� �ð� �ݴ� �������� �����ؾ� �մϴ�
        // - ����: ���� ���� ��� �ﰢ���� �����ϰ� �ð� �������� �����ؾ� �մϴ�
        public HalfEdgeData(List<Vector2> points)
        {
            // CCW �������� ����Ʈ ����
            points = MyMath.EnsureCounterClockwise(points);
            int size = points.Count;
            HalfEdgeFace face = new HalfEdgeFace();
            faces.Add(face);

            HalfEdge prevLeftEdge = null;
            HalfEdge prevRightEdge = null;

            for (int i = 0; i < size; i++)
            {
                Vector2 point = points[i];

                // �̰Ŵ� ���ؽ��z �ϳ��� �����? ���� ���� ���ؽ��� ������ �������ϴµ�
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
            
            // �ϼ��� ���ؽ����� ��ȸ�ϸ� Ÿ���� ���Ѵ�.
            for (int i = 0; i < vertices.Count; i++)
            {
                int indexPrev = (i - 1 + vertices.Count) % vertices.Count;
                int indexNext = (i + 1) % vertices.Count;
                vertices[i].DetermineType(vertices[indexPrev], vertices[indexNext]);
            }
        }

                // �밢���߰�
        public void AddDiagonal(HalfEdgeVertex lower, HalfEdgeVertex upper)
        {
            Debug.Log("�밢�� �߰� ����");
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
                Debug.LogWarning(lower.Coordinate + " " + upper.Coordinate +  " ���� Face�� ���� Edge���� ã�� ���߽��ϴ�.");
                return;
            }
            AddDiagonal(lower, upper, edgesHasSameFace[0].incidentFace);
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
            Debug.Log("���� ���� Face : <color=green>"+ rightFace.GetHashCode() +"</color>");
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