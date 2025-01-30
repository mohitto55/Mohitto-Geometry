using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath : SplinePath
{
    public bool EditLine = false;
    public float NodeSize = 0.5f;
    public Bezier Nodes;
    [SerializeField, HideInInspector] List<Vector2> Paths = new List<Vector2>();
    public override Spline Path => Nodes;
}