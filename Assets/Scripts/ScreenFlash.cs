using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }
    
    [Header("Screen Flash Settings")]
    [Tooltip("Image used for screen flash effect")]
    [SerializeField] private Image flashImage;
    
    [Tooltip("Flash color when enemy reaches fire range")]
    [SerializeField] private Color flashColor = Color.red;
    
    [Tooltip("Flash duration in seconds")]
    [SerializeField] private float flashDuration = 0.3f;
    
    [Tooltip("Maximum flash alpha (0-1)")]
    [SerializeField] private float maxAlpha = 0.5f;
    
    private Canvas flashCanvas;
    private bool isFlashing = false;
    private float flashTimer = 0f;
    
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
        
        SetupFlashUI();
    }
    
    void SetupFlashUI()
    {
        // Create canvas if it doesn't exist
        if (flashCanvas == null)
        {
            GameObject canvasObj = new GameObject("ScreenFlashCanvas");
            flashCanvas = canvasObj.AddComponent<Canvas>();
            flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flashCanvas.sortingOrder = 999; // Very high to appear on top
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            canvasObj.transform.SetParent(transform);
        }
        
        // Create flash image if it doesn't exist
        if (flashImage == null)
        {
            GameObject imageObj = new GameObject("FlashImage");
            imageObj.transform.SetParent(flashCanvas.transform);
            
            flashImage = imageObj.AddComponent<Image>();
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            
            // Make image fill the entire screen
            RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
    
    void Update()
    {
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;
            
            // Calculate alpha based on timer (fade out effect)
            float alpha = Mathf.Lerp(maxAlpha, 0f, flashTimer / flashDuration);
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            
            // Stop flashing when duration is reached
            if (flashTimer >= flashDuration)
            {
                StopFlash();
            }
        }
    }
    
    public void FlashScreen()
    {
        if (!isFlashing)
        {
            isFlashing = true;
            flashTimer = 0f;
            
            // Set initial flash color with max alpha
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, maxAlpha);
        }
    }
    
    private void StopFlash()
    {
        isFlashing = false;
        flashTimer = 0f;
        
        // Reset to transparent
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
