using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SpriteRegionEditorTool : MonoBehaviour
{
    public Sprite sourceSprite;

    [System.Serializable]
    public class Region
    {
        [SerializeField]
        public string regionName = "Region";
        
        [SerializeField]
        public List<Vector2Int> pixels = new List<Vector2Int>();
        
        [SerializeField]
        public Color color = Color.white;
        
        public int PixelCount => pixels.Count;
    }

    [SerializeField]
    public List<Region> regions = new List<Region>();

    private Texture2D sourceTexture;
    private Texture2D previewTexture;
    private Color[] sourcePixels;

    private bool[,] visited;
    private int width;
    private int height;

    private float whiteThreshold = 0.9f;

    public void DetectRegions()
    {
        if (sourceSprite == null)
        {
            Debug.LogWarning("No source sprite assigned!");
            return;
        }

        // Clear existing regions
        int previousRegionCount = regions.Count;
        regions.Clear();

        sourceTexture = sourceSprite.texture;
        width = sourceTexture.width;
        height = sourceTexture.height;

        sourcePixels = sourceTexture.GetPixels();
        visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y] && IsWhite(GetPixel(x, y)))
                {
                    Region region = new Region();
                    FloodFill(x, y, region);
                    region.color = Random.ColorHSV();
                    region.regionName = $"Region {regions.Count + 1}";
                    regions.Add(region);
                }
            }
        }

        Debug.Log($"Detected {regions.Count} regions (previously had {previousRegionCount}).");
        GeneratePreview();
    }

    void FloodFill(int startX, int startY, Region region)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            region.pixels.Add(p);

            foreach (var n in GetNeighbors(p))
            {
                if (IsInside(n) &&
                    !visited[n.x, n.y] &&
                    IsWhite(GetPixel(n.x, n.y)))
                {
                    visited[n.x, n.y] = true;
                    queue.Enqueue(n);
                }
            }
        }
    }

    public void GeneratePreview()
    {
        if (sourceTexture == null) return;

        previewTexture = new Texture2D(width, height);
        Color[] previewPixels = new Color[width * height];

        // Start with black
        for (int i = 0; i < previewPixels.Length; i++)
            previewPixels[i] = Color.black;

        foreach (var region in regions)
        {
            foreach (var p in region.pixels)
            {
                previewPixels[p.y * width + p.x] = region.color;
            }
        }

        previewTexture.SetPixels(previewPixels);
        previewTexture.Apply();

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<SpriteRenderer>();

        renderer.sprite = Sprite.Create(previewTexture,
                                        new Rect(0, 0, width, height),
                                        new Vector2(0.5f, 0.5f),
                                        sourceSprite.pixelsPerUnit);
    }

    void OnValidate()
    {
        GeneratePreview();
    }

    bool IsWhite(Color c)
    {
        return c.r > whiteThreshold &&
               c.g > whiteThreshold &&
               c.b > whiteThreshold;
    }

    Color GetPixel(int x, int y)
    {
        return sourcePixels[y * width + x];
    }

    bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width &&
               p.y >= 0 && p.y < height;
    }

    List<Vector2Int> GetNeighbors(Vector2Int p)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(p.x + 1, p.y),
            new Vector2Int(p.x - 1, p.y),
            new Vector2Int(p.x, p.y + 1),
            new Vector2Int(p.x, p.y - 1)
        };
    }
}