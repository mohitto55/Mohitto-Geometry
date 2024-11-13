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


        // ġ���� -180 - 180 ������ ������ ��������.
        float SignedAngle(Vector2 prev, Vector2 center, Vector2 next)
        {
            Vector2 ctop = prev - center;
            Vector2 cton = next - center;
            // ctop�� cton���� �� ������ ���� ���Ѵ�.
            // ������ ��������� ��� �������� �Ǻ������� ���� UnsignedAngle�� ���� ���̴�.
            // UnsignedAngle�� -��ȣ�� ���� 180�� �̸� ���� �־�����.
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