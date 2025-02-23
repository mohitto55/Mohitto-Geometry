using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class HermitePath : SplinePathBase<Hermite>
{
    public override void AddPoint(Vector2 point)
    {
        if (path.Count > 0)
        {
            Vector2[] lastPoints = path[path.Count - 1].Points;
            Vector2 spawnPos = new Vector2(lastPoints[1].x + 1, lastPoints[1].y + 1);
            Hermite newHermite = new Hermite(lastPoints[1], Vector2.right, spawnPos, Vector2.right);
            path.Add(newHermite);
        }
        else
        {
            Hermite newHermite = new Hermite(Vector2.zero);
            path.Add(newHermite);
        }
    }
    
    public HermitePath(SplinePathBase<Hermite> other) : base(other)
    {
    }
    
    public HermitePath(IEnumerable<Hermite> slices)
    {
        path = slices.ToList();
    }
    public HermitePath(){}
}
