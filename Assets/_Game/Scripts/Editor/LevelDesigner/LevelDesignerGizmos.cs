using UnityEngine;
using UnityEditor;
using LevelDesigner;

namespace LevelDesignerEditor
{
    public static class LevelDesignerGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawLevelPathDataGizmos(LevelPathFollower follower, GizmoType gizmoType)
        {
            if (follower.PathData == null || !follower.PathData.IsValid) return;

            LevelPathData pathData = follower.PathData;
            bool isSelected = (gizmoType & GizmoType.Selected) != 0;

            // Draw path curve
            Color pathColor = isSelected ? Color.cyan : new Color(0f, 1f, 1f, 0.5f);
            Gizmos.color = pathColor;

            float length = pathData.TotalLength;
            Vector3 prevPos = pathData.GetPositionAtDistance(0f);
            float step = isSelected ? 1f : 5f;

            for (float d = step; d <= length; d += step)
            {
                Vector3 pos = pathData.GetPositionAtDistance(d);
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }

            // Draw start and end markers
            Gizmos.color = Color.green;
            Vector3 startPos = pathData.GetPositionAtDistance(0f);
            Gizmos.DrawWireSphere(startPos, 1f);
            DrawArrow(startPos, pathData.GetTangentAtDistance(0f) * 3f);

            Gizmos.color = Color.red;
            Vector3 endPos = pathData.GetPositionAtDistance(length);
            Gizmos.DrawWireSphere(endPos, 1f);

            // Draw distance markers when selected
            if (isSelected)
            {
                Gizmos.color = Color.yellow;
                for (float d = 100f; d < length; d += 100f)
                {
                    Vector3 pos = pathData.GetPositionAtDistance(d);
                    Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
                }
            }

            // Draw current position if playing
            if (Application.isPlaying && follower.IsFollowing)
            {
                Gizmos.color = Color.magenta;
                Vector3 currentPos = follower.GetPathPosition();
                Gizmos.DrawWireSphere(currentPos, 1.5f);
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawLevelManagerGizmos(LevelManager manager, GizmoType gizmoType)
        {
            if (manager.CurrentSettings == null) return;

            LevelSettings settings = manager.CurrentSettings;
            if (settings.pathData == null || !settings.pathData.IsValid) return;

            bool isSelected = (gizmoType & GizmoType.Selected) != 0;
            if (!isSelected) return;

            LevelPathData pathData = settings.pathData;

            // Draw enemy placements
            if (settings.enemyPlacements != null)
            {
                foreach (EnemyPlacementData placement in settings.enemyPlacements)
                {
                    if (placement.enemyPrefab == null) continue;

                    Vector3 pathPos = pathData.GetPositionAtDistance(placement.pathDistance);
                    Vector3 pathForward = pathData.GetTangentAtDistance(placement.pathDistance);
                    Vector3 pathUp = pathData.GetUpAtDistance(placement.pathDistance);
                    Vector3 pathRight = Vector3.Cross(pathUp, pathForward).normalized;

                    Vector3 worldPos = pathPos
                        + pathRight * placement.offsetFromPath.x
                        + pathUp * placement.offsetFromPath.y
                        + pathForward * placement.offsetFromPath.z;

                    // Color by enemy type
                    if (placement.enemyPrefab.name.Contains("Light"))
                        Gizmos.color = Color.yellow;
                    else if (placement.enemyPrefab.name.Contains("Medium"))
                        Gizmos.color = new Color(1f, 0.5f, 0f);
                    else if (placement.enemyPrefab.name.Contains("Heavy"))
                        Gizmos.color = Color.red;
                    else
                        Gizmos.color = Color.magenta;

                    // Draw enemy marker
                    float size = 1f * placement.scaleMultiplier;
                    Gizmos.DrawWireCube(worldPos, Vector3.one * size);

                    // Draw line to path
                    Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
                    Gizmos.DrawLine(worldPos, pathPos);

                    // Draw formation preview
                    if (placement.useFormation)
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
                        DrawFormationPreview(worldPos, placement.formation, placement.formationCount, placement.formationSpacing);
                    }
                }
            }
        }

        private static void DrawArrow(Vector3 position, Vector3 direction)
        {
            if (direction.magnitude < 0.01f) return;

            Gizmos.DrawRay(position, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

            Gizmos.DrawRay(position + direction, right * direction.magnitude * 0.3f);
            Gizmos.DrawRay(position + direction, left * direction.magnitude * 0.3f);
        }

        private static void DrawFormationPreview(Vector3 center, FormationType formation, int count, float spacing)
        {
            switch (formation)
            {
                case FormationType.Line:
                    float lineStart = -(count - 1) * spacing * 0.5f;
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 pos = center + new Vector3(lineStart + i * spacing, 0f, 0f);
                        Gizmos.DrawWireSphere(pos, 0.5f);
                    }
                    break;

                case FormationType.V:
                    Gizmos.DrawWireSphere(center, 0.5f);
                    for (int i = 1; i < count; i++)
                    {
                        int side = (i % 2 == 1) ? 1 : -1;
                        int row = (i + 1) / 2;
                        Vector3 pos = center + new Vector3(side * row * spacing, 0f, -row * spacing);
                        Gizmos.DrawWireSphere(pos, 0.5f);
                    }
                    break;

                case FormationType.Circle:
                    for (int i = 0; i < count; i++)
                    {
                        float angle = i * (360f / count) * Mathf.Deg2Rad;
                        Vector3 pos = center + new Vector3(Mathf.Cos(angle) * spacing, 0f, Mathf.Sin(angle) * spacing);
                        Gizmos.DrawWireSphere(pos, 0.5f);
                    }
                    break;

                case FormationType.Diamond:
                    Gizmos.DrawWireSphere(center, 0.5f);
                    if (count > 1) Gizmos.DrawWireSphere(center + new Vector3(spacing, 0f, 0f), 0.5f);
                    if (count > 2) Gizmos.DrawWireSphere(center + new Vector3(-spacing, 0f, 0f), 0.5f);
                    if (count > 3) Gizmos.DrawWireSphere(center + new Vector3(0f, 0f, spacing), 0.5f);
                    if (count > 4) Gizmos.DrawWireSphere(center + new Vector3(0f, 0f, -spacing), 0.5f);
                    break;
            }
        }
    }
}
