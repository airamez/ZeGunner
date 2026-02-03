using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }
    
    [Header("Wave Settings")]
    // Wave increments are now configured per-spawner
    // TankSpawner and HelicopterSpawner each have their own baseCountWaveIncrement (absolute), baseSpeedWaveIncrement (absolute), and spawn increments (absolute)
    
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
        // Check for SPACE key or mouse right click to start next wave
        if (waitingForNextWave)
        {
            bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            bool rightMouseClicked = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            
            if (spacePressed || rightMouseClicked)
            {
                StartNextWave();
            }
        }
        
        // Check if wave is complete - all spawned enemies must be destroyed
        if (waveInProgress && !waitingForNextWave)
        {
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
    
    // Debug method to force skip to next wave
    public void ForceNextWave()
    {
        // Hide wave complete panel if showing
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }
        
        // Reset wave state
        waveInProgress = false;
        waitingForNextWave = false;
        
        // Reset spawn counters
        tanksSpawnedThisWave = 0;
        helicoptersSpawnedThisWave = 0;
        tanksDestroyedThisWave = 0;
        helicoptersDestroyedThisWave = 0;
        
        // Start next wave
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        // Resume time when starting next wave
        Time.timeScale = 1f;
        
        // Resume the game timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResumeTimer();
        }
        
        // Find spawners if not already found
        if (tankSpawner == null)
            tankSpawner = FindAnyObjectByType<TankSpawner>();
        if (helicopterSpawner == null)
            helicopterSpawner = FindAnyObjectByType<HelicopterSpawner>();
        
        // Get base counts and absolute increment values from spawners
        int baseTankCount = tankSpawner != null ? tankSpawner.BaseTankCount : 5;
        int baseHelicopterCount = helicopterSpawner != null ? helicopterSpawner.BaseHelicopterCount : 2;
        int tankCountIncrement = tankSpawner != null ? tankSpawner.BaseCountWaveIncrement : 2;
        int heliCountIncrement = helicopterSpawner != null ? helicopterSpawner.BaseCountWaveIncrement : 1;
        
        // Distance progression removed - now using fixed min/max distances from spawners
        
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }
        
        currentWave++;
        waitingForNextWave = false;
        waveInProgress = true;
        
        // Calculate enemies for this wave
        if (currentWave == 1)
        {
            // First wave uses base counts
            tanksToSpawnThisWave = baseTankCount;
            helicoptersToSpawnThisWave = baseHelicopterCount;
        }
        else
        {
            // For subsequent waves, add absolute increment to previous wave's count
            tanksToSpawnThisWave = tanksToSpawnThisWave + tankCountIncrement;
            helicoptersToSpawnThisWave = helicoptersToSpawnThisWave + heliCountIncrement;
        }
        
        // Reset counters
        tanksSpawnedThisWave = 0;
        helicoptersSpawnedThisWave = 0;
        tanksDestroyedThisWave = 0;
        helicoptersDestroyedThisWave = 0;
        waveStartTime = Time.time;
        
        // Reset base HP to 100 for new wave
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetBaseHP();
        }
        
        
        OnWaveStart?.Invoke();
        
        // Initial score display will show 0/0 until spawners register their spawns
        // The spawners will update the count via RegisterTankSpawned/RegisterHelicopterSpawned
    }
    
    // Called by spawners after all spawning is complete to update the display
    public void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateWaveInfo(currentWave, tanksDestroyedThisWave, tanksToSpawnThisWave,
                helicoptersDestroyedThisWave, helicoptersToSpawnThisWave);
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
        // Stop time when showing wave complete screen
        Time.timeScale = 0f;
        
        // Pause the game timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.PauseTimer();
        }
        
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
            
            if (waveCompleteText != null)
            {
                waveCompleteText.text = $"WAVE {currentWave} COMPLETE!\n\n" +
                                        $"Time: {FormatTime(waveDuration)}\n\n" +
                                        $"Tanks Destroyed: {tanksDestroyedThisWave}\n" +
                                        $"Helicopters Destroyed: {helicoptersDestroyedThisWave}\n\n" +
                                        "Press SPACE or Right Click to Continue";
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
        UpdateScoreDisplay();
    }
    
    public void RegisterHelicopterSpawned()
    {
        helicoptersSpawnedThisWave++;
        UpdateScoreDisplay();
    }
    
    // Called when enemies are destroyed (by player or reaching base)
    public void RegisterTankDestroyed()
    {
        tanksDestroyedThisWave++;
        UpdateScoreDisplay();
    }
    
    public void RegisterHelicopterDestroyed()
    {
        helicoptersDestroyedThisWave++;
        UpdateScoreDisplay();
    }
    
    // Get current min speed with absolute increment and limits
    public float GetCurrentTankMinSpeed()
    {
        if (tankSpawner == null) return 2f;
        
        // Calculate: base + (wave-1) * absoluteIncrement
        float currentMinSpeed = tankSpawner.BaseMinSpeed + ((currentWave - 1) * tankSpawner.BaseSpeedWaveIncrement);
        
        // Respect min speed limit
        float finalMinSpeed = Mathf.Min(currentMinSpeed, tankSpawner.MinSpeedLimit);
        
        return finalMinSpeed;
    }
    
    // Get current max speed with absolute increment and limits
    public float GetCurrentTankMaxSpeed()
    {
        if (tankSpawner == null) return 5f;
        
        // Calculate: base + (wave-1) * absoluteIncrement
        float currentMaxSpeed = tankSpawner.BaseMaxSpeed + ((currentWave - 1) * tankSpawner.BaseSpeedWaveIncrement);
        
        // Respect max speed limit
        float finalMaxSpeed = Mathf.Min(currentMaxSpeed, tankSpawner.MaxSpeedLimit);
        
        return finalMaxSpeed;
    }
    
    // Get current min speed for helicopters with absolute increment and limits
    public float GetCurrentHelicopterMinSpeed()
    {
        if (helicopterSpawner == null) return 8f;
        
        // Calculate: base + (wave-1) * absoluteIncrement
        float currentMinSpeed = helicopterSpawner.BaseMinSpeed + ((currentWave - 1) * helicopterSpawner.BaseSpeedWaveIncrement);
        
        // Respect min speed limit
        float finalMinSpeed = Mathf.Min(currentMinSpeed, helicopterSpawner.MinSpeedLimit);
        
        return finalMinSpeed;
    }
    
    // Get current max speed for helicopters with absolute increment and limits
    public float GetCurrentHelicopterMaxSpeed()
    {
        if (helicopterSpawner == null) return 15f;
        
        // Calculate: base + (wave-1) * absoluteIncrement
        float currentMaxSpeed = helicopterSpawner.BaseMaxSpeed + ((currentWave - 1) * helicopterSpawner.BaseSpeedWaveIncrement);
        
        // Respect max speed limit
        float finalMaxSpeed = Mathf.Min(currentMaxSpeed, helicopterSpawner.MaxSpeedLimit);
        
        return finalMaxSpeed;
    }
    
    // Legacy method - returns tank speed multiplier for backwards compatibility
    public float GetSpeedMultiplier()
    {
        // Return a simple multiplier based on current wave for backwards compatibility
        return 1f + (0.1f * (currentWave - 1)); // 10% increase per wave as fallback
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
