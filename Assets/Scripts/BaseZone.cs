using UnityEngine;

public class BaseZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RegisterTankReachedBase();
            }
            Destroy(other.gameObject);
        }
    }
}
