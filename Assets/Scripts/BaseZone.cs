using UnityEngine;

public class BaseZone : MonoBehaviour
{
    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (!col.isTrigger)
            {
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
        if (other.CompareTag("Enemy"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(20f); // Direct contact does 20 damage
            }
            Destroy(other.gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.DamageBase(20f); // Direct contact does 20 damage
            }
            Destroy(collision.gameObject);
        }
    }
}
