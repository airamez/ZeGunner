using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject rocketProjectile;
    [SerializeField] private GameObject rocketExplosionOnGround;
    [SerializeField] private AudioClip rocketFiringSound;
    [SerializeField] private float projectileSpeed = 80f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float rocketScale = 0.05f;
    [SerializeField] private float maxProjectileDistance = 500f;
    
    [Header("Spawn Point")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private float spawnOffset = 1f;
    [SerializeField] private float launcherHeight = 2f;
    
    [Header("Camera Movement")]
    [SerializeField] private float verticalSpeed = 5f;
    [SerializeField] private float minHeight = 2f;
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
        if (rocketProjectile == null)
        {
            return;
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterShot();
        }
        
        // Play rocket firing sound with volume control
        PlayRocketFiringSound();
        
        // Calculate spawn position above the camera
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraUp = mainCamera.transform.up;
        
        // Spawn position: forward offset + upward offset based on launcher height
        Vector3 spawnPosition = cameraPosition + 
                              (cameraForward * spawnOffset) + 
                              (cameraUp * launcherHeight);
        
        // Calculate direction from spawn position to crosshair (center of screen)
        Vector3 crosshairPosition = cameraPosition + cameraForward * 100f; // Point far in front of camera
        Vector3 fireDirection = (crosshairPosition - spawnPosition).normalized;
        
        GameObject projectile = Instantiate(rocketProjectile, spawnPosition, Quaternion.LookRotation(fireDirection));
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        else
        {
            
        }
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false; // Rockets don't use gravity
        
        rb.linearVelocity = fireDirection * projectileSpeed;
        
        // Set rocket scale and orientation
        projectile.transform.localScale = Vector3.one * rocketScale;
        
        // Add collider if rocket doesn't have one
        Collider rocketCollider = projectile.GetComponent<Collider>();
        if (rocketCollider == null)
        {
            CapsuleCollider capsuleCollider = projectile.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 2f;
            capsuleCollider.direction = 2; // Z-axis
        }
        
        // Make rocket follow its velocity direction
        Vector3 velocityDirection = rb.linearVelocity.normalized;
        Quaternion rocketRotation = Quaternion.LookRotation(velocityDirection) * Quaternion.Euler(90, 0, 0);
        projectile.transform.rotation = rocketRotation;
        
        // Add rocket collision script
        RocketCollision rocketScript = projectile.GetComponent<RocketCollision>();
        if (rocketScript == null)
        {
            rocketScript = projectile.AddComponent<RocketCollision>();
        }
        
        // Set max distance for rocket
        rocketScript.SetMaxDistance(maxProjectileDistance);
        
        // Set explosion prefab for ground hits
        if (rocketExplosionOnGround != null)
        {
            rocketScript.SetExplosionPrefab(rocketExplosionOnGround);
        }
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
            
            // Calculate minimum height from ground level (0,0,0)
            float minY = minHeight; // Direct height from ground level
            
            // Clamp the new position to prevent going below minimum height or above maximum elevation
            newY = Mathf.Clamp(newY, minY, maxElevation);
            
            // Apply the new position
            transform.position = new Vector3(currentPosition.x, newY, currentPosition.z);
        }
    }
    
    void PlayRocketFiringSound()
    {
        if (rocketFiringSound != null)
        {
            // Create temporary AudioSource for volume control
            GameObject tempAudio = new GameObject("TempRocketFiringSound");
            tempAudio.transform.position = transform.position;
            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            
            // Apply global volume from VolumeManager with 50% base volume
            if (VolumeManager.Instance != null)
            {
                // Start at 50% of Windows 100%, then apply global volume multiplier
                audioSource.volume = 0.5f * VolumeManager.Instance.GetMasterVolume();
            }
            else
            {
                // Fallback to 50% if VolumeManager not available
                audioSource.volume = 0.5f;
            }
            
            audioSource.PlayOneShot(rocketFiringSound);
            Destroy(tempAudio, rocketFiringSound.length + 0.1f); // Clean up after sound plays
        }
    }
}
