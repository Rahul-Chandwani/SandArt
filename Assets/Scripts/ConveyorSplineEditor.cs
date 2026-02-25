using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ConveyorSpline))]
public class ConveyorSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ConveyorSpline spline = (ConveyorSpline)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Setup Instructions:\n\n" +
            "1. Add child transforms as spline points (or they'll be auto-detected)\n" +
            "2. Add ConveyorCollider to each mesh part inside this conveyor\n" +
            "3. Assign a sand piece prefab\n" +
            "4. Sand cubes will attach when they collide with any conveyor part",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Add Colliders to All Children", GUILayout.Height(30)))
        {
            AddCollidersToChildren(spline);
        }
    }
    
    void AddCollidersToChildren(ConveyorSpline spline)
    {
        int added = 0;
        
        foreach (Transform child in spline.transform)
        {
            // Check if it has a mesh renderer
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Add ConveyorCollider if not present
                ConveyorCollider conveyorCollider = child.GetComponent<ConveyorCollider>();
                if (conveyorCollider == null)
                {
                    child.gameObject.AddComponent<ConveyorCollider>();
                    added++;
                }
                
                // Ensure it has a collider
                Collider col = child.GetComponent<Collider>();
                if (col == null)
                {
                    MeshCollider meshCol = child.gameObject.AddComponent<MeshCollider>();
                    meshCol.convex = true;
                    meshCol.isTrigger = true;
                }
                else
                {
                    col.isTrigger = true;
                }
            }
        }
        
        EditorUtility.SetDirty(spline);
        Debug.Log($"Added ConveyorCollider to {added} children");
    }
}
