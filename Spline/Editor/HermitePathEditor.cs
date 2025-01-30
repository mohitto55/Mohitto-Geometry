using UnityEngine;
using UnityEditor;

using System.Reflection;
using UnityEngine.Serialization;

[CustomEditor(typeof(HermitePath), true), CanEditMultipleObjects]
public class HermitePathEditor : Editor {
    public HermitePath hermitePath;
    private void OnSceneGUI() {
        if (hermitePath == null)
        {
            hermitePath = (HermitePath)target;
            return;
        }

        Hermite path = hermitePath.Nodes;
        if (path != null)
        {
            Vector2[] lines = path.CalculateEvenlySpacedPoints(0.01f, 1);
            for (int i = 0; i < lines.Length - 1; i++) {
                Handles.color = Color.green;
                Handles.DrawLine(lines[i], lines[i+1]);
            }
            if (hermitePath.EditLine) {
                float nodeSize = hermitePath.NodeSize;
                for (int i = 0; i < path.NumSegments; i++) {
                    int k = i * 2;
                    Handles.color = Color.red;
                    Vector2 movePos = Handles.FreeMoveHandle(path[k], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k, movePos);

                    Handles.color = Color.white;
                    movePos = Handles.FreeMoveHandle(path[k + 1], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k + 1, movePos);
                    Handles.DrawLine(path[k], path[k + 1]);

                    Handles.color = Color.red;
                    movePos = Handles.FreeMoveHandle(path[k + 2], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k + 2, movePos);

                    Handles.color = Color.white;
                    movePos = Handles.FreeMoveHandle(path[k + 3], nodeSize, Vector2.zero, Handles.CylinderHandleCap);
                    path.MovePoint(k + 3, movePos);
                    Handles.DrawLine(path[k + 3], path[k + 2]);
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
                    for (int i = 0; i < hermitePath.Nodes.NumSegments+1; i++) {
                        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
                        float distance = Vector2.Distance(mousePos, hermitePath.Nodes[i * 2]);
                        Debug.Log(i + "번째 노드 " + distance);
                        if (distance <= nodeSize) {
                            hermitePath.Nodes.DeletePoint(i);
                        }
                    }
                    Debug.Log("우클릭");
                }
            }
        }
        EditorUtility.SetDirty(hermitePath);
    }

    private void OnEnable() {
    }
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (hermitePath == null)
            hermitePath = target as HermitePath;
        EditorGUILayout.HelpBox("파란 점 - 도착지점\n빨간 노드에 쉬프트 + 우클릭하면 노드 삭제", MessageType.Info);
        if (GUILayout.Button("Add Node")) {
            if (hermitePath.Nodes == null || hermitePath.Nodes.NumPoints == 0)
                hermitePath.Nodes = new Hermite(new Vector2(1, 1));
            Vector2 spawnPos = new Vector2(hermitePath.Nodes[hermitePath.Nodes.NumPoints - 1].x + 1, hermitePath.Nodes[hermitePath.Nodes.NumPoints - 1].y + 1);
            hermitePath.Nodes.AddPoint(new Vector2(spawnPos.x, spawnPos.y));
            Debug.Log(hermitePath.Nodes.NumSegments + " / " + hermitePath.Nodes.NumPoints);
        }

        serializedObject.ApplyModifiedProperties();
    }
}