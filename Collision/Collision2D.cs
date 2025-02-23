using UnityEngine;


namespace CustomCollision
{
    [System.Serializable]
    public abstract class Collision2D : ICollision
    {
        public Collision2D(Vector2 center)
        {
            Center = center;
        }
        
        public Vector2 Center { get; set; }
        public abstract void DrawGizmo();

        public abstract CollisionResult Collision(Collision2D other);
    }

    public struct CollisionResult
    {
        public bool isHit;
        public Collision2D hitCollision;
        public Vector2 hitPoint;
    }
}