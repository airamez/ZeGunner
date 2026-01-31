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
        Debug.Log("Hit object has Enemy tag: " + collision.gameObject.CompareTag("Enemy"));
        Debug.Log("Hit object layer: " + collision.gameObject.layer);
        
        // Check all colliders on the hit object
        Collider[] hitColliders = collision.gameObject.GetComponents<Collider>();
        Debug.Log("Hit object has " + hitColliders.Length + " colliders");
        foreach (Collider col in hitColliders)
        {
            Debug.Log("  Collider: " + col.GetType().Name + " - IsTrigger: " + col.isTrigger);
        }
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Rocket destroyed enemy (direct hit)!");
            
            // Let the enemy handle its own destruction (with its configured explosion)
            Tank tank = collision.gameObject.GetComponent<Tank>();
            if (tank != null)
            {
                tank.SendMessage("DestroyTank", true, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Helicopter heli = collision.gameObject.GetComponent<Helicopter>();
                if (heli != null)
                {
                    heli.SendMessage("DestroyHelicopter", true, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    // Fallback: destroy directly if no Tank/Helicopter component
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.RegisterTankDestroyed(collision.gameObject.transform.position);
                    }
                    Destroy(collision.gameObject);
                }
            }
            
            Destroy(gameObject);
        }
        else
        {
            // Check if hit object is part of a tank (has parent with Enemy tag)
            Transform parent = collision.gameObject.transform;
            while (parent != null)
            {
                if (parent.CompareTag("Enemy"))
                {
                    Debug.Log("Rocket destroyed enemy tank (hit part: " + collision.gameObject.name + ")!");
                    
                    // Let the enemy handle its own destruction (with its configured explosion)
                    Tank tank = parent.GetComponent<Tank>();
                    if (tank != null)
                    {
                        tank.SendMessage("DestroyTank", true, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        Helicopter heli = parent.GetComponent<Helicopter>();
                        if (heli != null)
                        {
                            heli.SendMessage("DestroyHelicopter", true, SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            // Fallback: destroy directly if no Tank/Helicopter component
                            if (ScoreManager.Instance != null)
                            {
                                ScoreManager.Instance.RegisterTankDestroyed(parent.position);
                            }
                            Destroy(parent.gameObject);
                        }
                    }
                    
                    Destroy(gameObject);
                    return;
                }
                parent = parent.parent;
            }
            
            // If not part of a tank, check for terrain
            if (collision.gameObject.CompareTag("Terrain") || 
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
