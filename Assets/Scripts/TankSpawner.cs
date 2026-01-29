using UnityEngine;
using System.Collections.Generic;

public class TankSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private Transform baseTransform;
    [SerializeField] private TankSpawnerConfig config;
    
    [Header("Spawn Height")]
    [SerializeField] private float spawnHeight = 0.5f;
    
    private float nextSpawnTime = 0f;
    private List<GameObject> activeTanks = new List<GameObject>();
    
    void Update()
    {
        activeTanks.RemoveAll(tank => tank == null);
        
        if (Time.time >= nextSpawnTime && activeTanks.Count < config.maxTanks)
        {
            SpawnTank();
            nextSpawnTime = Time.time + config.spawnInterval;
        }
    }
    
    void SpawnTank()
    {
        if (tankPrefab == null || config == null)
        {
            Debug.LogWarning("Tank prefab or config not assigned!");
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(randomAngle) * config.spawnDistance,
            spawnHeight,
            Mathf.Sin(randomAngle) * config.spawnDistance
        );
        
        Vector3 spawnPosition = basePosition + spawnOffset;
        spawnPosition.y = spawnHeight;
        
        GameObject tank = Instantiate(tankPrefab, spawnPosition, Quaternion.identity);
        
        tank.tag = "Enemy";
        
        Tank tankScript = tank.GetComponent<Tank>();
        if (tankScript == null)
        {
            tankScript = tank.AddComponent<Tank>();
        }
        
        float speed = Random.Range(config.minSpeed, config.maxSpeed);
        tankScript.Initialize(basePosition, speed);
        
        activeTanks.Add(tank);
    }
}
