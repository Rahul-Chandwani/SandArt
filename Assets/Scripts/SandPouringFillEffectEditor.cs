using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SandPouringFillEffect))]
public class SandPouringFillEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SandPouringFillEffect effect = (SandPouringFillEffect)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        // Add helper section for unlock sequence
        SerializedProperty useRegionLocking = serializedObject.FindProperty("useRegionLocking");
        SerializedProperty unlockSequence = serializedObject.FindProperty("unlockSequence");
        
        if (useRegionLocking.boolValue && unlockSequence != null)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Unlock Sequence: Set the order in which regions unlock. " +
                "Leave empty for default sequential order (0, 1, 2, ...). " +
                "Example: [2, 0, 5, 1] will unlock regions in that order.",
                MessageType.Info
            );
            
            // Button to auto-populate with sequential order
            if (GUILayout.Button("Auto-Fill Sequential Order"))
            {
                SpriteRegionEditorTool regionTool = effect.GetComponent<SpriteRegionEditorTool>();
                if (regionTool != null && regionTool.regions != null)
                {
                    Undo.RecordObject(effect, "Auto-Fill Unlock Sequence");
                    unlockSequence.ClearArray();
                    for (int i = 0; i < regionTool.regions.Count; i++)
                    {
                        unlockSequence.InsertArrayElementAtIndex(i);
                        unlockSequence.GetArrayElementAtIndex(i).intValue = i;
                    }
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(effect);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No regions detected. Please detect regions first.", "OK");
                }
            }
            
            // Button to clear sequence
            if (unlockSequence.arraySize > 0 && GUILayout.Button("Clear Sequence (Use Default)"))
            {
                Undo.RecordObject(effect, "Clear Unlock Sequence");
                unlockSequence.ClearArray();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(effect);
            }
        }
    }
}
