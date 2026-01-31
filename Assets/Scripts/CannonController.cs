using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 80f;
    [SerializeField] private float fireRate = 0.5f;
    
    [Header("Projectile Options")]
    [SerializeField] private GameObject sphereProjectile;
    [SerializeField] private GameObject rocketProjectile;
    [SerializeField] private bool useRocketProjectile = false;
    [SerializeField] private float rocketScale = 0.05f;
    
    [Header("Spawn Point")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private float spawnOffset = 1f;
    
    [Header("Camera Movement")]
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float minHeightAboveBase = 2f;
    [SerializeField] private float maxElevation = 50f;
    
    private float nextFireTime = 0f;
    private Camera mainCamera;
    private Mouse mouse;
    private Keyboard keyboard;
    
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        mouse = Mouse.current;
        keyboard = Keyboard.current;
    }
    
    void Update()
    {
        // Only allow controls when game is playing
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return;
        }
        
        HandleVerticalMovement();
        
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    void Fire()
    {
        // Determine which projectile to use
        GameObject selectedProjectile = GetSelectedProjectile();
        
        if (selectedProjectile == null)
        {
            Debug.LogWarning("No projectile prefab assigned!");
            return;
        }
        
        Debug.Log("Fire() called, using projectile: " + selectedProjectile.name);
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterShot();
        }
        
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset;
        Debug.Log("Spawning projectile at: " + spawnPosition);
        
        GameObject projectile = Instantiate(selectedProjectile, spawnPosition, Quaternion.LookRotation(mainCamera.transform.forward));
        Debug.Log("Projectile instantiated: " + projectile.name);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.Log("Adding Rigidbody to projectile");
            rb = projectile.AddComponent<Rigidbody>();
        }
        else
        {
            Debug.Log("Found existing Rigidbody on projectile");
        }
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
        
        Vector3 fireDirection = mainCamera.transform.forward;
        // For rockets, we might need to adjust the forward direction
        if (useRocketProjectile)
        {
            // Rockets might have their forward axis aligned differently
            // Try different orientations if needed
            fireDirection = mainCamera.transform.forward;
        }
        
        rb.linearVelocity = fireDirection * projectileSpeed;
        Debug.Log("Camera forward: " + mainCamera.transform.forward);
        Debug.Log("Fire direction: " + fireDirection);
        Debug.Log("Set velocity to: " + rb.linearVelocity);
        
        // Fix scale and orientation for rocket AFTER setting velocity
        Debug.Log("useRocketProjectile is: " + useRocketProjectile);
        if (useRocketProjectile)
        {
            Debug.Log("Applying rocket fixes...");
            projectile.transform.localScale = Vector3.one * rocketScale;
            Debug.Log("Rocket scale set to: " + projectile.transform.localScale);
            
            // Add collider if rocket doesn't have one
            Collider rocketCollider = projectile.GetComponent<Collider>();
            if (rocketCollider == null)
            {
                Debug.Log("Adding capsule collider to rocket");
                CapsuleCollider capsuleCollider = projectile.AddComponent<CapsuleCollider>();
                capsuleCollider.radius = 0.5f;
                capsuleCollider.height = 2f;
                capsuleCollider.direction = 2; // Z-axis
            }
            else
            {
                Debug.Log("Rocket already has collider: " + rocketCollider.GetType().Name);
            }
            
            // Make rocket follow its velocity direction
            Vector3 velocityDirection = rb.linearVelocity.normalized;
            Quaternion rocketRotation = Quaternion.LookRotation(velocityDirection) * Quaternion.Euler(90, 0, 0);
            projectile.transform.rotation = rocketRotation;
            
            Debug.Log("Rocket orientation set to follow velocity: " + velocityDirection);
        }
        else
        {
            Debug.Log("Not using rocket projectile - skipping rocket fixes");
        }
        
        // Add appropriate collision handling
        if (useRocketProjectile)
        {
            // Add rocket collision script
            RocketCollision rocketScript = projectile.GetComponent<RocketCollision>();
            if (rocketScript == null)
            {
                Debug.Log("Adding RocketCollision script");
                projectile.AddComponent<RocketCollision>();
            }
            else
            {
                Debug.Log("Found existing RocketCollision script");
            }
        }
        else
        {
            // Use sphere projectile collision handling
            CannonProjectile projectileScript = projectile.GetComponent<CannonProjectile>();
            if (projectileScript == null)
            {
                Debug.Log("Adding CannonProjectile script to sphere");
                projectile.AddComponent<CannonProjectile>();
            }
            else
            {
                Debug.Log("Found existing CannonProjectile script");
            }
        }
    }
    
    GameObject GetSelectedProjectile()
    {
        // Try the selected projectile first
        if (useRocketProjectile && rocketProjectile != null)
        {
            return rocketProjectile;
        }
        else if (!useRocketProjectile && sphereProjectile != null)
        {
            return sphereProjectile;
        }
        
        // Fallback to the manually assigned projectile
        if (projectilePrefab != null)
        {
            return projectilePrefab;
        }
        
        // Final fallback - try to find sphere projectile
        if (sphereProjectile != null)
        {
            Debug.LogWarning("Using sphere projectile as fallback");
            return sphereProjectile;
        }
        
        return null;
    }
    
    void HandleVerticalMovement()
    {
        if (keyboard == null) return;
        
        float verticalInput = 0f;
        
        // W key moves up
        if (keyboard.wKey.isPressed)
        {
            verticalInput = 1f;
        }
        // S key moves down
        else if (keyboard.sKey.isPressed)
        {
            verticalInput = -1f;
        }
        
        if (verticalInput != 0f)
        {
            Vector3 currentPosition = transform.position;
            float newY = currentPosition.y + (verticalInput * verticalSpeed * Time.deltaTime);
            
            // Calculate minimum height (base top + minimum height above base)
            float baseTopY = 0f; // Base is at ground level (0,0,0)
            if (baseTransform != null)
            {
                baseTopY = baseTransform.position.y;
            }
            float minY = baseTopY + minHeightAboveBase;
            
            // Clamp the new position to prevent going below minimum height or above maximum elevation
            newY = Mathf.Clamp(newY, minY, maxElevation);
            
            // Apply the new position
            transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        }
    }
}
