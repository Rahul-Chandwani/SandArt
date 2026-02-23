using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRegionEditorTool))]
[RequireComponent(typeof(SpriteRenderer))]
public class SandRegionFiller : MonoBehaviour
{
    [Header("Sand Settings")]
    public float spawnRate = 500f;       // grains per second
    public float simulationSpeed = 0.5f; // simulation multiplier
    public float colorVariation = 0.1f;  // color variation
    public float maxFrameTime = 16f;     // max milliseconds per frame (ms)
    
    [Header("Performance")]
    public int pixelUnit = 1;            // group N x N pixels into a single unit (e.g., 4 = 4x4 pixels = 1 unit)

    private SpriteRegionEditorTool regionTool;
    private Texture2D runtimeTexture;
    private Color[] pixels;

    private int width;
    private int height;

    private bool[,] sandGrid;      // where sand exists
    private bool[,] regionMask;    // valid region area
    private bool[,] filledGrid;    // pixels that have been filled with sand

    private int currentRegionIndex = -1;
    private bool isSimulating = false;

    private float spawnTimer = 0f;
    private Vector2Int spawnPoint;
    private int sandCount = 0;
    private List<Vector2Int> activeSandParticles = new List<Vector2Int>();

    void Start()
    {
        regionTool = GetComponent<SpriteRegionEditorTool>();

        Texture2D source = regionTool.sourceSprite.texture;

        width = source.width;
        height = source.height;

        runtimeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        pixels = source.GetPixels();

        runtimeTexture.SetPixels(pixels);
        runtimeTexture.Apply();

        GetComponent<SpriteRenderer>().sprite = Sprite.Create(
            runtimeTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            regionTool.sourceSprite.pixelsPerUnit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isSimulating)
        {
            StartNextRegion();
        }

        if (isSimulating)
        {
            float startTime = Time.realtimeSinceStartup;
            
            TrySpawnSand();
            SimulateSand();
            
            // Apply texture updates asynchronously to prevent frame drops
            runtimeTexture.Apply();
        }
    }

    void StartNextRegion()
    {
        currentRegionIndex++;

        if (currentRegionIndex >= regionTool.regions.Count)
        {
            Debug.Log("All regions filled.");
            return;
        }

        SetupRegion(regionTool.regions[currentRegionIndex]);
        isSimulating = true;
    }

    void SetupRegion(SpriteRegionEditorTool.Region region)
    {
        // Ensure pixelUnit is at least 1
        int unit = Mathf.Max(1, pixelUnit);
        
        int unitWidth = (width + unit - 1) / unit;
        int unitHeight = (height + unit - 1) / unit;
        
        sandGrid = new bool[unitWidth, unitHeight];
        regionMask = new bool[unitWidth, unitHeight];
        filledGrid = new bool[unitWidth, unitHeight];
        activeSandParticles.Clear();
        sandCount = 0;
        spawnTimer = 0f;

        // Convert region pixels to unit coordinates
        HashSet<Vector2Int> unitPixels = new HashSet<Vector2Int>();
        foreach (var p in region.pixels)
        {
            int ux = p.x / unit;
            int uy = p.y / unit;
            unitPixels.Add(new Vector2Int(ux, uy));
        }

        // Mark region mask with unit coordinates
        foreach (var p in unitPixels)
        {
            if (p.x >= 0 && p.x < unitWidth && p.y >= 0 && p.y < unitHeight)
                regionMask[p.x, p.y] = true;
        }

        // Find top center spawn point in unit coordinates
        int maxY = int.MinValue;
        float sumX = 0f;

        foreach (var p in unitPixels)
        {
            if (p.y > maxY)
                maxY = p.y;

            sumX += p.x;
        }

        int centerX = Mathf.RoundToInt(sumX / unitPixels.Count);
        spawnPoint = new Vector2Int(centerX, maxY);
        
        Debug.Log($"Region setup with unit size {unit}. Grid: {unitWidth}x{unitHeight} units. Spawn point: {spawnPoint}");
    }

