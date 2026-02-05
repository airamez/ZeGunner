using UnityEngine;

public class Helicopter : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private bool isInitialized = false;
    
    [Header("Rotor Animation")]
    [SerializeField] private float topRotorSpeed = 1000f;
    [SerializeField] private float tailRotorSpeed = 1500f;
    
    private Transform topRotor;
    private Transform tailRotor;
    private GameObject explosionPrefab;
    private AudioClip explosionSound;
    private AudioClip firingSound;
    private bool isDestroyed = false;
    private GameObject projectilePrefab;
    private float distanceToFire;
    private float rateOfFire;
    private float projectileDamage;
    private float projectileSpeed;
    private float projectileScale;
    private bool isFiring = false;
    private float nextFireTime;
    
    // Barrel spawn points - auto-detected by name
    private Transform barrelSpawn1;
    private Transform barrelSpawn2;
    private bool useBarrelSpawn1Next = true; // Track which barrel to use for alternating fire
    
    // Zigzag movement variables
    private float zigzagDelay;
    private float minLateralSpeed;
    
    // Smooth rotation variables
    private float rotationSpeed = 2f; // Speed of smooth rotation
    private Quaternion targetRotation;
    private bool isTurningToBase = false;
    private float maxLateralSpeed;
    private float distanceToStartZigzag;
    private float nextZigzagTime;
    private Vector3 baseDirection;
    private bool zigzagLeft = true;
    
    public void Initialize(Vector3 target, float speed, GameObject explosion, AudioClip sound, GameObject projectile, float fireDist, float fireRate, float damage, float projSpeed, float projScale, AudioClip fireSound, float zigzagDelay, float minLateralSpeed, float maxLateralSpeed, float distanceToStartZigzag)
    {
        targetPosition = target;
        moveSpeed = speed;
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
        
        // Zigzag parameters
        this.zigzagDelay = zigzagDelay;
        this.minLateralSpeed = minLateralSpeed;
        this.maxLateralSpeed = maxLateralSpeed;
        this.distanceToStartZigzag = distanceToStartZigzag;
        
        // Initialize zigzag timing
        CalculateNextZigzagTime();
        
        isInitialized = true;
        
        // Find rotor components
        FindRotorComponents();
        
        // Auto-detect barrel spawn points if not assigned
        FindBarrelSpawnPoints();
        
        // Keep helicopter horizontal (parallel to horizon)
        Vector3 directionToBase = (targetPosition - transform.position).normalized;
        if (directionToBase != Vector3.zero)
        {
            // Only use horizontal direction, ignore vertical component
            directionToBase.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToBase);
            targetRotation = transform.rotation; // Initialize target rotation
        }
        
    }
    
    void FindRotorComponents()
    {
        // Find top rotor
        topRotor = transform.Find("Top_Rotor");
        if (topRotor == null)
        {
            
        }
        
        // Find tail rotor
        tailRotor = transform.Find("Tail_Rotor");
        if (tailRotor == null)
        {
            
        }
        
    }
    
    void FindBarrelSpawnPoints()
    {
        // Auto-detect barrel spawn points by name
        barrelSpawn1 = transform.Find("BarrelSpawn1");
        barrelSpawn2 = transform.Find("BarrelSpawn2");
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Stop all activity when game is over
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return;
        }
        
        // Animate rotors
        AnimateRotors();
        
        // Check distance to base
        float distanceToBase = Vector3.Distance(transform.position, targetPosition);
        
        // Check if helicopter reached the base
        if (distanceToBase < 5f)
        {
            ReachBase();
            return;
        }
        
        // Reset turning state when not in firing range
        if (distanceToBase > distanceToFire)
        {
            isTurningToBase = false;
        }
        
        // Update zigzag timing (only when within zigzag range)
        if (distanceToBase <= distanceToStartZigzag && Time.time >= nextZigzagTime)
        {
            UpdateZigzagDirection();
            CalculateNextZigzagTime();
        }
        
        // Check if helicopter should stop and fire at base
        if (!isFiring && distanceToBase <= distanceToFire)
        {
            isFiring = true;
            
            // Set next fire time for first shot (wait rateOfFire before shooting)
            nextFireTime = Time.time + rateOfFire;
            
            // Flash screen if helicopter is not in player's field of view
            if (!IsInPlayerFieldOfView())
            {
                ScreenFlash.Instance?.FlashScreen();
            }
            
            // Start smooth turning toward base when entering firing lane
            Vector3 toBase = (targetPosition - transform.position).normalized;
            if (toBase != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(toBase);
                isTurningToBase = true;
            }
        }
        
        // Fire at base when in range (hover in place)
        if (isFiring)
        {
            // Continue smooth turning toward base while firing
            Vector3 toBase = (targetPosition - transform.position).normalized;
            if (toBase != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(toBase);
                isTurningToBase = true;
            }
            
            // Apply rotation smoothing even while firing
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            if (Time.time >= nextFireTime)
            {
                FireAtBase();
                nextFireTime = Time.time + rateOfFire;
            }
            return; // Don't move while firing
        }
        
        // Move toward the base with lateral zigzag movement (only when close enough)
        Vector3 directionToBase = (targetPosition - transform.position).normalized;
        
        if (directionToBase != Vector3.zero)
        {
            // Store base direction for zigzag calculations
            baseDirection = directionToBase;
            
            // Check if helicopter should zigzag (within start distance)
            bool shouldZigzag = distanceToBase <= distanceToStartZigzag;
            
            Vector3 totalMovement;
            
            if (shouldZigzag)
            {
                // Calculate perpendicular direction for lateral movement
                Vector3 perpendicular = Vector3.Cross(baseDirection, Vector3.up).normalized;
                
                // Apply lateral movement based on current zigzag direction
                float lateralSpeed = Random.Range(minLateralSpeed, maxLateralSpeed);
                Vector3 lateralMovement = perpendicular * (zigzagLeft ? lateralSpeed : -lateralSpeed) * Time.deltaTime;
                
                // Combine forward movement with lateral movement
                Vector3 forwardMovement = directionToBase * moveSpeed * Time.deltaTime;
                totalMovement = forwardMovement + lateralMovement;
            }
            else
            {
                // Move straight toward base (no zigzag)
                totalMovement = directionToBase * moveSpeed * Time.deltaTime;
            }
            
            // Move helicopter
            transform.position += totalMovement;
            
            // Handle rotation based on state
            if (!isTurningToBase)
            {
                // Return to horizontal orientation when not firing
                Vector3 horizontalDirection = directionToBase;
                horizontalDirection.y = 0; // Force horizontal orientation
                if (horizontalDirection != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(horizontalDirection);
                }
            }
            
            // Apply smooth rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
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
        
        // Spawn projectile from alternating barrel spawn point
        Vector3 spawnPos;
        if (barrelSpawn1 != null && barrelSpawn2 != null)
        {
            // Alternate between barrel 1 and barrel 2
            spawnPos = useBarrelSpawn1Next ? barrelSpawn1.position : barrelSpawn2.position;
            useBarrelSpawn1Next = !useBarrelSpawn1Next; // Toggle for next shot
        }
        else if (barrelSpawn1 != null)
        {
            // Only barrel 1 available
            spawnPos = barrelSpawn1.position;
        }
        else if (barrelSpawn2 != null)
        {
            // Only barrel 2 available
            spawnPos = barrelSpawn2.position;
        }
        else
        {
            // Fallback: spawn below helicopter
            spawnPos = transform.position + Vector3.down * 1f;
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
    
    void AnimateRotors()
    {
        // Spin top rotor (around Y axis - vertical)
        if (topRotor != null)
        {
            topRotor.Rotate(Vector3.up, topRotorSpeed * Time.deltaTime);
        }
        
        // Spin tail rotor (around X axis - horizontal)
        if (tailRotor != null)
        {
            tailRotor.Rotate(Vector3.right, tailRotorSpeed * Time.deltaTime);
        }
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
            DestroyHelicopter(true);
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
            DestroyHelicopter(true);
        }
    }
    
    public void DestroyHelicopter(bool byPlayer)
    {
        if (isDestroyed) return;
        isDestroyed = true;
        
        // Always register with WaveManager for wave completion tracking
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterHelicopterDestroyed();
        }
        
        // Only register with ScoreManager if destroyed by player (for stats)
        if (byPlayer && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterHelicopterDestroyed(transform.position);
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
                Debug.LogError($"[Helicopter] Error playing explosion sound: {e.Message}");
                // Fallback to original method
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }
        }
        else
        {
            Debug.Log("[Helicopter] No explosion sound assigned");
        }
       
        Destroy(gameObject);
    }
    
    // Called when helicopter reaches the base - destroy without player credit
    public void ReachBase()
    {
        // Damage the base
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.DamageBase(projectileDamage * 5f); // Heavy damage for reaching base
        }
        
        DestroyHelicopter(false);
    }
    
    void CalculateNextZigzagTime()
    {
        // Random delay between zigzag changes (50% to 150% of base delay)
        float randomFactor = Random.Range(0.5f, 1.5f);
        nextZigzagTime = Time.time + (zigzagDelay * randomFactor);
    }
    
    void UpdateZigzagDirection()
    {
        // Simply toggle between left and right zigzag direction
        // The actual lateral speed is randomized each frame in movement
        zigzagLeft = !zigzagLeft;
    }
    
    bool IsInPlayerFieldOfView()
    {
        // Find the player's camera (assuming main camera)
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return false;
        
        // Check if helicopter is within camera's view frustum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        Bounds helicopterBounds = GetComponent<Collider>().bounds;
        
        return GeometryUtility.TestPlanesAABB(planes, helicopterBounds);
    }
}
