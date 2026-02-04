using UnityEngine;

public class Tank : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private float minSpeed;
    private float maxSpeed;
    private bool isInitialized = false;
    
    [Header("Zigzag Movement")]
    [SerializeField] private float zigzagMinDelay = 1f; // Minimum seconds before changing zigzag direction
    [SerializeField] private float zigzagMaxDelay = 15f; // Maximum seconds before changing zigzag direction
    [SerializeField] private float zigzagSmoothing = 0.3f; // Smooth turning transitions (higher = slower turn)
    [SerializeField] private float straightLineDistance = 10f; // Distance from base to move straight
    [SerializeField] private float minLateralAngle = 3f; // Minimum lateral angle
    [SerializeField] private float maxLateralAngle = 25f; // Maximum lateral angle
    
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
    
    // Barrel spawn point - auto-detected by name
    private Transform barrelTransform;
    
    private GameObject explosionPrefab;
    private AudioClip explosionSound;
    private AudioClip firingSound;
    private bool isDestroyed = false;
    
    public void Initialize(Vector3 target, float speed, float minSpd, float maxSpd, float straightDistance, float delay, GameObject explosion, AudioClip sound, GameObject projectile, float fireDist, float fireRate, float damage, float projSpeed, float projScale, AudioClip fireSound)
    {
        targetPosition = target;
        moveSpeed = speed;
        minSpeed = minSpd;
        maxSpeed = maxSpd;
        straightLineDistance = straightDistance;
        zigzagMaxDelay = delay;
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
            // Try common barrel names
            string[] commonNames = { "BarrelSpawn", "Barrel", "Cannon", "Gun", "TurretBarrel", "FirePoint", "Muzzle" };
            foreach (string name in commonNames)
            {
                Transform found = transform.Find(name);
                if (found != null)
                {
                    barrelTransform = found;
                    Debug.Log($"[Tank] Auto-found barrel transform: {name}");
                    break;
                }
            }
            
            if (barrelTransform == null)
            {
                // Search all children recursively
                barrelTransform = FindDeepChild(transform, "barrel");
                if (barrelTransform != null)
                {
                    Debug.Log($"[Tank] Auto-found barrel transform by search: {barrelTransform.name}");
                }
            }
        }
        
        // Log the result
        if (barrelTransform != null)
        {
            Debug.Log($"[Tank] Using barrel transform: {barrelTransform.name} at position {barrelTransform.position}");
        }
        else
        {
            Debug.LogWarning("[Tank] No barrel transform assigned or found. Projectiles will spawn from tank center.");
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
        // Random delay between zigzagMinDelay and zigzagMaxDelay
        // This ensures tanks maintain their zigzag direction for varying durations
        float randomDelay = Random.Range(zigzagMinDelay, zigzagMaxDelay);
        nextZigzagTime = Time.time + randomDelay;
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
            
            // Fire immediately when reaching line of fire
            FireAtBase();
            
            // Set next fire time for subsequent shots
            nextFireTime = Time.time + rateOfFire;
            
            // Flash screen if tank is not in player's field of view
            if (!IsInPlayerFieldOfView())
            {
                ScreenFlash.Instance?.FlashScreen();
            }
            
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
            Vector3 baseCenter = targetPosition;
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
            
            // Minimal smoothing for sharp direction changes (creating sharp edges)
            // Very low smoothing value means almost instant direction changes
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
    
    bool IsInPlayerFieldOfView()
    {
        // Find the player's camera (assuming main camera)
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return false;
        
        // Check if tank is within camera's view frustum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        Bounds tankBounds = GetComponent<Collider>().bounds;
        
        return GeometryUtility.TestPlanesAABB(planes, tankBounds);
    }
    
    void FireAtBase()
    {
        // Play firing sound with volume control
        if (firingSound != null)
        {
            // Create temporary AudioSource for volume control
            GameObject tempAudio = new GameObject("TempFiringSound");
            tempAudio.transform.position = transform.position;
            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            
            // Apply global volume from VolumeManager
            if (VolumeManager.Instance != null)
            {
                audioSource.volume = VolumeManager.Instance.GetMasterVolume();
            }
            
            audioSource.PlayOneShot(firingSound);
            Destroy(tempAudio, firingSound.length + 0.1f); // Clean up after sound plays
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
        
        // Spawn projectile from barrel position if available, otherwise use front of tank
        Vector3 spawnPos;
        if (barrelTransform != null)
        {
            spawnPos = barrelTransform.position;
        }
        else
        {
            // Fallback: spawn from in front of tank
            spawnPos = transform.position + transform.forward * 2f + Vector3.up * 1.5f;
        }
        
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
        // SMOOTH SNAKE-LIKE MOVEMENT:
        // Tank moves with moderate lateral zigzag while smoothly transitioning between directions
        // Tank slows/stops when turning to the new direction (via zigzagSmoothing interpolation)
        
        Vector3 toBase = (targetPosition - transform.position).normalized;
        toBase.y = 0;
        
        // Use moderate angles for natural zigzag
        float maxAngle = Random.Range(maxLateralAngle - 3f, maxLateralAngle); // 22-25 degrees
        float minAngle = Random.Range(minLateralAngle, minLateralAngle + 3f);  // 3-6 degrees
        
        float zigzagAngle;
        
        if (lastZigzagWasLeft)
        {
            // Last was left, so now go right (positive angle)
            zigzagAngle = Random.Range(minAngle, maxAngle);
            lastZigzagWasLeft = false;
        }
        else
        {
            // Last was right, so now go left (negative angle)
            zigzagAngle = Random.Range(-maxAngle, -minAngle);
            lastZigzagWasLeft = true;
        }
        
        // Apply angle for zigzag movement
        Quaternion rotation = Quaternion.Euler(0, zigzagAngle, 0);
        Vector3 tentativeDirection = rotation * toBase;
        
        // CRITICAL: Verify forward progress with dot product
        float dotProduct = Vector3.Dot(toBase, tentativeDirection);
        
        if (dotProduct < 0.9f) // Must have strong forward progress
        {
            // Use a safer angle with some variety
            float safeAngle = Random.Range(5f, 12f);
            zigzagAngle = zigzagAngle > 0 ? safeAngle : -safeAngle;
            rotation = Quaternion.Euler(0, zigzagAngle, 0);
            tentativeDirection = rotation * toBase;
        }
        
        // Smoothly transition to new direction for natural turning motion
        zigzagDirection = tentativeDirection;
        currentDirection = tentativeDirection;
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
            try
            {
                // Create temporary AudioSource for volume control
                GameObject tempAudio = new GameObject("TempExplosionSound");
                tempAudio.transform.position = transform.position;
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = explosionSound;
                // Apply global volume control
                if (VolumeManager.Instance != null)
                {
                    audioSource.volume = VolumeManager.Instance.GetMasterVolume();
                }
                else
                {
                    audioSource.volume = 1f; // Fallback to full volume
                }
                audioSource.Play();
                Destroy(tempAudio, explosionSound.length + 0.1f); // Clean up after sound
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Tank] Error playing explosion sound: {e.Message}");
                // Fallback to original method
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }
        }
        else
        {
            Debug.Log("[Tank] No explosion sound assigned");
        }
        
        Destroy(gameObject);
    }
    
    // Helper method to find child transform by name (recursive search)
    Transform FindDeepChild(Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result != null)
            return result;
        
        foreach (Transform child in parent)
        {
            result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
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
