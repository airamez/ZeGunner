using UnityEngine;
using System.Collections.Generic;

public class HelicopterSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject helicopterPrefab;
    [SerializeField] private Transform baseTransform;
    
    [Header("Spawn Settings")]
    [Tooltip("Distance from the base where helicopters will spawn")]
    [SerializeField] private float spawnDistance = 60f;
    
    [Tooltip("Minimum height for helicopter spawn")]
    [SerializeField] private float minSpawnHeight = 20f;
    
    [Tooltip("Maximum height for helicopter spawn")]
    [SerializeField] private float maxSpawnHeight = 40f;
    
    [Header("Helicopter Speed")]
    [Tooltip("Minimum speed of spawned helicopters")]
    [SerializeField] private float minSpeed = 8f;
    
    [Tooltip("Maximum speed of spawned helicopters")]
    [SerializeField] private float maxSpeed = 15f;
    
    [Header("Spawn Rate")]
    [Tooltip("Time between helicopter spawns in seconds")]
    [SerializeField] private float spawnInterval = 8f;
    
    [Tooltip("Maximum number of helicopters alive at once")]
    [SerializeField] private int maxHelicopters = 5;
    
    [Header("Explosion Settings")]
    [Tooltip("Folder path for helicopter explosion effects")]
    [SerializeField] private string explosionFolderPath = "Assets/JMO Assets/WarFX/_Effects/Explosions";
    
    private float nextSpawnTime = 0f;
    private List<GameObject> activeHelicopters = new List<GameObject>();
    
    void Start()
    {
        Debug.Log("HelicopterSpawner started");
        Debug.Log("Helicopter prefab assigned: " + (helicopterPrefab != null ? helicopterPrefab.name : "NULL"));
        Debug.Log("Base transform assigned: " + (baseTransform != null ? baseTransform.name : "NULL"));
    }
    
    void Update()
    {
        activeHelicopters.RemoveAll(helicopter => helicopter == null);
        
        if (Time.time >= nextSpawnTime && activeHelicopters.Count < maxHelicopters)
        {
            SpawnHelicopter();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnHelicopter()
    {
        if (helicopterPrefab == null)
        {
            Debug.LogWarning("Helicopter prefab not assigned!");
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(randomAngle) * spawnDistance,
            0f,
            Mathf.Sin(randomAngle) * spawnDistance
        );
        
        // Random height between min and max
        float randomHeight = Random.Range(minSpawnHeight, maxSpawnHeight);
        
        Vector3 spawnPosition = basePosition + spawnOffset;
        spawnPosition.y = randomHeight;
        
        GameObject helicopter = Instantiate(helicopterPrefab, spawnPosition, Quaternion.identity);
        
        // Set helicopter tag
        helicopter.tag = "Enemy";
        
        // Add Rigidbody for collision detection
        Rigidbody rb = helicopter.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = helicopter.AddComponent<Rigidbody>();
            rb.useGravity = false; // Helicopters don't fall
            rb.isKinematic = false; // Allow collision detection
        }
        
        // Add Collider if missing
        Collider helicopterCollider = helicopter.GetComponent<Collider>();
        if (helicopterCollider == null)
        {
            // Add BoxCollider as default
            BoxCollider boxCollider = helicopter.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one * 5f; // Adjust size as needed
            boxCollider.isTrigger = false; // Need physical collision
            Debug.Log("Added BoxCollider to helicopter");
        }
        else
        {
            // Ensure existing collider is not a trigger
            if (helicopterCollider.isTrigger)
            {
                helicopterCollider.isTrigger = false;
                Debug.Log("Changed helicopter collider to non-trigger");
            }
        }
        
        // Add helicopter movement script if it exists
        Helicopter helicopterScript = helicopter.GetComponent<Helicopter>();
        if (helicopterScript == null)
        {
            helicopterScript = helicopter.AddComponent<Helicopter>();
        }
        
        // Initialize helicopter with random speed
        float speed = Random.Range(minSpeed, maxSpeed);
        helicopterScript.Initialize(basePosition, speed, explosionFolderPath);
        
        activeHelicopters.Add(helicopter);
        
        Debug.Log("Spawned helicopter at height: " + randomHeight + "m with speed: " + speed);
    }
}
