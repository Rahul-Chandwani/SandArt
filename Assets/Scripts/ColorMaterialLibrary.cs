using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ColorMaterialLibrary", menuName = "Sand System/Color Material Library")]
public class ColorMaterialLibrary : ScriptableObject
{
    [System.Serializable]
    public class ColorMaterial
    {
        public string colorName;
        public Material material;
        public Color color;
    }
    
    [Header("Color Materials")]
    public List<ColorMaterial> colorMaterials = new List<ColorMaterial>();
    
    public Material GetMaterialByName(string colorName)
    {
        ColorMaterial colorMat = colorMaterials.Find(cm => cm.colorName == colorName);
        return colorMat?.material;
    }
    
    public Color GetColorByName(string colorName)
    {
        ColorMaterial colorMat = colorMaterials.Find(cm => cm.colorName == colorName);
        return colorMat != null ? colorMat.color : Color.white;
    }
    
    public string[] GetAllColorNames()
    {
        string[] names = new string[colorMaterials.Count];
        for (int i = 0; i < colorMaterials.Count; i++)
        {
            names[i] = colorMaterials[i].colorName;
        }
        return names;
    }
    
    public bool HasColor(string colorName)
    {
        return colorMaterials.Exists(cm => cm.colorName == colorName);
    }
}
