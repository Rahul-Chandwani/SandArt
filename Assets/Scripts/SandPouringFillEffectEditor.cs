using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SandPouringFillEffect))]
public class SandPouringFillEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SandPouringFillEffect effect = (SandPouringFillEffect)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Start Fill Animation", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                effect.StartFillAnimation();
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test the fill animation.");
            }
        }
        
        if (GUILayout.Button("Restart Animation", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                effect.RestartAnimation();
            }
            else
            {
                Debug.LogWarning("Enter Play Mode to test the fill animation.");
            }
        }
        
        GUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "1. Assign a sprite to the SpriteRegionEditorTool\n" +
            "2. Click 'Detect Regions' in the SpriteRegionEditorTool\n" +
            "3. Enter Play Mode\n" +
            "4. The sand pouring effect will start automatically (if 'Fill On Start' is enabled)",
            MessageType.Info
        );
    }
}
