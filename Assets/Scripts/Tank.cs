using UnityEngine;

public class Tank : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private bool isInitialized = false;
    private float reachedBaseDistance = 2f;
    private bool hasReachedBase = false;
    
    public void Initialize(Vector3 target, float speed)
    {
        targetPosition = target;
        moveSpeed = speed;
        isInitialized = true;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Check if tank reached base
        if (!hasReachedBase)
        {
            float distanceToBase = Vector3.Distance(transform.position, targetPosition);
            if (distanceToBase <= reachedBaseDistance)
            {
                hasReachedBase = true;
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.RegisterTankReachedBase();
                    Debug.Log("Tank reached base at distance: " + distanceToBase);
                }
                Destroy(gameObject);
            }
        }
    }
}
