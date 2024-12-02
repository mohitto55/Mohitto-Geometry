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
        /// 어차피 이 Type은 실행되기 전에 한번만 수행된다.
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
                // 양쪽이 모두 수평이라면?
                if(MyMath.FloatZero(Mathf.Abs(v2.Coordinate.y - this.Coordinate.y)))
                {
                    this.type = Vtype.REGULAR;
                    return true;
                }
                // 한쪽이 아래쪽으로 향한다면?
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
        /// 정점이 CCW 방향으로 정렬되어 있다 가정했을때 
        ///  시계방향으로 움직이고 있으면 왼쪽 Edge인걸로 간주한다.
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
        /// Right가 필요한 Edge들은 SPLIT, MERGE 두개다
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
