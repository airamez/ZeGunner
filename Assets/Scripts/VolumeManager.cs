using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance { get; private set; }
    
    [Header("Volume Settings")]
    [Range(0f, 2f)]
    public float masterVolume = 1.0f;
    
    [Header("UI")]
    public TextMeshProUGUI volumeText;
    public GameObject volumePanel;
    public float volumeDisplayTime = 2f;
    
    private float volumeDisplayTimer;
    private bool isShowingVolume;
    
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
    }
    
    void Start()
    {
        Debug.Log("[VolumeManager] VolumeManager started");
        
        // Load saved volume or use default
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        Debug.Log($"[VolumeManager] Loaded volume: {masterVolume:P0}");
        ApplyVolume();
        
        // Create volume UI if not assigned
        if (volumePanel == null)
        {
            Debug.Log("[VolumeManager] Creating volume UI");
            CreateVolumeUI();
        }
        else
        {
            volumePanel.SetActive(false);
        }
    }
    
    void Update()
    {
        HandleVolumeInput();
        UpdateVolumeDisplay();
    }
    
    void HandleVolumeInput()
    {
        // Check for volume controls
        if (Keyboard.current != null)
        {
            // Comma key (,) - Decrease volume
            if (Keyboard.current.commaKey.wasPressedThisFrame)
            {
                Debug.Log("[VolumeManager] Comma key pressed - Decreasing volume");
                DecreaseVolume();
            }
            
            // Period key (.) - Increase volume
            if (Keyboard.current.periodKey.wasPressedThisFrame)
            {
                Debug.Log("[VolumeManager] Period key pressed - Increasing volume");
                IncreaseVolume();
            }
            
            // Also support < and > keys (using comma and period as alternatives)
            // Note: < and > keys are typically the same as comma and period on most keyboards
            // The Input System doesn't have separate lessThanKey/greaterThanKey
        }
        else
        {
            Debug.LogWarning("[VolumeManager] Keyboard.current is null - Input System may not be initialized");
        }
    }
    
    void DecreaseVolume()
    {
        masterVolume = Mathf.Max(0f, masterVolume - 0.1f);
        ApplyVolume();
        ShowVolumeDisplay();
        SaveVolume();
    }
    
    void IncreaseVolume()
    {
        masterVolume = Mathf.Min(2f, masterVolume + 0.1f);
        ApplyVolume();
        ShowVolumeDisplay();
        SaveVolume();
    }
    
    void ApplyVolume()
    {
        // Apply to global AudioListener
        AudioListener.volume = masterVolume;
        
        Debug.Log($"[VolumeManager] Volume set to {masterVolume:P0}");
    }
    
    void ShowVolumeDisplay()
    {
        if (volumePanel != null && volumeText != null)
        {
            volumePanel.SetActive(true);
            volumeText.text = $"Volume: {masterVolume:P0}";
            volumeDisplayTimer = volumeDisplayTime;
            isShowingVolume = true;
        }
    }
    
    void UpdateVolumeDisplay()
    {
        if (isShowingVolume && volumePanel != null)
        {
            volumeDisplayTimer -= Time.deltaTime;
            
            if (volumeDisplayTimer <= 0f)
            {
                volumePanel.SetActive(false);
                isShowingVolume = false;
            }
        }
    }
    
    void SaveVolume()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }
    
    // Public method for other scripts to get current volume
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    // Public method to set volume from UI
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp(volume, 0f, 2f);
        ApplyVolume();
        ShowVolumeDisplay();
        SaveVolume();
    }
    
    void CreateVolumeUI()
    {
        // Find or create canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("VolumeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create volume panel
        volumePanel = new GameObject("VolumePanel");
        volumePanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = volumePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(300, 80);
        panelRect.anchoredPosition = new Vector2(0, 100);
        
        // Add background
        Image bgImage = volumePanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        
        // Create volume text
        GameObject textObj = new GameObject("VolumeText");
        textObj.transform.SetParent(volumePanel.transform, false);
        volumeText = textObj.AddComponent<TextMeshProUGUI>();
        
        RectTransform textRect = volumeText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        volumeText.fontSize = 24;
        volumeText.color = Color.white;
        volumeText.alignment = TextAlignmentOptions.Center;
        volumeText.text = "Volume: 100%";
        
        // Hide initially
        volumePanel.SetActive(false);
    }
}
