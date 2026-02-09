using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using LevelDesigner;

namespace LevelDesignerEditor
{
    [EditorTool("Enemy Placement", typeof(LevelManager))]
    public class EnemyPlacementTool : EditorTool
    {
        private GUIContent toolIcon;
        private LevelSettings currentSettings;
        private LevelPathData currentPath;
        private int selectedEnemyIndex = -1;
        private GameObject selectedPrefab;
        private int prefabSelectionIndex = 0;

        // Default enemy prefabs (will be populated from project)
        private string[] defaultEnemyPaths = new string[]
        {
            "Assets/_Game/Prefabs/Enemy_Light.prefab",
            "Assets/_Game/Prefabs/Enemy_Medium.prefab",
            "Assets/_Game/Prefabs/Enemy_Heavy.prefab"
        };
        private GameObject[] enemyPrefabs;
        private string[] enemyNames;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (toolIcon == null)
                {
                    toolIcon = new GUIContent(
                        EditorGUIUtility.IconContent("AvatarSelector").image,
                        "Enemy Placement Tool"
                    );
                }
                return toolIcon;
            }
        }

        public override void OnActivated()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            LoadEnemyPrefabs();
        }

        public override void OnWillBeDeactivated()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void LoadEnemyPrefabs()
        {
            var prefabList = new System.Collections.Generic.List<GameObject>();
            var nameList = new System.Collections.Generic.List<string>();

            foreach (string path in defaultEnemyPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabList.Add(prefab);
                    nameList.Add(prefab.name);
                }
            }

            enemyPrefabs = prefabList.ToArray();
            enemyNames = nameList.ToArray();

            if (enemyPrefabs.Length > 0)
            {
                selectedPrefab = enemyPrefabs[0];
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            LevelManager manager = target as LevelManager;
            if (manager == null || manager.CurrentSettings == null) return;

            currentSettings = manager.CurrentSettings;
            currentPath = currentSettings.pathData;

            if (currentPath == null || !currentPath.IsValid) return;

            DrawPathPreview();
            DrawEnemyPlacements();
            HandleEnemyInteraction();
            DrawToolPanel();
        }

        private void DrawPathPreview()
        {
            // Draw a faint path preview
            Handles.color = new Color(0f, 1f, 1f, 0.3f);
            float length = currentPath.TotalLength;
            Vector3 prevPos = currentPath.GetPositionAtDistance(0f);

            for (float d = 5f; d <= length; d += 5f)
            {
                Vector3 pos = currentPath.GetPositionAtDistance(d);
                Handles.DrawLine(prevPos, pos);
                prevPos = pos;
            }
        }

        private void DrawEnemyPlacements()
        {
            if (currentSettings.enemyPlacements == null) return;

            for (int i = 0; i < currentSettings.enemyPlacements.Count; i++)
            {
                EnemyPlacementData placement = currentSettings.enemyPlacements[i];
                Vector3 worldPos = CalculateWorldPosition(placement);

                float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.15f;

                // Color based on enemy type and selection
                if (i == selectedEnemyIndex)
                {
                    Handles.color = Color.green;
                    handleSize *= 1.3f;
                }
                else if (placement.enemyPrefab != null)
                {
                    if (placement.enemyPrefab.name.Contains("Light"))
                        Handles.color = Color.yellow;
                    else if (placement.enemyPrefab.name.Contains("Medium"))
                        Handles.color = new Color(1f, 0.5f, 0f);
                    else if (placement.enemyPrefab.name.Contains("Heavy"))
                        Handles.color = Color.red;
                    else
                        Handles.color = Color.magenta;
                }
                else
                {
                    Handles.color = Color.gray;
                }

                // Draw enemy marker
                if (Handles.Button(worldPos, Quaternion.identity, handleSize, handleSize * 1.2f, Handles.CubeHandleCap))
                {
                    selectedEnemyIndex = i;
                    SceneView.RepaintAll();
                }

                // Draw connection to path
                Vector3 pathPos = currentPath.GetPositionAtDistance(placement.pathDistance);
                Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.5f);
                Handles.DrawDottedLine(worldPos, pathPos, 2f);

                // Draw label
                string label = placement.enemyPrefab != null ? placement.enemyPrefab.name : "None";
                Handles.Label(worldPos + Vector3.up * 2f, $"[{i}] {label}");
            }
        }

        private void HandleEnemyInteraction()
        {
            Event e = Event.current;

            // Handle selected enemy movement
            if (selectedEnemyIndex >= 0 && selectedEnemyIndex < currentSettings.enemyPlacements.Count)
            {
                EnemyPlacementData placement = currentSettings.enemyPlacements[selectedEnemyIndex];
                Vector3 worldPos = CalculateWorldPosition(placement);

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(currentSettings, "Move Enemy Placement");

                    // Convert world position back to path-relative offset
                    Vector3 newOffset = WorldPositionToOffset(newPos, placement.pathDistance);
                    placement.offsetFromPath = newOffset;

                    EditorUtility.SetDirty(currentSettings);
                }
            }

            // Ctrl+Click to place new enemy
            if (e.type == EventType.MouseDown && e.button == 0 && e.control && selectedPrefab != null)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                // Find closest point on path
                float closestDistance = 0f;
                float closestDist = float.MaxValue;

                for (float d = 0f; d <= currentPath.TotalLength; d += 5f)
                {
                    Vector3 pathPos = currentPath.GetPositionAtDistance(d);
                    // Calculate distance from ray to point
                    Vector3 rayToPoint = pathPos - ray.origin;
                    float projectedDist = Vector3.Dot(rayToPoint, ray.direction);
                    Vector3 closestPointOnRay = ray.origin + ray.direction * Mathf.Max(0f, projectedDist);
                    float dist = Vector3.Distance(closestPointOnRay, pathPos);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestDistance = d;
                    }
                }

                // Calculate offset from path
                Vector3 pathPoint = currentPath.GetPositionAtDistance(closestDistance);
                Plane plane = new Plane(Vector3.up, pathPoint);
                if (plane.Raycast(ray, out float hitDist))
                {
                    Vector3 hitPoint = ray.GetPoint(hitDist);
                    Vector3 offset = WorldPositionToOffset(hitPoint, closestDistance);

                    Undo.RecordObject(currentSettings, "Add Enemy Placement");

                    EnemyPlacementData newPlacement = new EnemyPlacementData
                    {
                        enemyPrefab = selectedPrefab,
                        pathDistance = closestDistance,
                        offsetFromPath = offset
                    };

                    currentSettings.AddEnemyPlacement(newPlacement);
                    selectedEnemyIndex = currentSettings.enemyPlacements.Count - 1;
                    EditorUtility.SetDirty(currentSettings);

                    e.Use();
                }
            }

            // Delete selected enemy
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedEnemyIndex >= 0)
            {
                Undo.RecordObject(currentSettings, "Remove Enemy Placement");
                currentSettings.RemoveEnemyPlacement(selectedEnemyIndex);
                selectedEnemyIndex = -1;
                EditorUtility.SetDirty(currentSettings);
                e.Use();
            }
        }

        private Vector3 CalculateWorldPosition(EnemyPlacementData placement)
        {
            Vector3 pathPos = currentPath.GetPositionAtDistance(placement.pathDistance);
            Vector3 pathForward = currentPath.GetTangentAtDistance(placement.pathDistance);
            Vector3 pathUp = currentPath.GetUpAtDistance(placement.pathDistance);
            Vector3 pathRight = Vector3.Cross(pathUp, pathForward).normalized;

            return pathPos
                + pathRight * placement.offsetFromPath.x
                + pathUp * placement.offsetFromPath.y
                + pathForward * placement.offsetFromPath.z;
        }

        private Vector3 WorldPositionToOffset(Vector3 worldPos, float pathDistance)
        {
            Vector3 pathPos = currentPath.GetPositionAtDistance(pathDistance);
            Vector3 pathForward = currentPath.GetTangentAtDistance(pathDistance);
            Vector3 pathUp = currentPath.GetUpAtDistance(pathDistance);
            Vector3 pathRight = Vector3.Cross(pathUp, pathForward).normalized;

            Vector3 delta = worldPos - pathPos;

            return new Vector3(
                Vector3.Dot(delta, pathRight),
                Vector3.Dot(delta, pathUp),
                Vector3.Dot(delta, pathForward)
            );
        }

        private void DrawToolPanel()
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(10, 10, 220, 280));
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Enemy Placement Tool", EditorStyles.boldLabel);
            GUILayout.Space(5);

            int enemyCount = currentSettings.enemyPlacements?.Count ?? 0;
            GUILayout.Label($"Enemies: {enemyCount}");

            GUILayout.Space(10);

            // Prefab selection
            GUILayout.Label("Enemy Type:");
            if (enemyNames != null && enemyNames.Length > 0)
            {
                int newIndex = EditorGUILayout.Popup(prefabSelectionIndex, enemyNames);
                if (newIndex != prefabSelectionIndex)
                {
                    prefabSelectionIndex = newIndex;
                    selectedPrefab = enemyPrefabs[prefabSelectionIndex];
                }
            }

            GUILayout.Space(10);

            // Selected enemy info
            if (selectedEnemyIndex >= 0 && selectedEnemyIndex < enemyCount)
            {
                EnemyPlacementData placement = currentSettings.enemyPlacements[selectedEnemyIndex];

                GUILayout.Label($"Selected: [{selectedEnemyIndex}]", EditorStyles.boldLabel);
                GUILayout.Label($"Distance: {placement.pathDistance:F1}m");
                GUILayout.Label($"Offset: {placement.offsetFromPath:F1}");

                GUILayout.Space(5);

                // Allow changing the prefab of selected enemy
                if (GUILayout.Button("Set to Current Type") && selectedPrefab != null)
                {
                    Undo.RecordObject(currentSettings, "Change Enemy Type");
                    placement.enemyPrefab = selectedPrefab;
                    EditorUtility.SetDirty(currentSettings);
                }

                // Formation toggle
                bool newUseFormation = EditorGUILayout.Toggle("Use Formation", placement.useFormation);
                if (newUseFormation != placement.useFormation)
                {
                    Undo.RecordObject(currentSettings, "Toggle Formation");
                    placement.useFormation = newUseFormation;
                    EditorUtility.SetDirty(currentSettings);
                }
            }
            else
            {
                GUILayout.Label("No enemy selected");
            }

            GUILayout.Space(10);
            GUILayout.Label("Ctrl+Click: Add enemy", EditorStyles.miniLabel);
            GUILayout.Label("Delete: Remove selected", EditorStyles.miniLabel);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            Handles.EndGUI();
        }
    }
}
