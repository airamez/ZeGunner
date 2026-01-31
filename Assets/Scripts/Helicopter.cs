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
    
    // Firing system
    private GameObject projectilePrefab;
    private float distanceToFire;
    private float rateOfFire;
    private float projectileDamage;
    private float projectileSpeed;
    private float projectileScale;
    private bool isFiring = false;
    private float nextFireTime;
    
    public void Initialize(Vector3 target, float speed, GameObject explosion, AudioClip sound, GameObject projectile, float fireDist, float fireRate, float damage, float projSpeed, float projScale)
    {
        targetPosition = target;
        moveSpeed = speed;
        explosionPrefab = explosion;
        explosionSound = sound;
        
        // Firing parameters
        projectilePrefab = projectile;
        distanceToFire = fireDist;
        rateOfFire = fireRate;
        projectileDamage = damage;
        projectileSpeed = projSpeed;
        projectileScale = projScale;
        
        isInitialized = true;
        
        // Find rotor components
        FindRotorComponents();
        
        // Point helicopter toward base initially
        Vector3 directionToBase = (targetPosition - transform.position).normalized;
        if (directionToBase != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToBase);
        }
        
        Debug.Log("Helicopter initialized with speed: " + speed);
    }
    
    void FindRotorComponents()
    {
        // Find top rotor
        topRotor = transform.Find("Top_Rotor");
        if (topRotor == null)
        {
            Debug.LogWarning("Top_Rotor not found on helicopter prefab!");
        }
        
        // Find tail rotor
        tailRotor = transform.Find("Tail_Rotor");
        if (tailRotor == null)
        {
            Debug.LogWarning("Tail_Rotor not found on helicopter prefab!");
        }
        
        Debug.Log("Found rotors - Top: " + (topRotor != null ? "Yes" : "No") + ", Tail: " + (tailRotor != null ? "Yes" : "No"));
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Animate rotors (always spin)
        AnimateRotors();
        
        // Check distance to base
        float distanceToBase = Vector3.Distance(transform.position, targetPosition);
        
        // Check if helicopter should stop and fire at base
        if (!isFiring && distanceToBase <= distanceToFire)
        {
            isFiring = true;
            nextFireTime = Time.time; // Fire immediately when entering range
            Debug.Log("Helicopter in firing range at distance: " + distanceToBase);
            
            // Face the base
            Vector3 toBase = (targetPosition - transform.position).normalized;
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
        
        // Move toward the base
        Vector3 directionToBase = (targetPosition - transform.position).normalized;
        
        if (directionToBase != Vector3.zero)
        {
            // Move helicopter
            transform.position += directionToBase * moveSpeed * Time.deltaTime;
            
            // Rotate helicopter to face movement direction
            transform.rotation = Quaternion.LookRotation(directionToBase);
        }
    }
    
    void FireAtBase()
    {
        if (projectilePrefab == null)
        {
            // No projectile, just damage base directly
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(projectileDamage);
            }
            Debug.Log("Helicopter fired at base (no projectile) - Damage: " + projectileDamage);
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
        
        Debug.Log("Helicopter fired projectile at base");
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
        Debug.Log("Helicopter collision with: " + collision.gameObject.name);
        
        // Check if hit by projectile
        GameObject hitObject = collision.gameObject;
        if (hitObject.CompareTag("Projectile") || 
            hitObject.name.Contains("Projectile") || 
            hitObject.name.Contains("Rocket") || 
            hitObject.name.Contains("Sphere") ||
            hitObject.GetComponent<CannonProjectile>() != null ||
            hitObject.GetComponent<RocketCollision>() != null)
        {
            Debug.Log("Helicopter hit by projectile!");
            DestroyHelicopter(true);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Helicopter trigger with: " + other.gameObject.name);
        
        // Also check trigger collisions (some projectiles use triggers)
        GameObject hitObject = other.gameObject;
        if (hitObject.CompareTag("Projectile") || 
            hitObject.name.Contains("Projectile") || 
            hitObject.name.Contains("Rocket") || 
            hitObject.name.Contains("Sphere") ||
            hitObject.GetComponent<CannonProjectile>() != null ||
            hitObject.GetComponent<RocketCollision>() != null)
        {
            Debug.Log("Helicopter hit by projectile (trigger)!");
            DestroyHelicopter(true);
        }
    }
    
    public void DestroyHelicopter(bool byPlayer)
    {
        if (byPlayer && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterTankDestroyed(transform.position); // Reuse tank destroyed for now
        }
        
        // Play explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            // Optional: Destroy explosion after some time
            Destroy(explosion, 5f);
        }
        
        // Play explosion sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
        
        Debug.Log("Helicopter destroyed by " + (byPlayer ? "player" : "reaching base"));
        Destroy(gameObject);
    }
}
