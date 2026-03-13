using UnityEngine;

public class SandColorIdentifier : MonoBehaviour
{
    [Header("Color Identity")]
    public string colorName = "";
    
    // Optional reference to the library for validation
    [Header("Optional Reference")]
    public ColorMaterialLibrary colorLibrary;
    
    public string GetColorName()
    {
        return colorName;
    }
    
    public void SetColorName(string name)
    {
        colorName = name;
    }
    
    public bool HasColorName()
    {
        return !string.IsNullOrEmpty(colorName);
    }
    public void OnValidate()
    {
        ChangeColor();
    }

    public void ChangeColor()
    {
        if (string.IsNullOrEmpty(colorName)) return;
        
        // Use the assigned library first, fallback to Instance
        ColorMaterialLibrary library = colorLibrary != null ? colorLibrary : ColorMaterialLibrary.Instance;
        if (library == null) return;
        
        Material newMaterial = library.GetMaterialByName(colorName);
        if (newMaterial == null) return;
        
        // Apply material to all MeshRenderers on this object and its children
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.sharedMaterial = newMaterial;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(renderer);
            #endif
        }
        
        // Also apply to the main MeshRenderer if it exists
        MeshRenderer mainRenderer = GetComponent<MeshRenderer>();
        if (mainRenderer != null)
        {
            mainRenderer.sharedMaterial = newMaterial;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(mainRenderer);
            #endif
        }
        
        Debug.Log($"Applied material '{newMaterial.name}' for color '{colorName}' to {gameObject.name}");
    }
}
