using UnityEngine;

public class SimpleRangeIndicator : MonoBehaviour
{
    void Start()
    {
        // Create a simple red circle for testing
        CreateTestCircle("TestCircle", Color.red, 20f, 5f);
    }
    
    void CreateTestCircle(string name, Color color, float radius, float height)
    {
        GameObject circleObj = new GameObject(name);
        circleObj.transform.SetParent(transform);
        
        LineRenderer lineRenderer = circleObj.AddComponent<LineRenderer>();
        
        // Create material
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lineRenderer.material = mat;
        
        // Set line properties
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 65;
        
        // Draw circle
        for (int i = 0; i <= 64; i++)
        {
            float angle = (float)i / 64f * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            Vector3 position = new Vector3(x, height, z);
            lineRenderer.SetPosition(i, position);
        }
    }
}
