using UnityEngine;

public class WaterRangeIndicator : MonoBehaviour
{
    [Header("Water Range Indicator Settings")]
    [SerializeField] private bool showIndicator = true;
    [SerializeField] private Color waterColorDeep = new Color(0.15f, 0.35f, 0.55f, 0.75f); // Realistic water blue
    [SerializeField] private Color waterColorShallow = new Color(0.25f, 0.45f, 0.65f, 0.5f); // Lighter edge
    [SerializeField] private float waterHeight = 0.08f; // Very close to ground
    [SerializeField] private int waterSegments = 128; // Higher quality
    [SerializeField] private float waveSpeed = 0.5f; // Slower, more realistic
    [SerializeField] private float waveAmplitude = 0.02f; // Subtle waves
    [SerializeField] private float edgeFadeDistance = 2f; // Fade at edges
    
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
        
        // Create water material with reliable transparent shader
        Material waterMaterial = new Material(Shader.Find("Unlit/Transparent"));
        waterMaterial.color = waterColorDeep;
        waterMaterial.renderQueue = 3000; // Render after opaque objects
        
        meshRenderer.material = waterMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
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
        
        // Multi-ring mesh for better quality and edge fading
        int rings = 3;
        int totalVertices = 1 + (waterSegments * rings);
        Vector3[] vertices = new Vector3[totalVertices];
        Color[] colors = new Color[totalVertices];
        
        // Center vertex
        vertices[0] = new Vector3(0, waterHeight, 0);
        colors[0] = waterColorDeep;
        
        // Create concentric rings
        int vertexIndex = 1;
        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = currentRadius * ((ring + 1f) / rings);
            float alpha = ring == rings - 1 ? 0.3f : 1f; // Fade outer edge
            Color ringColor = Color.Lerp(waterColorDeep, waterColorShallow, (float)ring / rings);
            ringColor.a *= alpha;
            
            for (int i = 0; i < waterSegments; i++)
            {
                float angle = (float)i / waterSegments * 2f * Mathf.PI;
                float x = Mathf.Cos(angle) * ringRadius;
                float z = Mathf.Sin(angle) * ringRadius;
                vertices[vertexIndex] = new Vector3(x, waterHeight, z);
                colors[vertexIndex] = ringColor;
                vertexIndex++;
            }
        }
        
        // Create triangles
        int triangleCount = waterSegments * (1 + (rings - 1) * 2);
        int[] triangles = new int[triangleCount * 3];
        int triIndex = 0;
        
        // Center to first ring
        for (int i = 0; i < waterSegments; i++)
        {
            int nextIndex = (i + 1) % waterSegments + 1;
            triangles[triIndex++] = 0;
            triangles[triIndex++] = i + 1;
            triangles[triIndex++] = nextIndex;
        }
        
        // Between rings
        for (int ring = 0; ring < rings - 1; ring++)
        {
            int ringStart = 1 + ring * waterSegments;
            int nextRingStart = 1 + (ring + 1) * waterSegments;
            
            for (int i = 0; i < waterSegments; i++)
            {
                int current = ringStart + i;
                int next = ringStart + (i + 1) % waterSegments;
                int currentNext = nextRingStart + i;
                int nextNext = nextRingStart + (i + 1) % waterSegments;
                
                // First triangle
                triangles[triIndex++] = current;
                triangles[triIndex++] = currentNext;
                triangles[triIndex++] = next;
                
                // Second triangle
                triangles[triIndex++] = next;
                triangles[triIndex++] = currentNext;
                triangles[triIndex++] = nextNext;
            }
        }
        
        waterMesh.vertices = vertices;
        waterMesh.triangles = triangles;
        waterMesh.colors = colors;
        
        // Generate normals
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }
        waterMesh.normals = normals;
        
        // Generate UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = new Vector2(0.5f, 0.5f);
        
        vertexIndex = 1;
        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = (ring + 1f) / rings;
            for (int i = 0; i < waterSegments; i++)
            {
                float angle = (float)i / waterSegments * 2f * Mathf.PI;
                uvs[vertexIndex++] = new Vector2(
                    0.5f + Mathf.Cos(angle) * ringRadius * 0.5f,
                    0.5f + Mathf.Sin(angle) * ringRadius * 0.5f
                );
            }
        }
        waterMesh.uv = uvs;
        
        waterMesh.RecalculateBounds();
    }
    
    void AnimateWater()
    {
        if (waterMesh == null) return;
        
        Vector3[] vertices = waterMesh.vertices;
        float time = Time.time * waveSpeed;
        
        // Animate all vertices with realistic wave patterns
        for (int i = 1; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i];
            float distanceFromCenter = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
            float normalizedDist = distanceFromCenter / currentRadius;
            
            // Multiple wave frequencies for realistic water
            float wave1 = Mathf.Sin(time * 2f + distanceFromCenter * 0.5f) * waveAmplitude;
            float wave2 = Mathf.Sin(time * 1.5f - distanceFromCenter * 0.3f + pos.x * 0.1f) * waveAmplitude * 0.5f;
            float wave3 = Mathf.Cos(time * 1.8f + pos.z * 0.1f) * waveAmplitude * 0.3f;
            
            float totalWave = wave1 + wave2 + wave3;
            
            // Reduce wave amplitude at edges for smooth transition
            totalWave *= (1f - normalizedDist * 0.5f);
            
            vertices[i] = new Vector3(pos.x, waterHeight + totalWave, pos.z);
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
