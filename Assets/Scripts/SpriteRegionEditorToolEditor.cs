using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteRegionEditorTool))]
public class SpriteRegionEditorToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteRegionEditorTool tool = (SpriteRegionEditorTool)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Detect Regions"))
        {
            tool.DetectRegions();
            EditorUtility.SetDirty(tool);
        }

        if (tool.regions != null && tool.regions.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Region Colors", EditorStyles.boldLabel);

            int columns = 4;
            int rows = Mathf.CeilToInt(tool.regions.Count / (float)columns);

            EditorGUI.BeginChangeCheck();

            for (int r = 0; r < rows; r++)
            {
                EditorGUILayout.BeginHorizontal();

                for (int c = 0; c < columns; c++)
                {
                    int index = r * columns + c;
                    if (index >= tool.regions.Count) break;

                    tool.regions[index].color =
                        EditorGUILayout.ColorField(tool.regions[index].color,
                                                   GUILayout.Width(60),
                                                   GUILayout.Height(40));
                }

                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tool);
                tool.GeneratePreview();
            }
        }
    }
}