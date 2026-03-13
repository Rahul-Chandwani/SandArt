using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class SpriteSequenceManager : MonoBehaviour
{
    [Header("Sprite Sequence Settings")]
    [SerializeField] private List<SpriteRegionEditorTool> sprites = new List<SpriteRegionEditorTool>();
    [SerializeField] private Transform rotatingObject; // Object that rotates 90 degrees per sprite completion
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationDuration = 1f; // Duration of rotation animation
    [SerializeField] private Ease rotationEase = Ease.OutQuad; // Easing for rotation animation
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Axis to rotate around (Y-axis by default)
    [SerializeField] private float rotationAngle = 90f; // Degrees to rotate per sprite completion
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private int currentSpriteIndex = 0;
    private bool isRotating = false;
    private float currentRotation = 0f;
    
    void Start()
    {
        InitializeSequence();
    }
    
    void Update()
    {
        CheckCurrentSpriteCompletion();
    }
    
    void InitializeSequence()
    {
        // Disable all sprites except the first one
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i] != null)
            {
                sprites[i].gameObject.SetActive(i == 0);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"<color=cyan>Sprite {i} ({sprites[i].name}) set to {(i == 0 ? "ACTIVE" : "INACTIVE")}</color>");
                }
            }
        }
        
        // Set initial rotation
        if (rotatingObject != null)
        {
            currentRotation = 0f;
            rotatingObject.rotation = Quaternion.identity;
        }
        
        currentSpriteIndex = 0;
        
        if (enableDebugLogs)
        {
            Debug.Log($"<color=green>Sprite sequence initialized. Starting with sprite 0. Total sprites: {sprites.Count}</color>");
        }
    }
    
    void CheckCurrentSpriteCompletion()
    {
        // Don't check if we're currently rotating or if sequence is complete
        if (isRotating || currentSpriteIndex >= sprites.Count) return;
        
        SpriteRegionEditorTool currentSprite = sprites[currentSpriteIndex];
        if (currentSprite == null) return;
        
        // Check if current sprite is completely filled by sand pieces
        if (IsSpriteCompletelyFilled(currentSprite))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=yellow>Sprite {currentSpriteIndex} ({currentSprite.name}) is completely filled by sand pieces!</color>");
            }
            
            AdvanceToNextSprite();
        }
        else if (enableDebugLogs)
        {
            // Debug info about current fill progress
            int totalRegions = currentSprite.GetTotalRegionCount();
            int filledRegions = currentSprite.GetFilledRegionCount();
            float fillProgress = currentSprite.GetFillProgress();
            
            // Only log every few seconds to avoid spam
            if (Time.time % 2f < Time.deltaTime)
            {
                Debug.Log($"<color=cyan>Sprite {currentSpriteIndex} progress: {filledRegions}/{totalRegions} regions filled ({fillProgress * 100f:F1}%)</color>");
            }
        }
    }
    
    bool IsSpriteCompletelyFilled(SpriteRegionEditorTool sprite)
    {
        if (sprite == null) return false;
        
        // Get the total number of regions and filled regions
        int totalRegions = sprite.GetTotalRegionCount();
        int filledRegions = sprite.GetFilledRegionCount();
        
        // Consider sprite complete when ALL regions are filled by sand pieces
        bool isComplete = totalRegions > 0 && filledRegions >= totalRegions;
        
        if (enableDebugLogs && isComplete)
        {
            Debug.Log($"<color=cyan>Sprite {sprite.name} is completely filled! All {totalRegions} regions have been filled by sand pieces.</color>");
        }
        
        return isComplete;
    }
    
    void AdvanceToNextSprite()
    {
        // Move to next sprite index first
        int previousSpriteIndex = currentSpriteIndex;
        currentSpriteIndex++;
        
        // Enable next sprite (if exists) before rotation
        if (currentSpriteIndex < sprites.Count)
        {
            if (sprites[currentSpriteIndex] != null)
            {
                sprites[currentSpriteIndex].gameObject.SetActive(true);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"<color=green>Enabled sprite {currentSpriteIndex} ({sprites[currentSpriteIndex].name})</color>");
                }
            }
        }
        
        // Rotate the object and disable previous sprite after rotation completes
        RotateObjectAndDisablePrevious(previousSpriteIndex);
        
        // Check if sequence is complete
        if (currentSpriteIndex >= sprites.Count)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"<color=magenta>All sprites completed! Sequence finished.</color>");
            }
            
            OnSequenceComplete();
        }
    }
    
    void RotateObjectAndDisablePrevious(int previousSpriteIndex)
    {
        if (rotatingObject == null)
        {
            // If no rotating object, just disable previous sprite immediately
            DisablePreviousSprite(previousSpriteIndex);
            return;
        }
        
        isRotating = true;
        currentRotation += rotationAngle;
        
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.AngleAxis(currentRotation, rotationAxis);
        
        if (enableDebugLogs)
        {
            Debug.Log($"<color=blue>Rotating object to {currentRotation} degrees around {rotationAxis}</color>");
        }
        
        // Animate rotation using DOTween
        rotatingObject.DORotateQuaternion(targetRotation, rotationDuration)
            .SetEase(rotationEase)
            .OnComplete(() => {
                isRotating = false;
                
                // Disable previous sprite AFTER rotation completes
                DisablePreviousSprite(previousSpriteIndex);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"<color=blue>Rotation completed. Current rotation: {currentRotation} degrees</color>");
                }
            });
    }
    
    void DisablePreviousSprite(int previousSpriteIndex)
    {
        if (previousSpriteIndex >= 0 && previousSpriteIndex < sprites.Count && sprites[previousSpriteIndex] != null)
        {
            sprites[previousSpriteIndex].gameObject.SetActive(false);
            
            if (enableDebugLogs)
            {
                Debug.Log($"<color=orange>Disabled previous sprite {previousSpriteIndex} ({sprites[previousSpriteIndex].name}) after rotation completed</color>");
            }
        }
    }
    
    void OnSequenceComplete()
    {
        // Override this method or add UnityEvents for custom behavior when sequence completes
        if (enableDebugLogs)
        {
            Debug.Log("<color=magenta>Sprite sequence completed! All sprites have been filled.</color>");
        }
    }
    
    // Public methods for external control
    public void ResetSequence()
    {
        // Stop any ongoing rotation
        if (rotatingObject != null)
        {
            rotatingObject.DOKill();
        }
        
        isRotating = false;
        
        // Reset rotation to initial state
        if (rotatingObject != null)
        {
            currentRotation = 0f;
            rotatingObject.rotation = Quaternion.identity;
        }
        
        // Reset sprite index
        currentSpriteIndex = 0;
        
        // Disable all sprites except the first one
        for (int i = 0; i < sprites.Count; i++)
        {
            if (sprites[i] != null)
            {
                sprites[i].gameObject.SetActive(i == 0);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"<color=cyan>Sprite {i} ({sprites[i].name}) set to {(i == 0 ? "ACTIVE" : "INACTIVE")}</color>");
                }
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("<color=cyan>Sprite sequence reset to beginning. First sprite is now active.</color>");
        }
    }
    
    public void ForceAdvanceToNextSprite()
    {
        if (currentSpriteIndex < sprites.Count - 1)
        {
            AdvanceToNextSprite();
            
            if (enableDebugLogs)
            {
                Debug.Log("<color=yellow>Manually advanced to next sprite</color>");
            }
        }
    }
    
    public int GetCurrentSpriteIndex()
    {
        return currentSpriteIndex;
    }
    
    public int GetTotalSpriteCount()
    {
        return sprites.Count;
    }
    
    public bool IsSequenceComplete()
    {
        return currentSpriteIndex >= sprites.Count;
    }
    
    public SpriteRegionEditorTool GetCurrentSprite()
    {
        if (currentSpriteIndex >= 0 && currentSpriteIndex < sprites.Count)
        {
            return sprites[currentSpriteIndex];
        }
        return null;
    }
    
    public float GetSequenceProgress()
    {
        if (sprites.Count == 0) return 1f;
        return (float)currentSpriteIndex / sprites.Count;
    }
    
    // Gizmos for visualization
    void OnDrawGizmosSelected()
    {
        if (rotatingObject != null)
        {
            // Draw rotation axis
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rotatingObject.position, rotationAxis * 2f);
            
            // Draw rotation arc
            Gizmos.color = Color.yellow;
            Vector3 perpendicular = Vector3.Cross(rotationAxis, Vector3.forward);
            if (perpendicular == Vector3.zero)
                perpendicular = Vector3.Cross(rotationAxis, Vector3.up);
            
            for (int i = 0; i <= 4; i++)
            {
                float angle = i * rotationAngle;
                Vector3 direction = Quaternion.AngleAxis(angle, rotationAxis) * perpendicular;
                Gizmos.DrawRay(rotatingObject.position, direction * 1.5f);
            }
        }
        
        // Draw sprite sequence indicators
        if (sprites.Count > 0)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] != null)
                {
                    Gizmos.color = i == currentSpriteIndex ? Color.green : Color.gray;
                    Gizmos.DrawWireCube(sprites[i].transform.position, Vector3.one * 0.5f);
                }
            }
        }
    }
}