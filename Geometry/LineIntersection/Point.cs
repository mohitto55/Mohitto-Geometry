using System;
using UnityEngine;

namespace Swewep_Line_Algorithm
{
    public class Point
    {
        public float x;
        public float y;

        public string Label = "";

        public Point(Vector2 vec)
        {
            this.x = vec.x;
            this.y = vec.y;
        }
        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point operator +(Point x, Point y)
        {
            return new Point(x.x + y.x, x.y + y.y);
        }

        public static Point operator -(Point x)
        {
            x.x *= -1;
            x.y *= -1;
            return x;
        }

        public static Point operator -(Point x, Point y)
        {
            return new Point(x.x - y.x, x.y - y.y);
        }

        public static bool operator ==(Point x, Point y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return Math.Abs(x.x - y.x) < 0.00005f && Math.Abs(x.y - y.y) < 0.00005f;
        }

        public static bool operator !=(Point x, Point y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            return Label + "(" + x + ", " + y + ")";
        }

        public float Magnitude() => (float)Math.Sqrt(x * x + y * y);
        
        public Vector3 ToVector()
        {
            return new Vector3(x, y, 0);
        }
    }
}
