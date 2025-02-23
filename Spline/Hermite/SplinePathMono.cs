using UnityEngine;

public class SplinePathMono<TSpline> : SplinePathMono where TSpline : SplinePathBase, new()
{
    [SerializeReference] public TSpline _spline = new TSpline();
    public override SplinePathBase Spline => _spline;
}


public abstract class SplinePathMono : MonoBehaviour
{
    public abstract SplinePathBase Spline { get; }
}