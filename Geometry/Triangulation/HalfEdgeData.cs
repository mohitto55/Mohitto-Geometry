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
        public void AddDiagonal(HalfEdgeVertex upper, HalfEdgeVertex lower)
        {
            Debug.Log("�밢�� �߰� ����");
            if (upper == null || lower == null)
                return;
            
            if (upper.IncidentEdge.vertex.Coordinate.y < lower.IncidentEdge.vertex.Coordinate.y)
            {
                HalfEdgeVertex temp = lower;
                lower = upper;
                upper = temp;
            }

            HalfEdge[] edgesHasSameFace = HalfEdgeUtility.GetEdgesHasSameFace(upper, lower);
            if (edgesHasSameFace == null)
            {
                Debug.LogWarning("���� Face�� ���� Edge���� ã�� ���߽��ϴ�.");
                return;
            }

            AddDiagonal(upper, lower, edgesHasSameFace[0].incidentFace);
        }
        // �밢���߰�
        private void AddDiagonal(HalfEdgeVertex upper, HalfEdgeVertex lower, HalfEdgeFace face)
        {
            if (upper == null || lower == null)
                return;
            
            if (upper.IncidentEdge.vertex.Coordinate.y < lower.IncidentEdge.vertex.Coordinate.y)
            {
                HalfEdgeVertex temp = lower;
                lower = upper;
                upper = temp;
            }
            
            HalfEdge upperEdge = HalfEdgeUtility.GetEdgesHasSameFace(upper, face);
            HalfEdge lowerEdge = HalfEdgeUtility.GetEdgesHasSameFace(lower, face);
            if (lowerEdge == null || upperEdge == null)
            {
                Debug.LogWarning("������ �����ϴ�");
                return;
            }
            
            // ���� ������ �Ʒ��� �������� �����ؼ� ���� ��ü�� �ֵ��̷� ǥ���� �� �ִ�.
            // �밢������ �� ���ο� edge�� �����
            HalfEdge leftEdge = new HalfEdge();
            HalfEdge rightEdge = new HalfEdge();

            leftEdge.vertex = lower;
            rightEdge.vertex = upper;

            leftEdge.twin = rightEdge;
            rightEdge.twin = leftEdge;

            // ���Ӱ� �밢���� ����� ���� �������� ���� vertex�� edge�� ������Ʈ ������Ѵ�.
            HalfEdge prevLowerEdge = lowerEdge.prev;
            HalfEdge prevUpperEdge = upperEdge.prev;

            prevLowerEdge.next = leftEdge;
            leftEdge.prev = prevLowerEdge;
            leftEdge.next = upperEdge;
            upper.IncidentEdge.prev = leftEdge;

            prevUpperEdge.next = rightEdge;
            rightEdge.prev = prevUpperEdge;
            rightEdge.next = lowerEdge;
            lower.IncidentEdge.prev = rightEdge;

            leftEdge.incidentFace = leftEdge.next.incidentFace;
            // ������ ������ Edge�� outerComponent�� ���ɼ��� �ֱ� ������ ������ ������ ���� �̷��� ���ش�.
            leftEdge.incidentFace.OuterComponent = leftEdge;

            HalfEdgeFace rightFace = new HalfEdgeFace { OuterComponent = rightEdge };
            Debug.Log("���� ���� Face : <color=green>"+ rightFace.GetHashCode() +"</color>");
            HalfEdge current = rightEdge;
            int i = 0;
            do
            {
                Debug.Log(i + " " + current.vertex.Coordinate);
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