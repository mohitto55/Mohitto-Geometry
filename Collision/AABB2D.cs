
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCollision
{
    public class AABB2D : Collision2D
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public Vector2 LeftBottom => new Vector2(Center.x - Width / 2, Center.y - Height / 2);
        public Vector2 LeftTop => new Vector2(Center.x - Width / 2, Center.y + Height / 2);
        public Vector2 RightDown => new Vector2(Center.x + Width / 2, Center.y - Height / 2);
        public Vector2 RightTop => new Vector2(Center.x + Width / 2, Center.y + Height / 2);
        
        public override void DrawGizmo()
        {
            Gizmos.DrawLine(LeftBottom, RightDown);
            Gizmos.DrawLine(RightDown, RightTop);
            Gizmos.DrawLine(RightTop, LeftTop);
            Gizmos.DrawLine(LeftTop, LeftBottom);
        }

        public override CollisionResult Collision(Collision2D other)
        {
            CollisionResult result = new CollisionResult();
            if (other is (AABB2D))
            {
                result = Collision(this, (AABB2D)other);
            }

            return result;
        }

        public static CollisionResult Collision(AABB2D aabb1, AABB2D aabb2)
        {
            CollisionResult result = new CollisionResult();
            float s1MinX = aabb1.LeftBottom.x;
            float s1MaxX = aabb1.RightTop.x;
            float s2MinX = aabb2.LeftBottom.x;
            float s2MaxX = aabb2.RightTop.x;
            
            float s1MinY = aabb1.LeftBottom.y;
            float s1MaxY = aabb1.RightTop.y;
            float s2MinY = aabb2.LeftBottom.y;
            float s2MaxY = aabb2.RightTop.y;
            
            if (s2MinX <= s1MaxX && s1MinX <= s2MaxX)
            {
                if (s2MinY <= s1MaxY && s1MinY <= s2MaxY)
                {
                    result.isHit = true;
                    result.hitCollision = aabb2;
                }
            }

            return result;
        }

        public AABB2D(Vector2 center, float width, float height) : base(center)
        {
            Width = width;
            Height = height;
        }
        
        public AABB2D(IEnumerable<Vector2> points) : base(CalculateCenter(points))
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            foreach (Vector2 p in points)
            {
                minX = p.x < minX ? p.x : minX;
                minY = p.y < minY ? p.y : minY;
                maxX = p.x > maxX ? p.x : maxX;
                maxY = p.y > maxY ? p.y : maxY;
            }

            Width = maxX - minX;
            Height = maxY - minY;
        }

        private static Vector2 CalculateCenter(IEnumerable<Vector2> points)
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            foreach (Vector2 p in points)
            {
                minX = p.x < minX ? p.x : minX;
                minY = p.y < minY ? p.y : minY;
                maxX = p.x > maxX ? p.x : maxX;
                maxY = p.y > maxY ? p.y : maxY;
            }

            return new Vector2(Mathf.Lerp(minX, maxX, 0.5f), Mathf.Lerp(minY, maxY, 0.5f));
        }
    }
}
