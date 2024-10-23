using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonotoneTriangulation : MonoBehaviour
{
    [SerializeField] private List<Vector2> Points = new List<Vector2>();
    private Dictionary<int, int> TurnVertexType = new Dictionary<int, int>();

    private void Start()
    {
        Polygon2 polygon = new Polygon2(Points);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            MyGizmos.DrawWireCicle(Points[i], 3, 30);
        }
    }

    public void MakeMonotone(List<Vector2> positions)
    {
        var queue = new PriorityQueue<Vector2>();
        for (int i = 0; i < positions.Count; i++)
        {
            queue.Enqueue(positions[i], (int)positions[i].y);
        }
        
        while (!queue.IsEmpty())
        {
            Vector2 cur = queue.Dequeue();
        }
    }
}

public class Polygon2
{
    public List<Vector2> vertices;
    
    public Polygon2(List<Vector2> vertices)
    {
        this.vertices = vertices;
    }
}