using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConveyorCollider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConveyorSpline conveyorSpline;
    
    [Header("Settings")]
    [SerializeField] private string sandCubeTag = "SandCube";
    [SerializeField] private bool debugMode = false;
    
    void Start()
    {
        // Auto-find conveyor spline in parent if not assigned
        if (conveyorSpline == null)
        {
            conveyorSpline = GetComponentInParent<ConveyorSpline>();
        }
        
        if (conveyorSpline == null)
        {
            Debug.LogWarning($"ConveyorSpline not found for {gameObject.name}");
        }
        
        // Ensure this has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if it's a sand cube by tag or component
        bool isSandCube = other.CompareTag(sandCubeTag) || other.GetComponent<SandCubeMovement>() != null;
        
        if (isSandCube)
        {
            if (conveyorSpline != null)
            {
                if (debugMode) Debug.Log($"Sand cube {other.name} collided with conveyor part {gameObject.name}");
                
                // Get collision point - use the closest point on this collider
                Vector3 collisionPoint = GetComponent<Collider>().ClosestPoint(other.transform.position);
                
                // Attach to spline
                conveyorSpline.AttachCube(other.transform, collisionPoint);
            }
            else
            {
                Debug.LogError($"ConveyorSpline is null for {gameObject.name}!");
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Also handle non-trigger collisions as backup
        bool isSandCube = collision.gameObject.CompareTag(sandCubeTag) || 
                         collision.gameObject.GetComponent<SandCubeMovement>() != null;
        
        if (isSandCube)
        {
            if (conveyorSpline != null)
            {
                if (debugMode) Debug.Log($"Sand cube {collision.gameObject.name} collided (physics) with conveyor part {gameObject.name}");
                
                // Get collision point from contact
                Vector3 collisionPoint = collision.contacts.Length > 0 ? 
                    collision.contacts[0].point : 
                    collision.transform.position;
                
                // Attach to spline
                conveyorSpline.AttachCube(collision.transform, collisionPoint);
            }
        }
    }
}
