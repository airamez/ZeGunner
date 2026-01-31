using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }
    
    private float gameTime = 0f;
    private bool isRunning = false;
    private TextMeshProUGUI timerText;
    
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
        Debug.Log("GameTimer Start() called");
        // Find or create timer text
        timerText = CreateTimerText();
        if (timerText != null)
        {
            Debug.Log("Timer text created successfully");
        }
        else
        {
            Debug.LogError("Failed to create timer text!");
        }
    }
    
    void Update()
    {
        if (isRunning)
        {
            gameTime += Time.unscaledDeltaTime; // Use unscaled time so it works when game is paused
            UpdateTimerDisplay();
        }
    }
    
    TextMeshProUGUI CreateTimerText()
    {
        Debug.Log("Creating timer text...");
        
        // Find the GameUISetup to get the canvas
        GameUISetup uiSetup = FindAnyObjectByType<GameUISetup>();
        if (uiSetup == null)
        {
            Debug.LogWarning("GameUISetup not found!");
            return null;
        }
        
        Debug.Log("Found GameUISetup");
        
        // Get the canvas from GameUISetup using reflection
        var canvasField = typeof(GameUISetup).GetField("canvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Canvas canvas = canvasField?.GetValue(uiSetup) as Canvas;
        
        if (canvas == null)
        {
            Debug.LogWarning("Canvas not found!");
            return null;
        }
        
        Debug.Log("Found canvas, creating timer UI...");
        
        // Create timer text object
        GameObject timerObj = new GameObject("GameTimer");
        timerObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = timerObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(300, 60);
        rect.anchoredPosition = new Vector2(0, -30);
        
        TextMeshProUGUI text = timerObj.AddComponent<TextMeshProUGUI>();
        text.text = "00:00:00";
        text.fontSize = 36;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        
        // Add styling
        text.outlineColor = Color.black;
        text.outlineWidth = 0.2f;
        
        // Create background for timer
        GameObject timerBg = new GameObject("TimerBackground");
        timerBg.transform.SetParent(timerObj.transform, false);
        RectTransform bgRect = timerBg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(-10, -5);
        bgRect.offsetMax = new Vector2(10, 5);
        
        Image bgImage = timerBg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        bgImage.raycastTarget = false;
        
        // Move background behind text
        timerBg.transform.SetAsFirstSibling();
        
        Debug.Log("Timer UI created successfully");
        return text;
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(gameTime);
        }
    }
    
    string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
    
    public void StartTimer()
    {
        gameTime = 0f;
        isRunning = true;
        Debug.Log("Timer started");
    }
    
    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("Timer stopped at: " + FormatTime(gameTime));
    }
    
    public void PauseTimer()
    {
        isRunning = false;
    }
    
    public void ResumeTimer()
    {
        isRunning = true;
    }
    
    public string GetSurvivalTime()
    {
        return FormatTime(gameTime);
    }
    
    public void ShowTimer(bool show)
    {
        if (timerText != null && timerText.gameObject != null)
        {
            timerText.gameObject.SetActive(show);
            if (timerText.transform.parent != null)
            {
                timerText.transform.parent.gameObject.SetActive(show);
            }
        }
    }
}
