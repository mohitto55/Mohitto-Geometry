using UnityEngine;
using UnityEditor;

using System.Reflection;
using UnityEngine.Serialization;

[CustomEditor(typeof(HermitePathMono), true), CanEditMultipleObjects]
public class HermitePathEditor : Editor {
    public HermitePathMono hermitePathMono;
    [FormerlySerializedAs("hermitePathBase")] public HermitePath hermitePath;
    private void OnSceneGUI() {
        SearilizedObjectCheck();

        Vector2[] lines = hermitePath.CalculateEvenlySpacedPoints(0.01f, 1);
        for (int i = 0; i < lines.Length - 1; i++) {
            Handles.color = Color.green;
            Handles.DrawLine(lines[i], lines[i+1]);
        }
        if (hermitePath.EditLine) {
            float nodeSize = hermitePath.NodeSize;
            for (int i = 0; i < hermitePath.SegmentCount; i++) {
                Spline segment = hermitePath.GetSegment(i);
                
                Handles.color = Color.red;
                Vector2 movePos = Handles.FreeMoveHandle(segment.Segment[0], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                segment.MovePoint(0, movePos);
                if (i - 1 >= 0)
                {
                    Spline prevSegment = hermitePath.GetSegment(i - 1);
                    prevSegment.EndPoint = movePos;
                }
                

                Handles.color = Color.white;
                movePos = Handles.FreeMoveHandle(segment.Segment[0] + segment.Segment[1], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                segment.MovePoint(1, movePos - segment.Segment[0]);
                Handles.DrawLine(segment.Segment[0], segment.Segment[0] + segment.Segment[1]);

                if (i == hermitePath.SegmentCount - 1)
                {
                    Handles.color = Color.blue;
                    movePos = Handles.FreeMoveHandle(segment.Segment[2], nodeSize, Vector2.zero,
                        Handles.CylinderHandleCap);
                    segment.MovePoint(2, movePos);
                }

                Handles.color = Color.white;
                movePos = Handles.FreeMoveHandle(segment.Segment[2] - segment.Segment[3], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                segment.MovePoint(3, -(movePos - segment.Segment[2]));
                Handles.DrawLine(segment.Segment[2], segment.Segment[2] - segment.Segment[3]);

                // movePos = Handles.FreeMoveHandle(path[k + 2], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                // path.MovePoint(k + 2, movePos);
                // Handles.DrawLine(path[k + 2], path[k + 3]);
                // if (i == path.NumSegments - 1) {
                //     Handles.color = Color.blue;
                //     movePos = Handles.FreeMoveHandle(path[k + 3], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                //     path.MovePoint(k + 3, movePos);
                // }
            }
            Event guiEvent = Event.current;
            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.shift) {
                for (int i = 0; i < hermitePath.SegmentCount + 1; i++) {
                    Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
                    float distance = Vector2.Distance(mousePos, hermitePath.GetSegment(i).Handles[0]);
                    Debug.Log(i + "번째 노드 " + distance);
                    if (distance <= nodeSize) {
                        hermitePath.RemoveSegment(i);
                    }
                }
                Debug.Log("우클릭");
            }
        }
        
        EditorUtility.SetDirty(hermitePathMono);
    }
    
    private void OnEnable() {
    }
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        SearilizedObjectCheck();
        EditorGUILayout.HelpBox("파란 점 - 도착지점\n빨간 노드에 쉬프트 + 우클릭하면 노드 삭제", MessageType.Info);
        if (GUILayout.Button("Add Node"))
        {
            if (hermitePath.SegmentCount > 0)
            {
                Vector2[] lastSegment = hermitePath.GetSegment(hermitePath.SegmentCount - 1).Segment;
                Vector2 spawnPos = new Vector2(lastSegment[3].x + 1, lastSegment[3].y + 1);
                hermitePath.AddPoint(spawnPos);
            }
            else
            {
                hermitePath.AddPoint(Vector2.zero);
            }
        }
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }

    void SearilizedObjectCheck()
    {
        if (hermitePath == null)
        {
            hermitePathMono = target as HermitePathMono;
            hermitePath = (HermitePath)hermitePathMono.Spline;
        }
    }
}