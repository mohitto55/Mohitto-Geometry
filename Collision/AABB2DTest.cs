using System;
using UnityEngine;

namespace CustomCollision
{
    public class AABB2DTest : MonoBehaviour
    {
        public Transform t1;
        public Transform t2;

        public Vector2 aabb1Size;
        public Vector2 aabb2Size;
        public void OnDrawGizmos()
        {
            if (t1 == null || t2 == null)
                return;
            
            AABB2D aabb1 = new AABB2D(t1.transform.position, aabb1Size.x, aabb1Size.y);
            AABB2D aabb2 = new AABB2D(t2.transform.position, aabb2Size.x, aabb2Size.y);

            CollisionResult result = aabb1.Collision(aabb2);
            Gizmos.color = result.isHit ? Color.red : Color.green;
            
            aabb1.DrawGizmo();
            aabb2.DrawGizmo();
        }
    }
}