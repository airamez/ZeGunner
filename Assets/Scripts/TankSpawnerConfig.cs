using UnityEngine;

[CreateAssetMenu(fileName = "TankSpawnerConfig", menuName = "ZeGunner/Tank Spawner Config")]
public class TankSpawnerConfig : ScriptableObject
{
    [Header("Spawn Settings")]
    [Tooltip("Distance from the base where tanks will spawn")]
    public float spawnDistance = 50f;
    
    [Header("Tank Speed")]
    [Tooltip("Minimum speed of spawned tanks")]
    public float minSpeed = 2f;
    
    [Tooltip("Maximum speed of spawned tanks")]
    public float maxSpeed = 5f;
    
    [Header("Spawn Rate")]
    [Tooltip("Time between tank spawns in seconds")]
    public float spawnInterval = 3f;
    
    [Tooltip("Maximum number of tanks alive at once")]
    public int maxTanks = 10;
}
