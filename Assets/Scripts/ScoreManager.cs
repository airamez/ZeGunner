using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    
    private int tanksDestroyed = 0;
    private int shotsFired = 0;
    private int shotsHit = 0;
    private float longestDistance = 0f;
    private float baseHP = 100f;
    private bool isGameOver = false;
    
    private Transform baseTransform;
    private Transform cameraTransform;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        GameObject baseObj = GameObject.Find("Base");
        if (baseObj != null)
        {
            baseTransform = baseObj.transform;
            Debug.Log("Base found at position: " + baseTransform.position);
        }
        else
        {
            Debug.LogWarning("Base GameObject not found in scene!");
        }
        
        // Find the main camera (turret)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
            Debug.Log("Camera found for height tracking");
        }
        else
        {
            Debug.LogWarning("Main camera not found!");
        }
        
        Debug.Log("ScoreManager initialized. scoreText is " + (scoreText == null ? "NULL" : "assigned"));
        UpdateUI();
    }
    
    public void RegisterShot()
    {
        shotsFired++;
        UpdateUI();
    }
    
    public void RegisterTankDestroyed(Vector3 tankPosition)
    {
        tanksDestroyed++;
        shotsHit++;
        
        if (baseTransform != null)
        {
            float distance = Vector3.Distance(tankPosition, baseTransform.position);
            if (distance > longestDistance)
            {
                longestDistance = distance;
            }
        }
        
        UpdateUI();
    }
    
    public void DamageBase(float damage)
    {
        Debug.Log("DamageBase called with damage: " + damage + ", current HP: " + baseHP + ", isGameOver: " + isGameOver);
        
        if (isGameOver) 
        {
            Debug.Log("Already game over, ignoring damage");
            return;
        }
        
        baseHP -= damage;
        Debug.Log("Base HP after damage: " + baseHP);
        
        if (baseHP <= 0)
        {
            baseHP = 0;
            isGameOver = true;
            Debug.Log("GAME OVER - Base Destroyed! HP reached zero.");
            
            // Show game over screen
            if (GameManager.Instance != null)
            {
                Debug.Log("Calling GameManager.Instance.ShowGameOver()...");
                GameManager.Instance.ShowGameOver();
            }
            else
            {
                Debug.LogError("GameManager.Instance is null!");
            }
        }
        
        UpdateUI();
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    void Update()
    {
        // Continuously update UI to show current turret height
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (scoreText == null) return;
        
        float accuracy = shotsFired > 0 ? (shotsHit / (float)shotsFired) * 100f : 0f;
        
        // Get turret elevation from camera with max limit of 50
        float turretElevation = 0f;
        if (cameraTransform != null)
        {
            turretElevation = Mathf.Min(cameraTransform.position.y, 50f);
        }
        
        scoreText.text = $"Enemies Destroyed: {tanksDestroyed}\n" +
                         $"Longest Kill: {longestDistance:F1}\n" +
                         $"Accuracy: {accuracy:F1}%\n" +
                         $"Turret Elevation: {turretElevation:F1}\n" +
                         $"Base HP: {baseHP:F0}";
    }
}
