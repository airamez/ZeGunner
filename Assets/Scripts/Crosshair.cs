using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private float size = 20f;
    [SerializeField] private float thickness = 2f;
    [SerializeField] private float gap = 10f;
    [SerializeField] private Color crosshairColor = Color.white;
    
    private Texture2D crosshairTexture;
    
    void Start()
    {
        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, crosshairColor);
        crosshairTexture.Apply();
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
    }
    
    void OnDestroy()
    {
        if (crosshairTexture != null)
        {
            Destroy(crosshairTexture);
        }
    }
}
