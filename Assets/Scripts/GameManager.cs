using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public enum GameState { Menu, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;
    
    private GameObject gameOverScreen;
    
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
    
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StartTimer();
            GameTimer.Instance.ShowTimer(true);
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
        titleRect.anchorMin = new Vector2(0.5f, 0.6f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(800, 150);
        titleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.red;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Survival time
        GameObject survivalObj = new GameObject("SurvivalTime");
        survivalObj.transform.SetParent(gameOverScreen.transform, false);
        RectTransform survivalRect = survivalObj.AddComponent<RectTransform>();
        survivalRect.anchorMin = new Vector2(0.5f, 0.4f);
        survivalRect.anchorMax = new Vector2(0.5f, 0.55f);
        survivalRect.sizeDelta = new Vector2(600, 100);
        survivalRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI survivalText = survivalObj.AddComponent<TextMeshProUGUI>();
        survivalText.text = "You survived " + survivalTime;
        survivalText.fontSize = 42;
        survivalText.fontStyle = FontStyles.Bold;
        survivalText.color = Color.white;
        survivalText.alignment = TextAlignmentOptions.Center;
        
        // Restart message
        GameObject restartObj = new GameObject("RestartMessage");
        restartObj.transform.SetParent(gameOverScreen.transform, false);
        RectTransform restartRect = restartObj.AddComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.5f, 0.2f);
        restartRect.anchorMax = new Vector2(0.5f, 0.35f);
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
    
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
    }
}
