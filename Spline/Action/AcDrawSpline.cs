using Logic.Action;
using UnityEngine;

public class AcDrawSpline : IAction<Spline>
{
    public void ActionExecute(Spline target)
    {
        target.DrawOnGizmo();
    }
}
