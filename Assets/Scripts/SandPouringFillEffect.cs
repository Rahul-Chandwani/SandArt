using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SpriteRegionEditorTool))]
public class SandPouringFillEffect : MonoBehaviour
{
    [Header("Fill Settings")]
    [SerializeField] private float fillTime = 2f; // time in seconds to fill all regions
    [SerializeField] private float colorVariation = 0.4f; // 0-1, how much color varies
    [SerializeField] private float slopesteepness = 2f; // higher = steeper slope (vertical spread favored)
    
    [Header("Animation")]
    [SerializeField] private bool fillOnStart = true;
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private SpriteRegionEditorTool regionTool;
    private Texture2D fillTexture;
    private SpriteRenderer spriteRenderer;
    private int width;
    private int height;
    private bool isFilling = false;
    
    private class PixelFillData
    {
        public Vector2Int position;
        public float distanceFromCenter;
        public Color targetColor;
        public int regionIndex;
    }
    
    void Start()
    {
        regionTool = GetComponent<SpriteRegionEditorTool>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        Debug.Log("SandPouringFillEffect Start() called");
        
        if (regionTool == null)
        {
            Debug.LogError("SpriteRegionEditorTool component not found!");
            return;
        }
        
        if (regionTool.sourceSprite != null)
        {
            width = regionTool.sourceSprite.texture.width;
            height = regionTool.sourceSprite.texture.height;
            Debug.Log($"Sprite dimensions: {width}x{height}");
            Debug.Log($"Regions detected: {regionTool.regions?.Count ?? 0}");
            
            if (fillOnStart)
            {
                StartFillAnimation();
            }
        }
        else
        {
            Debug.LogWarning("No source sprite assigned to SpriteRegionEditorTool!");
        }
    }
    
    public void StartFillAnimation()
    {
        if (isFilling) return;
        
        if (regionTool.regions == null || regionTool.regions.Count == 0)
        {
            Debug.LogWarning("No regions detected. Please detect regions first.");
            return;
        }
        
        StartCoroutine(FillRegionsWithSandEffect());
    }
    
