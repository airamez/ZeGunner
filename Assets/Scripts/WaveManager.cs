using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    
    [Header("Wave Settings")]
    [Tooltip("Percentage increase per wave (0.2 = 20%)")]
    public float waveIncreasePercent = 0.2f;
    
    // References to spawners to get base counts
    private TankSpawner tankSpawner;
    private HelicopterSpawner helicopterSpawner;
    
    [Header("Current Wave State")]
    private int currentWave = 0;
    private int tanksToSpawnThisWave = 0;
    private int helicoptersToSpawnThisWave = 0;
    private int tanksSpawnedThisWave = 0;
    private int helicoptersSpawnedThisWave = 0;
    private int tanksDestroyedThisWave = 0;
    private int helicoptersDestroyedThisWave = 0;
    private float waveStartTime = 0f;
    private float waveEndTime = 0f;
    private bool waveInProgress = false;
    private bool waitingForNextWave = false;
    
    [Header("Wave Complete UI")]
    private GameObject waveCompletePanel;
    private TextMeshProUGUI waveCompleteText;
    
    // Events for spawners
    public System.Action OnWaveStart;
    public System.Action OnWaveComplete;
    
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
        // Find spawners in scene
        tankSpawner = FindAnyObjectByType<TankSpawner>();
        helicopterSpawner = FindAnyObjectByType<HelicopterSpawner>();
        
        CreateWaveCompleteUI();
    }
    
    void Update()
    {
        // Check for keyboard input to start next wave (no mouse)
        if (waitingForNextWave)
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                StartNextWave();
            }
        }
        
        // Check if wave is complete - all SPAWNED enemies must be destroyed
        if (waveInProgress && !waitingForNextWave)
        {
            // Only check completion after all enemies have been spawned
            bool allSpawned = tanksSpawnedThisWave >= tanksToSpawnThisWave && 
                              helicoptersSpawnedThisWave >= helicoptersToSpawnThisWave;
            bool allDestroyed = tanksDestroyedThisWave >= tanksSpawnedThisWave && 
                                helicoptersDestroyedThisWave >= helicoptersSpawnedThisWave;
            
            if (allSpawned && allDestroyed && tanksSpawnedThisWave > 0)
            {
                CompleteWave();
            }
        }
    }
    
    public void StartFirstWave()
    {
        currentWave = 0;
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        // Find spawners if not already found
        if (tankSpawner == null)
            tankSpawner = FindAnyObjectByType<TankSpawner>();
        if (helicopterSpawner == null)
            helicopterSpawner = FindAnyObjectByType<HelicopterSpawner>();
        
        // Get base counts from spawners
        int baseTankCount = tankSpawner != null ? tankSpawner.BaseTankCount : 5;
        int baseHelicopterCount = helicopterSpawner != null ? helicopterSpawner.BaseHelicopterCount : 2;
        
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }
        
        currentWave++;
        waitingForNextWave = false;
        waveInProgress = true;
        
        // Calculate enemies for this wave with 20% increase per wave
        float multiplier = 1f + (waveIncreasePercent * (currentWave - 1));
        tanksToSpawnThisWave = Mathf.RoundToInt(baseTankCount * multiplier);
        helicoptersToSpawnThisWave = Mathf.RoundToInt(baseHelicopterCount * multiplier);
        
        // Reset counters
        tanksSpawnedThisWave = 0;
        helicoptersSpawnedThisWave = 0;
        tanksDestroyedThisWave = 0;
        helicoptersDestroyedThisWave = 0;
        waveStartTime = Time.time;
        
        
        OnWaveStart?.Invoke();
        
        // Initial score display will show 0/0 until spawners register their spawns
        // The spawners will update the count via RegisterTankSpawned/RegisterHelicopterSpawned
    }
    
    // Called by spawners after all spawning is complete to update the display
    public void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateWaveInfo(currentWave, tanksDestroyedThisWave, tanksSpawnedThisWave,
                helicoptersDestroyedThisWave, helicoptersSpawnedThisWave);
        }
    }
    
    void CompleteWave()
    {
        waveInProgress = false;
        waitingForNextWave = true;
        waveEndTime = Time.time;
        float waveDuration = waveEndTime - waveStartTime;
        
        OnWaveComplete?.Invoke();
        
        ShowWaveCompleteScreen(waveDuration);
    }
    
    void ShowWaveCompleteScreen(float waveDuration)
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
            
            if (waveCompleteText != null)
            {
                waveCompleteText.text = $"WAVE {currentWave} COMPLETE!\n\n" +
                                        $"Time: {FormatTime(waveDuration)}\n\n" +
                                        $"Tanks Destroyed: {tanksDestroyedThisWave}\n" +
                                        $"Helicopters Destroyed: {helicoptersDestroyedThisWave}\n\n" +
                                        $"Press Any Key to Continue";
            }
        }
    }
    
    void CreateWaveCompleteUI()
    {
        // Find existing canvas or create one
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("WaveCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create wave complete panel
        waveCompletePanel = new GameObject("WaveCompletePanel");
        waveCompletePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = waveCompletePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Semi-transparent background
        Image bg = waveCompletePanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        
        // Create text
        GameObject textObj = new GameObject("WaveCompleteText");
        textObj.transform.SetParent(waveCompletePanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 400);
        textRect.anchoredPosition = Vector2.zero;
        
        waveCompleteText = textObj.AddComponent<TextMeshProUGUI>();
        waveCompleteText.fontSize = 36;
        waveCompleteText.color = Color.white;
        waveCompleteText.alignment = TextAlignmentOptions.Center;
        waveCompleteText.text = "Wave Complete!";
        
        waveCompletePanel.SetActive(false);
    }
    
    // Called by spawners to check if they can spawn
    public bool CanSpawnTank()
    {
        return waveInProgress && !waitingForNextWave && tanksSpawnedThisWave < tanksToSpawnThisWave;
    }
    
    public bool CanSpawnHelicopter()
    {
        return waveInProgress && !waitingForNextWave && helicoptersSpawnedThisWave < helicoptersToSpawnThisWave;
    }
    
    // Called by spawners when they spawn
    public void RegisterTankSpawned()
    {
        tanksSpawnedThisWave++;
    }
    
    public void RegisterHelicopterSpawned()
    {
        helicoptersSpawnedThisWave++;
    }
    
    // Called when enemies are destroyed
    public void RegisterTankDestroyed()
    {
        tanksDestroyedThisWave++;
        
        if (ScoreManager.Instance != null)
        {
            // Use spawned count, not target count, for accurate display
            ScoreManager.Instance.UpdateWaveInfo(currentWave, tanksDestroyedThisWave, tanksSpawnedThisWave,
                helicoptersDestroyedThisWave, helicoptersSpawnedThisWave);
        }
    }
    
    public void RegisterHelicopterDestroyed()
    {
        helicoptersDestroyedThisWave++;
        
        if (ScoreManager.Instance != null)
        {
            // Use spawned count, not target count, for accurate display
            ScoreManager.Instance.UpdateWaveInfo(currentWave, tanksDestroyedThisWave, tanksSpawnedThisWave,
                helicoptersDestroyedThisWave, helicoptersSpawnedThisWave);
        }
    }
    
    // Get wave settings with multiplier
    public float GetSpeedMultiplier()
    {
        return 1f + (waveIncreasePercent * (currentWave - 1));
    }
    
    // Getters for game over screen
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetTanksDestroyedThisWave()
    {
        return tanksDestroyedThisWave;
    }
    
    public int GetHelicoptersDestroyedThisWave()
    {
        return helicoptersDestroyedThisWave;
    }
    
    public int GetTotalTanksThisWave()
    {
        return tanksToSpawnThisWave;
    }
    
    public int GetTotalHelicoptersThisWave()
    {
        return helicoptersToSpawnThisWave;
    }
    
    public bool IsWaveInProgress()
    {
        return waveInProgress && !waitingForNextWave;
    }
    
    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }
}
