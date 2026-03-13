using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SandColorIdentifier))]
public class SandColorIdentifierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SandColorIdentifier identifier = (SandColorIdentifier)target;
        
        SerializedProperty colorLibrary = serializedObject.FindProperty("colorLibrary");
        
        EditorGUILayout.LabelField("Color Identity", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(colorLibrary);
        
        serializedObject.ApplyModifiedProperties();
        
        // Get color library
        ColorMaterialLibrary library = identifier.colorLibrary;
        
        if (library != null)
        {
            string[] colorNames = library.GetAllColorNames();
            
            if (colorNames.Length > 0)
            {
                // Find current index
                int currentIndex = System.Array.IndexOf(colorNames, identifier.colorName);
                if (currentIndex < 0) currentIndex = 0;
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Select Color:", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup("Color Name", currentIndex, colorNames);
                if (EditorGUI.EndChangeCheck() && newIndex >= 0 && newIndex < colorNames.Length)
                {
                    Undo.RecordObject(identifier, "Change Color");
                    identifier.colorName = colorNames[newIndex];
                    identifier.ChangeColor();
                    EditorUtility.SetDirty(identifier);
                }
                
                if (newIndex >= 0 && newIndex < colorNames.Length)
                {
                    // Show color preview
                    Color previewColor = library.GetColorByName(colorNames[newIndex]);
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Color Preview:");
                    Rect rect = GUILayoutUtility.GetRect(200, 40);
                    EditorGUI.DrawRect(rect, previewColor);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Color Library has no colors defined.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Assign a Color Library to select colors.", MessageType.Info);
            
            // Fallback to text field
            SerializedProperty colorName = serializedObject.FindProperty("colorName");
            EditorGUILayout.PropertyField(colorName);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
