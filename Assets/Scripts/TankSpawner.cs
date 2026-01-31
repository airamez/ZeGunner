using UnityEngine;
using System.Collections.Generic;

public class TankSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private Transform baseTransform;
    
    [Header("Tank Settings")]
    [SerializeField] private float tankScale = 1.0f;
    
    [Tooltip("Distance from base where tanks switch from zigzag to straight-line movement")]
    [SerializeField] private float closeStraightLineDistance = 10f;
    
    [Tooltip("Minimum time tank will maintain current direction before changing")]
    [SerializeField] private float zigzagMinInterval = 2f;
    
    [Tooltip("Random additional time added to min interval before direction change")]
    [SerializeField] private float zigzagIntervalOffset = 3f;
    
    [Tooltip("Maximum angle deviation for zigzag movement")]
    [SerializeField] private float maxZigzagAngle = 30f;

    [Tooltip("Distance from the base where tanks will spawn")]
    [SerializeField] private float spawnDistance = 50f;

    [Tooltip("Minimum speed of spawned tanks")]
    [SerializeField] private float minSpeed = 2f;
    
    [Tooltip("Maximum speed of spawned tanks")]
    [SerializeField] private float maxSpeed = 5f;
   
    [Tooltip("Time between tank spawns in seconds")]
    [SerializeField] private float spawnInterval = 3f;
    
    [Tooltip("Maximum number of tanks alive at once")]
    [SerializeField] private int maxTanks = 10;
    
    [SerializeField] private float spawnHeight = 0.5f;
    
    private float nextSpawnTime = 0f;
    private List<GameObject> activeTanks = new List<GameObject>();
    
    void Start()
    {
        Debug.Log("TankSpawner started");
        Debug.Log("Tank prefab assigned: " + (tankPrefab != null ? tankPrefab.name : "NULL"));
        Debug.Log("Base transform assigned: " + (baseTransform != null ? baseTransform.name : "NULL"));
    }
    
    void Update()
    {
        activeTanks.RemoveAll(tank => tank == null);
        
        if (Time.time >= nextSpawnTime && activeTanks.Count < maxTanks)
        {
            SpawnTank();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnTank()
    {
        if (tankPrefab == null)
        {
            Debug.LogWarning("Tank prefab not assigned!");
            return;
        }
        
        Vector3 basePosition = baseTransform != null ? baseTransform.position : Vector3.zero;
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        Vector3 spawnOffset = new Vector3(
            Mathf.Cos(randomAngle) * spawnDistance,
            spawnHeight,
            Mathf.Sin(randomAngle) * spawnDistance
        );
        
        Vector3 spawnPosition = basePosition + spawnOffset;
        spawnPosition.y = spawnHeight;
        
        GameObject tank = Instantiate(tankPrefab, spawnPosition, Quaternion.identity);
        
        tank.transform.localScale = Vector3.one * tankScale;
        
        Collider tankCollider = tank.GetComponent<Collider>();
        if (tankCollider == null)
        {
            BoxCollider boxCollider = tank.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one;
        }
        
        tank.tag = "Enemy";
        
        Tank tankScript = tank.GetComponent<Tank>();
        if (tankScript == null)
        {
            tankScript = tank.AddComponent<Tank>();
        }
        
        float speed = Random.Range(minSpeed, maxSpeed);
        tankScript.Initialize(basePosition, speed, closeStraightLineDistance, zigzagMinInterval, zigzagIntervalOffset, maxZigzagAngle);
        
        activeTanks.Add(tank);
    }
}
