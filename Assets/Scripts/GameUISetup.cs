using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class GameUISetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    [Header("Instructions")]
    [SerializeField] private TextAsset instructionsFile;
    
    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0, 0, 0, 0.85f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas canvas;
    private GameObject menuPanel;
    private GameObject gameOverPanel;
    private GameObject gameUI;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGameUI();
        }
    }
    
    void Update()
    {
        // Debug every frame for troubleshooting
        if (Time.frameCount % 30 == 0) // Log every 0.5 seconds
        {
            Debug.Log("Frame " + Time.frameCount + " - GameManager.Instance: " + (GameManager.Instance != null ? "exists" : "null"));
            if (GameManager.Instance != null)
            {
                Debug.Log("Frame " + Time.frameCount + " - Current State: " + GameManager.Instance.CurrentState);
            }
        }
        
        if (GameManager.Instance != null)
        {
            // Check if we're in menu state and any key is pressed
            if (GameManager.Instance.CurrentState == GameManager.GameState.Menu)
            {
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    Debug.Log("KEY DETECTED! Any key pressed in Menu state - calling StartGame()");
                    StartGame();
                    return;
                }
            }
            // Check if we're in game over state and any key is pressed
            else if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
            {
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    Debug.Log("KEY DETECTED! Any key pressed in GameOver state - calling RestartGame()");
                    RestartGame();
                    return;
                }
            }
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null! Trying to create GameManager...");
            CreateGameManager();
        }
    }
    
    void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        Debug.Log("Created GameManager manually");
    }
    
    [ContextMenu("Setup Game UI")]
    public void SetupGameUI()
    {
        // No EventSystem needed since we're not using clickable buttons
        Debug.Log("Skipping EventSystem creation - using keyboard input only");
        
        // Create or find Canvas
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Setup CanvasScaler for responsive UI
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }
        
        // Create Menu Panel
        menuPanel = CreatePanel("MenuPanel", canvas.transform);
        CreateMenuContent(menuPanel);
        
        // Create Game Over Panel
        gameOverPanel = CreatePanel("GameOverPanel", canvas.transform);
        CreateGameOverContent(gameOverPanel);
        gameOverPanel.SetActive(false);
        
        // Create Game UI (for score display during gameplay)
        gameUI = CreateGameUI(canvas.transform);
        gameUI.SetActive(false);
        
        // Setup GameManager references
        SetupGameManager();
        
        Debug.Log("Game UI Setup Complete!");
    }
    
    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image img = panel.AddComponent<Image>();
        img.color = panelColor;
        
        return panel;
    }
    
    void CreateMenuContent(GameObject panel)
    {
        // Title
        GameObject titleObj = CreateTextObject("Title", panel.transform, "ZE GUNNER", 72, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.85f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(800, 100);
        
        // Instructions
        string instructions = "- Move the turret using the mouse\n" +
                             "- Fire with the mouse left button\n" +
                             "- Move the Turret up and down using W and S keys\n" +
                             "- If enemies get close to the base they will fire at it\n" +
                             "- If the Base HP reach zero you lose\n" +
                             "- Have fun!";
        
        if (instructionsFile != null)
        {
            instructions = instructionsFile.text;
        }
        
        GameObject instructionsObj = CreateTextObject("Instructions", panel.transform, instructions, 28, FontStyles.Normal);
        RectTransform instrRect = instructionsObj.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.5f, 0.35f);
        instrRect.anchorMax = new Vector2(0.5f, 0.75f);
        instrRect.sizeDelta = new Vector2(700, 400);
        
        // "Press Any Key to Start" message
        GameObject startMsgObj = CreateTextObject("StartMessage", panel.transform, "Press Any Key to Start", 36, FontStyles.Bold);
        RectTransform msgRect = startMsgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.15f);
        msgRect.anchorMax = new Vector2(0.5f, 0.25f);
        msgRect.sizeDelta = new Vector2(600, 80);
        
        // Make it pulse/flash
        TextMeshProUGUI msgText = startMsgObj.GetComponent<TextMeshProUGUI>();
        msgText.color = new Color(1f, 1f, 0.5f, 1f); // Yellow color
        
        Debug.Log("Created Start Message: Press Any Key to Start");
    }
    
    void CreateGameOverContent(GameObject panel)
    {
        // Game Over Title
        GameObject titleObj = CreateTextObject("GameOverTitle", panel.transform, "GAME OVER", 72, FontStyles.Bold);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.6f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(800, 150);
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.color = Color.red;
        
        // Subtitle
        GameObject subtitleObj = CreateTextObject("Subtitle", panel.transform, "Your base was destroyed!", 36, FontStyles.Normal);
        RectTransform subRect = subtitleObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.45f);
        subRect.anchorMax = new Vector2(0.5f, 0.55f);
        subRect.sizeDelta = new Vector2(600, 80);
        
        // "Press Any Key to Restart" message
        GameObject restartMsgObj = CreateTextObject("RestartMessage", panel.transform, "Press Any Key to Restart", 36, FontStyles.Bold);
        RectTransform msgRect = restartMsgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.2f);
        msgRect.anchorMax = new Vector2(0.5f, 0.35f);
        msgRect.sizeDelta = new Vector2(600, 80);
        
        // Make it pulse/flash
        TextMeshProUGUI msgText = restartMsgObj.GetComponent<TextMeshProUGUI>();
        msgText.color = new Color(1f, 1f, 0.5f, 1f); // Yellow color
        
        Debug.Log("Created Restart Message: Press Any Key to Restart");
    }
    
    GameObject CreateGameUI(Transform parent)
    {
        GameObject gameUIObj = new GameObject("GameUI");
        gameUIObj.transform.SetParent(parent, false);
        
        RectTransform rect = gameUIObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // The ScoreManager already has its own UI text, so this is just a container
        // You can add additional game UI elements here if needed
        
        return gameUIObj;
    }
    
    GameObject CreateTextObject(string name, Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false; // Don't block button clicks
        
        return textObj;
    }
    
    GameObject CreateButton(string name, Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        
        Image img = buttonObj.AddComponent<Image>();
        img.color = buttonColor;
        img.raycastTarget = true; // Ensure button receives clicks
        
        Button btn = buttonObj.AddComponent<Button>();
        btn.targetGraphic = img;
        
        // Make button work even when Time.timeScale = 0
        ColorBlock colors = btn.colors;
        colors.fadeDuration = 0f;
        btn.colors = colors;
        
        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        
        // Add click listener
        btn.onClick.AddListener(onClick);
        
        return buttonObj;
    }
    
    void SetupGameManager()
    {
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<GameManager>();
            Debug.Log("Created GameManager during UI setup");
        }
        
        // Set initial state to Menu
        if (gm != null)
        {
            gm.SetGameState(GameManager.GameState.Menu);
            Debug.Log("Set initial GameState to Menu");
        }
        
        // Find the buttons and panels we created
        Button startBtn = menuPanel.GetComponentInChildren<Button>();
        Button restartBtn = gameOverPanel.GetComponentInChildren<Button>();
        TextMeshProUGUI instructionsText = menuPanel.transform.Find("Instructions")?.GetComponent<TextMeshProUGUI>();
        
        // The GameManager will find these by name if needed
        Debug.Log("GameManager setup - Menu Panel: " + menuPanel.name + ", GameOver Panel: " + gameOverPanel.name);
    }
    
    public void StartGame()
    {
        Debug.Log("=== START GAME CALLED ===");
        Debug.Log("GameManager.Instance: " + (GameManager.Instance != null ? "exists" : "null"));
        Debug.Log("Menu Panel: " + (menuPanel != null ? "exists" : "null"));
        Debug.Log("Game UI: " + (gameUI != null ? "exists" : "null"));
        
        if (GameManager.Instance != null)
        {
            Debug.Log("Calling GameManager.Instance.StartGame()...");
            GameManager.Instance.StartGame();
            Debug.Log("GameManager.Instance.StartGame() completed");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
        
        if (menuPanel != null) 
        {
            menuPanel.SetActive(false);
            Debug.Log("Menu panel hidden");
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameUI != null) 
        {
            gameUI.SetActive(true);
            Debug.Log("Game UI shown");
        }
        
        Debug.Log("=== START GAME FINISHED ===");
    }
    
    void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
