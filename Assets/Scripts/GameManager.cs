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
        // Setup button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        // Load instructions
        LoadInstructions();
        
        // Set initial state to Menu (but don't pause yet - GameUISetup will handle UI)
        CurrentState = GameState.Menu;
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
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f; // Pause the game
        
        if (menuPanel != null) menuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        // Unlock cursor for game over screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Game Over!");
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
