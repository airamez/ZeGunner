using UnityEngine;

public class Tank : MonoBehaviour
{
    private Vector3 targetPosition;
    private float moveSpeed;
    private float minSpeed;
    private float maxSpeed;
    private bool isInitialized = false;
    private float reachedBaseDistance = 2f;
    private bool hasReachedBase = false;
    
    [Header("Zigzag Movement")]
    [SerializeField] private float zigzagMinInterval = 2f; // Minimum time before direction change
    [SerializeField] private float zigzagIntervalOffset = 3f; // Random additional time
    [SerializeField] private float maxZigzagAngle = 30f; // Maximum angle deviation in degrees
    [SerializeField] private float zigzagSmoothing = 0.5f; // How smooth the direction changes are
    [SerializeField] private float straightLineDistance = 10f; // Distance from base to move straight
    
    private float nextZigzagTime;
    private Vector3 currentDirection;
    private Vector3 zigzagDirection;
    private bool lastZigzagWasLeft = false; // Track last direction for alternating
    
    public void Initialize(Vector3 target, float speed, float minSpd, float maxSpd, float straightDistance, float minInterval, float intervalOffset, float maxAngle)
    {
        targetPosition = target;
        moveSpeed = speed;
        minSpeed = minSpd;
        maxSpeed = maxSpd;
        straightLineDistance = straightDistance;
        zigzagMinInterval = minInterval;
        zigzagIntervalOffset = intervalOffset;
        maxZigzagAngle = maxAngle;
        isInitialized = true;
        
        // Initialize zigzag movement
        currentDirection = (targetPosition - transform.position).normalized;
        currentDirection.y = 0;
        zigzagDirection = currentDirection;
        CalculateNextZigzagTime();
        lastZigzagWasLeft = false; // Reset alternating direction
        
        if (currentDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(currentDirection);
        }
    }
    
    void CalculateNextZigzagTime()
    {
        // Calculate next direction change time: min + random offset
        float randomOffset = Random.Range(0f, zigzagIntervalOffset);
        nextZigzagTime = Time.time + zigzagMinInterval + randomOffset;
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Check distance to base
        float distanceToBase = Vector3.Distance(transform.position, targetPosition);
        bool shouldMoveStraight = distanceToBase <= straightLineDistance;
        
        Vector3 movementDirection;
        
        if (shouldMoveStraight)
        {
            // Move straight toward base center when close
            Vector3 baseCenter = targetPosition; // targetPosition should be the base center
            movementDirection = (baseCenter - transform.position).normalized;
            movementDirection.y = 0;
            
            // Ensure tank is heading directly to base center
            if (movementDirection != Vector3.zero)
            {
                // Reset zigzag to straight direction toward base center
                zigzagDirection = movementDirection;
                currentDirection = movementDirection;
                
                // Force immediate rotation to face base center
                transform.rotation = Quaternion.LookRotation(movementDirection);
            }
        }
        else
        {
            // Use zigzag movement when far from base
            // Update zigzag direction periodically
            if (Time.time >= nextZigzagTime)
            {
                UpdateZigzagDirection();
                CalculateNextZigzagTime();
            }
            
            // Smoothly interpolate to the zigzag direction
            zigzagDirection = Vector3.Slerp(zigzagDirection, currentDirection, zigzagSmoothing * Time.deltaTime);
            movementDirection = zigzagDirection;
        }
        
        // Move in the calculated direction
        transform.position += movementDirection * moveSpeed * Time.deltaTime;
        
        // Update tank rotation to face movement direction
        if (movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(movementDirection);
        }
        
        // Check if tank reached base
        if (!hasReachedBase)
        {
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
    
    void UpdateZigzagDirection()
    {
        // Calculate the base direction towards the target
        Vector3 baseDirection = (targetPosition - transform.position).normalized;
        baseDirection.y = 0;
        
        // Generate boat-like zigzag that never moves away from base
        Vector3 toBase = (targetPosition - transform.position).normalized;
        toBase.y = 0;
        
        float zigzagAngle;
        string direction;
        
        if (lastZigzagWasLeft)
        {
            // Last was left, so now go right
            zigzagAngle = Random.Range(10f, maxZigzagAngle); // Positive angle = right turn
            lastZigzagWasLeft = false;
            direction = "RIGHT";
        }
        else
        {
            // Last was right (or first turn), so now go left
            zigzagAngle = Random.Range(-maxZigzagAngle, -10f); // Negative angle = left turn
            lastZigzagWasLeft = true;
            direction = "LEFT";
        }
        
        Debug.Log("Zigzag " + direction + " - Angle: " + zigzagAngle + "°");
        
        // Change speed with each direction change
        ChangeSpeed();
        
        // CRITICAL: Calculate safe zigzag angle that never moves away from base
        // Boat movement: can go sideways but never backward
        float safeAngle = zigzagAngle;
        
        // Limit angle to ensure forward progress (never more than 75 degrees from base direction)
        float maxSafeAngle = 75f; // Maximum deviation from base direction
        safeAngle = Mathf.Clamp(safeAngle, -maxSafeAngle, maxSafeAngle);
        
        // Apply the safe angle to direction toward base
        Quaternion rotation = Quaternion.Euler(0, safeAngle, 0);
        currentDirection = rotation * toBase;
        
        // FINAL VERIFICATION: Ensure dot product is always positive (moving toward base)
        float dotProduct = Vector3.Dot(currentDirection.normalized, toBase.normalized);
        if (dotProduct <= 0.2588f) // cos(75°) = 0.2588 - minimum forward progress
        {
            // Force a safer angle if still moving away
            safeAngle = lastZigzagWasLeft ? 30f : -30f; // Conservative 30-degree turn
            rotation = Quaternion.Euler(0, safeAngle, 0);
            currentDirection = rotation * toBase;
            
            Debug.LogWarning("Tank angle too wide - forced to safe angle: " + safeAngle + "°");
        }
        
        // Log final direction for debugging
        float finalDot = Vector3.Dot(currentDirection.normalized, toBase.normalized);
        Debug.Log("Final dot product: " + finalDot + " (always positive = moving toward base)");
    }
    
    void ChangeSpeed()
    {
        // Generate random speed within min/max range
        float newSpeed = Random.Range(minSpeed, maxSpeed);
        moveSpeed = newSpeed;
        
        Debug.Log("Tank speed changed to: " + moveSpeed.ToString("F2") + " (range: " + minSpeed + " - " + maxSpeed + ")");
    }
}
