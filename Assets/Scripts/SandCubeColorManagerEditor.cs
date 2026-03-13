using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SandCubeColorManager))]
public class SandCubeColorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SandCubeColorManager manager = (SandCubeColorManager)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Cube Management", EditorStyles.boldLabel);
        
        // Show cube count
        EditorGUILayout.LabelField($"Cubes Found: {manager.GetCubeCount()}");
        
        GUILayout.Space(5);
        
        // Find cubes button
        if (GUILayout.Button("Find All Sand Cubes", GUILayout.Height(30)))
        {
            Undo.RecordObject(manager, "Find All Sand Cubes");
            manager.FindAllSandCubes();
            EditorUtility.SetDirty(manager);
        }
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Color Assignment", EditorStyles.boldLabel);
        
        // Random assignment button
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // Light green
        if (GUILayout.Button("🎲 Randomly Assign Colors", GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("Random Color Assignment", 
                $"Assign random colors to {manager.GetCubeCount()} cubes?", 
                "Yes", "Cancel"))
            {
                Undo.RecordObjects(manager.GetAllCubes().ToArray(), "Random Color Assignment");
                manager.RandomlyAssignColors();
                EditorUtility.SetDirty(manager);
                
                // Force all inspectors to repaint
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(5);
        
        // Specific color assignment
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Assign Specific Color to All", GUILayout.Height(25)))
        {
            SerializedProperty specificColor = serializedObject.FindProperty("specificColorToAssign");
            string colorName = specificColor.stringValue;
            
            if (string.IsNullOrEmpty(colorName))
            {
                EditorUtility.DisplayDialog("Error", "Please specify a color name first!", "OK");
            }
            else if (EditorUtility.DisplayDialog("Specific Color Assignment", 
                $"Assign color '{colorName}' to all {manager.GetCubeCount()} cubes?", 
                "Yes", "Cancel"))
            {
                Undo.RecordObjects(manager.GetAllCubes().ToArray(), "Specific Color Assignment");
                manager.AssignColorToAll(colorName);
                EditorUtility.SetDirty(manager);
                
                // Force all inspectors to repaint
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Statistics button
        if (GUILayout.Button("📊 Show Statistics"))
        {
            manager.ShowColorStatistics();
        }
        
        // Clear colors button
        GUI.backgroundColor = new Color(1f, 0.8f, 0.8f); // Light red
        if (GUILayout.Button("Clear All Colors"))
        {
            if (EditorUtility.DisplayDialog("Clear Colors", 
                $"Clear color assignments from {manager.GetCubeCount()} cubes?", 
                "Yes", "Cancel"))
            {
                Undo.RecordObjects(manager.GetAllCubes().ToArray(), "Clear All Colors");
                manager.ClearAllColors();
                EditorUtility.SetDirty(manager);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Cleanup button
        if (GUILayout.Button("🧹 Remove Null References"))
        {
            Undo.RecordObject(manager, "Remove Null References");
            manager.RemoveNullReferences();
            EditorUtility.SetDirty(manager);
        }
        
        // Help box
        if (manager.GetCubeCount() == 0)
        {
            EditorGUILayout.HelpBox(
                "No cubes found. Click 'Find All Sand Cubes' to automatically detect all objects with SandColorIdentifier components in the scene.",
                MessageType.Info
            );
        }
        
        // Show available colors if library is assigned
        SerializedProperty colorLibrary = serializedObject.FindProperty("colorLibrary");
        if (colorLibrary.objectReferenceValue != null)
        {
            ColorMaterialLibrary library = colorLibrary.objectReferenceValue as ColorMaterialLibrary;
            string[] colors = library.GetAllColorNames();
            
            if (colors.Length > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField($"Available Colors ({colors.Length}):", EditorStyles.boldLabel);
                
                string colorList = string.Join(", ", colors);
                EditorGUILayout.HelpBox(colorList, MessageType.None);
            }
        }
    }
}