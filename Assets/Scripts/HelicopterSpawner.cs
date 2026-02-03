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
    [Tooltip("Initial minimum distance for wave 1")]
    [SerializeField] private float initialMinSpawnDistance = 50f;

    [Tooltip("Minimum distance from the base where helicopters will spawn (upper limit for min as waves increase)")]
    [SerializeField] private float minSpawnDistance = 50f;
    
    [Tooltip("Initial maximum distance for wave 1")]
    [SerializeField] private float initialMaxSpawnDistance = 70f;
    
    [Tooltip("Absolute maximum spawn distance (upper limit)")]
    [SerializeField] private float maxSpawnDistance = 150f;
    
    [Tooltip("Minimum height for helicopter spawn")]
    [SerializeField] private float minSpawnHeight = 20f;
    
    [Tooltip("Maximum height for helicopter spawn")]
    [SerializeField] private float maxSpawnHeight = 40f;
    
    [Tooltip("Absolute increase in minimum spawn distance per wave (5 = +5 units per wave)")]
    [SerializeField] private float minSpawnIncrement = 5f;
    
    [Tooltip("Absolute increase in maximum spawn distance per wave (15 = +15 units per wave)")]
    [SerializeField] private float maxSpawnIncrement = 15f;
    
    [Header("Helicopter Speed")]
    [Tooltip("Base minimum speed of spawned helicopters (wave 1)")]
    [SerializeField] private float baseMinSpeed = 8f;
    
    [Tooltip("Base maximum speed of spawned helicopters (wave 1)")]
    [SerializeField] private float baseMaxSpeed = 15f;
    
    [Header("Wave Settings")]
    [Tooltip("Number of helicopters for wave 1")]
    [SerializeField] private int baseHelicopterCount = 2;
    
    [Tooltip("Absolute increase in helicopter count per wave (1 = +1 helicopter per wave)")]
    [SerializeField] private int baseCountWaveIncrement = 1;
    
    [Tooltip("Absolute increase in helicopter speed per wave (1.0 = +1.0 speed per wave)")]
    [SerializeField] private float baseSpeedWaveIncrement = 1.0f;
    
    [Header("Helicopter Zigzag Movement")]
    [Tooltip("Base delay between zigzag direction changes (seconds)")]
    [SerializeField] private float zigzagDelay = 4f;
    
    [Tooltip("Minimum lateral (sideways) speed for zigzag movement")]
    [SerializeField] private float minLateralSpeed = 2f;
    
    [Tooltip("Maximum lateral (sideways) speed for zigzag movement")]
    [SerializeField] private float maxLateralSpeed = 5f;
    
    [Tooltip("Distance from base where helicopter starts zigzag movement")]
    [SerializeField] private float distanceToStartZigzag = 100f;
    
    public int BaseHelicopterCount => baseHelicopterCount;
    public int BaseCountWaveIncrement => baseCountWaveIncrement;
    public float BaseSpeedWaveIncrement => baseSpeedWaveIncrement;
    public float DistanceToFire => distanceToFire;
    public AudioClip FiringSound => firingSound;
    public float ZigzagDelay => zigzagDelay;
    public float MinLateralSpeed => minLateralSpeed;
    public float MaxLateralSpeed => maxLateralSpeed;
    public float DistanceToStartZigzag => distanceToStartZigzag;
    public float MinSpawnIncrement => minSpawnIncrement;
    public float MaxSpawnIncrement => maxSpawnIncrement;
    public float BaseMinSpeed => baseMinSpeed;
    public float BaseMaxSpeed => baseMaxSpeed;
    
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
    
    [Tooltip("Sound played when helicopter fires at base")]
    [SerializeField] private AudioClip firingSound;
    
    [Tooltip("Time between shots in seconds")]
    [SerializeField] private float rateOfFire = 1.5f;
    
    [Tooltip("Damage dealt to base HP per projectile hit")]
    [SerializeField] private float hitPoints = 10f;
    
    [Tooltip("Speed of fired projectiles")]
    [SerializeField] private float projectileSpeed = 30f;
    
    [Tooltip("Scale of fired projectiles")]
    [SerializeField] private float projectileScale = 0.1f;
    
    private List<GameObject> activeHelicopters = new List<GameObject>();
    
    void Start()
    {
        // No initialization needed - distances calculated per wave
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
        bool spawnedAny = false;
        while (WaveManager.Instance.CanSpawnHelicopter())
        {
            SpawnHelicopter();
            spawnedAny = true;
        }
        
        // Update score display after spawning
        if (spawnedAny)
        {
            WaveManager.Instance.UpdateScoreDisplay();
        }
    }
    
    void SpawnHelicopter()
    {
        if (helicopterPrefab == null)
        {
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Calculate current spawn distances using absolute increments
        int currentWave = WaveManager.Instance != null ? WaveManager.Instance.GetCurrentWave() : 1;
        
        // Calculate max spawn distance: initial + (wave-1) * increment
        float currentMaxSpawnDistance = initialMaxSpawnDistance + ((currentWave - 1) * maxSpawnIncrement);
        currentMaxSpawnDistance = Mathf.Min(currentMaxSpawnDistance, maxSpawnDistance);
        
        // Calculate min spawn distance: initial + (wave-1) * increment
        float currentMinSpawnDistance = initialMinSpawnDistance + ((currentWave - 1) * minSpawnIncrement);
        currentMinSpawnDistance = Mathf.Min(currentMinSpawnDistance, minSpawnDistance);

        // Ensure min is strictly less than max to avoid invalid Range
        if (currentMinSpawnDistance >= currentMaxSpawnDistance)
        {
            currentMinSpawnDistance = Mathf.Max(0f, currentMaxSpawnDistance - 1f);
        }

        // Spawn at random distance between current min and current max
        float randomDistance = Random.Range(currentMinSpawnDistance, currentMaxSpawnDistance);
        
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
        }
        
        Rigidbody rigidbody = helicopter.GetComponent<Rigidbody>();
        
        Collider[] colliders = helicopter.GetComponents<Collider>();
        
        // Also check children for colliders
        Collider[] childColliders = helicopter.GetComponentsInChildren<Collider>();
        // Apply absolute speed increments
        float minSpeed = WaveManager.Instance.GetCurrentHelicopterMinSpeed();
        float maxSpeed = WaveManager.Instance.GetCurrentHelicopterMaxSpeed();
        float speed = Random.Range(minSpeed, maxSpeed);
        
        helicopterScript.Initialize(basePosition, speed, explosionPrefab, explosionSound, projectilePrefab, distanceToFire, rateOfFire, hitPoints, projectileSpeed, projectileScale, firingSound, zigzagDelay, minLateralSpeed, maxLateralSpeed, distanceToStartZigzag);
        
        activeHelicopters.Add(helicopter);
        
        // Register spawn with WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterHelicopterSpawned();
        }
    }
    
    // Remove UpdateSpawnDistances - now using fixed min/max distances
}
