using UnityEngine;
using UnityEngine.InputSystem;

public class CannonController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 45f;
    [SerializeField] private float fireRate = 0.5f;
    
    [Header("Spawn Point")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private float spawnOffset = 1f;
    
    private float nextFireTime = 0f;
    private Camera mainCamera;
    private Mouse mouse;
    
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        mouse = Mouse.current;
    }
    
    void Update()
    {
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not assigned!");
            return;
        }
        
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset;
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.linearVelocity = mainCamera.transform.forward * projectileSpeed;
        
        CannonProjectile projectileScript = projectile.GetComponent<CannonProjectile>();
        if (projectileScript == null)
        {
            projectile.AddComponent<CannonProjectile>();
        }
    }
}
