using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState { Menu, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;
    
    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameUI;
    
    [Header("Menu UI")]
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private Button startButton;
    
    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;
    
    [Header("Instructions File")]
    [SerializeField] private TextAsset instructionsFile;
    
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
        // Find UI panels if not assigned
        FindUIPanels();
        
        // Setup button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        Debug.Log("GameManager Start - Menu Panel: " + (menuPanel != null ? "found" : "not found"));
        Debug.Log("GameManager Start - GameOver Panel: " + (gameOverPanel != null ? "found" : "not found"));
        Debug.Log("GameManager Start - Game UI: " + (gameUI != null ? "found" : "not found"));
        
        // Load instructions
        LoadInstructions();
        
        // Set initial state to Menu (but don't pause yet - GameUISetup will handle UI)
        CurrentState = GameState.Menu;
    }
    
    void FindUIPanels()
    {
        // Find the GameUISetup and get its panels
        GameUISetup uiSetup = FindAnyObjectByType<GameUISetup>();
        if (uiSetup != null)
        {
            // Use reflection to get private fields
            var menuPanelField = typeof(GameUISetup).GetField("menuPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameOverPanelField = typeof(GameUISetup).GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameUIField = typeof(GameUISetup).GetField("gameUI", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (menuPanelField != null && menuPanel == null)
                menuPanel = menuPanelField.GetValue(uiSetup) as GameObject;
            if (gameOverPanelField != null && gameOverPanel == null)
                gameOverPanel = gameOverPanelField.GetValue(uiSetup) as GameObject;
            if (gameUIField != null && gameUI == null)
                gameUI = gameUIField.GetValue(uiSetup) as GameObject;
                
            Debug.Log("Found UI panels from GameUISetup");
        }
        else
        {
            Debug.LogWarning("GameUISetup not found!");
        }
    }
    
    void LoadInstructions()
    {
        if (instructionsText != null)
        {
            if (instructionsFile != null)
            {
                instructionsText.text = instructionsFile.text;
            }
            else
            {
                // Fallback instructions
                instructionsText.text = "- Move the turret using the mouse\n" +
                                       "- Fire with the mouse left button\n" +
                                       "- Move the Turret up and down using W and S keys\n" +
                                       "- If enemies get close to the base they will fire at it\n" +
                                       "- If the Base HP reach zero you lose\n" +
                                       "- Have fun!";
            }
        }
    }
    
    public void ShowMenu()
    {
        CurrentState = GameState.Menu;
        Time.timeScale = 0f; // Pause the game
        
        if (menuPanel != null) menuPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
        
        // Unlock cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f; // Resume the game
        
        if (menuPanel != null) menuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        
        // Lock cursor for gameplay (optional - depends on your game)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
        
        Debug.Log("Game Started!");
    }
    
    public void ShowGameOver()
    {
        Debug.Log("=== SHOW GAME OVER CALLED ===");
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f; // Pause the game
        
        Debug.Log("Menu Panel: " + (menuPanel != null ? "exists" : "null"));
        Debug.Log("GameOver Panel: " + (gameOverPanel != null ? "exists" : "null"));
        Debug.Log("Game UI: " + (gameUI != null ? "exists" : "null"));
        
        if (menuPanel != null) 
        {
            menuPanel.SetActive(false);
            Debug.Log("Menu panel hidden");
        }
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(true);
            Debug.Log("Game over panel shown");
        }
        else
        {
            Debug.LogError("GameOver panel is null!");
        }
        if (gameUI != null) 
        {
            gameUI.SetActive(false); // Hide game UI during game over
            Debug.Log("Game UI hidden");
        }
        
        // Unlock cursor for game over screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("=== SHOW GAME OVER FINISHED ===");
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; // Make sure time is running before reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }
    
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("GameState changed to: " + newState);
    }
}
