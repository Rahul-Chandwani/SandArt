using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteRegionEditorTool))]
public class SpriteRegionEditorToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpriteRegionEditorTool tool = (SpriteRegionEditorTool)target;

        // Use serializedObject for the top fields for built-in undo support
        serializedObject.Update();
        SerializedProperty sourceSprite = serializedObject.FindProperty("sourceSprite");
        SerializedProperty colorLibrary = serializedObject.FindProperty("colorLibrary");

        EditorGUILayout.PropertyField(sourceSprite);
        EditorGUILayout.PropertyField(colorLibrary);

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        if (GUILayout.Button("Detect Regions"))
        {
            // Record state for Undo and Dirty marking
            Undo.RecordObject(tool, "Detect Regions");
            tool.DetectRegions();
            EditorUtility.SetDirty(tool);
        }

        if (tool.regions != null && tool.regions.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Detected Regions: {tool.regions.Count}", EditorStyles.boldLabel);

            ColorMaterialLibrary library = tool.colorLibrary;
            string[] colorNames = library != null ? library.GetAllColorNames() : new string[0];

            for (int i = tool.regions.Count - 1; i >= 0; i--)
            {
                var region = tool.regions[i];

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"Region {i}", GUILayout.Width(70));

                // COLOR PICKER CHANGE CHECK
                EditorGUI.BeginChangeCheck();
                Color newColor = EditorGUILayout.ColorField(region.color, GUILayout.Width(60), GUILayout.Height(30));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tool, "Change Region Color");
                    region.color = newColor;
                    EditorUtility.SetDirty(tool);
                    tool.GeneratePreview();
                }

                // DROPDOWN CHANGE CHECK
                if (library != null && colorNames.Length > 0)
                {
                    int currentIndex = System.Array.IndexOf(colorNames, region.colorName);
                    if (currentIndex < 0) currentIndex = 0;

                    EditorGUI.BeginChangeCheck();
                    int newIndex = EditorGUILayout.Popup(currentIndex, colorNames, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(tool, "Change Region Dropdown");
                        region.colorName = colorNames[newIndex];
                        // Automatically update the color picker to match the library choice
                        region.color = library.GetColorByName(region.colorName);
                        EditorUtility.SetDirty(tool);
                        tool.GeneratePreview();
                    }
                }

                // REMOVE BUTTON
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(30)))
                {
                    Undo.RecordObject(tool, "Remove Region");
                    tool.regions.RemoveAt(i);
                    EditorUtility.SetDirty(tool);
                    tool.GeneratePreview();
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
        }
    }
}