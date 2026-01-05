using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FishSpawn
{
    public Fish prefab;
    public int spawnChance;

    [Header("Spawn Depth Interval")]
    public float minZ;
    public float maxZ;
}
public class FishManager : MonoBehaviour
{
    [Header("Fish Types")]
    public List<FishSpawn> fishPrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    public int maxBoids = 30;

    public float minDistanceFromCamera = 10f;
    public float maxDistanceFromCamera = 40f;

    [Header("Spawn Area (Centered on Manager, XZ)")]
    public Vector2 spawnArea = new Vector2(10f, 5f);

    [HideInInspector]
    public List<Fish> boids = new();

    private Camera mainCamera;
    private float timer;

    private void Start()
    {
        mainCamera = Camera.main;
        timer = spawnInterval;
    }

    private void Update()
    {
        if (boids.Count >= maxBoids)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TrySpawnFish();
            timer = spawnInterval;
        }
    }

    private void TrySpawnFish()
    {
        if (fishPrefabs == null || fishPrefabs.Count == 0)
            return;

        const int maxAttempts = 30;

        Vector3 camXZ = new(
            mainCamera.transform.position.x,
            0f,
            mainCamera.transform.position.z
        );

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 spawnPos = GetRandomPointInSpawnArea();

            Vector3 spawnXZ = new(spawnPos.x, 0f, spawnPos.z);
            float distance = Vector3.Distance(camXZ, spawnXZ);

            // Camera distance rule
            if (distance < minDistanceFromCamera || distance > maxDistanceFromCamera)
                continue;

            // Decide which fish spawns (global weights preserved)
            FishSpawn chosenFish = GetWeightedRandomFishSpawn();

            // Local Z relative to manager
            float localZ = spawnPos.z - transform.position.z;

            // Validate fish against Z constraint
            if (localZ < chosenFish.minZ || localZ > chosenFish.maxZ)
            {
                // Fish not allowed here → abort this attempt
                return;
            }

            // --- VALID SPAWN ---
            Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Fish fish = Instantiate(chosenFish.prefab, spawnXZ, rot);

            fish.manager = this;
            boids.Add(fish);

            return;
        }
    }

    private Vector3 GetRandomPointInSpawnArea()
    {
        float x = Random.Range(-spawnArea.x, spawnArea.x);
        float z = Random.Range(-spawnArea.y, spawnArea.y);

        return new Vector3(
            transform.position.x + x,
            transform.position.y,
            transform.position.z + z
        );
    }

    private FishSpawn GetWeightedRandomFishSpawn()
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
                return fs;
        }

        return fishPrefabs[0];
    }
}