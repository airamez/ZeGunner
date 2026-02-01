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
    [Tooltip("Minimum distance from the base where helicopters will spawn")]
    [SerializeField] private float minSpawnDistance = 50f;
    
    [Tooltip("Maximum distance from the base where helicopters will spawn")]
    [SerializeField] private float maxSpawnDistance = 70f;
    
    [Tooltip("Minimum height for helicopter spawn")]
    [SerializeField] private float minSpawnHeight = 20f;
    
    [Tooltip("Maximum height for helicopter spawn")]
    [SerializeField] private float maxSpawnHeight = 40f;
    
    [Header("Helicopter Speed")]
    [Tooltip("Base minimum speed of spawned helicopters (wave 1)")]
    [SerializeField] private float baseMinSpeed = 8f;
    
    [Tooltip("Base maximum speed of spawned helicopters (wave 1)")]
    [SerializeField] private float baseMaxSpeed = 15f;
    
    [Header("Wave Settings")]
    [Tooltip("Number of helicopters for wave 1")]
    [SerializeField] private int baseHelicopterCount = 2;
    
    [Tooltip("Percentage increase in helicopter count per wave (20 = 20%)")]
    [SerializeField] private float baseCountWaveIncrement = 20f;
    
    [Tooltip("Percentage increase in helicopter speed per wave (10 = 10%)")]
    [SerializeField] private float baseSpeedWaveIncrement = 10f;
    
    public int BaseHelicopterCount => baseHelicopterCount;
    public float BaseCountWaveIncrement => baseCountWaveIncrement;
    public float BaseSpeedWaveIncrement => baseSpeedWaveIncrement;
    public float DistanceToFire => distanceToFire;
    
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
    
    private List<GameObject> activeHelicopters = new List<GameObject>();
    
    // Current wave distances (modified by wave progression)
    private float currentMinSpawnDistance;
    private float currentMaxSpawnDistance;
    
    void Start()
    {
        // Initialize current distances
        currentMinSpawnDistance = minSpawnDistance;
        currentMaxSpawnDistance = maxSpawnDistance;
    }
    
    void Update()
    {
        // Only spawn when game is playing
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return;
        }
        
        // Check if WaveManager allows spawning
        if (WaveManager.Instance == null || !WaveManager.Instance.CanSpawnHelicopter())
        {
            return;
        }
        
        activeHelicopters.RemoveAll(helicopter => helicopter == null);
        
        // Spawn all helicopters at once at wave start
        float speedMultiplier = WaveManager.Instance.GetHelicopterSpeedMultiplier();
        bool spawnedAny = false;
        while (WaveManager.Instance.CanSpawnHelicopter())
        {
            SpawnHelicopter(speedMultiplier);
            spawnedAny = true;
        }
        
        // Update score display after spawning
        if (spawnedAny)
        {
            WaveManager.Instance.UpdateScoreDisplay();
        }
    }
    
    void SpawnHelicopter(float speedMultiplier)
    {
        if (helicopterPrefab == null)
        {
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(randomAngle) * randomDistance,
            0f,
            Mathf.Sin(randomAngle) * randomDistance
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
        }
        // Ensure proper collision detection settings
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Better collision detection
        
        // Check existing collider setup
        
        // Add helicopter movement script if it exists
        Helicopter helicopterScript = helicopter.GetComponent<Helicopter>();
        if (helicopterScript == null)
        {
            helicopterScript = helicopter.AddComponent<Helicopter>();
            Debug.Log("Added Helicopter component to helicopter");
        }
        
        // Debug log all components and settings
        Debug.Log($"=== HELICOPTER SETUP ===");
        Debug.Log($"Name: {helicopter.name}");
        Debug.Log($"Tag: {helicopter.tag}");
        Debug.Log($"Layer: {helicopter.layer}");
        Debug.Log($"Active: {helicopter.activeInHierarchy}");
        
        Rigidbody rigidbody = helicopter.GetComponent<Rigidbody>();
        Debug.Log($"Rigidbody: {rigidbody != null}, UseGravity: {rigidbody?.useGravity}, IsKinematic: {rigidbody?.isKinematic}");
        
        Collider[] colliders = helicopter.GetComponents<Collider>();
        Debug.Log($"Colliders found: {colliders.Length}");
        
        // Also check children for colliders
        Collider[] childColliders = helicopter.GetComponentsInChildren<Collider>();
        Debug.Log($"Child colliders found: {childColliders.Length}");
        
        foreach (Collider col in colliders)
        {
            Debug.Log($"  - {col.GetType().Name} (on root): IsTrigger={col.isTrigger}, Enabled={col.enabled}, GameObject={col.gameObject.name}");
        }
        
        foreach (Collider col in childColliders)
        {
            Debug.Log($"  - {col.GetType().Name} (child): IsTrigger={col.isTrigger}, Enabled={col.enabled}, GameObject={col.gameObject.name}");
        }
        
        Debug.Log($"Helicopter component: {helicopterScript != null}");
        Debug.Log($"========================");
        
        // Apply wave speed multiplier (only to max speed, min stays constant)
        float minSpeed = baseMinSpeed;
        float maxSpeed = baseMaxSpeed * speedMultiplier;
        float speed = Random.Range(minSpeed, maxSpeed);
        
        helicopterScript.Initialize(basePosition, speed, explosionPrefab, explosionSound, projectilePrefab, distanceToFire, rateOfFire, hitPoints, projectileSpeed, projectileScale);
        
        activeHelicopters.Add(helicopter);
        
        // Register spawn with WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterHelicopterSpawned();
        }
    }
    
    // Remove UpdateSpawnDistances - now using fixed min/max distances
}
