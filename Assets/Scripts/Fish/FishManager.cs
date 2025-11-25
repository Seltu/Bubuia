using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FishSpawn
{
    public Fish prefab;
    public int spawnChance;
}

public class FishManager : MonoBehaviour
{
    [Header("Fish Types")]
    public List<FishSpawn> fishPrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    public int maxBoids = 30;
    public float minDistanceFromCamera = 10f;

    [Header("Spawn Area (Centered on Manager, XZ)")]
    public Vector2 spawnArea = new Vector2(10f, 5f); // X range, Z range

    [HideInInspector] 
    public List<Fish> boids = new();

    private Camera mainCamera;
    private float timer;

    void Start()
    {
        mainCamera = Camera.main;
        timer = spawnInterval;
    }

    void Update()
    {
        if (boids.Count >= maxBoids) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TrySpawnFish();
            timer = spawnInterval;
        }
    }

    void TrySpawnFish()
    {
        if (fishPrefabs == null || fishPrefabs.Count == 0) return;

        for (int i = 0; i < 10; i++)
        {
            Vector3 spawnPos = GetRandomPointInSpawnArea();

            // Distância no plano XZ
            Vector3 camPosXZ = new(mainCamera.transform.position.x, 0, mainCamera.transform.position.z);
            Vector3 spawnXZ = new(spawnPos.x, 0, spawnPos.z);

            float camDistance = Vector3.Distance(spawnXZ, camPosXZ);

            if (camDistance >= minDistanceFromCamera)
            {
                Fish prefab = GetWeightedRandomFish();

                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                Fish fish = Instantiate(prefab, spawnPos, rot);
                fish.manager = this;
                boids.Add(fish);

                break;
            }
        }
    }

    Vector3 GetRandomPointInSpawnArea()
    {
        float x = Random.Range(-spawnArea.x, spawnArea.x);
        float z = Random.Range(-spawnArea.y, spawnArea.y);

        return new Vector3(
            transform.position.x + x,
            transform.position.y,
            transform.position.z + z
        );
    }

    Fish GetWeightedRandomFish()
    {
        int totalWeight = 0;
        foreach (var fs in fishPrefabs)
            totalWeight += fs.spawnChance;

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var fs in fishPrefabs)
        {
            cumulative += fs.spawnChance;
            if (roll < cumulative)
                return fs.prefab;
        }

        return fishPrefabs[0].prefab; 
    }
}
