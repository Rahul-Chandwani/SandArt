using UnityEngine;

public class SandCubeMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveDistance = 10f;
    
    [Header("Arrow Reference")]
    [SerializeField] private Transform arrowTransform;
    
    [Header("Direction Settings")]
    [Tooltip("Which local axis of the arrow points in the movement direction")]
    [SerializeField] private ArrowAxis arrowAxis = ArrowAxis.Up;
    
    [Tooltip("Constrain movement to specific world axes")]
    [SerializeField] private bool constrainToZAxis = false;
    
    public enum ArrowAxis
    {
        Forward,    // Z axis (blue)
        Up,         // Y axis (green)
        Right       // X axis (red)
    }
    
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float moveProgress = 0f;
    
    void Start()
    {
        // If arrow transform is not assigned, try to find it
        if (arrowTransform == null)
        {
            // Look for a child object with "arrow" in its name
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.name.ToLower().Contains("arrow"))
                {
                    arrowTransform = child;
                    break;
                }
            }
        }
        
        if (arrowTransform == null)
        {
            Debug.LogWarning($"Arrow transform not found in {gameObject.name}. Using cube's forward direction.");
        }
    }
    
    void Update()
    {
        if (isMoving)
        {
            moveProgress += Time.deltaTime * moveSpeed / moveDistance;
            
            if (moveProgress >= 1f)
            {
                transform.position = targetPosition;
                isMoving = false;
                moveProgress = 0f;
            }
            else
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
            }
        }
    }
    
    public void StartMovement()
    {
        if (isMoving) return;
        
        // Get the direction from the arrow (or cube if arrow not found)
        Vector3 moveDirection;
        
        if (arrowTransform != null)
        {
            // The arrow points in its local right direction (red axis)
            moveDirection = arrowTransform.TransformDirection(Vector3.right);
            
            Debug.Log($"Arrow local rotation: {arrowTransform.localRotation.eulerAngles}");
            Debug.Log($"Arrow world rotation: {arrowTransform.rotation.eulerAngles}");
            Debug.Log($"Arrow right in world space: {moveDirection}");
        }
        else
        {
            // Fallback to cube's forward direction
            moveDirection = transform.forward;
        }
        
        // Normalize the direction
        moveDirection.Normalize();
        
        // Constrain to Z axis if enabled
        if (constrainToZAxis)
        {
            // Keep only the Z component, determine forward or backward based on sign
            float zSign = Mathf.Sign(moveDirection.z);
            if (zSign == 0) zSign = 1; // Default to forward if exactly zero
            moveDirection = new Vector3(0, 0, zSign);
        }
        
        // Calculate target position
        startPosition = transform.position;
        targetPosition = startPosition + (moveDirection * moveDistance);
        
        isMoving = true;
        moveProgress = 0f;
        
        Debug.Log($"{gameObject.name} moving from {startPosition} to {targetPosition}");
        Debug.Log($"Movement direction: {moveDirection}");
    }
    
    public void StopMovement()
    {
        isMoving = false;
        moveProgress = 0f;
    }
    
    public void ResetPosition()
    {
        if (isMoving)
        {
            transform.position = startPosition;
            isMoving = false;
            moveProgress = 0f;
        }
    }
    
    void OnMouseDown()
    {
        StartMovement();
    }
    
    // Visualize the movement direction in the editor
    void OnDrawGizmosSelected()
    {
        if (arrowTransform != null)
        {
            // Show the arrow's right direction in world space (movement direction)
            Vector3 direction = arrowTransform.TransformDirection(Vector3.right);
            
            if (constrainToZAxis)
            {
                float zSign = Mathf.Sign(direction.z);
                if (zSign == 0) zSign = 1;
                direction = new Vector3(0, 0, zSign);
            }
            
            // Yellow line shows movement direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * moveDistance);
            Gizmos.DrawWireSphere(transform.position + direction * moveDistance, 0.5f);
            
            // Draw arrow's local axes for debugging
            Gizmos.color = Color.red;
            Gizmos.DrawRay(arrowTransform.position, arrowTransform.right * 2f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(arrowTransform.position, arrowTransform.up * 2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(arrowTransform.position, arrowTransform.forward * 2f);
        }
    }
    
    // Public properties for inspector control
    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }
    
    public float MoveDistance
    {
        get { return moveDistance; }
        set { moveDistance = value; }
    }
}
