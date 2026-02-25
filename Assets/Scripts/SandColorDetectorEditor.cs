using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SandColorDetector))]
public class SandColorDetectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SandColorDetector detector = (SandColorDetector)target;
        
        // Draw default inspector for references and settings
        SerializedProperty regionTool = serializedObject.FindProperty("regionTool");
        SerializedProperty fillEffect = serializedObject.FindProperty("fillEffect");
        SerializedProperty colorLibrary = serializedObject.FindProperty("colorLibrary");
        SerializedProperty particleEffectObject = serializedObject.FindProperty("particleEffectObject");
        SerializedProperty colorMatchThreshold = serializedObject.FindProperty("colorMatchThreshold");
        SerializedProperty debugMode = serializedObject.FindProperty("debugMode");
        
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(regionTool);
        EditorGUILayout.PropertyField(fillEffect);
        EditorGUILayout.PropertyField(colorLibrary);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Effects", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(particleEffectObject, new GUIContent("Particle Effect Object"));
        
        SerializedProperty pouringParticlePrefab = serializedObject.FindProperty("pouringParticlePrefab");
        SerializedProperty particleMoveSpeed = serializedObject.FindProperty("particleMoveSpeed");
        SerializedProperty particlesPerPiece = serializedObject.FindProperty("particlesPerPiece");
        SerializedProperty topPixelOffset = serializedObject.FindProperty("topPixelOffset");
        
        EditorGUILayout.PropertyField(pouringParticlePrefab, new GUIContent("Pouring Particle Prefab"));
        EditorGUILayout.PropertyField(particleMoveSpeed, new GUIContent("Particle Move Speed"));
        EditorGUILayout.PropertyField(particlesPerPiece, new GUIContent("Particles Per Piece"));
        EditorGUILayout.PropertyField(topPixelOffset, new GUIContent("Top Pixel Offset"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Detection Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(colorMatchThreshold);
        EditorGUILayout.PropertyField(debugMode);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Region Fill Settings", EditorStyles.boldLabel);
        
        // Get color library
        ColorMaterialLibrary library = (ColorMaterialLibrary)colorLibrary.objectReferenceValue;
        string[] colorNames = library != null ? library.GetAllColorNames() : new string[0];
        
        SerializedProperty regionFillSettings = serializedObject.FindProperty("regionFillSettings");
        
        if (regionFillSettings.arraySize > 0)
        {
            for (int i = 0; i < regionFillSettings.arraySize; i++)
            {
                SerializedProperty element = regionFillSettings.GetArrayElementAtIndex(i);
                SerializedProperty regionId = element.FindPropertyRelative("regionId");
                SerializedProperty colorName = element.FindPropertyRelative("colorName");
                SerializedProperty piecesNeeded = element.FindPropertyRelative("piecesNeededToFill");
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Region {regionId.intValue}", EditorStyles.boldLabel);
                
                // Color name dropdown
                if (library != null && colorNames.Length > 0)
                {
                    int currentIndex = System.Array.IndexOf(colorNames, colorName.stringValue);
                    if (currentIndex < 0) currentIndex = 0;
                    
                    int newIndex = EditorGUILayout.Popup("Color Name", currentIndex, colorNames);
                    if (newIndex >= 0 && newIndex < colorNames.Length)
                    {
                        colorName.stringValue = colorNames[newIndex];
                        
                        // Show color preview
                        Color previewColor = library.GetColorByName(colorNames[newIndex]);
                        EditorGUI.DrawRect(GUILayoutUtility.GetRect(100, 20), previewColor);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign Color Library to see available colors", MessageType.Warning);
                    EditorGUILayout.PropertyField(colorName);
                }
                
                EditorGUILayout.PropertyField(piecesNeeded);
                
                // Show progress (runtime only)
                if (Application.isPlaying)
                {
                    SerializedProperty piecesCollected = element.FindPropertyRelative("piecesCollected");
                    SerializedProperty fillProgress = element.FindPropertyRelative("fillProgress");
                    
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.IntField("Pieces Collected", piecesCollected.intValue);
                    EditorGUILayout.Slider("Fill Progress", fillProgress.floatValue, 0f, 1f);
                    EditorGUI.EndDisabledGroup();
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No regions configured. Click 'Auto-Populate Regions' to get started.", MessageType.Info);
        }
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Region Management", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Auto-Populate Regions from Region Tool"))
        {
            detector.AutoPopulateRegions();
            EditorUtility.SetDirty(detector);
        }
        
        if (GUILayout.Button("Reset All Region Progress"))
        {
            detector.ResetAllRegions();
            EditorUtility.SetDirty(detector);
        }
    }
}
