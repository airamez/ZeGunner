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
    
    [Tooltip("Minimum time tank will maintain current direction before changing")]
    [SerializeField] private float zigzagMinInterval = 2f;
    
    [Tooltip("Random additional time added to min interval before direction change")]
    [SerializeField] private float zigzagIntervalOffset = 3f;
    
    [Tooltip("Maximum angle deviation for zigzag movement")]
    [SerializeField] private float maxZigzagAngle = 30f;

    [Tooltip("Minimum distance from the base where tanks will spawn")]
    [SerializeField] private float minSpawnDistance = 40f;
    
    [Tooltip("Maximum distance from the base where tanks will spawn")]
    [SerializeField] private float maxSpawnDistance = 60f;

    [Tooltip("Base minimum speed of spawned tanks (wave 1)")]
    [SerializeField] private float baseMinSpeed = 2f;
    
    [Tooltip("Base maximum speed of spawned tanks (wave 1)")]
    [SerializeField] private float baseMaxSpeed = 5f;
   
    [Tooltip("Number of tanks for wave 1")]
    [SerializeField] private int baseTankCount = 5;
    
    [Tooltip("Percentage increase in tank count per wave (20 = 20%)")]
    [SerializeField] private float baseCountWaveIncrement = 20f;
    
    [Tooltip("Percentage increase in tank speed per wave (10 = 10%)")]
    [SerializeField] private float baseSpeedWaveIncrement = 10f;
    
    public int BaseTankCount => baseTankCount;
    public float BaseCountWaveIncrement => baseCountWaveIncrement;
    public float BaseSpeedWaveIncrement => baseSpeedWaveIncrement;
    public float DistanceToFire => distanceToFire;
    public AudioClip FiringSound => firingSound;
    
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
    
    [SerializeField] private float spawnHeight = 0.5f;
    
    private List<GameObject> activeTanks = new List<GameObject>();
    
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
        if (WaveManager.Instance == null || !WaveManager.Instance.CanSpawnTank())
        {
            return;
        }
        
        activeTanks.RemoveAll(tank => tank == null);
        
        // Spawn all tanks at once at wave start
        float speedMultiplier = WaveManager.Instance.GetTankSpeedMultiplier();
        bool spawnedAny = false;
        while (WaveManager.Instance.CanSpawnTank())
        {
            SpawnTank(speedMultiplier);
            spawnedAny = true;
        }
        
        // Update score display after spawning
        if (spawnedAny)
        {
            WaveManager.Instance.UpdateScoreDisplay();
        }
    }
    
    void SpawnTank(float speedMultiplier)
    {
        if (tankPrefab == null)
        {
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
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
        
        // Apply wave speed multiplier (only to max speed, min stays constant)
        float minSpeed = baseMinSpeed;
        float maxSpeed = baseMaxSpeed * speedMultiplier;
        float speed = Random.Range(minSpeed, maxSpeed);
        
        tankScript.Initialize(basePosition, speed, minSpeed, maxSpeed, closeStraightLineDistance, zigzagMinInterval, zigzagIntervalOffset, maxZigzagAngle, explosionPrefab, explosionSound, projectilePrefab, distanceToFire, rateOfFire, hitPoints, projectileSpeed, projectileScale, firingSound);
        
        activeTanks.Add(tank);
        
        // Register spawn with WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterTankSpawned();
        }
    }
    
    // Remove UpdateSpawnDistances - now using fixed min/max distances
}
