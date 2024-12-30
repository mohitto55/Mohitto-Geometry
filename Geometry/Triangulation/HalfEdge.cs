using System.Collections.Generic;
using UnityEngine;

namespace Geometry.DCEL
{
    public class HalfEdge
    {
        // Edge의 앞쪽 방향에 있는 버텍스
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
        /// 정점이 CCW 방향으로 정렬되어 있다 가정했을때 
        ///  시계방향으로 움직이고 있으면 왼쪽 Edge인걸로 간주한다.
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
        /// Right가 필요한 Edge들은 SPLIT, MERGE 두개다
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
        
        // 폴리곤이 Vertex에 비해 오른쪽에 존재하는지 확인하는 함수
        public bool PolygonInteriorLiesToTheRight(HalfEdgeFace face)
        {
            // 정점이 다각형의 "왼쪽"에 있는지, "오른쪽"에 있는지 빠르게 판단하려면, 이전 정점과 다음 정점의 인덱스를 비교합니다.
            //     정점의 인덱스가 **오름차순(Ascending)**이면 특정 패턴이 나타나고, **내림차순(Descending)**이면 다른 패턴이 나타납니다.
            //     부호에 따른 규칙:
            // 반시계 방향 (Counter-Clockwise Polygon):
            // 오름차순: 정규 정점이 다각형의 왼쪽에 있음.
            //     내림차순: 정규 정점이 다각형의 오른쪽에 있음.
            //     시계 방향 (Clockwise Polygon):
            // 오름차순: 정규 정점이 다각형의 오른쪽에 있음.
            //     내림차순: 정규 정점이 다각형의 왼쪽에 있음
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