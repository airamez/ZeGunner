using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSensitivityManager : MonoBehaviour
{
    [Header("Mouse Sensitivity Settings")]
    [SerializeField] private float defaultSensitivity = 2f;
    [SerializeField] private float minSensitivity = 0.5f;
    [SerializeField] private float maxSensitivity = 10f;
    [SerializeField] private float sensitivityStep = 0.5f;
    
    private float currentSensitivity;
    private bool showSensitivityMessage = false;
    private float messageTimer = 0f;
    private const float MESSAGE_DURATION = 2f;
    
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
    }
    
    void Update()
    {
        HandleSensitivityInput();
        UpdateMessageDisplay();
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
        
        // Show message
        showSensitivityMessage = true;
        messageTimer = MESSAGE_DURATION;
        
        Debug.Log($"Mouse sensitivity changed to: {currentSensitivity:F1}");
    }
    
    void UpdateMessageDisplay()
    {
        if (showSensitivityMessage)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
            {
                showSensitivityMessage = false;
            }
        }
    }
    
    void OnGUI()
    {
        if (showSensitivityMessage)
        {
            // Create style for the message
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.yellow;
            
            // Display message at top center of screen
            string message = $"Mouse Sensitivity: {currentSensitivity:F1}\n(+ / - to adjust)";
            Rect messageRect = new Rect(Screen.width / 2 - 150, 50, 300, 60);
            GUI.Box(messageRect, message, style);
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
