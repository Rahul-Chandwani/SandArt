using UnityEngine;
using System.Collections.Generic;

public class SandCubeSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableCube
    {
        public string prefabName;
        public string colorName;
        public int sandPiecesCount = 10;
        public Vector3 spawnPoint;
        public Vector3 spawnRotation;
        public Vector3 finalPoint;
        public Vector3 finalRotation;
        public float moveSpeed = 5f;
        [HideInInspector] public GameObject spawnedCube;
        [HideInInspector] public bool hasReachedFinal = false;
    }
    
    [Header("References")]
    public SandCubeManager cubeManager;
    
    [Header("Spawn Settings")]
    public List<SpawnableCube> cubesToSpawn = new List<SpawnableCube>();
    
    [Header("Spawn Control")]
    public bool spawnOnStart = false;
    public float spawnDelay = 0.5f; // Delay between spawning each cube
    
    private int currentSpawnIndex = 0;
    private float spawnTimer = 0f;
    private bool isSpawning = false;
    
    void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }
    
    void Update()
    {
        if (isSpawning)
        {
            spawnTimer += Time.deltaTime;
            
            if (spawnTimer >= spawnDelay && currentSpawnIndex < cubesToSpawn.Count)
            {
                SpawnCube(currentSpawnIndex);
                currentSpawnIndex++;
                spawnTimer = 0f;
                
                if (currentSpawnIndex >= cubesToSpawn.Count)
                {
                    isSpawning = false;
                }
            }
        }
        
        // Move spawned cubes to their final positions
        foreach (var cubeData in cubesToSpawn)
        {
            if (cubeData.spawnedCube != null && !cubeData.hasReachedFinal)
            {
                MoveCubeToFinal(cubeData);
            }
        }
    }
    
    public void StartSpawning()
    {
        currentSpawnIndex = 0;
        spawnTimer = 0f;
        isSpawning = true;
        
        Debug.Log($"Started spawning {cubesToSpawn.Count} cubes");
    }
    
    void SpawnCube(int index)
    {
        if (cubeManager == null)
        {
            Debug.LogError("SandCubeManager not assigned!");
            return;
        }
        
        SpawnableCube cubeData = cubesToSpawn[index];
        
        // Find the prefab
        SandCubeManager.SandCubePrefab prefabData = cubeManager.sandCubePrefabs.Find(p => p.prefabName == cubeData.prefabName);
        if (prefabData == null || prefabData.prefab == null)
        {
            Debug.LogWarning($"Prefab '{cubeData.prefabName}' not found!");
            return;
        }
        
        // Instantiate at spawn point
        GameObject cube = Instantiate(prefabData.prefab, transform);
        cube.name = $"{cubeData.prefabName}_{cubeData.colorName}_{index}";
        cube.transform.position = cubeData.spawnPoint;
        cube.transform.rotation = Quaternion.identity;
        
        // Add movement component (disabled initially, will be enabled after reaching final point)
        SandCubeMovement movement = cube.GetComponent<SandCubeMovement>();
        if (movement == null)
        {
            movement = cube.AddComponent<SandCubeMovement>();
        }
        movement.enabled = false; // Disable until cube reaches final position
        
        // Add grinder component
        SandCubeGrinder grinder = cube.GetComponent<SandCubeGrinder>();
        if (grinder == null)
        {
            grinder = cube.AddComponent<SandCubeGrinder>();
        }
        grinder.totalSandPieces = cubeData.sandPiecesCount;
        
        // Add color identifier
        SandColorIdentifier colorIdentifier = cube.GetComponent<SandColorIdentifier>();
        if (colorIdentifier == null)
        {
            colorIdentifier = cube.AddComponent<SandColorIdentifier>();
        }
        colorIdentifier.colorLibrary = cubeManager.colorLibrary;
        colorIdentifier.SetColorName(cubeData.colorName);
        
        // Ensure collider exists
        Collider collider = cube.GetComponent<Collider>();
        if (collider == null)
        {
            cube.AddComponent<BoxCollider>();
        }
        
        // Add tag
        if (cube.tag == "Untagged")
        {
            cube.tag = "SandCube";
        }
        
        // Apply material from color library
        Material material = cubeManager.colorLibrary.GetMaterialByName(cubeData.colorName);
        if (material != null)
        {
            Renderer[] renderers = cube.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer is SpriteRenderer)
                    continue;
                    
                renderer.material = material;
            }
        }
        
        cubeData.spawnedCube = cube;
        cubeData.hasReachedFinal = false;
        
        Debug.Log($"Spawned cube {index}: {cubeData.prefabName} ({cubeData.colorName}) at {cubeData.spawnPoint}");
    }
    
    void MoveCubeToFinal(SpawnableCube cubeData)
    {
        Vector3 currentPos = cubeData.spawnedCube.transform.position;
        Vector3 targetPos = cubeData.finalPoint;
        
        float distance = Vector3.Distance(currentPos, targetPos);
        
        if (distance < 0.1f)
        {
            // Reached final position
            cubeData.spawnedCube.transform.position = targetPos;
            cubeData.hasReachedFinal = true;
            
            // Enable click-to-move functionality
            SandCubeMovement movement = cubeData.spawnedCube.GetComponent<SandCubeMovement>();
            if (movement != null)
            {
                movement.enabled = true;
            }
            
            Debug.Log($"Cube {cubeData.spawnedCube.name} reached final position");
        }
        else
        {
            // Move towards final position
            Vector3 direction = (targetPos - currentPos).normalized;
            cubeData.spawnedCube.transform.position += direction * cubeData.moveSpeed * Time.deltaTime;
        }
    }
    
    public void ClearAllSpawnedCubes()
    {
        foreach (var cubeData in cubesToSpawn)
        {
            if (cubeData.spawnedCube != null)
            {
                Destroy(cubeData.spawnedCube);
            }
        }
        
        Debug.Log("Cleared all spawned cubes");
    }
    
    public void ResetSpawner()
    {
        ClearAllSpawnedCubes();
        currentSpawnIndex = 0;
        spawnTimer = 0f;
        isSpawning = false;
    }
    
    void OnDrawGizmos()
    {
        if (cubesToSpawn == null) return;
        
        foreach (var cubeData in cubesToSpawn)
        {
            // Draw spawn point
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cubeData.spawnPoint, 0.3f);
            
            // Draw final point
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cubeData.finalPoint, 0.3f);
            
            // Draw line between spawn and final
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(cubeData.spawnPoint, cubeData.finalPoint);
        }
    }
}
