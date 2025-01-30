using UnityEngine;
using UnityEditor;

using System.Reflection;

[CustomEditor(typeof(BezierPath), true), CanEditMultipleObjects]
public class BezierPathEditor : Editor {
    public BezierPath bezierPath;
    private void OnSceneGUI() {
        if (bezierPath == null)
            bezierPath = target as BezierPath;
        Bezier path = bezierPath.Nodes;
        if (path != null) {
            for (int i = 0; i < path.NumSegments; i++) {
                int k = i * 3;
                Handles.DrawBezier(path[k], path[k + 3], path[k + 1], path[k + 2], Color.green, null, 2);
            }
            if (bezierPath.EditLine) {
                float nodeSize = bezierPath.NodeSize;
                for (int i = 0; i < path.NumSegments; i++) {
                    int k = i * 3;
                    Handles.color = Color.red;
                    Vector2 movePos = Handles.FreeMoveHandle(path[k], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k, movePos);

                    Handles.color = Color.white;
                    movePos = Handles.FreeMoveHandle(path[k + 1], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k + 1, movePos);
                    Handles.DrawLine(path[k], path[k + 1]);

                    movePos = Handles.FreeMoveHandle(path[k + 2], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k + 2, movePos);
                    Handles.DrawLine(path[k + 2], path[k + 3]);
                    if (i == path.NumSegments - 1) {
                        Handles.color = Color.blue;
                        movePos = Handles.FreeMoveHandle(path[k + 3], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                        path.MovePoint(k + 3, movePos);
                    }
                }
                Event guiEvent = Event.current;
                if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.shift) {
                    for (int i = 0; i < bezierPath.Nodes.NumSegments + 1; i++) {
                        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
                        float distance = Vector2.Distance(mousePos, bezierPath.Nodes[i * 3]);
                        //Debug.Log(guiEvent.mousePosition);
                        Debug.Log(i + "번째 노드 " + distance);
                        if (distance <= nodeSize) {
                            bezierPath.Nodes.DeletePoint(i);
                        }
                    }
                    Debug.Log("우클릭");
                }
            }
        }
        EditorUtility.SetDirty(bezierPath);
    }

    private void OnEnable() {
    }
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (bezierPath == null)
            bezierPath = target as BezierPath;
        EditorGUILayout.HelpBox("파란 점 - 도착지점\n빨간 노드에 쉬프트 + 우클릭하면 노드 삭제", MessageType.Info);
        if (GUILayout.Button("Add Node")) {
            if (bezierPath.Nodes == null || bezierPath.Nodes.NumPoints == 0)
                bezierPath.Nodes = new Bezier(new Vector2(1, 1));
            Vector2 spawnPos = new Vector2(bezierPath.Nodes[bezierPath.Nodes.NumPoints - 1].x + 1, bezierPath.Nodes[bezierPath.Nodes.NumPoints - 1].y + 1);
            bezierPath.Nodes.AddPoint(new Vector2(spawnPos.x, spawnPos.y));
            Debug.Log(bezierPath.Nodes.NumSegments);
        }

        serializedObject.ApplyModifiedProperties();
    }
}