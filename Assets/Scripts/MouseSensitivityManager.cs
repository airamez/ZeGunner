using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class MouseSensitivityManager : MonoBehaviour
{
    [Header("Mouse Sensitivity Settings")]
    [SerializeField] private float defaultSensitivity = 2f;
    [SerializeField] private float minSensitivity = 0.5f;
    [SerializeField] private float maxSensitivity = 10f;
    [SerializeField] private float sensitivityStep = 0.5f;
    
    private float currentSensitivity;
    private GameObject sensitivityPanel;
    private TextMeshProUGUI sensitivityText;
    private float displayTimer = 0f;
    private const float DISPLAY_DURATION = 2f;
    
    public static MouseSensitivityManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize sensitivity
        currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);
        
        // Create UI
        CreateSensitivityUI();
    }
    
    void Update()
    {
        HandleSensitivityInput();
        UpdateDisplay();
    }
    
    void HandleSensitivityInput()
    {
        if (Keyboard.current != null)
        {
            // Increase sensitivity with + key
            if (Keyboard.current.equalsKey.wasPressedThisFrame || Keyboard.current.numpadPlusKey.wasPressedThisFrame)
            {
                ChangeSensitivity(sensitivityStep);
            }
            
            // Decrease sensitivity with - key
            if (Keyboard.current.minusKey.wasPressedThisFrame || Keyboard.current.numpadMinusKey.wasPressedThisFrame)
            {
                ChangeSensitivity(-sensitivityStep);
            }
        }
    }
    
    void ChangeSensitivity(float change)
    {
        currentSensitivity = Mathf.Clamp(currentSensitivity + change, minSensitivity, maxSensitivity);
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MouseSensitivity", currentSensitivity);
        PlayerPrefs.Save();
        
        // Show UI
        ShowSensitivityDisplay();
    }
    
    void UpdateDisplay()
    {
        if (sensitivityPanel != null && sensitivityPanel.activeInHierarchy)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0f)
            {
                sensitivityPanel.SetActive(false);
            }
        }
    }
    
    void CreateSensitivityUI()
    {
        // Find or create canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SensitivityCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create sensitivity panel
        sensitivityPanel = new GameObject("SensitivityPanel");
        sensitivityPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = sensitivityPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(300, 80);
        panelRect.anchoredPosition = new Vector2(0, -100); // Below center (opposite of volume)
        
        // Add background
        Image bgImage = sensitivityPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        // Create sensitivity text
        GameObject textObj = new GameObject("SensitivityText");
        textObj.transform.SetParent(sensitivityPanel.transform, false);
        sensitivityText = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform textRect = sensitivityText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        sensitivityText.fontSize = 24;
        sensitivityText.color = Color.white;
        sensitivityText.alignment = TextAlignmentOptions.Center;
        sensitivityText.text = $"Mouse Sensitivity: {currentSensitivity:F1}";
        
        // Hide initially
        sensitivityPanel.SetActive(false);
    }
    
    void ShowSensitivityDisplay()
    {
        if (sensitivityPanel != null && sensitivityText != null)
        {
            sensitivityText.text = $"Mouse Sensitivity: {currentSensitivity:F1}";
            sensitivityPanel.SetActive(true);
            displayTimer = DISPLAY_DURATION;
        }
    }
    
    public float GetSensitivity()
    {
        return currentSensitivity;
    }
    
    public void SetSensitivity(float sensitivity)
    {
        currentSensitivity = Mathf.Clamp(sensitivity, minSensitivity, maxSensitivity);
        PlayerPrefs.SetFloat("MouseSensitivity", currentSensitivity);
        PlayerPrefs.Save();
    }
    
    public float GetNormalizedSensitivity()
    {
        // Return 0-1 value for UI sliders
        return Mathf.InverseLerp(minSensitivity, maxSensitivity, currentSensitivity);
    }
    
    public void SetSensitivityFromNormalized(float normalizedValue)
    {
        SetSensitivity(Mathf.Lerp(minSensitivity, maxSensitivity, normalizedValue));
    }
}
