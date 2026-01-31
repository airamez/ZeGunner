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
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color titleColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color instructionColor = new Color(0.9f, 0.9f, 0.9f);
    
    [Header("Typography")]
    [SerializeField] private Font titleFont;
    [SerializeField] private Font bodyFont;
    [SerializeField] private int titleFontSize = 72;
    [SerializeField] private int instructionFontSize = 28;
    [SerializeField] private int scoreFontSize = 24;
    
    private Canvas canvas;
    private GameObject menuPanel;
    private GameObject gameOverPanel;
    private GameObject gameUI;
    
    void Start()
    {
        // Ensure WaveManager exists
        if (WaveManager.Instance == null)
        {
            GameObject wmObj = new GameObject("WaveManager");
            wmObj.AddComponent<WaveManager>();
        }
        
        if (autoSetupOnStart)
        {
            SetupGameUI();
        }
    }
    
    void Update()
    {
        if (GameManager.Instance != null)
        {
            // Check if we're in menu state and any key is pressed
            if (GameManager.Instance.CurrentState == GameManager.GameState.Menu)
            {
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    StartGame();
                    return;
                }
            }
            // Check if we're in game over state and any key is pressed
            else if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
            {
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    RestartGame();
                    return;
                }
            }
        }
        else
        {
            CreateGameManager();
        }
    }
    
    void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        
        // Also create WaveManager if it doesn't exist
        if (FindAnyObjectByType<WaveManager>() == null)
        {
            GameObject wmObj = new GameObject("WaveManager");
            wmObj.AddComponent<WaveManager>();
        }
    }
    
    [ContextMenu("Setup Game UI")]
    public void SetupGameUI()
    {
        // No EventSystem needed since we're not using clickable buttons
        
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
        // Title with professional styling
        GameObject titleObj = CreateStyledTextObject("Title", panel.transform, "ZE GUNNER", titleFontSize, FontStyles.Bold, titleColor, TextAlignmentOptions.Center);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.85f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.sizeDelta = new Vector2(800, 100);
        
        // Add shadow effect to title
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        titleText.outlineColor = Color.black;
        titleText.outlineWidth = 0.2f;
        
        // Instructions container
        GameObject instructionsContainer = new GameObject("InstructionsContainer");
        instructionsContainer.transform.SetParent(panel.transform, false);
        RectTransform containerRect = instructionsContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.25f);
        containerRect.anchorMax = new Vector2(0.5f, 0.65f);
        containerRect.sizeDelta = new Vector2(800, 400);
        containerRect.anchoredPosition = Vector2.zero;
        
        // Instructions background panel
        GameObject instructionsBg = new GameObject("InstructionsBackground");
        instructionsBg.transform.SetParent(instructionsContainer.transform, false);
        RectTransform bgRect = instructionsBg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(-20, -20);
        bgRect.offsetMax = new Vector2(20, 20);
        Image bgImage = instructionsBg.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.3f);
        bgImage.raycastTarget = false;
        
        // Instructions text (left-aligned)
        string instructions = "- Move the turret (Aiming) using the mouse\n" +
                             "- Fire with the left mouse button\n" +
                             "- Move the Turret up and down using W and S keys\n" +
                             "- If enemies get close to the base they will fire at it\n" +
                             "- If the Base HP reach zero you lose\n" +
                             "- Have fun!";
        
        if (instructionsFile != null)
        {
            instructions = instructionsFile.text;
        }
        
        GameObject instructionsObj = CreateStyledTextObject("Instructions", instructionsContainer.transform, instructions, instructionFontSize, FontStyles.Normal, instructionColor, TextAlignmentOptions.TopLeft);
        RectTransform instrRect = instructionsObj.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0, 0);
        instrRect.anchorMax = new Vector2(1, 1);
        instrRect.offsetMin = new Vector2(20, 20);
        instrRect.offsetMax = new Vector2(-20, -20);
        
        // Add styling to instructions
        TextMeshProUGUI instrText = instructionsObj.GetComponent<TextMeshProUGUI>();
        instrText.lineSpacing = 1.2f;
        instrText.outlineColor = Color.black;
        instrText.outlineWidth = 0.1f;
        
        // "Press Any Key to Start" message with animation
        GameObject startMsgObj = CreateAnimatedTextObject("StartMessage", panel.transform, "Press Any Key to Start", 36, FontStyles.Bold, new Color(1f, 1f, 0.5f, 1f));
        RectTransform msgRect = startMsgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.08f);
        msgRect.anchorMax = new Vector2(0.5f, 0.18f);
        msgRect.sizeDelta = new Vector2(600, 80);
        
    }
    
    void CreateGameOverContent(GameObject panel)
    {
        
        // Game Over Title with professional styling
        GameObject titleObj = CreateStyledTextObject("GameOverTitle", panel.transform, "GAME OVER", titleFontSize, FontStyles.Bold, Color.red, TextAlignmentOptions.Center);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax = new Vector2(0.5f, 0.85f);
        titleRect.sizeDelta = new Vector2(800, 150);
        
        // Add shadow effect to title
        TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        titleText.outlineColor = Color.black;
        titleText.outlineWidth = 0.3f;
        
        // Subtitle with styling
        GameObject subtitleObj = CreateStyledTextObject("Subtitle", panel.transform, "Your base was destroyed!", 36, FontStyles.Normal, new Color(1f, 0.8f, 0.8f), TextAlignmentOptions.Center);
        RectTransform subRect = subtitleObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.45f);
        subRect.anchorMax = new Vector2(0.5f, 0.55f);
        subRect.sizeDelta = new Vector2(600, 80);
        
        // Add styling to subtitle
        TextMeshProUGUI subtitleText = subtitleObj.GetComponent<TextMeshProUGUI>();
        subtitleText.outlineColor = Color.black;
        subtitleText.outlineWidth = 0.15f;
        
        // "Press Any Key to Restart" message with animation
        GameObject restartMsgObj = CreateAnimatedTextObject("RestartMessage", panel.transform, "Press Any Key", 36, FontStyles.Bold, new Color(1f, 1f, 0.5f, 1f));
        RectTransform msgRect = restartMsgObj.GetComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0.5f, 0.25f);
        msgRect.anchorMax = new Vector2(0.5f, 0.35f);
        msgRect.sizeDelta = new Vector2(600, 80);
        
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
        
        // Create professional score display panel
        GameObject scorePanel = new GameObject("ScorePanel");
        scorePanel.transform.SetParent(gameUIObj.transform, false);
        
        RectTransform scorePanelRect = scorePanel.AddComponent<RectTransform>();
        scorePanelRect.anchorMin = new Vector2(0, 1);
        scorePanelRect.anchorMax = new Vector2(0, 1);
        scorePanelRect.sizeDelta = new Vector2(1000, 500);
        scorePanelRect.anchoredPosition = new Vector2(20, -20);
        
        // Score background with styling
        Image scoreBg = scorePanel.AddComponent<Image>();
        scoreBg.color = new Color(0, 0, 0, 0.7f);
        scoreBg.raycastTarget = false;
        
        // No border needed - clean look with just the background
        
        // Create score text object for ScoreManager to use
        GameObject scoreTextObj = CreateStyledTextObject("ScoreText", scorePanel.transform, "", scoreFontSize, FontStyles.Bold, Color.white, TextAlignmentOptions.TopLeft);
        RectTransform scoreTextRect = scoreTextObj.GetComponent<RectTransform>();
        scoreTextRect.anchorMin = new Vector2(0, 0);
        scoreTextRect.anchorMax = new Vector2(1, 1);
        scoreTextRect.offsetMin = new Vector2(10, 10);
        scoreTextRect.offsetMax = new Vector2(-10, -10);
        
        // Add styling to score text
        TextMeshProUGUI scoreText = scoreTextObj.GetComponent<TextMeshProUGUI>();
        scoreText.lineSpacing = 1.3f;
        scoreText.outlineColor = Color.black;
        scoreText.outlineWidth = 0.15f;
        
        // Find ScoreManager and assign the text object
        ScoreManager scoreManager = FindAnyObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            // Use reflection to set the private scoreText field
            var scoreTextField = typeof(ScoreManager).GetField("scoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (scoreTextField != null)
            {
                scoreTextField.SetValue(scoreManager, scoreText);
            }
        }
        
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
    
    GameObject CreateStyledTextObject(string name, Transform parent, string text, int fontSize, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        
        return textObj;
    }
    
    GameObject CreateAnimatedTextObject(string name, Transform parent, string text, int fontSize, FontStyles style, Color color)
    {
        GameObject textObj = CreateStyledTextObject(name, parent, text, fontSize, style, color, TextAlignmentOptions.Center);
        
        // Add pulsing animation component
        TextAnimator animator = textObj.AddComponent<TextAnimator>();
        animator.Initialize(color);
        
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
        }
        
        // Create GameTimer component
        GameTimer timer = FindAnyObjectByType<GameTimer>();
        if (timer == null)
        {
            GameObject timerObj = new GameObject("GameTimer");
            timer = timerObj.AddComponent<GameTimer>();
        }
        
        // Set initial state to Menu
        if (gm != null)
        {
            gm.SetGameState(GameManager.GameState.Menu);
        }
        
        // Find the buttons and panels we created
        Button startBtn = menuPanel.GetComponentInChildren<Button>();
        Button restartBtn = gameOverPanel.GetComponentInChildren<Button>();
        TextMeshProUGUI instructionsText = menuPanel.transform.Find("Instructions")?.GetComponent<TextMeshProUGUI>();
        
        // The GameManager will find these by name if needed
    }
    
    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        
        if (menuPanel != null) 
        {
            menuPanel.SetActive(false);
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameUI != null) 
        {
            gameUI.SetActive(true);
        }
    }
    
    void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}
