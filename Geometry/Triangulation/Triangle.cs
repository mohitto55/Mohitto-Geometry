using UnityEngine;


namespace Geometry
{
    public class Triangle
    {
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        public float Area
        {
            get
            {
                Vector2 p1Top3 = p3 - p1;
                Vector2 p1Top2 = p2 - p1;
                float lineLength1 = p1Top3.magnitude;
                float lineLength2 = p1Top2.magnitude;
                float angle = Vector2.Angle(p1Top3, p1Top2) * Mathf.Deg2Rad;
                float area = lineLength1 * lineLength2 * Mathf.Sin(angle) / 2;
                return area;
            }
        }

        public Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
}