using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    
    // Wave tracking
    private int currentWave = 1;
    private int tanksDestroyedThisWave = 0;
    private int totalTanksThisWave = 0;
    private int helicoptersDestroyedThisWave = 0;
    private int totalHelicoptersThisWave = 0;
    
    // Total stats
    private int totalTanksDestroyed = 0;
    private int totalHelicoptersDestroyed = 0;
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
        }
        else
        {
            
        }
        
        // Find the main camera (turret)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        else
        {
            
        }
        
        UpdateUI();
    }
    
    public void RegisterShot()
    {
        shotsFired++;
        UpdateUI();
    }
    
    public void RegisterTankDestroyed(Vector3 tankPosition)
    {
        totalTanksDestroyed++;
        shotsHit++;
        
        if (baseTransform != null)
        {
            float distance = Vector3.Distance(tankPosition, baseTransform.position);
            if (distance > longestDistance)
            {
                longestDistance = distance;
            }
        }
        
        // Notify WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterTankDestroyed();
        }
        
        UpdateUI();
    }
    
    public void RegisterHelicopterDestroyed(Vector3 helicopterPosition)
    {
        totalHelicoptersDestroyed++;
        shotsHit++;
        
        if (baseTransform != null)
        {
            float distance = Vector3.Distance(helicopterPosition, baseTransform.position);
            if (distance > longestDistance)
            {
                longestDistance = distance;
            }
        }
        
        // Notify WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterHelicopterDestroyed();
        }
        
        UpdateUI();
    }
    
    public void UpdateWaveInfo(int wave, int tanksDestroyed, int totalTanks, int helisDestroyed, int totalHelis)
    {
        currentWave = wave;
        tanksDestroyedThisWave = tanksDestroyed;
        totalTanksThisWave = totalTanks;
        helicoptersDestroyedThisWave = helisDestroyed;
        totalHelicoptersThisWave = totalHelis;
        UpdateUI();
    }
    
    public void DamageBase(float damage)
    {
        
        if (isGameOver) 
        {
            return;
        }
        
        baseHP -= damage;
        
        if (baseHP <= 0)
        {
            baseHP = 0;
            isGameOver = true;
            
            // Show game over screen
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowGameOver();
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
        
        scoreText.text = $"Wave: {currentWave}\n" +
                         $"Tanks: {tanksDestroyedThisWave}/{totalTanksThisWave}\n" +
                         $"Helicopters: {helicoptersDestroyedThisWave}/{totalHelicoptersThisWave}\n" +
                         $"Longest Kill: {longestDistance:F1}\n" +
                         $"Accuracy: {accuracy:F1}%\n" +
                         $"Base HP: {baseHP:F0}";
    }
    
    // Getters for game over screen
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetTotalTanksDestroyed()
    {
        return totalTanksDestroyed;
    }
    
    public int GetTotalHelicoptersDestroyed()
    {
        return totalHelicoptersDestroyed;
    }
    
    public float GetBaseHP()
    {
        return baseHP;
    }
}
