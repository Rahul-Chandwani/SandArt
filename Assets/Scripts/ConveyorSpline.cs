using UnityEngine;
using System.Collections.Generic;

public class ConveyorSpline : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField] private List<Transform> splinePoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 2f; // Units per second (constant speed)
    [SerializeField] private bool closedLoop = true;
    [SerializeField] private bool useSmoothCurve = true;
    
    [Header("Sand Piece Settings")]
    [SerializeField] private GameObject sandPiecePrefab;
    [SerializeField] private float sandPieceHeightOffset = 0.5f;
    [SerializeField] private float distanceBetweenPieces = 1f; // Distance between spawned pieces in world units
    
    [Header("Particle Effects")]
    [SerializeField] private GameObject spawnParticleEffectPrefab; // Particle effect to enable when sand pieces are spawning
    
    [Header("Sand Cube Grinding")]
    [SerializeField] private float cubeHeightOffset = 1f;
    [SerializeField] private Vector3 cubePositionOffset = Vector3.zero;
    
    private List<SandPieceOnSpline> activeSandPieces = new List<SandPieceOnSpline>();
    private List<GrindingCube> grindingCubes = new List<GrindingCube>();
    private float cachedSplineLength = -1f;
    private float[] arcLengthTable;
    private int arcLengthSamples = 200;
    private float globalSplineDistance = 0f; // Global distance tracker for spawning
    private int lastSpawnFrame = -1; // Track last frame a piece was spawned
    
    void Start()
    {
        // Auto-detect spline points if not set
        if (splinePoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                splinePoints.Add(child);
            }
        }
        
        // Build arc length table for constant speed
        BuildArcLengthTable();
        cachedSplineLength = arcLengthTable[arcLengthTable.Length - 1];
    }
    
    void BuildArcLengthTable()
    {
        arcLengthTable = new float[arcLengthSamples + 1];
        arcLengthTable[0] = 0f;
        
        Vector3 prevPoint = GetPointOnSplineRaw(0f);
        float totalLength = 0f;
        
        for (int i = 1; i <= arcLengthSamples; i++)
        {
            float t = i / (float)arcLengthSamples;
            Vector3 currentPoint = GetPointOnSplineRaw(t);
            totalLength += Vector3.Distance(prevPoint, currentPoint);
            arcLengthTable[i] = totalLength;
            prevPoint = currentPoint;
        }
    }
    
    float GetTFromDistance(float distance)
    {
        // Wrap distance for closed loop
        if (closedLoop && cachedSplineLength > 0)
        {
            distance = distance % cachedSplineLength;
            if (distance < 0) distance += cachedSplineLength;
        }
        else
        {
            distance = Mathf.Clamp(distance, 0, cachedSplineLength);
        }
        
        // Binary search in arc length table
        for (int i = 0; i < arcLengthTable.Length - 1; i++)
        {
            if (distance >= arcLengthTable[i] && distance <= arcLengthTable[i + 1])
            {
                float segmentLength = arcLengthTable[i + 1] - arcLengthTable[i];
                if (segmentLength < 0.0001f) return i / (float)arcLengthSamples;
                
                float localT = (distance - arcLengthTable[i]) / segmentLength;
                float t = (i + localT) / arcLengthSamples;
                return t;
            }
        }
        
        return 1f;
    }
    
    void Update()
    {
        // Increment global distance tracker (FPS-independent)
        globalSplineDistance += moveSpeed * Time.deltaTime;
        
        // STRICT: Only allow ONE piece spawn per frame across ALL cubes
        bool hasSpawnedThisFrame = false;
        
        // Check each grinding cube and spawn if needed
        for (int i = grindingCubes.Count - 1; i >= 0; i--)
        {
            if (grindingCubes[i].cubeTransform == null)
            {
                // Cleanup: Disable and destroy particle effect
                if (grindingCubes[i].particleEffect != null)
                {
                    grindingCubes[i].particleEffect.SetActive(false);
                    Destroy(grindingCubes[i].particleEffect);
                }
                grindingCubes.RemoveAt(i);
                continue;
            }
            
            // Skip if we already spawned this frame
            if (hasSpawnedThisFrame)
            {
                continue;
            }
            
            GrindingCube cube = grindingCubes[i];
            
            // Check if this cube can and should spawn
            if (cube.grinder != null && cube.grinder.CanSpawnPiece())
            {
                // Check if we've reached the next spawn distance for this cube
                if (globalSplineDistance >= cube.nextSpawnAtDistance)
                {
                    // DOUBLE CHECK: Ensure we haven't spawned this frame
                    if (lastSpawnFrame == Time.frameCount)
                    {
                        Debug.LogWarning($"<color=red>BLOCKED: Attempted to spawn twice in frame {Time.frameCount}</color>");
                        break;
                    }
                    
                    // Spawn the piece with the cube's material and color name
                    SpawnSandPieceOnSpline(cube.splinePosition, cube.cubeTransform.name, cube.cubeMaterial, cube.colorName);
                    cube.grinder.IncrementPiecesSpawned();
                    
                    // Schedule next spawn at EXACT interval
                    cube.nextSpawnAtDistance += distanceBetweenPieces;
                    
                    // Mark that we spawned
                    hasSpawnedThisFrame = true;
                    lastSpawnFrame = Time.frameCount;
                    
                    Debug.Log($"<color=green>[Frame {Time.frameCount}] Spawned piece {cube.grinder.GetPiecesSpawned()}/{cube.grinder.totalSandPieces} from {cube.cubeTransform.name}. Next at {cube.nextSpawnAtDistance:F3}</color>");
                    
                    // BREAK immediately after spawning
                    break;
                }
            }
            else
            {
                // Cube finished spawning - disable particle effect
                if (cube.particleEffect != null && cube.particleEffect.activeSelf)
                {
                    cube.particleEffect.SetActive(false);
                    Debug.Log($"<color=grey>Disabled particle effect for {cube.cubeTransform.name} (finished spawning)</color>");
                }
            }
        }
        
        // Update sand pieces on the spline
        for (int i = activeSandPieces.Count - 1; i >= 0; i--)
        {
            if (activeSandPieces[i].pieceTransform == null)
            {
                activeSandPieces.RemoveAt(i);
                continue;
            }
            
            UpdateSandPieceOnSpline(activeSandPieces[i]);
        }
    }
    
    void SpawnFromCube(GrindingCube cube)
    {
        // This method is no longer used
    }
    
    public void AttachCube(Transform cubeTransform, Vector3 collisionPoint)
    {
        // Find the closest point on the spline
        float closestT = FindClosestPointOnSpline(collisionPoint);
        
        // Get the cube's material (from MeshRenderer, excluding SpriteRenderer)
        Material cubeMaterial = null;
        MeshRenderer[] meshRenderers = cubeTransform.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length > 0)
        {
            cubeMaterial = meshRenderers[0].sharedMaterial;
        }
        
        // Get the cube's color name
        string colorName = "";
        SandColorIdentifier cubeIdentifier = cubeTransform.GetComponent<SandColorIdentifier>();
        if (cubeIdentifier != null)
        {
            colorName = cubeIdentifier.GetColorName();
        }
        
        // Start grinding the cube at the collision point
        SandCubeGrinder grinder = cubeTransform.GetComponent<SandCubeGrinder>();
        if (grinder != null)
        {
            // Calculate collision normal (pointing away from spline)
            Vector3 splinePoint = GetPointOnSplineRaw(closestT);
            Vector3 collisionNormal = (collisionPoint - splinePoint).normalized;
            
            grinder.StartGrinding(collisionPoint, collisionNormal);
        }
        
        // Create particle effect at grinding position
        GameObject particleEffect = null;
        if (spawnParticleEffectPrefab != null)
        {
            Vector3 particlePos = GetPointOnSplineRaw(closestT) + Vector3.up * sandPieceHeightOffset;
            particleEffect = Instantiate(spawnParticleEffectPrefab, particlePos, Quaternion.identity);
            particleEffect.SetActive(true);
            
            // Set particle color to match cube
            if (cubeMaterial != null)
            {
                SetParticleEffectColor(particleEffect, cubeMaterial.color);
            }
            
            Debug.Log($"<color=magenta>Enabled particle effect for {cubeTransform.name} at grinding position</color>");
        }
        
        // Schedule first spawn immediately (at current global distance)
        GrindingCube grindingCube = new GrindingCube
        {
            cubeTransform = cubeTransform,
            grinder = grinder,
            splinePosition = closestT,
            nextSpawnAtDistance = globalSplineDistance, // Spawn first piece immediately
            fixedPosition = collisionPoint, // Store the fixed position
            cubeMaterial = cubeMaterial, // Store material for spawned pieces
            colorName = colorName, // Store color name for spawned pieces
            particleEffect = particleEffect // Store particle effect reference
        };
        
        grindingCubes.Add(grindingCube);
        
        // Disable the cube's movement script
        SandCubeMovement movement = cubeTransform.GetComponent<SandCubeMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
        
        // Disable rigidbody if present to prevent physics movement
        Rigidbody rb = cubeTransform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Only set velocities if not already kinematic
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }
        
        Debug.Log($"<color=yellow>Cube {cubeTransform.name} attached. Will spawn {grinder.totalSandPieces} pieces with {distanceBetweenPieces} units between them. Color: {colorName}</color>");
    }
    
    void UpdateGrindingCube(GrindingCube grindingCube)
    {
        // This method is no longer used - spawning is handled in Update()
    }
    

    Vector3 GetPointOnSplineRaw(float t)
    {
        return GetPointOnSpline(t);
    }
    
    void SpawnSandPieceOnSpline(float splinePosition, string cubeName, Material material, string colorName)
    {
        if (sandPiecePrefab == null) return;
        
        // Count pieces BEFORE spawning
        int countBefore = activeSandPieces.Count;
        
        Vector3 spawnPos = GetPointOnSplineRaw(splinePosition) + Vector3.up * sandPieceHeightOffset;
        GameObject sandPiece = Instantiate(sandPiecePrefab, spawnPos, Quaternion.identity);
        
        // Verify only ONE object was created
        if (sandPiece == null)
        {
            Debug.LogError("<color=red>Failed to instantiate sand piece!</color>");
            return;
        }
        
        // Apply material to all child MeshRenderers (7 child objects)
        if (material != null)
        {
            MeshRenderer[] childRenderers = sandPiece.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in childRenderers)
            {
                renderer.material = material;
            }
        }
        
        // Add and set color identifier
        SandColorIdentifier identifier = sandPiece.GetComponent<SandColorIdentifier>();
        if (identifier == null)
        {
            identifier = sandPiece.AddComponent<SandColorIdentifier>();
        }
        identifier.SetColorName(colorName);
        
        // Convert spline position to distance
        float spawnDistance = splinePosition * cachedSplineLength;
        
        SandPieceOnSpline pieceData = new SandPieceOnSpline
        {
            pieceTransform = sandPiece.transform,
            distanceTraveled = spawnDistance
        };
        
        activeSandPieces.Add(pieceData);
        
        // Count pieces AFTER spawning
        int countAfter = activeSandPieces.Count;
        int piecesAdded = countAfter - countBefore;
        
        if (piecesAdded != 1)
        {
            Debug.LogError($"<color=red>ERROR: Expected to add 1 piece, but added {piecesAdded}! Before: {countBefore}, After: {countAfter}</color>");
        }
        
        Debug.Log($"<color=cyan>[Frame {Time.frameCount}] Created sand piece #{countAfter} from {cubeName} with color '{colorName}'</color>");
    }
    
    void SetParticleEffectColor(GameObject particleObject, Color color)
    {
        // Get all particle systems (including children)
        ParticleSystem[] particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>();
        
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
        }
    }
    
    void UpdateSandPieceOnSpline(SandPieceOnSpline pieceData)
    {
        // Move along the spline at constant speed using distance
        pieceData.distanceTraveled += moveSpeed * Time.deltaTime;
        
        // Convert distance to t parameter using arc length table
        float t = GetTFromDistance(pieceData.distanceTraveled);
        
        // Get position on spline
        Vector3 splinePos = GetPointOnSplineRaw(t);
        Vector3 targetPos = splinePos + Vector3.up * sandPieceHeightOffset;
        
        // Update piece position
        pieceData.pieceTransform.position = targetPos;
        
        // Rotate piece to face forward along spline
        Vector3 forward = GetTangentOnSpline(t);
        if (forward != Vector3.zero)
        {
            pieceData.pieceTransform.rotation = Quaternion.LookRotation(forward);
        }
    }
    
    float FindClosestPointOnSpline(Vector3 worldPoint)
    {
        if (splinePoints.Count < 2) return 0f;
        
        float closestT = 0f;
        float closestDistance = float.MaxValue;
        
        // Sample the spline to find closest point
        int samples = 50;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 pointOnSpline = GetPointOnSpline(t);
            float distance = Vector3.Distance(worldPoint, pointOnSpline);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestT = t;
            }
        }
        
        return closestT;
    }
    
    public Vector3 GetPointOnSpline(float t)
    {
        if (splinePoints.Count == 0) return Vector3.zero;
        if (splinePoints.Count == 1) return splinePoints[0].position;
        
        // Ensure t is in valid range
        if (closedLoop)
        {
            // Wrap t for closed loop
            t = t - Mathf.Floor(t);
        }
        else
        {
            t = Mathf.Clamp01(t);
        }
        
        int pointCount = splinePoints.Count;
        float scaledT = t * pointCount;
        int index = Mathf.FloorToInt(scaledT);
        float localT = scaledT - index;
        
        // Handle edge case for closed loop
        if (closedLoop && index >= pointCount)
        {
            index = 0;
            localT = 0;
        }
        
        if (!closedLoop && index >= pointCount - 1)
        {
            return splinePoints[pointCount - 1].position;
        }
        
        // Get points for interpolation
        Vector3 p0, p1, p2, p3;
        
        if (useSmoothCurve)
        {
            // Catmull-Rom spline for smooth curves
            p0 = GetSplinePoint(index - 1);
            p1 = GetSplinePoint(index);
            p2 = GetSplinePoint(index + 1);
            p3 = GetSplinePoint(index + 2);
            
            return CatmullRom(p0, p1, p2, p3, localT);
        }
        else
        {
            // Linear interpolation
            p1 = GetSplinePoint(index);
            p2 = GetSplinePoint(index + 1);
            return Vector3.Lerp(p1, p2, localT);
        }
    }
    
    Vector3 GetSplinePoint(int index)
    {
        if (splinePoints.Count == 0) return Vector3.zero;
        
        if (closedLoop)
        {
            // Wrap index for closed loop
            index = ((index % splinePoints.Count) + splinePoints.Count) % splinePoints.Count;
        }
        else
        {
            // Clamp for open spline
            index = Mathf.Clamp(index, 0, splinePoints.Count - 1);
        }
        
        return splinePoints[index].position;
    }
    
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom spline interpolation for smooth curves
        float t2 = t * t;
        float t3 = t2 * t;
        
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
    
    public Vector3 GetTangentOnSpline(float t)
    {
        float delta = 0.01f;
        Vector3 p1 = GetPointOnSpline(Mathf.Max(0, t - delta));
        Vector3 p2 = GetPointOnSpline(Mathf.Min(1, t + delta));
        return (p2 - p1).normalized;
    }
    
    public float CalculateSplineLength()
    {
        if (arcLengthTable != null && arcLengthTable.Length > 0)
        {
            return arcLengthTable[arcLengthTable.Length - 1];
        }
        return 1f;
    }
    
    float GetSplineLength()
    {
        if (cachedSplineLength < 0)
        {
            cachedSplineLength = CalculateSplineLength();
        }
        return cachedSplineLength;
    }
    
    void OnDrawGizmos()
    {
        if (splinePoints.Count < 2) return;
        
        Gizmos.color = Color.cyan;
        
        // Draw spline with more detail
        int segments = 50;
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            
            Vector3 p1 = GetPointOnSpline(t1);
            Vector3 p2 = GetPointOnSpline(t2);
            
            Gizmos.DrawLine(p1, p2);
        }
        
        // Draw spline points
        Gizmos.color = Color.white;
        foreach (Transform point in splinePoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }
        
        // Draw sand piece height offset
        Gizmos.color = Color.yellow;
        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;
            
            Vector3 p1 = GetPointOnSpline(t1) + Vector3.up * sandPieceHeightOffset;
            Vector3 p2 = GetPointOnSpline(t2) + Vector3.up * sandPieceHeightOffset;
            
            Gizmos.DrawLine(p1, p2);
        }
        
        // Draw cube grinding position
        Gizmos.color = Color.red;
        for (int i = 0; i < splinePoints.Count; i++)
        {
            if (splinePoints[i] != null)
            {
                Vector3 cubePos = splinePoints[i].position + Vector3.up * cubeHeightOffset + cubePositionOffset;
                Gizmos.DrawWireCube(cubePos, Vector3.one * 0.5f);
            }
        }
    }
    
    [System.Serializable]
    private class GrindingCube
    {
        public Transform cubeTransform;
        public SandCubeGrinder grinder;
        public float splinePosition;
        public float nextSpawnAtDistance; // The global distance at which to spawn next piece
        public Vector3 fixedPosition; // The collision point where cube stays
        public Material cubeMaterial; // Material to apply to spawned pieces
        public string colorName; // Color name to copy to spawned pieces
        public GameObject particleEffect; // Active particle effect for this cube
    }
    
    [System.Serializable]
    private class SandPieceOnSpline
    {
        public Transform pieceTransform;
        public float distanceTraveled; // Distance traveled along spline in world units
    }
}
