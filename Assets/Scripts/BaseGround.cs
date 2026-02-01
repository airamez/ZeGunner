using UnityEngine;

public class BaseGround : MonoBehaviour
{
    [Header("Base Visual Settings")]
    [SerializeField] private bool addVisuals = true;
    [SerializeField] private float baseRadius = 15f;
    [SerializeField] private Color baseColor = new Color(0.3f, 0.4f, 0.5f, 0.8f);
    
    void Start()
    {
        if (addVisuals)
        {
            AddBaseVisuals();
        }
    }
    
    void AddBaseVisuals()
    {
        // Create a simple visual indicator for the base
        GameObject baseIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseIndicator.name = "BaseIndicator";
        baseIndicator.transform.SetParent(transform);
        baseIndicator.transform.localPosition = new Vector3(0, 0.1f, 0);
        baseIndicator.transform.localScale = new Vector3(baseRadius * 2f, 0.2f, baseRadius * 2f);
        
        // Add semi-transparent material
        Renderer renderer = baseIndicator.GetComponent<Renderer>();
        Material baseMaterial = new Material(Shader.Find("Standard"));
        baseMaterial.color = baseColor;
        baseMaterial.SetFloat("_Metallic", 0.2f);
        baseMaterial.SetFloat("_Smoothness", 0.3f);
        baseMaterial.SetFloat("_Mode", 3); // Transparent
        baseMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        baseMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        baseMaterial.SetInt("_ZWrite", 0);
        baseMaterial.EnableKeyword("_ALPHABLEND_ON");
        baseMaterial.renderQueue = 3000;
        renderer.material = baseMaterial;
        
        // Remove collider for visual only
        Destroy(baseIndicator.GetComponent<Collider>());
    }
}
