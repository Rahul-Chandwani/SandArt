using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteSequenceManager))]
public class SpriteSequenceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpriteSequenceManager manager = (SpriteSequenceManager)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sequence Control", EditorStyles.boldLabel);
        
        // Show current status
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField($"Current Sprite: {manager.GetCurrentSpriteIndex() + 1} / {manager.GetTotalSpriteCount()}");
            EditorGUILayout.LabelField($"Progress: {(manager.GetSequenceProgress() * 100f):F1}%");
            EditorGUILayout.LabelField($"Is Complete: {manager.IsSequenceComplete()}");
            
            // Show current sprite fill status
            var currentSprite = manager.GetCurrentSprite();
            if (currentSprite != null)
            {
                EditorGUILayout.LabelField($"Current Sprite Fill: {currentSprite.GetFilledRegionCount()} / {currentSprite.GetTotalRegionCount()} regions");
                EditorGUILayout.LabelField($"Fill Progress: {(currentSprite.GetFillProgress() * 100f):F1}%");
            }
            
            EditorGUILayout.Space();
            
            // Control buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset Sequence"))
            {
                manager.ResetSequence();
            }
            
            if (GUILayout.Button("Force Next Sprite"))
            {
                manager.ForceAdvanceToNextSprite();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to see sequence status and controls", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Validation
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        
        if (manager.GetTotalSpriteCount() == 0)
        {
            EditorGUILayout.HelpBox("No sprites assigned! Add SpriteRegionEditorTool components to the Sprites list.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"Sequence has {manager.GetTotalSpriteCount()} sprites configured.", MessageType.Info);
        }
        
        // Check for missing rotating object
        SerializedProperty rotatingObjectProp = serializedObject.FindProperty("rotatingObject");
        if (rotatingObjectProp.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("No rotating object assigned! The object won't rotate between sprites.", MessageType.Warning);
        }
        
        // Repaint in play mode to show live updates
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}
#endif