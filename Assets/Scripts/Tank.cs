using UnityEngine;

public class Tank : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private float minSpeed;
    private float maxSpeed;
    private bool isInitialized = false;
    
    [Header("Zigzag Movement")]
    [SerializeField] private float zigzagMinInterval = 2f; // Minimum time before direction change
    [SerializeField] private float zigzagIntervalOffset = 3f; // Random additional time
    [SerializeField] private float maxZigzagAngle = 30f; // Maximum angle deviation in degrees
    [SerializeField] private float zigzagSmoothing = 0.5f; // How smooth the direction changes are
    [SerializeField] private float straightLineDistance = 10f; // Distance from base to move straight
    
    private float nextZigzagTime;
    private Vector3 currentDirection;
    private Vector3 zigzagDirection;
    private bool lastZigzagWasLeft = false; // Track last direction for alternating
    
    // Firing system
    private GameObject projectilePrefab;
    private float distanceToFire;
    private float rateOfFire;
    private float projectileDamage;
    private float projectileSpeed;
    private float projectileScale;
    private bool isFiring = false;
    private float nextFireTime;
    private Transform barrelTransform;
    private GameObject explosionPrefab;
    private AudioClip explosionSound;
    private AudioClip firingSound;
    private bool isDestroyed = false;
    
    public void Initialize(Vector3 target, float speed, float minSpd, float maxSpd, float straightDistance, float minInterval, float intervalOffset, float maxAngle, GameObject explosion, AudioClip sound, GameObject projectile, float fireDist, float fireRate, float damage, float projSpeed, float projScale, AudioClip fireSound)
    {
        targetPosition = target;
        moveSpeed = speed;
        minSpeed = minSpd;
        maxSpeed = maxSpd;
        straightLineDistance = straightDistance;
        zigzagMinInterval = minInterval;
        zigzagIntervalOffset = intervalOffset;
        maxZigzagAngle = maxAngle;
        explosionPrefab = explosion;
        explosionSound = sound;
        firingSound = fireSound;
        
        // Firing parameters
        projectilePrefab = projectile;
        distanceToFire = fireDist;
        rateOfFire = fireRate;
        projectileDamage = damage;
        projectileSpeed = projSpeed;
        projectileScale = projScale;
        
        isInitialized = true;
        
        // Find barrel transform - try multiple possible names
        string[] barrelNames = { "Barrel", "Turret_Barrel", "Gun_Barrel", "Cannon_Barrel", "Gun", "Cannon", "Turret_Gun" };
        
        foreach (string barrelName in barrelNames)
        {
            barrelTransform = transform.Find(barrelName);
            if (barrelTransform != null)
            {
                break;
            }
        }
        
        // Also try searching in children recursively
        if (barrelTransform == null)
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>())
            {
                foreach (string barrelName in barrelNames)
                {
                    if (child.name.Contains(barrelName))
                    {
                        barrelTransform = child;
                        break;
                    }
                }
                if (barrelTransform != null) break;
            }
        }
        
        if (barrelTransform == null)
        {
            
        }
        
        // Initialize zigzag movement
        currentDirection = (targetPosition - transform.position).normalized;
        currentDirection.y = 0;
        zigzagDirection = currentDirection;
        CalculateNextZigzagTime();
        lastZigzagWasLeft = false; // Reset alternating direction
        
        if (currentDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }
    
    void CalculateNextZigzagTime()
    {
        // Calculate next direction change time: min + random offset
        float randomOffset = Random.Range(0f, zigzagIntervalOffset);
        nextZigzagTime = Time.time + zigzagMinInterval + randomOffset;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Check distance to base
        float distanceToBase = Vector3.Distance(transform.position, targetPosition);
        
        // Destroy tank if it reaches the base
        if (distanceToBase < 2f)
        {
            ReachBase();
            return;
        }
        
        // Check if tank should stop and fire at base
        if (!isFiring && distanceToBase <= distanceToFire)
        {
            isFiring = true;
            nextFireTime = Time.time; // Fire immediately when entering range
            
            // Face the base
            Vector3 toBase = (targetPosition - transform.position).normalized;
            toBase.y = 0;
            if (toBase != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(toBase);
            }
        }
        
        // Fire at base when in range (don't move)
        if (isFiring)
        {
            if (Time.time >= nextFireTime)
            {
                FireAtBase();
                nextFireTime = Time.time + rateOfFire;
            }
            return; // Don't move while firing
        }
        
        bool shouldMoveStraight = distanceToBase <= straightLineDistance;
        
        Vector3 movementDirection;
        
        if (shouldMoveStraight)
        {
            // Move straight toward base center when close
            Vector3 baseCenter = targetPosition; // targetPosition should be the base center
            movementDirection = (baseCenter - transform.position).normalized;
            movementDirection.y = 0;
            
            // Ensure tank is heading directly to base center
            if (movementDirection != Vector3.zero)
            {
                // Reset zigzag to straight direction toward base center
                zigzagDirection = movementDirection;
                currentDirection = movementDirection;
                
                // Force immediate rotation to face base center
                transform.rotation = Quaternion.LookRotation(movementDirection);
            }
        }
        else
        {
            // Use zigzag movement when far from base
            // Update zigzag direction periodically
            if (Time.time >= nextZigzagTime)
            {
                UpdateZigzagDirection();
                CalculateNextZigzagTime();
            }
            
            // Smoothly interpolate to the zigzag direction
            zigzagDirection = Vector3.Slerp(zigzagDirection, currentDirection, zigzagSmoothing * Time.deltaTime);
            movementDirection = zigzagDirection;
        }
        
        // Move in the calculated direction
        transform.position += movementDirection * moveSpeed * Time.deltaTime;
        
        // Update tank rotation to face movement direction
        if (movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection);
        }
    }
    
    void FireAtBase()
    {
        // Play firing sound
        if (firingSound != null)
        {
            AudioSource.PlayClipAtPoint(firingSound, transform.position);
        }
        
        if (projectilePrefab == null)
        {
            // No projectile, just damage base directly
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(projectileDamage);
            }
            return;
        }
        
        // Spawn projectile from in front of tank
        Vector3 spawnPos = transform.position + transform.forward * 2f + Vector3.up * 0.5f;
        Vector3 direction = (targetPosition - spawnPos).normalized;
        
        // Create projectile - EnemyProjectile will handle rotation
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        // Apply scale
        projectile.transform.localScale = Vector3.one * projectileScale;
        
        // Add EnemyProjectile component to handle damage
        EnemyProjectile enemyProj = projectile.GetComponent<EnemyProjectile>();
        if (enemyProj == null)
        {
            enemyProj = projectile.AddComponent<EnemyProjectile>();
        }
        enemyProj.Initialize(projectileDamage, projectileSpeed, direction);
        
    }
    
    void UpdateZigzagDirection()
    {
        // Calculate the base direction towards the target
        Vector3 baseDirection = (targetPosition - transform.position).normalized;
        baseDirection.y = 0;
        
        // Generate boat-like zigzag that never moves away from base
        Vector3 toBase = (targetPosition - transform.position).normalized;
        toBase.y = 0;
        
        float zigzagAngle;
        
        if (lastZigzagWasLeft)
        {
            // Last was left, so now go right
            zigzagAngle = Random.Range(10f, maxZigzagAngle); // Positive angle = right turn
            lastZigzagWasLeft = false;
        }
        else
        {
            // Last was right (or first turn), so now go left
            zigzagAngle = Random.Range(-maxZigzagAngle, -10f); // Negative angle = left turn
            lastZigzagWasLeft = true;
        }
        
        // Change speed with each direction change
        ChangeSpeed();
        
        // CRITICAL: Calculate safe zigzag angle that never moves away from base
        // Boat movement: can go sideways but never backward
        float safeAngle = zigzagAngle;
        
        // Limit angle to ensure forward progress (never more than 75 degrees from base direction)
        float maxSafeAngle = 75f; // Maximum deviation from base direction
        safeAngle = Mathf.Clamp(safeAngle, -maxSafeAngle, maxSafeAngle);
        
        // Apply the safe angle to direction toward base
        Quaternion rotation = Quaternion.Euler(0, safeAngle, 0);
        currentDirection = rotation * toBase;
        
        // FINAL VERIFICATION: Ensure dot product is always positive (moving toward base)
        float dotProduct = Vector3.Dot(currentDirection.normalized, toBase.normalized);
        if (dotProduct <= 0.2588f) // cos(75°) = 0.2588 - minimum forward progress
        {
            // Force a safer angle if still moving away
            safeAngle = lastZigzagWasLeft ? 30f : -30f; // Conservative 30-degree turn
            rotation = Quaternion.Euler(0, safeAngle, 0);
            currentDirection = rotation * toBase;
            
        }
        
        // Log final direction for debugging
        float finalDot = Vector3.Dot(currentDirection.normalized, toBase.normalized);
    }
    
    void ChangeSpeed()
    {
        // Generate random speed within min/max range
        float newSpeed = Random.Range(minSpeed, maxSpeed);
        moveSpeed = newSpeed;
        
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;
        
        // Check if hit by projectile
        GameObject hitObject = collision.gameObject;
        if (hitObject.CompareTag("Projectile") || 
            hitObject.name.Contains("Projectile") || 
            hitObject.name.Contains("Rocket") || 
            hitObject.name.Contains("Sphere") ||
            hitObject.GetComponent<CannonProjectile>() != null ||
            hitObject.GetComponent<RocketCollision>() != null)
        {
            DestroyTank(true);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;
        
        // Also check trigger collisions (some projectiles use triggers)
        GameObject hitObject = other.gameObject;
        if (hitObject.CompareTag("Projectile") || 
            hitObject.name.Contains("Projectile") || 
            hitObject.name.Contains("Rocket") || 
            hitObject.name.Contains("Sphere") ||
            hitObject.GetComponent<CannonProjectile>() != null ||
            hitObject.GetComponent<RocketCollision>() != null)
        {
            DestroyTank(true);
        }
    }
    
    public void DestroyTank(bool byPlayer)
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        // Always register with WaveManager for wave completion tracking
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterTankDestroyed();
        }
        
        // Only register with ScoreManager if destroyed by player (for stats)
        if (byPlayer && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterTankDestroyed(transform.position);
        }
        
        // Play explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5f);
        }
        
        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        Destroy(gameObject);
    }
    
    // Called when tank reaches the base - destroy without player credit
    public void ReachBase()
    {
        // Damage the base
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.DamageBase(projectileDamage * 5f); // Heavy damage for reaching base
        }
        
        DestroyTank(false);
    }
}