    void TrySpawnSand()
    {
        spawnTimer += Time.deltaTime * simulationSpeed;

        float interval = 1f / spawnRate;

        while (spawnTimer >= interval)
        {
            spawnTimer -= interval;

            if (regionMask[spawnPoint.x, spawnPoint.y] &&
                !sandGrid[spawnPoint.x, spawnPoint.y])
            {
                PlaceSand(spawnPoint.x, spawnPoint.y);
            }
        }
    }

    void SimulateSand()
    {
        // Process only active sand particles instead of checking entire grid
        for (int i = activeSandParticles.Count - 1; i >= 0; i--)
        {
            Vector2Int pos = activeSandParticles[i];
            if (sandGrid[pos.x, pos.y])
            {
                TryMoveSand(pos.x, pos.y);
            }
            else
            {
                // Remove settled sand particles from active list
                activeSandParticles.RemoveAt(i);
            }
        }
    }

    void TryMoveSand(int x, int y)
    {
        if (CanMove(x, y - 1))
        {
            MoveSand(x, y, x, y - 1);
        }
        else
        {
            bool leftFirst = Random.value < 0.5f;

            if (leftFirst)
            {
                if (CanMove(x - 1, y - 1))
                    MoveSand(x, y, x - 1, y - 1);
                else if (CanMove(x + 1, y - 1))
                    MoveSand(x, y, x + 1, y - 1);
            }
            else
            {
                if (CanMove(x + 1, y - 1))
                    MoveSand(x, y, x + 1, y - 1);
                else if (CanMove(x - 1, y - 1))
                    MoveSand(x, y, x - 1, y - 1);
            }
        }
    }

    bool CanMove(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;

        if (!regionMask[x, y])
            return false;

        if (sandGrid[x, y])
            return false;

        return true;
    }

    void MoveSand(int fromX, int fromY, int toX, int toY)
    {
        int unit = Mathf.Max(1, pixelUnit);
        
        sandGrid[fromX, fromY] = false;
        sandGrid[toX, toY] = true;

        Color filledColor = GetVariedColor(regionTool.regions[currentRegionIndex].color);

        // Mark both units as filled
        filledGrid[fromX, fromY] = true;
        filledGrid[toX, toY] = true;

        // Fill all pixels in the unit blocks
        FillUnitBlock(fromX, fromY, filledColor, unit);
        FillUnitBlock(toX, toY, filledColor, unit);
    }

    void FillUnitBlock(int unitX, int unitY, Color color, int unit)
    {
        int startX = unitX * unit;
        int startY = unitY * unit;

        for (int dy = 0; dy < unit && startY + dy < height; dy++)
        {
            for (int dx = 0; dx < unit && startX + dx < width; dx++)
            {
                int pixelX = startX + dx;
                int pixelY = startY + dy;
                int pixelIndex = pixelY * width + pixelX;

                pixels[pixelIndex] = color;
                runtimeTexture.SetPixel(pixelX, pixelY, color);
            }
        }
    }

    void PlaceSand(int x, int y)
    {
        int unit = Mathf.Max(1, pixelUnit);
        
        sandGrid[x, y] = true;
        filledGrid[x, y] = true;
        activeSandParticles.Add(new Vector2Int(x, y));
        sandCount++;
        
        Color c = GetVariedColor(regionTool.regions[currentRegionIndex].color);

        // Fill all pixels in the unit block
        FillUnitBlock(x, y, c, unit);
    }

    Color GetVariedColor(Color baseColor)
    {
        float v = Random.Range(-colorVariation, colorVariation);

        return new Color(
            Mathf.Clamp01(baseColor.r + v),
            Mathf.Clamp01(baseColor.g + v),
            Mathf.Clamp01(baseColor.b + v),
            1f);
    }
}