using UnityEngine;

public class Helicopter : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private bool isInitialized = false;
    private float reachedBaseDistance = 5f;
    private bool hasReachedBase = false;
    
    [Header("Rotor Animation")]
    [SerializeField] private float topRotorSpeed = 1000f;
    [SerializeField] private float tailRotorSpeed = 1500f;
    
    private Transform topRotor;
    private Transform tailRotor;
    private string explosionFolderPath;
    
    public void Initialize(Vector3 target, float speed, string explosionPath)
    {
        targetPosition = target;
        moveSpeed = speed;
        explosionFolderPath = explosionPath;
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
        
        // Animate rotors
        AnimateRotors();
        
        // Move toward the base
        Vector3 directionToBase = (targetPosition - transform.position).normalized;
        
        if (directionToBase != Vector3.zero)
        {
            // Move helicopter
            transform.position += directionToBase * moveSpeed * Time.deltaTime;
            
            // Rotate helicopter to face movement direction
            transform.rotation = Quaternion.LookRotation(directionToBase);
        }
        
        // Check if helicopter reached base
        if (!hasReachedBase)
        {
            float distanceToBase = Vector3.Distance(transform.position, targetPosition);
            if (distanceToBase <= reachedBaseDistance)
            {
                hasReachedBase = true;
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.RegisterTankReachedBase(); // Reuse tank reached base for now
                }
                DestroyHelicopter(false); // False = not by player
            }
        }
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
    
    void DestroyHelicopter(bool byPlayer)
    {
        if (byPlayer && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterTankDestroyed(transform.position); // Reuse tank destroyed for now
        }
        
        // Play explosion effect if ExplosionManager exists
        if (ExplosionManager.Instance != null)
        {
            ExplosionManager.Instance.PlayExplosionEffect(transform.position);
        }
        
        Debug.Log("Helicopter destroyed by " + (byPlayer ? "player" : "reaching base"));
        Destroy(gameObject);
    }
}
