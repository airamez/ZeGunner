using UnityEngine;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool regenerateInEditor = false;
    [SerializeField] private int textureResolution = 1024;
    
    [Header("Grass Colors")]
    [SerializeField] private Color grassDark = new Color(0.15f, 0.28f, 0.08f, 1f);
    [SerializeField] private Color grassMedium = new Color(0.22f, 0.38f, 0.12f, 1f);
    [SerializeField] private Color grassLight = new Color(0.35f, 0.48f, 0.18f, 1f);
    [SerializeField] private Color grassDry = new Color(0.45f, 0.42f, 0.22f, 1f);
    
    [Header("Dirt Colors")]
    [SerializeField] private Color dirtDark = new Color(0.25f, 0.18f, 0.10f, 1f);
    [SerializeField] private Color dirtLight = new Color(0.42f, 0.32f, 0.18f, 1f);
    [SerializeField] private Color mud = new Color(0.30f, 0.22f, 0.12f, 1f);
    
    [Header("Mountain Colors")]
    [SerializeField] private Color rockGray = new Color(0.45f, 0.42f, 0.38f, 1f);
    [SerializeField] private Color rockBrown = new Color(0.38f, 0.28f, 0.18f, 1f);
    [SerializeField] private Color mountainBrown = new Color(0.35f, 0.25f, 0.15f, 1f);
    [SerializeField] private Color snowWhite = new Color(0.95f, 0.97f, 1.0f, 1f);
    
    [Header("Height Thresholds (0-1 normalized)")]
    [SerializeField] private float grassMaxHeight = 0.3f;
    [SerializeField] private float dirtMaxHeight = 0.5f;
    [SerializeField] private float rockMaxHeight = 0.75f;
    [SerializeField] private float snowMinHeight = 0.85f;
    
    [Header("Noise Settings")]
    [SerializeField] private float largeNoiseScale = 0.008f;
    [SerializeField] private float mediumNoiseScale = 0.025f;
    [SerializeField] private float smallNoiseScale = 0.08f;
    [SerializeField] private float microNoiseScale = 0.2f;
    
    [Header("Variation Settings")]
    [SerializeField] private float dirtPatchFrequency = 0.15f;
    [SerializeField] private float grassVariation = 0.25f;
    
    private TerrainData terrainData;
    private int width;
    private int height;
    private float[,] heightMap;
    
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
            terrain = FindAnyObjectByType<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("No terrain found!");
                return;
            }
        }
        
        terrainData = terrain.terrainData;
        width = textureResolution;
        height = textureResolution;
        
        // Get heightmap data
        int heightmapRes = terrainData.heightmapResolution;
        heightMap = terrainData.GetHeights(0, 0, heightmapRes, heightmapRes);
        
        // Create procedural texture
        Texture2D proceduralTexture = GenerateProceduralTexture();
        
        // Apply to terrain
        ApplyTextureToTerrain(proceduralTexture);
        
        Debug.Log("Terrain textures generated successfully!");
    }
    
    float GetHeightAtPosition(float normalizedX, float normalizedY)
    {
        int heightmapRes = terrainData.heightmapResolution;
        int hx = Mathf.Clamp(Mathf.FloorToInt(normalizedX * (heightmapRes - 1)), 0, heightmapRes - 1);
        int hy = Mathf.Clamp(Mathf.FloorToInt(normalizedY * (heightmapRes - 1)), 0, heightmapRes - 1);
        return heightMap[hy, hx];
    }
    
    float GetSlopeAtPosition(float normalizedX, float normalizedY)
    {
        float delta = 0.01f;
        float h1 = GetHeightAtPosition(normalizedX - delta, normalizedY);
        float h2 = GetHeightAtPosition(normalizedX + delta, normalizedY);
        float h3 = GetHeightAtPosition(normalizedX, normalizedY - delta);
        float h4 = GetHeightAtPosition(normalizedX, normalizedY + delta);
        
        float slopeX = Mathf.Abs(h2 - h1) / (2f * delta);
        float slopeY = Mathf.Abs(h4 - h3) / (2f * delta);
        
        return Mathf.Sqrt(slopeX * slopeX + slopeY * slopeY);
    }
    
    Texture2D GenerateProceduralTexture()
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, true);
        
        // Use seed for consistent results
        Random.InitState(42);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalizedX = (float)x / width;
                float normalizedY = (float)y / height;
                
                // Get terrain height and slope at this position
                float terrainHeight = GetHeightAtPosition(normalizedX, normalizedY);
                float slope = GetSlopeAtPosition(normalizedX, normalizedY);
                
                // Generate multi-octave noise for natural variation
                float largeNoise = Mathf.PerlinNoise(x * largeNoiseScale, y * largeNoiseScale);
                float mediumNoise = Mathf.PerlinNoise(x * mediumNoiseScale + 100, y * mediumNoiseScale + 100);
                float smallNoise = Mathf.PerlinNoise(x * smallNoiseScale + 200, y * smallNoiseScale + 200);
                float microNoise = Mathf.PerlinNoise(x * microNoiseScale + 300, y * microNoiseScale + 300);
                
                // Combine noises with different weights (fractal brownian motion style)
                float combinedNoise = largeNoise * 0.5f + mediumNoise * 0.25f + smallNoise * 0.15f + microNoise * 0.1f;
                
                // Determine base color based on height
                Color pixelColor = GetHeightBasedColor(terrainHeight, slope, combinedNoise, normalizedX, normalizedY);
                
                // Add micro-detail variation
                pixelColor = AddMicroDetail(pixelColor, microNoise, terrainHeight);
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        return texture;
    }
    
    Color GetHeightBasedColor(float terrainHeight, float slope, float noise, float normalizedX, float normalizedY)
    {
        Color baseColor;
        
        // Snow at highest elevations
        if (terrainHeight >= snowMinHeight)
        {
            float snowBlend = Mathf.InverseLerp(snowMinHeight, 1f, terrainHeight);
            Color rockBase = Color.Lerp(rockBrown, rockGray, noise);
            baseColor = Color.Lerp(rockBase, snowWhite, snowBlend * (0.7f + noise * 0.3f));
            
            // Add snow variation
            float snowNoise = Mathf.PerlinNoise(normalizedX * 50, normalizedY * 50);
            if (snowNoise > 0.6f)
            {
                baseColor = Color.Lerp(baseColor, snowWhite, 0.3f);
            }
        }
        // Rocky mountain areas
        else if (terrainHeight >= rockMaxHeight)
        {
            float rockBlend = Mathf.InverseLerp(rockMaxHeight, snowMinHeight, terrainHeight);
            baseColor = Color.Lerp(mountainBrown, rockBrown, rockBlend);
            
            // Add rock texture variation
            float rockNoise = Mathf.PerlinNoise(normalizedX * 30, normalizedY * 30);
            baseColor = Color.Lerp(baseColor, rockGray, rockNoise * 0.4f);
        }
        // Transition zone (dirt/rock mix)
        else if (terrainHeight >= dirtMaxHeight)
        {
            float transitionBlend = Mathf.InverseLerp(dirtMaxHeight, rockMaxHeight, terrainHeight);
            Color dirtBase = Color.Lerp(dirtDark, dirtLight, noise);
            baseColor = Color.Lerp(dirtBase, mountainBrown, transitionBlend);
            
            // Add rocky patches
            float patchNoise = Mathf.PerlinNoise(normalizedX * 20, normalizedY * 20);
            if (patchNoise > 0.7f)
            {
                baseColor = Color.Lerp(baseColor, rockGray, 0.4f);
            }
        }
        // Dirt areas
        else if (terrainHeight >= grassMaxHeight)
        {
            float dirtBlend = Mathf.InverseLerp(grassMaxHeight, dirtMaxHeight, terrainHeight);
            Color grassBase = GetGrassColor(noise, normalizedX, normalizedY);
            Color dirtBase = Color.Lerp(dirtDark, dirtLight, noise);
            baseColor = Color.Lerp(grassBase, dirtBase, dirtBlend);
            
            // Add dirt patches in grass
            float patchNoise = Mathf.PerlinNoise(normalizedX * 15 + 500, normalizedY * 15 + 500);
            if (patchNoise > (1f - dirtPatchFrequency))
            {
                baseColor = Color.Lerp(baseColor, mud, 0.6f);
            }
        }
        // Grass areas (lowest elevation)
        else
        {
            baseColor = GetGrassColor(noise, normalizedX, normalizedY);
            
            // Add random dirt patches
            float patchNoise = Mathf.PerlinNoise(normalizedX * 12 + 700, normalizedY * 12 + 700);
            if (patchNoise > (1f - dirtPatchFrequency * 0.5f))
            {
                float patchIntensity = Mathf.PerlinNoise(normalizedX * 25, normalizedY * 25);
                baseColor = Color.Lerp(baseColor, dirtLight, patchIntensity * 0.5f);
            }
        }
        
        // Steep slopes get more rocky/dirt appearance
        if (slope > 0.3f)
        {
            float slopeInfluence = Mathf.Clamp01((slope - 0.3f) * 2f);
            baseColor = Color.Lerp(baseColor, rockGray, slopeInfluence * 0.5f);
        }
        
        return baseColor;
    }
    
    Color GetGrassColor(float noise, float normalizedX, float normalizedY)
    {
        // Multiple grass types for variety
        float grassTypeNoise = Mathf.PerlinNoise(normalizedX * 8, normalizedY * 8);
        float grassDetailNoise = Mathf.PerlinNoise(normalizedX * 40, normalizedY * 40);
        
        Color grassBase;
        
        if (grassTypeNoise < 0.3f)
        {
            // Dark lush grass
            grassBase = grassDark;
        }
        else if (grassTypeNoise < 0.6f)
        {
            // Medium grass
            grassBase = grassMedium;
        }
        else if (grassTypeNoise < 0.85f)
        {
            // Light grass
            grassBase = grassLight;
        }
        else
        {
            // Dry/yellow grass patches
            grassBase = grassDry;
        }
        
        // Blend between grass types for smooth transitions
        float blendNoise = Mathf.PerlinNoise(normalizedX * 15 + 400, normalizedY * 15 + 400);
        Color blendTarget = blendNoise < 0.5f ? grassMedium : grassLight;
        grassBase = Color.Lerp(grassBase, blendTarget, blendNoise * grassVariation);
        
        // Add fine detail variation
        float detailVariation = (grassDetailNoise - 0.5f) * 0.15f;
        grassBase = new Color(
            Mathf.Clamp01(grassBase.r + detailVariation),
            Mathf.Clamp01(grassBase.g + detailVariation * 1.2f),
            Mathf.Clamp01(grassBase.b + detailVariation * 0.5f),
            1f
        );
        
        return grassBase;
    }
    
    Color AddMicroDetail(Color baseColor, float microNoise, float terrainHeight)
    {
        // Add subtle brightness variation for texture
        float brightnessVariation = (microNoise - 0.5f) * 0.08f;
        
        // Less variation at higher elevations (rock/snow)
        if (terrainHeight > rockMaxHeight)
        {
            brightnessVariation *= 0.5f;
        }
        
        return new Color(
            Mathf.Clamp01(baseColor.r + brightnessVariation),
            Mathf.Clamp01(baseColor.g + brightnessVariation),
            Mathf.Clamp01(baseColor.b + brightnessVariation),
            1f
        );
    }
    
    void ApplyTextureToTerrain(Texture2D texture)
    {
        // Update terrain data with the new texture
        terrainData.terrainLayers = new TerrainLayer[0]; // Clear existing layers
        
        // Create a new terrain layer with our generated texture
        TerrainLayer newLayer = new TerrainLayer();
        newLayer.diffuseTexture = texture;
        newLayer.normalMapTexture = null;
        newLayer.tileSize = new Vector2(terrainData.size.x, terrainData.size.z);
        newLayer.tileOffset = Vector2.zero;
        newLayer.smoothness = 0f; // No reflection/glossiness
        newLayer.metallic = 0f; // No metallic reflection
        
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
        Debug.Log("Reset functionality would require saving original texture first");
    }
}
