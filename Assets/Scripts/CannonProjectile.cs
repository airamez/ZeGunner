using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 10f;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;
        
        // Direct hit check
        if (hitObject.CompareTag("Enemy"))
        {
            // Let the enemy handle its own destruction (with its configured explosion)
            Tank tank = hitObject.GetComponent<Tank>();
            if (tank != null)
            {
                tank.SendMessage("DestroyTank", true, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Helicopter heli = hitObject.GetComponent<Helicopter>();
                if (heli != null)
                {
                    heli.SendMessage("DestroyHelicopter", true, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    // Fallback: destroy directly if no Tank/Helicopter component
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.RegisterTankDestroyed(hitObject.transform.position);
                    }
                    Destroy(hitObject.gameObject);
                }
            }
            
            Destroy(gameObject);
            return;
        }
        
        // Check if hit object is part of an enemy (has parent with Enemy tag)
        Transform parent = collision.gameObject.transform;
        while (parent != null)
        {
            if (parent.CompareTag("Enemy"))
            {
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
        
        // If not an enemy, just destroy the projectile
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Helicopter heli = other.gameObject.GetComponent<Helicopter>();
            if (heli != null)
            {
                heli.SendMessage("DestroyHelicopter", true, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Destroy(other.gameObject);
            }
            
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"CannonProjectile OnTriggerEnter hit non-enemy: {other.gameObject.name} with tag: {other.gameObject.tag}");
        }
    }
}
