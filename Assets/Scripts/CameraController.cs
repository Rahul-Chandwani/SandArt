using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private KeyCode moveKey = KeyCode.Z;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Optional Settings")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private bool movePosition = true;
    [SerializeField] private bool moveRotation = true;
    
    private Camera cam;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // Store original camera position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(moveKey) && !isMoving)
        {
            if (targetTransform != null)
            {
                StartCoroutine(MoveCameraToTarget());
            }
            else
            {
                Debug.LogWarning("Target Transform is not assigned!");
            }
        }
        
        // Optional: Return to original position with another key
        if (Input.GetKeyDown(KeyCode.X) && !isMoving)
        {
            StartCoroutine(MoveCameraToPosition(originalPosition, originalRotation));
        }
    }
    
    IEnumerator MoveCameraToTarget()
    {
        isMoving = true;
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        Vector3 targetPosition = targetTransform.position;
        Quaternion targetRotation = targetTransform.rotation;
        
        float elapsed = 0f;
        float duration = 1f / moveSpeed;
        
        Debug.Log($"<color=cyan>Moving camera to {targetTransform.name}</color>");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Apply curve for smooth movement
            if (useSmoothing)
            {
                t = moveCurve.Evaluate(t);
            }
            
            // Move position
            if (movePosition)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            }
            
            // Move rotation
            if (moveRotation)
            {
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            }
            
            yield return null;
        }
        
        // Ensure final position and rotation are exact
        if (movePosition)
        {
            transform.position = targetPosition;
        }
        
        if (moveRotation)
        {
            transform.rotation = targetRotation;
        }
        
        isMoving = false;
        Debug.Log("<color=green>Camera movement completed</color>");
    }
    
    IEnumerator MoveCameraToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        isMoving = true;
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        float elapsed = 0f;
        float duration = 1f / moveSpeed;
        
        Debug.Log("<color=cyan>Returning camera to original position</color>");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Apply curve for smooth movement
            if (useSmoothing)
            {
                t = moveCurve.Evaluate(t);
            }
            
            // Move position
            if (movePosition)
            {
                transform.position = Vector3.Lerp(startPosition, targetPos, t);
            }
            
            // Move rotation
            if (moveRotation)
            {
                transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);
            }
            
            yield return null;
        }
        
        // Ensure final position and rotation are exact
        if (movePosition)
        {
            transform.position = targetPos;
        }
        
        if (moveRotation)
        {
            transform.rotation = targetRot;
        }
        
        isMoving = false;
        Debug.Log("<color=green>Camera returned to original position</color>");
    }
    
    // Public methods for external control
    public void SetTarget(Transform newTarget)
    {
        targetTransform = newTarget;
    }
    
    public void MoveToTarget()
    {
        if (targetTransform != null && !isMoving)
        {
            StartCoroutine(MoveCameraToTarget());
        }
    }
    
    public void ReturnToOriginal()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveCameraToPosition(originalPosition, originalRotation));
        }
    }
    
    public void SetOriginalPosition()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        Debug.Log("Set new original camera position");
    }
    
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // Instant movement (no animation)
    public void MoveToTargetInstant()
    {
        if (targetTransform != null)
        {
            if (movePosition)
            {
                transform.position = targetTransform.position;
            }
            
            if (moveRotation)
            {
                transform.rotation = targetTransform.rotation;
            }
            
            Debug.Log($"<color=yellow>Camera instantly moved to {targetTransform.name}</color>");
        }
    }
}