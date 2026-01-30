using UnityEngine;
using System.Collections.Generic;

public class TankSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private TankSpawnerConfig config;
    
    [Header("Tank Options")]
    [SerializeField] private GameObject oldTankPrefab;
    [SerializeField] private GameObject newTankPrefab;
    [SerializeField] private bool useNewTank = false;
    [SerializeField] private float tankScale = 1.0f;
    
    [Header("Spawn Height")]
    [SerializeField] private float spawnHeight = 0.5f;
    
    private float nextSpawnTime = 0f;
    private List<GameObject> activeTanks = new List<GameObject>();
    
    void Start()
    {
        Debug.Log("TankSpawner started");
        Debug.Log("Use new tank: " + useNewTank);
        Debug.Log("Old tank prefab assigned: " + (oldTankPrefab != null ? oldTankPrefab.name : "NULL"));
        Debug.Log("New tank prefab assigned: " + (newTankPrefab != null ? newTankPrefab.name : "NULL"));
        Debug.Log("Config assigned: " + (config != null ? config.name : "NULL"));
        Debug.Log("Base transform assigned: " + (baseTransform != null ? baseTransform.name : "NULL"));
    }
    
    void Update()
    {
        activeTanks.RemoveAll(tank => tank == null);
        
        Debug.Log("Active tanks count: " + activeTanks.Count + " / Max tanks: " + (config != null ? config.maxTanks.ToString() : "CONFIG NULL"));
        Debug.Log("Time until next spawn: " + (nextSpawnTime - Time.time).ToString("F2") + "s");
        
        if (config == null)
        {
            Debug.LogWarning("Config is null, cannot spawn tanks");
            return;
        }
        
        if (Time.time >= nextSpawnTime && activeTanks.Count < config.maxTanks)
        {
            Debug.Log("Attempting to spawn tank...");
            SpawnTank();
            nextSpawnTime = Time.time + config.spawnInterval;
        }
    }
    
    void SpawnTank()
    {
        Debug.Log("=== SPAWNING TANK ===");
        
        GameObject selectedTank = GetSelectedTank();
        
        if (selectedTank == null || config == null)
        {
            Debug.LogWarning("Tank prefab or config not assigned!");
            Debug.LogWarning("Selected tank null: " + (selectedTank == null));
            Debug.LogWarning("Config null: " + (config == null));
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        Debug.Log("Base position: " + basePosition);
        
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Debug.Log("Random angle: " + randomAngle * Mathf.Rad2Deg + " degrees");
        
        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(randomAngle) * config.spawnDistance,
            spawnHeight,
            Mathf.Sin(randomAngle) * config.spawnDistance
        );
        
        Vector3 spawnPosition = basePosition + spawnOffset;
        spawnPosition.y = spawnHeight;
        Debug.Log("Spawn position: " + spawnPosition);
        
        GameObject tank = Instantiate(selectedTank, spawnPosition, Quaternion.identity);
        Debug.Log("Tank instantiated: " + tank.name);
        
        // Set tank scale
        tank.transform.localScale = Vector3.one * tankScale;
        Debug.Log("Tank scale set to: " + tank.transform.localScale);
        
        // Add collider if tank doesn't have one
        Collider tankCollider = tank.GetComponent<Collider>();
        if (tankCollider == null)
        {
            Debug.Log("Adding box collider to tank");
            BoxCollider boxCollider = tank.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one; // Default size, will scale with tank scale
        }
        else
        {
            Debug.Log("Tank already has collider: " + tankCollider.GetType().Name);
        }
        
        tank.tag = "Enemy";
        Debug.Log("Set tank tag to Enemy. Current tag: " + tank.tag);
        
        // Verify the tag was set correctly
        if (tank.CompareTag("Enemy"))
        {
            Debug.Log("Tank tag verification: SUCCESS - Enemy tag confirmed");
        }
        else
        {
            Debug.LogError("Tank tag verification: FAILED - Enemy tag not found!");
        }
        
        Tank tankScript = tank.GetComponent<Tank>();
        if (tankScript == null)
        {
            Debug.Log("Adding Tank script to prefab");
            tankScript = tank.AddComponent<Tank>();
        }
        else
        {
            Debug.Log("Found existing Tank script");
        }
        
        float speed = Random.Range(config.minSpeed, config.maxSpeed);
        Debug.Log("Setting tank speed: " + speed);
        tankScript.Initialize(basePosition, speed);
        
        activeTanks.Add(tank);
        Debug.Log("Tank added to active tanks list. Total: " + activeTanks.Count);
        Debug.Log("=== TANK SPAWNED ===");
    }
    
    GameObject GetSelectedTank()
    {
        // Try the selected tank first
        if (useNewTank && newTankPrefab != null)
        {
            return newTankPrefab;
        }
        else if (!useNewTank && oldTankPrefab != null)
        {
            return oldTankPrefab;
        }
        
        // Fallback to the manually assigned tank prefab
        if (tankPrefab != null)
        {
            return tankPrefab;
        }
        
        // Final fallback - try to find old tank prefab
        if (oldTankPrefab != null)
        {
            Debug.LogWarning("Using old tank prefab as fallback");
            return oldTankPrefab;
        }
        
        return null;
    }
}