    IEnumerator FillRegionsWithSandEffect()
    {
        isFilling = true;
        Debug.Log("Starting sand pouring effect...");
        
        // Create texture with base region colors visible from start
        fillTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        fillTexture.filterMode = FilterMode.Point;
        fillTexture.wrapMode = TextureWrapMode.Clamp;
        
        Color[] pixels = new Color[width * height];
        
        // Initialize with black background
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;
        
        // Fill all regions with their base colors first
        int totalPixelsInRegions = 0;
        for (int regionIndex = 0; regionIndex < regionTool.regions.Count; regionIndex++)
        {
            var region = regionTool.regions[regionIndex];
            Debug.Log($"Region {regionIndex}: {region.pixels.Count} pixels, color: {region.color}");
            
            foreach (var pixel in region.pixels)
            {
                int pixelIndex = pixel.y * width + pixel.x;
                if (pixelIndex >= 0 && pixelIndex < pixels.Length)
                {
                    pixels[pixelIndex] = region.color;
                    totalPixelsInRegions++;
                }
            }
        }
        
        Debug.Log($"Total pixels in all regions: {totalPixelsInRegions}");
        
        fillTexture.SetPixels(pixels);
        fillTexture.Apply();
        
        // Update sprite renderer
        spriteRenderer.sprite = Sprite.Create(
            fillTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            regionTool.sourceSprite.pixelsPerUnit
        );
        
        Debug.Log("Base colors applied, starting sand pouring animation...");
        
        // Wait a moment so user can see the base colors
        yield return new WaitForSeconds(0.5f);
        
        // Prepare all pixels with distance from bottom center for sand pouring effect
        List<PixelFillData> allPixels = new List<PixelFillData>();
        
        for (int regionIndex = 0; regionIndex < regionTool.regions.Count; regionIndex++)
        {
            var region = regionTool.regions[regionIndex];
            Vector2 bottomCenter = CalculateRegionBottomCenter(region);
            
            foreach (var pixel in region.pixels)
            {
                // Calculate weighted distance for steeper slope
                float dx = Mathf.Abs(pixel.x - bottomCenter.x);
                float dy = Mathf.Abs(pixel.y - bottomCenter.y);
                
                // Weight vertical distance less to create steeper slope
                float weightedDistance = dx + (dy / slopesteepness);
                
                PixelFillData fillData = new PixelFillData
                {
                    position = pixel,
                    distanceFromCenter = weightedDistance,
                    targetColor = GetVariedColor(region.color),
                    regionIndex = regionIndex
                };
                allPixels.Add(fillData);
            }
        }
        
        // Sort by distance from bottom center (closest first)
        allPixels = allPixels.OrderBy(p => p.distanceFromCenter).ToList();
        
        Debug.Log($"Prepared {allPixels.Count} pixels for sand pouring");
        Debug.Log($"First pixel color: {allPixels[0].targetColor}, Last pixel color: {allPixels[allPixels.Count-1].targetColor}");
        
        // Calculate pixels per frame based on fill time
        float totalFrames = fillTime / Time.deltaTime;
        int pixelsPerFrame = Mathf.Max(1, Mathf.CeilToInt(allPixels.Count / totalFrames));
        
        Debug.Log($"Fill time: {fillTime}s, Pixels per frame: {pixelsPerFrame}");
        
        // Now pour sand effect with color variations
        int currentPixelIndex = 0;
        int frameCount = 0;
        
        while (currentPixelIndex < allPixels.Count)
        {
            int endIndex = Mathf.Min(currentPixelIndex + pixelsPerFrame, allPixels.Count);
            
            // Get current pixels array
            Color[] currentPixels = fillTexture.GetPixels();
            
            // Update pixels with varied colors
            for (int i = currentPixelIndex; i < endIndex; i++)
            {
                var fillData = allPixels[i];
                int pixelIndex = fillData.position.y * width + fillData.position.x;
                
                if (pixelIndex >= 0 && pixelIndex < currentPixels.Length)
                {
                    currentPixels[pixelIndex] = fillData.targetColor;
                }
            }
            
            fillTexture.SetPixels(currentPixels);
            fillTexture.Apply();
            
            currentPixelIndex = endIndex;
            frameCount++;
            
            if (frameCount % 30 == 0)
            {
                Debug.Log($"Sand pouring progress: {currentPixelIndex}/{allPixels.Count} pixels");
            }
            
            yield return null;
        }
        
        Debug.Log("Sand pouring effect complete!");
        isFilling = false;
    }
    
    Vector2 CalculateRegionCenter(SpriteRegionEditorTool.Region region)
    {
        if (region.pixels.Count == 0)
            return Vector2.zero;
        
        Vector2 sum = Vector2.zero;
        foreach (var pixel in region.pixels)
        {
            sum += new Vector2(pixel.x, pixel.y);
        }
        
        return sum / region.pixels.Count;
    }
    
    Vector2 CalculateRegionBottomCenter(SpriteRegionEditorTool.Region region)
    {
        if (region.pixels.Count == 0)
            return Vector2.zero;
        
        // Find the bottom-most Y coordinate and average X coordinate
        float minY = float.MaxValue;
        float sumX = 0;
        int count = 0;
        
        foreach (var pixel in region.pixels)
        {
            if (pixel.y < minY)
            {
                minY = pixel.y;
            }
            sumX += pixel.x;
            count++;
        }
        
        float avgX = sumX / count;
        
        return new Vector2(avgX, minY);
    }
    
    Color GetVariedColor(Color baseColor)
    {
        // Convert to HSV for better color variation
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        
        // Vary the value (brightness) significantly to create visible lighter/darker shades
        float variation = Random.Range(-colorVariation, colorVariation);
        v = Mathf.Clamp01(v + variation);
        
        // Vary saturation for more natural look
        float satVariation = Random.Range(-colorVariation * 0.5f, colorVariation * 0.5f);
        s = Mathf.Clamp01(s + satVariation);
        
        // Slightly vary hue for even more variation
        float hueVariation = Random.Range(-0.02f, 0.02f);
        h = Mathf.Repeat(h + hueVariation, 1f);
        
        Color result = Color.HSVToRGB(h, s, v);
        result.a = 1f; // Ensure full opacity
        
        return result;
    }
    
    // Public method to restart the animation
    public void RestartAnimation()
    {
        if (isFilling)
        {
            StopAllCoroutines();
            isFilling = false;
        }
        
        StartFillAnimation();
    }
}
