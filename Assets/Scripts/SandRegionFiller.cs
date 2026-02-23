using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRegionEditorTool))]
[RequireComponent(typeof(SpriteRenderer))]
public class SandRegionFiller : MonoBehaviour
{
    [Header("Sand Settings")]
    public float spawnRate = 3000f;      // grains per second
    public float simulationSpeed = 1f;   // simulation multiplier
    public float colorVariation = 0.1f;

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
            SimulateSand();
            TrySpawnSand();
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
        sandGrid = new bool[width, height];
        regionMask = new bool[width, height];
        filledGrid = new bool[width, height];

        foreach (var p in region.pixels)
            regionMask[p.x, p.y] = true;

        // Find top center spawn point
        int maxY = int.MinValue;
        float sumX = 0f;

        foreach (var p in region.pixels)
        {
            if (p.y > maxY)
                maxY = p.y;

            sumX += p.x;
        }

        int centerX = Mathf.RoundToInt(sumX / region.pixels.Count);
        spawnPoint = new Vector2Int(centerX, maxY);
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
        for (int y = 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!sandGrid[x, y]) continue;

                TryMoveSand(x, y);
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
        sandGrid[fromX, fromY] = false;
        sandGrid[toX, toY] = true;

        Color filledColor = GetVariedColor(regionTool.regions[currentRegionIndex].color);

        // Mark both pixels as filled and color them
        filledGrid[fromX, fromY] = true;
        filledGrid[toX, toY] = true;

        pixels[fromY * width + fromX] = filledColor;
        pixels[toY * width + toX] = filledColor;

        runtimeTexture.SetPixel(fromX, fromY, filledColor);
        runtimeTexture.SetPixel(toX, toY, filledColor);
    }

    void PlaceSand(int x, int y)
    {
        sandGrid[x, y] = true;
        filledGrid[x, y] = true;
        
        Color c = GetVariedColor(regionTool.regions[currentRegionIndex].color);

        pixels[y * width + x] = c;
        runtimeTexture.SetPixel(x, y, c);
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