using UnityEngine;
using System.Collections.Generic;

public class TankSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private Transform baseTransform;
    
    [Header("Tank Settings")]
    [SerializeField] private float tankScale = 1.0f;
    
    [Tooltip("Distance from base where tanks switch from zigzag to straight-line movement")]
    [SerializeField] private float closeStraightLineDistance = 10f;
    
    [Tooltip("Seconds between zigzag direction changes")]
    [SerializeField] private float zigzagDelay = 3f;

    [Header("Spawn Settings")]
    [Tooltip("Initial minimum distance for wave 1")]
    [SerializeField] private float initialMinSpawnDistance = 40f;

    [Tooltip("Minimum distance from the base where tanks will spawn (upper limit for min as waves increase)")]
    [SerializeField] private float minSpawnDistance = 40f;
    
    [Tooltip("Initial maximum distance for wave 1")]
    [SerializeField] private float initialMaxSpawnDistance = 60f;
    
    [Tooltip("Absolute maximum spawn distance (upper limit)")]
    [SerializeField] private float maxSpawnDistance = 100f;
    
    [Tooltip("Height at which tanks spawn")]
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Tank Speed")]
    [Tooltip("Base minimum speed of spawned tanks (wave 1)")]
    [SerializeField] private float baseMinSpeed = 2f;
    
    [Tooltip("Base maximum speed of spawned tanks (wave 1)")]
    [SerializeField] private float baseMaxSpeed = 5f;
    
    [Tooltip("Maximum speed limit for tanks (cannot exceed this)")]
    [SerializeField] private float maxSpeedLimit = 20f;
    
    [Tooltip("Minimum speed limit for tanks (cannot exceed this)")]
    [SerializeField] private float minSpeedLimit = 8f;
   
    [Header("Wave Settings")]
    [Tooltip("Number of tanks for wave 1")]
    [SerializeField] private int baseTankCount = 5;
    
    [Tooltip("Absolute increase in tank count per wave (2 = +2 tanks per wave)")]
    [SerializeField] private int baseCountWaveIncrement = 2;
    
    [Tooltip("Absolute increase in tank speed per wave (0.5 = +0.5 speed per wave)")]
    [SerializeField] private float baseSpeedWaveIncrement = 0.5f;
    
    [Tooltip("Absolute increase in minimum spawn distance per wave (5 = +5 units per wave)")]
    [SerializeField] private float minSpawnIncrement = 5f;
    
    [Tooltip("Absolute increase in maximum spawn distance per wave (10 = +10 units per wave)")]
    [SerializeField] private float maxSpawnIncrement = 10f;
    
    public int BaseTankCount => baseTankCount;
    public int BaseCountWaveIncrement => baseCountWaveIncrement;
    public float BaseSpeedWaveIncrement => baseSpeedWaveIncrement;
    public float DistanceToFire => distanceToFire;
    public AudioClip FiringSound => firingSound;
    public float MaxSpeedLimit => maxSpeedLimit;
    public float MinSpeedLimit => minSpeedLimit;
    public float BaseMinSpeed => baseMinSpeed;
    public float BaseMaxSpeed => baseMaxSpeed;
    public float MinSpawnIncrement => minSpawnIncrement;
    public float MaxSpawnIncrement => maxSpawnIncrement;
    
    [Header("Explosion Settings")]
    [Tooltip("Explosion prefab for tank destruction")]
    [SerializeField] private GameObject explosionPrefab;
    
    [Tooltip("Explosion sound for tank destruction")]
    [SerializeField] private AudioClip explosionSound;
    
    [Header("Enemy Firing")]
    [Tooltip("Projectile prefab for tanks to fire at base")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Distance from base where tanks stop and start firing")]
    [SerializeField] private float distanceToFire = 15f;
    
    [Tooltip("Sound played when tank fires at base")]
    [SerializeField] private AudioClip firingSound;
    
    [Tooltip("Time between shots in seconds")]
    [SerializeField] private float rateOfFire = 2f;
    
    [Tooltip("Damage dealt to base HP per projectile hit")]
    [SerializeField] private float hitPoints = 5f;
    
    [Tooltip("Speed of fired projectiles")]
    [SerializeField] private float projectileSpeed = 20f;
    
    [Tooltip("Scale of fired projectiles")]
    [SerializeField] private float projectileScale = 0.1f;
    
    [Tooltip("Height at which projectiles are fired from tanks")]
    [SerializeField] private float projectileSpawnHeight = 1.5f;
    
    private List<GameObject> activeTanks = new List<GameObject>();
    
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
        if (WaveManager.Instance == null || !WaveManager.Instance.CanSpawnTank())
        {
            return;
        }
        
        activeTanks.RemoveAll(tank => tank == null);
        
        // Spawn all tanks at once at wave start
        bool spawnedAny = false;
        while (WaveManager.Instance.CanSpawnTank())
        {
            SpawnTank();
            spawnedAny = true;
        }
        
        // Update score display after spawning
        if (spawnedAny)
        {
            WaveManager.Instance.UpdateScoreDisplay();
        }
    }
    
    void SpawnTank()
    {
        if (tankPrefab == null)
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
            spawnHeight,
            Mathf.Sin(randomAngle) * randomDistance
        );
        
        Vector3 spawnPosition = basePosition + spawnOffset;
        spawnPosition.y = spawnHeight;
        
        GameObject tank = Instantiate(tankPrefab, spawnPosition, Quaternion.identity);
        
        tank.transform.localScale = Vector3.one * tankScale;
        
        Collider tankCollider = tank.GetComponent<Collider>();
        if (tankCollider == null)
        {
            BoxCollider boxCollider = tank.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one;
        }
        
        tank.tag = "Enemy";
        
        Tank tankScript = tank.GetComponent<Tank>();
        if (tankScript == null)
        {
            tankScript = tank.AddComponent<Tank>();
        }
        
        // Apply compound speed increments with limits
        float minSpeed = WaveManager.Instance.GetCurrentTankMinSpeed();
        float maxSpeed = WaveManager.Instance.GetCurrentTankMaxSpeed();
        float speed = Random.Range(minSpeed, maxSpeed);
        
        tankScript.Initialize(basePosition, speed, minSpeed, maxSpeed, closeStraightLineDistance, zigzagDelay, explosionPrefab, explosionSound, projectilePrefab, distanceToFire, rateOfFire, hitPoints, projectileSpeed, projectileScale, firingSound, projectileSpawnHeight);
        
        activeTanks.Add(tank);
        
        // Register spawn with WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterTankSpawned();
        }
    }
    
    // Remove UpdateSpawnDistances - now using fixed min/max distances
}
