using UnityEngine;
using System.Collections.Generic;

public class SandCubeColorManager : MonoBehaviour
{
    [Header("Color Management")]
    [SerializeField] private ColorMaterialLibrary colorLibrary;
    [SerializeField] private List<SandColorIdentifier> sandCubes = new List<SandColorIdentifier>();
    
    [Header("Auto-Detection")]
    [SerializeField] private bool autoDetectOnStart = true;
    [SerializeField] private bool includeInactiveObjects = false;
    
    void Start()
    {
        if (autoDetectOnStart)
        {
            FindAllSandCubes();
        }
    }
    
    [ContextMenu("Find All Sand Cubes")]
    public void FindAllSandCubes()
    {
        sandCubes.Clear();
        
        // Find all SandColorIdentifier components in the scene
        SandColorIdentifier[] allIdentifiers = includeInactiveObjects 
            ? FindObjectsOfType<SandColorIdentifier>(true) 
            : FindObjectsOfType<SandColorIdentifier>();
        
        foreach (SandColorIdentifier identifier in allIdentifiers)
        {
            sandCubes.Add(identifier);
            
            // Auto-assign color library if manager has one and cube doesn't
            if (colorLibrary != null && identifier.colorLibrary == null)
            {
                identifier.colorLibrary = colorLibrary;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(identifier);
                #endif
            }
        }
        
        Debug.Log($"Found {sandCubes.Count} sand cubes with SandColorIdentifier components");
        
        // Auto-assign color library if not set
        if (colorLibrary == null && sandCubes.Count > 0)
        {
            // Try to get library from first cube that has one
            foreach (SandColorIdentifier cube in sandCubes)
            {
                if (cube.colorLibrary != null)
                {
                    colorLibrary = cube.colorLibrary;
                    Debug.Log($"Auto-assigned color library from {cube.name}");
                    break;
                }
            }
        }
        
        // If we have a library, ensure all cubes have it assigned
        if (colorLibrary != null)
        {
            int librariesAssigned = 0;
            foreach (SandColorIdentifier cube in sandCubes)
            {
                if (cube.colorLibrary == null)
                {
                    cube.colorLibrary = colorLibrary;
                    librariesAssigned++;
                    #if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(cube);
                    #endif
                }
            }
            
            if (librariesAssigned > 0)
            {
                Debug.Log($"Assigned color library to {librariesAssigned} cubes that didn't have one");
            }
        }
    }
    
    [ContextMenu("Randomly Assign Colors")]
    public void RandomlyAssignColors()
    {
        if (colorLibrary == null)
        {
            Debug.LogError("No Color Library assigned! Please assign a ColorMaterialLibrary.");
            return;
        }
        
        if (sandCubes.Count == 0)
        {
            Debug.LogWarning("No sand cubes found! Use 'Find All Sand Cubes' first.");
            return;
        }
        
        string[] availableColors = colorLibrary.GetAllColorNames();
        if (availableColors.Length == 0)
        {
            Debug.LogError("Color Library has no colors defined!");
            return;
        }
        
        int colorsAssigned = 0;
        
        foreach (SandColorIdentifier cube in sandCubes)
        {
            if (cube == null) continue;
            
            // Pick a random color
            string randomColor = availableColors[Random.Range(0, availableColors.Length)];
            
            // Assign the color library and color name
            cube.colorLibrary = colorLibrary;
            cube.colorName = randomColor;
            
            cube.ChangeColor();
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(cube);
            #endif
            
            colorsAssigned++;
            
            Debug.Log($"Assigned color '{randomColor}' and library to {cube.name}");
        }
        
        #if UNITY_EDITOR
        // Force inspector refresh
        UnityEditor.Selection.activeGameObject = null;
        UnityEditor.Selection.activeGameObject = gameObject;
        #endif
        
        Debug.Log($"<color=green>Successfully assigned random colors and library to {colorsAssigned} cubes!</color>");
    }
    
    [ContextMenu("Assign Specific Color to All")]
    public void AssignSpecificColorToAll()
    {
        AssignColorToAll(specificColorToAssign);
    }
    
    [Header("Specific Color Assignment")]
    [SerializeField] private string specificColorToAssign = "";
    
    public void AssignColorToAll(string colorName)
    {
        if (colorLibrary == null)
        {
            Debug.LogError("No Color Library assigned!");
            return;
        }
        
        if (string.IsNullOrEmpty(colorName))
        {
            Debug.LogError("No color name specified!");
            return;
        }
        
        if (!colorLibrary.HasColor(colorName))
        {
            Debug.LogError($"Color '{colorName}' not found in library!");
            return;
        }
        
        if (sandCubes.Count == 0)
        {
            Debug.LogWarning("No sand cubes found!");
            return;
        }
        
        int colorsAssigned = 0;
        
        foreach (SandColorIdentifier cube in sandCubes)
        {
            if (cube == null) continue;
            
            // Assign the color library and color name
            cube.colorLibrary = colorLibrary;
            cube.colorName = colorName;
            
            cube.ChangeColor();
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(cube);
            #endif
            
            colorsAssigned++;
        }
        
        #if UNITY_EDITOR
        // Force inspector refresh
        UnityEditor.Selection.activeGameObject = null;
        UnityEditor.Selection.activeGameObject = gameObject;
        #endif
        
        Debug.Log($"<color=blue>Assigned color '{colorName}' and library to {colorsAssigned} cubes!</color>");
    }
    
    [ContextMenu("Clear All Colors")]
    public void ClearAllColors()
    {
        foreach (SandColorIdentifier cube in sandCubes)
        {
            if (cube == null) continue;
            
            cube.colorName = "";
        }
        
        Debug.Log("Cleared all color assignments");
    }
    
    [ContextMenu("Remove Null References")]
    public void RemoveNullReferences()
    {
        int removedCount = sandCubes.RemoveAll(cube => cube == null);
        Debug.Log($"Removed {removedCount} null references from the list");
    }
    
    public void AddCube(SandColorIdentifier cube)
    {
        if (cube != null && !sandCubes.Contains(cube))
        {
            sandCubes.Add(cube);
            Debug.Log($"Added {cube.name} to the cube list");
        }
    }
    
    public void RemoveCube(SandColorIdentifier cube)
    {
        if (sandCubes.Remove(cube))
        {
            Debug.Log($"Removed {cube.name} from the cube list");
        }
    }
    
    public int GetCubeCount()
    {
        return sandCubes.Count;
    }
    
    public List<SandColorIdentifier> GetAllCubes()
    {
        return new List<SandColorIdentifier>(sandCubes);
    }
    
    // Get statistics about color distribution
    [ContextMenu("Show Color Statistics")]
    public void ShowColorStatistics()
    {
        if (sandCubes.Count == 0)
        {
            Debug.Log("No cubes to analyze");
            return;
        }
        
        Dictionary<string, int> colorCounts = new Dictionary<string, int>();
        int unassignedCount = 0;
        
        foreach (SandColorIdentifier cube in sandCubes)
        {
            if (cube == null) continue;
            
            if (string.IsNullOrEmpty(cube.colorName))
            {
                unassignedCount++;
            }
            else
            {
                if (colorCounts.ContainsKey(cube.colorName))
                {
                    colorCounts[cube.colorName]++;
                }
                else
                {
                    colorCounts[cube.colorName] = 1;
                }
            }
        }
        
        Debug.Log("=== Color Statistics ===");
        Debug.Log($"Total cubes: {sandCubes.Count}");
        Debug.Log($"Unassigned: {unassignedCount}");
        
        foreach (var kvp in colorCounts)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} cubes");
        }
    }
}