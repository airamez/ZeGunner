using UnityEngine;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool regenerateInEditor = false;
    
    [Header("Texture Generation Settings")]
    [SerializeField] private float grassScale = 0.1f;
    [SerializeField] private float dirtScale = 0.05f;
    [SerializeField] private float noiseScale = 0.02f;
    [SerializeField] private float grassThreshold = 0.4f;
    [SerializeField] private float dirtThreshold = 0.6f;
    
    [Header("Color Settings")]
    [SerializeField] private Color grassColor = new Color(0.2f, 0.4f, 0.1f, 1f);
    [SerializeField] private Color dirtColor = new Color(0.4f, 0.3f, 0.2f, 1f);
    [SerializeField] private Color rockColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private float colorVariation = 0.1f;
    
    private TerrainData terrainData;
    private int width;
    private int height;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateTerrainTextures();
        }
    }
    
    [ContextMenu("Generate Terrain Textures")]
    public void GenerateTerrainTextures()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("No terrain found!");
                return;
            }
        }
        
        terrainData = terrain.terrainData;
        width = terrainData.alphamapWidth;
        height = terrainData.alphamapHeight;
        
        // Create procedural texture
        Texture2D proceduralTexture = GenerateProceduralTexture();
        
        // Apply to terrain
        ApplyTextureToTerrain(proceduralTexture);
        
        Debug.Log("Terrain textures generated successfully!");
    }
    
    Texture2D GenerateProceduralTexture()
    {
        Texture2D texture = new Texture2D(width, height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Generate noise values
                float grassNoise = Mathf.PerlinNoise(x * grassScale, y * grassScale);
                float dirtNoise = Mathf.PerlinNoise(x * dirtScale + 100, y * dirtScale + 100);
                float mainNoise = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                
                // Combine noises for variety
                float combinedNoise = (grassNoise * 0.5f + dirtNoise * 0.3f + mainNoise * 0.2f);
                
                // Determine terrain type based on noise
                Color pixelColor;
                
                if (combinedNoise < grassThreshold)
                {
                    // Grass with variation
                    pixelColor = grassColor;
                    pixelColor = AddColorVariation(pixelColor);
                    
                    // Add grass detail
                    float grassDetail = Mathf.PerlinNoise(x * grassScale * 4, y * grassScale * 4);
                    if (grassDetail > 0.7f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.green, 0.2f);
                    }
                }
                else if (combinedNoise < dirtThreshold)
                {
                    // Dirt with variation
                    pixelColor = dirtColor;
                    pixelColor = AddColorVariation(pixelColor);
                    
                    // Add dirt patches
                    float dirtDetail = Mathf.PerlinNoise(x * dirtScale * 3, y * dirtScale * 3);
                    if (dirtDetail > 0.6f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.yellow, 0.1f);
                    }
                }
                else
                {
                    // Rock/rough areas
                    pixelColor = rockColor;
                    pixelColor = AddColorVariation(pixelColor);
                    
                    // Add rock texture
                    float rockDetail = Mathf.PerlinNoise(x * noiseScale * 2, y * noiseScale * 2);
                    if (rockDetail > 0.5f)
                    {
                        pixelColor = Color.Lerp(pixelColor, Color.gray, 0.3f);
                    }
                }
                
                // Add edge blending for smoother transitions
                float edgeBlend = Mathf.PerlinNoise(x * noiseScale * 0.5f, y * noiseScale * 0.5f);
                pixelColor = Color.Lerp(pixelColor, grassColor * 0.8f, edgeBlend * 0.2f);
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    Color AddColorVariation(Color baseColor)
    {
        float variation = (Random.value - 0.5f) * colorVariation;
        return new Color(
            Mathf.Clamp(baseColor.r + variation, 0f, 1f),
            Mathf.Clamp(baseColor.g + variation, 0f, 1f),
            Mathf.Clamp(baseColor.b + variation, 0f, 1f),
            baseColor.a
        );
    }
    
    void ApplyTextureToTerrain(Texture2D texture)
    {
        // Create a new material for the terrain
        Material terrainMaterial = new Material(Shader.Find("Standard"));
        
        // Set the main texture
        terrainMaterial.mainTexture = texture;
        
        // Configure material properties
        terrainMaterial.SetFloat("_Metallic", 0f);
        terrainMaterial.SetFloat("_Glossiness", 0.1f);
        
        // Apply material to terrain's renderer
        Renderer terrainRenderer = terrain.GetComponent<Renderer>();
        if (terrainRenderer != null)
        {
            terrainRenderer.material = terrainMaterial;
        }
        
        // Update terrain data with the new texture
        terrainData.terrainLayers = new TerrainLayer[0]; // Clear existing layers
        
        // Create a new terrain layer with our generated texture
        TerrainLayer newLayer = new TerrainLayer();
        newLayer.diffuseTexture = texture;
        newLayer.normalMapTexture = null;
        newLayer.tileSize = Vector2.one;
        newLayer.tileOffset = Vector2.zero;
        
        // Add the layer to terrain
        terrainData.terrainLayers = new TerrainLayer[] { newLayer };
        
        // Update splatmap to use our layer
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 1];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                splatmapData[x, y, 0] = 1.0f; // Full coverage with our texture
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
        
        // Save the texture as an asset for future use
        #if UNITY_EDITOR
        string path = "Assets/GeneratedTerrainTexture.png";
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Debug.Log($"Terrain texture saved to: {path}");
        #endif
    }
    
    void OnValidate()
    {
        if (regenerateInEditor && Application.isPlaying)
        {
            GenerateTerrainTextures();
        }
    }
    
    [ContextMenu("Reset to Original")]
    public void ResetToOriginal()
    {
        // This would restore the original texture if you saved it
        Debug.Log("Reset functionality would require saving original texture first");
    }
}
