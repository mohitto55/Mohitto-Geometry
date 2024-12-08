using System;
using System.Collections;
using System.Collections.Generic;
using Swewep_Line_Algorithm;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Monotone
{
    public class HalfEdgeVertex
    {
        public Vector2 Coordinate;
        public HalfEdge IncidentEdge;
        
        /// <summary>
        /// ������ �� Type�� ����Ǳ� ���� �ѹ��� ����ȴ�.
        /// </summary>
        public Vtype type;

        public enum Vtype
        {
            START,
            END,
            REGULAR,
            SPLIT,
            MERGE,
        }
        public HalfEdgeVertex(Vector2 v)
        {
            Coordinate = v;
        }

        bool CheckParallel(HalfEdgeVertex v1, HalfEdgeVertex v2)
        {
            if (MyMath.FloatZero(Mathf.Abs(v1.Coordinate.y - this.Coordinate.y)))
            {
                // ������ ��� �����̶��?
                if(MyMath.FloatZero(Mathf.Abs(v2.Coordinate.y - this.Coordinate.y)))
                {
                    this.type = Vtype.REGULAR;
                    return true;
                }
                // ������ �Ʒ������� ���Ѵٸ�?
                if (v2.Coordinate.y < Coordinate.y)
                {
                    this.type = Vtype.START;
                }
                else if (v2.Coordinate.y > Coordinate.y)
                {
                    this.type = Vtype.END;
                }
                return true;
            }
            return false;
        }
        

        /// <summary>
        /// ������ CCW �������� ���ĵǾ� �ִ� ���������� 
        ///  �ð�������� �����̰� ������ ���� Edge�ΰɷ� �����Ѵ�.
        /// </summary>
        /// <returns></returns>
        public HalfEdge LeftEdge()
        {
            HalfEdge prev = IncidentEdge.prev;
            HalfEdge next = IncidentEdge.next;
            float ccw = MyMath.CCW(prev.vertex.Coordinate, Coordinate, next.vertex.Coordinate);
            if (ccw == -1)
            {
                return IncidentEdge;
            }
            else
            {
                return next;
            }
        }
        /// <summary>
        /// Right�� �ʿ��� Edge���� SPLIT, MERGE �ΰ���
        /// </summary>
        /// <returns></returns>
        public HalfEdge RightEdge()
        {
            HalfEdge prev = IncidentEdge.prev;
            HalfEdge next = IncidentEdge.next;
            float ccw = MyMath.CCW(prev.vertex.Coordinate, Coordinate, next.vertex.Coordinate);
            
            
            if (ccw >= 1)
            {
                return next;
            }
            else
            {
                return IncidentEdge;
            }
            if (prev.vertex.Coordinate.x < next.vertex.Coordinate.x)
                return next;
            else
            {
                return IncidentEdge;
            }
        }
        
        // �������� Vertex�� ���� �����ʿ� �����ϴ��� Ȯ���ϴ� �Լ�
        
        public bool PolygonInteriorLiesToTheRight()
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
            HalfEdgeFace face = IncidentEdge.incidentFace;
            List<Vector2> vector2s = HalfEdgeUtility.VerticesToVec(face.GetAdjacentVertices());
            bool isCCW = MyMath.IsCounterClockwise(vector2s);
            
            Vector2 pPrev = isCCW ? IncidentEdge.prev.vertex.Coordinate : IncidentEdge.next.vertex.Coordinate;
            Vector2 pNext = isCCW ? IncidentEdge.next.vertex.Coordinate : IncidentEdge.prev.vertex.Coordinate;
            
            if (pPrev.y >= this.Coordinate.y &&
                pNext.y <= this.Coordinate.y) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator ==(HalfEdgeVertex x, HalfEdgeVertex y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return Math.Abs(x.Coordinate.x - y.Coordinate.x) < 0.00005f && Math.Abs(x.Coordinate.y - y.Coordinate.y) < 0.00005f;
        }
        public static bool operator !=(HalfEdgeVertex x, HalfEdgeVertex y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return Coordinate.ToString();
        }
    }
}
