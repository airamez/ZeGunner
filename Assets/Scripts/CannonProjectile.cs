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
        if (collision.gameObject.CompareTag("Enemy"))
        {
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
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
