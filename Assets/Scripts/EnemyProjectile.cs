using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector3 direction;
    private bool isInitialized = false;
    private float lifetime = 10f;
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
            rb.isKinematic = true;
        }
        else
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        
        // Add Collider if missing
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.5f;
            sphereCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
        
        // Try to set tag, but don't fail if tag doesn't exist
        try
        {
            gameObject.tag = "EnemyProjectile";
        }
        catch (System.Exception)
        {
            // Tag not defined in Unity, ignore
        }
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Move projectile in straight line toward target
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate to face direction of travel (tip-first)
        if (direction != Vector3.zero)
        {
            // LookRotation makes Z+ point toward direction
            // Add 90 on X to make projectile tip point toward direction
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        }
        
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if hit the base
        if (other.gameObject.name.Contains("Base") || IsTaggedAs(other.gameObject, "Base"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(damage);
            }
            Destroy(gameObject);
            return;
        }
        
        // Destroy on terrain hit
        if (IsTaggedAs(other.gameObject, "Terrain") || 
            other.gameObject.GetComponent<Terrain>() != null ||
            other.gameObject.name.Contains("Terrain") ||
            other.gameObject.name.Contains("Ground"))
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if hit the base
        if (collision.gameObject.name.Contains("Base") || IsTaggedAs(collision.gameObject, "Base"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(damage);
            }
            Destroy(gameObject);
            return;
        }
        
        // Destroy on any collision
        Destroy(gameObject);
    }
    
    // Safe tag check that doesn't throw if tag doesn't exist
    bool IsTaggedAs(GameObject obj, string tagName)
    {
        try
        {
            return obj.CompareTag(tagName);
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}
