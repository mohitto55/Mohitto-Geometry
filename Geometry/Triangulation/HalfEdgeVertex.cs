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
        public Vtype type;

        public enum Vtype
        {
            START,
            END,
            REGULAR,
            SPLIT,
            MERGE   
        }
        public HalfEdgeVertex(Vector2 v)
        {
            Coordinate = v;
        }

        public void DetermineType(HalfEdgeVertex first, HalfEdgeVertex second)
        {

            Vector2 v1 = first.Coordinate - this.Coordinate;
            Vector2 v2 = second.Coordinate - this.Coordinate;
            float angle = MyMath.SignedAngle(v1, v2);
            // ����� ������ �ڽź��� ���ٸ�
            if (first.Coordinate.y < this.Coordinate.y && second.Coordinate.y < this.Coordinate.y)
            {
                if (angle >= 0) this.type = Vtype.START;
                if (angle <= 0) this.type = Vtype.SPLIT;
            }
            // ����� ������ �ڽź��� ���ٸ�
            else if (first.Coordinate.y > this.Coordinate.y && second.Coordinate.y > this.Coordinate.y)
            {
                // ���ΰ��� 180�� �̸��̰� 
                if (angle >= 0) this.type = Vtype.END;
                if (angle <= 0) this.type = Vtype.MERGE;
            }
            // �ϳ��� �Ʒ�, �ϳ��� ���� ���� ���ؽ��� ����ġ���� �ʴ´�.
            else
            {
                this.type = Vtype.REGULAR;
            }
        }

        // public HalfEdge CWEdge()
        // {
        //     HalfEdgeVertex prevV = IncidentEdge.prev.vertex;
        //     HalfEdgeVertex nextV = IncidentEdge.next.vertex;
        //     // �ð����
        //     if (MyMath.CCW(prevV.Coordinate, Coordinate, nextV.Coordinate) < 0)
        //     {
        //         
        //     }
        // }

        /// <summary>
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
        
        public HalfEdge RightEdge()
        {
            HalfEdge prev = IncidentEdge.prev;
            HalfEdge next = IncidentEdge.next;
            float ccw = MyMath.CCW(prev.vertex.Coordinate, Coordinate, next.vertex.Coordinate);
            Debug.LogWarning(prev.vertex.Coordinate + " " + Coordinate + " " + next.vertex.Coordinate);

            Debug.LogWarning(ccw);
            if (ccw >= 0)
            {
                return IncidentEdge;
            }
            else
            {
                return next;
            }
            if (prev.vertex.Coordinate.x < next.vertex.Coordinate.x)
                return next;
            else
            {
                return IncidentEdge;
            }
        }
        
        public bool PolygonInteriorLiesToTheRight() {
            
            Vector2 pPrev = IncidentEdge.prev.vertex.Coordinate;
            Vector2 pNext = IncidentEdge.next.vertex.Coordinate;
            
            if (pPrev.y > this.Coordinate.y &&
                pNext.y < this.Coordinate.y) {
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
    }
}
