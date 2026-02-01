using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadarHUD : MonoBehaviour
{
    [Header("Radar Settings")]
    [SerializeField] private float radarSize = 225f;
    [SerializeField] private float radarRange = 150f; // Max distance to show on radar
    [SerializeField] private float dotSize = 6f;
    [SerializeField] private float heightLineWidth = 2f;
    [SerializeField] private float maxHeightLineLength = 30f;
    
    [Header("Colors")]
    [SerializeField] private Color radarBackgroundColor = new Color(0f, 0.2f, 0f, 0.7f);
    [SerializeField] private Color radarBorderColor = new Color(0f, 1f, 0f, 0.8f);
    [SerializeField] private Color tankColor = new Color(1f, 0.3f, 0.3f, 1f); // Red for tanks
    [SerializeField] private Color helicopterColor = new Color(1f, 1f, 0.3f, 1f); // Yellow for helicopters
    [SerializeField] private Color centerColor = new Color(0f, 1f, 0f, 1f); // Green for base/center
    [SerializeField] private Color rangeCircleColor = new Color(0f, 0.5f, 0f, 0.5f);
    
    [Header("References")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private Transform cameraTransform;
    
    private GameObject radarPanel;
    private RectTransform radarRect;
    private List<GameObject> enemyDots = new List<GameObject>();
    private List<GameObject> heightLines = new List<GameObject>();
    private GameObject centerDot;
    private GameObject directionIndicator;
    
    void Start()
    {
        // Find base transform if not assigned
        if (baseTransform == null)
        {
            GameObject baseObj = GameObject.Find("Base");
            if (baseObj != null)
                baseTransform = baseObj.transform;
        }
        
        // Find camera transform if not assigned
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
                cameraTransform = mainCam.transform;
        }
        
        CreateRadarHUD();
    }
    
    void CreateRadarHUD()
    {
        // Find or create canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("RadarCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create radar panel
        radarPanel = new GameObject("RadarHUD");
        radarPanel.transform.SetParent(canvas.transform, false);
        
        radarRect = radarPanel.AddComponent<RectTransform>();
        radarRect.anchorMin = new Vector2(1, 1);
        radarRect.anchorMax = new Vector2(1, 1);
        radarRect.pivot = new Vector2(1, 1);
        radarRect.anchoredPosition = new Vector2(-20, -20);
        radarRect.sizeDelta = new Vector2(radarSize, radarSize);
        
        // Add background image (circular)
        Image bgImage = radarPanel.AddComponent<Image>();
        bgImage.color = radarBackgroundColor;
        bgImage.raycastTarget = false;
        
        // Create circular mask
        GameObject maskObj = new GameObject("RadarMask");
        maskObj.transform.SetParent(radarPanel.transform, false);
        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;
        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.sprite = CreateCircleSprite();
        maskImage.raycastTarget = false;
        
        // Create radar border (circle outline)
        CreateRadarBorder();
        
        // Create range circles
        CreateRangeCircles();
        
        // Create center dot (base position)
        centerDot = CreateDot(centerColor, dotSize * 1.5f, "CenterDot");
        RectTransform centerRect = centerDot.GetComponent<RectTransform>();
        centerRect.anchoredPosition = Vector2.zero;
        
        // Create direction indicator (shows where camera is looking)
        CreateDirectionIndicator();
    }
    
    void CreateRadarBorder()
    {
        GameObject borderObj = new GameObject("RadarBorder");
        borderObj.transform.SetParent(radarPanel.transform, false);
        
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.pivot = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(radarSize, radarSize);
        
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.sprite = CreateCircleOutlineSprite();
        borderImage.color = radarBorderColor;
        borderImage.raycastTarget = false;
    }
    
    void CreateRangeCircles()
    {
        // Create inner range circles (25%, 50%, 75%)
        float[] ranges = { 0.25f, 0.5f, 0.75f };
        foreach (float range in ranges)
        {
            GameObject circleObj = new GameObject($"RangeCircle_{range}");
            circleObj.transform.SetParent(radarPanel.transform, false);
            
            RectTransform circleRect = circleObj.AddComponent<RectTransform>();
            circleRect.anchorMin = new Vector2(0.5f, 0.5f);
            circleRect.anchorMax = new Vector2(0.5f, 0.5f);
            circleRect.pivot = new Vector2(0.5f, 0.5f);
            circleRect.anchoredPosition = Vector2.zero;
            circleRect.sizeDelta = new Vector2(radarSize * range, radarSize * range);
            
            Image circleImage = circleObj.AddComponent<Image>();
            circleImage.sprite = CreateCircleOutlineSprite();
            circleImage.color = rangeCircleColor;
            circleImage.raycastTarget = false;
        }
    }
    
    void CreateDirectionIndicator()
    {
        directionIndicator = new GameObject("DirectionIndicator");
        directionIndicator.transform.SetParent(radarPanel.transform, false);
        
        RectTransform dirRect = directionIndicator.AddComponent<RectTransform>();
        dirRect.anchorMin = new Vector2(0.5f, 0.5f);
        dirRect.anchorMax = new Vector2(0.5f, 0.5f);
        dirRect.pivot = new Vector2(0.5f, 0f);
        dirRect.anchoredPosition = Vector2.zero;
        dirRect.sizeDelta = new Vector2(4f, radarSize * 0.45f);
        
        Image dirImage = directionIndicator.AddComponent<Image>();
        dirImage.color = new Color(0f, 1f, 0f, 0.6f);
        dirImage.raycastTarget = false;
    }
    
    GameObject CreateDot(Color color, float size, string name = "EnemyDot")
    {
        GameObject dot = new GameObject(name);
        dot.transform.SetParent(radarPanel.transform, false);
        
        RectTransform dotRect = dot.AddComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0.5f, 0.5f);
        dotRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotRect.pivot = new Vector2(0.5f, 0.5f);
        dotRect.sizeDelta = new Vector2(size, size);
        
        Image dotImage = dot.AddComponent<Image>();
        dotImage.sprite = CreateCircleSprite();
        dotImage.color = color;
        dotImage.raycastTarget = false;
        
        return dot;
    }
    
    GameObject CreateHeightLine(Color color, string name = "HeightLine")
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(radarPanel.transform, false);
        
        RectTransform lineRect = line.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0f);
        lineRect.sizeDelta = new Vector2(heightLineWidth, 0f);
        
        Image lineImage = line.AddComponent<Image>();
        lineImage.color = color;
        lineImage.raycastTarget = false;
        
        return line;
    }
    
    Sprite CreateCircleSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float center = resolution / 2f;
        float radius = resolution / 2f - 1;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    Sprite CreateCircleOutlineSprite()
    {
        int resolution = 64;
        Texture2D texture = new Texture2D(resolution, resolution);
        
        float center = resolution / 2f;
        float outerRadius = resolution / 2f - 1;
        float innerRadius = outerRadius - 2;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= outerRadius && dist >= innerRadius)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }
    
    void Update()
    {
        if (radarPanel == null || baseTransform == null) return;
        
        UpdateDirectionIndicator();
        UpdateEnemyPositions();
    }
    
    void UpdateDirectionIndicator()
    {
        if (cameraTransform == null || directionIndicator == null) return;
        
        // Get camera's forward direction on the XZ plane
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        
        // Calculate angle from forward (Z+)
        float angle = Mathf.Atan2(cameraForward.x, cameraForward.z) * Mathf.Rad2Deg;
        
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, -angle);
    }
    
    void UpdateEnemyPositions()
    {
        // Find all enemies
        Tank[] tanks = FindObjectsByType<Tank>(FindObjectsSortMode.None);
        Helicopter[] helicopters = FindObjectsByType<Helicopter>(FindObjectsSortMode.None);
        
        int totalEnemies = tanks.Length + helicopters.Length;
        
        // Ensure we have enough dots and height lines
        while (enemyDots.Count < totalEnemies)
        {
            enemyDots.Add(CreateDot(tankColor, dotSize));
            heightLines.Add(CreateHeightLine(helicopterColor));
        }
        
        // Hide excess dots and lines
        for (int i = totalEnemies; i < enemyDots.Count; i++)
        {
            if (enemyDots[i] != null)
                enemyDots[i].SetActive(false);
            if (heightLines[i] != null)
                heightLines[i].SetActive(false);
        }
        
        int dotIndex = 0;
        
        // Update tank positions (dots only, no height line)
        foreach (Tank tank in tanks)
        {
            if (tank == null || dotIndex >= enemyDots.Count) continue;
            
            UpdateEnemyDot(dotIndex, tank.transform.position, tankColor, false, 0f);
            dotIndex++;
        }
        
        // Update helicopter positions (dots with height lines)
        foreach (Helicopter heli in helicopters)
        {
            if (heli == null || dotIndex >= enemyDots.Count) continue;
            
            float height = heli.transform.position.y;
            UpdateEnemyDot(dotIndex, heli.transform.position, helicopterColor, true, height);
            dotIndex++;
        }
    }
    
    void UpdateEnemyDot(int index, Vector3 worldPosition, Color color, bool showHeightLine, float height)
    {
        if (index >= enemyDots.Count) return;
        
        GameObject dot = enemyDots[index];
        GameObject heightLine = heightLines[index];
        
        if (dot == null) return;
        
        // Calculate position relative to base
        Vector3 relativePos = worldPosition - baseTransform.position;
        
        // Calculate distance and angle
        float distance = new Vector2(relativePos.x, relativePos.z).magnitude;
        float angle = Mathf.Atan2(relativePos.x, relativePos.z);
        
        // Normalize distance to radar scale
        float normalizedDistance = Mathf.Clamp01(distance / radarRange);
        float radarRadius = (radarSize / 2f) * 0.9f; // 90% of radar size
        
        // Calculate position on radar (rotate based on camera direction)
        float cameraAngle = 0f;
        if (cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();
            cameraAngle = Mathf.Atan2(cameraForward.x, cameraForward.z);
        }
        
        // Adjust angle relative to camera direction (enemy in front = top of radar)
        float adjustedAngle = angle - cameraAngle;
        
        float radarX = Mathf.Sin(adjustedAngle) * normalizedDistance * radarRadius;
        float radarY = Mathf.Cos(adjustedAngle) * normalizedDistance * radarRadius;
        
        // Update dot position
        dot.SetActive(true);
        RectTransform dotRect = dot.GetComponent<RectTransform>();
        dotRect.anchoredPosition = new Vector2(radarX, radarY);
        
        // Update dot color
        Image dotImage = dot.GetComponent<Image>();
        if (dotImage != null)
            dotImage.color = color;
        
        // Update height line for helicopters
        if (heightLine != null)
        {
            if (showHeightLine && height > 1f)
            {
                heightLine.SetActive(true);
                RectTransform lineRect = heightLine.GetComponent<RectTransform>();
                lineRect.anchoredPosition = new Vector2(radarX, radarY);
                
                // Height line length based on helicopter height
                float lineLength = Mathf.Clamp(height * 0.5f, 2f, maxHeightLineLength);
                lineRect.sizeDelta = new Vector2(heightLineWidth, lineLength);
                
                // Line points upward from the dot
                lineRect.pivot = new Vector2(0.5f, 0f);
                
                Image lineImage = heightLine.GetComponent<Image>();
                if (lineImage != null)
                    lineImage.color = color;
            }
            else
            {
                heightLine.SetActive(false);
            }
        }
    }
    
    public void SetRadarRange(float range)
    {
        radarRange = range;
    }
    
    public void SetRadarSize(float size)
    {
        radarSize = size;
        if (radarRect != null)
            radarRect.sizeDelta = new Vector2(radarSize, radarSize);
    }
}
