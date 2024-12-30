using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DCEL
{
    public class HalfEdge
    {
        // Edge�� ���� ���⿡ �ִ� ���ؽ�
        public HalfEdgeVertex vertex;
        public HalfEdge twin;
        private HalfEdgeFace incidentFace;

        public HalfEdgeFace IncidentFace
        {
            get
            {
                return incidentFace;
            }
            set
            {
                if (incidentFace != null)
                    incidentFace.IncidentEdges.Remove(this);
                incidentFace = value;
                if(incidentFace != null)
                    incidentFace.IncidentEdges.Add(this);
            }
        }
        public HalfEdge next;
        public HalfEdge prev;
        public Color edgeColor = Color.white;

        public HalfEdge()
        {
            this.vertex = null;
        }
        public HalfEdgeVertex GetDestination()
        {
            return twin.vertex;
        }

        public override string ToString()
        {
            return prev?.vertex.Coordinate.ToString() + vertex?.Coordinate.ToString() + next?.vertex.Coordinate.ToString();
        }

        public Vector2 ToVector2()
        {
            if(prev != null)
                return vertex.Coordinate - prev.vertex.Coordinate;
            return Vector2.zero;
        }
        
                /// <summary>
        /// ������ CCW �������� ���ĵǾ� �ִ� ���������� 
        ///  �ð�������� �����̰� ������ ���� Edge�ΰɷ� �����Ѵ�.
        /// </summary>
        /// <returns></returns>
        public HalfEdge LeftEdge()
        {
            HalfEdge prev = this.prev;
            HalfEdge next = this.next;

            Vector2 prevDir = (prev.vertex.Coordinate - vertex.Coordinate).normalized;
            Vector2 nextDir = (next.vertex.Coordinate - vertex.Coordinate).normalized;

            return prevDir.x < nextDir.x ? this : next;
        }
        /// <summary>
        /// Right�� �ʿ��� Edge���� SPLIT, MERGE �ΰ���
        /// </summary>
        /// <returns></returns>
        public HalfEdge RightEdge()
        {
            HalfEdge prev = this.prev;
            HalfEdge next = this.next;

            Vector2 prevDir = (prev.vertex.Coordinate - this.vertex.Coordinate).normalized;
            Vector2 nextDir = (next.vertex.Coordinate - this.vertex.Coordinate).normalized;

            return prevDir.x > nextDir.x ? this : next;
        }
        
        // �������� Vertex�� ���� �����ʿ� �����ϴ��� Ȯ���ϴ� �Լ�
        public bool PolygonInteriorLiesToTheRight(HalfEdgeFace face)
        {
            // ������ �ٰ����� "����"�� �ִ���, "������"�� �ִ��� ������ �Ǵ��Ϸ���, ���� ������ ���� ������ �ε����� ���մϴ�.
            //     ������ �ε����� **��������(Ascending)**�̸� Ư�� ������ ��Ÿ����, **��������(Descending)**�̸� �ٸ� ������ ��Ÿ���ϴ�.
            //     ��ȣ�� ���� ��Ģ:
            // �ݽð� ���� (Counter-Clockwise Polygon):
            // ��������: ���� ������ �ٰ����� ���ʿ� ����.
            //     ��������: ���� ������ �ٰ����� �����ʿ� ����.
            //     �ð� ���� (Clockwise Polygon):
            // ��������: ���� ������ �ٰ����� �����ʿ� ����.
            //     ��������: ���� ������ �ٰ����� ���ʿ� ����
            List<Vector2> vector2s = HalfEdgeUtility.VerticesToVec2(face.GetAdjacentVertices());
            bool isCCW = MyMath.IsCounterClockwise(vector2s);
            
            Vector2 pPrev = isCCW ? this.prev.vertex.Coordinate : this.next.vertex.Coordinate;
            Vector2 pNext = isCCW ? this.next.vertex.Coordinate : this.prev.vertex.Coordinate;
            
            if (pPrev.y >= this.vertex.Coordinate.y &&
                pNext.y <= this.vertex.Coordinate.y) {
                return true;
            } else {
                return false;
            }
        }
    }
}