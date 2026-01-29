using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    private Canvas canvas;
    private TextMeshProUGUI scoreText;
    
    void Start()
    {
        CreateUI();
    }
    
    void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("ScoreCanvas");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create TextMeshPro text
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(canvasObj.transform);
        
        scoreText = textObj.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 24;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.TopLeft;
        scoreText.text = "Initializing...";
        
        // Position at top left
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);
        rectTransform.sizeDelta = new Vector2(400, 200);
        
        // Add outline for better visibility
        var outline = textObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        // Connect to ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.scoreText = scoreText;
            Debug.Log("ScoreUI connected to ScoreManager");
        }
        else
        {
            Debug.LogError("ScoreManager.Instance is null! Make sure ScoreManager component is on the same GameObject.");
        }
    }
}
