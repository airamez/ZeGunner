using UnityEngine;
using System.Collections.Generic;

public class HelicopterSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject helicopterPrefab;
    [SerializeField] private Transform baseTransform;
    
    [Header("Helicopter Settings")]
    [Tooltip("Scale factor for spawned helicopters")]
    [SerializeField] private float helicopterScale = 1.0f;
    
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
    [Tooltip("Explosion prefab for helicopter destruction")]
    [SerializeField] private GameObject explosionPrefab;
    
    [Tooltip("Explosion sound for helicopter destruction")]
    [SerializeField] private AudioClip explosionSound;
    
    [Header("Enemy Firing")]
    [Tooltip("Projectile prefab for helicopters to fire at base")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Distance from base where helicopters stop and start firing")]
    [SerializeField] private float distanceToFire = 25f;
    
    [Tooltip("Time between shots in seconds")]
    [SerializeField] private float rateOfFire = 1.5f;
    
    [Tooltip("Damage dealt to base HP per projectile hit")]
    [SerializeField] private float hitPoints = 10f;
    
    [Tooltip("Speed of fired projectiles")]
    [SerializeField] private float projectileSpeed = 30f;
    
    [Tooltip("Scale of fired projectiles")]
    [SerializeField] private float projectileScale = 0.1f;
    
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
        // Only spawn when game is playing
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return;
        }
        
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
        
        // Apply scale
        helicopter.transform.localScale = Vector3.one * helicopterScale;
        
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
        
        // Initialize helicopter with random speed and firing parameters
        float speed = Random.Range(minSpeed, maxSpeed);
        helicopterScript.Initialize(basePosition, speed, explosionPrefab, explosionSound, projectilePrefab, distanceToFire, rateOfFire, hitPoints, projectileSpeed, projectileScale);
        
        activeHelicopters.Add(helicopter);
        
        Debug.Log("Spawned helicopter at height: " + randomHeight + "m with speed: " + speed);
    }
}
