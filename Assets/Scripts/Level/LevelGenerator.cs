using UnityEngine;

[ExecuteAlways]
public class SimplePerlinTerrain : MonoBehaviour
{
    [Header("Terrain Size")]
    public int terrainWidth = 256;
    public int terrainDepth = 256;
    public float terrainHeight = 50f;

    [Header("Resolution")]
    [Tooltip("Allowed: 33, 65, 129, 257, 513")]
    public int heightmapResolution = 129;

    [Header("Perlin Noise")]
    public float noiseScale = 20f;
    public float offsetX = 0f;
    public float offsetZ = 0f;

    [Header("Rendering")]
    [Tooltip("Use this for URP/HDRP/custom shaders")]
    public Material terrainMaterial;

    [Tooltip("Optional Terrain Layers (grass, rock, etc.)")]
    public TerrainLayer[] terrainLayers;

    private Terrain terrain;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
    }

    private void Start()
    {
        Regenerate();
    }

#if UNITY_EDITOR
    [ContextMenu("Regenerate Terrain")]
    private void RegenerateInEditor()
    {
        Regenerate();
    }
#endif

    private void Regenerate()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return;
        }

        // Randomize offsets so generation changes each time
        offsetX = Random.Range(0f, 9999f);
        offsetZ = Random.Range(0f, 9999f);

        GenerateTerrain();
        ApplyMaterial();
        ApplyTerrainLayers();
    }

    private void GenerateTerrain()
    {
        TerrainData data = terrain.terrainData;

        // Set resolution + size
        data.heightmapResolution = heightmapResolution;
        data.size = new Vector3(terrainWidth, terrainHeight, terrainDepth);

        int res = data.heightmapResolution;
        float[,] heights = new float[res, res];

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                float xCoord = (float)x / res * noiseScale + offsetX;
                float zCoord = (float)z / res * noiseScale + offsetZ;

                heights[z, x] = Mathf.PerlinNoise(xCoord, zCoord);
            }
        }

        data.SetHeights(0, 0, heights);
    }

    private void ApplyMaterial()
    {
        if (terrainMaterial != null)
        {
            terrain.materialTemplate = terrainMaterial;
        }
    }

    private void ApplyTerrainLayers()
    {
        if (terrainLayers != null && terrainLayers.Length > 0)
        {
            terrain.terrainData.terrainLayers = terrainLayers;
        }
    }
}
