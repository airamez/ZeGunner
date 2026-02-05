using UnityEngine;

public class TerrainTextureGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public Terrain terrain;
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private int textureResolution = 1024;
    
    [Header("Grass Colors - Battlefield")]
    [SerializeField] private Color grassDark = new Color(0.22f, 0.35f, 0.15f, 1f);
    [SerializeField] private Color grassMedium = new Color(0.28f, 0.42f, 0.20f, 1f);
    [SerializeField] private Color grassLight = new Color(0.35f, 0.50f, 0.25f, 1f);
    [SerializeField] private Color grassDry = new Color(0.45f, 0.48f, 0.28f, 1f);
    [SerializeField] private Color grassBurnt = new Color(0.18f, 0.22f, 0.12f, 1f);
    
    [Header("Base Pavement")]
    [SerializeField] private float pavementRadius = 100f;
    [SerializeField] private float pavementRoughness = 5f;
    [SerializeField] private Color dirtColor = new Color(0.70f, 0.60f, 0.45f, 1f);
    [SerializeField] private Vector3 basePosition;
    
    [Header("Variation Settings")]
    [SerializeField] private float largePatchScale = 0.003f;
    [SerializeField] private float mediumPatchScale = 0.015f;
    [SerializeField] private float detailScale = 0.08f;
    
    private TerrainData terrainData;
    
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
                Debug.LogError("[TerrainTextureGenerator] No terrain found!");
                return;
            }
        }
        
        terrainData = terrain.terrainData;
        
        // Find base position if not set
        if (basePosition == Vector3.zero)
        {
            BaseGround baseGround = FindAnyObjectByType<BaseGround>();
            if (baseGround != null)
            {
                basePosition = baseGround.transform.position;
                Debug.Log($"[TerrainTextureGenerator] Found base at: {basePosition}");
            }
        }
        
        Texture2D texture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, true);
        Vector3 terrainSize = terrainData.size;
        Vector3 terrainPos = terrain.transform.position;
        
        Debug.Log($"[TerrainTextureGenerator] Terrain pos: {terrainPos}, size: {terrainSize}");
        Debug.Log($"[TerrainTextureGenerator] Base pos: {basePosition}, pavement radius: {pavementRadius}");
        
        int pavementPixels = 0;
        
        for (int y = 0; y < textureResolution; y++)
        {
            for (int x = 0; x < textureResolution; x++)
            {
                float nx = (float)x / textureResolution;
                float ny = (float)y / textureResolution;
                
                // World position of this pixel
                float worldX = terrainPos.x + (nx * terrainSize.x);
                float worldZ = terrainPos.z + (ny * terrainSize.z);
                
                // Distance from base center (rough circle with noise)
                float distFromBase = Mathf.Sqrt(
                    Mathf.Pow(worldX - basePosition.x, 2) + 
                    Mathf.Pow(worldZ - basePosition.z, 2)
                );
                
                // Three-layer noise for natural grass variation
                float large = Mathf.PerlinNoise(nx * 2000f * largePatchScale, ny * 2000f * largePatchScale);
                float medium = Mathf.PerlinNoise(nx * 2000f * mediumPatchScale + 100, ny * 2000f * mediumPatchScale + 100);
                float detail = Mathf.PerlinNoise(nx * 2000f * detailScale + 200, ny * 2000f * detailScale + 200);
                
                // Add roughness to pavement edge
                float roughEdge = pavementRadius + (medium - 0.5f) * pavementRoughness * 2f;
                
                // Base grass color blending
                Color grassColor = Color.Lerp(grassDark, grassMedium, large);
                grassColor = Color.Lerp(grassColor, grassLight, medium * 0.5f);
                
                // Occasional dry patches (battlefield wear)
                if (large > 0.65f && medium > 0.5f)
                {
                    float dryBlend = (large - 0.65f) * 2f * medium;
                    grassColor = Color.Lerp(grassColor, grassDry, dryBlend);
                }
                
                // Occasional burnt/scorched patches (explosion marks)
                if (large < 0.25f && detail > 0.6f)
                {
                    float burntBlend = (0.25f - large) * 2f * (detail - 0.6f) * 2.5f;
                    burntBlend = Mathf.Clamp01(burntBlend);
                    grassColor = Color.Lerp(grassColor, grassBurnt, burntBlend * 0.7f);
                }
                
                // Fine detail variation
                float microVariation = (detail - 0.5f) * 0.08f;
                grassColor.r = Mathf.Clamp01(grassColor.r + microVariation);
                grassColor.g = Mathf.Clamp01(grassColor.g + microVariation * 1.2f);
                grassColor.b = Mathf.Clamp01(grassColor.b + microVariation * 0.6f);
                
                // Base pavement
                Color finalColor = grassColor;
                float pavementStrength = 0f;
                
                if (distFromBase < roughEdge)
                {
                    pavementStrength = 1f - Mathf.Clamp01(distFromBase / roughEdge);
                    pavementStrength = pavementStrength * 0.98f;
                }
                
                // Apply pavement color
                if (pavementStrength > 0f)
                {
                    Color variedDirt = Color.Lerp(dirtColor, dirtColor * 0.7f, detail * 0.5f);
                    finalColor = Color.Lerp(grassColor, variedDirt, pavementStrength);
                    pavementPixels++;
                }
                
                texture.SetPixel(x, y, finalColor);
            }
        }
        
        texture.Apply();
        Debug.Log($"[TerrainTextureGenerator] Pavement pixels painted: {pavementPixels} / {textureResolution * textureResolution}");
        ApplyTextureToTerrain(texture);
        Debug.Log($"[TerrainTextureGenerator] Generated: {textureResolution}x{textureResolution} with base pavement ({pavementRadius}m)");
    }
    
    void ApplyTextureToTerrain(Texture2D texture)
    {
        terrainData.terrainLayers = new TerrainLayer[0];
        
        TerrainLayer layer = new TerrainLayer();
        layer.diffuseTexture = texture;
        layer.tileSize = new Vector2(terrainData.size.x, terrainData.size.z);
        layer.smoothness = 0f;
        layer.metallic = 0f;
        
        terrainData.terrainLayers = new TerrainLayer[] { layer };
        
        float[,,] splatmap = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 1];
        for (int y = 0; y < terrainData.alphamapHeight; y++)
            for (int x = 0; x < terrainData.alphamapWidth; x++)
                splatmap[x, y, 0] = 1f;
        
        terrainData.SetAlphamaps(0, 0, splatmap);
        
        #if UNITY_EDITOR
        System.IO.File.WriteAllBytes("Assets/GeneratedTerrainTexture.png", texture.EncodeToPNG());
        #endif
    }
}
