using UnityEngine;

public class Tank : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private bool isInitialized = false;
    
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
    }
}
