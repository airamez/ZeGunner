using UnityEngine;

public class RocketCollision : MonoBehaviour
{
    private float lifetime = 10f;
    
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
        
        // Check if hit object is part of a tank (has parent with Enemy tag)
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
        
        // If not part of a tank, just destroy the rocket
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RegisterTankDestroyed(other.gameObject.transform.position);
            }
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
