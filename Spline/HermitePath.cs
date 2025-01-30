using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HermitePath : SplinePath
{
    public bool EditLine = false;
    public float NodeSize = 0.5f;
    public Hermite Nodes;
    [SerializeField, HideInInspector] List<Vector2> Paths = new List<Vector2>();
    public override Spline Path => Nodes;
}
