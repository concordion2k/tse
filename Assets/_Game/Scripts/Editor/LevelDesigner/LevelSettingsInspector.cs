using UnityEngine;
using UnityEditor;
using LevelDesigner;

namespace LevelDesignerEditor
{
    [CustomEditor(typeof(LevelSettings))]
    public class LevelSettingsInspector : Editor
    {
        private LevelSettings settings;
        private bool showEnemyList = true;
        private Vector2 enemyListScroll;

        private SerializedProperty levelNameProp;
        private SerializedProperty descriptionProp;
        private SerializedProperty pathDataProp;
        private SerializedProperty environmentTypeProp;
        private SerializedProperty skyboxMaterialProp;
        private SerializedProperty terrainPrefabProp;
        private SerializedProperty terrainHeightOffsetProp;
        private SerializedProperty ambientLightColorProp;
        private SerializedProperty fogDensityProp;
        private SerializedProperty fogColorProp;
        private SerializedProperty enemyPlacementsProp;
        private SerializedProperty musicTrackProp;

        private void OnEnable()
        {
            settings = (LevelSettings)target;

            levelNameProp = serializedObject.FindProperty("levelName");
            descriptionProp = serializedObject.FindProperty("description");
            pathDataProp = serializedObject.FindProperty("pathData");
            environmentTypeProp = serializedObject.FindProperty("environmentType");
            skyboxMaterialProp = serializedObject.FindProperty("skyboxMaterial");
            terrainPrefabProp = serializedObject.FindProperty("terrainPrefab");
            terrainHeightOffsetProp = serializedObject.FindProperty("terrainHeightOffset");
            ambientLightColorProp = serializedObject.FindProperty("ambientLightColor");
            fogDensityProp = serializedObject.FindProperty("fogDensity");
            fogColorProp = serializedObject.FindProperty("fogColor");
            enemyPlacementsProp = serializedObject.FindProperty("enemyPlacements");
            musicTrackProp = serializedObject.FindProperty("musicTrack");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            DrawValidationWarnings();

            EditorGUILayout.Space(10);
            DrawLevelInfo();

            EditorGUILayout.Space(10);
            DrawPathSection();

            EditorGUILayout.Space(10);
            DrawEnvironmentSection();

            EditorGUILayout.Space(10);
            DrawEnemySection();

            EditorGUILayout.Space(10);
            DrawAudioSection();

            EditorGUILayout.Space(10);
            DrawActions();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

            if (settings.IsValid)
            {
                GUILayout.Label("✓ Valid", EditorStyles.miniLabel);
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("✗ Invalid", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawValidationWarnings()
        {
            if (settings.pathData == null)
            {
                EditorGUILayout.HelpBox("No path data assigned. Create or assign a LevelPathData asset.", MessageType.Warning);
            }
            else if (!settings.pathData.IsValid)
            {
                EditorGUILayout.HelpBox("Path data is invalid. Ensure it has at least 2 knots.", MessageType.Warning);
            }
        }

        private void DrawLevelInfo()
        {
            EditorGUILayout.LabelField("Level Info", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(levelNameProp);
            EditorGUILayout.PropertyField(descriptionProp);

            EditorGUI.indentLevel--;
        }

        private void DrawPathSection()
        {
            EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(pathDataProp);

            if (settings.pathData != null && settings.pathData.IsValid)
            {
                EditorGUILayout.LabelField($"Length: {settings.pathData.TotalLength:F1}m", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Knots: {settings.pathData.KnotCount}", EditorStyles.miniLabel);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Path"))
            {
                CreateNewPathData();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private void DrawEnvironmentSection()
        {
            EditorGUILayout.LabelField("Environment", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(environmentTypeProp);
            EditorGUILayout.PropertyField(skyboxMaterialProp);

            EnvironmentType envType = (EnvironmentType)environmentTypeProp.enumValueIndex;
            if (envType == EnvironmentType.Ground || envType == EnvironmentType.Hybrid)
            {
                EditorGUILayout.PropertyField(terrainPrefabProp);
                EditorGUILayout.PropertyField(terrainHeightOffsetProp);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Lighting", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(ambientLightColorProp);
            EditorGUILayout.PropertyField(fogDensityProp);
            if (fogDensityProp.floatValue > 0f)
            {
                EditorGUILayout.PropertyField(fogColorProp);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawEnemySection()
        {
            EditorGUILayout.BeginHorizontal();
            showEnemyList = EditorGUILayout.Foldout(showEnemyList, "Enemy Placements", true);
            int count = enemyPlacementsProp.arraySize;
            EditorGUILayout.LabelField($"({count})", EditorStyles.miniLabel, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            if (showEnemyList)
            {
                EditorGUI.indentLevel++;

                if (count == 0)
                {
                    EditorGUILayout.HelpBox("No enemies placed. Use the Enemy Placement tool in Scene view or add manually below.", MessageType.Info);
                }
                else
                {
                    enemyListScroll = EditorGUILayout.BeginScrollView(enemyListScroll, GUILayout.MaxHeight(200));

                    for (int i = 0; i < count; i++)
                    {
                        SerializedProperty element = enemyPlacementsProp.GetArrayElementAtIndex(i);
                        SerializedProperty prefabProp = element.FindPropertyRelative("enemyPrefab");
                        SerializedProperty distanceProp = element.FindPropertyRelative("pathDistance");

                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                        string prefabName = prefabProp.objectReferenceValue != null
                            ? prefabProp.objectReferenceValue.name
                            : "None";

                        EditorGUILayout.LabelField($"[{i}] {prefabName}", GUILayout.Width(120));
                        EditorGUILayout.LabelField($"{distanceProp.floatValue:F0}m", GUILayout.Width(60));

                        if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(40)))
                        {
                            // Expand the element for editing
                            element.isExpanded = !element.isExpanded;
                        }

                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            enemyPlacementsProp.DeleteArrayElementAtIndex(i);
                            break;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (element.isExpanded)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(element, true);
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Enemy"))
                {
                    enemyPlacementsProp.InsertArrayElementAtIndex(enemyPlacementsProp.arraySize);
                }
                if (count > 0 && GUILayout.Button("Sort by Distance"))
                {
                    settings.SortEnemyPlacements();
                    EditorUtility.SetDirty(settings);
                }
                if (count > 0 && GUILayout.Button("Clear All"))
                {
                    if (EditorUtility.DisplayDialog("Clear Enemies", "Remove all enemy placements?", "Yes", "Cancel"))
                    {
                        enemyPlacementsProp.ClearArray();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
        }

        private void DrawAudioSection()
        {
            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(musicTrackProp);

            EditorGUI.indentLevel--;
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Focus in Scene"))
            {
                FocusPathInScene();
            }

            if (GUILayout.Button("Duplicate Level"))
            {
                DuplicateLevel();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewPathData()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Path Data",
                "NewPath",
                "asset",
                "Choose a location for the new path data",
                "Assets/_Game/LevelData/Paths"
            );

            if (!string.IsNullOrEmpty(path))
            {
                LevelPathData newPath = ScriptableObject.CreateInstance<LevelPathData>();
                AssetDatabase.CreateAsset(newPath, path);
                AssetDatabase.SaveAssets();

                pathDataProp.objectReferenceValue = newPath;
                serializedObject.ApplyModifiedProperties();

                Selection.activeObject = newPath;
            }
        }

        private void FocusPathInScene()
        {
            if (settings.pathData != null && settings.pathData.IsValid)
            {
                Vector3 pathStart = settings.pathData.GetPositionAtDistance(0f);
                SceneView.lastActiveSceneView.LookAt(pathStart);
            }
        }

        private void DuplicateLevel()
        {
            string path = AssetDatabase.GetAssetPath(settings);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CopyAsset(path, newPath);
            AssetDatabase.SaveAssets();

            LevelSettings newSettings = AssetDatabase.LoadAssetAtPath<LevelSettings>(newPath);
            Selection.activeObject = newSettings;
        }
    }
}
