using System;
using UnityEngine;

public class CircleCenterTest : MonoBehaviour
{
    [SerializeField] private Transform p0;
    [SerializeField] private Transform p1;
    [SerializeField] private Transform p2;

    private void OnDrawGizmos()
    {
        if (p0 == null || p1 == null || p2 == null)
            return;
        
        Vector2 center = MyMath.GetCircumCircleCenter(p0.position, p1.position, p2.position);
        float radius = Vector2.Distance(p0.position, center);
        MyGizmos.DrawWireCircle(center, radius);
        Gizmos.color = Color.red;
        MyGizmos.DrawWireCircle(center, 1);
        Gizmos.color = Color.green;
        MyGizmos.DrawWireCircle(p0.position, 1f);
        MyGizmos.DrawWireCircle(p1.position, 1f);
        MyGizmos.DrawWireCircle(p2.position, 1f);
    }
}
