using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteRegionEditorTool))]
public class SpriteRegionEditorToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpriteRegionEditorTool tool = (SpriteRegionEditorTool)target;
        
        // Draw sprite and color library fields
        SerializedProperty sourceSprite = serializedObject.FindProperty("sourceSprite");
        SerializedProperty colorLibrary = serializedObject.FindProperty("colorLibrary");
        SerializedProperty regions = serializedObject.FindProperty("regions");
        
        EditorGUILayout.PropertyField(sourceSprite);
        EditorGUILayout.PropertyField(colorLibrary);
        
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        if (GUILayout.Button("Detect Regions"))
        {
            tool.DetectRegions();
            EditorUtility.SetDirty(tool);
        }

        if (tool.regions != null && tool.regions.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Detected Regions: {tool.regions.Count}", EditorStyles.boldLabel);
            
            // Get color names from library
            ColorMaterialLibrary library = tool.colorLibrary;
            string[] colorNames = library != null ? library.GetAllColorNames() : new string[0];
            
            EditorGUI.BeginChangeCheck();
            
            // Display each region with color picker and dropdown
            for (int i = tool.regions.Count - 1; i >= 0; i--)
            {
                var region = tool.regions[i];
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                
                // Region name
                EditorGUILayout.LabelField($"Region {i}", GUILayout.Width(70));
                
                // Color picker
                region.color = EditorGUILayout.ColorField(region.color, GUILayout.Width(60), GUILayout.Height(30));
                
                // Color name dropdown
                if (library != null && colorNames.Length > 0)
                {
                    int currentIndex = System.Array.IndexOf(colorNames, region.colorName);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    int newIndex = EditorGUILayout.Popup(currentIndex, colorNames, GUILayout.Width(100));
                    if (newIndex >= 0 && newIndex < colorNames.Length)
                    {
                        region.colorName = colorNames[newIndex];
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No Color Library", GUILayout.Width(100));
                }
                
                // Pixel count
                EditorGUILayout.LabelField($"{region.PixelCount} pixels", GUILayout.Width(80));
                
                // Remove button
                GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Remove Region", 
                        $"Are you sure you want to remove Region {i}?", 
                        "Yes", "No"))
                    {
                        tool.regions.RemoveAt(i);
                        EditorUtility.SetDirty(tool);
                        tool.GeneratePreview();
                    }
                }
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tool);
                tool.GeneratePreview();
            }
        }
    }
}
