using Unity.VisualScripting;
using UnityEngine;

namespace Monotone
{
    public class HalfEdgeTest : MonoBehaviour
    {
        public Transform p1;
        public Transform p2;
        public Transform p3;
        void Start()
        {

        }
        
        void Update()
        {

        }


        // 치역이 -180 - 180 까지의 각도를 구해진다.
        float SignedAngle(Vector2 prev, Vector2 center, Vector2 next)
        {
            Vector2 ctop = prev - center;
            Vector2 cton = next - center;
            // ctop와 cton으로 둘 사이의 각을 구한다.
            // 하지만 여기까지는 어디가 내부인지 판별을하지 않은 UnsignedAngle을 구한 것이다.
            // UnsignedAngle는 -부호가 없고 180도 미만 값만 주어진다.
            float angle = Vector2.Angle(ctop, cton);
            if (ctop.x * cton.y - ctop.y * cton.x > 0)
            {
                return Mathf.PI * 2 - angle;
            }

            return angle;
        }

        void OnDrawGizmos()
        {
            if (p1 && p2 && p3)
            {
                float angle = Vector2.Angle(p1.position - p2.position, p3.position - p2.position);
                //float angle = SignedAngle(p1.position, p2.position, p3.position);
                Gizmos.DrawLine(p1.position, p2.position);
                Gizmos.DrawLine(p2.position, p3.position);
                Debug.Log(angle);
            }
        }
    }
}