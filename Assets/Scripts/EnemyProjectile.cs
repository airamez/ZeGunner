using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector3 direction;
    private bool isInitialized = false;
    private float lifetime = 10f; // Destroy after 10 seconds if no collision
    private float spawnTime;
    
    public void Initialize(float projectileDamage, float projectileSpeed, Vector3 moveDirection)
    {
        damage = projectileDamage;
        speed = projectileSpeed;
        direction = moveDirection.normalized;
        isInitialized = true;
        spawnTime = Time.time;
        
        // Add Rigidbody if missing
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;
        }
        
        // Add Collider if missing
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f;
            sphereCol.isTrigger = true;
        }
        
        // Set tag
        gameObject.tag = "EnemyProjectile";
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Move projectile
        transform.position += direction * speed * Time.deltaTime;
        
        // Destroy after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if hit the base
        if (other.gameObject.name.Contains("Base") || other.gameObject.CompareTag("Base"))
        {
            // Damage the base
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(damage);
            }
            
            Debug.Log("Enemy projectile hit base - Damage: " + damage);
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if hit the base
        if (collision.gameObject.name.Contains("Base") || collision.gameObject.CompareTag("Base"))
        {
            // Damage the base
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(damage);
            }
            
            Debug.Log("Enemy projectile hit base (collision) - Damage: " + damage);
            Destroy(gameObject);
        }
    }
}
