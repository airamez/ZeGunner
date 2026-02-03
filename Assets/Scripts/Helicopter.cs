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
    private AudioClip fireRangeReachedSound;
    private bool isDestroyed = false;
    private bool hasPlayedFireRangeSound = false;
    private GameObject projectilePrefab;
    private float distanceToFire;
    private float rateOfFire;
    private float projectileDamage;
    private float projectileSpeed;
    private float projectileScale;
    private bool isFiring = false;
    private float nextFireTime;
    
    // Zigzag movement variables
    private float zigzagDelay;
    private float minLateralSpeed;
    private float maxLateralSpeed;
    private float distanceToStartZigzag;
    private float nextZigzagTime;
    private Vector3 baseDirection;
    private bool zigzagLeft = true;
    
    public void Initialize(Vector3 target, float speed, GameObject explosion, AudioClip sound, GameObject projectile, float fireDist, float fireRate, float damage, float projSpeed, float projScale, AudioClip fireSound, AudioClip fireRangeSound, float zigzagDelay, float minLateralSpeed, float maxLateralSpeed, float distanceToStartZigzag)
    {
        targetPosition = target;
        moveSpeed = speed;
        explosionPrefab = explosion;
        explosionSound = sound;
        firingSound = fireSound;
        fireRangeReachedSound = fireRangeSound;
        
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
        
        // Point helicopter toward base initially
        Vector3 directionToBase = (targetPosition - transform.position).normalized;
        if (directionToBase != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToBase);
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
    
    void Update()
    {
        if (!isInitialized) return;
        
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
            nextFireTime = Time.time + rateOfFire; // Wait for rate of fire before first shot
            
            // Play fire range reached sound (only once)
            if (!hasPlayedFireRangeSound && fireRangeReachedSound != null)
            {
                // Create temporary AudioSource for volume control
                GameObject tempAudio = new GameObject("TempFireRangeSound");
                tempAudio.transform.position = transform.position;
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = fireRangeReachedSound;
                audioSource.volume = 1.5f; // 150% volume - adjust as needed
                audioSource.Play();
                Destroy(tempAudio, fireRangeReachedSound.length + 0.1f); // Clean up after sound
                hasPlayedFireRangeSound = true;
            }
            
            // Face the base
            Vector3 toBase = (targetPosition - transform.position).normalized;
            toBase.y = 0;
            if (toBase != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(toBase);
            }
        }
        
        // Fire at base when in range (hover in place)
        if (isFiring)
        {
            // Keep facing the base
            Vector3 toBase = (targetPosition - transform.position).normalized;
            if (toBase != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(toBase);
            }
            
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
            
            // Always face the base (not movement direction)
            transform.rotation = Quaternion.LookRotation(directionToBase);
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
        
        // Spawn projectile below helicopter
        Vector3 spawnPos = transform.position + Vector3.down * 1f;
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
            Debug.Log($"[Helicopter] Playing explosion sound: {explosionSound.name}");
            try
            {
                // Create temporary AudioSource for volume control
                GameObject tempAudio = new GameObject("TempExplosionSound");
                tempAudio.transform.position = transform.position;
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = explosionSound;
                audioSource.volume = 2.0f; // 200% volume for explosions
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
}
