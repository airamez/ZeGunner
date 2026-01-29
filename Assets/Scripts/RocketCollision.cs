using UnityEngine;

public class RocketCollision : MonoBehaviour
{
    private float lifetime = 10f;
    
    void Start()
    {
        Debug.Log("RocketCollision script started on " + gameObject.name);
        Destroy(gameObject, lifetime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("=== ROCKET COLLISION ===");
        Debug.Log("Rocket hit: " + collision.gameObject.name + " with tag: " + collision.gameObject.tag);
        Debug.Log("Collision force: " + collision.relativeVelocity.magnitude);
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Rocket destroyed enemy!");
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RegisterTankDestroyed(collision.gameObject.transform.position);
            }
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Terrain") || 
                 collision.gameObject.GetComponent<Terrain>() != null)
        {
            Debug.Log("Rocket hit terrain, destroying rocket");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Rocket hit something else, destroying rocket");
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("=== ROCKET TRIGGER ===");
        Debug.Log("Rocket trigger entered: " + other.gameObject.name + " with tag: " + other.tag);
        
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Rocket destroyed enemy via trigger!");
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RegisterTankDestroyed(other.gameObject.transform.position);
            }
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
