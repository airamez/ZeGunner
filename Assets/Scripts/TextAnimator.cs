using UnityEngine;
using TMPro;

public class TextAnimator : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private Color originalColor;
    private float pulseSpeed = 1.5f;
    private float minAlpha = 0.6f;
    private float maxAlpha = 1f;
    
    public void Initialize(Color color)
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        originalColor = color;
    }
    
    void Update()
    {
        if (textComponent != null)
        {
            // Create pulsing effect
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color currentColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            textComponent.color = currentColor;
        }
    }
}
