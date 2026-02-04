using UnityEngine;
using System.Collections;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager Instance { get; private set; }
    
    [Header("Explosion Effects")]
    [SerializeField] private GameObject[] explosionPrefabs;
    
    [Header("Explosion Sounds")]
    [SerializeField] private AudioClip[] explosionSounds;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Apply global volume control
        if (VolumeManager.Instance != null)
        {
            audioSource.volume = VolumeManager.Instance.GetMasterVolume();
        }
        
        // Load explosion prefabs and sounds
        LoadExplosionEffects();
        LoadExplosionSounds();
    }
    
    void Update()
    {
        // Update volume when global volume changes
        if (VolumeManager.Instance != null && audioSource != null)
        {
            audioSource.volume = VolumeManager.Instance.GetMasterVolume();
        }
    }
    
    void LoadExplosionEffects()
    {
        // Load WarFX explosion prefabs
        explosionPrefabs = new GameObject[]
        {
            Resources.Load<GameObject>("JMO Assets/WarFX/_Effects/Explosions/WFX_Explosion"),
            Resources.Load<GameObject>("JMO Assets/WarFX/_Effects/Explosions/WFX_Explosion Small"),
            Resources.Load<GameObject>("JMO Assets/WarFX/_Effects/Explosions/WFX_Explosion Simple"),
            Resources.Load<GameObject>("JMO Assets/WarFX/_Effects/Explosions/WFX_Explosion LandMine"),
            Resources.Load<GameObject>("JMO Assets/WarFX/_Effects/Explosions/WFX_Explosion StarSmoke"),
            Resources.Load<GameObject>("JMO Assets/WarFX/_Effects/Explosions/WFX_Nuke")
        };
    }
    
    void LoadExplosionSounds()
    {
        // Load grenade sounds as backup
        explosionSounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade1"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade2"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade3"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade4"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade5"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade6"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade7"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade8"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade9"),
            Resources.Load<AudioClip>("Grenade Sound FX/Grenade/Grenade10")
        };
    }
    
    public void PlayExplosionEffect(Vector3 position)
    {
        if (explosionPrefabs == null || explosionPrefabs.Length == 0)
        {
            CreateFallbackExplosion(position);
            return;
        }
        
        // Pick a random explosion prefab
        int randomIndex = Random.Range(0, explosionPrefabs.Length);
        GameObject randomExplosion = explosionPrefabs[randomIndex];
        
        if (randomExplosion != null)
        {
            // Spawn the explosion effect at the specified position
            GameObject spawnedExplosion = Instantiate(randomExplosion, position, Quaternion.identity);
            // Check if the prefab is working (not purple) by checking if it has active renderers
            StartCoroutine(CheckExplosionValidity(spawnedExplosion, position));
        }
        else
        {
            CreateFallbackExplosion(position);
        }
    }
    
    System.Collections.IEnumerator CheckExplosionValidity(GameObject explosion, Vector3 position)
    {
        // Wait a frame for the explosion to initialize
        yield return null;
        
        // Check if the explosion has any active particle systems or renderers
        bool hasValidEffects = false;
        ParticleSystem[] particles = explosion.GetComponentsInChildren<ParticleSystem>();
        Renderer[] renderers = explosion.GetComponentsInChildren<Renderer>();
        
        if (particles.Length > 0)
        {
            foreach (ParticleSystem ps in particles)
            {
                if (ps.isPlaying && ps.particleCount > 0)
                {
                    hasValidEffects = true;
                    break;
                }
            }
        }
        
        if (!hasValidEffects && renderers.Length > 0)
        {
            // Check if renderers have valid materials (not purple)
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material != null && renderer.material.color != Color.magenta)
                {
                    hasValidEffects = true;
                    break;
                }
            }
        }
        
        // If no valid effects found, destroy the purple explosion and create fallback
        if (!hasValidEffects)
        {
            Destroy(explosion);
            CreateFallbackExplosion(position);
        }
    }
    
    void CreateFallbackExplosion(Vector3 position)
    {
        // Create a simple particle system explosion
        GameObject explosion = new GameObject("FallbackExplosion");
        explosion.transform.position = position;
        
        ParticleSystem particleSystem = explosion.AddComponent<ParticleSystem>();
        
        // Configure the particle system
        var main = particleSystem.main;
        main.startColor = new Color(1f, 0.5f, 0f, 1f); // Orange color
        main.startSize = 2f;
        main.startSpeed = 5f;
        main.startLifetime = 1f;
        main.duration = 0.5f;
        main.loop = false;
        
        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 100;
        
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Play the particle system
        particleSystem.Play();
        
        // Destroy after particles finish
        Destroy(explosion, 2f);
    }
    
    public void PlayExplosionSound()
    {
        if (explosionSounds == null || explosionSounds.Length == 0)
        {
            return;
        }
        
        // Pick a random sound
        int randomIndex = Random.Range(0, explosionSounds.Length);
        AudioClip randomSound = explosionSounds[randomIndex];
        
        if (randomSound != null)
        {
            // Play the sound at the camera position (2D sound)
            audioSource.PlayOneShot(randomSound);
        }
        else
        {
            Debug.LogWarning("Random explosion sound is null at index: " + randomIndex);
        }
    }
    
    public void PlayExplosionSound(Vector3 position)
    {
        if (explosionSounds == null || explosionSounds.Length == 0)
        {
            return;
        }
        
        // Pick a random sound
        int randomIndex = Random.Range(0, explosionSounds.Length);
        AudioClip randomSound = explosionSounds[randomIndex];
        
        if (randomSound != null)
        {
            // Play the sound at the specified position (3D sound)
            AudioSource.PlayClipAtPoint(randomSound, position);
        }
        else
        {
            Debug.LogWarning("Random explosion sound is null at index: " + randomIndex);
        }
    }
}
