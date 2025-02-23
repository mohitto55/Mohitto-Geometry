using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

public class TriangleCenterTest : MonoBehaviour
{
    [SerializeField] private Transform p0;
    [SerializeField] private Transform p1;
    [SerializeField] private Transform p2;

    private void OnDrawGizmos()
    {
        if (p0 == null || p1 == null || p2 == null)
            return;
        List<Vector2> triangle = new List<Vector2>() { p0.position, p1.position, p2.position };
        Vector2 center = MyGeometry.GetCircumCircleCenter(p0.position, p1.position, p2.position);
        Gizmos.color = Color.red;
        MyGizmos.DrawWireCircle(center, 0.5f);
        Gizmos.color = Color.green;
        MyGizmos.DrawPolygon(triangle);
    }
}
