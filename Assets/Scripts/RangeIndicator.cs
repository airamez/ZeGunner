using UnityEngine;
using UnityEngine.InputSystem;

public class RangeIndicator : MonoBehaviour
{
    [Header("Range Indicator Settings")]
    [SerializeField] private bool showIndicators = true;
    [SerializeField] private Color tankRangeColor = new Color(1f, 0.3f, 0f, 0.4f); // Red-orange
    [SerializeField] private Color helicopterRangeColor = new Color(1f, 0.7f, 0f, 0.3f); // Yellow-orange
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private float tankLineWidth = 0.15f;
    [SerializeField] private float helicopterLineWidth = 0.1f;
    [SerializeField] private float heightAboveGround = 0.1f;
    [SerializeField] private float radiusReduction = 2.5f; // Reduce diameter by 2.5 units
    
    private GameObject tankRangeCircle;
    private GameObject helicopterRangeCircle;
    private LineRenderer tankLineRenderer;
    private LineRenderer helicopterLineRenderer;
    
    void Start()
    {
        CreateRangeCircles();
        UpdateRangeCircles();
    }
    
    void CreateRangeCircles()
    {
        // Create tank range circle
        tankRangeCircle = new GameObject("TankRangeCircle");
        tankRangeCircle.transform.SetParent(transform);
        
        tankLineRenderer = tankRangeCircle.AddComponent<LineRenderer>();
        tankLineRenderer.material = CreateCircleMaterial(tankRangeColor);
        tankLineRenderer.startWidth = tankLineWidth;
        tankLineRenderer.endWidth = tankLineWidth;
        tankLineRenderer.loop = true;
        tankLineRenderer.useWorldSpace = false;
        
        // Create helicopter range circle
        helicopterRangeCircle = new GameObject("HelicopterRangeCircle");
        helicopterRangeCircle.transform.SetParent(transform);
        
        helicopterLineRenderer = helicopterRangeCircle.AddComponent<LineRenderer>();
        helicopterLineRenderer.material = CreateCircleMaterial(helicopterRangeColor);
        helicopterLineRenderer.startWidth = helicopterLineWidth;
        helicopterLineRenderer.endWidth = helicopterLineWidth;
        helicopterLineRenderer.loop = true;
        helicopterLineRenderer.useWorldSpace = false;
    }
    
    Material CreateCircleMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
    
    void UpdateRangeCircles()
    {
        // Get spawner references
        TankSpawner tankSpawner = FindAnyObjectByType<TankSpawner>();
        HelicopterSpawner helicopterSpawner = FindAnyObjectByType<HelicopterSpawner>();
        
        if (tankSpawner != null && tankLineRenderer != null)
        {
            float tankRange = tankSpawner.DistanceToFire - radiusReduction;
            DrawCircle(tankLineRenderer, tankRange);
        }
        
        if (helicopterSpawner != null && helicopterLineRenderer != null)
        {
            float helicopterRange = helicopterSpawner.DistanceToFire - radiusReduction;
            DrawCircle(helicopterLineRenderer, helicopterRange);
        }
    }
    
    void DrawCircle(LineRenderer lineRenderer, float radius)
    {
        lineRenderer.positionCount = circleSegments + 1;
        
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (float)i / circleSegments * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            Vector3 position = new Vector3(x, heightAboveGround, z);
            lineRenderer.SetPosition(i, position);
        }
    }
    
    void Update()
    {
        // Toggle indicators with R key
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ToggleIndicators();
            Debug.Log($"Range indicators {(showIndicators ? "enabled" : "disabled")}");
        }
        
        // Update circles if spawner values change
        if (showIndicators)
        {
            UpdateRangeCircles();
            
            if (tankRangeCircle != null)
                tankRangeCircle.SetActive(true);
            if (helicopterRangeCircle != null)
                helicopterRangeCircle.SetActive(true);
        }
        else
        {
            if (tankRangeCircle != null)
                tankRangeCircle.SetActive(false);
            if (helicopterRangeCircle != null)
                helicopterRangeCircle.SetActive(false);
        }
    }
    
    public void ToggleIndicators()
    {
        showIndicators = !showIndicators;
    }
    
    public void SetIndicatorVisibility(bool visible)
    {
        showIndicators = visible;
    }
    
    void OnValidate()
    {
        // Update colors in editor when values change
        if (tankLineRenderer != null && tankLineRenderer.material != null)
        {
            tankLineRenderer.material.color = tankRangeColor;
        }
        
        if (helicopterLineRenderer != null && helicopterLineRenderer.material != null)
        {
            helicopterLineRenderer.material.color = helicopterRangeColor;
        }
    }
}
