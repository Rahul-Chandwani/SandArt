using System.Collections.Generic;
using UnityEngine;

public class SplineArrowMover : MonoBehaviour
{
    [Header("Spline Reference")]
    public ConveyorSpline conveyorSpline;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public int numberOfArrows = 10;
    public float heightOffset = 1f;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Movement (FPS Independent)")]
    public float moveSpeed = 2f; // Units per second - same as conveyor speed

    private List<ArrowData> arrows = new List<ArrowData>();
    private float splineLength;

    [System.Serializable]
    private class ArrowData
    {
        public Transform arrowTransform;
        public float distanceTraveled; // Distance along spline in world units
    }

    void Start()
    {
        if (conveyorSpline == null)
        {
            Debug.LogError("ConveyorSpline not assigned to SplineArrowMover!");
            return;
        }

        // Get spline length for proper spacing
        splineLength = conveyorSpline.CalculateSplineLength();
        
        SpawnArrows();
        
        Debug.Log($"<color=cyan>SplineArrowMover: Spawned {numberOfArrows} arrows on spline of length {splineLength}</color>");
    }

    void Update()
    {
        if (conveyorSpline == null || arrows.Count == 0)
            return;

        // Update each arrow's position along the spline (FPS-independent)
        for (int i = 0; i < arrows.Count; i++)
        {
            if (arrows[i].arrowTransform == null)
                continue;

            // Move arrow along spline at constant speed
            arrows[i].distanceTraveled += moveSpeed * Time.deltaTime;

            // Wrap around for closed loop
            if (arrows[i].distanceTraveled >= splineLength)
            {
                arrows[i].distanceTraveled -= splineLength;
            }

            UpdateArrowPosition(arrows[i]);
        }
    }

    void SpawnArrows()
    {
        if (arrowPrefab == null || conveyorSpline == null || numberOfArrows <= 0)
        {
            Debug.LogWarning("Cannot spawn arrows: missing references or invalid count");
            return;
        }

        // Calculate spacing between arrows
        float spacingDistance = splineLength / numberOfArrows;

        for (int i = 0; i < numberOfArrows; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, transform);
            
            ArrowData arrowData = new ArrowData
            {
                arrowTransform = arrow.transform,
                distanceTraveled = i * spacingDistance // Even spacing along spline
            };

            arrows.Add(arrowData);
            UpdateArrowPosition(arrowData);
        }

        Debug.Log($"<color=green>Spawned {numberOfArrows} arrows with {spacingDistance:F2} unit spacing</color>");
    }

    void UpdateArrowPosition(ArrowData arrowData)
    {
        // Convert distance to t parameter (0-1)
        float t = arrowData.distanceTraveled / splineLength;
        
        // Get position and tangent from conveyor spline
        Vector3 splinePosition = conveyorSpline.GetPointOnSpline(t);
        Vector3 splineTangent = conveyorSpline.GetTangentOnSpline(t);

        // Apply height offset
        Vector3 finalPosition = splinePosition + Vector3.up * heightOffset;
        arrowData.arrowTransform.position = finalPosition;

        // Rotate to face movement direction
        if (splineTangent != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(splineTangent, Vector3.up);
            Quaternion finalRotation = lookRotation * Quaternion.Euler(rotationOffset);
            arrowData.arrowTransform.rotation = finalRotation;
        }
    }

    // Public method to sync speed with conveyor
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // Clean up arrows when destroyed
    void OnDestroy()
    {
        foreach (var arrowData in arrows)
        {
            if (arrowData.arrowTransform != null)
            {
                DestroyImmediate(arrowData.arrowTransform.gameObject);
            }
        }
        arrows.Clear();
    }

    // Gizmos for debugging
    void OnDrawGizmosSelected()
    {
        if (conveyorSpline == null) return;

        // Draw arrow positions
        Gizmos.color = Color.yellow;
        foreach (var arrowData in arrows)
        {
            if (arrowData.arrowTransform != null)
            {
                Gizmos.DrawWireSphere(arrowData.arrowTransform.position, 0.3f);
            }
        }

        // Draw height offset line
        Gizmos.color = Color.cyan;
        int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            
            Vector3 p1 = conveyorSpline.GetPointOnSpline(t1) + Vector3.up * heightOffset;
            Vector3 p2 = conveyorSpline.GetPointOnSpline(t2) + Vector3.up * heightOffset;
            
            Gizmos.DrawLine(p1, p2);
        }
    }
}