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
}
