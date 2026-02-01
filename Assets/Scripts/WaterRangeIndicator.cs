using UnityEngine;

public class WaterRangeIndicator : MonoBehaviour
{
    [Header("Water Range Indicator Settings")]
    [SerializeField] private bool showIndicator = true;
    [SerializeField] private Color waterColorDeep = new Color(0.3f, 0.6f, 0.8f, 0.85f); // Light blue water
    [SerializeField] private Color waterColorShallow = new Color(0.4f, 0.7f, 0.9f, 0.6f); // Very light blue water
    [SerializeField] private float waterHeight = 0.1f; // Closer to ground
    [SerializeField] private int waterSegments = 64;
    [SerializeField] private float waveSpeed = 0.8f;
    [SerializeField] private float waveAmplitude = 0.05f;
    
    private GameObject waterPool;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh waterMesh;
    private float currentRadius;
    
    void Start()
    {
        CreateWaterPool();
        UpdateWaterRadius();
    }
    
    void Update()
    {
        if (showIndicator && waterPool != null)
        {
            UpdateWaterRadius();
            AnimateWater();
        }
    }
    
    void CreateWaterPool()
    {
        // Create water pool GameObject
        waterPool = new GameObject("WaterRangeIndicator");
        waterPool.transform.SetParent(transform);
        
        // Add components
        meshFilter = waterPool.AddComponent<MeshFilter>();
        meshRenderer = waterPool.AddComponent<MeshRenderer>();
        
        // Create mesh
        waterMesh = new Mesh();
        meshFilter.mesh = waterMesh;
        
        // Create material - use Sprites/Default for guaranteed visibility
        Material waterMaterial = new Material(Shader.Find("Sprites/Default"));
        waterMaterial.color = waterColorDeep;
        
        meshRenderer.material = waterMaterial;
    }
    
    void UpdateWaterRadius()
    {
        // Find the shortest firing distance
        TankSpawner tankSpawner = FindAnyObjectByType<TankSpawner>();
        HelicopterSpawner helicopterSpawner = FindAnyObjectByType<HelicopterSpawner>();
        
        float shortestDistance = float.MaxValue;
        
        if (tankSpawner != null)
        {
            shortestDistance = Mathf.Min(shortestDistance, tankSpawner.DistanceToFire);
        }
        
        if (helicopterSpawner != null)
        {
            shortestDistance = Mathf.Min(shortestDistance, helicopterSpawner.DistanceToFire);
        }
        
        if (shortestDistance != float.MaxValue)
        {
            currentRadius = shortestDistance - 2.5f; // Reduce by 3 units (increase by 2 units)
            GenerateWaterMesh();
        }
    }
    
    void GenerateWaterMesh()
    {
        waterMesh.Clear();
        
        // Simple single-ring mesh (working version)
        Vector3[] vertices = new Vector3[waterSegments + 1];
        int[] triangles = new int[waterSegments * 3];
        
        // Center vertex
        vertices[0] = new Vector3(0, waterHeight, 0);
        
        // Outer ring vertices
        for (int i = 0; i < waterSegments; i++)
        {
            float angle = (float)i / waterSegments * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * currentRadius;
            float z = Mathf.Sin(angle) * currentRadius;
            vertices[i + 1] = new Vector3(x, waterHeight, z);
        }
        
        // Create triangles
        for (int i = 0; i < waterSegments; i++)
        {
            int nextIndex = (i + 1) % waterSegments + 1;
            
            triangles[i * 3] = 0; // Center
            triangles[i * 3 + 1] = i + 1; // Current vertex
            triangles[i * 3 + 2] = nextIndex; // Next vertex
        }
        
        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        
        // Generate normals (all pointing up)
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }
        waterMesh.normals = normals;
        
        // Generate UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = new Vector2(0.5f, 0.5f); // Center
        
        for (int i = 0; i < waterSegments; i++)
        {
            float angle = (float)i / waterSegments * 2f * Mathf.PI;
            uvs[i + 1] = new Vector2(
                (Mathf.Cos(angle) + 1) * 0.5f,
                (Mathf.Sin(angle) + 1) * 0.5f
            );
        }
        waterMesh.uv = uvs;
        
        waterMesh.RecalculateBounds();
    }
    
    void AnimateWater()
    {
        if (waterMesh == null) return;
        
        Vector3[] vertices = waterMesh.vertices;
        float time = Time.time * waveSpeed;
        
        // Animate outer ring vertices with simple wave effect
        for (int i = 1; i < vertices.Length; i++)
        {
            float angle = (float)(i - 1) / waterSegments * 2f * Mathf.PI;
            float waveOffset = Mathf.Sin(time + angle * 3f) * waveAmplitude;
            vertices[i] = new Vector3(vertices[i].x, waterHeight + waveOffset, vertices[i].z);
        }
        
        waterMesh.vertices = vertices;
        waterMesh.RecalculateNormals();
    }
    
    public void ToggleIndicator()
    {
        showIndicator = !showIndicator;
        if (waterPool != null)
        {
            waterPool.SetActive(showIndicator);
        }
    }
    
    public void SetIndicatorVisibility(bool visible)
    {
        showIndicator = visible;
        if (waterPool != null)
        {
            waterPool.SetActive(visible);
        }
    }
    
    [ContextMenu("Regenerate Water Pool")]
    public void RegenerateWaterPool()
    {
        if (waterPool != null)
        {
            DestroyImmediate(waterPool);
        }
        CreateWaterPool();
        UpdateWaterRadius();
    }
}
