using UnityEngine;

public class SandCubeGrinder : MonoBehaviour
{
    [Header("Grinding Settings")]
    public int totalSandPieces = 10;
    
    [Header("Smoothing")]
    [SerializeField] private float scaleTransitionSpeed = 20f; // Increased for faster grinding
    
    private bool isGrinding = false;
    private Vector3 originalScale;
    private Vector3 fixedPosition; // Store the position where grinding started
    private bool hasFixedPosition = false;
    private int piecesSpawned = 0;
    private float targetScaleX = 1f;
    private float currentScaleX = 1f;
    
    void Start()
    {
        originalScale = transform.localScale;
        currentScaleX = 1f;
        targetScaleX = 1f;
    }
    
    void Update()
    {
        if (isGrinding)
        {
            // Keep cube at fixed position
            if (hasFixedPosition)
            {
                transform.position = fixedPosition;
            }
            
            // Calculate target scale based on pieces spawned
            targetScaleX = 1f - ((float)piecesSpawned / totalSandPieces);
            
            // Smoothly interpolate to target scale
            currentScaleX = Mathf.Lerp(currentScaleX, targetScaleX, Time.deltaTime * scaleTransitionSpeed);
            
            // Apply scale only on X axis
            Vector3 newScale = originalScale;
            newScale.x = originalScale.x * currentScaleX;
            transform.localScale = newScale;
            
            // Destroy when all pieces are spawned and scale is nearly zero
            if (piecesSpawned >= totalSandPieces && currentScaleX < 0.01f)
            {
                Destroy(gameObject);
            }
        }
    }
    
    public void StartGrinding(Vector3 contactPoint, Vector3 contactNormal)
    {
        // Always start grinding, even if already grinding (reset)
        isGrinding = true;
        fixedPosition = transform.position; // Lock current position
        hasFixedPosition = true;
        
        // Disable sprite renderer (arrow) when grinding starts
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            Debug.Log($"{gameObject.name}: Disabled sprite renderer");
        }
        
        // Reset pieces spawned if starting fresh grind
        if (piecesSpawned >= totalSandPieces)
        {
            piecesSpawned = 0;
            currentScaleX = 1f;
            targetScaleX = 1f;
            transform.localScale = originalScale; // Reset scale
        }
        
        Debug.Log($"{gameObject.name} started grinding at {fixedPosition}. Will spawn {totalSandPieces} sand pieces. Current pieces: {piecesSpawned}");
    }
    
    public bool IsGrinding()
    {
        return isGrinding;
    }
    
    public bool CanSpawnPiece()
    {
        return isGrinding && piecesSpawned < totalSandPieces;
    }
    
    public void IncrementPiecesSpawned()
    {
        piecesSpawned++;
        Debug.Log($"{gameObject.name}: Spawned piece {piecesSpawned}/{totalSandPieces}, target scale: {targetScaleX * 100f}%");
    }
    
    public int GetRemainingPieces()
    {
        return totalSandPieces - piecesSpawned;
    }
    
    public int GetPiecesSpawned()
    {
        return piecesSpawned;
    }
    
    public Vector3 GetCollisionPoint()
    {
        return fixedPosition;
    }
}
