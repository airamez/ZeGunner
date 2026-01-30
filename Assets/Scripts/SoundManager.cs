using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
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
        
        // Load explosion sounds from the folder
        LoadExplosionSounds();
    }
    
    void LoadExplosionSounds()
    {
        // For now, we'll manually assign the sounds
        // In a real project, you might want to load them dynamically
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
        
        Debug.Log("Loaded " + explosionSounds.Length + " explosion sounds");
    }
    
    public void PlayExplosionSound()
    {
        if (explosionSounds == null || explosionSounds.Length == 0)
        {
            Debug.LogWarning("No explosion sounds loaded!");
            return;
        }
        
        // Pick a random sound
        int randomIndex = Random.Range(0, explosionSounds.Length);
        AudioClip randomSound = explosionSounds[randomIndex];
        
        if (randomSound != null)
        {
            // Play the sound at the camera position (2D sound)
            audioSource.PlayOneShot(randomSound);
            Debug.Log("Playing explosion sound: " + randomSound.name);
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
            Debug.LogWarning("No explosion sounds loaded!");
            return;
        }
        
        // Pick a random sound
        int randomIndex = Random.Range(0, explosionSounds.Length);
        AudioClip randomSound = explosionSounds[randomIndex];
        
        if (randomSound != null)
        {
            // Play the sound at the specified position (3D sound)
            AudioSource.PlayClipAtPoint(randomSound, position);
            Debug.Log("Playing explosion sound at position: " + position + " - " + randomSound.name);
        }
        else
        {
            Debug.LogWarning("Random explosion sound is null at index: " + randomIndex);
        }
    }
}
