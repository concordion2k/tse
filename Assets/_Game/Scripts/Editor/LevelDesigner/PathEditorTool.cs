using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using LevelDesigner;

namespace LevelDesignerEditor
{
    [EditorTool("Path Editor", typeof(LevelPathFollower))]
    public class PathEditorTool : EditorTool
    {
        private GUIContent toolIcon;
        private LevelPathData currentPath;
        private int selectedKnotIndex = -1;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (toolIcon == null)
                {
                    toolIcon = new GUIContent(
                        EditorGUIUtility.IconContent("AvatarPivot").image,
                        "Path Editor Tool"
                    );
                }
                return toolIcon;
            }
        }

        public override void OnActivated()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void OnWillBeDeactivated()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            LevelPathFollower follower = target as LevelPathFollower;
            if (follower == null || follower.PathData == null) return;

            currentPath = follower.PathData;

            DrawPathVisualization();
            HandleKnotInteraction();
            DrawToolPanel();
        }

        private void DrawPathVisualization()
        {
            if (!currentPath.IsValid) return;

            // Draw the path curve
            Handles.color = Color.cyan;
            float length = currentPath.TotalLength;
            Vector3 prevPos = currentPath.GetPositionAtDistance(0f);

            for (float d = 1f; d <= length; d += 1f)
            {
                Vector3 pos = currentPath.GetPositionAtDistance(d);
                Handles.DrawLine(prevPos, pos);
                prevPos = pos;
            }

            // Draw distance markers every 100 units
            Handles.color = Color.yellow;
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.yellow;
            labelStyle.fontSize = 10;

            for (float d = 0f; d <= length; d += 100f)
            {
                Vector3 pos = currentPath.GetPositionAtDistance(d);
                Handles.DrawWireCube(pos, Vector3.one * 0.5f);
                Handles.Label(pos + Vector3.up * 2f, $"{d:F0}m", labelStyle);
            }

            // Draw knot handles
            for (int i = 0; i < currentPath.KnotCount; i++)
            {
                Vector3 knotPos = currentPath.GetKnotPosition(i);
                float handleSize = HandleUtility.GetHandleSize(knotPos) * 0.1f;

                // Highlight selected knot
                if (i == selectedKnotIndex)
                {
                    Handles.color = Color.green;
                    handleSize *= 1.5f;
                }
                else
                {
                    Handles.color = Color.white;
                }

                // Draw knot sphere
                if (Handles.Button(knotPos, Quaternion.identity, handleSize, handleSize * 1.2f, Handles.SphereHandleCap))
                {
                    selectedKnotIndex = i;
                    SceneView.RepaintAll();
                }

                // Draw knot label
                Handles.Label(knotPos + Vector3.up * 1.5f, $"[{i}]");

                // Draw speed indicator for this segment
                if (i < currentPath.KnotCount - 1)
                {
                    PathSegmentSettings settings = currentPath.GetSegmentSettings(i);
                    Vector3 midPoint = Vector3.Lerp(knotPos, currentPath.GetKnotPosition(i + 1), 0.5f);
                    Handles.Label(midPoint + Vector3.up * 1f, $"Speed: {settings.speed:F0}");
                }
            }
        }

        private void HandleKnotInteraction()
        {
            Event e = Event.current;

            // Handle selected knot movement
            if (selectedKnotIndex >= 0 && selectedKnotIndex < currentPath.KnotCount)
            {
                Vector3 knotPos = currentPath.GetKnotPosition(selectedKnotIndex);

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(knotPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(currentPath, "Move Path Knot");
                    currentPath.SetKnotPosition(selectedKnotIndex, newPos);
                    EditorUtility.SetDirty(currentPath);
                }
            }

            // Shift+Click to add new knot
            if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                // Find intersection with a plane at the last knot's height
                float height = 0f;
                if (currentPath.KnotCount > 0)
                {
                    height = currentPath.GetKnotPosition(currentPath.KnotCount - 1).y;
                }

                Plane plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
                if (plane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);

                    Undo.RecordObject(currentPath, "Add Path Knot");
                    currentPath.AddKnot(hitPoint);
                    selectedKnotIndex = currentPath.KnotCount - 1;
                    EditorUtility.SetDirty(currentPath);

                    e.Use();
                }
            }

            // Delete selected knot
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedKnotIndex >= 0)
            {
                Undo.RecordObject(currentPath, "Remove Path Knot");
                currentPath.RemoveKnot(selectedKnotIndex);
                selectedKnotIndex = -1;
                EditorUtility.SetDirty(currentPath);
                e.Use();
            }
        }

        private void DrawToolPanel()
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 220, 200));
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Path Editor Tool", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.Label($"Knots: {currentPath.KnotCount}");
            GUILayout.Label($"Length: {currentPath.TotalLength:F1}m");

            GUILayout.Space(10);

            if (selectedKnotIndex >= 0 && selectedKnotIndex < currentPath.KnotCount)
            {
                GUILayout.Label($"Selected: Knot [{selectedKnotIndex}]");

                if (selectedKnotIndex < currentPath.KnotCount - 1)
                {
                    PathSegmentSettings settings = currentPath.GetSegmentSettings(selectedKnotIndex);

                    GUILayout.Label("Segment Speed:");
                    float newSpeed = EditorGUILayout.FloatField(settings.speed);
                    if (newSpeed != settings.speed)
                    {
                        settings.speed = Mathf.Max(1f, newSpeed);
                        Undo.RecordObject(currentPath, "Change Segment Speed");
                        currentPath.SetSegmentSettings(selectedKnotIndex, settings);
                        EditorUtility.SetDirty(currentPath);
                    }
                }
            }
            else
            {
                GUILayout.Label("No knot selected");
            }

            GUILayout.Space(10);
            GUILayout.Label("Shift+Click: Add knot", EditorStyles.miniLabel);
            GUILayout.Label("Delete: Remove selected", EditorStyles.miniLabel);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            Handles.EndGUI();
        }
    }
}
