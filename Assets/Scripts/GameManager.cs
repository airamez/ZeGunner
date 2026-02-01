using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;
    
    private GameObject gameOverScreen;
    private GameObject pauseScreen;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        CurrentState = GameState.Menu;
    }
    
    void Update()
    {
        // Check for ESC key to pause/unpause or close game
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused)
            {
                // Close the game when ESC is pressed while paused
                Application.Quit();
            }
        }
        
        // Check for SPACE key to resume from pause
        if (CurrentState == GameState.Paused)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                ResumeGame();
            }
        }
    }
    
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StartTimer();
            GameTimer.Instance.ShowTimer(true);
        }
        
        // Start the first wave
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartFirstWave();
        }
    }
    
    public void ShowGameOver()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        
        // Get survival time
        string survivalTime = "00:00:00";
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
            GameTimer.Instance.ShowTimer(false);
            survivalTime = GameTimer.Instance.GetSurvivalTime();
        }
        
        // Create game over screen directly
        CreateGameOverScreen(survivalTime);
    }
    
    void CreateGameOverScreen(string survivalTime)
    {
        // Destroy existing game over screen if any
        if (gameOverScreen != null)
        {
            Destroy(gameOverScreen);
        }
        
        // Get wave and stats info
        int wavesSurvived = 0;
        int totalTanks = 0;
        int totalHelis = 0;
        
        if (WaveManager.Instance != null)
        {
            wavesSurvived = WaveManager.Instance.GetCurrentWave();
        }
        if (ScoreManager.Instance != null)
        {
            totalTanks = ScoreManager.Instance.GetTotalTanksDestroyed();
            totalHelis = ScoreManager.Instance.GetTotalHelicoptersDestroyed();
        }
        
        // Find or create canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameOverCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create game over panel
        gameOverScreen = new GameObject("GameOverScreen");
        gameOverScreen.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = gameOverScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Dark background
        Image bg = gameOverScreen.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);
        
        // Game Over title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(gameOverScreen.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.85f);
        titleRect.sizeDelta = new Vector2(800, 120);
        titleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.red;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Stats display
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(gameOverScreen.transform, false);
        RectTransform statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.35f);
        statsRect.anchorMax = new Vector2(0.5f, 0.65f);
        statsRect.sizeDelta = new Vector2(600, 250);
        statsRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = $"Waves Survived: {wavesSurvived}\n" +
                         $"Total Time: {survivalTime}\n\n" +
                         $"Tanks Destroyed: {totalTanks}\n" +
                         $"Helicopters Destroyed: {totalHelis}";
        statsText.fontSize = 36;
        statsText.fontStyle = FontStyles.Bold;
        statsText.color = Color.white;
        statsText.alignment = TextAlignmentOptions.Center;
        
        // Restart message
        GameObject restartObj = new GameObject("RestartMessage");
        restartObj.transform.SetParent(gameOverScreen.transform, false);
        RectTransform restartRect = restartObj.AddComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.5f, 0.15f);
        restartRect.anchorMax = new Vector2(0.5f, 0.25f);
        restartRect.sizeDelta = new Vector2(600, 80);
        restartRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI restartText = restartObj.AddComponent<TextMeshProUGUI>();
        restartText.text = "Press Any Key";
        restartText.fontSize = 36;
        restartText.fontStyle = FontStyles.Bold;
        restartText.color = new Color(1f, 1f, 0.5f, 1f);
        restartText.alignment = TextAlignmentOptions.Center;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }
    
    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        
        // Pause the timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.PauseTimer();
        }
        
        // Show pause screen
        CreatePauseScreen();
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        
        // Resume the timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResumeTimer();
        }
        
        // Hide pause screen
        if (pauseScreen != null)
        {
            Destroy(pauseScreen);
            pauseScreen = null;
        }
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void CreatePauseScreen()
    {
        // Destroy existing pause screen if any
        if (pauseScreen != null)
        {
            Destroy(pauseScreen);
        }
        
        // Find or create canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PauseCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create pause panel
        pauseScreen = new GameObject("PauseScreen");
        pauseScreen.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = pauseScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Semi-transparent background
        Image bg = pauseScreen.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f);
        
        // Paused title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(pauseScreen.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.55f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(600, 100);
        titleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "PAUSED";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Continue message
        GameObject continueObj = new GameObject("ContinueMessage");
        continueObj.transform.SetParent(pauseScreen.transform, false);
        RectTransform continueRect = continueObj.AddComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(0.5f, 0.35f);
        continueRect.anchorMax = new Vector2(0.5f, 0.45f);
        continueRect.sizeDelta = new Vector2(600, 80);
        continueRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI continueText = continueObj.AddComponent<TextMeshProUGUI>();
        continueText.text = "Press SPACE to Continue\n\nPress ESC Again to Close Game";
        continueText.fontSize = 36;
        continueText.fontStyle = FontStyles.Bold;
        continueText.color = new Color(1f, 1f, 0.5f, 1f);
        continueText.alignment = TextAlignmentOptions.Center;
    }
    
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
    }
}
