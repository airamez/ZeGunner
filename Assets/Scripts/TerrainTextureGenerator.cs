using UnityEngine;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private Terrain terrain;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool regenerateInEditor = false;
    [SerializeField] private int textureResolution = 4096;
    
    [Header("Grass Colors - Professional Battlefield")]
    [SerializeField] private Color grassDark = new Color(0.28f, 0.42f, 0.20f, 1f);
    [SerializeField] private Color grassMedium = new Color(0.32f, 0.48f, 0.24f, 1f);
    [SerializeField] private Color grassLight = new Color(0.38f, 0.54f, 0.28f, 1f);
    [SerializeField] private Color grassDry = new Color(0.42f, 0.48f, 0.26f, 1f);
    
    [Header("Dirt Colors")]
    [SerializeField] private Color dirtDark = new Color(0.25f, 0.18f, 0.10f, 1f);
    [SerializeField] private Color dirtLight = new Color(0.42f, 0.32f, 0.18f, 1f);
    [SerializeField] private Color mud = new Color(0.30f, 0.22f, 0.12f, 1f);
    [SerializeField] private Color mudWet = new Color(0.22f, 0.16f, 0.08f, 1f);
    
    [Header("Water & Wet Areas")]
    [SerializeField] private Color waterDeep = new Color(0.12f, 0.20f, 0.28f, 1f);
    [SerializeField] private Color waterShallow = new Color(0.18f, 0.28f, 0.35f, 1f);
    [SerializeField] private Color waterMuddy = new Color(0.25f, 0.22f, 0.15f, 1f);
    
    [Header("Burnt & Dead Areas")]
    [SerializeField] private Color burntDark = new Color(0.15f, 0.12f, 0.08f, 1f);
    [SerializeField] private Color burntMedium = new Color(0.22f, 0.18f, 0.12f, 1f);
    [SerializeField] private Color ashGray = new Color(0.35f, 0.33f, 0.30f, 1f);
    
    [Header("Stones & Rocks")]
    [SerializeField] private Color stoneGray = new Color(0.55f, 0.52f, 0.48f, 1f);
    [SerializeField] private Color stoneBrown = new Color(0.48f, 0.40f, 0.28f, 1f);
    [SerializeField] private Color stoneDark = new Color(0.35f, 0.32f, 0.28f, 1f);
    
    [Header("Bushes & Vegetation")]
    [SerializeField] private Color bushDark = new Color(0.18f, 0.32f, 0.12f, 1f);
    [SerializeField] private Color bushMedium = new Color(0.25f, 0.42f, 0.15f, 1f);
    [SerializeField] private Color bushLight = new Color(0.32f, 0.48f, 0.18f, 1f);
    
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
    
    [Header("Noise Settings - Smooth Battlefield Terrain")]
    [SerializeField] private float largeNoiseScale = 0.005f;
    [SerializeField] private float mediumNoiseScale = 0.015f;
    [SerializeField] private float smallNoiseScale = 0.05f;
    [SerializeField] private float microNoiseScale = 0.15f;
    
    [Header("Variation Settings")]
    [SerializeField] private float dirtPatchFrequency = 0.08f;
    
    [Header("Feature Frequencies (0-1)")]
    [Tooltip("Frequency of burnt grass patches in the battlefield")]
    [SerializeField, Range(0f, 1f)] private float burntPatchFrequency = 0.35f;
    [Tooltip("Frequency of stone/scattered rock patches")]
    [SerializeField, Range(0f, 1f)] private float stoneFrequency = 0.25f;
    [Tooltip("Frequency of small holes/depressions")]
    [SerializeField, Range(0f, 1f)] private float holeFrequency = 0.20f;
    [Tooltip("Frequency of bush clusters")]
    [SerializeField, Range(0f, 1f)] private float bushFrequency = 0.40f;
    [Tooltip("Frequency of water puddles in low areas")]
    [SerializeField, Range(0f, 1f)] private float waterPuddleFrequency = 0.15f;
    [Tooltip("Frequency of mud patches")]
    [SerializeField, Range(0f, 1f)] private float mudPatchFrequency = 0.30f;
    
    [Header("Feature Settings")]
    [SerializeField] private float puddleMaxHeight = 0.25f;
    [SerializeField] private float holeDarkness = 0.5f;
    
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
            Color grassBase = GetGrassColor(noise, normalizedX, normalizedY, terrainHeight);
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
            baseColor = GetGrassColor(noise, normalizedX, normalizedY, terrainHeight);
            
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
    
    Color GetGrassColor(float noise, float normalizedX, float normalizedY, float terrainHeight)
    {
        // Smooth grass variation for professional battlefield look
        float grassTypeNoise = Mathf.PerlinNoise(normalizedX * 5, normalizedY * 5);
        float grassDetailNoise = Mathf.PerlinNoise(normalizedX * 30, normalizedY * 30);
        
        // Blend between grass colors smoothly - no harsh patches
        Color grassBase = Color.Lerp(grassDark, grassMedium, grassTypeNoise);
        
        // Add secondary blend for more natural variation
        float secondaryBlend = Mathf.PerlinNoise(normalizedX * 10 + 100, normalizedY * 10 + 100);
        if (secondaryBlend > 0.5f)
        {
            grassBase = Color.Lerp(grassBase, grassLight, (secondaryBlend - 0.5f) * 0.6f);
        }
        
        // Occasional dry grass patches (very subtle)
        float dryPatchNoise = Mathf.PerlinNoise(normalizedX * 3 + 200, normalizedY * 3 + 200);
        if (dryPatchNoise > 0.75f)
        {
            grassBase = Color.Lerp(grassBase, grassDry, (dryPatchNoise - 0.75f) * 0.8f);
        }
        
        // Add very subtle detail variation for texture
        float detailVariation = (grassDetailNoise - 0.5f) * 0.06f;
        grassBase = new Color(
            Mathf.Clamp01(grassBase.r + detailVariation),
            Mathf.Clamp01(grassBase.g + detailVariation * 1.1f),
            Mathf.Clamp01(grassBase.b + detailVariation * 0.4f),
            1f
        );
        
        // Apply terrain features in grass areas
        grassBase = ApplyTerrainFeatures(grassBase, normalizedX, normalizedY, terrainHeight, grassTypeNoise);
        
        return grassBase;
    }
    
    Color ApplyTerrainFeatures(Color baseColor, float normalizedX, float normalizedY, float terrainHeight, float grassNoise)
    {
        Color result = baseColor;
        
        // Water puddles - only in low areas
        if (terrainHeight < puddleMaxHeight)
        {
            float waterNoise = Mathf.PerlinNoise(normalizedX * 25 + 400, normalizedY * 25 + 400);
            float waterDetail = Mathf.PerlinNoise(normalizedX * 60 + 500, normalizedY * 60 + 500);
            
            if (waterNoise > (1f - waterPuddleFrequency) && waterDetail > 0.4f)
            {
                float waterBlend = Mathf.InverseLerp(1f - waterPuddleFrequency, 1f, waterNoise);
                waterBlend *= waterDetail;
                // Mix shallow and muddy water
                Color waterColor = Color.Lerp(waterShallow, waterMuddy, grassNoise * 0.5f);
                result = Color.Lerp(result, waterColor, waterBlend * 0.85f);
            }
        }
        
        // Mud patches - create wet, dark areas
        float mudNoise = Mathf.PerlinNoise(normalizedX * 18 + 600, normalizedY * 18 + 600);
        float mudDetail = Mathf.PerlinNoise(normalizedX * 45 + 700, normalizedY * 45 + 700);
        
        if (mudNoise > (1f - mudPatchFrequency) && mudDetail > 0.35f)
        {
            float mudBlend = Mathf.InverseLerp(1f - mudPatchFrequency, 1f, mudNoise);
            mudBlend *= mudDetail;
            // Blend between wet and dry mud
            Color mudColor = Color.Lerp(mudWet, mud, mudDetail);
            result = Color.Lerp(result, mudColor, mudBlend * 0.75f);
        }
        
        // Burnt grass patches - scorched battlefield areas
        float burntNoise = Mathf.PerlinNoise(normalizedX * 12 + 800, normalizedY * 12 + 800);
        float burntDetail = Mathf.PerlinNoise(normalizedX * 35 + 900, normalizedY * 35 + 900);
        
        if (burntNoise > (1f - burntPatchFrequency) && burntDetail > 0.3f)
        {
            float burntBlend = Mathf.InverseLerp(1f - burntPatchFrequency, 1f, burntNoise);
            burntBlend *= burntDetail;
            // Blend through burnt stages
            Color burntColor = Color.Lerp(burntMedium, burntDark, burntDetail);
            burntColor = Color.Lerp(burntColor, ashGray, (1f - burntDetail) * 0.3f);
            result = Color.Lerp(result, burntColor, burntBlend * 0.8f);
        }
        
        // Stone/rock patches - scattered rocks and stones
        float stoneNoise = Mathf.PerlinNoise(normalizedX * 22 + 1000, normalizedY * 22 + 1000);
        float stoneDetail = Mathf.PerlinNoise(normalizedX * 55 + 1100, normalizedY * 55 + 1100);
        
        if (stoneNoise > (1f - stoneFrequency) && stoneDetail > 0.5f)
        {
            float stoneBlend = Mathf.InverseLerp(1f - stoneFrequency, 1f, stoneNoise);
            stoneBlend *= stoneDetail;
            // Mix stone types
            Color stoneColor = Color.Lerp(stoneBrown, stoneGray, grassNoise);
            stoneColor = Color.Lerp(stoneColor, stoneDark, stoneDetail * 0.4f);
            result = Color.Lerp(result, stoneColor, stoneBlend * 0.7f);
        }
        
        // Holes/depressions - darker areas in the ground
        float holeNoise = Mathf.PerlinNoise(normalizedX * 16 + 1200, normalizedY * 16 + 1200);
        float holeDetail = Mathf.PerlinNoise(normalizedX * 40 + 1300, normalizedY * 40 + 1300);
        
        if (holeNoise > (1f - holeFrequency) && holeDetail > 0.45f)
        {
            float holeBlend = Mathf.InverseLerp(1f - holeFrequency, 1f, holeNoise);
            holeBlend *= holeDetail;
            // Darken the base color for hole effect
            float darkenAmount = holeDarkness * holeBlend;
            result = new Color(
                Mathf.Clamp01(result.r * (1f - darkenAmount * 0.5f)),
                Mathf.Clamp01(result.g * (1f - darkenAmount * 0.6f)),
                Mathf.Clamp01(result.b * (1f - darkenAmount * 0.7f)),
                1f
            );
            // Add dirt at bottom of holes
            result = Color.Lerp(result, dirtDark, holeBlend * 0.4f);
        }
        
        // Bush clusters - denser vegetation patches
        float bushNoise = Mathf.PerlinNoise(normalizedX * 14 + 1400, normalizedY * 14 + 1400);
        float bushDetail = Mathf.PerlinNoise(normalizedX * 50 + 1500, normalizedY * 50 + 1500);
        
        if (bushNoise > (1f - bushFrequency) && bushDetail > 0.4f)
        {
            float bushBlend = Mathf.InverseLerp(1f - bushFrequency, 1f, bushNoise);
            bushBlend *= bushDetail;
            // Blend bush colors
            Color bushColor = Color.Lerp(bushDark, bushMedium, grassNoise);
            bushColor = Color.Lerp(bushColor, bushLight, bushDetail * 0.5f);
            result = Color.Lerp(result, bushColor, bushBlend * 0.65f);
        }
        
        return result;
    }
    
    Color AddMicroDetail(Color baseColor, float microNoise, float terrainHeight)
    {
        // Add very subtle brightness variation for professional texture
        float brightnessVariation = (microNoise - 0.5f) * 0.04f;
        
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
    }
}
