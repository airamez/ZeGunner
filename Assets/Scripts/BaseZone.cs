using UnityEngine;

public class BaseZone : MonoBehaviour
{
    void Start()
    {
        Debug.Log("BaseZone initialized on " + gameObject.name);
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning("BaseZone collider is not set as trigger! Setting it now.");
                col.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("BaseZone has no collider component!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("BaseZone trigger entered by: " + other.gameObject.name + " with tag: " + other.tag);
        
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy reached base! Damaging base.");
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(20f); // Direct contact does 20 damage
            }
            Destroy(other.gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("BaseZone collision entered by: " + collision.gameObject.name + " with tag: " + collision.gameObject.tag);
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy reached base via collision! Damaging base.");
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(20f); // Direct contact does 20 damage
            }
            Destroy(collision.gameObject);
        }
    }
}
