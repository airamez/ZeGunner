using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private float size = 20f;
    [SerializeField] private float thickness = 2f;
    [SerializeField] private float gap = 10f;
    [SerializeField] private Color crosshairColor = Color.white;
    
    [Header("Distance Display")]
    [SerializeField] private float maxDetectionDistance = 1000f;
    [SerializeField] private Font distanceFont;
    [SerializeField] private int fontSize = 14;
    
    private Texture2D crosshairTexture;
    private float currentDistance = -1f;
    private GUIStyle distanceStyle;
    
    void Start()
    {
        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, crosshairColor);
        crosshairTexture.Apply();
        
        // Setup distance text style
        distanceStyle = new GUIStyle();
        distanceStyle.font = distanceFont;
        distanceStyle.fontSize = fontSize;
        distanceStyle.normal.textColor = crosshairColor;
        distanceStyle.alignment = TextAnchor.MiddleCenter;
    }
    
    void Update()
    {
        DetectTargetDistance();
    }
    
    void DetectTargetDistance()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            currentDistance = -1f;
            return;
        }
        
        // Raycast from center of screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        // Use sphere cast with adaptive radius - larger radius for far distances
        // At 800m, radius is ~4m which helps hit targets without being too imprecise
        float sphereRadius = 3f;
        
        if (Physics.SphereCast(ray, sphereRadius, out hit, maxDetectionDistance))
        {
            // Check if hit object is a tank or helicopter
            GameObject hitObject = hit.collider.gameObject;
            
            // Check for Tank component
            Tank tank = hitObject.GetComponent<Tank>();
            if (tank != null)
            {
                currentDistance = hit.distance;
                return;
            }
            
            // Check for Helicopter component
            Helicopter helicopter = hitObject.GetComponent<Helicopter>();
            if (helicopter != null)
            {
                currentDistance = hit.distance;
                return;
            }
            
            // Also check parent objects in case collider is on a child
            tank = hitObject.GetComponentInParent<Tank>();
            if (tank != null)
            {
                currentDistance = hit.distance;
                return;
            }
            
            helicopter = hitObject.GetComponentInParent<Helicopter>();
            if (helicopter != null)
            {
                currentDistance = hit.distance;
                return;
            }
        }
        
        currentDistance = -1f;
    }
    
    void OnGUI()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        
        GUI.color = crosshairColor;
        
        GUI.DrawTexture(new Rect(centerX - size, centerY - thickness / 2f, size - gap, thickness), crosshairTexture);
        
        GUI.DrawTexture(new Rect(centerX + gap, centerY - thickness / 2f, size - gap, thickness), crosshairTexture);
        
        GUI.DrawTexture(new Rect(centerX - thickness / 2f, centerY - size, thickness, size - gap), crosshairTexture);
        
        GUI.DrawTexture(new Rect(centerX - thickness / 2f, centerY + gap, thickness, size - gap), crosshairTexture);
        
        // Draw distance below crosshair
        if (currentDistance >= 0)
        {
            float distanceY = centerY + size + 5f;
            string distanceText = $"[{Mathf.RoundToInt(currentDistance)}m]";
            GUI.Label(new Rect(centerX - 50f, distanceY, 100f, 20f), distanceText, distanceStyle);
        }
    }
    
    void OnDestroy()
    {
        if (crosshairTexture != null)
        {
            Destroy(crosshairTexture);
        }
    }
}
