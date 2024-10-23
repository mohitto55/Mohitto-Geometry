using System;
using System.Collections.Generic;
using Swewep_Line_Algorithm;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using Random = UnityEngine.Random;


// public class EventPoint : IComparable<EventPoint>
// {
//     public enum EventType
//     {
//         UPPER,
//         LOWER,
//         INTERSECTION
//     }
//
//     public EventType type;
//     public Vector2 p1;
//     public Vector2 p2;
//     
//     public int CompareTo(EventPoint other)
//     {
//         return this.p1.x < other.p1.y ? 1 : -1;
//     }
// }
[Serializable]
public class LineSegmentTransform
{
    public Transform p1;
    public Transform p2;
}
public class SweepLineObject : MonoBehaviour
{
    public List<LineSegmentTransform> Transforms = new List<LineSegmentTransform>();
    private List<Segment> Endpoints = new List<Segment>();
    private List<Point> intersectionPoints = new List<Point>();

    public float width = 10;
    public float height = 10;
    public int lineCount = 1;

    public float PointRadius = 1;
    public float IntersectionPointRadius = 1;
    public float DebugPointRadius = 1;
    public float DebugWaitSecond = 0.2f;

    private DebugPoint debugPoint;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSweepLineIntersection();
        }
    }

    void StartSweepLineIntersection()
    {
        if (Transforms != null)
        {
            for (int i = 0; i < Transforms.Count; i++)
            {
                Destroy(Transforms[i].p1.gameObject);
                Destroy(Transforms[i].p2.gameObject);
            }
        }
        
        Transforms = new List<LineSegmentTransform>();
        Endpoints = new List<Segment>();
        intersectionPoints = new List<Point>();
        for (int i = 0; i < lineCount; i++)
        {
            LineSegmentTransform newTransform = new LineSegmentTransform();
            newTransform.p1 = new GameObject((1 * i).ToString()).transform;
            newTransform.p1.position = new Vector3(Random.Range(0, width), Random.Range(0, height));
            newTransform.p2 = new GameObject((1 * i + 1).ToString()).transform;
            newTransform.p2.position = new Vector3(Random.Range(0, width), Random.Range(0, height));
            Transforms.Add(newTransform);
        }
        for (int i = 0; i < Transforms.Count; i++)
        {
            Endpoints.Add(new Segment(Transforms[i].p1.position, Transforms[i].p2.position));
        }

        debugPoint = new DebugPoint();
        StartCoroutine(Swewep_Line_Algorithm.LS.SweepIntersections(Endpoints, intersectionPoints, debugPoint, DebugWaitSecond, false));
    }

    private void OnDrawGizmos()
    {
        List<Segment> endpoints = new List<Segment>();
        for (int i = 0; i < Transforms.Count; i++)
        {
            if (Transforms[i].p1 && Transforms[i].p2)
            {
                Segment lineSegment = new Segment(Transforms[i].p1.position, Transforms[i].p2.position);
                endpoints.Add(lineSegment);
                Gizmos.color = Color.red;
                MyGizmos.DrawWireCicle(lineSegment.Start.ToVector(), PointRadius, 30);
                Gizmos.color = Color.blue;
                MyGizmos.DrawWireCicle(lineSegment.End.ToVector(), PointRadius, 30);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(lineSegment.Start.ToVector(), lineSegment.End.ToVector());
            }
        }
        
        Gizmos.color = Color.green;
        for (int i = 0; i < intersectionPoints.Count; i++)
        {
            if(intersectionPoints[i] != null)
                MyGizmos.DrawWireCicle(intersectionPoints[i].ToVector(), IntersectionPointRadius, 30);
        }

        if (debugPoint != null)
        {
            if (debugPoint.p.y != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector3(0, debugPoint.p.y), new Vector3(width, debugPoint.p.y));
                MyGizmos.DrawWireCicle(debugPoint.p, DebugPointRadius, 30);
            }
        }
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(new Vector3(width / 2, height / 2, 0), new Vector3(width, height, 0));
    }
}

