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
    private int tanksReachedBase = 0;
    
    private Transform baseTransform;
    
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
    
    public void RegisterTankReachedBase()
    {
        tanksReachedBase++;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (scoreText == null) return;
        
        float accuracy = shotsFired > 0 ? (shotsHit / (float)shotsFired) * 100f : 0f;
        
        scoreText.text = $"Enemies Destroyed: {tanksDestroyed}\n" +
                         $"Longest Kill: {longestDistance:F1}\n" +
                         $"Accuracy: {accuracy:F1}%\n" +
                         $"Enemies Reached Base: {tanksReachedBase}";
    }
}
